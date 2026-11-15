namespace InsuranceAgency.Core.Interfaces;

public interface IInsuranceCalculator
{
    decimal CalculatePolicyPrice(decimal insuredAmount, decimal riskCoefficient);
}
