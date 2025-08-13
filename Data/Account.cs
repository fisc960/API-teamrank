namespace GemachApp.Data
{
    public class Account
    {
        public int AccountId { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public List<Transaction> Transactions { get; set; }
        public DateTime UpdateBalDate { get; set; }
        public decimal? TotalAmount { get; set; }

    }
}
