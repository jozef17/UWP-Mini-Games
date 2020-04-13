using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace UWPMiniGames
{

    public sealed class Game : State
    {
        private IGame CurrentGame = null;

        public Game(State previousState, IGame game) : base(previousState)
        {
            VisibleButtons = new string[] { "Back", "Pause" };
            CurrentGame = game;
        }

        public override void HandleTap(TappedRoutedEventArgs e)
        {
            if (CurrentGame != null)
                CurrentGame.HandleTap(e);
        }

        public override void HandleKey(KeyRoutedEventArgs e)
        {
            if (CurrentGame != null)
                CurrentGame.HandleKey(e);
        }

        public override State HandleClick(string buttonName)
        {
            if (buttonName == "Back")
            {
                if (CurrentGame != null)
                {
                    CurrentGame.SaveGamne();
                    CurrentGame.ResetGame();
                }
                return PrevState;
            }
            return new Pause(this);
        }

        public override void Tick(CanvasDrawEventArgs args, long delta)
        {
            if (CurrentGame != null)
                CurrentGame.Render(args, delta);
        }

    }

}