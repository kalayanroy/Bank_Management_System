using System.ComponentModel.DataAnnotations;

namespace BankMSWeb.Models
{
    public class TransactionLimit
    {
        [Key]
        public int LimitId { get; set; }
        public string AccountType { get; set; }
        public decimal TransactionDailyLimit { get; set; }
        public decimal TransactionFee { get; set; }
    }
}
