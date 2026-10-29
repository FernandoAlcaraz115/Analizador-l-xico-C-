namespace AnalizadorLexico.Model
{
    /// <summary>
    /// Tipos específicos de tokens reconocidos por la gramática léxica.
    /// </summary>
    public enum TokenType
    {
        // Palabras Reservadas de Control
        KwIf,
        KwElse,
        KwWhile,
        KwFor,
        KwDo,
        KwSwitch,
        KwCase,
        KwBreak,
        KwContinue,
        KwReturn,
        KwDefault,

        // Palabras Reservadas de Tipos de Datos
        KwInt,
        KwFloat,
        KwDouble,
        KwChar,
        KwString,
        KwBool,
        KwVoid,

        // Palabras Reservadas de Estructura / POO
        KwClass,
        KwFunction,
        KwPublic,
        KwPrivate,
        KwStatic,
        KwNew,
        KwMain,

        // Palabras Reservadas de Entrada / Salida
        KwPrint,
        KwPrintln,
        KwRead,

        // Literales / Constantes
        LitInt,
        LitFloat,
        LitString,
        LitChar,
        LitTrue,
        LitFalse,
        LitNull,

        // Identificadores
        Identifier,

        // Operadores Aritméticos
        OpPlus,             // +
        OpMinus,            // -
        OpMultiply,         // *
        OpDivide,           // /
        OpModulo,           // %
        OpIncrement,        // ++
        OpDecrement,        // --

        // Operadores Relacionales
        OpEqual,            // ==
        OpNotEqual,         // !=
        OpLess,             // <
        OpLessOrEqual,      // <=
        OpGreater,          // >
        OpGreaterOrEqual,   // >=

        // Operadores Lógicos
        OpAnd,              // &&
        OpOr,               // ||
        OpNot,              // !

        // Operadores de Asignación
        OpAssign,           // =
        OpPlusAssign,       // +=
        OpMinusAssign,      // -=
        OpMultiplyAssign,   // *=
        OpDivideAssign,     // /=
        OpModuloAssign,     // %=

        // Delimitadores y Puntuación
        OpenParen,          // (
        CloseParen,         // )
        OpenBrace,          // {
        CloseBrace,         // }
        OpenBracket,        // [
        CloseBracket,       // ]
        Semicolon,          // ;
        Comma,              // ,
        Colon,              // :
        Dot,                // .
        Arrow,              // ->

        // Comentarios
        CommentLine,        // // ...
        CommentBlock,       // /* ... */

        // Tokens Especiales
        EndOfFile,          // Fin de archivo ($)
        Unknown             // Carácter o secuencia no reconocida
    }
}
