using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Data;

namespace NotationalFerocity.WPF
{
    /// <summary>
    /// An adorner class that contains a TextBox to provide editing capability 
    /// for an EditBox control. The editable TextBox resides in the 
    /// AdornerLayer. When the EditBox is in editing mode, the TextBox is given a size 
    /// it with desired size; otherwise, arrange it with size(0,0,0,0).
    /// </summary>
    internal sealed class EditBoxAdorner : Adorner
    {
        //Visual children
        private readonly VisualCollection _visualChildren;
        
        //The TextBox that this adorner covers.
        private readonly TextBox _textBox;
        
        //Whether the EditBox is in editing mode which means the Adorner 
        //is visible.
        private bool _isVisible;

        //Extra padding for the content when it is displayed in the TextBox
        private const double _extraWidth = 15;

        /// <summary>
        /// Inialize the EditBoxAdorner.
        /// </summary>
        public EditBoxAdorner(UIElement adornedElement, 
                              UIElement adorningElement): base(adornedElement)
        {
            _textBox = adorningElement as TextBox;

            Debug.Assert(_textBox != null, "No TextBox!");

            _visualChildren = new VisualCollection(this);

            BuildTextBox();
        }

        /// <summary>
        /// Specifies whether a TextBox is visible 
        /// when the IsEditing property changes.
        /// </summary>
        /// <param name="isVisible"></param>
        public void UpdateVisibilty(bool isVisible)
        {
            _isVisible = isVisible;

            InvalidateMeasure();
        }

        /// <summary>
        /// Override to measure elements.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            _textBox.IsEnabled = _isVisible;
            //if in editing mode, measure the space the adorner element 
            //should cover.

            if (!_isVisible)
            {
                return new Size(0, 0);
            }
            
            AdornedElement.Measure(constraint);
            _textBox.Measure(constraint);

            // Since the adorner is to cover the EditBox, it should return 
            // the AdornedElement.Width, the extra 15 is to make it more 
            // clear.
            return new Size(AdornedElement.DesiredSize.Width + _extraWidth,
                            _textBox.DesiredSize.Height);
        }

        /// <summary>
        /// override function to arrange elements.
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_isVisible)
            {
                _textBox.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            }
            else
            {
                // If not is editable mode, no need to show elements.
                _textBox.Arrange(new Rect(0, 0, 0, 0));
            }

            return finalSize;
        }

        /// <summary>
        /// override property to return infomation about visual tree.
        /// </summary>
        protected override int VisualChildrenCount 
        { 
            get
            {
                return _visualChildren.Count;
            } 
        }

        /// <summary>
        /// override function to return infomation about visual tree.
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            return _visualChildren[index];
        }

        /// <summary>
        /// Inialize necessary properties and hook necessary events on TextBox, 
        /// then add it into tree.
        /// </summary>
        private void BuildTextBox()
        {
            _visualChildren.Add(_textBox);

            //Bind Text onto AdornedElement.
            var binding = new Binding("Text")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Source = AdornedElement
            };

            _textBox.SetBinding(TextBox.TextProperty, binding);
        }
    }
}