using System.Windows;
using System.Windows.Controls;

namespace ImageSearch
{
    class MarginWrapPanel: WrapPanel
    {
        public Thickness ItemMargin
        {
            get => itemMargin;
            set
            {
                itemMargin = value;
                foreach (UIElement u in Children)
                {
                    if (u is FrameworkElement f)
                    {
                        f.Margin = value;
                    }
                }
            }
        }
        private Thickness itemMargin;
    }
}
