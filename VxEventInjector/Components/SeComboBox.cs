using System.Windows;
using System.Windows.Controls;

namespace VxEventInjector.Components
{
    public class SeComboBox : ComboBox
    {
        #region ToggleButtonStyle
        public static readonly DependencyProperty ToggleButtonStyleProperty = DependencyProperty.Register("ToggleButtonStyle",
            typeof(Style), typeof(SeComboBox));
        public Style ToggleButtonStyle
        {
            get { return (Style)GetValue(ToggleButtonStyleProperty); }
            set { SetValue(ToggleButtonStyleProperty, value); }
        }
        #endregion
    }
}
