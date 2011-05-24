using System;
using System.Windows;
using System.Windows.Controls;

namespace NotationalFerocity.WPF
{
    [Serializable]
    public class FontDefinition : TextBlock
    {
        [NonSerialized]
        internal static readonly DependencyProperty[] _properties = new[]
        {
            FontFamilyProperty,
            FontSizeProperty,
            FontStyleProperty,
            FontWeightProperty,
            FontStretchProperty
        };

        public object this[DependencyProperty index]
        {
            get
            {
                return GetValue(index);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}pt", FontFamily, FontSize);
        }

        public void ApplyToDependencyObject(DependencyObject control)
        {
            foreach (var property in _properties)
            {
                control.SetValue(property, GetValue(property));
            }
        }

        public static FontDefinition FromDependencyObject(DependencyObject control)
        {
            var fontDefinition = new FontDefinition();

            foreach (var property in _properties)
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