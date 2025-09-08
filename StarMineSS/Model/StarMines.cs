namespace StarMineSS.Model
{
    public class GameState
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int Mines { get; set; }
        public bool GameOver { get; set; }
        public bool Won { get; set; }

        public Cell[,] Cells { get; set; } = default!;
        private int _safeRevealed;

        public static GameState Create(int rows = 5, int cols = 5, int mines = 3)
        {
            if (rows <= 0 || cols <= 0) throw new ArgumentException("Invalid board size");
            var maxMines = Math.Max(1, rows * cols - 1);
            mines = Math.Clamp(mines, 1, maxMines);

            var g = new GameState
            {
                Rows = rows,
                Cols = cols,
                Mines = mines,
                Cells = new Cell[rows, cols]
            };

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    g.Cells[r, c] = new Cell();

            // ბომბების განლაგება
            var total = rows * cols;
            var rnd = Random.Shared;
            foreach (var pick in Enumerable.Range(0, total)
                                           .OrderBy(_ => rnd.Next())
                                           .Take(mines))
            {
                int rr = pick / cols, cc = pick % cols;
                g.Cells[rr, cc].HasBomb = true;
            }

            return g;
        }

        public void Reveal(int r, int c)
        {
            if (GameOver) return;
            if (r < 0 || r >= Rows || c < 0 || c >= Cols) return;

            var cell = Cells[r, c];
            if (cell.Revealed) return;

            cell.Revealed = true;

            if (cell.HasBomb)
            {
                GameOver = true;
                Won = false;
                // ყველა ბომბი გამოჩნდეს
                for (int i = 0; i < Rows; i++)
                    for (int j = 0; j < Cols; j++)
                        if (Cells[i, j].HasBomb) Cells[i, j].Revealed = true;
                return;
            }

            _safeRevealed++;
            int safeTotal = Rows * Cols - Mines;
            if (_safeRevealed == safeTotal)
            {
                GameOver = true;
                Won = true;
                // სურვილის მიხედვით ყველაც ვაჩვენოთ (უსაფრთხოებიც)
                for (int i = 0; i < Rows; i++)
                    for (int j = 0; j < Cols; j++)
                        Cells[i, j].Revealed = true;
            }
        }

        public ClientGame ToClient()
        {
            var board = new string[Rows][];
            for (int i = 0; i < Rows; i++)
            {
                board[i] = new string[Cols];
                for (int j = 0; j < Cols; j++)
                {
                    var cell = Cells[i, j];
                    if (!cell.Revealed) board[i][j] = "hidden";
                    else board[i][j] = cell.HasBomb ? "bomb" : "star";
                }
            }
            return new ClientGame
            {
                Id = Id,
                Rows = Rows,
                Cols = Cols,
                Mines = Mines,
                GameOver = GameOver,
                Won = Won,
                Board = board
            };
        }
    }

    public class Cell
    {
        public bool HasBomb { get; set; }
        public bool Revealed { get; set; }
    }

    public class ClientGame
    {
        public Guid Id { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int Mines { get; set; }
        public bool GameOver { get; set; }
        public bool Won { get; set; }
        public string[][] Board { get; set; } = default!;
    }
}