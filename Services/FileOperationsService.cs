using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ScheduleIDevelopementEnvironementManager.Services
{
    /// <summary>
    /// Service for handling file operations, executable launching, and directory management
    /// </summary>
    public class FileOperationsService
    {
        private readonly ILogger<FileOperationsService> _logger;
        
        public FileOperationsService(ILogger<FileOperationsService> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Launches an executable file asynchronously
        /// </summary>
        /// <param name="executablePath">Full path to the executable</param>
        /// <param name="arguments">Optional command line arguments</param>
        /// <param name="workingDirectory">Optional working directory (defaults to executable's directory)</param>
        /// <returns>True if successfully launched, false otherwise</returns>
        public async Task<bool> LaunchExecutableAsync(string executablePath, string? arguments = null, string? workingDirectory = null)
        {
            try
            {
                _logger.LogInformation("Launching executable: {ExecutablePath}", executablePath);
                
                // Validate executable exists
                if (!File.Exists(executablePath))
                {
                    _logger.LogError("Executable not found: {ExecutablePath}", executablePath);
                    return false;
                }
                
                // Set working directory to executable's directory if not specified
                if (string.IsNullOrEmpty(workingDirectory))
                {
                    workingDirectory = Path.GetDirectoryName(executablePath);
                }
                
                return await Task.Run(() =>
                {
                    try
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = executablePath,
                            Arguments = arguments ?? string.Empty,
                            WorkingDirectory = workingDirectory,
                            UseShellExecute = true, // Use shell execute to handle file associations
                            CreateNoWindow = false
                        };
                        
                        using var process = Process.Start(startInfo);
                        if (process != null)
                        {
                            _logger.LogInformation("Successfully launched executable: {ExecutablePath} (PID: {ProcessId})", 
                                executablePath, process.Id);
                            return true;
                        }
                        else
                        {
                            _logger.LogError("Failed to start process for executable: {ExecutablePath}", executablePath);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error launching executable: {ExecutablePath}", executablePath);
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LaunchExecutableAsync for: {ExecutablePath}", executablePath);
                return false;
            }
        }
        
        /// <summary>
        /// Deletes a directory and all its contents asynchronously
        /// </summary>
        /// <param name="directoryPath">Path to the directory to delete</param>
        /// <param name="recursive">Whether to delete subdirectories (default: true)</param>
        /// <returns>True if successfully deleted, false otherwise</returns>
        public async Task<bool> DeleteDirectoryAsync(string directoryPath, bool recursive = true)
        {
            try
            {
                _logger.LogInformation("Deleting directory: {DirectoryPath}", directoryPath);
                
                if (!Directory.Exists(directoryPath))
                {
                    _logger.LogWarning("Directory does not exist: {DirectoryPath}", directoryPath);
                    return true; // Already deleted
                }
                
                return await Task.Run(() =>
                {
                    try
                    {
                        // Remove read-only attributes from files to ensure deletion
                        if (recursive)
                        {
                            RemoveReadOnlyAttributes(directoryPath);
                        }
                        
                        Directory.Delete(directoryPath, recursive);
                        _logger.LogInformation("Successfully deleted directory: {DirectoryPath}", directoryPath);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting directory: {DirectoryPath}", directoryPath);
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteDirectoryAsync for: {DirectoryPath}", directoryPath);
                return false;
            }
        }
        
        /// <summary>
        /// Removes read-only attributes from all files in a directory recursively
        /// This helps ensure files can be deleted
        /// </summary>
        private void RemoveReadOnlyAttributes(string directoryPath)
        {
            try
            {
                var directory = new DirectoryInfo(directoryPath);
                
                // Remove read-only from the directory itself
                if ((directory.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    directory.Attributes = directory.Attributes & ~FileAttributes.ReadOnly;
                }
                
                // Process all files
                foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        if ((file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            file.Attributes = file.Attributes & ~FileAttributes.ReadOnly;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not remove read-only attribute from file: {FilePath}", file.FullName);
                    }
                }
                
                // Process all subdirectories
                foreach (var subDir in directory.GetDirectories("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        if ((subDir.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            subDir.Attributes = subDir.Attributes & ~FileAttributes.ReadOnly;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not remove read-only attribute from directory: {DirectoryPath}", subDir.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error removing read-only attributes from: {DirectoryPath}", directoryPath);
            }
        }
        
        /// <summary>
        /// Opens a directory in Windows Explorer
        /// </summary>
        /// <param name="directoryPath">Path to the directory to open</param>
        /// <returns>True if successfully opened, false otherwise</returns>
        public async Task<bool> OpenDirectoryInExplorerAsync(string directoryPath)
        {
            try
            {
                _logger.LogInformation("Opening directory in Explorer: {DirectoryPath}", directoryPath);
                
                if (!Directory.Exists(directoryPath))
                {
                    _logger.LogError("Directory does not exist: {DirectoryPath}", directoryPath);
                    return false;
                }
                
                return await Task.Run(() =>
                {
                    try
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"\"{directoryPath}\"",
                            UseShellExecute = true,
                            CreateNoWindow = true
                        };
                        
                        using var process = Process.Start(startInfo);
                        if (process != null)
                        {
                            _logger.LogInformation("Successfully opened directory in Explorer: {DirectoryPath}", directoryPath);
                            return true;
                        }
                        else
                        {
                            _logger.LogError("Failed to open directory in Explorer: {DirectoryPath}", directoryPath);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error opening directory in Explorer: {DirectoryPath}", directoryPath);
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OpenDirectoryInExplorerAsync for: {DirectoryPath}", directoryPath);
                return false;
            }
        }
        
        /// <summary>
        /// Gets the size of a directory and all its contents
        /// </summary>
        /// <param name="directoryPath">Path to the directory</param>
        /// <returns>Size in bytes, or 0 if error</returns>
        public async Task<long> GetDirectorySizeAsync(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return 0;
                }
                
                return await Task.Run(() =>
                {
                    try
                    {
                        var directory = new DirectoryInfo(directoryPath);
                        return directory.GetFiles("*", SearchOption.AllDirectories)
                                       .Sum(file => file.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error calculating directory size for: {DirectoryPath}", directoryPath);
                        return 0L;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDirectorySizeAsync for: {DirectoryPath}", directoryPath);
                return 0;
            }
        }
        
        /// <summary>
        /// Gets the number of files in a directory
        /// </summary>
        /// <param name="directoryPath">Path to the directory</param>
        /// <param name="recursive">Whether to count files in subdirectories (default: true)</param>
        /// <returns>Number of files, or 0 if error</returns>
        public async Task<int> GetFileCountAsync(string directoryPath, bool recursive = true)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return 0;
                }
                
                return await Task.Run(() =>
                {
                    try
                    {
                        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                        return Directory.GetFiles(directoryPath, "*", searchOption).Length;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error counting files in directory: {DirectoryPath}", directoryPath);
                        return 0;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFileCountAsync for: {DirectoryPath}", directoryPath);
                return 0;
            }
        }
        
        /// <summary>
        /// Checks if a file is currently in use by another process
        /// </summary>
        /// <param name="filePath">Path to the file to check</param>
        /// <returns>True if file is in use, false otherwise</returns>
        public bool IsFileInUse(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }
                
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return false; // File is not in use
            }
            catch (IOException)
            {
                return true; // File is in use
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking if file is in use: {FilePath}", filePath);
                return false;
            }
        }
        
        /// <summary>
        /// Validates that an executable file exists and can be launched
        /// </summary>
        /// <param name="executablePath">Path to the executable</param>
        /// <returns>True if valid executable, false otherwise</returns>
        public bool IsValidExecutable(string executablePath)
        {
            try
            {
                if (string.IsNullOrEmpty(executablePath))
                {
                    return false;
                }
                
                if (!File.Exists(executablePath))
                {
                    return false;
                }
                
                // Check file extension
                var extension = Path.GetExtension(executablePath).ToLowerInvariant();
                if (extension != ".exe" && extension != ".com" && extension != ".bat" && extension != ".cmd")
                {
                    return false;
                }
                
                // Try to get file info to ensure it's accessible
                var fileInfo = new FileInfo(executablePath);
                return fileInfo.Length > 0; // Executable should have some content
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validating executable: {ExecutablePath}", executablePath);
                return false;
            }
        }
    }
}