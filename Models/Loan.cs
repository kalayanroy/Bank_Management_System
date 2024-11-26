using System.ComponentModel.DataAnnotations;

namespace BankMSWeb.Models
{
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }
        public int? AccountId { get; set; }
        public string? LoanType { get; set; }
        public decimal? LoanAmount { get; set; }
        public decimal? InterestRate { get; set; }
        public string? LoanStatus { get; set; }
        public string? Term { get; set; }
        public DateTime? AppliedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Remarks{ get; set; }
        public string? ApprovedBy { get; set; }
        public Account? Account { get; set; }
        public string? AccountNo => Account?.AccountNo ?? "N/A";
    }
    public class Repayment
    {
        [Key]
        public int RepaymentID { get; set; }
        public int LoanID { get; set; }
        public DateTime DueDate { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Penalty { get; set; }
        public bool IsPaid { get; set; }

        // Navigation property
        public Loan? Loan { get; set; }
    }
    public class RepaymentViewModel
    {
        
        public int RepaymentID { get; set; }
        public int LoanID { get; set; }
        public DateTime DueDate { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Penalty { get; set; }
        public bool IsPaid { get; set; }
        public int? AccountId { get; set; }
        public string? LoanType { get; set; }
        public decimal? LoanAmount { get; set; }
        public decimal? InterestRate { get; set; }
        public string? LoanStatus { get; set; }
        public string? Term { get; set; }
        public string? InstallmentNo { get; set; }
        public decimal? MonthlyInstallmentAmount { get; set; }
    }
    public class LoanVw
    {
        [Key]
        public int LoanId { get; set; }
        public string? LoanType { get; set; }
        public decimal? LoanAmount { get; set; }
        public decimal? InterestRate { get; set; }
        public string? LoanStatus { get; set; }
        public string? Term { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Remarks { get; set; }
        public string? ApprovedBy { get; set; }
    }
}
