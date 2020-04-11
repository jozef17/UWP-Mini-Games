using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace UWPMiniGames
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// Curent GUI state
        private State CurrentState = null;

        /// Tracking timedifference between drawings
        private long LastDraw = 0;

        ///  Background Image
        private CanvasBitmap Background = null;
        Transform2DEffect Img = null;


        public MainPage()
        {
            this.InitializeComponent();

            // Set initials state & Reorganise Elements based on current width y Height
            CurrentState = new GameSelection(Canvas, Game1, /*Game2,*/ Game3, NewGame, ContinueGame, Back, Pause);
            CurrentState.SetState();

            // Add Window Changed "listener"
            Window.Current.SizeChanged += Canvas_Resize;
        }

        private void HandleClick(object sender, RoutedEventArgs e)
        {
            CurrentState = CurrentState.HandleClick(((Button)sender).Name);
            CurrentState.SetState();
        }

        private void Canvas_Resize(object sender, WindowSizeChangedEventArgs e)
        {
            // Resize Background Image
            if (Background != null)
            {
                Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
                Rect backBounds = Background.Bounds;

                float widthRatio = (float)bounds.Width / (float)backBounds.Width;
                float heightRatio = (float)bounds.Height / (float)backBounds.Height;

                Img = new Transform2DEffect() { Source = Background };
                Img.TransformMatrix = Matrix3x2.CreateScale(widthRatio, heightRatio);
            }

            CurrentState.Resize();
        }

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            // Get delta from last draw
            long mills = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long delta = mills - LastDraw;
            LastDraw = mills;

            // Set Background
            if (Img == null && Background != null)
                Canvas_Resize(null, null);
            if (Img != null)
                args.DrawingSession.DrawImage(Img);

            // Draw Game State
            if (LastDraw > 0)
            {
                CurrentState.Tick(args, delta);
            }

            Canvas.Invalidate();
        }

        private void Canvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CurrentState.HandleTap(e);
        }

        private void Canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(LoadBackgroundAsync(sender).AsAsyncAction());
        }

        async Task LoadBackgroundAsync(CanvasControl sender)
        {
            Background = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/image.png"));
            Canvas_Resize(null, null);
        }

        private void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                // Go back when pressed Esc
                State s = CurrentState.HandleClick("Back");
                if (s != null)
                {
                    CurrentState = s;
                    CurrentState.SetState();
                }
            }
            else
            {
                CurrentState.HandleKey(e);
            }
        }
    }
}
