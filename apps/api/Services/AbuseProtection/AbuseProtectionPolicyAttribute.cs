namespace ResumeReview.Api.Services.AbuseProtection;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AbuseProtectionPolicyAttribute : Attribute
{
    public AbuseProtectionPolicyAttribute(string policyName)
    {
        PolicyName = policyName;
    }

    public string PolicyName { get; }
}
