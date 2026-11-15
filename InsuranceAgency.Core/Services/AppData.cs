using InsuranceAgency.Core.Models;

namespace InsuranceAgency.Core.Services;

public class AppData
{
    public List<Client> Clients { get; set; } = new();
    public List<Agent> Agents { get; set; } = new();
    public List<InsurancePolicy> Policies { get; set; } = new();
    public List<Claim> Claims { get; set; } = new();
}
