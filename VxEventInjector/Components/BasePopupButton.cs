using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace VxEventInjector.Components
{
    [TemplatePart(Name = "PART_button", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_popup", Type = typeof(Popup))]
    [ContentProperty("PopupContent")]
    public abstract class BasePopupButton : Control
    {
        static BasePopupButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BasePopupButton), new FrameworkPropertyMetadata(typeof(BasePopupButton)));
        }

        protected ToggleButton _button;
        protected Popup _popup;

        public event EventHandler<EventArgs> PopupOpened;
        public event EventHandler<EventArgs> PopupOpening;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Code to get the Template parts as instance member
            _button = GetTemplateChild("PART_button") as ToggleButton;
            _button.Checked += ToggleButton_Checked;
            _button.Unchecked += ToggleButton_Unchecked;

            _popup = GetTemplateChild("PART_popup") as Popup;
            if (_popup != null)
            {
                _popup.Opened += Popup_Opened;
                if (CustomPopupPlacementCallback != null)
                {
                    _popup.CustomPopupPlacementCallback = CustomPopupPlacementCallback;
                }
            }
        }

        void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (PopupOpening != null)
            {
                PopupOpening.Invoke(sender, e);
            }
            IsOpen = true;
        }

        void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            IsOpen = false;
        }

        protected virtual void Popup_Opened(object sender, EventArgs e)
        {
            if (PopupOpened != null)
            {
                PopupOpened.Invoke(sender, e);
            }
        }

        #region ButtonContent
        public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register("ButtonContent",
            typeof(object), typeof(BasePopupButton), null);

        public object ButtonContent
        {
            get { return (object)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }
        #endregion

        #region PopupContent
        public static readonly DependencyProperty PopupContentProperty = DependencyProperty.Register("PopupContent",
            typeof(object), typeof(BasePopupButton), null);

        public object PopupContent
        {
            get { return (object)GetValue(PopupContentProperty); }
            set { SetValue(PopupContentProperty, value); }
        }
        #endregion

        #region PopupPlacement
        public static readonly DependencyProperty PopupPlacementProperty = DependencyProperty.Register("PopupPlacement",
            typeof(PlacementMode), typeof(BasePopupButton), new PropertyMetadata(PlacementMode.Bottom));

        public PlacementMode PopupPlacement
        {
            get { return (PlacementMode)GetValue(PopupPlacementProperty); }
            set { SetValue(PopupPlacementProperty, value); }
        }
        #endregion

        #region PopupPlacementTarget
        public static readonly DependencyProperty PopupPlacementTargetProperty = DependencyProperty.Register("PopupPlacementTarget",
            typeof(UIElement), typeof(BasePopupButton));

        public UIElement PopupPlacementTarget
        {
            get { return (UIElement)GetValue(PopupPlacementTargetProperty); }
            set { SetValue(PopupPlacementTargetProperty, value); }
        }
        #endregion

        #region ButtonStyle
        public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register("ButtonStyle",
            typeof(Style), typeof(BasePopupButton));

        public Style ButtonStyle
        {
            get { return (Style)GetValue(ButtonStyleProperty); }
            set { SetValue(ButtonStyleProperty, value); }
        }
        #endregion

        #region IsOpen
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen",
            typeof(bool), typeof(BasePopupButton), new PropertyMetadata(false));

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        #endregion

        #region PopupAnimation
        public static readonly DependencyProperty PopupAnimationProperty = DependencyProperty.Register("PopupAnimation",
            typeof(PopupAnimation), typeof(BasePopupButton), new PropertyMetadata(PopupAnimation.Fade));

        public PopupAnimation PopupAnimation
        {
            get { return (PopupAnimation)GetValue(PopupAnimationProperty); }
            set { SetValue(PopupAnimationProperty, value); }
        }
        #endregion

        public CustomPopupPlacementCallback CustomPopupPlacementCallback
        {
            set;
            get;
        }

        /// <summary>
        /// Gets the popup associated with the button.
        /// </summary>
        public Popup Popup
        {
            get { return _popup; }
        }
    }
}
