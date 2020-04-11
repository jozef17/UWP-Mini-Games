using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Input;

namespace UWPMiniGames
{
    enum Result : byte
    {
        XWin, OWin, Game, NoWin
    }

    public class TicTacToe : IGame
    {
        private const string SaveFile = "tictactoe.dat";

        // GUI elements
        private CanvasControl Canvas = null;

        private CanvasBitmap Grid = null;
        private CanvasBitmap X = null;
        private CanvasBitmap O = null;

        // Game Status
        private Result State = Result.Game;

        private byte?[,] Game = new byte?[3, 3] { { null, null, null },
                                                  { null, null, null },
                                                  { null, null, null } };
        private byte move = 0;

        TicTacToeMinimax AI = new TicTacToeMinimax();

        public TicTacToe(CanvasControl canvas)
        {
            Task task = LoadResourcesAsync(canvas);
            Canvas = canvas;
        }

        private async Task LoadResourcesAsync(CanvasControl canvas)
        {
            // Async load resources
            Grid = await CanvasBitmap.LoadAsync(canvas, new Uri("ms-appx:///Assets/TicTacToe/Grid.png"));
            X = await CanvasBitmap.LoadAsync(canvas, new Uri("ms-appx:///Assets/TicTacToe/X.png"));
            O = await CanvasBitmap.LoadAsync(canvas, new Uri("ms-appx:///Assets/TicTacToe/O.png"));
        }

        public bool WasSaved()
        {
            return File.Exists(ApplicationData.Current.LocalFolder.Path + "\\" + SaveFile);
        }

        public void ResetGame()
        {
            for (byte i = 0; i < 3; i++)
                for (byte j = 0; j < 3; j++)
                    Game[i, j] = null;

            AI = new TicTacToeMinimax();
            State = Result.Game;
            move = 0;
        }

        public void LoadGame()
        {
            StreamReader file = new StreamReader(ApplicationData.Current.LocalFolder.Path + "\\" + SaveFile);
            string text = file.ReadLine();
            string[] state = text.Split(" ");
            file.Close();

            byte max = 0;
            byte[,] moves = new byte[9, 2];
            for (byte i = 0; i < 3; i++)
                for (byte j = 0; j < 3; j++)
                    if (state[i * 3 + j] == "-1")
                    {
                        Game[i, j] = null;
                    }
                    else
                    {
                        Game[i, j] = byte.Parse(state[i * 3 + j]);
                        moves[(byte)Game[i, j], 0] = i;
                        moves[(byte)Game[i, j], 1] = j;

                        if (Game[i, j] > max)
                            max = (byte)Game[i, j];
                    }

            for (byte i = 0; i <= max; i++)
                AI.goTo(moves[i, 0], moves[i, 1]); ;
        }

        public void SaveGamne()
        {
            // Dont save if game didn't started
            if ((move > 0) && (State == Result.Game))
            {
                // Convert State to String
                StringBuilder sb = new StringBuilder();
                for (byte i = 0; i < 3; i++)
                    for (byte j = 0; j < 3; j++)
                        if (Game[i, j] == null)
                            sb.Append("-1 ");
                        else
                            sb.Append(Game[i, j].ToString() + " ");

                // Save State
                using (StreamWriter file = new StreamWriter(ApplicationData.Current.LocalFolder.Path + "\\" + SaveFile))
                    file.WriteLine(sb.ToString());

                return;
            }

            // Delete old save file
            if (File.Exists(ApplicationData.Current.LocalFolder.Path + "\\" + SaveFile))
                File.Delete(ApplicationData.Current.LocalFolder.Path + "\\" + SaveFile);
        }


        public void Render(CanvasDrawEventArgs args, long delta)
        {
            Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            Rect imgBounds = Grid.Bounds;

            // Calculate scale
            float ratio = GetRatio(bounds, imgBounds);
            Matrix3x2 scale = Matrix3x2.CreateScale(ratio, ratio);

            // Draw TicTacToe Grid
            Transform2DEffect Img = new Transform2DEffect() { Source = Grid };
            Img.TransformMatrix = scale;

            Vector2 startPoint = GetStartPoint(bounds, imgBounds, ratio);
            args.DrawingSession.DrawImage(Img, startPoint);

            // Draw State
            for (byte i = 0; i < 3; i++)
                for (byte j = 0; j < 3; j++)
                {
                    Vector2 location = startPoint + new Vector2(i * ratio * 166, j * ratio * 166);

                    if (Game[i, j] % 2 == 0)
                    {
                        Transform2DEffect ImgX = new Transform2DEffect() { Source = X };
                        ImgX.TransformMatrix = scale;

                        args.DrawingSession.DrawImage(ImgX, location);
                    }
                    else if (Game[i, j] % 2 == 1)
                    {
                        Transform2DEffect ImgO = new Transform2DEffect() { Source = O };
                        ImgO.TransformMatrix = scale;

                        args.DrawingSession.DrawImage(ImgO, location);
                    }
                }

            // Display Game result
            if (State != Result.Game)
            {
                // Display PAUSE TEXT
                args.DrawingSession.FillRectangle((float)bounds.Width / 2 - 300, (float)bounds.Height / 2 - 75, 600, 150, Color.FromArgb(255, 255, 0, 0));

                CanvasTextFormat format = new CanvasTextFormat() { FontSize = 92 };
                args.DrawingSession.DrawText("GAME OVER", (float)bounds.Width / 2 - 250, (float)bounds.Height / 2 - 65, Color.FromArgb(255, 0, 0, 0), format);
            }
        }

        public void HandleTap(TappedRoutedEventArgs e)
        {
            if (State != Result.Game)
                return;

            // Get Bounds
            Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            Rect imgBounds = Grid.Bounds;

            // Get Startingpoint & Ratio
            float ratio = GetRatio(bounds, imgBounds);
            Vector2 startPoint = GetStartPoint(bounds, imgBounds, ratio);
            Point tapped = e.GetPosition(Canvas);

            // Tapped outside of grid
            if ((tapped.X < startPoint.X) || (tapped.Y < startPoint.Y))
                return;

            for (byte i = 1; i <= 3; i++)
                for (byte j = 1; j <= 3; j++)
                {
                    Vector2 location = startPoint + new Vector2(i * ratio * 166, j * ratio * 166);
                    if ((tapped.X < location.X) && (tapped.Y < location.Y))
                    {
                        if (Game[i - 1, j - 1] == null)
                        {
                            Game[i - 1, j - 1] = move++;
                            AI.goTo((byte)(i - 1), (byte)(j - 1));
                            State = TicTacToeMinimax.Evaluate(Game);

                            // If game continues, get oponent move
                            if (State == Result.Game)
                            {
                                (byte x, byte y) o = AI.getMove();
                                Game[o.x, o.y] = move++;
                                State = TicTacToeMinimax.Evaluate(Game);
                            }
                        }
                        return;
                    }
                }
        }

        private float GetRatio(Rect canvasBounds, Rect imageBounds) => ((float)0.8 * (float)canvasBounds.Height / (float)imageBounds.Height);

        private Vector2 GetStartPoint(Rect canvasBounds, Rect imageBounds, float ratio)
        {
            float x = (float)canvasBounds.Width / 2f - ((float)imageBounds.Width * ratio) / 2f + ((float)canvasBounds.Width - (float)imageBounds.Width * ratio) / 4f;
            float y = (float)canvasBounds.Height / 2f - ((float)imageBounds.Height * ratio) / 2f;

            return new Vector2(x, y);
        }

        public void HandleKey(KeyRoutedEventArgs e) { }

    }


    class TicTacToeMinimax
    {
        // Current state
        TicTacToeNode Root = null;

        public TicTacToeMinimax()
        {
            // Init tree
            Root = new TicTacToeNode();
        }

        public static Result Evaluate(byte?[,] initialState)
        {
            bool?[,] state = new bool?[3, 3];

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    if (initialState[i, j] != null)
                        state[i, j] = initialState[i, j] % 2 == 0;
                }

            return Evaluate(state);
        }

        /// <summary>
        /// Evaluate current game state
        /// </summary>
        /// <param name="initialState"></param>
        /// <returns>Gamestate, X wins, O wins, No-Winner, Game hasn't finnished</returns>
        public static Result Evaluate(bool?[,] initialState)
        {
            bool nullFound = false;

            for (int i = 0; i < 3; i++)
            {
                // Check lines
                if ((initialState[i, 0] == initialState[i, 1]) && (initialState[i, 1] == initialState[i, 2]))
                {
                    if (initialState[i, 0] == true)
                        return Result.XWin;
                    if (initialState[i, 0] == false)
                        return Result.OWin;
                    nullFound = true;
                }

                // Check rows
                if ((initialState[0, i] == initialState[1, i]) && (initialState[1, i] == initialState[2, i]))
                {
                    if (initialState[0, i] == true)
                        return Result.XWin;
                    if (initialState[0, i] == false)
                        return Result.OWin;
                    nullFound = true;
                }

                if (!nullFound)
                    for (int j = 0; j < 3; j++)
                        if (initialState[i, j] == null)
                            nullFound = true;
            }

            // Check diagonals
            if ((initialState[0, 0] == initialState[1, 1]) && (initialState[1, 1] == initialState[2, 2]) && (initialState[1, 1] != null))
            {
                if (initialState[1, 1] == true)
                    return Result.XWin;
                return Result.OWin;
            }

            if ((initialState[0, 2] == initialState[1, 1]) && (initialState[1, 1] == initialState[2, 0]) && (initialState[1, 1] != null))
            {
                if (initialState[1, 1] == true)
                    return Result.XWin;
                return Result.OWin;
            }

            if (nullFound)
                return Result.Game;
            return Result.NoWin;
        }

        /// <summary>
        /// Players move to [x,y]
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void goTo(byte x, byte y)
        {
            Root = Root.getChild(x, y);
        }

        /// <summary>
        /// Get Best UI move
        /// </summary>
        /// <returns>UI moves (x, y)</returns>
        public (byte, byte) getMove()
        {
            Root = Root.getMax();
            return (Root.X, Root.Y);
        }

    }

    /// <summary>
    /// Tic-Tac-Tome Min Max node
    /// </summary>
    class TicTacToeNode
    {
        private TicTacToeNode[] Children = null;
        private int Val;
        private byte MoveX, MoveY;

        public byte X { get => MoveX; }
        public byte Y { get => MoveY; }

        /// <summary>
        /// Initialize Minimax Tree
        /// </summary>
        public TicTacToeNode() : this(new bool?[3, 3] { { null, null, null }, { null, null, null }, { null, null, null } }, 0, byte.MaxValue, byte.MaxValue) { }

        private TicTacToeNode(bool?[,] state, byte depth, byte moveX, byte moveY)
        {
            MoveX = moveX;
            MoveY = moveY;

            // Evaluate State
            Result res = TicTacToeMinimax.Evaluate(state);

            if (res == Result.Game)
            {
                if (depth % 2 == 0)
                    Val = 2;
                else
                    Val = -2;

                byte childNum = 0;

                Children = new TicTacToeNode[9 - depth];
                for (byte i = 0; i < 3; i++)
                    for (byte j = 0; j < 3; j++)
                        if (state[i, j] == null)
                        {
                            //bool?[,] child = state.Clone() as bool?[,];

                            state[i, j] = depth % 2 == 0;
                            Children[childNum] = new TicTacToeNode(state, (byte)(depth + 1), i, j);
                            state[i, j] = null;

                            if ((depth % 2 == 0) && (Children[childNum].Val < Val))
                                Val = Children[childNum].Val;
                            else if ((depth % 2 == 1) && (Children[childNum].Val > Val))
                                Val = Children[childNum].Val;
                            childNum++;

                            // If AI move && max found - do not contiue evaluation
                            // (all min nodes are needed - can't predict player's move)
                            if ((depth % 2 == 1) && (Val == 1))
                                break;
                        }
            }
            else if (res == Result.XWin)
            {
                Val = -1;
            }
            else if (res == Result.OWin)
            {
                Val = 1;
            }
            else if (res == Result.NoWin)
            {
                Val = 0;
            };

        }

        public TicTacToeNode getChild(byte x, byte y)
        {
            // Get child at with move to X Y
            var node = from child in Children
                       where (child.MoveX == x) && (child.MoveY == y)
                       select child;

            return node.First();
        }

        public TicTacToeNode getMax()
        {
            // Get best possible move
            var max = from child in Children
                      where child.Val == Val
                      select child;

            return max.First();
        }

    }

}