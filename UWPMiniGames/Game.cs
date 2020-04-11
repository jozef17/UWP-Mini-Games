using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace UWPMiniGames
{

    public class Game : State
    {
        private IGame game = null;

        public Game(State previousState, IGame game) : base(previousState)
        {
            VisibleButtons = new string[] { "Back", "Pause" };
            this.game = game;
        }

        public override void HandleTap(TappedRoutedEventArgs e)
        {
            if (game != null)
            {
                game.HandleTap(e);
            }
        }

        public override void HandleKey(KeyRoutedEventArgs e)
        {
            if (game != null)
            {
                game.HandleKey(e);
            }
        }

        public override State HandleClick(string buttonName)
        {
            if (buttonName == "Back")
            {
                if (game != null)
                {
                    game.SaveGamne();
                    game.ResetGame();
                }
                return PrevState;
            }
            return new Pause(this);
        }

        public override void Tick(CanvasDrawEventArgs args, long delta)
        {
            if (game != null)
            {
                game.Render(args, delta);
            }
        }

    }

}