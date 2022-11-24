using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace UWPMiniGames
{
    public sealed class Pause : State
    {
        public Pause(State previousState) : base(previousState)
        {
            VisibleButtons = new string[] { "Back" };
        }

        public override State HandleClick(string buttonName)
        {
            return PrevState;
        }

        public override void Tick(CanvasDrawEventArgs args, long delta)
        {
            PrevState.Tick(args, 0);

            Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;

            // Display PAUSE TEXT
            args.DrawingSession.FillRectangle((float)bounds.Width / 2 - 300, (float)bounds.Height / 2 - 75, 600, 150, Color.FromArgb(255, 255, 0, 0));

            CanvasTextFormat format = new CanvasTextFormat() { FontSize = 92 };
            args.DrawingSession.DrawText("PAUSE", (float)bounds.Width / 2 - 125, (float)bounds.Height / 2 - 65, Color.FromArgb(255, 0, 0, 0), format);
        }

    }
}