namespace UnityEngine.Terminal
{
    using System;
    using UnityEngine;

    [Serializable]
    public class TerminalSettings 
    {
        [Header("Window")]
        [Range(0, 1)]
        [SerializeField]
        public float MaxHeight = 0.7f;
        [SerializeField]
        [Range(0, 1)]
        public float SmallTerminalRatio = 0.33f;
        [Range(100, 1000)]
        [SerializeField]
        public float ToggleSpeed = 360;
        [SerializeField] 
        public string ToggleHotkey = "`";
        [SerializeField] 
        public string ToggleFullHotkey = "#`";
        [SerializeField] 
        public int BufferSize = 512;
        [SerializeField]
        public int FontSize = 15;
        [Header("Input")]
        [SerializeField]
        public Font ConsoleFont;
        [SerializeField]
        public string InputCaret = ">";
        [SerializeField]
        public bool RightAlignButtons = false;

        [Header("Theme")]
        [Range(0, 1)]
        [SerializeField] public float InputContrast = 0.0f;
        [Range(0, 1)]
        [SerializeField] public float InputAlpha = 0.5f;

        [SerializeField] public Color BackgroundColor = new Color(255, 255, 255, 135);
        [SerializeField] public Color ForegroundColor = Color.white;
        [SerializeField] public Color ShellColor = Color.white;
        [SerializeField] public Color InputColor = Color.cyan;
        [SerializeField] public Color WarningColor = Color.yellow;
        [SerializeField] public Color ErrorColor = Color.red;
    }
}
