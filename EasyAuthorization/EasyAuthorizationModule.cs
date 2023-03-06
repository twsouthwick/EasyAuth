using EasyAuthorization.Configuration;
using System;
using System.Configuration;
using System.Net;
using System.Security.Claims;
using System.Web;

namespace EasyAuthorization;

internal sealed class EasyAuthorizationModule : IHttpModule
{
    public void Dispose()
    {
    }

    public void Init(HttpApplication context)
    {
        if (ConfigurationManager.GetSection("easyAuth") is EasyAuthorizationConfigurationSection { IsEnabled: true, Allow.Roles: { } roleString, Type: { } roleType })
        {
            var roles = roleString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            // EasyAuth uses this as their claim type, which does not match the ClaimsIdentity.RoleClaimType for AAD 
            const string EasyAuthRolesClaimType = "roles";
            const string EasyAuthGroupClaimType = "groups";
            var roleClaimType = roleType == RoleType.Groups ? EasyAuthGroupClaimType : EasyAuthRolesClaimType;

            context.PostAuthenticateRequest += (s, e) => TransformEasyAuthClaim(((HttpApplication)s).Context, roleClaimType);
            context.AuthorizeRequest += (s, e) => AuthorizeRequest(((HttpApplication)s).Context, roles);
        }
    }

    private void AuthorizeRequest(HttpContext context, string[] roles)
    {
        try
        {
            foreach (var allowedRole in roles)
            {
                if (context.User.IsInRole(allowedRole))
                {
                    return;
                }
            }
        }
        catch
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.End();
        }

        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.End();
    }

    // This will replace the HttpContext.User with an updated one with the EasyAuth roles claim type if an identity exists that came from EasyAuth
    private void TransformEasyAuthClaim(HttpContext context, string claimTypeToTransform)
    {
        const string AzureAdIdentityType = "aad";

        if (context.User is ClaimsPrincipal principal)
        {
            var newPrincipal = new ClaimsPrincipal();
            var hasEasyAuth = false;

            foreach (var identity in principal.Identities)
            {
                if (string.Equals(AzureAdIdentityType, identity.AuthenticationType, StringComparison.Ordinal))
                {
                    var newIdentity = new ClaimsIdentity(identity, null, identity.AuthenticationType, identity.NameClaimType, claimTypeToTransform);
                    newPrincipal.AddIdentity(newIdentity);
                    hasEasyAuth = true;
                }
                else
                {
                    newPrincipal.AddIdentity(identity);
                }
            };

            if (hasEasyAuth)
            {
                context.User = newPrincipal;
            }
        }
    }
}

