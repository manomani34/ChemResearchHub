using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ChemResearchHub.Web.Authorization.Permissions;

public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(
        IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider =
            new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(
                PermissionConstants.PolicyPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        var permission = policyName[
            PermissionConstants.PolicyPrefix.Length..];

        if (string.IsNullOrWhiteSpace(permission) ||
            !PermissionCatalog.Exists(permission))
        {
            return Task.FromResult<AuthorizationPolicy?>(null);
        }

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permission))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallbackPolicyProvider.GetFallbackPolicyAsync();
}
