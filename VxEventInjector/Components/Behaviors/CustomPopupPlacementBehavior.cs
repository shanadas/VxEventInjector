using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace VxEventInjector.Components.Behaviors
{
    /// <summary>
    /// Code for this class was found at http://www.shujaat.net/2014/03/wpf-custom-tooltip-placement-with-more.html.
    /// </summary>
    public class CustomPopupPlacementBehavior : Behavior<FrameworkElement>
    {
        public PopupHorizontalPlacement HorizontalPlacement { get; set; }
        public PopupVerticalPlacement VerticalPlacement { get; set; }
        public double HorizontalOffset { get; set; }

        public CustomPopupPlacementBehavior()
        {
            HorizontalOffset = 0;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject is BasePopupButton)
            {
                BasePopupButton button = AssociatedObject as BasePopupButton;
                button.PopupPlacement = PlacementMode.Custom;
                button.CustomPopupPlacementCallback = CalculatePopupPlacement;
            }
            else if (AssociatedObject is Popup)
            {
                Popup popup = AssociatedObject as Popup;
                popup.Placement = PlacementMode.Custom;
                popup.CustomPopupPlacementCallback = CalculatePopupPlacement;
            }
        }

        protected virtual CustomPopupPlacement[] CalculatePopupPlacement(Size popupSize, Size targetSize, Point offset)
        {
            double verticalOffset = GetVerticalOffset(VerticalPlacement, popupSize.Height, targetSize.Height, offset.Y);
            double horizontalOffset = GetHorizontalOffset(HorizontalPlacement, popupSize.Width, targetSize.Width, offset.X + HorizontalOffset);
            CustomPopupPlacement placement1 = new CustomPopupPlacement(new Point(horizontalOffset, verticalOffset), PopupPrimaryAxis.Horizontal);
            return new CustomPopupPlacement[] { placement1 };
        }

        private double GetHorizontalOffset(PopupHorizontalPlacement horizontalPlacement, double popupWidth, double targetWidth, double offsetWidth)
        {
            double horizontalOffset = offsetWidth;
            switch (horizontalPlacement)
            {
                case PopupHorizontalPlacement.Left:
                    horizontalOffset += -popupWidth;
                    break;

                case PopupHorizontalPlacement.Center:
                    horizontalOffset += targetWidth / 2 - popupWidth / 2;
                    break;

                case PopupHorizontalPlacement.Right:
                    horizontalOffset += targetWidth;
                    break;

                case PopupHorizontalPlacement.RightAligned:
                    horizontalOffset += targetWidth - popupWidth;
                    break;

                default:
                    throw new Exception("Invalid Vertical Placement");
            }
            return horizontalOffset;
        }

        private double GetVerticalOffset(PopupVerticalPlacement verticalPlacement, double popupHeight, double targetHeight, double offsetHeight)
        {
            double verticalOffset = offsetHeight;
            switch (verticalPlacement)
            {
                case PopupVerticalPlacement.Top:
                    verticalOffset += -popupHeight;
                    break;

                case PopupVerticalPlacement.Center:
                    verticalOffset += targetHeight / 2 - popupHeight / 2;
                    break;

                case PopupVerticalPlacement.Bottom:
                    verticalOffset += targetHeight;
                    break;

                default:
                    throw new Exception("Invalid Vertical Placement");
            }
            return verticalOffset;
        }
    }

    public enum PopupHorizontalPlacement
    {
        Left = 0, Center = 1, Right = 2, RightAligned = 3
    }

    public enum PopupVerticalPlacement
    {
        Top = 0, Center = 1, Bottom = 2
    }
}
