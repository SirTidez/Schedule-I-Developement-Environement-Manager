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
                
                logger.LogInformation("Config loaded. ManagedEnvironmentPath: '{Path}', GameInstallPath: '{GamePath}', SelectedBranches: {Count}",
                    config?.ManagedEnvironmentPath ?? "null", 
                    config?.GameInstallPath ?? "null", 
                    config?.SelectedBranches?.Count ?? 0);
                
                if (config != null && IsManagedEnvironmentConfigured(config))
                {
                    // Managed environment exists, show the managed environment loaded form as main screen
                    logger.LogInformation("Managed environment configuration found, showing ManagedEnvironmentLoadedForm as main screen");
                    
                    try
                    {
                        var managedForm = new ManagedEnvironmentLoadedForm(config);
                        logger.LogInformation("ManagedEnvironmentLoadedForm created successfully");
                        return managedForm;
                    }
                    catch (Exception formEx)
                    {
                        logger.LogError(formEx, "Error creating ManagedEnvironmentLoadedForm, falling back to MainForm");
                        return new MainForm();
                    }
                }
                else
                {
                    // No managed environment, show the main form for setup
                    logger.LogInformation("No managed environment configuration found, showing MainForm for setup");
                    if (config != null)
                    {
                        logger.LogInformation("Configuration validation failed. ManagedEnvironmentConfigured: {Configured}", 
                            IsManagedEnvironmentConfigured(config));
                    }
                    return new MainForm();
                }
            }
            catch (Exception ex)
            {
                // If there's any error checking configuration, fall back to main form
                // Note: We can't easily log here since logger might be out of scope
                return new MainForm();
            }
        }
        
        private static bool IsManagedEnvironmentConfigured(DevEnvironmentConfig config)
        {
            // Check if we have the essential paths and at least one branch selected
            var hasManagedPath = !string.IsNullOrEmpty(config.ManagedEnvironmentPath);
            var hasGamePath = !string.IsNullOrEmpty(config.GameInstallPath);
            var hasBranches = config.SelectedBranches.Count > 0;
            
            // We can't log here easily without setting up logging again, but we can check each condition
            return hasManagedPath && hasGamePath && hasBranches;
        }
    }
}
