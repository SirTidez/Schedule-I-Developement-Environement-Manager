namespace ScheduleIDevelopementEnvironementManager.Models
{
    /// <summary>
    /// Represents the configuration for a managed development environment
    /// </summary>
    public class DevEnvironmentConfig
    {
        public string SteamLibraryPath { get; set; } = string.Empty;
        public string GameInstallPath { get; set; } = string.Empty;
        public string ManagedEnvironmentPath { get; set; } = string.Empty;
        public List<string> SelectedBranches { get; set; } = new List<string>();
        public string? InstalledBranch { get; set; } = null;
        
        /// <summary>
        /// Dictionary mapping branch names to their current build IDs
        /// </summary>
        public Dictionary<string, string> BranchBuildIds { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Timestamp when the configuration was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Version of the configuration format
        /// </summary>
        public string ConfigVersion { get; set; } = "1.0";
        
        public static readonly List<string> AvailableBranches = new List<string>
        {
            "main-branch",
            "beta-branch", 
            "alternate-branch",
            "alternate-beta-branch"
        };
        
        /// <summary>
        /// Gets the build ID for a specific branch
        /// </summary>
        public string GetBuildIdForBranch(string branchName)
        {
            return BranchBuildIds.TryGetValue(branchName, out var buildId) ? buildId : string.Empty;
        }
        
        /// <summary>
        /// Sets the build ID for a specific branch
        /// </summary>
        public void SetBuildIdForBranch(string branchName, string buildId)
        {
            if (BranchBuildIds.ContainsKey(branchName))
            {
                BranchBuildIds[branchName] = buildId;
            }
            else
            {
                BranchBuildIds.Add(branchName, buildId);
            }
            LastUpdated = DateTime.Now;
        }
        
        /// <summary>
        /// Updates the configuration with new values
        /// </summary>
        public void UpdateConfiguration(string steamLibraryPath, string gameInstallPath, string managedEnvironmentPath, List<string> selectedBranches)
        {
            SteamLibraryPath = steamLibraryPath;
            GameInstallPath = gameInstallPath;
            ManagedEnvironmentPath = managedEnvironmentPath;
            SelectedBranches = selectedBranches ?? new List<string>();
            LastUpdated = DateTime.Now;
        }
    }
}
