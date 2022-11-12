using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace UWPMiniGames
{
    public interface IGame
    {

        bool WasSaved();

        void ResetGame();

        void LoadGame();

        void SaveGamne();

        void Render(CanvasDrawEventArgs args, long delta);

        void HandleTap(TappedRoutedEventArgs e);

        void HandleKey(KeyRoutedEventArgs e);
    }

}
