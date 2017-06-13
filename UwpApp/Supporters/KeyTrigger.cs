using System;
using System.Reactive.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;

namespace SSH.Supporters
{
    public class KeyTrigger : Trigger
    {
        private IDisposable actionExecutor;

        protected override void OnAttached()
        {
            var fe = (FrameworkElement)AssociatedObject;
            var keyObs = Observable
                .FromEventPattern<KeyEventHandler, KeyRoutedEventArgs>(
                    handler => fe.KeyDown += handler,
                    handler => fe.KeyDown -= handler);

            actionExecutor = keyObs
                .Where(pattern => pattern.EventArgs.Key == Key)
                .Subscribe(d =>
                {
                    d.EventArgs.Handled = true;
                    Interaction.ExecuteActions(this, Actions, null);
                });
        }

        protected override void OnDetaching()
        {
            actionExecutor?.Dispose();
        }

        public VirtualKey Key { get; set; }
    }
}