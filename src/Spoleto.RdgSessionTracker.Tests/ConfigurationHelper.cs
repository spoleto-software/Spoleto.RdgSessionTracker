using Microsoft.Extensions.Configuration;
using Spoleto.RdgSessionTracker.Tests.Models;

namespace Spoleto.RdgSessionTracker.Tests
{
    internal class ConfigurationHelper
    {
        private static readonly IConfigurationRoot _config;

        static ConfigurationHelper()
        {
            _config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: true)
               .AddUserSecrets("e259b509-5896-45a1-92ae-46cdc6f17854")
               .Build();
        }

        public static IConfigurationRoot Configuration => _config;

        public static TestOption GetTestOption()
        {
            var options = _config.GetSection(nameof(TestOption)).Get<TestOption>()!;

            return options;
        }
    }
}
