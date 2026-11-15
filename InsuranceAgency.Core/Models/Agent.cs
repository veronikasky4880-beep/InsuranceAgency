namespace InsuranceAgency.Core.Models;

public class Agent : Person
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public decimal CommissionPercent { get; set; }
}
