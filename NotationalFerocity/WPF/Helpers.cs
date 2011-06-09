using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace NotationalFerocity.WPF
{
    public static class Helpers
    {
        public static FrameworkElement FindByName(string name, FrameworkElement root)
        {
            var tree = new Stack<FrameworkElement>();

            tree.Push(root);

            while (tree.Count > 0)
            {
                var current = tree.Pop();

                if (current.Name == name)
                {
                    return current;
                }

                int count = VisualTreeHelper.GetChildrenCount(current);

                for (int i = 0; i < count; ++i)
                {
                    var child = VisualTreeHelper.GetChild(current, i);

                    if (child is FrameworkElement)
                    {
                        tree.Push((FrameworkElement) child);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Walk visual tree to find the first DependencyObject 
        /// of the specific type.
        /// </summary>
        public static DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        {
            // Walk the visual tree to get the parent(ItemsControl) 
            // of this control
            var parent = startObject;

            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                {
                    break;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }
    }
}