using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace UWPMiniGames
{
    public abstract class State
    {
        private State PreviousState = null;

        protected string[] VisibleButtons;
        protected Button[] Buttons;
        protected CanvasControl Canvas;

        public State PrevState { get => PreviousState; }


        public State(State previousState)
        {
            this.PreviousState = previousState;
            this.Buttons = previousState.Buttons;
            this.Canvas = previousState.Canvas;
        }

        public State(CanvasControl canvas, params Button[] buttons)
        {
            this.Buttons = buttons;
            this.Canvas = canvas;
        }


        public void Resize()
        {
            Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            double width = bounds.Width;
            double height = bounds.Height;

            Resize(width, height);
        }

        public void SetState()
        {
            Resize();

            var hiddenButtons = from button in Buttons
                                where !VisibleButtons.Contains(button.Name)
                                select button;

            foreach (Button b in hiddenButtons)
            {
                b.Visibility = Visibility.Collapsed;
                b.IsEnabled = false;
            }

            // Get Visible Buttons
            var visibleButtons = from button in Buttons
                                 where VisibleButtons.Contains(button.Name)
                                 select button;

            foreach (Button b in visibleButtons)
            {
                b.Visibility = Visibility.Visible;
                b.IsEnabled = true;
            }
        }

        public virtual void HandleTap(TappedRoutedEventArgs e) { }

        public virtual void HandleKey(KeyRoutedEventArgs e) { }


        public virtual void Tick(CanvasDrawEventArgs args, long delta) { }

        protected virtual void Resize(double width, double height) { }

        public abstract State HandleClick(String buttonName);

    }
}