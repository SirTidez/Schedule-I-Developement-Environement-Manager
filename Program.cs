using System;
using System.IO;
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
                
                // Use enhanced validation with auto-healing
                bool configIsValid = false;
                DevEnvironmentConfig? validatedConfig = config;
                
                if (config != null)
                {
                    // First try the simple enhanced validation
                    if (IsManagedEnvironmentConfigured(config, logger))
                    {
                        // If basic validation passes, perform auto-healing validation
                        var validationResult = configService.ValidateAndHealConfiguration(config).GetAwaiter().GetResult();
                        configIsValid = validationResult.isValid;
                        validatedConfig = validationResult.config;
                        
                        if (configIsValid)
                        {
                            logger.LogInformation("Configuration validation and auto-healing completed successfully");
                        }
                        else
                        {
                            logger.LogWarning("Configuration validation failed even after auto-healing attempts");
                        }
                    }
                    else
                    {
                        logger.LogInformation("Basic enhanced validation failed, skipping auto-healing");
                    }
                }
                
                if (configIsValid && validatedConfig != null)
                {
                    // Managed environment exists, show the managed environment loaded form as main screen
                    logger.LogInformation("Managed environment configuration found, showing ManagedEnvironmentLoadedForm as main screen");
                    
                    try
                    {
                        var managedForm = new ManagedEnvironmentLoadedForm(validatedConfig);
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
                        logger.LogInformation("Configuration validation failed - enhanced validation did not pass");
                    }
                    return new MainForm();
                }
            }
            catch (Exception ex)
            {
                // If there's any error checking configuration, fall back to main form
                // Log error to console since logger might be out of scope
                Console.WriteLine($"Configuration check failed: {ex.Message}");
                return new MainForm();
            }
        }
        
        private static bool IsManagedEnvironmentConfigured(DevEnvironmentConfig config, ILogger<object> logger)
        {
            logger.LogInformation("Starting enhanced managed environment validation...");
            
            // Check if we have the essential path strings
            var hasManagedPath = !string.IsNullOrEmpty(config.ManagedEnvironmentPath);
            var hasGamePath = !string.IsNullOrEmpty(config.GameInstallPath);
            var hasBranches = config.SelectedBranches.Count > 0;
            
            logger.LogInformation("Basic path validation - ManagedPath: {HasManagedPath}, GamePath: {HasGamePath}, Branches: {BranchCount}", 
                hasManagedPath, hasGamePath, config.SelectedBranches.Count);
            
            if (!hasManagedPath || !hasGamePath || !hasBranches)
            {
                logger.LogInformation("Basic validation failed - missing essential configuration");
                return false;
            }
            
            // Enhanced validation: Check if paths actually exist on filesystem
            bool managedPathExists = Directory.Exists(config.ManagedEnvironmentPath);
            bool gamePathExists = Directory.Exists(config.GameInstallPath);
            
            logger.LogInformation("Filesystem validation - ManagedPath exists: {ManagedExists} ({ManagedPath}), GamePath exists: {GameExists} ({GamePath})", 
                managedPathExists, config.ManagedEnvironmentPath, gamePathExists, config.GameInstallPath);
            
            if (!managedPathExists)
            {
                logger.LogWarning("Managed environment path does not exist: {Path}", config.ManagedEnvironmentPath);
                return false;
            }
            
            if (!gamePathExists)
            {
                logger.LogWarning("Game install path does not exist: {Path}", config.GameInstallPath);
                return false;
            }
            
            // Enhanced validation: Check if at least one selected branch is actually installed
            bool hasValidBranch = false;
            foreach (var branchName in config.SelectedBranches)
            {
                var branchPath = Path.Combine(config.ManagedEnvironmentPath, branchName);
                var executablePath = Path.Combine(branchPath, "Schedule I.exe");
                
                bool branchExists = Directory.Exists(branchPath);
                bool executableExists = File.Exists(executablePath);
                
                logger.LogInformation("Branch validation - {Branch}: Directory exists: {DirExists}, Executable exists: {ExeExists}", 
                    branchName, branchExists, executableExists);
                
                if (branchExists && executableExists)
                {
                    hasValidBranch = true;
                    logger.LogInformation("Found valid installed branch: {Branch}", branchName);
                    break;
                }
            }
            
            if (!hasValidBranch)
            {
                logger.LogWarning("No valid installed branches found among selected branches: {Branches}", 
                    string.Join(", ", config.SelectedBranches));
                return false;
            }
            
            logger.LogInformation("Enhanced managed environment validation passed successfully");
            return true;
        }
    }
}
