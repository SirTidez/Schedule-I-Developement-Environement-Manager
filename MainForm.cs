using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScheduleIDevelopementEnvironementManager.Models;
using ScheduleIDevelopementEnvironementManager.Services;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace ScheduleIDevelopementEnvironementManager
{
    public partial class MainForm : Form
    {
        private readonly SteamService _steamService;
        private readonly ConfigurationService _configService;
        private readonly ILogger<MainForm> _logger;
        private DevEnvironmentConfig _config;
        private List<SteamGameInfo> _availableGames;

        public MainForm()
        {
            InitializeComponent();
            
            // Set up dependency injection
            var services = new ServiceCollection();
            
            // Use file logging instead of console logging
            var fileLoggingFactory = new FileLoggingServiceFactory();
            services.AddLogging(builder => builder.AddProvider(new FileLoggingProvider(fileLoggingFactory)));
            
            services.AddSingleton<SteamService>();
            services.AddSingleton<ConfigurationService>();
            services.AddSingleton<MainForm>();
            
            var serviceProvider = services.BuildServiceProvider();
            _steamService = serviceProvider.GetRequiredService<SteamService>();
            _configService = serviceProvider.GetRequiredService<ConfigurationService>();
            _logger = serviceProvider.GetRequiredService<ILogger<MainForm>>();
            
            _config = new DevEnvironmentConfig();
            _availableGames = new List<SteamGameInfo>();
            
            InitializeForm();
            LoadConfiguration();
            LoadSteamInformation();
        }

        private void InitializeForm()
        {
            this.Text = "Schedule I Development Environment Manager";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            CreateControls();
            SetupEventHandlers();
        }

        private async void LoadConfiguration()
        {
            try
            {
                _logger.LogInformation("Loading configuration...");
                
                var loadedConfig = await _configService.LoadConfigurationAsync();
                if (loadedConfig != null)
                {
                    _config = loadedConfig;
                    _logger.LogInformation("Configuration loaded successfully");
                    
                    // Update UI with loaded configuration
                    if (!string.IsNullOrEmpty(_config.SteamLibraryPath))
                    {
                        txtSteamLibrary!.Text = _config.SteamLibraryPath;
                    }
                    
                    if (!string.IsNullOrEmpty(_config.GameInstallPath))
                    {
                        txtGameInstall!.Text = _config.GameInstallPath;
                    }
                    
                    if (!string.IsNullOrEmpty(_config.ManagedEnvironmentPath))
                    {
                        txtManagedEnv!.Text = _config.ManagedEnvironmentPath;
                    }
                    
                    // Update branch selections
                    UpdateBranchCheckboxes();
                    
                    // Update current branch display
                    if (!string.IsNullOrEmpty(_config.InstalledBranch))
                    {
                        var branchIndex = DevEnvironmentConfig.AvailableBranches.IndexOf(_config.InstalledBranch);
                        if (branchIndex >= 0)
                        {
                            cboCurrentBranch!.SelectedIndex = branchIndex;
                        }
                    }
                    
                    // Update configuration info display
                    UpdateConfigurationInfoDisplay();
                }
                else
                {
                    _logger.LogInformation("No existing configuration found, using defaults");
                    UpdateConfigurationInfoDisplay();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration");
            }
        }

        private void CreateControls()
        {
            // Steam Library Section
            var lblSteamLibrary = new Label
            {
                Text = "Steam Library Path:",
                Location = new Point(20, 20),
                Size = new Size(150, 20),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            txtSteamLibrary = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(500, 23),
                ReadOnly = true,
                Text = "C:\\Program Files (x86)\\Steam\\steamapps" // Default placeholder
            };

            btnBrowseSteamLibrary = new Button
            {
                Text = "Browse...",
                Location = new Point(530, 44),
                Size = new Size(80, 25)
            };

            // Game Installation Section
            var lblGameInstall = new Label
            {
                Text = "Schedule I Game Path:",
                Location = new Point(20, 80),
                Size = new Size(150, 20),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            txtGameInstall = new TextBox
            {
                Location = new Point(20, 105),
                Size = new Size(500, 23),
                ReadOnly = true
            };

            btnBrowseGameInstall = new Button
            {
                Text = "Browse...",
                Location = new Point(530, 104),
                Size = new Size(80, 25)
            };

            // Managed Environment Section
            var lblManagedEnv = new Label
            {
                Text = "Managed Environment Path:",
                Location = new Point(20, 140),
                Size = new Size(150, 20),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            txtManagedEnv = new TextBox
            {
                Location = new Point(20, 165),
                Size = new Size(500, 23),
                ReadOnly = true
            };

            btnBrowseManagedEnv = new Button
            {
                Text = "Browse...",
                Location = new Point(530, 164),
                Size = new Size(80, 25)
            };

            // Current Branch Detection Section
            var lblCurrentBranch = new Label
            {
                Text = "Currently Installed Branch:",
                Location = new Point(20, 200),
                Size = new Size(150, 20),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            cboCurrentBranch = new ComboBox
            {
                Location = new Point(20, 225),
                Size = new Size(300, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Populate the branch dropdown
            cboCurrentBranch.Items.AddRange(DevEnvironmentConfig.AvailableBranches.ToArray());

            // Branch Selection Section
            var lblBranches = new Label
            {
                Text = "Select Branches for Managed Environment:",
                Location = new Point(20, 260),
                Size = new Size(200, 20),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            chkMainBranch = new CheckBox
            {
                Text = "Main",
                Location = new Point(20, 285),
                Size = new Size(120, 20)
            };

            chkBetaBranch = new CheckBox
            {
                Text = "Beta",
                Location = new Point(150, 285),
                Size = new Size(120, 20)
            };

            chkAlternateBranch = new CheckBox
            {
                Text = "Alternate",
                Location = new Point(280, 285),
                Size = new Size(120, 20)
            };

            chkAlternateBetaBranch = new CheckBox
            {
                Text = "Alternate Beta",
                Location = new Point(410, 285),
                Size = new Size(150, 20)
            };

            // Status and Progress
            lblStatus = new Label
            {
                Text = "Ready",
                Location = new Point(20, 330),
                Size = new Size(600, 20),
                ForeColor = Color.Green
            };

            // Configuration Info Display
            var lblConfigInfo = new Label
            {
                Text = "Configuration Information:",
                Location = new Point(20, 360),
                Size = new Size(200, 20),
                Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold)
            };

            var txtConfigInfo = new TextBox
            {
                Location = new Point(20, 385),
                Size = new Size(600, 60),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font(this.Font.FontFamily, 8)
            };

            // Store reference to config info textbox
            this.txtConfigInfo = txtConfigInfo;

            progressBar = new ProgressBar
            {
                Location = new Point(20, 455),
                Size = new Size(600, 23),
                Visible = false
            };

            // Action Buttons
            btnCreateEnvironment = new Button
            {
                Text = "Create Managed Environment",
                Location = new Point(20, 400),
                Size = new Size(200, 35),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),
                Enabled = false
            };

            btnRefresh = new Button
            {
                Text = "Refresh Steam Info",
                Location = new Point(240, 400),
                Size = new Size(150, 35)
            };

            btnExit = new Button
            {
                Text = "Exit",
                Location = new Point(410, 400),
                Size = new Size(80, 35)
            };

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblSteamLibrary, txtSteamLibrary, btnBrowseSteamLibrary,
                lblGameInstall, txtGameInstall, btnBrowseGameInstall,
                lblManagedEnv, txtManagedEnv, btnBrowseManagedEnv,
                lblCurrentBranch, cboCurrentBranch,
                lblBranches, chkMainBranch, chkBetaBranch, chkAlternateBranch, chkAlternateBetaBranch,
                lblStatus, progressBar, btnCreateEnvironment, btnRefresh, btnExit
            });
        }

        private void SetupEventHandlers()
        {
            btnBrowseSteamLibrary!.Click += BtnBrowseSteamLibrary_Click;
            btnBrowseGameInstall!.Click += BtnBrowseGameInstall_Click;
            btnBrowseManagedEnv!.Click += BtnBrowseManagedEnv_Click;
            btnCreateEnvironment!.Click += BtnCreateEnvironment_Click;
            btnRefresh!.Click += BtnRefresh_Click;
            btnExit!.Click += BtnExit_Click;

            // Branch selection change handlers
            chkMainBranch!.CheckedChanged += BranchSelection_Changed;
            chkBetaBranch!.CheckedChanged += BranchSelection_Changed;
            chkAlternateBranch!.CheckedChanged += BranchSelection_Changed;
            chkAlternateBetaBranch!.CheckedChanged += BranchSelection_Changed;
            
            // Current branch dropdown change handler
            cboCurrentBranch!.SelectedIndexChanged += CurrentBranch_SelectionChanged;
        }

        private void LoadSteamInformation()
        {
            try
            {
                _logger.LogInformation("Loading Steam information...");
                lblStatus!.Text = "Loading Steam information...";
                lblStatus!.ForeColor = Color.Blue;

                // Get Steam library paths
                var libraryPaths = _steamService.GetSteamLibraryPaths();
                if (libraryPaths.Count == 0)
                {
                    lblStatus!.Text = "No Steam libraries found. Please ensure Steam is installed.";
                    lblStatus!.ForeColor = Color.Red;
                    return;
                }

                // If multiple libraries found, show selection dialog
                if (libraryPaths.Count > 1)
                {
                    _logger.LogInformation("Multiple Steam libraries detected ({Count}), showing selection dialog", libraryPaths.Count);
                    ShowLibrarySelectionDialog();
                }
                else
                {
                    // Single library found, use it directly
                    _config.SteamLibraryPath = libraryPaths[0];
                    txtSteamLibrary!.Text = _config.SteamLibraryPath;
                    LoadGamesFromLibrary(_config.SteamLibraryPath);
                    
                    // Update configuration info display after loading Steam information
                    UpdateConfigurationInfoDisplay();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Steam information");
                lblStatus!.Text = $"Error: {ex.Message}";
                lblStatus!.ForeColor = Color.Red;
            }
        }

        private void ShowLibrarySelectionDialog()
        {
            try
            {
                lblStatus!.Text = "Multiple Steam libraries detected. Please select one...";
                lblStatus!.ForeColor = Color.Blue;

                using var dialog = new SteamLibrarySelectionDialog(_steamService, _logger);
                var result = dialog.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    var selectedLibrary = dialog.SelectedLibraryPath;
                    _logger.LogInformation("User selected Steam library: {Path}", selectedLibrary);
                    
                    _config.SteamLibraryPath = selectedLibrary;
                    txtSteamLibrary!.Text = selectedLibrary;
                    
                    lblStatus!.Text = "Loading games from selected library...";
                    LoadGamesFromLibrary(selectedLibrary);
                    
                    // Update configuration info display after library selection
                    UpdateConfigurationInfoDisplay();
                }
                else
                {
                    lblStatus!.Text = "Library selection cancelled. Please refresh to try again.";
                    lblStatus!.ForeColor = Color.Orange;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing library selection dialog");
                lblStatus!.Text = $"Error showing library selection: {ex.Message}";
                lblStatus!.ForeColor = Color.Red;
            }
        }

        private void LoadGamesFromLibrary(string libraryPath)
        {
            try
            {
                _availableGames = _steamService.GetSteamGames(libraryPath);
                var scheduleIGame = _availableGames.FirstOrDefault(g => _steamService.IsScheduleIGame(g));
                
                if (scheduleIGame != null)
                {
                    _config.GameInstallPath = scheduleIGame.InstallPath;
                    txtGameInstall!.Text = scheduleIGame.InstallPath;
                    lblStatus!.Text = $"Found Schedule I: {scheduleIGame.Name} (ID: {scheduleIGame.AppId})";
                    lblStatus!.ForeColor = Color.Green;
                    
                    // Detect the installed branch
                    DetectAndSetInstalledBranch(scheduleIGame.InstallPath);
                }
                else
                {
                    lblStatus!.Text = "Schedule I not found in this Steam library. Searching other libraries...";
                    lblStatus!.ForeColor = Color.Orange;
                    
                    // Search other libraries for Schedule I
                    SearchOtherLibrariesForScheduleI(libraryPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading games from library");
                lblStatus!.Text = $"Error loading games: {ex.Message}";
                lblStatus!.ForeColor = Color.Red;
            }
        }

        private void SearchOtherLibrariesForScheduleI(string currentLibraryPath)
        {
            try
            {
                _logger.LogInformation("Searching other Steam libraries for Schedule I");
                
                // Use the SteamService to find Schedule I in any library
                var scheduleIGame = _steamService.FindScheduleIGameInLibraries();
                
                if (scheduleIGame != null)
                {
                    // Found Schedule I in another library
                    _logger.LogInformation("Found Schedule I in different library: {Path}", scheduleIGame.LibraryPath);
                    
                    // Ask user if they want to switch to the library containing Schedule I
                    var result = MessageBox.Show(
                        $"Schedule I was found in a different Steam library:\n\n{scheduleIGame.LibraryPath}\n\nWould you like to switch to this library?",
                        "Schedule I Found in Different Library",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        // Switch to the library containing Schedule I
                        _config.SteamLibraryPath = scheduleIGame.LibraryPath;
                        txtSteamLibrary!.Text = scheduleIGame.LibraryPath;
                        
                        // Load the game information
                        _config.GameInstallPath = scheduleIGame.InstallPath;
                        txtGameInstall!.Text = scheduleIGame.InstallPath;
                        lblStatus!.Text = $"Found Schedule I: {scheduleIGame.Name} (ID: {scheduleIGame.AppId})";
                        lblStatus!.ForeColor = Color.Green;
                        
                        // Detect the installed branch
                        DetectAndSetInstalledBranch(scheduleIGame.InstallPath);
                    }
                    else
                    {
                        lblStatus!.Text = "Schedule I not found in selected library. Please install the game or select a different library.";
                        lblStatus!.ForeColor = Color.Orange;
                    }
                }
                else
                {
                    lblStatus!.Text = "Schedule I not found in any Steam library. Please install the game.";
                    lblStatus!.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching other libraries for Schedule I");
                lblStatus!.Text = $"Error searching libraries: {ex.Message}";
                lblStatus!.ForeColor = Color.Red;
            }
        }

        private void DetectAndSetInstalledBranch(string gameInstallPath)
        {
            try
            {
                _logger.LogInformation("Detecting installed branch for: {Path}", gameInstallPath);
                
                // Use the SteamService to detect the branch and build ID
                var (detectedBranch, buildId) = _steamService.GetBranchAndBuildIdFromManifest(gameInstallPath);
                
                if (!string.IsNullOrEmpty(detectedBranch))
                {
                    _config.InstalledBranch = detectedBranch;
                    
                    // Set the dropdown to the detected branch
                    if (cboCurrentBranch != null)
                    {
                        cboCurrentBranch.SelectedItem = detectedBranch;
                        _logger.LogInformation("Detected branch: {Branch}", detectedBranch);
                    }
                    
                    // Update build ID for the detected branch
                    if (!string.IsNullOrEmpty(buildId))
                    {
                        _config.SetBuildIdForBranch(detectedBranch, buildId);
                        _logger.LogInformation("Updated build ID for {Branch}: {BuildId}", detectedBranch, buildId);
                    }
                    
                    // Auto-select the detected branch in the checkboxes
                    AutoSelectDetectedBranch(detectedBranch);
                    
                    // Save configuration after detecting branch
                    SaveConfigurationAsync();
                }
                else
                {
                    _logger.LogWarning("Could not detect branch for: {Path}", gameInstallPath);
                    if (cboCurrentBranch != null)
                    {
                        cboCurrentBranch.SelectedIndex = -1; // Clear selection
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting installed branch");
                if (cboCurrentBranch != null)
                {
                    cboCurrentBranch.SelectedIndex = -1; // Clear selection
                }
            }
        }

        private void AutoSelectDetectedBranch(string detectedBranch)
        {
            // Clear all checkboxes first
            if (chkMainBranch != null) chkMainBranch.Checked = false;
            if (chkBetaBranch != null) chkBetaBranch.Checked = false;
            if (chkAlternateBranch != null) chkAlternateBranch.Checked = false;
            if (chkAlternateBetaBranch != null) chkAlternateBetaBranch.Checked = false;
            
            // Check the appropriate checkbox based on detected branch
            switch (detectedBranch)
            {
                case "main-branch":
                    if (chkMainBranch != null) chkMainBranch.Checked = true;
                    break;
                case "beta-branch":
                    if (chkBetaBranch != null) chkBetaBranch.Checked = true;
                    break;
                case "alternate-branch":
                    if (chkAlternateBranch != null) chkAlternateBranch.Checked = true;
                    break;
                case "alternate-beta-branch":
                    if (chkAlternateBetaBranch != null) chkAlternateBetaBranch.Checked = true;
                    break;
            }
            
            // Update the configuration
            UpdateSelectedBranches();
        }

        private void UpdateBranchCheckboxes()
        {
            // Update checkboxes based on loaded configuration
            if (chkMainBranch != null) chkMainBranch.Checked = _config.SelectedBranches.Contains("main-branch");
            if (chkBetaBranch != null) chkBetaBranch.Checked = _config.SelectedBranches.Contains("beta-branch");
            if (chkAlternateBranch != null) chkAlternateBranch.Checked = _config.SelectedBranches.Contains("alternate-branch");
            if (chkAlternateBetaBranch != null) chkAlternateBetaBranch.Checked = _config.SelectedBranches.Contains("alternate-beta-branch");
        }

        private async void SaveConfigurationAsync()
        {
            try
            {
                // Update selected branches from checkboxes
                _config.SelectedBranches.Clear();
                if (chkMainBranch?.Checked == true) _config.SelectedBranches.Add("main-branch");
                if (chkBetaBranch?.Checked == true) _config.SelectedBranches.Add("beta-branch");
                if (chkAlternateBranch?.Checked == true) _config.SelectedBranches.Add("alternate-branch");
                if (chkAlternateBetaBranch?.Checked == true) _config.SelectedBranches.Add("alternate-beta-branch");

                // Update configuration with current values
                _config.UpdateConfiguration(
                    txtSteamLibrary?.Text ?? string.Empty,
                    txtGameInstall?.Text ?? string.Empty,
                    txtManagedEnv?.Text ?? string.Empty,
                    _config.SelectedBranches
                );

                await _configService.SaveConfigurationAsync(_config);
                _logger.LogInformation("Configuration saved successfully");
                
                // Update the configuration info display
                UpdateConfigurationInfoDisplay();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration");
            }
        }

        private void UpdateConfigurationInfoDisplay()
        {
            try
            {
                if (txtConfigInfo == null) return;

                var configInfo = new System.Text.StringBuilder();
                configInfo.AppendLine($"Configuration Version: {_config.ConfigVersion}");
                configInfo.AppendLine($"Last Updated: {_config.LastUpdated:yyyy-MM-dd HH:mm:ss}");
                configInfo.AppendLine();
                
                if (!string.IsNullOrEmpty(_config.SteamLibraryPath))
                {
                    configInfo.AppendLine($"Steam Library: {_config.SteamLibraryPath}");
                }
                
                if (!string.IsNullOrEmpty(_config.GameInstallPath))
                {
                    configInfo.AppendLine($"Game Install: {_config.GameInstallPath}");
                }
                
                if (!string.IsNullOrEmpty(_config.ManagedEnvironmentPath))
                {
                    configInfo.AppendLine($"Managed Environment: {_config.ManagedEnvironmentPath}");
                }
                
                if (!string.IsNullOrEmpty(_config.InstalledBranch))
                {
                    configInfo.AppendLine($"Installed Branch: {_config.InstalledBranch}");
                }
                
                configInfo.AppendLine();
                configInfo.AppendLine("Branch Build IDs:");
                foreach (var branch in DevEnvironmentConfig.AvailableBranches)
                {
                    var buildId = _config.GetBuildIdForBranch(branch);
                    var status = _config.SelectedBranches.Contains(branch) ? "[SELECTED]" : "[NOT SELECTED]";
                    configInfo.AppendLine($"  {branch}: {buildId} {status}");
                }

                // Add logging information
                configInfo.AppendLine();
                configInfo.AppendLine("Logging Information:");
                try
                {
                    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var logDir = Path.Combine(appDataPath, "TVGS", "Schedule I", "Developer Env", "logs");
                    var configDir = Path.Combine(appDataPath, "TVGS", "Schedule I", "Developer Env", "config");
                    
                    configInfo.AppendLine($"Log Directory: {logDir}");
                    configInfo.AppendLine($"Config Directory: {configDir}");
                    
                    if (Directory.Exists(logDir))
                    {
                        var logFiles = Directory.GetFiles(logDir, "*.log");
                        configInfo.AppendLine($"Log Files: {logFiles.Length} found");
                        
                        if (logFiles.Length > 0)
                        {
                            var latestLog = logFiles.OrderByDescending(f => File.GetLastWriteTime(f)).First();
                            configInfo.AppendLine($"Latest Log: {Path.GetFileName(latestLog)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    configInfo.AppendLine($"Error getting logging info: {ex.Message}");
                }

                txtConfigInfo.Text = configInfo.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration info display");
            }
        }

        private void CurrentBranch_SelectionChanged(object? sender, EventArgs e)
        {
            if (cboCurrentBranch?.SelectedItem != null)
            {
                var selectedBranch = cboCurrentBranch.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedBranch))
                {
                    _config.InstalledBranch = selectedBranch;
                    _logger.LogInformation("User manually selected branch: {Branch}", selectedBranch);
                    
                    // Auto-select the manually selected branch in the checkboxes
                    AutoSelectDetectedBranch(selectedBranch);
                }
            }
        }

        private void BtnBrowseSteamLibrary_Click(object? sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Select Steam Library Folder",
                ShowNewFolderButton = false
            };

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                _config.SteamLibraryPath = folderDialog.SelectedPath;
                txtSteamLibrary!.Text = _config.SteamLibraryPath;
                LoadGamesFromLibrary(_config.SteamLibraryPath);
                
                // Save configuration after changing Steam library
                SaveConfigurationAsync();
            }
        }

        private void BtnBrowseGameInstall_Click(object? sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Select Schedule I Game Installation Folder",
                ShowNewFolderButton = false
            };

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                _config.GameInstallPath = folderDialog.SelectedPath;
                txtGameInstall!.Text = _config.GameInstallPath;
                
                // Detect the installed branch
                DetectAndSetInstalledBranch(folderDialog.SelectedPath);
                
                ValidateConfiguration();
                
                // Save configuration after changing game install path
                SaveConfigurationAsync();
            }
        }

        private void BtnBrowseManagedEnv_Click(object? sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Select Managed Environment Folder",
                ShowNewFolderButton = true
            };

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                _config.ManagedEnvironmentPath = folderDialog.SelectedPath;
                txtManagedEnv!.Text = _config.ManagedEnvironmentPath;
                ValidateConfiguration();
                
                // Save configuration after changing managed environment path
                SaveConfigurationAsync();
            }
        }

        private void BranchSelection_Changed(object? sender, EventArgs e)
        {
            UpdateSelectedBranches();
            ValidateConfiguration();
        }

        private void UpdateSelectedBranches()
        {
            _config.SelectedBranches.Clear();
            
            if (chkMainBranch!.Checked) _config.SelectedBranches.Add("main-branch");
            if (chkBetaBranch!.Checked) _config.SelectedBranches.Add("beta-branch");
            if (chkAlternateBranch!.Checked) _config.SelectedBranches.Add("alternate-branch");
            if (chkAlternateBetaBranch!.Checked) _config.SelectedBranches.Add("alternate-beta-branch");
            
            // Save configuration when branches change
            SaveConfigurationAsync();
        }

        private void ValidateConfiguration()
        {
            bool isValid = !string.IsNullOrEmpty(_config.GameInstallPath) &&
                          !string.IsNullOrEmpty(_config.ManagedEnvironmentPath) &&
                          _config.SelectedBranches.Count > 0;

            btnCreateEnvironment!.Enabled = isValid;
            
            if (isValid)
            {
                lblStatus!.Text = "Configuration is valid. Ready to create managed environment.";
                lblStatus!.ForeColor = Color.Green;
            }
            else
            {
                lblStatus!.Text = "Please complete all required fields and select at least one branch.";
                lblStatus!.ForeColor = Color.Orange;
            }
        }

        private async void BtnCreateEnvironment_Click(object? sender, EventArgs e)
        {
            try
            {
                btnCreateEnvironment!.Enabled = false;
                progressBar!.Visible = true;
                progressBar!.Value = 0;
                lblStatus!.Text = "Creating managed environment...";
                lblStatus!.ForeColor = Color.Blue;

                await Task.Run(() => CreateManagedEnvironment());

                lblStatus!.Text = "Managed environment created successfully!";
                lblStatus!.ForeColor = Color.Green;
                progressBar!.Value = 100;
                
                // Save final configuration after successful environment creation
                SaveConfigurationAsync();
                
                MessageBox.Show("Managed environment created successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                progressBar!.Visible = false;
            }
        }

        private void CreateManagedEnvironment()
        {
            try
            {
                // Create the main managed environment directory
                if (!Directory.Exists(_config.ManagedEnvironmentPath))
                {
                    Directory.CreateDirectory(_config.ManagedEnvironmentPath);
                }

                // Create branch directories
                foreach (var branch in _config.SelectedBranches)
                {
                    var branchPath = Path.Combine(_config.ManagedEnvironmentPath, branch);
                    if (!Directory.Exists(branchPath))
                    {
                        Directory.CreateDirectory(branchPath);
                    }

                    // Copy game files to branch directory (this is a simplified copy)
                    var sourceFiles = Directory.GetFiles(_config.GameInstallPath, "*.*", SearchOption.AllDirectories);
                    var totalFiles = sourceFiles.Length;
                    
                    for (int i = 0; i < totalFiles; i++)
                    {
                        var sourceFile = sourceFiles[i];
                        var relativePath = Path.GetRelativePath(_config.GameInstallPath, sourceFile);
                        var targetFile = Path.Combine(branchPath, relativePath);
                        var targetDir = Path.GetDirectoryName(targetFile);
                        
                        if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }
                        
                        File.Copy(sourceFile, targetFile, true);
                        
                        // Update progress
                        var progress = (int)((i + 1) * 100.0 / totalFiles);
                        this.Invoke(() => progressBar!.Value = progress);
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

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            try
            {
                _logger.LogInformation("Refreshing Steam information...");
                
                // Clear current game information
                txtGameInstall!.Text = "";
                cboCurrentBranch!.SelectedIndex = -1;
                _config.GameInstallPath = "";
                _config.InstalledBranch = "";
                
                LoadSteamInformation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing Steam information");
                lblStatus!.Text = $"Error refreshing: {ex.Message}";
                lblStatus!.ForeColor = Color.Red;
            }
        }

        private void BtnExit_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        // Control declarations - made nullable to fix warnings
        private TextBox? txtSteamLibrary;
        private Button? btnBrowseSteamLibrary;
        private TextBox? txtGameInstall;
        private Button? btnBrowseGameInstall;
        private TextBox? txtManagedEnv;
        private Button? btnBrowseManagedEnv;
        private ComboBox? cboCurrentBranch;
        private CheckBox? chkMainBranch;
        private CheckBox? chkBetaBranch;
        private CheckBox? chkAlternateBranch;
        private CheckBox? chkAlternateBetaBranch;
        private Label? lblStatus;
        private TextBox? txtConfigInfo;
        private ProgressBar? progressBar;
        private Button? btnCreateEnvironment;
        private Button? btnRefresh;
        private Button? btnExit;
    }
}
