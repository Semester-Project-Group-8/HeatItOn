using Avalonia.Media;

namespace Frontend.Models
{
    public class GeneratorLegendItem
    {
        public string Name { get; set; } = string.Empty;
        public IBrush Fill { get; set; } = Brushes.Transparent;
        public double Percentage { get; set; }
    }
}