namespace InsuranceAgency.Core.Models;

public class InsurancePolicy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PolicyNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid AgentId { get; set; }
    public InsuranceType Type { get; set; }
    public decimal InsuredAmount { get; set; }
    public decimal RiskCoefficient { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
