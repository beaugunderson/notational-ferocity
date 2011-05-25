using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NotationalFerocity.WPF
{
    public class FontDefinition : TextBlock
    {
        internal static readonly DependencyProperty[] _properties = new[]
        {
            FontFamilyProperty,
            FontSizeProperty,
            FontStyleProperty,
            FontWeightProperty,
            FontStretchProperty
        };

        public override string ToString()
        {
            return string.Format("{0}, {1}pt", GetValue(FontFamilyProperty), GetValue(FontSizeProperty));
        }

        public void ApplyToDependencyObject(DependencyObject control)
        {
            foreach (var property in _properties
                .Where(property => property != null))
            {
                control.SetValue(property, GetValue(property));
            }
        }

        public static FontDefinition FromDependencyObject(DependencyObject control)
        {
            var fontDefinition = new FontDefinition();

            foreach (var property in _properties
                .Where(property => property != null))
            {
                fontDefinition.SetValue(property, control.GetValue(property));
            }

            return fontDefinition;
        }

        public DependencyObject ToDependencyObject()
        {
            var control = new DependencyObject();

            ApplyToDependencyObject(control);

            return control;
        }
    }
}