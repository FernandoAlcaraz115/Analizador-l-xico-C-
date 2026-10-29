namespace AnalizadorLexico.Model
{
    /// <summary>
    /// Representa un error léxico detectado durante el análisis del código fuente.
    /// </summary>
    public class LexicalError
    {
        public int Index { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string InvalidLexeme { get; set; }
        public string Message { get; set; }
        public string Suggestion { get; set; }

        public LexicalError()
        {
            InvalidLexeme = string.Empty;
            Message = string.Empty;
            Suggestion = string.Empty;
        }

        public LexicalError(int index, int line, int column, string invalidLexeme, string message, string suggestion)
        {
            Index = index;
            Line = line;
            Column = column;
            InvalidLexeme = invalidLexeme;
            Message = message;
            Suggestion = suggestion;
        }

        public override string ToString()
        {
            return string.Format("Error léxico en [L:{0}, C:{1}]: {2} (Carácter: '{3}')",
                Line, Column, Message, InvalidLexeme);
        }
    }
}
