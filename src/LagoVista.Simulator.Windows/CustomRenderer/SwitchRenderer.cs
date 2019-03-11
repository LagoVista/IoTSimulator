using LagoVista.Simulator.Windows.CustomRenderer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Switch), typeof(LGVSwitchRenderer))]

namespace LagoVista.Simulator.Windows.CustomRenderer
{
    
    public class LGVSwitchRenderer : ViewRenderer<Switch, ToggleSwitch>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var control = new ToggleSwitch();
                    control.Toggled += OnNativeToggled;
                    control.Loaded += OnControlLoaded;
                    control.ClearValue(ToggleSwitch.OnContentProperty);
                    control.ClearValue(ToggleSwitch.OffContentProperty);

                    SetNativeControl(control);
                }

                Control.IsOn = Element.IsToggled;               
            }
        }

        void OnNativeToggled(object sender, RoutedEventArgs routedEventArgs)
        {
            ((IElementController)Element).SetValueFromRenderer(Switch.IsToggledProperty, Control.IsOn);
        }

        void UpdateFlowDirection()
        {

        }

        void UpdateOnColor()
        {

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Switch.IsToggledProperty.PropertyName)
            {
                Control.IsOn = Element.IsToggled;
            }
            else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
            {
                UpdateFlowDirection();
            }
            else if (e.PropertyName == Switch.OnColorProperty.PropertyName)
                UpdateOnColor();
        }

        protected override bool PreventGestureBubbling { get; set; } = true;

        void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            UpdateOnColor();
            Control.Loaded -= OnControlLoaded;
        }

    }
}
