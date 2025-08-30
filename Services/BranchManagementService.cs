using Microsoft.Extensions.Logging;
using ScheduleIDevelopementEnvironementManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ScheduleIDevelopementEnvironementManager.Services
{
    /// <summary>
    /// Service for managing branch operations in the development environment
    /// </summary>
    public class BranchManagementService
    {
        private readonly ILogger<BranchManagementService> _logger;
        private readonly SteamService _steamService;
        private readonly ConfigurationService _configService;
        private readonly FileOperationsService _fileOperationsService;
        
        public BranchManagementService(
            ILogger<BranchManagementService> logger,
            SteamService steamService,
            ConfigurationService configService,
            FileOperationsService fileOperationsService)
        {
            _logger = logger;
            _steamService = steamService;
            _configService = configService;
            _fileOperationsService = fileOperationsService;
        }
        
        /// <summary>
        /// Gets information about all branches in the managed environment
        /// </summary>
        public async Task<List<BranchInfo>> GetAllBranchesAsync(DevEnvironmentConfig config)
        {
            try
            {
                _logger.LogInformation("Loading branch information from managed environment");
                var branches = new List<BranchInfo>();
                
                // Process all available branch types
                foreach (var branchName in DevEnvironmentConfig.AvailableBranches)
                {
                    var branchInfo = await GetBranchInfoAsync(branchName, config);
                    branches.Add(branchInfo);
                }
                
                // Identify current Steam branch
                var currentSteamBranch = await _steamService.GetCurrentBranchFromGamePathAsync(config.GameInstallPath);
                if (!string.IsNullOrEmpty(currentSteamBranch))
                {
                    var currentBranch = branches.FirstOrDefault(b => b.BranchName == currentSteamBranch);
                    if (currentBranch != null)
                    {
                        currentBranch.IsCurrentSteamBranch = true;
                    }
                }
                
                _logger.LogInformation("Loaded information for {Count} branches", branches.Count);
                return branches;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading branch information");
                throw;
            }
        }
        
        /// <summary>
        /// Gets detailed information about a specific branch
        /// </summary>
        public async Task<BranchInfo> GetBranchInfoAsync(string branchName, DevEnvironmentConfig config)
        {
            try
            {
                var branchInfo = new BranchInfo
                {
                    BranchName = branchName,
                    DisplayName = BranchInfo.GetDisplayName(branchName),
                    FolderPath = Path.Combine(config.ManagedEnvironmentPath, branchName),
                    ExecutablePath = Path.Combine(config.ManagedEnvironmentPath, branchName, "Schedule I.exe")
                };
                
                // Check if branch is installed
                if (branchInfo.IsInstalled)
                {
                    await PopulateInstalledBranchInfoAsync(branchInfo, config);
                }
                else
                {
                    branchInfo.Status = BranchStatus.NotInstalled;
                }
                
                return branchInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch info for {BranchName}", branchName);
                return new BranchInfo
                {
                    BranchName = branchName,
                    DisplayName = BranchInfo.GetDisplayName(branchName),
                    Status = BranchStatus.Error
                };
            }
        }
        
        /// <summary>
        /// Populates detailed information for an installed branch
        /// </summary>
        private async Task PopulateInstalledBranchInfoAsync(BranchInfo branchInfo, DevEnvironmentConfig config)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(branchInfo.FolderPath);
                branchInfo.LastModified = directoryInfo.LastWriteTime;
                
                // Calculate directory size and file count
                var (size, count) = await CalculateDirectoryStatsAsync(branchInfo.FolderPath);
                branchInfo.DirectorySize = size;
                branchInfo.FileCount = count;
                
                // Get local build ID
                branchInfo.LocalBuildId = config.GetBuildIdForBranch(branchInfo.BranchName);
                
                // Get current Steam build ID for comparison
                branchInfo.SteamBuildId = await _steamService.GetCurrentBuildIdForBranchAsync(branchInfo.BranchName) ?? string.Empty;
                
                // Determine status based on build ID comparison
                branchInfo.Status = await DetermineBranchStatusAsync(branchInfo, config);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error populating branch info for {BranchName}", branchInfo.BranchName);
                branchInfo.Status = BranchStatus.Error;
            }
        }
        
        /// <summary>
        /// Calculates total size and file count for a directory
        /// </summary>
        private async Task<(long size, int count)> CalculateDirectoryStatsAsync(string directoryPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var directoryInfo = new DirectoryInfo(directoryPath);
                    var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
                    
                    long totalSize = files.Sum(f => f.Length);
                    int fileCount = files.Length;
                    
                    return (totalSize, fileCount);
                }
                catch
                {
                    return (0, 0);
                }
            });
        }
        
        /// <summary>
        /// Determines branch status based on build ID comparison and Steam integration
        /// </summary>
        private async Task<BranchStatus> DetermineBranchStatusAsync(BranchInfo branchInfo, DevEnvironmentConfig config)
        {
            try
            {
                // Check if executable exists
                if (!branchInfo.IsInstalled)
                {
                    return BranchStatus.NotInstalled;
                }

                // Get current Steam branch to compare
                var currentSteamBranch = _steamService.GetCurrentBranchFromGamePath(config.GameInstallPath);
                
                // If this branch matches current Steam branch, get real-time build ID
                if (branchInfo.BranchName == currentSteamBranch)
                {
                    var currentBuildId = await _steamService.GetCurrentBuildIdAsync(config.GameInstallPath);
                    if (!string.IsNullOrEmpty(currentBuildId))
                    {
                        branchInfo.SteamBuildId = currentBuildId;
                        branchInfo.IsCurrentSteamBranch = true;
                    }
                }
                
                // If we can't get Steam build ID, check file timestamps
                if (string.IsNullOrEmpty(branchInfo.SteamBuildId))
                {
                    return CheckFileBasedStatus(branchInfo);
                }
                
                // If we don't have a local build ID, assume update may be available
                if (string.IsNullOrEmpty(branchInfo.LocalBuildId))
                {
                    // If this is the current Steam branch and files are recent, assume up to date
                    if (branchInfo.IsCurrentSteamBranch && 
                        branchInfo.LastModified > DateTime.Now.AddDays(-1))
                    {
                        return BranchStatus.UpToDate;
                    }
                    return BranchStatus.UpdateAvailable;
                }
                
                // Compare build IDs
                var buildIdMatch = branchInfo.LocalBuildId == branchInfo.SteamBuildId;
                
                // If build IDs match, verify with file timestamps for additional confidence
                if (buildIdMatch)
                {
                    return branchInfo.IsCurrentSteamBranch || 
                           branchInfo.LastModified > DateTime.Now.AddHours(-6) ?
                           BranchStatus.UpToDate : 
                           BranchStatus.UpdateAvailable;
                }
                
                return BranchStatus.UpdateAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error determining branch status for {BranchName}", branchInfo.BranchName);
                return BranchStatus.Error;
            }
        }
        
        /// <summary>
        /// Determines status based on file timestamps and installation age
        /// </summary>
        private BranchStatus CheckFileBasedStatus(BranchInfo branchInfo)
        {
            try
            {
                var ageInDays = (DateTime.Now - branchInfo.LastModified).TotalDays;
                
                // If files are very recent (within 6 hours), likely up to date
                if (ageInDays < 0.25)
                {
                    return BranchStatus.UpToDate;
                }
                
                // If files are moderately old (1-7 days), might need update
                if (ageInDays > 1 && ageInDays < 7)
                {
                    return BranchStatus.UpdateAvailable;
                }
                
                // If files are old (more than a week), probably needs update
                if (ageInDays >= 7)
                {
                    return BranchStatus.UpdateAvailable;
                }
                
                // Default to up to date for recently modified files
                return BranchStatus.UpToDate;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error in file-based status check for {BranchName}", branchInfo.BranchName);
                return BranchStatus.Error;
            }
        }
        
        /// <summary>
        /// Launches the game executable for a specific branch
        /// </summary>
        public async Task<bool> LaunchBranchAsync(BranchInfo branchInfo)
        {
            try
            {
                _logger.LogInformation("Launching game for branch: {BranchName}", branchInfo.BranchName);
                
                if (!branchInfo.IsInstalled)
                {
                    _logger.LogWarning("Cannot launch branch {BranchName} - not installed", branchInfo.BranchName);
                    return false;
                }
                
                return await _fileOperationsService.LaunchExecutableAsync(branchInfo.ExecutablePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching branch {BranchName}", branchInfo.BranchName);
                return false;
            }
        }
        
        /// <summary>
        /// Deletes a branch from the managed environment
        /// </summary>
        public async Task<bool> DeleteBranchAsync(BranchInfo branchInfo, DevEnvironmentConfig config)
        {
            try
            {
                _logger.LogInformation("Deleting branch: {BranchName}", branchInfo.BranchName);
                
                if (!branchInfo.IsInstalled)
                {
                    _logger.LogWarning("Branch {BranchName} is not installed", branchInfo.BranchName);
                    return true; // Already not installed
                }
                
                // Delete the branch directory
                await _fileOperationsService.DeleteDirectoryAsync(branchInfo.FolderPath);
                
                // Remove from selected branches
                config.SelectedBranches.Remove(branchInfo.BranchName);
                
                // Remove build ID tracking
                if (config.BranchBuildIds.ContainsKey(branchInfo.BranchName))
                {
                    config.BranchBuildIds.Remove(branchInfo.BranchName);
                }
                
                // Save updated configuration
                await _configService.SaveConfigurationAsync(config);
                
                _logger.LogInformation("Successfully deleted branch: {BranchName}", branchInfo.BranchName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting branch {BranchName}", branchInfo.BranchName);
                return false;
            }
        }
        
        /// <summary>
        /// Gets a list of branches that can be added (not currently in managed environment)
        /// </summary>
        public List<string> GetAvailableBranchesToAdd(DevEnvironmentConfig config)
        {
            var installedBranches = config.SelectedBranches.ToHashSet();
            return DevEnvironmentConfig.AvailableBranches
                .Where(branch => !installedBranches.Contains(branch))
                .ToList();
        }
        
        /// <summary>
        /// Refreshes branch information for all branches
        /// </summary>
        public async Task<List<BranchInfo>> RefreshBranchesAsync(DevEnvironmentConfig config)
        {
            try
            {
                _logger.LogInformation("Refreshing branch information");
                return await GetAllBranchesAsync(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing branch information");
                throw;
            }
        }
        
        /// <summary>
        /// Updates the local build ID for a branch after an update
        /// </summary>
        public async Task UpdateBranchBuildIdAsync(string branchName, string buildId, DevEnvironmentConfig config)
        {
            try
            {
                config.SetBuildIdForBranch(branchName, buildId);
                await _configService.SaveConfigurationAsync(config);
                _logger.LogInformation("Updated build ID for branch {BranchName}: {BuildId}", branchName, buildId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating build ID for branch {BranchName}", branchName);
                throw;
            }
        }
    }
}