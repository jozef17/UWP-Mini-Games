using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml.Input;

namespace UWPMiniGames
{
    enum Move : byte
    {
        Up, Down, Left, Right
    }

    public sealed class Snake : GameBase
    {
        // GUI elements
        private CanvasBitmap Head = null;

        // Game state
        private Move Direction = Move.Right;
        private int SnakeLength = 0;

        public Snake(CanvasControl canvas) : base("nake.dat", 10, 16, new Vector2(UnitSize, 10))
        {
            Task task = LoadResourcesAsync(canvas);
        }

        private async Task LoadResourcesAsync(CanvasControl canvas)
        {
            // Async load resources
            Head = await CanvasBitmap.LoadAsync(canvas, new Uri("ms-appx:///Assets/Snake/Head.png"));
        }


        public override void HandleTap(TappedRoutedEventArgs e) { }

        public override void LoadGame()
        {
            string[] state = GameUtil.ReadFile(SaveFile);

            // Load Game State
            LoadState(1, state);

            // Load Snake Directionm
            Direction = (Move)Enum.Parse(typeof(Move), state[0]);

            // Get Snake Lenght
            int? max = GameState.Cast<int?>().Max();
            SnakeLength = (int)max;
        }

        public override void ResetGame()
        {
            base.ResetGame();
            Direction = Move.Right;

            for (int i = 0; i < 3; i++)
                GameState[Height / 2, Width / 2 - i] = i + 1;
            SnakeLength = 3;

            // Generate point Position 
            (byte a, byte b) point = NextPoint();
            GameState[point.a, point.b] = 0;
        }

        public override void SaveGamne()
        {
            base.SaveGamne();

            if (!ended)
                GameUtil.SaveFile(SaveFile, Direction.ToString() + " " + SaveState());
        }

        protected override void Render(CanvasDrawEventArgs args, Rect bounds, float ratio, Vector2 startPoint)
        {
            // Display Gamne State
            float a = UnitSize * ratio;

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

        protected override void ProcessState()
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

        public override void HandleKey(KeyRoutedEventArgs e)
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
