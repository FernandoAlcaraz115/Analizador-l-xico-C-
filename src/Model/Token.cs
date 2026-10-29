namespace AnalizadorLexico.Model
{
    /// <summary>
    /// Representa un componente léxico individual (Token) reconocido en el código fuente.
    /// </summary>
    public class Token
    {
        public int Index { get; set; }
        public TokenType Type { get; set; }
        public TokenCategory Category { get; set; }
        public string Lexeme { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public int Length { get; set; }
        public string Description { get; set; }

        public Token()
        {
            Lexeme = string.Empty;
            Description = string.Empty;
        }

        public Token(int index, TokenType type, TokenCategory category, string lexeme, int line, int column, string description)
        {
            Index = index;
            Type = type;
            Category = category;
            Lexeme = lexeme;
            Line = line;
            Column = column;
            Length = lexeme != null ? lexeme.Length : 0;
            Description = description;
        }

        public override string ToString()
        {
            return string.Format("[L:{0}, C:{1}] {2} (Tipo: {3}) = '{4}'",
                Line, Column, Category, Type, Lexeme);
        }
    }
}
