using System.Configuration;

namespace EasyAuthorization.Configuration;

internal sealed class AuthorizationDefinition : ConfigurationElement
{
    [ConfigurationProperty("roles")]
    public string Roles => (string)base["roles"];
}