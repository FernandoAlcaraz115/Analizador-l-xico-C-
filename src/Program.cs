using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AnalizadorLexico.Core;
using AnalizadorLexico.Model;
using AnalizadorLexico.UI;

namespace AnalizadorLexico
{
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            // Si no se pasaron argumentos o se solicita explícitamente GUI:
            if (args.Length == 0 || (args.Length == 1 && (args[0] == "--gui" || args[0] == "-g")))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
                return 0;
            }

            // Modo Consola
            return RunConsoleMode(args);
        }

        static int RunConsoleMode(string[] args)
        {
            string filePath = null;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "--help" || arg == "-h" || arg == "/?")
                {
                    PrintHelp();
                    return 0;
                }
                else if (arg == "-f" || arg == "--file")
                {
                    if (i + 1 < args.Length)
                    {
                        filePath = args[++i];
                    }
                }
                else if (!arg.StartsWith("-"))
                {
                    filePath = arg;
                }
            }

            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("Error: Debe especificar un archivo de código para analizar en modo consola.");
                Console.WriteLine("Uso: AnalizadorLexico.exe <ruta_archivo>");
                Console.WriteLine("Para abrir la interfaz gráfica ejecute: AnalizadorLexico.exe");
                return 1;
            }

            if (!File.Exists(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: El archivo no existe: " + filePath);
                Console.ResetColor();
                return 1;
            }

            string source;
            try
            {
                source = File.ReadAllText(filePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error al leer el archivo: " + ex.Message);
                Console.ResetColor();
                return 1;
            }

            Console.WriteLine("================================================================================");
            Console.WriteLine("               ANALIZADOR LEXICO (SCANNER) - MODO CONSOLA                       ");
            Console.WriteLine("================================================================================");
            Console.WriteLine("Archivo analizado: " + Path.GetFullPath(filePath));
            Console.WriteLine("Tamano de archivo: " + source.Length + " caracteres");
            Console.WriteLine();

            Lexer lexer = new Lexer();
            lexer.Tokenize(source);

            // 1. Mostrar Tokens
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- TOKENS RECONOCIDOS ({0}) ---", lexer.Tokens.Count);
            Console.ResetColor();
            Console.WriteLine(string.Format("{0,-5} | {1,-6} | {2,-6} | {3,-18} | {4,-20} | {5}",
                "N°", "Linea", "Col", "Tipo Token", "Categoria", "Lexema"));
            Console.WriteLine(new string('-', 80));

            foreach (Token tok in lexer.Tokens)
            {
                ConsoleColor catColor = GetConsoleCategoryColor(tok.Category);
                Console.Write(string.Format("{0,-5} | {1,-6} | {2,-6} | ", tok.Index, tok.Line, tok.Column));
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(string.Format("{0,-18} | ", tok.Type));
                Console.ForegroundColor = catColor;
                Console.Write(string.Format("{0,-20} | ", tok.Category));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("'" + tok.Lexeme.Replace("\r", "\\r").Replace("\n", "\\n") + "'");
                Console.ResetColor();
            }

            Console.WriteLine();

            // 2. Mostrar Tabla de Símbolos
            List<SymbolItem> symbols = lexer.Symbols.GetAllSymbols();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("--- TABLA DE SIMBOLOS (IDENTIFICADORES: {0}) ---", symbols.Count);
            Console.ResetColor();
            Console.WriteLine(string.Format("{0,-4} | {1,-20} | {2,-12} | {3,-10} | {4}",
                "ID", "Identificador", "Tipo Inferido", "Apariciones", "Posiciones (L:C)"));
            Console.WriteLine(new string('-', 80));

            foreach (SymbolItem sym in symbols)
            {
                Console.WriteLine(string.Format("{0,-4} | {1,-20} | {2,-12} | {3,-10} | {4}",
                    sym.Id, sym.Name, sym.InferredType, sym.OccurrencesCount, sym.PositionsSummary));
            }

            Console.WriteLine();

            // 3. Mostrar Errores Léxicos
            if (lexer.Errors.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("--- ERRORES LEXICOS ENCONTRADOS ({0}) ---", lexer.Errors.Count);
                Console.ResetColor();
                Console.WriteLine(string.Format("{0,-4} | {1,-6} | {2,-6} | {3,-10} | {4}",
                    "N°", "Linea", "Col", "Lexema", "Descripcion y Sugerencia"));
                Console.WriteLine(new string('-', 80));

                foreach (LexicalError err in lexer.Errors)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("{0,-4} | {1,-6} | {2,-6} | '{3}' | {4}",
                        err.Index, err.Line, err.Column, err.InvalidLexeme, err.Message));
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("     >> Sugerencia: " + err.Suggestion);
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✔ No se detectaron errores lexicos en el codigo analizado.");
                Console.ResetColor();
                Console.WriteLine();
            }

            // 4. Métricas Finales
            Console.WriteLine("================================================================================");
            Console.WriteLine(string.Format("Resumen: {0} tokens | {1} errores | {2} simbolos | Tiempo: {3:0.00} ms",
                lexer.Stats.TotalTokens, lexer.Stats.TotalErrors, lexer.Stats.UniqueSymbols, lexer.Stats.ElapsedMilliseconds));
            Console.WriteLine("================================================================================");

            return lexer.Errors.Count > 0 ? 2 : 0;
        }

        static ConsoleColor GetConsoleCategoryColor(TokenCategory category)
        {
            switch (category)
            {
                case TokenCategory.PalabraReservada: return ConsoleColor.Magenta;
                case TokenCategory.Identificador: return ConsoleColor.Cyan;
                case TokenCategory.ConstanteEntera:
                case TokenCategory.ConstanteDecimal: return ConsoleColor.DarkYellow;
                case TokenCategory.ConstanteCadena:
                case TokenCategory.ConstanteCaracter: return ConsoleColor.Green;
                case TokenCategory.ConstanteBooleana: return ConsoleColor.DarkMagenta;
                case TokenCategory.OperadorAritmetico:
                case TokenCategory.OperadorRelacional:
                case TokenCategory.OperadorLogico:
                case TokenCategory.OperadorAsignacion: return ConsoleColor.Blue;
                case TokenCategory.Delimitador: return ConsoleColor.Gray;
                case TokenCategory.Comentario: return ConsoleColor.DarkGray;
                case TokenCategory.Error: return ConsoleColor.Red;
                default: return ConsoleColor.White;
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("Analizador Lexico en C#");
            Console.WriteLine("Modo de uso:");
            Console.WriteLine("  AnalizadorLexico.exe               (Abre la interfaz grafica)");
            Console.WriteLine("  AnalizadorLexico.exe <archivo>     (Ejecuta en consola sobre el archivo especificado)");
            Console.WriteLine("  AnalizadorLexico.exe -f <archivo>  (Ejecuta en consola especificando bandera de archivo)");
            Console.WriteLine("  AnalizadorLexico.exe --gui         (Fuerza apertura en modo interfaz grafica)");
            Console.WriteLine("  AnalizadorLexico.exe --help        (Muestra este mensaje de ayuda)");
        }
    }
}
