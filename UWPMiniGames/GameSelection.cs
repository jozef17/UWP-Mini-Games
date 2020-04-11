using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWPMiniGames
{
    public sealed class GameSelection : State
    {
        public GameSelection(CanvasControl canvas, params Button[] buttons) : base(canvas, buttons)
        {
            VisibleButtons = new string[] { "Game1", "Game2", "Game3" };
        }

        public override State HandleClick(string buttonName)
        {
            IGame game = null;
            switch (buttonName)
            {
                case "Back":
                    return null;
                case "Game1":
                    game = new Snake(Canvas);
                    break;
                case "Game2":
                    // TODO
                    break;
                case "Game3":
                    game = new TicTacToe(Canvas);
                    break;
            }
            return new GameMenu(this, game);
        }

        protected override void Resize(double width, double height)
        {
            var buttons = from button in Buttons
                          where VisibleButtons.Contains(button.Name)
                          select button;

            foreach (Button b in buttons)
            {
                if (b.Name == "Game1")
                {
                    b.Margin = new Thickness(0.25 * width - 25, 0, 0, 0);
                    // b.Margin = new Thickness(0.25 * width - 150, 0, 0, 0);
                }
                else if (b.Name == "Game3")
                {
                    b.Margin = new Thickness(0, 0, 0.25 * width - 25, 0);
                    // b.Margin = new Thickness(0, 0, 0.25 * width - 150, 0);
                }
            }

        }

    }
}