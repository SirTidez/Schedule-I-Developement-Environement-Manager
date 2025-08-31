using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScheduleIDevelopementEnvironementManager.Models;
using ScheduleIDevelopementEnvironementManager.Services;
using ScheduleIDevelopementEnvironementManager.UI;
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
            
            // Initialize diagnostics system
            FormDiagnostics.Initialize(_logger);
            FormDiagnostics.LogFormInitialization("MainForm");
            
            InitializeModernForm();
            // Since Program.cs now handles configuration checking, MainForm is only shown when setup is needed
            ShowModernSetupUI();
            
            FormDiagnostics.LogFormLoadComplete("MainForm");
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
                    // Environment was created successfully, show success message and close
                    _logger.LogInformation("Managed environment created successfully, application will restart");
                    
                    // Show success message
                    MessageBox.Show("Environment created successfully!\n\nThe application will now close. Please restart it to open your managed environment.", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Close this form and let the application restart to load the managed environment
                    this.Close();
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

        private void ShowModernSetupUI()
        {
            FormDiagnostics.StartPerformanceTracking("ShowModernSetupUI");
            FormDiagnostics.LogUserInteraction("ShowSetupUI", "MainForm", "Displaying modern setup interface");
            
            // Clear existing controls
            this.Controls.Clear();
            
            // Set up the modern setup UI
            this.Text = "🚀 Schedule I Development Manager - Welcome";
            this.Size = new Size(800, 650);  // Increased to accommodate GitHub info card
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            CreateModernSetupLayout();
            SetupModernSetupEventHandlers();
            
            FormDiagnostics.EndPerformanceTracking("ShowModernSetupUI");
        }

        private void CreateModernSetupLayout()
        {
            // Main container - increased size for better spacing
            var mainPanel = new Panel();
            mainPanel.Size = new Size(750, 570);
            mainPanel.Location = new Point(10, 10);
            mainPanel.BackColor = ModernUITheme.Colors.BackgroundPrimary;

            // Header section - increased height for better text spacing
            var headerPanel = ModernControls.CreateSectionPanel("", new Size(750, 120));
            headerPanel.Location = new Point(0, 0);
            headerPanel.BackColor = ModernUITheme.Colors.BackgroundSecondary;

            var titleLabel = ModernControls.CreateHeadingLabel("🚀 Schedule I Development Manager", true);
            titleLabel.Location = new Point(15, 15);
            titleLabel.Size = new Size(720, 40);  // Much wider to prevent cutoff
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.ForeColor = ModernUITheme.Colors.AccentPrimary;

            var subtitleLabel = ModernControls.CreateStatusLabel("No development environment detected. Set up your environment to get started.", ModernUITheme.Colors.AccentWarning);
            subtitleLabel.Location = new Point(15, 65);
            subtitleLabel.Size = new Size(720, 35);  // Increased height for proper text wrapping
            subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;

            headerPanel.Controls.AddRange(new Control[] { titleLabel, subtitleLabel });

            // Project information card - custom card with proper title sizing
            var projectCard = new Panel();
            projectCard.Size = new Size(700, 300);  // Slightly taller to accommodate title properly
            projectCard.Location = new Point(25, 140);
            projectCard.BackColor = ModernUITheme.Colors.BackgroundTertiary;
            projectCard.BorderStyle = BorderStyle.FixedSingle;
            
            // Custom title label with proper sizing
            var cardTitleLabel = ModernControls.CreateHeadingLabel("Managed Environment Creation Wizard", false);
            cardTitleLabel.Location = new Point(15, 15);
            cardTitleLabel.Size = new Size(670, 30); // Full width minus margins
            cardTitleLabel.ForeColor = ModernUITheme.Colors.AccentPrimary;
            cardTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            
            // Version and author info - centered vertically with more space from title
            var versionLabel = ModernControls.CreateFieldLabel("🏷️ Version: 0.5.0 ALPHA");
            versionLabel.Location = new Point(15, 75);
            versionLabel.Size = new Size(320, 25);
            
            var authorLabel = ModernControls.CreateFieldLabel("👤 Author: Schedule I Development Team");
            authorLabel.Location = new Point(350, 75);
            authorLabel.Size = new Size(330, 25);
            
            // Description - centered vertically
            var descLabel = ModernControls.CreateStatusLabel(
                "🎯 Create isolated development environments for different game branches. " +
                "Safely mod and test different versions without affecting your main installation.",
                ModernUITheme.Colors.TextSecondary);
            descLabel.Location = new Point(15, 105);
            descLabel.Size = new Size(670, 50);
            
            // GitHub links - centered vertically
            var releasesLink = new LinkLabel();
            releasesLink.Text = "📋 View Releases";
            releasesLink.Location = new Point(15, 170);
            releasesLink.Size = new Size(150, 25);
            releasesLink.LinkColor = ModernUITheme.Colors.AccentPrimary;
            releasesLink.VisitedLinkColor = ModernUITheme.Colors.AccentPrimary;
            releasesLink.ActiveLinkColor = ModernUITheme.Colors.AccentSuccess;
            releasesLink.Font = ModernUITheme.Typography.BodyMedium;
            releasesLink.BackColor = Color.Transparent;
            releasesLink.LinkClicked += (s, e) => {
                try {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://github.com/SirTidez/Schedule-I-Developement-Environement-Manager/releases",
                        UseShellExecute = true
                    });
                } catch (Exception ex) {
                    _logger.LogError(ex, "Error opening releases link");
                }
            };
            
            var readmeLink = new LinkLabel();
            readmeLink.Text = "📚 View README";
            readmeLink.Location = new Point(180, 170);
            readmeLink.Size = new Size(150, 25);
            readmeLink.LinkColor = ModernUITheme.Colors.AccentPrimary;
            readmeLink.VisitedLinkColor = ModernUITheme.Colors.AccentPrimary;
            readmeLink.ActiveLinkColor = ModernUITheme.Colors.AccentSuccess;
            readmeLink.Font = ModernUITheme.Typography.BodyMedium;
            readmeLink.BackColor = Color.Transparent;
            readmeLink.LinkClicked += (s, e) => {
                try {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://github.com/SirTidez/Schedule-I-Developement-Environement-Manager/blob/main/README.md",
                        UseShellExecute = true
                    });
                } catch (Exception ex) {
                    _logger.LogError(ex, "Error opening documentation link");
                }
            };
            
            var issuesLink = new LinkLabel();
            issuesLink.Text = "🐛 Report Issues";
            issuesLink.Location = new Point(345, 170);
            issuesLink.Size = new Size(150, 25);
            issuesLink.LinkColor = ModernUITheme.Colors.AccentPrimary;
            issuesLink.VisitedLinkColor = ModernUITheme.Colors.AccentPrimary;
            issuesLink.ActiveLinkColor = ModernUITheme.Colors.AccentSuccess;
            issuesLink.Font = ModernUITheme.Typography.BodyMedium;
            issuesLink.BackColor = Color.Transparent;
            issuesLink.LinkClicked += (s, e) => {
                try {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://github.com/SirTidez/Schedule-I-Developement-Environement-Manager/issues",
                        UseShellExecute = true
                    });
                } catch (Exception ex) {
                    _logger.LogError(ex, "Error opening issues link");
                }
            };
            
            var repoLink = new LinkLabel();
            repoLink.Text = "🔗 View Source Code";
            repoLink.Location = new Point(510, 170);
            repoLink.Size = new Size(170, 25);
            repoLink.LinkColor = ModernUITheme.Colors.AccentPrimary;
            repoLink.VisitedLinkColor = ModernUITheme.Colors.AccentPrimary;
            repoLink.ActiveLinkColor = ModernUITheme.Colors.AccentSuccess;
            repoLink.Font = ModernUITheme.Typography.BodyMedium;
            repoLink.BackColor = Color.Transparent;
            repoLink.LinkClicked += (s, e) => {
                try {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://github.com/SirTidez/Schedule-I-Developement-Environement-Manager",
                        UseShellExecute = true
                    });
                } catch (Exception ex) {
                    _logger.LogError(ex, "Error opening repository link");
                }
            };
            
            // Additional project information - centered vertically
            var buildInfoLabel = ModernControls.CreateFieldLabel("🏗️ Built with: .NET 8.0 Windows Forms");
            buildInfoLabel.Location = new Point(15, 205);
            buildInfoLabel.Size = new Size(330, 25);
            
            var licenseLabel = ModernControls.CreateFieldLabel("📜 License: Open Source");
            licenseLabel.Location = new Point(350, 205);
            licenseLabel.Size = new Size(330, 25);
            
            var lastUpdatedLabel = ModernControls.CreateStatusLabel("🕒 Development Status: Active", ModernUITheme.Colors.AccentSuccess);
            lastUpdatedLabel.Location = new Point(15, 235);
            lastUpdatedLabel.Size = new Size(330, 25);
            
            var supportLabel = ModernControls.CreateStatusLabel("🤝 Community Support: Available via GitHub Issues", ModernUITheme.Colors.TextSecondary);
            supportLabel.Location = new Point(15, 260);
            supportLabel.Size = new Size(670, 20);
            
            projectCard.Controls.AddRange(new Control[] { 
                cardTitleLabel, versionLabel, authorLabel, descLabel, releasesLink, readmeLink, issuesLink, repoLink,
                buildInfoLabel, licenseLabel, lastUpdatedLabel, supportLabel
            });

            // Action buttons panel - moved closer to bottom with reduced height
            var actionPanel = new Panel();
            actionPanel.Size = new Size(750, 115);
            actionPanel.Location = new Point(0, 450);
            actionPanel.BackColor = ModernUITheme.Colors.BackgroundSecondary;

            btnCreateEnvironment = ModernControls.CreateActionButton("🔧 Setup New Environment", ModernUITheme.ButtonStyle.Primary);
            btnCreateEnvironment.Location = new Point(90, 20);  // Centered: 15+75px offset
            btnCreateEnvironment.Size = new Size(220, 50);      // Larger buttons

            var btnLoadConfig = ModernControls.CreateActionButton("📁 Load Configuration", ModernUITheme.ButtonStyle.Secondary);
            btnLoadConfig.Location = new Point(325, 20);        // Centered: 250+75px offset
            btnLoadConfig.Size = new Size(200, 50);

            btnExit = ModernControls.CreateActionButton("❌ Exit", ModernUITheme.ButtonStyle.Danger);
            btnExit.Location = new Point(540, 20);              // Centered: 465+75px offset
            btnExit.Size = new Size(120, 50);

            var helpLabel = ModernControls.CreateStatusLabel("💡 Tip: Choose 'Setup New Environment' if this is your first time using the tool.", ModernUITheme.Colors.TextMuted);
            helpLabel.Location = new Point(90, 75);  // Moved up 10px due to shorter panel
            helpLabel.Size = new Size(570, 40);      // Adjusted width: 720-150px for centering

            actionPanel.Controls.AddRange(new Control[] { btnCreateEnvironment, btnLoadConfig, btnExit, helpLabel });

            // Add all panels to main panel
            mainPanel.Controls.AddRange(new Control[] { headerPanel, projectCard, actionPanel });

            // Add main panel to form
            this.Controls.Add(mainPanel);

            FormDiagnostics.LogBulkThemeApplication("MainForm_Setup", 8, 8);
        }

        private void SetupModernSetupEventHandlers()
        {
            FormDiagnostics.LogUserInteraction("SetupEventHandlers", "MainForm_Setup");
            
            if (btnCreateEnvironment != null)
            {
                btnCreateEnvironment.Click += BtnSetupEnvironment_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "SetupEnvironmentButton");
            }
            
            if (btnExit != null)
            {
                btnExit.Click += BtnExit_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "ExitButton");
            }
            
            // Find the Load Configuration button and set up its event handler
            var btnLoadConfig = this.Controls.Cast<Control>()
                .SelectMany(c => c.Controls.Cast<Control>())
                .SelectMany(c => c.Controls.Cast<Control>())
                .OfType<Button>()
                .FirstOrDefault(b => b.Text.Contains("Load Configuration"));
            
            if (btnLoadConfig != null)
            {
                btnLoadConfig.Click += BtnLoadConfiguration_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "LoadConfigurationButton");
            }
        }

        private void BtnLoadConfiguration_Click(object? sender, EventArgs e)
        {
            FormDiagnostics.LogUserInteraction("LoadConfigurationClick", "MainForm", "User clicked Load Configuration");
            
            try
            {
                _logger.LogInformation("Load Configuration button clicked, showing configuration window");
                
                // Open file dialog to load configuration
                using var openFileDialog = new OpenFileDialog
                {
                    Title = "Load Development Environment Configuration",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true,
                    InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TVGS", "Schedule I", "Developer Env", "config")
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // TODO: Implement configuration loading from file
                    FormDiagnostics.LogUserInteraction("ConfigurationFileSelected", "MainForm", openFileDialog.FileName);
                    MessageBox.Show("Configuration loading will be implemented in a future update.", 
                                  "Feature Coming Soon", 
                                  MessageBoxButtons.OK, 
                                  MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                FormDiagnostics.LogUserInteraction("LoadConfigurationError", "MainForm", ex.Message);
                _logger.LogError(ex, "Error handling Load Configuration button click");
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Static helper method to load the application icon
        public static Icon? LoadApplicationIcon()
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

        private void InitializeModernForm()
        {
            FormDiagnostics.StartPerformanceTracking("MainForm_Initialization");
            
            this.Text = "🚀 Schedule I Development Manager - Environment Setup";
            this.Size = new Size(900, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.BackColor = ModernUITheme.Colors.BackgroundPrimary;
            
            // Load the icon using the helper method
            this.Icon = LoadApplicationIcon();
            
            FormDiagnostics.EndPerformanceTracking("MainForm_Initialization");
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
                    var buildInfo = _config.GetBuildInfoForBranch(branch);
                    var status = _config.SelectedBranches.Contains(branch) ? "[SELECTED]" : "[NOT SELECTED]";
                    
                    if (buildInfo != null && !string.IsNullOrEmpty(buildInfo.BuildId))
                    {
                        configInfo.AppendLine($"  {branch}: {buildInfo.BuildId} (Updated: {buildInfo.UpdatedTime:yyyy-MM-dd HH:mm:ss}) {status}");
                    }
                    else
                    {
                        configInfo.AppendLine($"  {branch}: [No Build ID] {status}");
                    }
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
                
                // Show success message and close application for restart
                MessageBox.Show("Environment created successfully!\n\nThe application will now close. Please restart it to open your managed environment.", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Close this form and let the application restart to load the managed environment
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

        private void BtnSetupEnvironment_Click(object? sender, EventArgs e)
        {
            FormDiagnostics.LogUserInteraction("SetupEnvironmentClick", "MainForm", "User clicked Setup Environment");
            
            try
            {
                _logger.LogInformation("Setup button clicked, switching to full configuration interface");
                FormDiagnostics.LogFormNavigation("MainForm_Setup", "MainForm_Configuration", "User initiated setup");
                
                SwitchToModernConfigurationUI();
                
                // Automatically start loading Steam information
                LoadSteamInformation();
            }
            catch (Exception ex)
            {
                FormDiagnostics.LogUserInteraction("SetupEnvironmentError", "MainForm", ex.Message);
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

                // Show warning popup before first copy operation
                var warningResult = MessageBox.Show(
                    "IMPORTANT NOTICE:\n\n" +
                    "The application may appear to freeze or become unresponsive during the directory creation phase of the copy operation. " +
                    "This is normal behavior due to the large size of the game files.\n\n" +
                    "Please be patient and DO NOT close the application during this process. " +
                    "The copy progress window will show detailed progress once the initial setup is complete.\n\n" +
                    "Do you want to continue with the managed environment creation?",
                    "Large File Copy Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (warningResult == DialogResult.No)
                {
                    _logger.LogInformation("User cancelled managed environment creation after warning");
                    return;
                }

                // First, copy the current branch (what the user already has installed)
                if (_config.SelectedBranches.Contains(currentBranch))
                {
                    _logger.LogInformation("Copying current branch first: {CurrentBranch}", currentBranch);
                    
                    using var currentBranchProgressForm = new CopyProgressForm();
                    currentBranchProgressForm.Show();

                    try
                    {
                        await CopyGameToBranchAsync(currentBranch, currentBranchProgressForm);
                        currentBranchProgressForm.SetCopyComplete();
                    }
                    finally
                    {
                        currentBranchProgressForm.Close();
                        currentBranchProgressForm.Dispose();
                    }
                    
                    _logger.LogInformation("Completed copying current branch: {Branch}", currentBranch);
                }

                // Now process other selected branches (excluding the current one)
                var otherBranches = _config.SelectedBranches.Where(branch => branch != currentBranch).ToList();
                
                if (otherBranches.Count == 0)
                {
                    _logger.LogInformation("No additional branches selected, finished after copying current branch");
                    return;
                }

                // Process each other branch
                for (int i = 0; i < otherBranches.Count; i++)
                {
                    var targetBranch = otherBranches[i];
                    _logger.LogInformation("Processing branch {Index}/{Total}: {Branch}", i + 1, otherBranches.Count, targetBranch);

                    // Show branch switch prompt BEFORE copying the branch
                    using var switchPrompt = new BranchSwitchPromptForm(currentBranch, targetBranch);
                    var result = switchPrompt.ShowDialog();
                    if (result == DialogResult.Cancel)
                    {
                        _logger.LogInformation("User cancelled branch switch operation");
                        break;
                    }

                    // Wait for user to actually switch the branch
                    var switchSuccess = await _steamService.WaitForBranchSwitchAsync(targetBranch, _config.GameInstallPath);
                    if (!switchSuccess)
                    {
                        throw new Exception($"Failed to detect branch switch to {targetBranch} within timeout period");
                    }

                    // Update current branch for next iteration
                    currentBranch = targetBranch;
                    _logger.LogInformation("Successfully switched to branch: {Branch}", currentBranch);

                    // Now copy the branch after switching to it
                    using var branchProgressForm = new CopyProgressForm();
                    branchProgressForm.Show();

                    try
                    {
                        await CopyGameToBranchAsync(targetBranch, branchProgressForm);
                        branchProgressForm.SetCopyComplete();
                    }
                    catch (Exception ex)
                    {
                        branchProgressForm.SetCopyFailed(ex.Message);
                        throw;
                    }
                    finally
                    {
                        branchProgressForm.Close();
                        branchProgressForm.Dispose();
                    }

                    _logger.LogInformation("Completed copying branch: {Branch}", targetBranch);
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
                
                // Switch to modern configuration UI first
                SwitchToModernConfigurationUI();
                
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

        private void SwitchToModernConfigurationUI()
        {
            FormDiagnostics.StartPerformanceTracking("SwitchToModernConfigurationUI");
            
            try
            {
                _logger.LogInformation("Switching from setup UI to modern configuration UI");
                
                // Clear setup controls and show modern configuration UI
                this.Controls.Clear();
                
                // Reset form properties for configuration mode
                this.Text = "🔧 Schedule I Development Manager - Environment Configuration";
                this.Size = new Size(1000, 850);  // Increased to accommodate larger layout
                
                // Create and show modern configuration UI
                CreateModernConfigurationLayout();
                SetupModernConfigurationEventHandlers();
                
                // Load the configuration
                LoadConfiguration();
                
                FormDiagnostics.LogBulkThemeApplication("MainForm_Configuration", 15, 15);
            }
            catch (Exception ex)
            {
                FormDiagnostics.LogUserInteraction("SwitchToConfigurationError", "MainForm", ex.Message);
                _logger.LogError(ex, "Error switching to modern configuration UI");
                MessageBox.Show($"Error switching to configuration UI: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                FormDiagnostics.EndPerformanceTracking("SwitchToModernConfigurationUI");
            }
        }

        private void CreateModernConfigurationLayout()
        {
            FormDiagnostics.StartPerformanceTracking("CreateModernConfigurationLayout");
            
            // Main container - increased size for better spacing
            var mainPanel = new Panel();
            mainPanel.Size = new Size(950, 750);
            mainPanel.Location = new Point(10, 10);
            mainPanel.BackColor = ModernUITheme.Colors.BackgroundPrimary;

            // Header section - increased size for better text display
            var headerPanel = ModernControls.CreateSectionPanel("", new Size(950, 100));
            headerPanel.Location = new Point(0, 0);
            headerPanel.BackColor = ModernUITheme.Colors.BackgroundSecondary;

            var titleLabel = ModernControls.CreateHeadingLabel("🔧 Development Environment Configuration", true);
            titleLabel.Location = new Point(15, 15);
            titleLabel.Size = new Size(920, 40);  // Much wider to prevent text cutoff
            titleLabel.ForeColor = ModernUITheme.Colors.AccentPrimary;

            var statusLabel = ModernControls.CreateStatusLabel("Configure your Steam integration and managed environment settings", ModernUITheme.Colors.TextSecondary);
            statusLabel.Location = new Point(15, 55);
            statusLabel.Size = new Size(920, 30);  // Increased height for better text display

            headerPanel.Controls.AddRange(new Control[] { titleLabel, statusLabel });

            // Steam Integration Card - increased size and better spacing
            var steamCard = ModernControls.CreateInfoCard("🎮 Steam Integration", "");
            steamCard.Size = new Size(450, 220);  // Larger with better internal spacing
            steamCard.Location = new Point(15, 120);

            var lblSteamLibrary = ModernControls.CreateFieldLabel("Steam Library Path:");
            lblSteamLibrary.Location = new Point(15, 45);
            lblSteamLibrary.Size = new Size(420, 25);  // Much wider for long labels

            txtSteamLibrary = ModernControls.CreateModernTextBox(true, "Steam library path will be auto-detected...");
            txtSteamLibrary.Location = new Point(15, 75);
            txtSteamLibrary.Size = new Size(350, 30);  // Wider textbox

            btnBrowseSteamLibrary = ModernControls.CreateActionButton("📁", ModernUITheme.ButtonStyle.Secondary);
            btnBrowseSteamLibrary.Location = new Point(375, 75);  // 10px spacing from textbox
            btnBrowseSteamLibrary.Size = new Size(50, 30);

            var lblGameInstall = ModernControls.CreateFieldLabel("Schedule I Game Path:");
            lblGameInstall.Location = new Point(15, 120);
            lblGameInstall.Size = new Size(420, 25);  // Wider for full text display

            txtGameInstall = ModernControls.CreateModernTextBox(true);
            txtGameInstall.Location = new Point(15, 150);
            txtGameInstall.Size = new Size(350, 30);  // Wider textbox

            btnBrowseGameInstall = ModernControls.CreateActionButton("📁", ModernUITheme.ButtonStyle.Secondary);
            btnBrowseGameInstall.Location = new Point(375, 150);  // 10px spacing
            btnBrowseGameInstall.Size = new Size(50, 30);

            steamCard.Controls.AddRange(new Control[] { lblSteamLibrary, txtSteamLibrary, btnBrowseSteamLibrary, lblGameInstall, txtGameInstall, btnBrowseGameInstall });

            // Environment Management Card - increased size and better spacing
            var envCard = ModernControls.CreateInfoCard("🗂️ Environment Management", "");
            envCard.Size = new Size(450, 220);  // Larger with better internal spacing
            envCard.Location = new Point(485, 120);

            var lblManagedEnv = ModernControls.CreateFieldLabel("Managed Environment Path:");
            lblManagedEnv.Location = new Point(15, 45);
            lblManagedEnv.Size = new Size(420, 25);  // Much wider for full text

            txtManagedEnv = ModernControls.CreateModernTextBox(false, "Choose where to store managed environments...");
            txtManagedEnv.Location = new Point(15, 75);
            txtManagedEnv.Size = new Size(350, 30);  // Wider textbox

            btnBrowseManagedEnv = ModernControls.CreateActionButton("📁", ModernUITheme.ButtonStyle.Secondary);
            btnBrowseManagedEnv.Location = new Point(375, 75);  // 10px spacing
            btnBrowseManagedEnv.Size = new Size(50, 30);

            var lblCurrentBranch = ModernControls.CreateFieldLabel("Current Branch:");
            lblCurrentBranch.Location = new Point(15, 120);
            lblCurrentBranch.Size = new Size(420, 25);  // Much wider

            cboCurrentBranch = new ComboBox();
            cboCurrentBranch.Location = new Point(15, 150);
            cboCurrentBranch.Size = new Size(350, 30);  // Wider combobox
            cboCurrentBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCurrentBranch.Font = ModernUITheme.Typography.BodyMedium;
            cboCurrentBranch.BackColor = ModernUITheme.Colors.BackgroundTertiary;
            cboCurrentBranch.ForeColor = ModernUITheme.Colors.TextPrimary;
            cboCurrentBranch.Items.AddRange(DevEnvironmentConfig.AvailableBranches.ToArray());

            envCard.Controls.AddRange(new Control[] { lblManagedEnv, txtManagedEnv, btnBrowseManagedEnv, lblCurrentBranch, cboCurrentBranch });

            // Branch Selection Card - improved size and spacing
            var branchCard = ModernControls.CreateInfoCard("🌿 Branch Selection", "Select which branches to manage in your development environment");
            branchCard.Size = new Size(920, 160);  // Wider and taller for better spacing
            branchCard.Location = new Point(15, 360);

            chkMainBranch = ModernControls.CreateModernCheckBox("📍 Main Branch (Stable Release)", false);
            chkMainBranch.Location = new Point(15, 55);
            chkMainBranch.Size = new Size(430, 25);  // Wider for full text display

            chkBetaBranch = ModernControls.CreateModernCheckBox("🧪 Beta Branch (Preview Updates)", false);
            chkBetaBranch.Location = new Point(465, 55);  // Better spacing between columns
            chkBetaBranch.Size = new Size(430, 25);

            chkAlternateBranch = ModernControls.CreateModernCheckBox("🔀 Alternate Branch (Experimental)", false);
            chkAlternateBranch.Location = new Point(15, 90);  // More vertical spacing
            chkAlternateBranch.Size = new Size(430, 25);

            chkAlternateBetaBranch = ModernControls.CreateModernCheckBox("⚡ Alternate Beta Branch (Cutting Edge)", false);
            chkAlternateBetaBranch.Location = new Point(465, 90);
            chkAlternateBetaBranch.Size = new Size(430, 25);

            branchCard.Controls.AddRange(new Control[] { chkMainBranch, chkBetaBranch, chkAlternateBranch, chkAlternateBetaBranch });

            // Status and Actions Card - improved size and spacing
            var statusCard = ModernControls.CreateInfoCard("📊 Status & Actions", "");
            statusCard.Size = new Size(920, 140);  // Larger for better button and text spacing
            statusCard.Location = new Point(15, 540);

            lblStatus = ModernControls.CreateStatusLabel("🔄 Loading configuration...", ModernUITheme.Colors.AccentInfo);
            lblStatus.Location = new Point(15, 45);
            lblStatus.Size = new Size(450, 30);  // Wider and taller for better text display

            txtConfigInfo = new RichTextBox();
            txtConfigInfo.Location = new Point(15, 80);
            txtConfigInfo.Size = new Size(450, 45);  // Wider and taller
            txtConfigInfo.ReadOnly = true;
            txtConfigInfo.BackColor = ModernUITheme.Colors.LogBackground;
            txtConfigInfo.ForeColor = ModernUITheme.Colors.TextSecondary;
            txtConfigInfo.Font = ModernUITheme.Typography.BodySmall;
            txtConfigInfo.BorderStyle = BorderStyle.None;

            btnCreateEnvironment = ModernControls.CreateActionButton("🚀 Create Environment", ModernUITheme.ButtonStyle.Success);
            btnCreateEnvironment.Location = new Point(485, 45);  // Better spacing from text area
            btnCreateEnvironment.Size = new Size(180, 45);  // Larger button
            btnCreateEnvironment.Enabled = false;

            btnRefresh = ModernControls.CreateActionButton("🔄 Refresh", ModernUITheme.ButtonStyle.Info);
            btnRefresh.Location = new Point(680, 45);  // 15px spacing between buttons
            btnRefresh.Size = new Size(110, 45);

            btnExit = ModernControls.CreateActionButton("❌ Exit", ModernUITheme.ButtonStyle.Danger);
            btnExit.Location = new Point(805, 45);  // Aligned with other buttons
            btnExit.Size = new Size(90, 45);       // Larger and aligned height

            progressBar = new ProgressBar();
            progressBar.Location = new Point(485, 100);  // Below the buttons
            progressBar.Size = new Size(405, 20);         // Spans across buttons area
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Visible = false;

            statusCard.Controls.AddRange(new Control[] { lblStatus, txtConfigInfo, btnCreateEnvironment, btnRefresh, btnExit, progressBar });

            // Add all cards to main panel
            mainPanel.Controls.AddRange(new Control[] { headerPanel, steamCard, envCard, branchCard, statusCard });

            // Add main panel to form
            this.Controls.Add(mainPanel);

            FormDiagnostics.EndPerformanceTracking("CreateModernConfigurationLayout");
        }

        private void SetupModernConfigurationEventHandlers()
        {
            FormDiagnostics.LogUserInteraction("SetupEventHandlers", "MainForm_Configuration");
            
            // Steam browsing buttons
            if (btnBrowseSteamLibrary != null)
            {
                btnBrowseSteamLibrary.Click += BtnBrowseSteamLibrary_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "BrowseSteamLibraryButton");
            }
            
            if (btnBrowseGameInstall != null)
            {
                btnBrowseGameInstall.Click += BtnBrowseGameInstall_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "BrowseGameInstallButton");
            }
            
            if (btnBrowseManagedEnv != null)
            {
                btnBrowseManagedEnv.Click += BtnBrowseManagedEnv_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "BrowseManagedEnvButton");
            }
            
            // Branch selection checkboxes
            if (chkMainBranch != null)
            {
                chkMainBranch.CheckedChanged += BranchSelection_Changed;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "MainBranchCheckbox");
            }
            
            if (chkBetaBranch != null)
            {
                chkBetaBranch.CheckedChanged += BranchSelection_Changed;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "BetaBranchCheckbox");
            }
            
            if (chkAlternateBranch != null)
            {
                chkAlternateBranch.CheckedChanged += BranchSelection_Changed;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "AlternateBranchCheckbox");
            }
            
            if (chkAlternateBetaBranch != null)
            {
                chkAlternateBetaBranch.CheckedChanged += BranchSelection_Changed;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "AlternateBetaBranchCheckbox");
            }
            
            // Current branch combo box
            if (cboCurrentBranch != null)
            {
                cboCurrentBranch.SelectedIndexChanged += CurrentBranch_SelectionChanged;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "CurrentBranchComboBox");
            }
            
            // Action buttons
            if (btnCreateEnvironment != null)
            {
                btnCreateEnvironment.Click += BtnCreateEnvironment_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "CreateEnvironmentButton");
            }
            
            if (btnRefresh != null)
            {
                btnRefresh.Click += BtnRefresh_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "RefreshButton");
            }
            
            if (btnExit != null)
            {
                btnExit.Click += BtnExit_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "ExitButton");
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
        private RichTextBox? txtConfigInfo;
        private ProgressBar? progressBar;
        private Button? btnCreateEnvironment;
        private Button? btnRefresh;
        private Button? btnExit;

    }
}
