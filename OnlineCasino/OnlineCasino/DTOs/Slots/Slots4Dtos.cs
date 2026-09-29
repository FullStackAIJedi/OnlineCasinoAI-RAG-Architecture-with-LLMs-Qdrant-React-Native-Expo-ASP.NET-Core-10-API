namespace OnlineCasino.DTOs.Slots
{
    public class Spin4Request
    {
        /// <summary>Залогът. Трябва да е в min/max границите на играта.</summary>
        public decimal BetAmount { get; set; }

        /// <summary>Уникален идентификатор от клиента – за идемпотентност.</summary>
        public string ClientRoundId { get; set; } = null!;
    }

    /// <summary>Резултат от spin на 4-барабанната игра.</summary>
    public class Spin4Response
    {
        /// <summary>Публичен идентификатор на рунда.</summary>
        public string RoundId { get; set; } = null!;

        /// <summary>Четирите символа на барабаните.</summary>
        public List<string> Reels { get; set; } = new();

        /// <summary>Залогът за рунда.</summary>
        public decimal BetAmount { get; set; }

        /// <summary>Печалбата (0 при загуба).</summary>
        public decimal WinAmount { get; set; }

        /// <summary>Актуален баланс след рунда.</summary>
        public decimal Balance { get; set; }

        /// <summary>Резултат: <c>"Win"</c> или <c>"Lose"</c>.</summary>
        public string Result { get; set; } = null!;

        /// <summary>Колко еднакви символа – 4, 3 или 0.</summary>
        public int MatchCount { get; set; }
    }
}
