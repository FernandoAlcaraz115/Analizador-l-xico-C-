using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using AnalizadorLexico.Model;

namespace AnalizadorLexico.Core
{
    /// <summary>
    /// Motor del Analizador Léxico (Scanner).
    /// Convierte el flujo de caracteres del código fuente en una secuencia de tokens válidos
    /// y recopila cualquier error léxico sin interrumpir abruptamente el análisis.
    /// </summary>
    public class Lexer
    {
        private string _source;
        private readonly List<Token> _tokens;
        private readonly List<LexicalError> _errors;
        private readonly SymbolTable _symbolTable;
        private readonly Dictionary<string, TokenType> _keywords;

        private int _start;
        private int _current;
        private int _line;
        private int _column;
        private int _startColumn;
        private int _tokenCounter;
        private int _errorCounter;
        private TokenType _lastTokenType;
        private string _declarationType;   // tipo de la declaración en curso (para "int a, b, c;")

        public bool PreserveComments { get; set; }
        public LexerStats Stats { get; private set; }

        public List<Token> Tokens
        {
            get { return _tokens; }
        }

        public List<LexicalError> Errors
        {
            get { return _errors; }
        }

        public SymbolTable Symbols
        {
            get { return _symbolTable; }
        }

        public Lexer()
        {
            _tokens = new List<Token>();
            _errors = new List<LexicalError>();
            _symbolTable = new SymbolTable();
            _keywords = new Dictionary<string, TokenType>(StringComparer.Ordinal);
            PreserveComments = true;
            Stats = new LexerStats();

            InitializeKeywords();
        }

        private void InitializeKeywords()
        {
            // Control
            _keywords["if"] = TokenType.KwIf;
            _keywords["else"] = TokenType.KwElse;
            _keywords["while"] = TokenType.KwWhile;
            _keywords["for"] = TokenType.KwFor;
            _keywords["do"] = TokenType.KwDo;
            _keywords["switch"] = TokenType.KwSwitch;
            _keywords["case"] = TokenType.KwCase;
            _keywords["break"] = TokenType.KwBreak;
            _keywords["continue"] = TokenType.KwContinue;
            _keywords["return"] = TokenType.KwReturn;
            _keywords["default"] = TokenType.KwDefault;

            // Tipos de datos
            _keywords["int"] = TokenType.KwInt;
            _keywords["float"] = TokenType.KwFloat;
            _keywords["double"] = TokenType.KwDouble;
            _keywords["char"] = TokenType.KwChar;
            _keywords["string"] = TokenType.KwString;
            _keywords["bool"] = TokenType.KwBool;
            _keywords["void"] = TokenType.KwVoid;

            // Estructura y POO
            _keywords["class"] = TokenType.KwClass;
            _keywords["function"] = TokenType.KwFunction;
            _keywords["public"] = TokenType.KwPublic;
            _keywords["private"] = TokenType.KwPrivate;
            _keywords["static"] = TokenType.KwStatic;
            _keywords["new"] = TokenType.KwNew;
            _keywords["main"] = TokenType.KwMain;

            // Entrada y Salida
            _keywords["print"] = TokenType.KwPrint;
            _keywords["println"] = TokenType.KwPrintln;
            _keywords["read"] = TokenType.KwRead;

            // Constantes booleanas y nulas
            _keywords["true"] = TokenType.LitTrue;
            _keywords["false"] = TokenType.LitFalse;
            _keywords["null"] = TokenType.LitNull;
        }

        /// <summary>
        /// Ejecuta el análisis léxico sobre el texto fuente provisto.
        /// </summary>
        public void Tokenize(string source)
        {
            _source = source ?? string.Empty;
            _tokens.Clear();
            _errors.Clear();
            _symbolTable.Clear();
            Stats = new LexerStats();

            _start = 0;
            _current = 0;
            _line = 1;
            _column = 1;
            _startColumn = 1;
            _tokenCounter = 1;
            _errorCounter = 1;
            _lastTokenType = TokenType.EndOfFile;
            _declarationType = null;

            Stopwatch sw = Stopwatch.StartNew();

            while (!IsAtEnd())
            {
                _start = _current;
                _startColumn = _column;
                ScanToken();
            }

            // Token de fin de archivo
            _tokens.Add(new Token(
                _tokenCounter++,
                TokenType.EndOfFile,
                TokenCategory.FinDeArchivo,
                "$",
                _line,
                _column,
                "Fin de Archivo (EOF)"
            ));

            sw.Stop();

            // Computar estadísticas
            Stats.TotalTokens = _tokens.Count;
            Stats.TotalErrors = _errors.Count;
            Stats.TotalLines = _line;
            Stats.TotalCharacters = _source.Length;
            Stats.UniqueSymbols = _symbolTable.Count;
            Stats.ElapsedMilliseconds = sw.Elapsed.TotalMilliseconds;
        }

        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                // Delimitadores simples
                case '(':
                    AddToken(TokenType.OpenParen, TokenCategory.Delimitador, "Paréntesis de apertura");
                    break;
                case ')':
                    AddToken(TokenType.CloseParen, TokenCategory.Delimitador, "Paréntesis de cierre");
                    break;
                case '{':
                    AddToken(TokenType.OpenBrace, TokenCategory.Delimitador, "Llave de apertura");
                    break;
                case '}':
                    AddToken(TokenType.CloseBrace, TokenCategory.Delimitador, "Llave de cierre");
                    break;
                case '[':
                    AddToken(TokenType.OpenBracket, TokenCategory.Delimitador, "Corchete de apertura");
                    break;
                case ']':
                    AddToken(TokenType.CloseBracket, TokenCategory.Delimitador, "Corchete de cierre");
                    break;
                case ';':
                    AddToken(TokenType.Semicolon, TokenCategory.Delimitador, "Punto y coma");
                    break;
                case ',':
                    AddToken(TokenType.Comma, TokenCategory.Delimitador, "Coma");
                    break;
                case ':':
                    AddToken(TokenType.Colon, TokenCategory.Delimitador, "Dos puntos");
                    break;
                case '.':
                    if (IsDigit(Peek()))
                        ScanNumber(true);   // decimal sin parte entera, ej. .5
                    else
                        AddToken(TokenType.Dot, TokenCategory.Delimitador, "Punto de acceso");
                    break;

                // Operadores aritméticos y compuestos
                case '+':
                    if (Match('+'))
                        AddToken(TokenType.OpIncrement, TokenCategory.OperadorAritmetico, "Operador Incremento");
                    else if (Match('='))
                        AddToken(TokenType.OpPlusAssign, TokenCategory.OperadorAsignacion, "Asignación con Suma");
                    else
                        AddToken(TokenType.OpPlus, TokenCategory.OperadorAritmetico, "Operador Suma");
                    break;

                case '-':
                    if (Match('>'))
                        AddToken(TokenType.Arrow, TokenCategory.Delimitador, "Operador Flecha (->)");
                    else if (Match('-'))
                        AddToken(TokenType.OpDecrement, TokenCategory.OperadorAritmetico, "Operador Decremento");
                    else if (Match('='))
                        AddToken(TokenType.OpMinusAssign, TokenCategory.OperadorAsignacion, "Asignación con Resta");
                    else
                        AddToken(TokenType.OpMinus, TokenCategory.OperadorAritmetico, "Operador Resta");
                    break;

                case '*':
                    if (Match('='))
                        AddToken(TokenType.OpMultiplyAssign, TokenCategory.OperadorAsignacion, "Asignación con Multiplicación");
                    else
                        AddToken(TokenType.OpMultiply, TokenCategory.OperadorAritmetico, "Operador Multiplicación");
                    break;

                case '%':
                    if (Match('='))
                        AddToken(TokenType.OpModuloAssign, TokenCategory.OperadorAsignacion, "Asignación con Módulo");
                    else
                        AddToken(TokenType.OpModulo, TokenCategory.OperadorAritmetico, "Operador Módulo");
                    break;

                case '/':
                    if (Match('/'))
                    {
                        // Comentario de una línea
                        ScanLineComment();
                    }
                    else if (Match('*'))
                    {
                        // Comentario de bloque
                        ScanBlockComment();
                    }
                    else if (Match('='))
                    {
                        AddToken(TokenType.OpDivideAssign, TokenCategory.OperadorAsignacion, "Asignación con División");
                    }
                    else
                    {
                        AddToken(TokenType.OpDivide, TokenCategory.OperadorAritmetico, "Operador División");
                    }
                    break;

                // Operadores relacionales y de asignación
                case '=':
                    if (Match('='))
                        AddToken(TokenType.OpEqual, TokenCategory.OperadorRelacional, "Operador Igualdad");
                    else
                        AddToken(TokenType.OpAssign, TokenCategory.OperadorAsignacion, "Operador Asignación");
                    break;

                case '!':
                    if (Match('='))
                        AddToken(TokenType.OpNotEqual, TokenCategory.OperadorRelacional, "Operador Diferente");
                    else
                        AddToken(TokenType.OpNot, TokenCategory.OperadorLogico, "Operador Lógico NOT");
                    break;

                case '<':
                    if (Match('='))
                        AddToken(TokenType.OpLessOrEqual, TokenCategory.OperadorRelacional, "Operador Menor o Igual");
                    else
                        AddToken(TokenType.OpLess, TokenCategory.OperadorRelacional, "Operador Menor Que");
                    break;

                case '>':
                    if (Match('='))
                        AddToken(TokenType.OpGreaterOrEqual, TokenCategory.OperadorRelacional, "Operador Mayor o Igual");
                    else
                        AddToken(TokenType.OpGreater, TokenCategory.OperadorRelacional, "Operador Mayor Que");
                    break;

                // Operadores lógicos
                case '&':
                    if (Match('&'))
                    {
                        AddToken(TokenType.OpAnd, TokenCategory.OperadorLogico, "Operador Lógico AND");
                    }
                    else
                    {
                        AddError("&", "Operador '&' incompleto", "Utilice '&&' para la operación lógica AND.");
                    }
                    break;

                case '|':
                    if (Match('|'))
                    {
                        AddToken(TokenType.OpOr, TokenCategory.OperadorLogico, "Operador Lógico OR");
                    }
                    else
                    {
                        AddError("|", "Operador '|' incompleto", "Utilice '||' para la operación lógica OR.");
                    }
                    break;

                // Espacios en blanco
                case ' ':
                case '\r':
                    // Ignorar
                    break;
                case '\t':
                    // Tabulación (avance de columna)
                    break;
                case '\n':
                    _line++;
                    _column = 1;
                    break;

                // Literales de cadena y carácter
                case '"':
                    ScanString();
                    break;

                case '\'':
                    ScanChar();
                    break;

                default:
                    if (IsDigit(c))
                    {
                        ScanNumber(false);
                    }
                    else if (IsAlpha(c))
                    {
                        ScanIdentifierOrKeyword();
                    }
                    else
                    {
                        // Emojis y otros símbolos ocupan 2 "char" en C#: se reportan como uno solo
                        if (char.IsHighSurrogate(c) && char.IsLowSurrogate(Peek()))
                        {
                            Advance();
                        }
                        AddError(_source.Substring(_start, _current - _start), "Carácter inesperado o no reconocido por el analizador", 
                            "Verifique si es un símbolo tipográfico accidental o reemplace por un operador válido.");
                    }
                    break;
            }
        }

        private void ScanLineComment()
        {
            while (Peek() != '\n' && !IsAtEnd())
            {
                Advance();
            }

            if (PreserveComments)
            {
                string text = _source.Substring(_start, _current - _start);
                AddToken(TokenType.CommentLine, TokenCategory.Comentario, "Comentario de una línea");
            }
        }

        private void ScanBlockComment()
        {
            int commentStartLine = _line;
            int commentStartCol = _startColumn;

            while (!IsAtEnd())
            {
                if (Peek() == '*' && PeekNext() == '/')
                {
                    Advance(); // Consumir '*'
                    Advance(); // Consumir '/'

                    if (PreserveComments)
                    {
                        AddToken(TokenType.CommentBlock, TokenCategory.Comentario, "Comentario de bloque");
                        // Un comentario de varias líneas se reporta en la línea donde inicia
                        _tokens[_tokens.Count - 1].Line = commentStartLine;
                    }
                    return;
                }

                if (Peek() == '\n')
                {
                    _line++;
                    _column = 0;
                }

                Advance();
            }

            // Si llegamos aquí, el comentario no se cerró
            _line = commentStartLine;          // reportar el error donde inicia el comentario
            _startColumn = commentStartCol;
            _current = _start + 2;             // resaltar solo el "/*" en el editor
            AddError("/*", "Comentario de bloque sin cerrar", 
                string.Format("El comentario iniciado en L:{0}, C:{1} no tiene cierre '*/' antes del fin de archivo.", commentStartLine, commentStartCol));
            _current = _source.Length;
        }

        private void ScanString()
        {
            int stringStartLine = _line;
            int stringStartCol = _startColumn;

            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n')
                {
                    // Error: cadena multilínea no permitida sin escape
                    AddError(_source.Substring(_start, _current - _start), "Cadena no terminada antes del fin de línea", "Cierre las comillas dobles antes del salto de línea.");
                    return; // el '\n' lo procesa ScanToken (ahí se incrementa la línea)
                }

                if (Peek() == '\\')
                {
                    Advance(); // Consumir '\'
                    if (!IsAtEnd())
                    {
                        Advance(); // Consumir carácter escapado (\n, \t, \", etc.)
                    }
                }
                else
                {
                    Advance();
                }
            }

            if (IsAtEnd())
            {
                AddError(_source.Substring(_start, _current - _start), "Cadena sin comilla de cierre al final del archivo", 
                    string.Format("La cadena abierta en L:{0}, C:{1} debe cerrarse con comillas dobles.", stringStartLine, stringStartCol));
                return;
            }

            // Consumir la comilla de cierre '"'
            Advance();

            AddToken(TokenType.LitString, TokenCategory.ConstanteCadena, "Literal de Cadena (string)");
        }

        private void ScanChar()
        {
            int charStartLine = _line;
            int charStartCol = _startColumn;

            if (IsAtEnd() || Peek() == '\n' || Peek() == '\'')
            {
                if (Peek() == '\'') Advance();
                AddError("''", "Literal de carácter vacío o inválido", "Un literal de carácter debe contener un carácter entre comillas simples (ejemplo: 'a').");
                return;
            }

            if (Peek() == '\\')
            {
                Advance(); // Consumir '\'
                if (!IsAtEnd()) Advance(); // Consumir escape
            }
            else
            {
                Advance(); // Consumir carácter
            }

            if (Peek() != '\'')
            {
                // Carácter múltiple o sin cierre
                while (Peek() != '\'' && Peek() != '\n' && !IsAtEnd())
                {
                    Advance();
                }

                if (Peek() == '\'') Advance();

                AddError(_source.Substring(_start, _current - _start), "Literal de carácter mal formado o sin cerrar", 
                    "Use comillas simples para exactamente un carácter o comillas dobles para cadenas.");
                return;
            }

            // Consumir la comilla simple de cierre
            Advance();
            AddToken(TokenType.LitChar, TokenCategory.ConstanteCaracter, "Literal de Carácter (char)");
        }

        private void ScanNumber(bool startsWithDot)
        {
            bool isDecimal = startsWithDot;

            while (IsDigit(Peek()))
            {
                Advance();
            }

            // Buscar parte decimal
            if (!startsWithDot && Peek() == '.' && IsDigit(PeekNext()))
            {
                isDecimal = true;
                Advance(); // Consumir el '.'

                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }

            // Verificar si hay notación científica: e.g. 1e10, 2.5e-3
            if ((Peek() == 'e' || Peek() == 'E') && (IsDigit(PeekNext()) || ((PeekNext() == '+' || PeekNext() == '-') && _current + 2 < _source.Length && IsDigit(_source[_current + 2]))))
            {
                isDecimal = true;
                Advance(); // Consumir 'e' o 'E'
                if (Peek() == '+' || Peek() == '-')
                {
                    Advance(); // Consumir signo
                }
                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }

            // Si después del número sigue inmediatamente una letra o un punto extra, es un error léxico
            if (IsAlpha(Peek()) || Peek() == '.')
            {
                while (IsAlphaNumeric(Peek()) || Peek() == '.')
                {
                    Advance();
                }

                string malformed = _source.Substring(_start, _current - _start);
                AddError(malformed, "Número mal formado o identificador inválido", 
                    "Los identificadores no pueden iniciar con dígitos y los números no deben contener caracteres alfabéticos contiguos.");
                return;
            }

            string number = _source.Substring(_start, _current - _start);

            if (isDecimal)
            {
                double value;
                bool ok = double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
                if (!ok || double.IsInfinity(value))
                {
                    AddError(number, "Constante decimal fuera de rango",
                        "El valor excede el máximo de un double (aprox. 1.7e308).");
                    return;
                }
                AddToken(TokenType.LitFloat, TokenCategory.ConstanteDecimal, "Constante Numérica Decimal");
            }
            else
            {
                long value;
                bool ok = long.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out value);
                if (!ok || value > int.MaxValue)
                {
                    AddError(number, "Constante entera fuera de rango",
                        "El valor máximo de un entero (int) es 2147483647.");
                    return;
                }
                AddToken(TokenType.LitInt, TokenCategory.ConstanteEntera, "Constante Numérica Entera");
            }
        }

        private void ScanIdentifierOrKeyword()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }

            string text = _source.Substring(_start, _current - _start);
            TokenType type;

            if (_keywords.TryGetValue(text, out type))
            {
                // Es una palabra reservada o literal booleano/nulo
                TokenCategory cat = TokenCategory.PalabraReservada;
                string desc = string.Format("Palabra Reservada '{0}'", text);

                if (type == TokenType.LitTrue || type == TokenType.LitFalse)
                {
                    cat = TokenCategory.ConstanteBooleana;
                    desc = "Constante Booleana (" + text + ")";
                }
                else if (type == TokenType.LitNull)
                {
                    cat = TokenCategory.PalabraReservada;
                    desc = "Literal Null";
                }

                AddToken(type, cat, desc);
            }
            else
            {
                // Es un identificador de usuario
                string inferredType = GetInferredTypeFromPreviousToken();
                _symbolTable.RegisterOrUpdate(text, _line, _startColumn, inferredType);

                AddToken(TokenType.Identifier, TokenCategory.Identificador, "Identificador de usuario");
            }
        }

        private string GetInferredTypeFromPreviousToken()
        {
            // int a, b, c;  ->  b y c también son int
            if (_lastTokenType == TokenType.Comma && _declarationType != null)
            {
                return _declarationType;
            }

            switch (_lastTokenType)
            {
                case TokenType.KwInt: return "int";
                case TokenType.KwFloat: return "float";
                case TokenType.KwDouble: return "double";
                case TokenType.KwChar: return "char";
                case TokenType.KwString: return "string";
                case TokenType.KwBool: return "bool";
                case TokenType.KwVoid: return "void";
                case TokenType.KwClass: return "class";
                case TokenType.KwFunction: return "function";
                default: return "Identificador";
            }
        }

        private void AddToken(TokenType type, TokenCategory category, string description)
        {
            string lexeme = _source.Substring(_start, _current - _start);
            Token token = new Token(
                _tokenCounter++,
                type,
                category,
                lexeme,
                _line,
                _startColumn,
                description
            );

            token.Position = _start;
            _tokens.Add(token);
            Stats.IncrementCategory(category);
            _lastTokenType = type;
            UpdateDeclarationType(type);
        }

        // Recuerda el tipo de la declaración actual hasta que termina (; ( ) { })
        private void UpdateDeclarationType(TokenType type)
        {
            switch (type)
            {
                case TokenType.KwInt: _declarationType = "int"; break;
                case TokenType.KwFloat: _declarationType = "float"; break;
                case TokenType.KwDouble: _declarationType = "double"; break;
                case TokenType.KwChar: _declarationType = "char"; break;
                case TokenType.KwString: _declarationType = "string"; break;
                case TokenType.KwBool: _declarationType = "bool"; break;
                case TokenType.Semicolon:
                case TokenType.OpenParen:
                case TokenType.CloseParen:
                case TokenType.OpenBrace:
                case TokenType.CloseBrace:
                    _declarationType = null;
                    break;
            }
        }

        private void AddError(string invalidLexeme, string message, string suggestion)
        {
            LexicalError err = new LexicalError(
                _errorCounter++,
                _line,
                _startColumn,
                invalidLexeme,
                message,
                suggestion
            );

            err.Position = _start;
            err.Length = Math.Max(1, _current - _start);
            _errors.Add(err);
            Stats.IncrementCategory(TokenCategory.Error);
        }

        private char Advance()
        {
            char c = _source[_current++];
            _column++;
            return c;
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            _current++;
            _column++;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _source[_current];
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }

        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   c == '_' ||
                   (c > 127 && char.IsLetter(c));   // letras con acento y ñ (año, posición)
        }

        private static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }
    }
}
