using System.Configuration;

namespace EasyAuthorization.Configuration;

internal sealed class EasyAuthorizationConfigurationSection : ConfigurationSection
{
    [ConfigurationProperty("allow")]
    public AuthorizationDefinition Allow => (AuthorizationDefinition)base["allow"];
}