namespace UnityEngine.Terminal
{
    using System;
    using System.Text;
    using UnityEngine;
    using Assertions;
    using Microsoft.Extensions.Logging;
    using Profiling;

    public enum TerminalState
    {
        Close,
        OpenSmall,
        OpenFull
    }

    public class Terminal : MonoBehaviour
    {
        [VContainer.Inject]
        internal TerminalContext Context { get; set; }
        
        public TerminalSettings Settings => Context.Settings;
        public CommandShell Shell => Context.Shell;
        public ILogger<Terminal> Logger => Context.Logger;
        public CommandAutocomplete Autocomplete => Context.Autocomplete;
        public CommandHistory History => Context.History;
        public ITerminalBuffer Buffer => Context.Buffer;
        
        #region private shit

        private TerminalState state;
        private TextEditor editorState;
        private bool inputFix;
        private bool moveCursor;
        private bool initialOpen;
        private Rect window;
        private Rect windowOfHints;
        private float currentOpenT;
        private float openTarget;
        private float realWindowSize;
        private string commandText;
        private string cachedCommandText;
        private Vector2 scrollPosition;
        private GUIStyle windowStyle;
        private GUIStyle hintWindowStyle;
        private GUIStyle labelStyle;
        private GUIStyle inputStyle;
        private Texture2D backgroundTexture;
        private Texture2D inputBackgroundTexture;

        #endregion


        public bool IssuedError => Context.Shell.IssuedErrorMessage != null;

        public bool IsClosed => state == TerminalState.Close && Mathf.Approximately(currentOpenT, openTarget);
        
        public void SetState(TerminalState newState)
        {
            inputFix = true;
            cachedCommandText = commandText;
            commandText = "";

            switch (newState)
            {
                case TerminalState.Close:
                    openTarget = 0;
                    break;
                case TerminalState.OpenSmall:
                {
                    openTarget = Screen.height * Settings.MaxHeight * Settings.SmallTerminalRatio;
                    if (currentOpenT > openTarget)
                    {
                        // Prevent resizing from OpenFull to OpenSmall if window y position
                        // is greater than OpenSmall's target
                        openTarget = 0;
                        state = TerminalState.Close;
                        return;
                    }

                    realWindowSize = openTarget;
                    scrollPosition.y = int.MaxValue;
                    break;
                }
                default:
                    realWindowSize = Screen.height * Settings.MaxHeight;
                    openTarget = realWindowSize;
                    break;
            }

            state = newState;
        }

        public void ToggleState(TerminalState newState) => SetState(state == newState ? TerminalState.Close : newState);
        
        private void OnEnable() 
            => Application.logMessageReceivedThreaded += this.HandleUnityLog;

        private void OnDisable() 
            => Application.logMessageReceivedThreaded -= this.HandleUnityLog;

        private void Start()
        {
            commandText = "";
            cachedCommandText = commandText;
            Assert.AreNotEqual(Settings.ToggleHotkey.ToLower(), "return", "Return is not a valid ToggleHotkey");

            SetupWindow();
            SetupInput();
            SetupLabels();
            

            this.Context.Shell.RegisterCommands();

            if (IssuedError) this.Logger.LogError("Error: {IssuedErrorMessage}", this.Shell.IssuedErrorMessage);

            foreach (var command in this.Shell.Commands) this.Autocomplete.Register(command.Key);
        }

        private void OnGUI()
        {
            if (Event.current.Equals(Event.KeyboardEvent(Settings.ToggleHotkey)))
            {
                SetState(TerminalState.OpenSmall);
                initialOpen = true;
            }
            else if (Event.current.Equals(Event.KeyboardEvent(Settings.ToggleFullHotkey)))
            {
                SetState(TerminalState.OpenFull);
                initialOpen = true;
            }

            if (IsClosed)
                return;

            HandleOpenness();
            window = GUILayout.Window(88, window, DrawConsole, "", windowStyle);
            //windowOfHints = GUILayout.Window(89, windowOfHints, DrawHintWindow, "", this.hintWindowStyle);
        }

        private void SetupWindow()
        {
            realWindowSize = Screen.height * Settings.MaxHeight / 3;
            window = new Rect(0, currentOpenT - realWindowSize, Screen.width, realWindowSize);
            //windowOfHints = new Rect(0, currentOpenT - realWindowSize, Screen.width, realWindowSize);
            // Set background color
            backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, Settings.BackgroundColor);
            backgroundTexture.Apply();

            windowStyle = new GUIStyle {
                normal = { background = backgroundTexture, textColor = Settings.ForegroundColor },
                padding = new RectOffset(4, 4, 4, 4),
                font = Settings.ConsoleFont
            }.DpToPixel();

            hintWindowStyle = new GUIStyle() {
                normal = { background = backgroundTexture, textColor = Settings.ForegroundColor },
                padding = new RectOffset(4, 4, 4, 4),
                font = Settings.ConsoleFont
            }.DpToPixel();
        }

        private void SetupLabels() => labelStyle = new GUIStyle 
        {
            font = Settings.ConsoleFont, 
            normal = { textColor = Settings.ForegroundColor }, 
            wordWrap = true,
            fontSize = Settings.FontSize
        }.DpToPixel();

        private void SetupInput()
        {
            inputStyle = new GUIStyle
            {
                padding = new RectOffset(4, 4, 4, 4).DpToPixel(),
                font = Settings.ConsoleFont,
                fixedHeight = (Settings.FontSize * 1.2f) * 1.6f,
                normal =
                {
                    textColor = Settings.InputColor
                },
                fontSize = (int)(Settings.FontSize * 1.2f)
            }.DpToPixel();

            var darkBackground = new Color
            {
                r = Settings.BackgroundColor.r - Settings.InputContrast, g = Settings.BackgroundColor.g - Settings.InputContrast,
                b = Settings.BackgroundColor.b - Settings.InputContrast,
                a = Settings.InputAlpha
            };

            inputBackgroundTexture = new Texture2D(1, 1);
            inputBackgroundTexture.SetPixel(0, 0, darkBackground);
            inputBackgroundTexture.Apply();
            inputStyle.normal.background = inputBackgroundTexture;
        }

        private void DrawHintWindow(int window2D)
        {
            GUILayout.BeginVertical();
            
            GUILayout.EndVertical();
        }

        private void DrawConsole(int window2D)
        {
            Profiler.BeginSample("terminal:drawConsole");
            GUILayout.BeginVertical();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUIStyle.none, GUIStyle.none);
            GUILayout.FlexibleSpace();
            DrawLogs();
            GUILayout.EndScrollView();

            if (moveCursor)
            {
                CursorToEnd();
                moveCursor = false;
            }

            if (Event.current.Equals(Event.KeyboardEvent("escape")))
                SetState(TerminalState.Close);
            else if (Event.current.Equals(Event.KeyboardEvent("return"))
                     || Event.current.Equals(Event.KeyboardEvent("[enter]")))
                EnterCommand();
            else if (Event.current.Equals(Event.KeyboardEvent("up")))
            {
                commandText = this.History.Previous();
                moveCursor = true;
            }
            else if (Event.current.Equals(Event.KeyboardEvent("down")))
                commandText = this.History.Next();
            else if (Event.current.Equals(Event.KeyboardEvent(Settings.ToggleHotkey)))
                ToggleState(TerminalState.OpenSmall);
            else if (Event.current.Equals(Event.KeyboardEvent(Settings.ToggleFullHotkey)))
                ToggleState(TerminalState.OpenFull);
            else if (Event.current.Equals(Event.KeyboardEvent("tab")))
            {
                CompleteCommand();
                moveCursor = true; // Wait till next draw call
            }

            GUILayout.BeginHorizontal();

            if (!string.Equals(Settings.InputCaret, "", StringComparison.Ordinal))
                GUILayout.Label(Settings.InputCaret, inputStyle,
                    GUILayout.Width(Settings.ConsoleFont.fontSize));

            GUI.SetNextControlName("command_text_field");
            commandText = GUILayout.TextField(commandText, inputStyle);

            if (inputFix && commandText.Length > 0)
            {
                commandText = cachedCommandText; // Otherwise the TextField picks up the ToggleHotkey character event
                inputFix = false;                // Prevents checking string Length every draw call
            }
            
            if (initialOpen)
            {
                GUI.FocusControl("command_text_field");
                initialOpen = false;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            Profiler.EndSample();
        }
        

        private void DrawLogs()
        {
            Profiler.BeginSample("terminal:drawLogs");
            foreach (var log in this.Buffer.GetLogItems())
            {
                labelStyle.normal.textColor = GetLogColor(log.LogLevel);
                GUILayout.Label(
                    log.CategoryName is null
                        ? $"{log.FormattedPayload}"
                        : $"[{log.CategoryName}] {log.FormattedPayload}", this.labelStyle.DpToPixel());
            }
            Profiler.EndSample();
        }
        
        private void HandleOpenness()
        {
            var dt = Settings.ToggleSpeed * Time.unscaledDeltaTime;

            if (currentOpenT < openTarget)
            {
                currentOpenT += dt;
                if (currentOpenT > openTarget) currentOpenT = openTarget;
            }
            else if (currentOpenT > openTarget)
            {
                currentOpenT -= dt;
                if (currentOpenT < openTarget) currentOpenT = openTarget;
            }
            else
            {
                if (inputFix) inputFix = false;
                return; // Already at target
            }

            window = new Rect(0, currentOpenT - realWindowSize, Screen.width, realWindowSize);
        }

        private void EnterCommand()
        {
            this.Logger.LogInformation("{cmd}", commandText);
            this.Shell.RunCommand(commandText);
            this.History.Push(commandText);

            if (IssuedError) this.Logger.LogError("Error: {0}", this.Shell.IssuedErrorMessage);

            commandText = "";
            scrollPosition.y = int.MaxValue;
        }

        private void CompleteCommand()
        {
            Profiler.BeginSample("terminal:completeCommand");
            var headText = commandText;
            var formatWidth = 0;

            var completionBuffer = this.Autocomplete.Complete(ref headText, ref formatWidth, out var disposer);
            var completionLength = completionBuffer.Length;

            if (completionLength != 0) commandText = headText;

            if (completionLength <= 1)
                return;

            // Print possible completions
            var logBuffer = new StringBuilder();

            foreach (var completion in completionBuffer)
            {
                if (string.IsNullOrEmpty(completion)) continue;
                logBuffer.Append(completion.PadRight(formatWidth + 4));
            }

            disposer.Dispose();

            Logger.LogInformation("{buffer}", logBuffer);
            scrollPosition.y = int.MaxValue;
            Profiler.EndSample();
        }

        private void CursorToEnd()
        {
            editorState ??= (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            editorState.MoveCursorToPosition(new Vector2(999, 999));
        }

        private void HandleUnityLog(string message, string stackTrace, LogType type)
        {
            if (this.Buffer is null)
                return;

            if (Settings.DisableUnityDebugLogHook)
            {
                this.OnDisable();
                return;
            }

            this.Buffer.HandleLog(null, message, Cast(type));
            scrollPosition.y = int.MaxValue;
        }

        private LogLevel Cast(LogType t)
        {
            switch (t)
            {
                case LogType.Error:
                    return LogLevel.Error;
                case LogType.Assert:
                    return LogLevel.Warning;
                case LogType.Warning:
                    return LogLevel.Warning;
                case LogType.Log:
                    return LogLevel.Information;
                case LogType.Exception:
                    return LogLevel.Critical;
                default:
                return LogLevel.None;
            }
        }

        private Color GetLogColor(LogLevel type) => type switch 
        {
            LogLevel.Information or LogLevel.Debug or LogLevel.Trace => Settings.ForegroundColor,
            LogLevel.Warning => Settings.WarningColor,
            LogLevel.Critical or LogLevel.Error => Settings.ErrorColor,
            LogLevel.None => Settings.InputColor,
            _ => Settings.ForegroundColor
        };
    }
}
