using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWPMiniGames
{
    public sealed class GameMenu : State
    {
        private IGame Game = null;
        private bool WasSaved = false;

        public GameMenu(State PreviousState, IGame Game) : base(PreviousState)
        {
            this.Game = Game;

            WasSaved = this.Game != null && this.Game.WasSaved();
            if (WasSaved)
            {
                VisibleButtons = new string[] { "NewGame", "ContinueGame", "Back" };
            }
            else
            {
                VisibleButtons = new string[] { "NewGame", "Back" };
            }
        }

        public override State HandleClick(string buttonName)
        {
            if (buttonName == "Back")
            {
                return PrevState;
            }

            // Load saved Game state
            if (buttonName == "ContinueGame")
            {
                Game.LoadGame();
            }

            return new Game(this, Game);
        }

        protected override void Resize(double width, double height)
        {
            bool saved = Game != null && Game.WasSaved();

            var buttons = from button in Buttons
                          where VisibleButtons.Contains(button.Name)
                          select button;

            foreach (Button b in buttons)
            {
                if (saved)
                {
                    if (b.Name == "ContinueGame")
                    {
                        b.Margin = new Thickness(0, 150, 0, 0);
                    }
                    else if (b.Name == "NewGame")
                    {
                        b.Margin = new Thickness(0, 0, 0, 150);
                    }
                }
                else if (b.Name == "NewGame")
                {
                    b.Margin = new Thickness(0, 0, 0, 0);
                }
            }
        }

        public override void Tick(CanvasDrawEventArgs args, long delta)
        {
            bool saved = (Game != null && Game.WasSaved());
            if (WasSaved != saved)
            {
                if (saved)
                    VisibleButtons = new string[] { "NewGame", "ContinueGame", "Back" };
                else
                    VisibleButtons = new string[] { "NewGame", "Back" };
                WasSaved = saved;
                SetState();
            }
        }

    }
}