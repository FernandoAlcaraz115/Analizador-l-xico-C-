using System;
using System.Collections.Generic;
using AnalizadorLexico.Model;

namespace AnalizadorLexico.Core
{
    /// <summary>
    /// Manejador de la Tabla de Símbolos para recopilar y consultar identificadores.
    /// </summary>
    public class SymbolTable
    {
        private readonly Dictionary<string, SymbolItem> _symbols;
        private int _nextId;

        public SymbolTable()
        {
            _symbols = new Dictionary<string, SymbolItem>(StringComparer.Ordinal);
            _nextId = 1;
        }

        public void Clear()
        {
            _symbols.Clear();
            _nextId = 1;
        }

        public SymbolItem RegisterOrUpdate(string name, int line, int column, string inferredType)
        {
            if (string.IsNullOrEmpty(name)) return null;

            SymbolItem item;
            if (_symbols.TryGetValue(name, out item))
            {
                item.AddOccurrence(line, column);
                if (item.InferredType == "Identificador" && !string.IsNullOrEmpty(inferredType) && inferredType != "Identificador")
                {
                    item.InferredType = inferredType;
                }
                return item;
            }

            item = new SymbolItem(_nextId++, name, line, column, inferredType);
            _symbols[name] = item;
            return item;
        }

        public bool Contains(string name)
        {
            return _symbols.ContainsKey(name);
        }

        public List<SymbolItem> GetAllSymbols()
        {
            List<SymbolItem> list = new List<SymbolItem>(_symbols.Values);
            list.Sort(delegate(SymbolItem a, SymbolItem b)
            {
                return a.Id.CompareTo(b.Id);
            });
            return list;
        }

        public int Count
        {
            get { return _symbols.Count; }
        }
    }
}
