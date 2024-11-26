using System.ComponentModel.DataAnnotations;

namespace BankMSWeb.Models
{
    public class SavingPlan
    {
        [Key]
        public int PlanID { get; set; }
        public int AccountID { get; set; }
        public string? PlanType { get; set; } 
        public decimal PrincipalAmount { get; set; }
        public decimal? DurationMonths { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public decimal MaturityAmount { get; set; }

        // Navigation property
        public Account? Account { get; set; }
        public string? AccountNo => Account?.AccountNo ?? "N/A";
    }
}
