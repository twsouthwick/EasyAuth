using System.Configuration;

namespace EasyAuthorization.Configuration;

internal sealed class EasyAuthorizationConfigurationSection : ConfigurationSection
{
    [ConfigurationProperty("allow")]
    public AuthorizationDefinition Allow => (AuthorizationDefinition)base["allow"];

    [ConfigurationProperty("type")]
    public RoleType Type => (RoleType)base["type"];

    [ConfigurationProperty("isEnabled", DefaultValue = true)]
    public bool IsEnabled => (bool)base["isEnabled"];
}
