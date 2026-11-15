using InsuranceAgency.Core.Interfaces;

namespace InsuranceAgency.Core.Services;

public class InsuranceCalculator : IInsuranceCalculator
{
    public decimal CalculatePolicyPrice(decimal insuredAmount, decimal riskCoefficient)
    {
        if (insuredAmount <= 0)
            throw new ArgumentException("Страхова сума має бути більшою за 0.");

        if (riskCoefficient <= 0 || riskCoefficient > 1)
            throw new ArgumentException("Коефіцієнт ризику має бути в межах від 0 до 1.");

        return Math.Round(insuredAmount * riskCoefficient, 2);
    }
}
