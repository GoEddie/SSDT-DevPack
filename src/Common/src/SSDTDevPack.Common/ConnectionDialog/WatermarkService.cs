using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace SSDTDevPack.Common.ConnectionDialog
{
    /// <summary>
    /// Class that provides the Watermark attached property
    /// </summary>
    public static class WatermarkService
    {
        /// <summary>
        /// Watermark Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
            "Watermark",
            typeof (object),
            typeof (WatermarkService),
            new FrameworkPropertyMetadata((object) null, new PropertyChangedCallback(OnWatermarkChanged)));

        #region Private Fields

        /// <summary>
        /// Dictionary of ItemsControls
        /// </summary>
        private static readonly Dictionary<object, ItemsControl> itemsControls = new Dictionary<object, ItemsControl>();

        #endregion

        /// <summary>
        /// Gets the Watermark property.  This dependency property indicates the watermark for the control.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> to get the property from</param>
        /// <returns>The value of the Watermark property</returns>
        public static object GetWatermark(DependencyObject d)
        {
            return (object) d.GetValue(WatermarkProperty);
        }

        /// <summary>
        /// Sets the Watermark property.  This dependency property indicates the watermark for the control.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> to set the property on</param>
        /// <param name="value">value of the property</param>
        public static void SetWatermark(DependencyObject d, object value)
        {
            d.SetValue(WatermarkProperty, value);
        }

        /// <summary>
        /// Handles changes to the Watermark property.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> that fired the event</param>
        /// <param name="e">A <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Control control = (Control) d;
            control.Loaded += Control_Loaded;

            if (d is ComboBox || d is TextBox)
            {
                control.GotKeyboardFocus += Control_GotKeyboardFocus;
                control.LostKeyboardFocus += Control_Loaded;
            }

            if (d is ItemsControl && !(d is ComboBox))
            {
                ItemsControl i = (ItemsControl) d;

                // for Items property  
                i.ItemContainerGenerator.ItemsChanged += ItemsChanged;
                itemsControls.Add(i.ItemContainerGenerator, i);

                // for ItemsSource property  
                DependencyPropertyDescriptor prop =
                    DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, i.GetType());
                prop.AddValueChanged(i, ItemsSourceChanged);
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handle the GotFocus event on the control
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private static void Control_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            Control c = (Control) sender;
            if (ShouldShowWatermark(c))
            {
                RemoveWatermark(c);
            }
        }

        /// <summary>
        /// Handle the Loaded and LostFocus event on the control
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private static void Control_Loaded(object sender, RoutedEventArgs e)
        {
            Control control = (Control) sender;
            if (ShouldShowWatermark(control))
            {
                ShowWatermark(control);
            }
        }

        /// <summary>
        /// Event handler for the items source changed event
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private static void ItemsSourceChanged(object sender, EventArgs e)
        {
            ItemsControl c = (ItemsControl) sender;
            if (c.ItemsSource != null)
            {
                if (ShouldShowWatermark(c))
                {
                    ShowWatermark(c);
                }
                else
                {
                    RemoveWatermark(c);
                }
            }
            else
            {
                ShowWatermark(c);
            }
        }

        /// <summary>
        /// Event handler for the items changed event
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="ItemsChangedEventArgs"/> that contains the event data.</param>
        private static void ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            ItemsControl control;
            if (itemsControls.TryGetValue(sender, out control))
            {
                if (ShouldShowWatermark(control))
                {
                    ShowWatermark(control);
                }
                else
                {
                    RemoveWatermark(control);
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Remove the watermark from the specified element
        /// </summary>
        /// <param name="control">Element to remove the watermark from</param>
        private  static void RemoveWatermark(UIElement control)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer != null)
            {
                Adorner[] adorners = layer.GetAdorners(control);
                if (adorners == null)
                {
                    return;
                }

                foreach (Adorner adorner in adorners)
                {
                    if (adorner is WatermarkAdorner)
                    {
                        adorner.Visibility = Visibility.Hidden;
                        layer.Remove(adorner);
                    }
                }
            }
        }

        /// <summary>
        /// Show the watermark on the specified control
        /// </summary>
        /// <param name="control">Control to show the watermark on</param>
        private static void ShowWatermark(Control control)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer != null)
            {
                layer.Add(new WatermarkAdorner(control, GetWatermark(control)));
            }
        }

        /// <summary>
        /// Indicates whether or not the watermark should be shown on the specified control
        /// </summary>
        /// <param name="c"><see cref="Control"/> to test</param>
        /// <returns>true if the watermark should be shown; false otherwise</returns>
        private static bool ShouldShowWatermark(Control c)
        {
            if (c is ComboBox)
            {
                return (c as ComboBox).Text == string.Empty;
            }
            else if (c is TextBoxBase)
            {
                return (c as TextBox).Text == string.Empty;
            }
            else if (c is ItemsControl)
            {
                return (c as ItemsControl).Items.Count == 0;
            }
            else
            {
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Adorner for the watermark
        /// </summary>
        internal class WatermarkAdorner : Adorner
        {
            #region Private Fields

            /// <summary>
            /// <see cref="ContentPresenter"/> that holds the watermark
            /// </summary>
            private readonly ContentPresenter contentPresenter;

            #endregion

            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="WatermarkAdorner"/> class
            /// </summary>
            /// <param name="adornedElement"><see cref="UIElement"/> to be adorned</param>
            /// <param name="watermark">The watermark</param>
            public WatermarkAdorner(UIElement adornedElement, object watermark) :
                base(adornedElement)
            {
                this.IsHitTestVisible = false;

                this.contentPresenter = new ContentPresenter();
                this.contentPresenter.Content = watermark;
                this.contentPresenter.Opacity = 0.5;
                this.contentPresenter.Margin = new Thickness(Control.Margin.Left + Control.Padding.Left,
                    Control.Margin.Top + Control.Padding.Top, 0, 0);

                if (this.Control is ItemsControl && !(this.Control is ComboBox))
                {
                    this.contentPresenter.VerticalAlignment = VerticalAlignment.Center;
                    this.contentPresenter.HorizontalAlignment = HorizontalAlignment.Center;
                }

                // Hide the control adorner when the adorned element is hidden
                Binding binding = new Binding("IsVisible");
                binding.Source = adornedElement;
                binding.Converter = new BooleanToVisibilityConverter();
                this.SetBinding(VisibilityProperty, binding);
            }

            #endregion

            #region Protected Properties

            /// <summary>
            /// Gets the number of children for the <see cref="ContainerVisual"/>.
            /// </summary>
            protected override int VisualChildrenCount
            {
                get { return 1; }
            }

            #endregion

            #region Private Properties

            /// <summary>
            /// Gets the control that is being adorned
            /// </summary>
            private Control Control
            {
                get { return (Control) this.AdornedElement; }
            }

            #endregion

            #region Protected Overrides

            /// <summary>
            /// Returns a specified child <see cref="Visual"/> for the parent <see cref="ContainerVisual"/>.
            /// </summary>
            /// <param name="index">A 32-bit signed integer that represents the index value of the child <see cref="Visual"/>. The value of index must be between 0 and <see cref="VisualChildrenCount"/> - 1.</param>
            /// <returns>The child <see cref="Visual"/>.</returns>
            protected override Visual GetVisualChild(int index)
            {
                return this.contentPresenter;
            }

            /// <summary>
            /// Implements any custom measuring behavior for the adorner.
            /// </summary>
            /// <param name="constraint">A size to constrain the adorner to.</param>
            /// <returns>A <see cref="Size"/> object representing the amount of layout space needed by the adorner.</returns>
            protected override Size MeasureOverride(Size constraint)
            {
                // Here's the secret to getting the adorner to cover the whole control
                this.contentPresenter.Measure(Control.RenderSize);
                return Control.RenderSize;
            }

            /// <summary>
            /// When overridden in a derived class, positions child elements and determines a size for a <see cref="FrameworkElement"/> derived class. 
            /// </summary>
            /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
            /// <returns>The actual size used.</returns>
            protected override Size ArrangeOverride(Size finalSize)
            {
                this.contentPresenter.Arrange(new Rect(finalSize));
                return finalSize;
            }

            #endregion
        }
    }
}