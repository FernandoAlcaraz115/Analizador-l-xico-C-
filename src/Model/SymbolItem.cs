using System.Collections.Generic;

namespace AnalizadorLexico.Model
{
    /// <summary>
    /// Entrada individual en la Tabla de Símbolos para un identificador único.
    /// </summary>
    public class SymbolItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string InferredType { get; set; }
        public int OccurrencesCount { get; set; }
        public int FirstLine { get; set; }
        public int FirstColumn { get; set; }
        public List<string> Positions { get; set; }

        public SymbolItem()
        {
            Name = string.Empty;
            InferredType = "Indeterminado";
            Positions = new List<string>();
        }

        public SymbolItem(int id, string name, int firstLine, int firstColumn, string inferredType)
        {
            Id = id;
            Name = name;
            FirstLine = firstLine;
            FirstColumn = firstColumn;
            InferredType = string.IsNullOrEmpty(inferredType) ? "Identificador" : inferredType;
            OccurrencesCount = 1;
            Positions = new List<string>();
            Positions.Add(string.Format("L:{0}, C:{1}", firstLine, firstColumn));
        }

        public void AddOccurrence(int line, int column)
        {
            OccurrencesCount++;
            Positions.Add(string.Format("L:{0}, C:{1}", line, column));
        }

        public string PositionsSummary
        {
            get
            {
                if (Positions == null || Positions.Count == 0) return string.Empty;
                return string.Join("; ", Positions.ToArray());
            }
        }
    }
}
