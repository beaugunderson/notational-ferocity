using System.Windows.Controls;
using System.Windows.Input;

namespace NotationalFerocity.WPF
{
    /// <summary>
    /// ClickLabel provides a Label that focuses its target element when clicked.
    /// </summary>
    public class ClickLabel : Label
    {
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (Target != null)
            {
                Target.Focus();
            }

            base.OnMouseDown(e);
        }
    }
}