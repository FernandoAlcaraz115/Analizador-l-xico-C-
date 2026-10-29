using System.Collections.Generic;
using AnalizadorLexico.Model;

namespace AnalizadorLexico.Core
{
    /// <summary>
    /// Estadísticas acumuladas tras el análisis léxico de un texto fuente.
    /// </summary>
    public class LexerStats
    {
        public int TotalTokens { get; set; }
        public int TotalErrors { get; set; }
        public int TotalLines { get; set; }
        public int TotalCharacters { get; set; }
        public int UniqueSymbols { get; set; }
        public double ElapsedMilliseconds { get; set; }
        public Dictionary<TokenCategory, int> CategoryCounts { get; set; }

        public LexerStats()
        {
            CategoryCounts = new Dictionary<TokenCategory, int>();
        }

        public void IncrementCategory(TokenCategory category)
        {
            if (!CategoryCounts.ContainsKey(category))
            {
                CategoryCounts[category] = 0;
            }
            CategoryCounts[category]++;
        }
    }
}
