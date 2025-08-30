using System.Text.Json;
using Microsoft.Extensions.Logging;
using ScheduleIDevelopementEnvironementManager.Models;
using System.IO;

namespace ScheduleIDevelopementEnvironementManager.Services
{
    /// <summary>
    /// Service for managing application configuration persistence
    /// </summary>
    public class ConfigurationService
    {
        private readonly ILogger<ConfigurationService> _logger;
        private readonly string _configDirectory;
        private readonly string _configFilePath;
        private const string ConfigFileName = "dev_environment_config.json";

        public ConfigurationService(ILogger<ConfigurationService> logger)
        {
            _logger = logger;
            
            // Set up configuration directory in AppData\LocalLow\TVGS\Schedule I\Developer Env\config
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _configDirectory = Path.Combine(appDataPath, "Schedule I Developer Env", "config");
            _configFilePath = Path.Combine(_configDirectory, ConfigFileName);
            
            _logger.LogInformation("Configuration directory: {ConfigDir}", _configDirectory);
            _logger.LogInformation("Configuration file: {ConfigFile}", _configFilePath);
        }

        /// <summary>
        /// Saves the development environment configuration to file
        /// </summary>
        public async Task SaveConfigurationAsync(DevEnvironmentConfig config)
        {
            try
            {
                // Ensure the configuration directory exists
                if (!Directory.Exists(_configDirectory))
                {
                    Directory.CreateDirectory(_configDirectory);
                    _logger.LogInformation("Created configuration directory: {ConfigDir}", _configDirectory);
                }

                // Serialize configuration to JSON
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonString = JsonSerializer.Serialize(config, jsonOptions);
                
                // Write to file
                await File.WriteAllTextAsync(_configFilePath, jsonString);
                
                _logger.LogInformation("Configuration saved successfully to: {ConfigFile}", _configFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration to: {ConfigFile}", _configFilePath);
                throw;
            }
        }

        /// <summary>
        /// Loads the development environment configuration from file
        /// </summary>
        public async Task<DevEnvironmentConfig?> LoadConfigurationAsync()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    _logger.LogInformation("Configuration file not found, returning default configuration");
                    return new DevEnvironmentConfig();
                }

                var jsonString = await File.ReadAllTextAsync(_configFilePath);
                
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var config = JsonSerializer.Deserialize<DevEnvironmentConfig>(jsonString, jsonOptions);
                
                if (config != null)
                {
                    _logger.LogInformation("Configuration loaded successfully from: {ConfigFile}", _configFilePath);
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize configuration, returning default");
                    config = new DevEnvironmentConfig();
                }

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration from: {ConfigFile}", _configFilePath);
                return new DevEnvironmentConfig();
            }
        }

        /// <summary>
        /// Checks if a configuration file exists
        /// </summary>
        public bool ConfigurationExists()
        {
            return File.Exists(_configFilePath);
        }

        /// <summary>
        /// Gets the configuration file path
        /// </summary>
        public string GetConfigFilePath()
        {
            return _configFilePath;
        }

        /// <summary>
        /// Gets the configuration directory path
        /// </summary>
        public string GetConfigDirectory()
        {
            return _configDirectory;
        }
    }
}
