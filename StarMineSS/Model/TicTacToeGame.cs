namespace StarMineSS.Model
{
    public class TicTacToeGame
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string[] Board { get; set; } = new string[9] { "", "", "", "", "", "", "", "", "" };
        public string Turn { get; set; } = "X";
        public string Winner { get; set; } = "";
        public bool IsDraw { get; set; } = false;
    }
}