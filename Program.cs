using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScheduleIDevelopementEnvironementManager.Models;
using ScheduleIDevelopementEnvironementManager.Services;

namespace ScheduleIDevelopementEnvironementManager
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // Check for managed environment configuration first
            var formToShow = DetermineMainForm();
            Application.Run(formToShow);
        }
        
        private static Form DetermineMainForm()
        {
            try
            {
                // Set up dependency injection for configuration checking
                var services = new ServiceCollection();
                
                // Use file logging instead of console logging
                var fileLoggingFactory = new FileLoggingServiceFactory();
                services.AddLogging(builder => builder.AddProvider(new FileLoggingProvider(fileLoggingFactory)));
                
                services.AddSingleton<ConfigurationService>();
                
                var serviceProvider = services.BuildServiceProvider();
                var configService = serviceProvider.GetRequiredService<ConfigurationService>();
                var logger = serviceProvider.GetRequiredService<ILogger<object>>();
                
                logger.LogInformation("Checking for managed environment configuration on startup...");
                
                // Try to load configuration
                var config = configService.LoadConfigurationAsync().GetAwaiter().GetResult();
                
                if (config != null && IsManagedEnvironmentConfigured(config))
                {
                    // Managed environment exists, show the managed environment loaded form as main screen
                    logger.LogInformation("Managed environment configuration found, showing ManagedEnvironmentLoadedForm as main screen");
                    return new ManagedEnvironmentLoadedForm(config);
                }
                else
                {
                    // No managed environment, show the main form for setup
                    logger.LogInformation("No managed environment configuration found, showing MainForm for setup");
                    return new MainForm();
                }
            }
            catch (Exception)
            {
                // If there's any error checking configuration, fall back to main form
                // Note: We can't log here since the logging service might not be available
                return new MainForm();
            }
        }
        
        private static bool IsManagedEnvironmentConfigured(DevEnvironmentConfig config)
        {
            // Check if we have the essential paths and at least one branch selected
            return !string.IsNullOrEmpty(config.ManagedEnvironmentPath) && 
                   !string.IsNullOrEmpty(config.GameInstallPath) &&
                   config.SelectedBranches.Count > 0;
        }
    }
}
