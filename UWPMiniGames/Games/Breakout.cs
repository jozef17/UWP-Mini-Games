using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml.Input;

namespace UWPMiniGames.Games
{
    internal class Breakout : GameBase
    {
        // Current level
        private int Level;
        // Pad lication 
        private int PadLoc;
        // Ball coordinates
        private Vector2 BallLoc;
        private double Direction;
        private double SinD; // Sin and Cos of current ball direction 
        private double CosD; // (optimization for calculating only once - when direction changes)
        
        public Breakout() : base("breakout.dat", 16, 24, new Vector2(0, 0), 25) { }

        public override void HandleKey(KeyRoutedEventArgs e)
        {
            if (ended)
            {
                return;
            }

            if ((e.Key == VirtualKey.Left) || (e.Key == VirtualKey.A))
            {
                PadLoc = Math.Max(0, PadLoc - 1);
            }
            else if ((e.Key == VirtualKey.Right) || (e.Key == VirtualKey.D))
            {
                PadLoc = Math.Min(Width - 4, PadLoc + 1);
            }
        }

        public override void HandleTap(TappedRoutedEventArgs e)
        { }

        public override void LoadGame()
        {
            string[] state = GameUtil.ReadFile(SaveFile);

            // Load Game State
            LoadState(5, state);

            // Load Direction
            SetDirection(double.Parse(state[0]));
            // Load Pad location
            PadLoc = int.Parse(state[1]);
            // Load Ball Location
            float ballX = float.Parse(state[2]);
            float ballY = float.Parse(state[3]);
            BallLoc = new Vector2(ballX, ballY);
            // Load levek
            Level = int.Parse(state[4]);
        }

        public override void SaveGamne()
        {
            base.SaveGamne();
            if (!ended)
            {
                GameUtil.SaveFile(SaveFile,
                    Direction.ToString() + " " +
                    PadLoc.ToString() + " " +
                    BallLoc.X.ToString() + " " + 
                    BallLoc.Y.ToString() + " " +
                    Level.ToString() + " " +
                    SaveState());
            }
        }

        protected override void ProcessState()
        {
            // Get ne ball location
            float distance = 0.05f;
            float newX = BallLoc.X + distance * (float)CosD;
            float newY = BallLoc.Y + -distance * (float)SinD;

            newX = Math.Max(0, Math.Min(newX, Width - 1));
            newY = Math.Max(0, Math.Min(newY, Height - 2));

            if (!WasBorderHit(newX, newY))
            {
                IsEmpty(newX, newY);
            }

            BallLoc = new Vector2(newX, newY);
        }

        public override void ResetGame()
        {
            base.ResetGame();

            // Reset Pad and Ball state
            Level = 1;
            PadLoc = (Width / 2) - 2;
            BallLoc = new Vector2(PadLoc - 1, Height - 3);
            SetDirection(7.0 / 4.0 * Math.PI);

            // Reset map
            int?[,] map = GetMap();
            for (byte i = 0; i < Height - 2; i++)
            {
                for (byte j = 0; j < Width; j++)
                {
                    GameState[i, j] = map[i, j];
                }
            }
        }

        protected override void Render(CanvasDrawEventArgs args, Rect bounds, float ratio, Vector2 startPoint)
        {
            float a = UnitSize * ratio;

            for (byte i = 0; i < Height - 2; i++)
            {
                for (byte j = 0; j < Width; j++)
                {
                    if (GameState[i, j] != null && GameState[i, j] > 0)
                    {
                        args.DrawingSession.FillRectangle(startPoint.X + j * a + 1, startPoint.Y + i * a + 1, a - 2, a - 2, Color.FromArgb(255, 0, 0, 189)/*GetColor((int)GameState[i, j])*/);
                    }
                    else if (GameState[i, j] == null)
                    {
                        args.DrawingSession.FillRectangle(startPoint.X + j * a, startPoint.Y + i * a, a, a, Color.FromArgb(128, 128, 128, 189)/*GetColor((int)GameState[i, j])*/);
                    }
                }
            }

            // Ball
            args.DrawingSession.FillCircle(startPoint.X + BallLoc.X * a + a / 2,
                                           startPoint.Y + BallLoc.Y * a + a / 2,
                                           a / 2, Color.FromArgb(255, 255, 0, 0));

            // Pad
            args.DrawingSession.FillRectangle(startPoint.X + PadLoc * a,
                                              startPoint.Y + (Height - 1) * a,
                                              a * 4, a, Color.FromArgb(255, 255, 0, 0));
        }

        private void SetDirection(double newDirection)
        {
            // Keep direction in 0 - 2PI range
            if (newDirection < 0)
            {
                SetDirection(newDirection + 2 * Math.PI);
                return;
            }

            if (newDirection > 2 * Math.PI)
            {
                SetDirection(newDirection - 2 * Math.PI);
                return;
            }

            Direction = newDirection;
            SinD = Math.Sin(Direction);
            CosD = Math.Cos(Direction);
        }

        private int?[,] GetMap()
        {
            if (Level == 1)
            {
                int?[,] map = new int?[Height, Width];
                for (int i = 0; i < Height - 2; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        map[i, j] = i < 8 ? 1 : 0;
                    }
                }

                return map;
            }
            throw new Exception("Invalid Level");
        }

        private bool WasBorderHit(float newX, float newY)
        {
            // Make new angle bit random
            double rand = rnd.NextDouble() * 0.4 - 0.2;

            if (newY >= 14)
            {
                // Check padloc / endgame check
                if (newX < PadLoc - 0.5 || newX > PadLoc + 3.5)
                {
                    ended = true;
                    return true;
                }

                if (Direction >= 1.5 * Math.PI && Direction < 2 * Math.PI) // Down direction: 3/2PI - 2PI
                {
                    SetDirection(Direction + Math.PI / 2 + rand);
                    return true;
                }
                else if (Direction > Math.PI && Direction < 1.5 * Math.PI) // Down direction: PI - 3/2PI
                {
                    SetDirection(Direction - Math.PI / 2 + rand);
                    return true;
                }
                else
                {
                    throw new Exception("Invalid Direction: " + Direction + " for Y: " + newY);
                }
            }
            else if (newY <= 0)
            {
                if (Direction <= Math.PI / 2 && Direction > 0) // Up direction: 0 - PI/2
                {
                    SetDirection(Direction - Math.PI / 2 + rand);
                    return true;
                }
                else if (Direction > Math.PI / 2 && Direction < Math.PI) // Up direction: PI/2 - PI
                {
                    SetDirection(Direction + Math.PI / 2 + rand);
                    return true;
                }
                else
                {
                    throw new Exception("Invalid Direction: " + Direction + " for Y: " + newY);
                }
            }

            if (newX >= Width - 1)
            {
                if (Direction > 1.5 * Math.PI && Direction <= 2 * Math.PI) // Right direction: 3/2PI - 2PI
                {
                    SetDirection(Direction - Math.PI / 2 + rand);
                    return true;
                }
                else if (Direction < Math.PI / 2.0 && Direction >= 0) // Right direction: 0 - PI/2
                {
                    SetDirection(Direction + Math.PI / 2 + rand);
                    return true;
                }
                else
                {
                    throw new Exception("Invalid Direction: " + Direction + " for X: " + newX);
                }
            }
            else if (newX <= 0)
            {
                if (Direction > Math.PI / 2 && Direction <= Math.PI) // Left direction: PI/2 - PI
                {
                    SetDirection(Direction - Math.PI / 2 + rand);
                    return true;
                }
                else if (Direction > Math.PI && Direction <= 1.5 * Math.PI) // Left direction: 0 - PI/2
                {
                    SetDirection(Direction + Math.PI / 2 + rand);
                    return true;
                }
                else
                {
                    throw new Exception("Invalid Direction: " + Direction + " for X: " + newX);
                }
            }

            return false;
        }

        private bool IsEmpty(float newX, float newY)
        {
            if (newY > Height - 3)
            {
                return true;
            }

            // Make new angle bit random
            double rand = rnd.NextDouble() * 0.4 - 0.2;

            if (Direction <= Math.PI / 2 && Direction > 0)
            {
                int y = (int)newY;
                int x = (int)(newX + 0.5);

                // Above
                if (GameState[y, x] == null || GameState[y, x] > 0)
                {
                    SetDirection(Direction - Math.PI / 2 + rand);
                    GameState[y, x] = (GameState[y, x] == null) ? GameState[y, x] : 0;
                    return false;
                }

                // Right
                if (GameState[y + 1, ((int)(newX + 1))] == null || GameState[y + 1, ((int)(newX + 1))] > 0)
                {
                    SetDirection(Direction + Math.PI / 2 + rand);
                    GameState[y + 1, ((int)(newX + 1))] = (GameState[y + 1, ((int)(newX + 1))] == null) ? GameState[y + 1, ((int)(newX + 1))] : 0;
                    return false;
                }
            }
            else if (Direction <= Math.PI && Direction > Math.PI / 2)
            {
                int y = (int)newY;
                int x = (int)(newX + 0.5);

                // Above
                if (GameState[y, x] == null || GameState[y, x] > 0)
                {
                    SetDirection(Direction + Math.PI / 2 + rand);
                    GameState[y, x] = (GameState[y, x] == null) ? GameState[y, x] : 0;
                    return false;
                }
                // Left
                if (GameState[y + 1, ((int)(newX))] == null || GameState[y + 1, ((int)(newX))] > 0)
                {
                    SetDirection(Direction - Math.PI / 2 + rand);
                    GameState[y + 1, ((int)(newX))] = (GameState[y + 1, ((int)(newX))] == null) ? GameState[y + 1, ((int)(newX))] : 0;
                    return false;
                }
            }
            else if (Direction <= 1.5 * Math.PI && Direction > Math.PI)
            {
                int y = (int)newY + 1;
                int x = (int)(newX + 0.5);

                // Below
                if (GameState[y, x] == null || GameState[y, x] > 0)
                {
                    SetDirection(Direction - Math.PI / 2 + rand);
                    GameState[y, x] = (GameState[y, x] == null) ? GameState[y, x] : 0;
                    return false;
                }

                // Left
                if (GameState[y - 1, ((int)(newX))] == null || GameState[y - 1, ((int)(newX))] > 0)
                {
                    SetDirection(Direction + Math.PI / 2 + rand);
                    GameState[y - 1, ((int)(newX))] = (GameState[y - 1, ((int)(newX))] == null) ? GameState[y - 1, ((int)(newX))] : 0;
                    return false;
                }
            }
            else if (Direction <= 2 * Math.PI && Direction > 1.5 * Math.PI)
            {
                int y = (int)newY + 1;
                int x = (int)(newX + 0.5);

                // Below
                if (GameState[y, x] == null || GameState[y, x] > 0)
                {
                    SetDirection(Direction + Math.PI / 2 + rand);
                    GameState[y, x] = (GameState[y, x] == null) ? GameState[y, x] : 0;
                    return false;
                }

                // RIGHT
                if (GameState[y - 1, ((int)(newX + 1))] == null || GameState[y - 1, ((int)(newX + 1))] > 0)
                {
                    SetDirection(Direction - Math.PI / 2 + rand);
                    GameState[y - 1, ((int)(newX + 1))] = (GameState[y - 1, ((int)(newX + 1))] == null) ? GameState[y - 1, ((int)(newX + 1))] : 0;
                    return false;
                }
            }

            return true;
        }

    }
}
