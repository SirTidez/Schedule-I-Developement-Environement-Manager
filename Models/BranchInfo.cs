using System;
using System.IO;

namespace ScheduleIDevelopementEnvironementManager.Models
{
    /// <summary>
    /// Represents information about a managed branch installation
    /// </summary>
    public class BranchInfo
    {
        /// <summary>
        /// Internal branch name (e.g., "main-branch", "beta-branch")
        /// </summary>
        public string BranchName { get; set; } = string.Empty;
        
        /// <summary>
        /// Human-readable display name for the branch
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
        
        /// <summary>
        /// Full path to the branch folder in managed environment
        /// </summary>
        public string FolderPath { get; set; } = string.Empty;
        
        /// <summary>
        /// Full path to the Schedule I.exe executable for this branch
        /// </summary>
        public string ExecutablePath { get; set; } = string.Empty;
        
        /// <summary>
        /// Total size of the branch directory in bytes
        /// </summary>
        public long DirectorySize { get; set; }
        
        /// <summary>
        /// Number of files in the branch directory
        /// </summary>
        public int FileCount { get; set; }
        
        /// <summary>
        /// When the branch folder was last modified
        /// </summary>
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// Build ID of the local managed copy
        /// </summary>
        public string LocalBuildId { get; set; } = string.Empty;
        
        /// <summary>
        /// Current build ID from Steam for this branch
        /// </summary>
        public string SteamBuildId { get; set; } = string.Empty;
        
        /// <summary>
        /// Current status of the branch (up-to-date, needs update, etc.)
        /// </summary>
        public BranchStatus Status { get; set; }
        
        /// <summary>
        /// Whether this branch matches the currently installed Steam branch
        /// </summary>
        public bool IsCurrentSteamBranch { get; set; }
        
        /// <summary>
        /// Whether the branch folder exists and has a valid executable
        /// </summary>
        public bool IsInstalled => !string.IsNullOrEmpty(FolderPath) && 
                                   Directory.Exists(FolderPath) && 
                                   File.Exists(ExecutablePath);
        
        /// <summary>
        /// Formatted display string for directory size
        /// </summary>
        public string FormattedSize => FormatFileSize(DirectorySize);
        
        /// <summary>
        /// Formatted display string for file count
        /// </summary>
        public string FormattedFileCount => FileCount > 0 ? $"{FileCount:N0}" : "---";
        
        /// <summary>
        /// Formatted display string for last modified date
        /// </summary>
        public string FormattedLastModified => LastModified != DateTime.MinValue ? 
                                               LastModified.ToString("MM/dd") : "---";
        
        /// <summary>
        /// Gets the status icon character for display
        /// </summary>
        public string StatusIcon => Status switch
        {
            BranchStatus.UpToDate => "✅",
            BranchStatus.UpdateAvailable => "⚠️",
            BranchStatus.NotInstalled => "➕",
            BranchStatus.Error => "❌",
            _ => "❓"
        };
        
        /// <summary>
        /// Gets the status description for tooltips
        /// </summary>
        public string StatusDescription => Status switch
        {
            BranchStatus.UpToDate => "Branch is up to date with Steam",
            BranchStatus.UpdateAvailable => "Steam has a newer version available",
            BranchStatus.NotInstalled => "Branch is not installed locally",
            BranchStatus.Error => "Error checking branch status",
            _ => "Unknown status"
        };
        
        /// <summary>
        /// Creates a display-friendly branch name from internal name
        /// </summary>
        public static string GetDisplayName(string branchName)
        {
            return branchName switch
            {
                "main-branch" => "Main Branch",
                "beta-branch" => "Beta Branch", 
                "alternate-branch" => "Alternate Branch",
                "alternate-beta-branch" => "Alternate Beta Branch",
                _ => branchName.Replace("-", " ").ToTitleCase()
            };
        }
        
        /// <summary>
        /// Formats file size in human-readable format
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "---";
            
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
    }
    
    /// <summary>
    /// Represents the status of a managed branch
    /// </summary>
    public enum BranchStatus
    {
        /// <summary>
        /// Branch is up to date with Steam version
        /// </summary>
        UpToDate,
        
        /// <summary>
        /// Steam has a newer version available for this branch
        /// </summary>
        UpdateAvailable,
        
        /// <summary>
        /// Branch is not installed in the managed environment
        /// </summary>
        NotInstalled,
        
        /// <summary>
        /// Error occurred while checking branch status
        /// </summary>
        Error
    }
}

/// <summary>
/// Extension methods for string manipulation
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Converts a string to title case
    /// </summary>
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
            
        var words = input.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }
        return string.Join(" ", words);
    }
}