namespace InsuranceAgency.Core.Models;

public class Client : Person
{
    public string Address { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
}
