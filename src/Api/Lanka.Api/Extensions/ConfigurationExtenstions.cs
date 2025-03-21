namespace Lanka.Api.Extensions;

internal static class ConfigurationExtensions
{
    public static void AddModuleConfiguration(
        this IConfigurationBuilder configurationBuilder,
        string[] modules
    )
    {
        foreach (string module in modules)
        {
            configurationBuilder.AddJsonFile($"modules.{module}.json", optional: false, reloadOnChange: true);
            configurationBuilder.AddJsonFile($"modules.{module}.Development.json", optional: true, reloadOnChange: true);
        }
    }
}
