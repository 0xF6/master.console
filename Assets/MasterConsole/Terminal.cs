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
        
        public TerminalSettings Settings => this.Context.Settings;
        public CommandShell Shell => this.Context.Shell;
        public ILogger<Terminal> Logger => this.Context.Logger;
        public CommandAutocomplete Autocomplete => this.Context.Autocomplete;
        public CommandHistory History => this.Context.History;
        public ITerminalBuffer Buffer => this.Context.Buffer;
        
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

        public bool IsClosed => this.state == TerminalState.Close && Mathf.Approximately(this.currentOpenT, this.openTarget);
        
        public void SetState(TerminalState newState)
        {
            this.inputFix = true;
            this.cachedCommandText = this.commandText;
            this.commandText = "";

            switch (newState)
            {
                case TerminalState.Close:
                    this.openTarget = 0;
                    break;
                case TerminalState.OpenSmall:
                {
                    this.openTarget = Screen.height * this.Settings.MaxHeight * this.Settings.SmallTerminalRatio;
                    if (this.currentOpenT > this.openTarget)
                    {
                        // Prevent resizing from OpenFull to OpenSmall if window y position
                        // is greater than OpenSmall's target
                        this.openTarget = 0;
                        this.state = TerminalState.Close;
                        return;
                    }

                    this.realWindowSize = this.openTarget;
                    this.scrollPosition.y = int.MaxValue;
                    break;
                }
                default:
                    this.realWindowSize = Screen.height * this.Settings.MaxHeight;
                    this.openTarget = this.realWindowSize;
                    break;
            }

            this.state = newState;
        }

        public void ToggleState(TerminalState newState) => this.SetState(this.state == newState ? TerminalState.Close : newState);
        
        private void OnEnable() 
            => Application.logMessageReceivedThreaded += this.HandleUnityLog;

        private void OnDisable() 
            => Application.logMessageReceivedThreaded -= this.HandleUnityLog;

        private void Start()
        {
            this.commandText = "";
            this.cachedCommandText = this.commandText;
            Assert.AreNotEqual(this.Settings.ToggleHotkey.ToLower(), "return", "Return is not a valid ToggleHotkey");

            this.SetupWindow();
            this.SetupInput();
            this.SetupLabels();
            

            this.Context.Shell.RegisterCommands();
            
            foreach (var command in this.Shell.Commands) 
                this.Autocomplete.Register(command.Key);
        }

        private void OnGUI()
        {
            if (Event.current.Equals(Event.KeyboardEvent(this.Settings.ToggleHotkey)))
            {
                this.SetState(TerminalState.OpenSmall);
                this.initialOpen = true;
            }
            else if (Event.current.Equals(Event.KeyboardEvent(this.Settings.ToggleFullHotkey)))
            {
                this.SetState(TerminalState.OpenFull);
                this.initialOpen = true;
            }

            if (this.IsClosed)
                return;

            this.HandleOpenness();
            this.window = GUILayout.Window(88, this.window, this.DrawConsole, "", this.windowStyle);
            //windowOfHints = GUILayout.Window(89, windowOfHints, DrawHintWindow, "", this.hintWindowStyle);
        }

        private void SetupWindow()
        {
            this.realWindowSize = Screen.height * this.Settings.MaxHeight / 3;
            this.window = new Rect(0, this.currentOpenT - this.realWindowSize, Screen.width, this.realWindowSize);
            //windowOfHints = new Rect(0, currentOpenT - realWindowSize, Screen.width, realWindowSize);
            // Set background color
            this.backgroundTexture = new Texture2D(1, 1);
            this.backgroundTexture.SetPixel(0, 0, this.Settings.BackgroundColor);
            this.backgroundTexture.Apply();

            this.windowStyle = new GUIStyle {
                normal = { background = this.backgroundTexture, textColor = this.Settings.ForegroundColor },
                padding = new RectOffset(4, 4, 4, 4),
                font = this.Settings.ConsoleFont
            }.DpToPixel();

            this.hintWindowStyle = new GUIStyle() {
                normal = { background = this.backgroundTexture, textColor = this.Settings.ForegroundColor },
                padding = new RectOffset(4, 4, 4, 4),
                font = this.Settings.ConsoleFont
            }.DpToPixel();
        }

        private void SetupLabels() =>
            this.labelStyle = new GUIStyle 
        {
            font = this.Settings.ConsoleFont, 
            normal = { textColor = this.Settings.ForegroundColor }, 
            wordWrap = true,
            fontSize = this.Settings.FontSize
        }.DpToPixel();

        private void SetupInput()
        {
            this.inputStyle = new GUIStyle
            {
                padding = new RectOffset(4, 4, 4, 4).DpToPixel(),
                font = this.Settings.ConsoleFont,
                fixedHeight = (this.Settings.FontSize * 1.2f) * 1.6f,
                normal =
                {
                    textColor = this.Settings.InputColor
                },
                fontSize = (int)(this.Settings.FontSize * 1.2f)
            }.DpToPixel();

            var darkBackground = new Color
            {
                r = this.Settings.BackgroundColor.r - this.Settings.InputContrast, g = this.Settings.BackgroundColor.g - this.Settings.InputContrast,
                b = this.Settings.BackgroundColor.b - this.Settings.InputContrast,
                a = this.Settings.InputAlpha
            };

            this.inputBackgroundTexture = new Texture2D(1, 1);
            this.inputBackgroundTexture.SetPixel(0, 0, darkBackground);
            this.inputBackgroundTexture.Apply();
            this.inputStyle.normal.background = this.inputBackgroundTexture;
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

            this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, false, false, GUIStyle.none, GUIStyle.none);
            GUILayout.FlexibleSpace();
            this.DrawLogs();
            GUILayout.EndScrollView();

            if (this.moveCursor)
            {
                this.CursorToEnd();
                this.moveCursor = false;
            }

            if (Event.current.Equals(Event.KeyboardEvent("escape")))
                this.SetState(TerminalState.Close);
            else if (Event.current.Equals(Event.KeyboardEvent("return"))
                     || Event.current.Equals(Event.KeyboardEvent("[enter]")))
                this.EnterCommand();
            else if (Event.current.Equals(Event.KeyboardEvent("up")))
            {
                this.commandText = this.History.Previous();
                this.moveCursor = true;
            }
            else if (Event.current.Equals(Event.KeyboardEvent("down")))
                this.commandText = this.History.Next();
            else if (Event.current.Equals(Event.KeyboardEvent(this.Settings.ToggleHotkey)))
                this.ToggleState(TerminalState.OpenSmall);
            else if (Event.current.Equals(Event.KeyboardEvent(this.Settings.ToggleFullHotkey)))
                this.ToggleState(TerminalState.OpenFull);
            else if (Event.current.Equals(Event.KeyboardEvent("tab")))
            {
                this.CompleteCommand();
                this.moveCursor = true; // Wait till next draw call
            }

            GUILayout.BeginHorizontal();

            if (!string.Equals(this.Settings.InputCaret, "", StringComparison.Ordinal))
                GUILayout.Label(this.Settings.InputCaret, this.inputStyle,
                    GUILayout.Width(this.Settings.ConsoleFont.fontSize));

            GUI.SetNextControlName("command_text_field");
            this.commandText = GUILayout.TextField(this.commandText, this.inputStyle);

            if (this.inputFix && this.commandText.Length > 0)
            {
                this.commandText = this.cachedCommandText; // Otherwise the TextField picks up the ToggleHotkey character event
                this.inputFix = false;                // Prevents checking string Length every draw call
            }
            
            if (this.initialOpen)
            {
                GUI.FocusControl("command_text_field");
                this.initialOpen = false;
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
                this.labelStyle.normal.textColor = this.GetLogColor(log.LogLevel);
                GUILayout.Label(
                    log.CategoryName is null
                        ? $"{log.FormattedPayload}"
                        : $"[{log.CategoryName}] {log.FormattedPayload}", this.labelStyle.DpToPixel());
            }
            Profiler.EndSample();
        }
        
        private void HandleOpenness()
        {
            var dt = this.Settings.ToggleSpeed * Time.unscaledDeltaTime;

            if (this.currentOpenT < this.openTarget)
            {
                this.currentOpenT += dt;
                if (this.currentOpenT > this.openTarget) this.currentOpenT = this.openTarget;
            }
            else if (this.currentOpenT > this.openTarget)
            {
                this.currentOpenT -= dt;
                if (this.currentOpenT < this.openTarget) this.currentOpenT = this.openTarget;
            }
            else
            {
                if (this.inputFix) this.inputFix = false;
                return; // Already at target
            }

            this.window = new Rect(0, this.currentOpenT - this.realWindowSize, Screen.width, this.realWindowSize);
        }

        private void EnterCommand()
        {
            this.Buffer.HandleLog(">", this.commandText, LogLevel.None);
            var isSuccess = this.Shell.RunCommand(this.commandText, out var error);
            this.History.Push(this.commandText);

            if (!isSuccess && !string.IsNullOrEmpty(error)) 
                this.Logger.LogError(error);

            this.commandText = "";
            this.scrollPosition.y = int.MaxValue;
        }

        private void CompleteCommand()
        {
            Profiler.BeginSample("terminal:completeCommand");
            var headText = this.commandText;
            var formatWidth = 0;

            var completionBuffer = this.Autocomplete.Complete(ref headText, ref formatWidth, out var disposer);
            var completionLength = completionBuffer.Length;

            if (completionLength != 0) this.commandText = headText;

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

            this.Logger.LogInformation("{buffer}", logBuffer);
            this.scrollPosition.y = int.MaxValue;
            Profiler.EndSample();
        }

        private void CursorToEnd()
        {
            this.editorState ??= (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            this.editorState.MoveCursorToPosition(new Vector2(999, 999));
        }

        private void HandleUnityLog(string message, string stackTrace, LogType type)
        {
            if (this.Buffer is null)
                return;

            if (this.Settings.DisableUnityDebugLogHook)
            {
                this.OnDisable();
                return;
            }

            this.Buffer.HandleLog(null, message, this.Cast(type));
            this.scrollPosition.y = int.MaxValue;
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
            LogLevel.Information or LogLevel.Debug or LogLevel.Trace => this.Settings.ForegroundColor,
            LogLevel.Warning => this.Settings.WarningColor,
            LogLevel.Critical or LogLevel.Error => this.Settings.ErrorColor,
            LogLevel.None => this.Settings.InputColor,
            _ => this.Settings.ForegroundColor
        };
    }
}
