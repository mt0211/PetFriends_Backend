namespace UserAuthenticationAPI.Helpers;
public class ConfigurationHelper
    {
        public static AppConfig LoadConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(typeof(ConfigurationHelper).Assembly.Location)) // Set base path to the directory of the assembly
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = configurationBuilder.Build();

            var appConfig = new AppConfig();
            configuration.Bind(appConfig);

            return appConfig;
        }
    }