using System.Drawing;
using AnalizadorLexico.Model;

namespace AnalizadorLexico.UI
{
    /// <summary>
    /// Paleta de colores y estilos visuales para una interfaz moderna en modo oscuro.
    /// Inspirado en paletas Slate / Dark Studio.
    /// </summary>
    public static class Theme
    {
        // Fondos
        public static readonly Color BgDark = Color.FromArgb(15, 23, 42);       // #0f172a (Slate 900)
        public static readonly Color BgCard = Color.FromArgb(30, 41, 59);       // #1e293b (Slate 800)
        public static readonly Color BgCardHover = Color.FromArgb(51, 65, 85);  // #334155 (Slate 700)
        public static readonly Color BgInput = Color.FromArgb(15, 23, 42);      // #0f172a
        public static readonly Color BorderColor = Color.FromArgb(51, 65, 85);  // #334155

        // Textos
        public static readonly Color TextPrimary = Color.FromArgb(248, 250, 252); // #f8fafc
        public static readonly Color TextSecondary = Color.FromArgb(148, 163, 184); // #94a3b8
        public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);   // #64748b

        // Acentos
        public static readonly Color Primary = Color.FromArgb(59, 130, 246);     // #3b82f6 Azul
        public static readonly Color PrimaryHover = Color.FromArgb(37, 99, 235); // #2563eb
        public static readonly Color Success = Color.FromArgb(16, 185, 129);     // #10b981 Verde
        public static readonly Color Warning = Color.FromArgb(245, 158, 11);     // #f59e0b Ámbar
        public static readonly Color Danger = Color.FromArgb(239, 68, 68);       // #ef4444 Rojo
        public static readonly Color Purple = Color.FromArgb(139, 92, 246);      // #8b5cf6 Violeta

        // Fuentes
        public static readonly Font FontUi = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        public static readonly Font FontUiBold = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        public static readonly Font FontTitle = new Font("Segoe UI", 13.5f, FontStyle.Bold);
        public static readonly Font FontSubtitle = new Font("Segoe UI", 10.5f, FontStyle.Regular);
        public static readonly Font FontCode = new Font("Consolas", 10.5f, FontStyle.Regular);
        public static readonly Font FontBadge = new Font("Segoe UI", 8.5f, FontStyle.Bold);

        /// <summary>
        /// Obtiene un color identificador para resaltar visualmente cada categoría de token.
        /// </summary>
        public static Color GetCategoryColor(TokenCategory category)
        {
            switch (category)
            {
                case TokenCategory.PalabraReservada:
                    return Color.FromArgb(192, 132, 252); // Púrpura suave
                case TokenCategory.Identificador:
                    return Color.FromArgb(56, 189, 248);  // Cyan brillante
                case TokenCategory.ConstanteEntera:
                case TokenCategory.ConstanteDecimal:
                    return Color.FromArgb(251, 191, 36);  // Dorado / Ámbar
                case TokenCategory.ConstanteCadena:
                case TokenCategory.ConstanteCaracter:
                    return Color.FromArgb(52, 211, 153);  // Verde esmeralda
                case TokenCategory.ConstanteBooleana:
                    return Color.FromArgb(244, 114, 182); // Rosa neón
                case TokenCategory.OperadorAritmetico:
                case TokenCategory.OperadorRelacional:
                case TokenCategory.OperadorLogico:
                case TokenCategory.OperadorAsignacion:
                    return Color.FromArgb(248, 113, 113); // Coral / Rojo suave
                case TokenCategory.Delimitador:
                    return Color.FromArgb(226, 232, 240); // Blanco plateado
                case TokenCategory.Comentario:
                    return Color.FromArgb(148, 163, 184); // Gris
                case TokenCategory.Error:
                    return Color.FromArgb(239, 68, 68);   // Rojo error
                default:
                    return TextPrimary;
            }
        }
    }
}
