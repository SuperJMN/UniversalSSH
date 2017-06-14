using System;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace SSH
{
    public class ScrollToEndBehavior : Behavior<ScrollViewer>
    {
        private IDisposable scroller;

        protected override void OnAttached()
        {
            scroller = Observable
                .Interval(TimeSpan.FromSeconds(0.2))
                .ObserveOnDispatcher()
                .Subscribe(_ => AssociatedObject.ScrollToEnd());
        }

        protected override void OnDetaching()
        {
            scroller?.Dispose();
        }
    }

    public static class ScrollExtensions
    {
        public static void ScrollToEnd(this ScrollViewer scrollViewer)
        {            
            scrollViewer.ChangeView(0.0f, scrollViewer.ExtentHeight, 1.0f);
        }
    }
}