namespace VxEventInjector
{
    class LogMsg
    {
        public const string BrushGray = "#6D6E71";
        public const string BrushRed = "#d73136";
        public const string BrushGreen = "#97cb59";
        public const string BrushBlue = "#00BCF1";
        public const string BrushOrange = "#F58A27";
        public string Message { get; set; }
        public string Brush { get; set; }

        public LogMsg()
        {
            Brush = BrushGray;
        }

        public LogMsg(string message) 
            : this()
        {
            Message = message;
        }

        public LogMsg(string message, string brush)
            : this(message)
        {
            Brush = brush;
        }
    }
}
