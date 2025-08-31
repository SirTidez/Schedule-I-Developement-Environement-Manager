using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ScheduleIDevelopementEnvironementManager.UI
{
    /// <summary>
    /// Enhanced diagnostics and logging system for form operations and user interactions
    /// </summary>
    public static class FormDiagnostics
    {
        private static ILogger? _logger;
        private static readonly Dictionary<string, Stopwatch> _performanceTimers = new();
        private static readonly Dictionary<string, DateTime> _formLoadTimes = new();
        private static int _totalFormsLoaded = 0;
        private static int _totalUserInteractions = 0;

        /// <summary>
        /// Initialize the diagnostics system with a logger
        /// </summary>
        public static void Initialize(ILogger logger)
        {
            _logger = logger;
            _logger.LogInformation("FormDiagnostics system initialized");
        }

        /// <summary>
        /// Log form initialization start
        /// </summary>
        public static void LogFormInitialization(string formName)
        {
            _totalFormsLoaded++;
            _formLoadTimes[formName] = DateTime.Now;
            _logger?.LogInformation("Form initialization started: {FormName} (Total forms loaded: {TotalForms})", 
                formName, _totalFormsLoaded);
        }

        /// <summary>
        /// Log form load completion with timing
        /// </summary>
        public static void LogFormLoadComplete(string formName)
        {
            if (_formLoadTimes.TryGetValue(formName, out var startTime))
            {
                var loadTime = DateTime.Now - startTime;
                _logger?.LogInformation("Form load completed: {FormName} in {LoadTime}ms", 
                    formName, loadTime.TotalMilliseconds);
                _formLoadTimes.Remove(formName);
            }
            else
            {
                _logger?.LogInformation("Form load completed: {FormName}", formName);
            }
        }

        /// <summary>
        /// Start performance tracking for a specific operation
        /// </summary>
        public static void StartPerformanceTracking(string operationName)
        {
            if (_performanceTimers.ContainsKey(operationName))
            {
                _performanceTimers[operationName].Restart();
            }
            else
            {
                _performanceTimers[operationName] = Stopwatch.StartNew();
            }
            
            _logger?.LogDebug("Started performance tracking: {Operation}", operationName);
        }

        /// <summary>
        /// End performance tracking and log results
        /// </summary>
        public static void EndPerformanceTracking(string operationName)
        {
            if (_performanceTimers.TryGetValue(operationName, out var timer))
            {
                timer.Stop();
                _logger?.LogInformation("Performance: {Operation} completed in {ElapsedMs}ms", 
                    operationName, timer.ElapsedMilliseconds);
                _performanceTimers.Remove(operationName);
            }
            else
            {
                _logger?.LogWarning("Attempted to end performance tracking for unknown operation: {Operation}", 
                    operationName);
            }
        }

        /// <summary>
        /// Log user interactions for analytics and debugging
        /// </summary>
        public static void LogUserInteraction(string action, string context, object? data = null)
        {
            _totalUserInteractions++;
            
            if (data != null)
            {
                _logger?.LogInformation("User interaction: {Action} in {Context} with data: {Data} (Total interactions: {TotalInteractions})", 
                    action, context, data, _totalUserInteractions);
            }
            else
            {
                _logger?.LogInformation("User interaction: {Action} in {Context} (Total interactions: {TotalInteractions})", 
                    action, context, _totalUserInteractions);
            }
        }

        /// <summary>
        /// Log button state changes for debugging
        /// </summary>
        public static void LogButtonStateChange(string buttonName, bool enabled, string reason = "")
        {
            var reasonText = string.IsNullOrEmpty(reason) ? "" : $" - {reason}";
            _logger?.LogDebug("Button state changed: {ButtonName} enabled={Enabled}{Reason}", 
                buttonName, enabled, reasonText);
        }

        /// <summary>
        /// Log bulk theme application for performance monitoring
        /// </summary>
        public static void LogBulkThemeApplication(string formName, int controlCount, int successCount)
        {
            _logger?.LogInformation("Theme applied to {FormName}: {SuccessCount}/{TotalCount} controls themed successfully", 
                formName, successCount, controlCount);
        }

        /// <summary>
        /// Log form navigation events
        /// </summary>
        public static void LogFormNavigation(string fromForm, string toForm, string reason = "")
        {
            var reasonText = string.IsNullOrEmpty(reason) ? "" : $" - {reason}";
            _logger?.LogInformation("Form navigation: {FromForm} -> {ToForm}{Reason}", 
                fromForm, toForm, reasonText);
        }

        /// <summary>
        /// Log validation errors for form debugging
        /// </summary>
        public static void LogValidationError(string formName, string fieldName, string error)
        {
            _logger?.LogWarning("Validation error in {FormName}.{FieldName}: {Error}", 
                formName, fieldName, error);
        }

        /// <summary>
        /// Log data loading operations
        /// </summary>
        public static void LogDataOperation(string operation, string dataType, int itemCount = 0, bool success = true)
        {
            if (success)
            {
                _logger?.LogInformation("Data operation successful: {Operation} {DataType} ({ItemCount} items)", 
                    operation, dataType, itemCount);
            }
            else
            {
                _logger?.LogError("Data operation failed: {Operation} {DataType}", 
                    operation, dataType);
            }
        }

        /// <summary>
        /// Get current diagnostics summary
        /// </summary>
        public static string GetDiagnosticsSummary()
        {
            return $"Forms loaded: {_totalFormsLoaded}, User interactions: {_totalUserInteractions}, Active timers: {_performanceTimers.Count}";
        }

        /// <summary>
        /// Log application startup metrics
        /// </summary>
        public static void LogApplicationStartup(TimeSpan startupTime, string version = "")
        {
            var versionText = string.IsNullOrEmpty(version) ? "" : $" v{version}";
            _logger?.LogInformation("Application startup completed{Version} in {StartupTime}ms", 
                versionText, startupTime.TotalMilliseconds);
        }

        /// <summary>
        /// Log memory usage information
        /// </summary>
        public static void LogMemoryUsage(string context = "")
        {
            var process = Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64 / 1024 / 1024; // Convert to MB
            var contextText = string.IsNullOrEmpty(context) ? "" : $" ({context})";
            
            _logger?.LogDebug("Memory usage{Context}: {WorkingSetMB} MB", contextText, workingSet);
        }
    }
}