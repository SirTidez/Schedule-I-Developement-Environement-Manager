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
            CheckManagedEnvironmentConfiguration();
        }

        private async void CheckManagedEnvironmentConfiguration()
        {
            try
            {
                _logger.LogInformation("Checking for managed environment configuration...");
                
                var loadedConfig = await _configService.LoadConfigurationAsync();
                if (loadedConfig != null && IsManagedEnvironmentConfigured(loadedConfig))
                {
                    // Managed environment exists, show the managed environment loaded form
                    _config = loadedConfig;
                    _logger.LogInformation("Managed environment configuration found, showing managed environment loaded form");
                    ShowManagedEnvironmentLoadedForm();
                }
                else
                {
                    // No managed environment, show setup UI
                    _logger.LogInformation("No managed environment configuration found, showing setup UI");
                    ShowSetupUI();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking managed environment configuration");
                ShowSetupUI();
            }
        }

        private bool IsManagedEnvironmentConfigured(DevEnvironmentConfig config)
        {
            // Check if we have the essential paths and at least one branch selected
            return !string.IsNullOrEmpty(config.ManagedEnvironmentPath) && 
                   !string.IsNullOrEmpty(config.GameInstallPath) &&
                   config.SelectedBranches.Count > 0;
        }

        private void ShowManagedEnvironmentLoadedForm()
        {
            try
            {
                _logger.LogInformation("Showing managed environment loaded form");
                
                // Create and show the managed environment loaded form
                using var managedEnvForm = new ManagedEnvironmentLoadedForm(_config);
                var result = managedEnvForm.ShowDialog();
                
                // After the form closes, check if we need to show setup UI
                if (result == DialogResult.Cancel || result == DialogResult.Abort)
                {
                    // User cancelled or closed the form, show setup UI
                    ShowSetupUI();
                }
                else
                {
                    // Form closed normally, exit the application
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing managed environment loaded form");
                MessageBox.Show($"Error showing managed environment loaded form: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowSetupUI();
            }
        }

        private void ShowCreateManagedEnvironmentForm()
        {
            try
            {
                _logger.LogInformation("Showing Create Managed Environment form");
                
                // Create and show the Create Managed Environment form
                using var createEnvForm = new CreateManagedEnvironmentForm();
                var result = createEnvForm.ShowDialog();
                
                if (result == DialogResult.OK)
                {
                    // Environment was created successfully, reload configuration and show managed environment loaded form
                    _logger.LogInformation("Managed environment created successfully, reloading configuration");
                    
                    // Reload configuration to get the newly created environment
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var newConfig = await _configService.LoadConfigurationAsync();
                            if (newConfig != null && IsManagedEnvironmentConfigured(newConfig))
                            {
                                _config = newConfig;
                                this.Invoke(() =>
                                {
                                    ShowManagedEnvironmentLoadedForm();
                                });
                            }
                            else
                            {
                                this.Invoke(() =>
                                {
                                    MessageBox.Show("Environment was created but configuration could not be loaded. Please restart the application.", 
                                        "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    ShowSetupUI();
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error reloading configuration after environment creation");
                            this.Invoke(() =>
                            {
                                MessageBox.Show($"Error reloading configuration: {ex.Message}. Please restart the application.", 
                                    "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ShowSetupUI();
                            });
                        }
                    });
                }
                else
                {
                    // User cancelled, stay on setup UI
                    _logger.LogInformation("User cancelled Create Managed Environment form");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing Create Managed Environment form");
                MessageBox.Show($"Error showing Create Managed Environment form: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowSetupUI()
        {
            // Clear existing controls
            this.Controls.Clear();
            
            // Set up the setup UI
            this.Text = "Schedule I Development Environment Manager - Setup";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            CreateSetupControls();
            SetupSetupEventHandlers();
        }

        private void CreateSetupControls()
        {
            // Title
            var lblTitle = new Label
            {
                Text = "No Development Environment Detected!",
                Location = new Point(50, 50),
                Size = new Size(500, 35), // Increased height from 30 to 35 to prevent text cutoff
                Font = new Font(this.Font.FontFamily, 16, FontStyle.Bold),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Description
            var lblDescription = new Label
            {
                Text = "Click 'Setup Environment' to configure your development environment, or 'Load Configuration' to use an existing configuration file.",
                Location = new Point(50, 100),
                Size = new Size(500, 40),
                Font = new Font(this.Font.FontFamily, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Create Environment Button
            btnCreateEnvironment = new Button
            {
                Text = "Setup Environment",
                Location = new Point(150, 180),
                Size = new Size(150, 40),
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold),
                BackColor = Color.LightGreen
            };

            // Load Configuration Button
            var btnLoadConfig = new Button
            {
                Text = "Load Configuration",
                Location = new Point(320, 180),
                Size = new Size(150, 40),
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold),
                BackColor = Color.LightBlue
            };

            // Exit Button
            btnExit = new Button
            {
                Text = "Exit",
                Location = new Point(250, 250),
                Size = new Size(100, 35)
            };

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblDescription, btnCreateEnvironment, btnLoadConfig, btnExit
            });
        }

        private void SetupSetupEventHandlers()
        {
            btnCreateEnvironment!.Click += BtnSetupEnvironment_Click;
            btnExit!.Click += BtnExit_Click;
            
            // Find the Load Configuration button and set up its event handler
            var btnLoadConfig = this.Controls.OfType<Button>().FirstOrDefault(b => b.Text == "Load Configuration");
            if (btnLoadConfig != null)
            {
                btnLoadConfig.Click += BtnLoadConfiguration_Click;
            }
        }

        private void BtnLoadConfiguration_Click(object? sender, EventArgs e)
        {
            try
            {
                _logger.LogInformation("Load Configuration button clicked, showing configuration window");
                
                // TODO: Show the configuration window (placeholder for now)
                MessageBox.Show("Configuration window functionality will be implemented later.", 
                              "Coming Soon", 
                              MessageBoxButtons.OK, 
                              MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Load Configuration button click");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Static helper method to load the application icon
        public static Icon LoadApplicationIcon()
        {
            try
            {
                var assembly = typeof(MainForm).Assembly;
                var resourceNames = assembly.GetManifestResourceNames();
                
                // Try to find PNG icon first
                var pngResourceName = resourceNames.FirstOrDefault(name => name.Contains(".png"));
                if (pngResourceName != null)
                {
                    using var pngStream = assembly.GetManifestResourceStream(pngResourceName);
                    if (pngStream != null)
                    {
                        // Convert PNG to Icon using Bitmap
                        using var bitmap = new Bitmap(pngStream);
                        return Icon.FromHandle(bitmap.GetHicon());
                    }
                }
                else
                {
                    // Fallback to ICO file
                    var iconResourceName = resourceNames.FirstOrDefault(name => name.Contains(".ico"));
                    if (iconResourceName != null)
                    {
                        using var iconStream = assembly.GetManifestResourceStream(iconResourceName);
                        if (iconStream != null)
                        {
                            return new Icon(iconStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load icon: {ex.Message}");
            }
            
            return null;
        }

        private void InitializeForm()
        {
            this.Text = "Schedule I Development Environment Manager";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            
            // Load the icon using the helper method
            this.Icon = LoadApplicationIcon();

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
                    
                    // Validate configuration after loading
                    ValidateConfiguration();
                }
                else
                {
                    _logger.LogInformation("No existing configuration found, using defaults");
                    UpdateConfigurationInfoDisplay();
                    
                    // Validate configuration even with defaults
                    ValidateConfiguration();
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
                Size = new Size(180, 25), // Increased width from 150 to 180 and height from 20 to 25
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            txtSteamLibrary = new TextBox
            {
                Location = new Point(20, 50), // Moved down from 45 to 50 for better spacing
                Size = new Size(500, 23),
                ReadOnly = true,
                Text = "C:\\Program Files (x86)\\Steam\\steamapps" // Default placeholder
            };

            btnBrowseSteamLibrary = new Button
            {
                Text = "Browse...",
                Location = new Point(530, 49), // Moved down from 44 to 49 to align with textbox
                Size = new Size(80, 25)
            };

            // Game Installation Section
            var lblGameInstall = new Label
            {
                Text = "Schedule I Game Path:",
                Location = new Point(20, 80),
                Size = new Size(180, 25), // Increased width and height for better text display
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            txtGameInstall = new TextBox
            {
                Location = new Point(20, 110), // Moved down from 105 to 110 for better spacing
                Size = new Size(500, 23),
                ReadOnly = true
            };

            btnBrowseGameInstall = new Button
            {
                Text = "Browse...",
                Location = new Point(530, 109), // Moved down from 104 to 109 to align with textbox
                Size = new Size(80, 25)
            };

            // Managed Environment Section
            var lblManagedEnv = new Label
            {
                Text = "Managed Environment Path:",
                Location = new Point(20, 140),
                Size = new Size(200, 25), // Increased width from 150 to 200 and height from 20 to 25
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            txtManagedEnv = new TextBox
            {
                Location = new Point(20, 170), // Moved down from 165 to 170 for better spacing
                Size = new Size(500, 23),
                ReadOnly = true
            };

            btnBrowseManagedEnv = new Button
            {
                Text = "Browse...",
                Location = new Point(530, 169), // Moved down from 164 to 169 to align with textbox
                Size = new Size(80, 25)
            };

            // Current Branch Detection Section
            var lblCurrentBranch = new Label
            {
                Text = "Currently Installed Branch:",
                Location = new Point(20, 200),
                Size = new Size(200, 25), // Increased width and height for better text display
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            cboCurrentBranch = new ComboBox
            {
                Location = new Point(20, 230), // Moved down from 225 to 230 for better spacing
                Size = new Size(300, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Populate the branch dropdown
            cboCurrentBranch.Items.AddRange(DevEnvironmentConfig.AvailableBranches.ToArray());

            // Branch Selection Section
            var lblBranches = new Label
            {
                Text = "Select Branches to Manage:",
                Location = new Point(20, 260),
                Size = new Size(220, 25), // Increased width from 200 to 220 and height from 20 to 25
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            chkMainBranch = new CheckBox
            {
                Text = "Main",
                Location = new Point(20, 290), // Moved down from 285 to 290 for better spacing
                Size = new Size(120, 20)
            };

            chkBetaBranch = new CheckBox
            {
                Text = "Beta",
                Location = new Point(150, 290), // Moved down from 285 to 290 for better spacing
                Size = new Size(120, 20)
            };

            chkAlternateBranch = new CheckBox
            {
                Text = "Alternate",
                Location = new Point(280, 290), // Moved down from 285 to 290 for better spacing
                Size = new Size(120, 20)
            };

            chkAlternateBetaBranch = new CheckBox
            {
                Text = "Alternate Beta",
                Location = new Point(410, 290), // Moved down from 285 to 290 for better spacing
                Size = new Size(150, 20)
            };

            // Status and Progress
            lblStatus = new Label
            {
                Text = "Ready",
                Location = new Point(20, 325), // Moved up from 330 to 325 to maintain good spacing
                Size = new Size(600, 25), // Increased height from 20 to 25 for better text display
                ForeColor = Color.Green
            };

            // Configuration Info Display
            var lblConfigInfo = new Label
            {
                Text = "Configuration Information:",
                Location = new Point(20, 360),
                Size = new Size(200, 25), // Increased height from 20 to 25 for better text display
                Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold)
            };

            var txtConfigInfo = new TextBox
            {
                Location = new Point(20, 390), // Moved down from 385 to 390 for better spacing
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
                Location = new Point(20, 460), // Moved down from 455 to 460 for better spacing
                Size = new Size(600, 23),
                Visible = false
            };

            // Action Buttons
            btnCreateEnvironment = new Button
            {
                Text = "Create Environment",
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
                    
                    // Validate configuration after loading Steam information
                    ValidateConfiguration();
                    
                    // Update status to guide user
                    lblStatus!.Text = "Steam information loaded! Now please browse for a Managed Environment Path.";
                    lblStatus!.ForeColor = Color.Blue;
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
                    
                    // Validate configuration after library selection
                    ValidateConfiguration();
                    
                    // Update status to guide user
                    lblStatus!.Text = "Library selected! Now please browse for a Managed Environment Path.";
                    lblStatus!.ForeColor = Color.Blue;
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
                    
                    // Update status to guide user
                    lblStatus!.Text = "Game and branch detected! Now please browse for a Managed Environment Path.";
                    lblStatus!.ForeColor = Color.Blue;
                    
                    // Validate configuration after detecting game and branch
                    ValidateConfiguration();
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
                        
                        // Update status to guide user
                        lblStatus!.Text = "Game and branch detected! Now please browse for a Managed Environment Path.";
                        lblStatus!.ForeColor = Color.Blue;
                        
                        // Validate configuration after detecting game and branch
                        ValidateConfiguration();
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
            
            // Validate configuration after auto-selecting branch
            ValidateConfiguration();
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
                
                // Update status to show progress
                lblStatus!.Text = "Managed Environment Path selected! Configuration should now be valid.";
                lblStatus!.ForeColor = Color.Blue;
                
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
                var missingItems = new List<string>();
                
                if (string.IsNullOrEmpty(_config.GameInstallPath))
                    missingItems.Add("Game Install Path");
                    
                if (string.IsNullOrEmpty(_config.ManagedEnvironmentPath))
                    missingItems.Add("Managed Environment Path");
                    
                if (_config.SelectedBranches.Count == 0)
                    missingItems.Add("Branch Selection");
                
                var missingText = string.Join(", ", missingItems);
                lblStatus!.Text = $"Please complete: {missingText}";
                lblStatus!.ForeColor = Color.Orange;
            }
        }

        private async void BtnCreateEnvironment_Click(object? sender, EventArgs e)
        {
            // Check if we're in setup mode (no status label)
            bool isSetupMode = lblStatus == null;
            
            if (isSetupMode)
            {
                // In setup mode, show the Create Managed Environment form
                try
                {
                    _logger.LogInformation("Setup mode: showing Create Managed Environment form");
                    ShowCreateManagedEnvironmentForm();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error showing Create Managed Environment form");
                    MessageBox.Show($"Error showing Create Managed Environment form: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            
            try
            {
                btnCreateEnvironment!.Enabled = false;
                lblStatus!.Text = "Creating managed environment...";
                lblStatus!.ForeColor = Color.Blue;

                await CreateManagedEnvironmentWithProgressAsync();

                lblStatus!.Text = "Managed environment created successfully!";
                lblStatus!.ForeColor = Color.Green;
                
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
            }
        }

        private void BtnSetupEnvironment_Click(object? sender, EventArgs e)
        {
            try
            {
                _logger.LogInformation("Setup button clicked, switching to full configuration interface");
                SwitchToNormalUI();
                
                // Automatically start loading Steam information
                LoadSteamInformation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching to configuration interface");
                MessageBox.Show($"Error switching to configuration interface: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CreateManagedEnvironmentWithProgressAsync()
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
                int branchIndex = 0;
                // Start processing the current branch first, then move to selected branches
                var progressForm = new CopyProgressForm();
                progressForm.Show();

                try
                {
                    await CopyGameToBranchAsync(currentBranch, progressForm);
                    progressForm.SetCopyComplete();
                }
                finally
                {
                    progressForm.Close();
                    progressForm.Dispose();
                }
                
                if (_config.SelectedBranches.Count == 1)
                {
                    _logger.LogInformation("No additional branches selected, finished after copying current branch");
                    return;
                }

                foreach (var branch in _config.SelectedBranches)
                {
                    if (branch == currentBranch)
                    {
                        //_logger.LogInformation("Skipping {Branch} - already current branch", branch);
                        branchIndex++;
                        continue;
                    }
                    _logger.LogInformation("Selected branch for environment: {Branch}", branch);

                    // Show branch switch prompt BEFORE copying the branch
                    using var switchPrompt = new BranchSwitchPromptForm(currentBranch, branch);
                    var result = switchPrompt.ShowDialog();
                    if (result == DialogResult.Cancel)
                    {
                        _logger.LogInformation("User cancelled branch switch operation");
                        break;
                    }

                    // Wait for user to actually switch the branch
                    var switchSuccess = await _steamService.WaitForBranchSwitchAsync(branch, _config.GameInstallPath);
                    if (!switchSuccess)
                    {
                        throw new Exception($"Failed to detect branch switch to {branch} within timeout period");
                    }

                    // Update current branch for next iteration
                    currentBranch = branch;
                    _logger.LogInformation("Successfully switched to branch: {Branch}", currentBranch);

                    // Now copy the branch after switching to it
                    var branchProgressForm = new CopyProgressForm();
                    branchProgressForm.Show();
                    
                    try
                    {
                        await CopyGameToBranchAsync(branch, branchProgressForm);
                        branchProgressForm.SetCopyComplete();
                    }
                    catch (Exception ex)
                    {
                        // Use the branch-specific progress form for error reporting
                        if (!branchProgressForm.IsDisposed)
                            branchProgressForm.SetCopyFailed(ex.Message);
                        throw;
                    }
                    finally
                    {
                        branchProgressForm.Close();
                        branchProgressForm.Dispose();
                    }
                    
                    branchIndex++;
                }

                // Process each selected branch
                //for (int i = 0; i < _config.SelectedBranches.Count; i++)
                //{
                //    var targetBranch = _config.SelectedBranches[i];
                    
                //    // Skip if this branch is already the current one
                //    //if (targetBranch == currentBranch)
                //    //{
                //    //    _logger.LogInformation("Skipping {Branch} - already current branch", targetBranch);
                //    //    continue;
                //    //}

                //    // Show progress form for this branch
                //    using var progressForm = new CopyProgressForm();
                //    progressForm.Show();
                    
                //    try
                //    {
                //        // Copy current game state to target branch folder
                //        await CopyGameToBranchAsync(targetBranch, progressForm);
                        
                //        // Close progress form
                //        progressForm.SetCopyComplete();
                //        progressForm.Close();
                        
                //        // If this isn't the last branch, prompt user to switch
                //        if (i < _config.SelectedBranches.Count - 1)
                //        {
                //            var nextBranch = _config.SelectedBranches[i + 1];
                            
                //            // Show branch switch prompt
                //            using var switchPrompt = new BranchSwitchPromptForm(currentBranch, nextBranch);
                //            var result = switchPrompt.ShowDialog();
                            
                //            if (result == DialogResult.Cancel)
                //            {
                //                _logger.LogInformation("User cancelled branch switch operation");
                //                break;
                //            }
                            
                //            // Wait for user to actually switch the branch
                //            var switchSuccess = await _steamService.WaitForBranchSwitchAsync(nextBranch, _config.GameInstallPath);
                            
                //            if (!switchSuccess)
                //            {
                //                throw new Exception($"Failed to detect branch switch to {nextBranch} within timeout period");
                //            }
                            
                //            // Update current branch for next iteration
                //            currentBranch = nextBranch;
                //            _logger.LogInformation("Successfully switched to branch: {Branch}", currentBranch);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        progressForm.SetCopyFailed(ex.Message);
                //        progressForm.Close();
                //        throw;
                //    }
                //}

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
                // Check if progress form is still valid
                if (progressForm == null || progressForm.IsDisposed)
                {
                    _logger.LogWarning("Progress form is null or disposed, cannot update UI");
                    return;
                }
                
                progressForm.UpdateStatus($"Copying game files to {branchName}...");
                progressForm.LogMessage($"Starting copy operation to {branchName}");

                var branchPath = Path.Combine(_config.ManagedEnvironmentPath, branchName);
                
                // Create branch directory if it doesn't exist
                if (!Directory.Exists(branchPath))
                {
                    Directory.CreateDirectory(branchPath);
                    if (!progressForm.IsDisposed)
                        progressForm.LogMessage($"Created directory: {branchPath}");
                }

                // Get all source files
                var sourceFiles = Directory.GetFiles(_config.GameInstallPath, "*.*", SearchOption.AllDirectories);
                var totalFiles = sourceFiles.Length;
                
                if (!progressForm.IsDisposed)
                    progressForm.LogMessage($"Found {totalFiles} files to copy");
                
                // Copy files with progress updates
                for (int i = 0; i < totalFiles; i++)
                {
                    var sourceFile = sourceFiles[i];
                    var relativePath = Path.GetRelativePath(_config.GameInstallPath, sourceFile);
                    var targetFile = Path.Combine(branchPath, relativePath);
                    var targetDir = Path.GetDirectoryName(targetFile);
                    
                    // Create target directory if needed
                    if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                        if (!progressForm.IsDisposed)
                            progressForm.LogMessage($"Created directory: {targetDir}");
                    }
                    
                    // Copy file
                    File.Copy(sourceFile, targetFile, true);
                    
                    // Update progress
                    var progress = (int)((i + 1) * 100.0 / totalFiles);
                    if (!progressForm.IsDisposed)
                        progressForm.UpdateProgress(progress);
                    
                    // Log every 100 files or for the last file
                    if ((i + 1) % 100 == 0 || i == totalFiles - 1)
                    {
                        if (!progressForm.IsDisposed)
                            progressForm.LogMessage($"Copied {i + 1}/{totalFiles} files ({progress}%)");
                    }
                    
                    // Small delay to allow UI updates
                    await Task.Delay(1);
                }
                
                if (!progressForm.IsDisposed)
                {
                    progressForm.LogMessage($"Successfully copied {totalFiles} files to {branchName}");
                    progressForm.UpdateStatus($"Copy to {branchName} completed successfully!");
                }
            }
            catch (Exception ex)
            {
                if (!progressForm.IsDisposed)
                    progressForm.LogMessage($"Error copying to {branchName}: {ex.Message}");
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

        private void StartSetupProcess()
        {
            try
            {
                _logger.LogInformation("Starting setup process: switching to normal UI for Steam library selection");
                
                // Switch to normal UI first
                SwitchToNormalUI();
                
                // Now start the Steam library selection process
                _logger.LogInformation("Setup process: initiating Steam library selection");
                LoadSteamInformation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in setup process");
                MessageBox.Show($"Error in setup process: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SwitchToNormalUI()
        {
            try
            {
                _logger.LogInformation("Switching from setup UI to normal UI");
                
                // Clear setup controls and show normal UI
                this.Controls.Clear();
                
                // Reset form properties
                this.Text = "Schedule I Development Environment Manager";
                this.Size = new Size(800, 700);
                
                // Create and show normal UI
                CreateControls();
                SetupEventHandlers();
                
                // Load the configuration (but not Steam information yet - that will be handled by StartSetupProcess)
                LoadConfiguration();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching to normal UI");
                MessageBox.Show($"Error switching to normal UI: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
