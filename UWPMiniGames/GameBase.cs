using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using System.Text;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Input;

namespace UWPMiniGames
{
    public abstract class GameBase : IGame
    {
        protected const byte UnitSize = 100;

        // Save file name
        private string FileName;

        // Play Area
        private byte H, W; // Width & Height
        private Vector2 Offset;

        private int TimeLeft;
        private int WaitTime;

        // Game util
        protected Random rnd = new Random();

        // Game State
        protected int?[,] GameState;
        protected bool ended = false;

        protected byte Height { get => H; }
        protected byte Width { get => W; }
        protected string SaveFile { get => FileName; }

        public GameBase(string SaveFile, byte Height, byte Width, Vector2 offset, int waitTime = 200)
        {
            WaitTime = waitTime;
            FileName = SaveFile;
            Offset = offset;
            W = Width;
            H = Height;

            GameState = new int?[Height, Width];

            ResetGame();
        }

        public void Render(CanvasDrawEventArgs args, long delta)
        {
            TimeLeft -= (int)delta;

            if (!ended && (TimeLeft <= 0))
            {
                ProcessState();
                TimeLeft += WaitTime;
            }

            Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            float ratio = GetRatio(bounds);
            float a = UnitSize * ratio;
            Vector2 startPoint = GetStartPoint(bounds, ratio);

            args.DrawingSession.FillRectangle(startPoint.X, startPoint.Y, Width * a, Height * a, Color.FromArgb(50, 255, 255, 255));

            Render(args, bounds, ratio, startPoint);

            if (ended)
            {
                // Display Game Over
                args.DrawingSession.FillRectangle((float)bounds.Width / 2 - 300, (float)bounds.Height / 2 - 75, 600, 150, Color.FromArgb(255, 255, 0, 0));

                CanvasTextFormat format = new CanvasTextFormat() { FontSize = 92 };
                args.DrawingSession.DrawText("GAME OVER", (float)bounds.Width / 2 - 250, (float)bounds.Height / 2 - 65, Color.FromArgb(255, 0, 0, 0), format);
            }
        }

        protected Vector2 GetStartPoint(Rect canvasBounds, float ratio)
        {
            float a = ratio * UnitSize;

            float x = (float)canvasBounds.Width / 2 - ((Width * a) / 2) + Offset.X * ratio;
            float y = (float)canvasBounds.Height / 2 - ((Height * a) / 2) + Offset.Y * ratio;

            return new Vector2(x, y);
        }

        protected float GetRatio(Rect canvasBounds) => ((float)0.75 * (float)canvasBounds.Height / (Height * UnitSize));

        public virtual void ResetGame()
        {
            for (byte i = 0; i < Height; i++)
                for (byte j = 0; j < Width; j++)
                    GameState[i, j] = null;

            TimeLeft = 4 * WaitTime;
            ended = false;
        }

        public bool WasSaved() => GameUtil.FileExists(SaveFile);

        protected void LoadState(int offset, string[] data)
        {
            // Load Game State
            for (byte i = 0; i < Height; i++)
                for (byte j = 0; j < Width; j++)
                {
                    if (data[i * Width + j + offset] == "-1")
                        GameState[i, j] = null;
                    else
                        GameState[i, j] = int.Parse(data[i * Width + j + offset]);
                }
        }

        protected string SaveState()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    if (GameState[i, j] == null)
                        sb.Append("-1 ");
                    else
                        sb.Append(GameState[i, j].ToString() + " ");

            return sb.ToString();
        }


        public virtual void SaveGamne()
        {
            if (ended)
                GameUtil.DeleteFile(SaveFile);
        }

        protected abstract void Render(CanvasDrawEventArgs args, Rect bounds, float ratio, Vector2 startPoint);
        protected abstract void ProcessState();

        public abstract void HandleKey(KeyRoutedEventArgs e);
        public abstract void HandleTap(TappedRoutedEventArgs e);
        public abstract void LoadGame();
    }
}