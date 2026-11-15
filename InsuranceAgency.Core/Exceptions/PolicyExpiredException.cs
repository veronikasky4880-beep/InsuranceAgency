namespace InsuranceAgency.Core.Exceptions;

public class PolicyExpiredException : Exception
{
    public PolicyExpiredException(string message) : base(message) { }
}
