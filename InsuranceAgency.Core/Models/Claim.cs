namespace InsuranceAgency.Core.Models;

public class Claim
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ClaimNumber { get; set; } = string.Empty;
    public Guid PolicyId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public decimal RequestedAmount { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
}
