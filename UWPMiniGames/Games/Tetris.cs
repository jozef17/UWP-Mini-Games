using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml.Input;

namespace UWPMiniGames
{

    public sealed class Tetris : GameBase
    {
        private Tetrimino Current = null;

        public Tetris() : base("tetris.dat", 20, 12, new Vector2(0, 0), 400) { }

        public override void HandleKey(KeyRoutedEventArgs e)
        {
            if (ended)
                return;

            if (e.Key == VirtualKey.R)
            {
                Current.Rotate();
            }
            else if ((e.Key == VirtualKey.Left) || (e.Key == VirtualKey.A))
            {
                Current.MoveLeft();
            }
            else if ((e.Key == VirtualKey.Right) || (e.Key == VirtualKey.D))
            {
                Current.MoveRight();
            }
        }

        public override void HandleTap(TappedRoutedEventArgs e) { }

        public override void LoadGame()
        {
            string[] data = GameUtil.ReadFile(SaveFile);

            // Load Game State
            LoadState(4, data);
            Current = new Tetrimino(GameState, data[0], byte.Parse(data[1]), int.Parse(data[2]), int.Parse(data[3]));
        }

        protected override void Render(CanvasDrawEventArgs args, Rect bounds, float ratio, Vector2 startPoint)
        {
            float a = UnitSize * ratio;

            // Render Game state
            for (byte i = 0; i < Height; i++)
                for (byte j = 0; j < Width; j++)
                {
                    if (GameState[i, j] != null)
                    {
                        args.DrawingSession.FillRectangle(startPoint.X + j * a, startPoint.Y + i * a, a, a, GetColor((int)GameState[i, j]));
                    }

                }

            // Render Current Element
            if (ended)
                return;

            int[,] pos = Current.CurrentPosition;
            for (int i = 0; i < 4; i++)
                if (pos[i, 1] >= 0)
                    args.DrawingSession.FillRectangle(startPoint.X + pos[i, 0] * a, startPoint.Y + pos[i, 1] * a, a, a, GetColor(Current.Color));
            // TODO
        }

        private Color GetColor(int c)
        {
            switch (c)
            {
                case 0:
                    return Color.FromArgb(255, 88, 215, 189);
                case 1:
                    return Color.FromArgb(255, 0, 0, 255);
                case 2:
                    return Color.FromArgb(255, 255, 147, 0);
                case 3:
                    return Color.FromArgb(255, 255, 255, 0);
                case 4:
                    return Color.FromArgb(255, 0, 255, 0);
                case 5:
                    return Color.FromArgb(255, 128, 0, 128);
                case 6:
                    return Color.FromArgb(255, 255, 0, 0);
            }
            return Color.FromArgb(0, 0, 0, 0);
        }

        protected override void ProcessState()
        {
            if (ended)
                return;

            try
            {
                int i = Height - 1; ;
                int offset = 0;
                if (Current.MoveDown())
                {
                    while (i >= 0)
                    {
                        bool full = true;
                        // Check line
                        for (int j = 0; j < Width; j++)
                            if (GameState[i, j] == null)
                            {
                                full = false;
                                break;
                            }

                        if (full)
                            offset++;
                        // Move line down
                        for (int j = 0; j < Width; j++)
                        {
                            if (i - offset < 0)
                                GameState[i, j] = null;
                            else
                                GameState[i, j] = GameState[i - offset, j];
                        }

                        if (!full)
                            i--;
                    }

                    Current = new Tetrimino(GameState);
                }
            }
            catch (GameOverException e)
            {
                Current = null;
                ended = true;
            }
        }

        public override void ResetGame()
        {
            base.ResetGame();

            Current = new Tetrimino(GameState);
        }

        public override void SaveGamne()
        {
            base.SaveGamne();

            if (!ended)
                GameUtil.SaveFile(SaveFile, Current.Save() + " " + SaveState());
        }

    }

    sealed class Tetrimino
    {
        static Random rnd = new Random();
        enum Type
        {
            I = 0, J = 1, L = 2, O = 3, S = 4, T = 5, Z = 6
        }

        private int?[,] GameState;
        private int[,] Position;
        private int[,] Shape;
        private int[] Center;
        private byte Rotation = 0;
        private Type T;

        public int[,] CurrentPosition { get => Position; }

        public byte Color { get => (byte)T; }

        public Tetrimino(int?[,] gameState, string type, byte rot, int x, int y)
        {
            GameState = gameState;
            Center = new int[] { x, y };
            Rotation = rot;
            T = (Type)Enum.Parse(typeof(Type), type);

            // Restore state
            Shape = getMatrix();
            Position = getPosition();
        }

        public Tetrimino(int?[,] gameState)
        {
            GameState = gameState;
            Rotation = (byte)rnd.Next(0, 4);

            // Get Tetrimino Type
            var values = Enum.GetValues(typeof(Type));
            T = (Type)values.GetValue(rnd.Next(values.Length));
            Shape = getMatrix();

            // Set Center
            int width = gameState.GetLength(1);
            Center = new int[] { width / 2, 0 };
            Position = getPosition();

            // Evaluate position
            for (byte i = 0; i < 4; i++)
                if ((Position[i, 1] >= 0) && (gameState[Position[i, 1], Position[i, 0]] != null))
                    throw new GameOverException();
        }

        private int[,] getMatrix()
        {
            switch (T)
            {
                case Type.I:
                    return new int[,] { { -2, 0 }, { -1, 0 }, { 0, 0 }, { 1, 0 } };
                case Type.O:
                    return new int[,] { { -1, -1 }, { 0, -1 }, { -1, 0 }, { 0, 0 } };
                case Type.T:
                    return new int[,] { { -1, -1 }, { 0, -1 }, { 1, -1 }, { 0, 0 } };
                case Type.Z:
                    return new int[,] { { -1, -1 }, { 0, -1 }, { 0, 0 }, { 1, 0 } };
                case Type.S:
                    return new int[,] { { 1, -1 }, { 0, -1 }, { -1, 0 }, { 0, 0 } };
                case Type.J:
                    return new int[,] { { 0, -2 }, { 0, -1 }, { -1, 0 }, { 0, 0 } };
                case Type.L:
                    return new int[,] { { -1, -2 }, { -1, -1 }, { -1, 0 }, { 0, 0 } };
            }
            return null;
        }

        private int[,] getPosition()
        {
            int r = Rotation > 1 ? -1 : 1;
            int x = Rotation % 2 == 0 ? 0 : 1;
            int y = x == 0 ? 1 : 0;

            int[,] position = new int[4, 2];

            for (int i = 0; i < 4; i++)
            {
                position[i, 0] = Center[0] + r * Shape[i, x];
                position[i, 1] = Center[1] + r * Shape[i, y];
            }
            return position;
        }

        public void Rotate()
        {
            if (T == Type.O)
                return;

            Rotation = (byte)((Rotation + 1) % 4);
            int[,] newPosition = getPosition();

            int height = GameState.GetLength(0);
            int width = GameState.GetLength(1);

            for (byte i = 0; i < 4; i++)
            {
                if (newPosition[i, 1] < 0)
                    continue;
                if ((newPosition[i, 1] >= height) // Bottom border
                    || (newPosition[i, 0] < 0) // Left Border
                    || (newPosition[i, 0] >= width) // Right Border
                    || (GameState[newPosition[i, 1], newPosition[i, 0]] != null)) // Allready occupied 
                {
                    Rotation = (byte)((Rotation - 1) % 4);
                    return;
                }
            }

            Position = newPosition;
        }

        public void MoveLeft()
        {
            for (byte i = 0; i < 4; i++)
                if ((Position[i, 0] == 0) || ((Position[i, 1]) >= 0 && GameState[Position[i, 1], Position[i, 0] - 1] != null))
                    return;

            for (byte i = 0; i < 4; i++)
                Position[i, 0]--;
            Center[0]--;
        }

        public void MoveRight()
        {
            int width = GameState.GetLength(1);
            for (byte i = 0; i < 4; i++)
                if ((Position[i, 0] == (width - 1)) || ((Position[i, 1]) >= 0 && GameState[Position[i, 1], Position[i, 0] + 1] != null))
                    return;

            for (byte i = 0; i < 4; i++)
                Position[i, 0]++;
            Center[0]++;
        }

        public bool MoveDown()
        {
            int height = GameState.GetLength(0);

            bool end = false;
            bool over = false;

            for (byte i = 0; i < 4; i++)
            {
                if ((Position[i, 1] + 1) >= height)
                {
                    end = true;
                    break;
                }
                else if ((Position[i, 1] >= 0) && (GameState[Position[i, 1] + 1, Position[i, 0]] != null))
                {
                    end = true;
                    break;
                }
            }

            for (byte i = 0; i < 4; i++)
            {
                if (!end)
                    Position[i, 1]++;
                else if (Position[i, 1] >= 0)
                    GameState[Position[i, 1], Position[i, 0]] = (byte)Color;
                else if (Position[i, 1] < 0)
                    over = true;
            }

            if (over)
                throw new GameOverException();
            if (!end)
                Center[1]++;
            return end;
        }

        public string Save()
        {
            return T + " " + Rotation + " " + Center[0] + " " + Center[1];
        }

    }

    sealed class GameOverException : Exception { }

}