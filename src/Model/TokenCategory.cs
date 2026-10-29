namespace AnalizadorLexico.Model
{
    /// <summary>
    /// Categoría general a la que pertenece un token.
    /// Útil para clasificación rápida, filtrado y coloreado sintáctico.
    /// </summary>
    public enum TokenCategory
    {
        PalabraReservada,
        Identificador,
        ConstanteEntera,
        ConstanteDecimal,
        ConstanteCadena,
        ConstanteCaracter,
        ConstanteBooleana,
        OperadorAritmetico,
        OperadorRelacional,
        OperadorLogico,
        OperadorAsignacion,
        Delimitador,
        Comentario,
        FinDeArchivo,
        Error
    }
}
