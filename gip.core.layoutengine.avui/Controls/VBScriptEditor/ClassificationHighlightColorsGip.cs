using Avalonia.Media;
using AvaloniaEdit.Highlighting;
using RoslynPad.Editor;

namespace gip.core.layoutengine.avui
{
    public class ClassificationHighlightColorsGip : ClassificationHighlightColors
    {
        public ClassificationHighlightColorsGip()
        {
            DefaultBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.White) };
            TypeBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Lime) };
            CommentBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Gray) };
            XmlCommentBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Gray) };
            KeywordBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.DeepSkyBlue) };
            PreprocessorKeywordBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.Green) };
            StringBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.DarkOrange) };
            BraceMatchingBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.White), Background = new SimpleHighlightingBrush(Colors.Gray) };
            ParameterBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.DodgerBlue) };
        }
    }
}
