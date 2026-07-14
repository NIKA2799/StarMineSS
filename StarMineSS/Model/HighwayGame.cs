namespace StarMineSS.Model
{
    public enum HighwayStatus
    {
        Active,
        Crashed,
        CashedOut
    }

    public class HighwayGame
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Bet { get; set; }
        public List<double> Ladder { get; set; } = new();
        public int CurrentStep { get; set; } = 0;

        // 1-based step at which the run crashes. Never sent to the client.
        public int CrashStep { get; set; }

        public HighwayStatus Status { get; set; } = HighwayStatus.Active;
        public decimal? Payout { get; set; }
    }
}