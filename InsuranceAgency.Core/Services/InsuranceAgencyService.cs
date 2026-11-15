using InsuranceAgency.Core.Exceptions;
using InsuranceAgency.Core.Interfaces;
using InsuranceAgency.Core.Models;
using InsuranceAgency.Core.Repositories;

namespace InsuranceAgency.Core.Services;

public class InsuranceAgencyService
{
    public AppData Data { get; }

    private readonly IRepository<Client> _clients;
    private readonly IRepository<Agent> _agents;
    private readonly IRepository<InsurancePolicy> _policies;
    private readonly IRepository<Claim> _claims;
    private readonly IInsuranceCalculator _calculator;

    public InsuranceAgencyService(AppData data)
    {
        Data = data;
        Data.Clients ??= new();
        Data.Agents ??= new();
        Data.Policies ??= new();
        Data.Claims ??= new();

        _clients = new InMemoryRepository<Client>(Data.Clients);
        _agents = new InMemoryRepository<Agent>(Data.Agents);
        _policies = new InMemoryRepository<InsurancePolicy>(Data.Policies);
        _claims = new InMemoryRepository<Claim>(Data.Claims);
        _calculator = new InsuranceCalculator();
    }

    public List<Client> GetClients() => _clients.GetAll().OrderBy(c => c.LastName).ToList();
    public List<Agent> GetAgents() => _agents.GetAll().OrderBy(a => a.LastName).ToList();
    public List<InsurancePolicy> GetPolicies() => _policies.GetAll().OrderBy(p => p.EndDate).ToList();
    public List<Claim> GetClaims() => _claims.GetAll().OrderBy(c => c.IncidentDate).ToList();

    public void AddClient(Client client)
    {
        ArgumentNullException.ThrowIfNull(client);
        ValidatePerson(client);

        if (string.IsNullOrWhiteSpace(client.Address))
            throw new ArgumentException("Адреса клієнта не може бути порожньою.");
        if (client.BirthDate.Date >= DateTime.Today)
            throw new ArgumentException("Дата народження має бути в минулому.");

        _clients.Add(client);
    }

    public void AddAgent(Agent agent)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ValidatePerson(agent);

        if (string.IsNullOrWhiteSpace(agent.EmployeeNumber))
            throw new ArgumentException("Номер співробітника не може бути порожнім.");
        if (_agents.GetAll().Any(a => a.EmployeeNumber.Equals(agent.EmployeeNumber, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Агент з таким номером співробітника вже існує.");
        if (agent.CommissionPercent < 0 || agent.CommissionPercent > 100)
            throw new ArgumentException("Комісія агента має бути від 0 до 100%.");

        _agents.Add(agent);
    }

    public InsurancePolicy CreatePolicy(Guid clientId, Guid agentId, InsuranceType type,
        decimal insuredAmount, decimal riskCoefficient, DateTime startDate, DateTime endDate)
    {
        if (!_clients.GetAll().Any(c => c.Id == clientId))
            throw new ArgumentException("Клієнта не знайдено.");
        if (!_agents.GetAll().Any(a => a.Id == agentId))
            throw new ArgumentException("Агента не знайдено.");
        if (!Enum.IsDefined(type))
            throw new ArgumentException("Невідомий тип страхування.");
        if (startDate.Date < DateTime.Today)
            throw new ArgumentException("Дата початку поліса не може бути в минулому.");
        if (endDate.Date <= startDate.Date)
            throw new ArgumentException("Дата завершення має бути пізніше дати початку.");

        decimal price = _calculator.CalculatePolicyPrice(insuredAmount, riskCoefficient);
        var policy = new InsurancePolicy
        {
            PolicyNumber = CreateNumber("POL"),
            ClientId = clientId,
            AgentId = agentId,
            Type = type,
            InsuredAmount = insuredAmount,
            RiskCoefficient = riskCoefficient,
            Price = price,
            StartDate = startDate.Date,
            EndDate = endDate.Date
        };

        _policies.Add(policy);
        return policy;
    }

    public Claim RegisterClaim(Guid policyId, string description, DateTime incidentDate, decimal requestedAmount)
    {
        InsurancePolicy? policy = _policies.GetAll().FirstOrDefault(p => p.Id == policyId);
        if (policy == null)
            throw new ArgumentException("Поліс не знайдено.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Опис страхового випадку не може бути порожнім.");
        if (incidentDate.Date > DateTime.Today)
            throw new ArgumentException("Дата страхового випадку не може бути в майбутньому.");
        if (incidentDate.Date < policy.StartDate.Date)
            throw new ArgumentException("Страховий випадок стався до початку дії поліса.");
        if (incidentDate.Date > policy.EndDate.Date)
            throw new PolicyExpiredException("На дату страхового випадку строк дії поліса вже завершився.");
        if (requestedAmount <= 0)
            throw new ArgumentException("Сума виплати має бути більшою за 0.");

        decimal reservedAmount = _claims.GetAll()
            .Where(c => c.PolicyId == policyId && (c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Paid))
            .Sum(c => c.RequestedAmount);

        if (requestedAmount + reservedAmount > policy.InsuredAmount)
            throw new ClaimAmountExceededException("Сума заяви разом із вже схваленими виплатами перевищує страхову суму.");

        var claim = new Claim
        {
            ClaimNumber = CreateNumber("CLM"),
            PolicyId = policyId,
            Description = description.Trim(),
            IncidentDate = incidentDate.Date,
            RequestedAmount = requestedAmount,
            Status = ClaimStatus.Submitted
        };

        _claims.Add(claim);
        return claim;
    }

    public void SetClaimStatus(Guid claimId, ClaimStatus newStatus)
    {
        Claim? claim = _claims.GetAll().FirstOrDefault(c => c.Id == claimId);
        if (claim == null)
            throw new ArgumentException("Страховий випадок не знайдено.");
        if (!Enum.IsDefined(newStatus))
            throw new ArgumentException("Невідомий статус страхового випадку.");
        if (claim.Status == ClaimStatus.Paid || claim.Status == ClaimStatus.Rejected)
            throw new InvalidOperationException("Неможливо змінити завершений страховий випадок.");
        if (claim.Status == ClaimStatus.Submitted && newStatus == ClaimStatus.Paid)
            throw new InvalidOperationException("Спочатку заяву потрібно схвалити.");
        if (newStatus == ClaimStatus.Submitted)
            throw new InvalidOperationException("Повернення до статусу «Подано» не допускається.");

        claim.Status = newStatus;
    }

    private static void ValidatePerson(Person person)
    {
        if (string.IsNullOrWhiteSpace(person.FirstName) || string.IsNullOrWhiteSpace(person.LastName))
            throw new ArgumentException("Ім'я та прізвище є обов'язковими.");
        if (string.IsNullOrWhiteSpace(person.Phone))
            throw new ArgumentException("Телефон не може бути порожнім.");
        if (string.IsNullOrWhiteSpace(person.Email) || !person.Email.Contains('@') || !person.Email.Contains('.'))
            throw new ArgumentException("Введіть коректну електронну адресу.");
    }

    private static string CreateNumber(string prefix)
    {
        return $"{prefix}-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }
}
