using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Input;

namespace UWPMiniGames
{
    enum Move : byte
    {
        Up, Down, Left, Right
    }

    class Snake : IGame
    {
        private const string SaveFile = "snake.dat";

        // Play Area
        private const byte Height = 10;
        private const byte Width = 16;
        private const byte UnitSize = 100;
        private const byte WaitTime = 200;

        // Dot position
        Random rnd = new Random();

        // GUI elements
        private CanvasControl Canvas = null;
        private CanvasBitmap Head = null;

        // Game state
        private Move Direction = Move.Right;
        private int?[,] GameState = new int?[Height, Width];
        private int TimeLeft = WaitTime;
        private int SnakeLength = 0;
        private bool ended = false;


        public Snake(CanvasControl canvas)
        {
            Task task = LoadResourcesAsync(canvas);
            Canvas = canvas;
            ResetGame();
        }

        private async Task LoadResourcesAsync(CanvasControl canvas)
        {
            // Async load resources
            Head = await CanvasBitmap.LoadAsync(canvas, new Uri("ms-appx:///Assets/Snake/Head.png"));
        }


        public void HandleTap(TappedRoutedEventArgs e) { }

        public void LoadGame()
        {
            StreamReader file = new StreamReader(ApplicationData.Current.LocalFolder.Path + "\\" + SaveFile);
            string text = file.ReadLine();
            string[] state = text.Split(" ");
            file.Close();

            // Load Direction
            switch (state[0])
            {
                case "Up":
                    Direction = Move.Up;
                    break;
                case "Down":
                    Direction = Move.Down;
                    break;
                case "Left":
                    Direction = Move.Left;
                    break;
                case "Right":
                    Direction = Move.Right;
                    break;
            }

            // Load Game State
            for (byte i = 0; i < Height; i++)
                for (byte j = 0; j < Width; j++)
                {
                    if (state[i * Width + j + 1] == "-1")
                    {
                        GameState[i, j] = null;
                    }
                    else
                    {
                        GameState[i, j] = int.Parse(state[i * Width + j + 1]);
                        if (GameState[i, j] > SnakeLength)
                            SnakeLength = (int)GameState[i, j];
                    }
                }
        }

        public void ResetGame()
        {
            Direction = Move.Right;

            for (byte i = 0; i < Height; i++)
                for (byte j = 0; j < Width; j++)
                    GameState[i, j] = null;

            for (int i = 0; i < 3; i++)
                GameState[Height / 2, Width / 2 - i] = i + 1;
            SnakeLength = 3;

            // Generate point Position 
            (byte a, byte b) point = NextPoint();
            GameState[point.a, point.b] = 0;

            TimeLeft = 4 * WaitTime;
            ended = false;
        }

        public void SaveGamne()
        {
            if (!ended)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Direction.ToString() + " ");
                for (int i = 0; i < Height; i++)
                    for (int j = 0; j < Width; j++)
                        if (GameState[i, j] == null)
                            sb.Append("-1 ");
                        else
                            sb.Append(GameState[i, j].ToString() + " ");

                using (StreamWriter file = new StreamWriter(ApplicationData.Current.LocalFolder.Path + "\\" + SaveFile))
                    file.WriteLine(sb.ToString());

                return;
            }

            // Delete old save file
            if (File.Exists(ApplicationData.Current.LocalFolder.Path + "\\" + SaveFile))
                File.Delete(ApplicationData.Current.LocalFolder.Path + "\\" + SaveFile);
        }

        public bool WasSaved()
        {
            return File.Exists(ApplicationData.Current.LocalFolder.Path + "\\" + SaveFile);
        }

        public void Render(CanvasDrawEventArgs args, long delta)
        {
            TimeLeft -= (int)delta;

            // Evaluate game status
            if (!ended && (TimeLeft <= 0))
            {
                HandleMove();
                TimeLeft += WaitTime;
            }

            Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            float ratio = GetRatio(bounds);
            float a = UnitSize * ratio;
            Vector2 startPoint = GetStartPoint(bounds, ratio);

            args.DrawingSession.FillRectangle(startPoint.X, startPoint.Y, Width * a, Height * a, Color.FromArgb(50, 255, 255, 255));

            for (byte i = 0; i < Height; i++)
                for (byte j = 0; j < Width; j++)
                {
                    if (GameState[i, j] == null)
                    {
                        continue;
                    }
                    else if (GameState[i, j] == 0)
                    {
                        args.DrawingSession.FillCircle(startPoint.X + j * a + a / 2, startPoint.Y + i * a + a / 2, a / 2, Color.FromArgb(255, 255, 0, 0));
                    }
                    else if (GameState[i, j] == 1)
                    {
                        Rect headBounds = Head.Bounds;
                        float f = (UnitSize / (float)headBounds.Height) * ratio; ;
                        Transform2DEffect Img = new Transform2DEffect() { Source = Head };


                        switch (Direction)
                        {
                            case Move.Right:
                                Img.TransformMatrix = Matrix3x2.CreateScale(f, f);
                                args.DrawingSession.DrawImage(Img, startPoint.X + j * a, startPoint.Y + i * a);
                                break;
                            case Move.Left:
                                Img.TransformMatrix = Matrix3x2.CreateScale(f, f) * Matrix3x2.CreateRotation((float)(Math.PI));
                                args.DrawingSession.DrawImage(Img, startPoint.X + (j + 1) * a, startPoint.Y + (i + 1) * a);
                                break;
                            case Move.Down:
                                Img.TransformMatrix = Matrix3x2.CreateScale(f, f) * Matrix3x2.CreateRotation((float)(Math.PI / (double)2));
                                args.DrawingSession.DrawImage(Img, startPoint.X + (j + 1) * a, startPoint.Y + i * a);
                                break;
                            case Move.Up:
                                Img.TransformMatrix = Matrix3x2.CreateScale(f, f) * Matrix3x2.CreateRotation((float)(3 * Math.PI / (double)2));
                                args.DrawingSession.DrawImage(Img, startPoint.X + j * a, startPoint.Y + (i + 1) * a);
                                break;
                        }
                    }
                    else if (GameState[i, j] > 1)
                    {
                        args.DrawingSession.FillRectangle(startPoint.X + j * a, startPoint.Y + i * a, a, a, Color.FromArgb(255, 0, 255, 0));
                    }
                }

            if (ended)
            {
                // Display PAUSE TEXT
                args.DrawingSession.FillRectangle((float)bounds.Width / 2 - 300, (float)bounds.Height / 2 - 75, 600, 150, Color.FromArgb(255, 255, 0, 0));

                CanvasTextFormat format = new CanvasTextFormat() { FontSize = 92 };
                args.DrawingSession.DrawText("GAME OVER", (float)bounds.Width / 2 - 250, (float)bounds.Height / 2 - 65, Color.FromArgb(255, 0, 0, 0), format);
            }
        }

        private float GetRatio(Rect canvasBounds) => ((float)0.75 * (float)canvasBounds.Height / (float)(Height * UnitSize));        //  TODO !!!


        private Vector2 GetStartPoint(Rect canvasBounds, float ratio)
        {
            float a = ratio * UnitSize;

            float x = (float)canvasBounds.Width / 2 - ((Width * a) / 2) + a;
            float y = (float)canvasBounds.Height / 2 - ((Height * a) / 2) + ratio * 10;

            return new Vector2(x, y);
        }

        private (byte, byte) NextPoint()
        {
            while (true)
            {
                byte a = (byte)rnd.Next(0, Height);
                byte b = (byte)rnd.Next(0, Width);

                if (GameState[a, b] == null)
                    return (a, b);
            }
        }

        private void HandleMove()
        {
            int endX = -1, endY = -1;
            int headX = -1, headY = -1;

            // update snake body
            for (byte i = 0; i < Height; i++)
                for (byte j = 0; j < Width; j++)
                    if (GameState[i, j] == null)
                    {
                        continue;
                    }
                    else if (GameState[i, j] == 1)
                    {
                        // Store Head location
                        headY = i;
                        headX = j;
                    }
                    else if (GameState[i, j] == SnakeLength)
                    {
                        // Store Tail Location
                        endX = j;
                        endY = i;
                    }
                    else if ((GameState[i, j] > 1) && (GameState[i, j] < SnakeLength))
                    {
                        GameState[i, j] += 1;
                    }

            // Process Head Movement (get X,Y of new Head position)
            int x = -1, y = -1;
            switch (Direction)
            {
                case Move.Right:
                    y = headY;
                    x = headX + 1;
                    break;
                case Move.Left:
                    y = headY;
                    x = headX - 1;
                    break;
                case Move.Down:
                    y = headY + 1;
                    x = headX;
                    break;
                case Move.Up:
                    y = headY - 1;
                    x = headX;
                    break;
            }

            // Check Game Borders
            if ((x < 0) || (y < 0) || (x >= Width) || (y >= Height) || ((GameState[y, x] != null) && (GameState[y, x] > 0)))
                ended = true;

            // Evaluate Head position
            if (!ended)
            {
                switch (GameState[y, x])
                {
                    case null:
                        // Move Snake
                        GameState[y, x] = 1;
                        GameState[headY, headX] = 2;
                        GameState[endY, endX] = null;
                        break;
                    case 0:
                        // Snake ate 
                        GameState[y, x] = 1;
                        GameState[headY, headX] = 2;

                        GameState[endY, endX] = ++SnakeLength;

                        (byte a, byte b) point = NextPoint();
                        GameState[point.a, point.b] = 0;
                        break;
                }
            }
        }

        public void HandleKey(KeyRoutedEventArgs e)
        {
            // Change snake direction
            if (!ended)
                switch (e.Key)
                {
                    case VirtualKey.W:
                    case VirtualKey.Up:
                        Direction = Move.Up;
                        break;
                    case VirtualKey.A:
                    case VirtualKey.Left:
                        Direction = Move.Left;
                        break;
                    case VirtualKey.S:
                    case VirtualKey.Down:
                        Direction = Move.Down;
                        break;
                    case VirtualKey.D:
                    case VirtualKey.Right:
                        Direction = Move.Right;
                        break;
                }
        }
    }
}
