 using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Linq;
using UWPMiniGames.Games;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWPMiniGames
{
    public sealed class GameSelection : State
    {
        public GameSelection(CanvasControl canvas, params Button[] buttons) : base(canvas, buttons)
        {
            VisibleButtons = new string[] { "Game1", "Game2", "Game3", "Game4" };
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
                    game = new Tetris();
                    break;
                case "Game3":
                    game = new TicTacToe(Canvas);
                    break;
                case "Game4":
                    game = new Breakout();
                    break;
            }
            return new GameMenu(this, game);
        }

        protected override void Resize(double width, double height)
        {
            var buttons = from button in Buttons
                          where VisibleButtons.Contains(button.Name)
                          select button;

            double gap = (width - VisibleButtons.Length * 200) / 4.0;

            foreach (Button b in buttons)
            {
                if (b.Name == "Game1")
                {
                    b.Margin = new Thickness(0.5 * gap, 0, 0, 0);
                }
                else if (b.Name == "Game2")
                {
                    b.Margin = new Thickness(1.5 * gap + 200, 0, 0, 0);
                }
                else if (b.Name == "Game3")
                {
                    b.Margin = new Thickness(2.5 * gap + 400, 0, 0, 0);
                }
                else if (b.Name == "Game4")
                {
                    b.Margin = new Thickness(3.5 * gap + 600, 0, 0, 0);
                }
            }

        }

    }
}