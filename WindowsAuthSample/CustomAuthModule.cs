using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using WindowsAuthSample.Configuration;

namespace WindowsAuthSample
{
    public class CustomAuthModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            var roles = Array.Empty<string>();

            if (ConfigurationManager.GetSection("MyAuthorization") is MyAuthorizationSection section)
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
            foreach (var allowedRole in roles)
            {
                if (context.User.IsInRole(allowedRole))
                {
                    return;
                }
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
}

namespace WindowsAuthSample.Configuration
{
    public class MyAuthorizationSection : ConfigurationSection
    {
        [ConfigurationProperty("allow")]
        public AuthorizationDefinition Allow => (AuthorizationDefinition)base["allow"];

        [ConfigurationProperty("roleClaimType")]
        public string RoleClaimType => (string)base["roleClaimType"];
    }

    public class AuthorizationDefinition : ConfigurationElement
    {
        [ConfigurationProperty("roles")]
        public string Roles => (string)base["roles"];
    }
}