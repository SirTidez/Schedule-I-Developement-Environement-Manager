using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScheduleIDevelopementEnvironementManager.Models;
using ScheduleIDevelopementEnvironementManager.Services;
using System.ComponentModel;
using System.IO;

namespace ScheduleIDevelopementEnvironementManager
{
    public partial class CreateManagedEnvironmentForm : Form
    {
        private readonly SteamService _steamService;
        private readonly ConfigurationService _configService;
        private readonly ILogger<CreateManagedEnvironmentForm> _logger;
        private DevEnvironmentConfig _config;
        private List<SteamGameInfo> _availableGames;

        // UI Controls
        private TextBox? txtSteamLibrary;
        private Button? btnBrowseSteamLibrary;
        private TextBox? txtGameInstall;
        private Button? btnBrowseGameInstall;
        private TextBox? txtManagedEnv;
        private Button? btnBrowseManagedEnv;
        private CheckBox? chkMainBranch;
        private CheckBox? chkBetaBranch;
        private CheckBox? chkAlternateBranch;
        private CheckBox? chkAlternateBetaBranch;
        private Label? lblStatus;
        private Button? btnCreateEnvironment;
        private Button? btnCancel;

        public CreateManagedEnvironmentForm()
        {
            InitializeComponent();
            
            // Set up dependency injection
            var services = new ServiceCollection();
            
            // Use file logging instead of console logging
            var fileLoggingFactory = new FileLoggingServiceFactory();
            services.AddLogging(builder => builder.AddProvider(new FileLoggingProvider(fileLoggingFactory)));
            
            services.AddSingleton<SteamService>();
            services.AddSingleton<ConfigurationService>();
            
            var serviceProvider = services.BuildServiceProvider();
            _steamService = serviceProvider.GetRequiredService<SteamService>();
            _configService = serviceProvider.GetRequiredService<ConfigurationService>();
            _logger = serviceProvider.GetRequiredService<ILogger<CreateManagedEnvironmentForm>>();
            
            _config = new DevEnvironmentConfig();
            _availableGames = new List<SteamGameInfo>();
            
            InitializeForm();
            LoadSteamInformation();
        }

        private void InitializeForm()
        {
            this.Text = "Create Managed Environment";
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            
            // Load the application icon
            this.Icon = MainForm.LoadApplicationIcon();

            CreateControls();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            // Steam Library Section
            var lblSteamLibrary = new Label
            {
                Text = "Steam Library Path:",
                Location = new Point(20, 25),
                Size = new Size(200, 25),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            txtSteamLibrary = new TextBox
            {
                Location = new Point(20, 55),
                Size = new Size(500, 23),
                ReadOnly = true,
                Text = "C:\\Program Files (x86)\\Steam\\steamapps" // Default placeholder
            };

            btnBrowseSteamLibrary = new Button
            {
                Text = "Browse...",
                Location = new Point(530, 54),
                Size = new Size(80, 25)
            };

            // Game Installation Section
            var lblGameInstall = new Label
            {
                Text = "Schedule I Game Path:",
                Location = new Point(20, 95),
                Size = new Size(200, 25),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            txtGameInstall = new TextBox
            {
                Location = new Point(20, 125),
                Size = new Size(500, 23),
                ReadOnly = true
            };

            btnBrowseGameInstall = new Button
            {
                Text = "Browse...",
                Location = new Point(530, 124),
                Size = new Size(80, 25)
            };

            // Managed Environment Section
            var lblManagedEnv = new Label
            {
                Text = "Managed Environment Path:",
                Location = new Point(20, 165),
                Size = new Size(200, 25),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            txtManagedEnv = new TextBox
            {
                Location = new Point(20, 195),
                Size = new Size(500, 23),
                ReadOnly = true
            };

            btnBrowseManagedEnv = new Button
            {
                Text = "Browse...",
                Location = new Point(530, 194),
                Size = new Size(80, 25)
            };

            // Branch Selection Section
            var lblBranches = new Label
            {
                Text = "Select Branches to Manage:",
                Location = new Point(20, 235),
                Size = new Size(250, 25),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            chkMainBranch = new CheckBox
            {
                Text = "Main Branch",
                Location = new Point(20, 270),
                Size = new Size(130, 25)
            };

            chkBetaBranch = new CheckBox
            {
                Text = "Beta Branch",
                Location = new Point(160, 270),
                Size = new Size(130, 25)
            };

            chkAlternateBranch = new CheckBox
            {
                Text = "Alternate Branch",
                Location = new Point(300, 270),
                Size = new Size(130, 25)
            };

            chkAlternateBetaBranch = new CheckBox
            {
                Text = "Alternate Beta Branch",
                Location = new Point(440, 270),
                Size = new Size(170, 25)
            };

            // Status Label
            lblStatus = new Label
            {
                Text = "Ready to configure managed environment",
                Location = new Point(20, 315),
                Size = new Size(600, 25),
                Font = new Font(this.Font.FontFamily, 9),
                ForeColor = Color.Blue
            };

            // Buttons
            btnCreateEnvironment = new Button
            {
                Text = "Create Managed Environment",
                Location = new Point(200, 360),
                Size = new Size(200, 45),
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold),
                BackColor = Color.LightGreen,
                Enabled = false
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(420, 360),
                Size = new Size(120, 45)
            };

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblSteamLibrary, txtSteamLibrary, btnBrowseSteamLibrary,
                lblGameInstall, txtGameInstall, btnBrowseGameInstall,
                lblManagedEnv, txtManagedEnv, btnBrowseManagedEnv,
                lblBranches, chkMainBranch, chkBetaBranch, chkAlternateBranch, chkAlternateBetaBranch,
                lblStatus, btnCreateEnvironment, btnCancel
            });
        }

        private void SetupEventHandlers()
        {
            btnBrowseSteamLibrary!.Click += BtnBrowseSteamLibrary_Click;
            btnBrowseGameInstall!.Click += BtnBrowseGameInstall_Click;
            btnBrowseManagedEnv!.Click += BtnBrowseManagedEnv_Click;
            btnCreateEnvironment!.Click += BtnCreateEnvironment_Click;
            btnCancel!.Click += BtnCancel_Click;

            // Branch selection change handlers
            chkMainBranch!.CheckedChanged += BranchSelectionChanged;
            chkBetaBranch!.CheckedChanged += BranchSelectionChanged;
            chkAlternateBranch!.CheckedChanged += BranchSelectionChanged;
            chkAlternateBetaBranch!.CheckedChanged += BranchSelectionChanged;
        }

        private void BranchSelectionChanged(object? sender, EventArgs e)
        {
            UpdateCreateButtonState();
        }

        private void UpdateCreateButtonState()
        {
            bool hasValidPaths = !string.IsNullOrEmpty(_config.SteamLibraryPath) &&
                                !string.IsNullOrEmpty(_config.GameInstallPath) &&
                                !string.IsNullOrEmpty(_config.ManagedEnvironmentPath);

            bool hasSelectedBranches = _config.SelectedBranches.Count > 0;

            btnCreateEnvironment!.Enabled = hasValidPaths && hasSelectedBranches;
        }

        private void LoadSteamInformation()
        {
            try
            {
                _logger.LogInformation("Loading Steam information...");
                lblStatus!.Text = "Loading Steam information...";
                lblStatus!.ForeColor = Color.Blue;

                // Load Steam library information
                var steamLibraries = _steamService.GetSteamLibraryPaths();
                if (steamLibraries.Count > 0)
                {
                    // Auto-select the first Steam library
                    _config.SteamLibraryPath = steamLibraries[0];
                    txtSteamLibrary!.Text = _config.SteamLibraryPath;
                    
                    // Look for Schedule I game in the selected library
                    _availableGames = _steamService.GetSteamGames(_config.SteamLibraryPath);
                    var scheduleIGame = _availableGames.FirstOrDefault(g => g.AppId == "3164500");
                    
                    if (scheduleIGame != null)
                    {
                        _config.GameInstallPath = scheduleIGame.InstallPath;
                        txtGameInstall!.Text = _config.GameInstallPath;
                        _logger.LogInformation("Schedule I game found at: {Path}", _config.GameInstallPath);
                    }
                    else
                    {
                        _logger.LogWarning("Schedule I game not found in selected Steam library");
                        lblStatus!.Text = "Schedule I game not found in selected Steam library";
                        lblStatus!.ForeColor = Color.Orange;
                    }
                }
                else
                {
                    _logger.LogWarning("No Steam libraries found");
                    lblStatus!.Text = "No Steam libraries found";
                    lblStatus!.ForeColor = Color.Red;
                }

                UpdateCreateButtonState();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Steam information");
                lblStatus!.Text = $"Error loading Steam information: {ex.Message}";
                lblStatus!.ForeColor = Color.Red;
            }
        }

        private void BtnBrowseSteamLibrary_Click(object? sender, EventArgs e)
        {
            try
            {
                using var dialog = new SteamLibrarySelectionDialog(_steamService, _logger);
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dialog.SelectedLibraryPath))
                {
                    _config.SteamLibraryPath = dialog.SelectedLibraryPath;
                    txtSteamLibrary!.Text = _config.SteamLibraryPath;
                    
                    // Reload game information for the new library
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            _availableGames = _steamService.GetSteamGames(_config.SteamLibraryPath);
                            var scheduleIGame = _availableGames.FirstOrDefault(g => g.AppId == "3164500");
                            
                            if (scheduleIGame != null)
                            {
                                _config.GameInstallPath = scheduleIGame.InstallPath;
                                this.Invoke(() =>
                                {
                                    txtGameInstall!.Text = _config.GameInstallPath;
                                    UpdateCreateButtonState();
                                });
                            }
                            else
                            {
                                this.Invoke(() =>
                                {
                                    txtGameInstall!.Text = "";
                                    _config.GameInstallPath = "";
                                    UpdateCreateButtonState();
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error loading games from new Steam library");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing Steam library");
                MessageBox.Show($"Error browsing Steam library: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBrowseGameInstall_Click(object? sender, EventArgs e)
        {
            try
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "Select Schedule I Game Installation Directory",
                    ShowNewFolderButton = false
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var selectedPath = dialog.SelectedPath;
                    
                    // Verify this is a valid Schedule I installation
                    if (File.Exists(Path.Combine(selectedPath, "ScheduleI.exe")))
                    {
                        _config.GameInstallPath = selectedPath;
                        txtGameInstall!.Text = selectedPath;
                        _logger.LogInformation("Game installation path set to: {Path}", selectedPath);
                    }
                    else
                    {
                        MessageBox.Show("The selected directory does not appear to contain Schedule I. " +
                                      "Please select the directory containing ScheduleI.exe", 
                                      "Invalid Directory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing game installation directory");
                MessageBox.Show($"Error browsing game installation directory: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateCreateButtonState();
            }
        }

        private void BtnBrowseManagedEnv_Click(object? sender, EventArgs e)
        {
            try
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "Select Managed Environment Directory",
                    ShowNewFolderButton = true
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _config.ManagedEnvironmentPath = dialog.SelectedPath;
                    txtManagedEnv!.Text = dialog.SelectedPath;
                    _logger.LogInformation("Managed environment path set to: {Path}", dialog.SelectedPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing managed environment directory");
                MessageBox.Show($"Error browsing managed environment directory: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateCreateButtonState();
            }
        }

        private async void BtnCreateEnvironment_Click(object? sender, EventArgs e)
        {
            try
            {
                btnCreateEnvironment!.Enabled = false;
                lblStatus!.Text = "Creating managed environment...";
                lblStatus!.ForeColor = Color.Blue;

                // Update selected branches
                UpdateSelectedBranches();

                // Create the managed environment
                await CreateManagedEnvironmentAsync();

                lblStatus!.Text = "Managed environment created successfully!";
                lblStatus!.ForeColor = Color.Green;
                
                // Save configuration
                await _configService.SaveConfigurationAsync(_config);
                
                MessageBox.Show("Managed environment created successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Close this form and return to main form
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating managed environment");
                lblStatus!.Text = $"Error: {ex.Message}";
                lblStatus!.ForeColor = Color.Red;
                MessageBox.Show($"Error creating managed environment: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCreateEnvironment!.Enabled = true;
            }
        }

        private void UpdateSelectedBranches()
        {
            _config.SelectedBranches.Clear();
            
            if (chkMainBranch!.Checked)
                _config.SelectedBranches.Add("main-branch");
            if (chkBetaBranch!.Checked)
                _config.SelectedBranches.Add("beta-branch");
            if (chkAlternateBranch!.Checked)
                _config.SelectedBranches.Add("alternate-branch");
            if (chkAlternateBetaBranch!.Checked)
                _config.SelectedBranches.Add("alternate-beta-branch");
        }

        private async Task CreateManagedEnvironmentAsync()
        {
            try
            {
                // Create the main managed environment directory
                if (!Directory.Exists(_config.ManagedEnvironmentPath))
                {
                    Directory.CreateDirectory(_config.ManagedEnvironmentPath);
                }

                // Get current branch from Steam
                var currentBranch = _steamService.GetCurrentBranchFromGamePath(_config.GameInstallPath);
                if (string.IsNullOrEmpty(currentBranch))
                {
                    currentBranch = "main-branch"; // Default fallback
                }

                _logger.LogInformation("Starting managed environment creation. Current branch: {CurrentBranch}", currentBranch);

                // Process each selected branch
                for (int i = 0; i < _config.SelectedBranches.Count; i++)
                {
                    var targetBranch = _config.SelectedBranches[i];
                    
                    // Skip if this branch is already the current one
                    if (targetBranch == currentBranch)
                    {
                        _logger.LogInformation("Skipping {Branch} - already current branch", targetBranch);
                        continue;
                    }

                    // Show progress form for this branch
                    using var progressForm = new CopyProgressForm();
                    progressForm.Show();
                    
                    try
                    {
                        // Copy current game state to target branch folder
                        await CopyGameToBranchAsync(targetBranch, progressForm);
                        
                        // Close progress form
                        progressForm.SetCopyComplete();
                        progressForm.Close();
                        
                        // If this isn't the last branch, prompt user to switch
                        if (i < _config.SelectedBranches.Count - 1)
                        {
                            var nextBranch = _config.SelectedBranches[i + 1];
                            
                            // Show branch switch prompt
                            using var switchPrompt = new BranchSwitchPromptForm(currentBranch, nextBranch);
                            var result = switchPrompt.ShowDialog();
                            
                            if (result == DialogResult.Cancel)
                            {
                                _logger.LogInformation("User cancelled branch switch operation");
                                break;
                            }
                            
                            // Wait for user to actually switch the branch
                            var switchSuccess = await _steamService.WaitForBranchSwitchAsync(nextBranch, _config.GameInstallPath);
                            
                            if (!switchSuccess)
                            {
                                throw new Exception($"Failed to detect branch switch to {nextBranch} within timeout period");
                            }
                            
                            // Update current branch for next iteration
                            currentBranch = nextBranch;
                            _logger.LogInformation("Successfully switched to branch: {Branch}", currentBranch);
                        }
                    }
                    catch (Exception ex)
                    {
                        progressForm.SetCopyFailed(ex.Message);
                        progressForm.Close();
                        throw;
                    }
                }

                _logger.LogInformation("Managed environment created successfully at: {Path}", _config.ManagedEnvironmentPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating managed environment");
                throw;
            }
        }

        private async Task CopyGameToBranchAsync(string branchName, CopyProgressForm progressForm)
        {
            try
            {
                progressForm.UpdateStatus($"Copying game files to {branchName}...");
                progressForm.LogMessage($"Starting copy operation to {branchName}");

                var branchPath = Path.Combine(_config.ManagedEnvironmentPath, branchName);
                
                // Create branch directory if it doesn't exist
                if (!Directory.Exists(branchPath))
                {
                    Directory.CreateDirectory(branchPath);
                    progressForm.LogMessage($"Created directory: {branchPath}");
                }

                // Get all files in the game directory
                var gameFiles = Directory.GetFiles(_config.GameInstallPath, "*", SearchOption.AllDirectories);
                var totalFiles = gameFiles.Length;
                var copiedFiles = 0;

                foreach (var sourceFile in gameFiles)
                {
                    try
                    {
                        // Calculate relative path from game directory
                        var relativePath = Path.GetRelativePath(_config.GameInstallPath, sourceFile);
                        var targetFile = Path.Combine(branchPath, relativePath);
                        
                        // Create target directory if it doesn't exist
                        var targetDir = Path.GetDirectoryName(targetFile);
                        if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }
                        
                        // Copy the file
                        File.Copy(sourceFile, targetFile, true);
                        copiedFiles++;
                        
                        // Update progress
                        var progress = (int)((double)copiedFiles / totalFiles * 100);
                        progressForm.UpdateProgress(progress);
                        progressForm.LogMessage($"Copied: {relativePath}");
                        
                        // Small delay to allow UI updates
                        await Task.Delay(10);
                    }
                    catch (Exception ex)
                    {
                        progressForm.LogMessage($"Error copying {sourceFile}: {ex.Message}");
                        // Continue with other files
                    }
                }

                progressForm.LogMessage($"Copy operation to {branchName} completed. {copiedFiles}/{totalFiles} files copied.");
            }
            catch (Exception ex)
            {
                progressForm.LogMessage($"Fatal error copying to {branchName}: {ex.Message}");
                throw;
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
