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
        var roles = Array.Empty<string>();

        if (ConfigurationManager.GetSection("EasyAuthorization") is EasyAuthorizationConfigurationSection section)
        {
            if (!string.IsNullOrEmpty(section.RoleClaimType))
            {
                context.PostAuthenticateRequest += (s, e) => RewriteRoleClaimType(((HttpApplication)s).Context, section.RoleClaimType);
            }

            roles = section.Allow.Roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        context.AuthorizeRequest += (s, e) => AuthorizeRequest(((HttpApplication)s).Context, roles);
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

    private void RewriteRoleClaimType(HttpContext context, string roleClaimType)
    {
        if (context.User is ClaimsPrincipal principal)
        {
            foreach (var identity in principal.Identities)
            {
                foreach (var claim in identity.Claims)
                {
                    if (string.Equals(claim.Type, roleClaimType, StringComparison.OrdinalIgnoreCase))
                    {
                        identity.AddClaim(new Claim(identity.RoleClaimType, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer, identity));
                    }
                }
            }
        }
    }
}
