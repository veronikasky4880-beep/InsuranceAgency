namespace InsuranceAgency.Core.Exceptions;

public class ClaimAmountExceededException : Exception
{
    public ClaimAmountExceededException(string message) : base(message) { }
}
