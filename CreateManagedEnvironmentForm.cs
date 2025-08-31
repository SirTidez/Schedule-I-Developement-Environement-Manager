using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScheduleIDevelopementEnvironementManager.Models;
using ScheduleIDevelopementEnvironementManager.Services;
using ScheduleIDevelopementEnvironementManager.UI;
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

        // Wizard State Management
        private int _currentStep = 1;
        private const int _totalSteps = 4;
        
        // UI Controls
        private Panel? _mainPanel;
        private Panel? _headerPanel;
        private Panel? _contentPanel;
        private Panel? _navigationPanel;
        private Label? _stepLabel;
        private Label? _titleLabel;
        private Label? _descriptionLabel;
        private Button? _btnNext;
        private Button? _btnPrevious;
        private Button? _btnCancel;
        
        // Legacy button references for compatibility
        private Button? btnCreateEnvironment;
        private Button? btnCancel;
        
        // Step-specific controls
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
            
            // Initialize diagnostics system
            FormDiagnostics.Initialize(_logger);
            FormDiagnostics.LogFormInitialization("CreateManagedEnvironmentForm");
            
            _config = new DevEnvironmentConfig();
            _availableGames = new List<SteamGameInfo>();
            
            InitializeModernWizardForm();
            LoadSteamInformationForWizard(); // Load Steam data synchronously
            
            FormDiagnostics.LogFormLoadComplete("CreateManagedEnvironmentForm");
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

            // Apply dark theme
            ApplyDarkTheme();

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
                Location = new Point(530, 55), // Aligned with textbox top
                Size = new Size(80, 23), // Match textbox height for perfect alignment
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Apply dark theme to controls
            ApplyDarkThemeToControl(lblSteamLibrary);
            ApplyDarkThemeToControl(txtSteamLibrary);
            ApplyDarkThemeToControl(btnBrowseSteamLibrary);

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
                Location = new Point(530, 125), // Aligned with textbox top
                Size = new Size(80, 23), // Match textbox height for perfect alignment
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Apply dark theme to controls
            ApplyDarkThemeToControl(lblGameInstall);
            ApplyDarkThemeToControl(txtGameInstall);
            ApplyDarkThemeToControl(btnBrowseGameInstall);

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
                Location = new Point(530, 195), // Aligned with textbox top
                Size = new Size(80, 23), // Match textbox height for perfect alignment
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Apply dark theme to controls
            ApplyDarkThemeToControl(lblManagedEnv);
            ApplyDarkThemeToControl(txtManagedEnv);
            ApplyDarkThemeToControl(btnBrowseManagedEnv);

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

            // Apply dark theme to controls
            ApplyDarkThemeToControl(lblBranches);
            ApplyDarkThemeToControl(chkMainBranch);
            ApplyDarkThemeToControl(chkBetaBranch);
            ApplyDarkThemeToControl(chkAlternateBranch);
            ApplyDarkThemeToControl(chkAlternateBetaBranch);

            // Status Label
            lblStatus = new Label
            {
                Text = "Ready to configure managed environment",
                Location = new Point(20, 315),
                Size = new Size(600, 25),
                Font = new Font(this.Font.FontFamily, 9),
                ForeColor = Color.Blue
            };

            // Apply dark theme to controls
            ApplyDarkThemeToControl(lblStatus);

            // Buttons
            btnCreateEnvironment = new Button
            {
                Text = "Create Managed Environment",
                Location = new Point(200, 360),
                Size = new Size(200, 45),
                Font = new Font(this.Font.FontFamily, 12, FontStyle.Bold),
                Enabled = false
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(420, 360),
                Size = new Size(120, 45)
            };

            // Apply dark theme to controls
            ApplyDarkThemeToControl(btnCreateEnvironment);
            ApplyDarkThemeToControl(btnCancel);

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

        #region Dark Theme Methods

        /// <summary>
        /// Applies dark theme to the form
        /// </summary>
        private void ApplyDarkTheme()
        {
            // Set form background to dark gray
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
        }

        /// <summary>
        /// Applies dark theme to individual controls
        /// </summary>
        /// <param name="control">The control to apply dark theme to</param>
        private void ApplyDarkThemeToControl(Control? control)
        {
            if (control == null) return;

            // Apply dark theme based on control type
            switch (control)
            {
                case Form form:
                    form.BackColor = Color.FromArgb(45, 45, 48);
                    form.ForeColor = Color.White;
                    break;

                case Label label:
                    label.BackColor = Color.Transparent;
                    label.ForeColor = Color.White;
                    break;

                case TextBox textBox:
                    textBox.BackColor = Color.FromArgb(30, 30, 30);
                    textBox.ForeColor = Color.White;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case Button button:
                    button.BackColor = Color.FromArgb(0, 122, 204); // Professional blue
                    button.ForeColor = Color.White;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = Color.FromArgb(0, 100, 180);
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 140, 230);
                    button.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 100, 180);
                    break;

                case ComboBox comboBox:
                    comboBox.BackColor = Color.FromArgb(30, 30, 30);
                    comboBox.ForeColor = Color.White;
                    comboBox.FlatStyle = FlatStyle.Flat;
                    break;

                case CheckBox checkBox:
                    checkBox.BackColor = Color.Transparent;
                    checkBox.ForeColor = Color.White;
                    break;

                case ProgressBar progressBar:
                    progressBar.BackColor = Color.FromArgb(30, 30, 30);
                    progressBar.ForeColor = Color.FromArgb(0, 122, 204);
                    break;

                case RichTextBox richTextBox:
                    richTextBox.BackColor = Color.FromArgb(30, 30, 30);
                    richTextBox.ForeColor = Color.White;
                    richTextBox.BorderStyle = BorderStyle.FixedSingle;
                    break;
            }

            // Recursively apply to child controls
            foreach (Control childControl in control.Controls)
            {
                ApplyDarkThemeToControl(childControl);
            }
        }

        #endregion

        #region Wizard Implementation Methods

        private void InitializeModernWizardForm()
        {
            FormDiagnostics.StartPerformanceTracking("WizardForm_Initialization");
            
            this.Text = "🧙‍♂️ Environment Setup Wizard - Schedule I Development Manager";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = ModernUITheme.Colors.BackgroundPrimary;
            
            this.Icon = MainForm.LoadApplicationIcon();

            CreateWizardLayout();
            SetupWizardEventHandlers();
            ShowStep(_currentStep);
            
            FormDiagnostics.EndPerformanceTracking("WizardForm_Initialization");
        }

        private void CreateWizardLayout()
        {
            FormDiagnostics.LogUserInteraction("CreateWizardLayout", "CreateManagedEnvironmentForm");
            
            _mainPanel = new Panel();
            _mainPanel.Size = new Size(950, 650);
            _mainPanel.Location = new Point(25, 25);
            _mainPanel.BackColor = ModernUITheme.Colors.BackgroundPrimary;

            _headerPanel = ModernControls.CreateSectionPanel("", new Size(950, 100));
            _headerPanel.Location = new Point(0, 0);
            _headerPanel.BackColor = ModernUITheme.Colors.BackgroundSecondary;

            _stepLabel = ModernControls.CreateHeadingLabel("Step 1 of 4", false);
            _stepLabel.Location = new Point(15, 15);
            _stepLabel.Size = new Size(200, 25);
            _stepLabel.ForeColor = ModernUITheme.Colors.AccentPrimary;

            _titleLabel = ModernControls.CreateHeadingLabel("Steam Library Detection", true);
            _titleLabel.Location = new Point(15, 45);
            _titleLabel.Size = new Size(650, 30);

            _descriptionLabel = ModernControls.CreateStatusLabel("Detecting your Steam installation and libraries...", ModernUITheme.Colors.TextSecondary);
            _descriptionLabel.Location = new Point(15, 75);
            _descriptionLabel.Size = new Size(920, 20);

            _headerPanel.Controls.AddRange(new Control[] { _stepLabel, _titleLabel, _descriptionLabel });

            _contentPanel = new Panel();
            _contentPanel.Size = new Size(950, 460);
            _contentPanel.Location = new Point(0, 100);
            _contentPanel.BackColor = ModernUITheme.Colors.BackgroundPrimary;

            _navigationPanel = new Panel();
            _navigationPanel.Size = new Size(950, 90);
            _navigationPanel.Location = new Point(0, 560);
            _navigationPanel.BackColor = ModernUITheme.Colors.BackgroundSecondary;

            _btnPrevious = ModernControls.CreateActionButton("⬅️ Previous", ModernUITheme.ButtonStyle.Secondary);
            _btnPrevious.Location = new Point(700, 25);
            _btnPrevious.Size = new Size(120, 40);
            _btnPrevious.Enabled = false;

            _btnNext = ModernControls.CreateActionButton("Next ➡️", ModernUITheme.ButtonStyle.Primary);
            _btnNext.Location = new Point(825, 25);
            _btnNext.Size = new Size(120, 40);

            _btnCancel = ModernControls.CreateActionButton("❌ Cancel", ModernUITheme.ButtonStyle.Danger);
            _btnCancel.Location = new Point(15, 25);
            _btnCancel.Size = new Size(120, 40);
            
            // Assign legacy button references for compatibility
            btnCreateEnvironment = _btnNext;
            btnCancel = _btnCancel;

            var progressLabel = ModernControls.CreateStatusLabel("🚀 Setting up your development environment...", ModernUITheme.Colors.TextSecondary);
            progressLabel.Location = new Point(150, 35);
            progressLabel.Size = new Size(500, 20);

            _navigationPanel.Controls.AddRange(new Control[] { _btnPrevious, _btnNext, _btnCancel, progressLabel });
            _mainPanel.Controls.AddRange(new Control[] { _headerPanel, _contentPanel, _navigationPanel });
            this.Controls.Add(_mainPanel);

            FormDiagnostics.LogBulkThemeApplication("CreateManagedEnvironmentForm_Wizard", 12, 12);
        }

        private void SetupWizardEventHandlers()
        {
            FormDiagnostics.LogUserInteraction("SetupEventHandlers", "CreateManagedEnvironmentForm");
            
            if (_btnNext != null)
            {
                _btnNext.Click += BtnNext_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "NextButton");
            }
            
            if (_btnPrevious != null)
            {
                _btnPrevious.Click += BtnPrevious_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "PreviousButton");
            }
            
            if (_btnCancel != null)
            {
                _btnCancel.Click += BtnCancel_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "CancelButton");
            }
        }

        private async void BtnNext_Click(object? sender, EventArgs e)
        {
            FormDiagnostics.LogUserInteraction("NextButtonClick", $"CreateManagedEnvironmentForm", _currentStep);
            
            try
            {
                if (_currentStep == _totalSteps)
                {
                    await CreateManagedEnvironmentAsync();
                }
                else
                {
                    ShowStep(_currentStep + 1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Next button click");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrevious_Click(object? sender, EventArgs e)
        {
            FormDiagnostics.LogUserInteraction("PreviousButtonClick", $"CreateManagedEnvironmentForm", _currentStep);
            
            if (_currentStep > 1)
            {
                ShowStep(_currentStep - 1);
            }
        }

        private void ShowStep(int stepNumber)
        {
            FormDiagnostics.LogUserInteraction("ShowStep", $"CreateManagedEnvironmentForm", stepNumber);
            FormDiagnostics.StartPerformanceTracking($"ShowStep_{stepNumber}");
            
            if (_contentPanel == null) return;
            
            _contentPanel.Controls.Clear();
            _currentStep = stepNumber;
            UpdateWizardHeader();
            
            switch (stepNumber)
            {
                case 1:
                    ShowStep1_SteamLibraryDetection();
                    break;
                case 2:
                    ShowStep2_GamePathConfiguration();
                    break;
                case 3:
                    ShowStep3_ManagedEnvironmentPath();
                    break;
                case 4:
                    ShowStep4_BranchSelection();
                    break;
                default:
                    ShowStep1_SteamLibraryDetection();
                    break;
            }
            
            UpdateNavigationButtons();
            FormDiagnostics.EndPerformanceTracking($"ShowStep_{stepNumber}");
        }

        private void UpdateWizardHeader()
        {
            if (_stepLabel == null || _titleLabel == null || _descriptionLabel == null) return;
            
            _stepLabel.Text = $"Step {_currentStep} of {_totalSteps}";
            
            var stepInfo = GetStepInfo(_currentStep);
            _titleLabel.Text = stepInfo.Title;
            _descriptionLabel.Text = stepInfo.Description;
        }

        private (string Title, string Description) GetStepInfo(int step)
        {
            return step switch
            {
                1 => ("🔍 Steam Library Detection", "Locating your Steam installation and game libraries"),
                2 => ("🎮 Game Path Configuration", "Configuring Schedule I game location"),
                3 => ("📁 Environment Location", "Setting up managed environment storage"),
                4 => ("🌿 Branch Selection", "Choosing branches to manage"),
                _ => ("Setup", "Configuring development environment")
            };
        }

        private void UpdateNavigationButtons()
        {
            if (_btnPrevious == null || _btnNext == null) return;
            
            _btnPrevious.Enabled = _currentStep > 1;
            
            if (_currentStep == _totalSteps)
            {
                _btnNext.Text = "🚀 Create Environment";
                _btnNext.BackColor = ModernUITheme.Colors.AccentSuccess;
            }
            else
            {
                _btnNext.Text = "Next ➡️";
                _btnNext.BackColor = ModernUITheme.Colors.AccentPrimary;
            }
            
            _btnNext.Enabled = ValidateCurrentStep();
            FormDiagnostics.LogButtonStateChange($"NextButton_Step{_currentStep}", _btnNext.Enabled, $"Step {_currentStep} validation");
        }

        private bool ValidateCurrentStep()
        {
            return _currentStep switch
            {
                1 => !string.IsNullOrEmpty(_config.SteamLibraryPath),
                2 => !string.IsNullOrEmpty(_config.GameInstallPath),
                3 => !string.IsNullOrEmpty(_config.ManagedEnvironmentPath),
                4 => _config.SelectedBranches.Count > 0,
                _ => false
            };
        }

        private void ShowStep1_SteamLibraryDetection()
        {
            FormDiagnostics.LogUserInteraction("ShowStep1", "SteamLibraryDetection");
            
            var contentCard = ModernControls.CreateInfoCard(
                "Steam Library Detection", 
                "We'll automatically detect your Steam installation and libraries. " +
                "If multiple libraries are found, you can select the one containing Schedule I.");
            contentCard.Size = new Size(880, 120);
            contentCard.Location = new Point(35, 30);

            var lblSteamLibrary = ModernControls.CreateFieldLabel("Steam Library Path:");
            lblSteamLibrary.Location = new Point(35, 170);
            lblSteamLibrary.Size = new Size(250, 25);
            
            txtSteamLibrary = ModernControls.CreateModernTextBox(true, "Detecting Steam libraries...");
            txtSteamLibrary.Location = new Point(35, 200);
            txtSteamLibrary.Size = new Size(720, 35);
            
            btnBrowseSteamLibrary = ModernControls.CreateActionButton("📁 Browse", ModernUITheme.ButtonStyle.Secondary);
            btnBrowseSteamLibrary.Location = new Point(765, 200);
            btnBrowseSteamLibrary.Size = new Size(100, 35);

            lblStatus = ModernControls.CreateStatusLabel("🔍 Scanning for Steam installations...", ModernUITheme.Colors.AccentInfo);
            lblStatus.Location = new Point(35, 250);
            lblStatus.Size = new Size(850, 25);

            _contentPanel?.Controls.AddRange(new Control[] {
                contentCard, lblSteamLibrary, txtSteamLibrary, btnBrowseSteamLibrary, lblStatus
            });
        }

        private void ShowStep2_GamePathConfiguration()
        {
            FormDiagnostics.LogUserInteraction("ShowStep2", "GamePathConfiguration");
            
            var contentCard = ModernControls.CreateInfoCard(
                "Schedule I Game Configuration", 
                "Verify the detected Schedule I game installation or browse to select a different location.");
            contentCard.Size = new Size(880, 120);
            contentCard.Location = new Point(35, 30);

            var lblGameInstall = ModernControls.CreateFieldLabel("Schedule I Game Installation Path:");
            lblGameInstall.Location = new Point(35, 170);
            lblGameInstall.Size = new Size(350, 25);
            
            txtGameInstall = ModernControls.CreateModernTextBox(true, "");
            txtGameInstall.Location = new Point(35, 200);
            txtGameInstall.Size = new Size(720, 35);
            txtGameInstall.Text = _config.GameInstallPath;
            
            btnBrowseGameInstall = ModernControls.CreateActionButton("📁 Browse", ModernUITheme.ButtonStyle.Secondary);
            btnBrowseGameInstall.Location = new Point(765, 200);
            btnBrowseGameInstall.Size = new Size(100, 35);

            _contentPanel?.Controls.AddRange(new Control[] {
                contentCard, lblGameInstall, txtGameInstall, btnBrowseGameInstall
            });
        }

        private void ShowStep3_ManagedEnvironmentPath()
        {
            FormDiagnostics.LogUserInteraction("ShowStep3", "ManagedEnvironmentPath");
            
            var contentCard = ModernControls.CreateInfoCard(
                "Managed Environment Storage", 
                "Choose where to store your managed development environments. " +
                "This location will contain separate copies of Schedule I for each branch.");
            contentCard.Size = new Size(880, 120);
            contentCard.Location = new Point(35, 30);

            var lblManagedEnv = ModernControls.CreateFieldLabel("Managed Environment Storage Path:");
            lblManagedEnv.Location = new Point(35, 170);
            lblManagedEnv.Size = new Size(350, 25);
            
            txtManagedEnv = ModernControls.CreateModernTextBox(false, "Select a directory for managed environments...");
            txtManagedEnv.Location = new Point(35, 200);
            txtManagedEnv.Size = new Size(720, 35);
            txtManagedEnv.Text = _config.ManagedEnvironmentPath;
            
            btnBrowseManagedEnv = ModernControls.CreateActionButton("📁 Browse", ModernUITheme.ButtonStyle.Primary);
            btnBrowseManagedEnv.Location = new Point(765, 200);
            btnBrowseManagedEnv.Size = new Size(100, 35);

            _contentPanel?.Controls.AddRange(new Control[] {
                contentCard, lblManagedEnv, txtManagedEnv, btnBrowseManagedEnv
            });
        }

        private void ShowStep4_BranchSelection()
        {
            FormDiagnostics.LogUserInteraction("ShowStep4", "BranchSelection");
            
            var contentCard = ModernControls.CreateInfoCard(
                "Branch Selection", 
                "Select which Schedule I branches you want to manage. " +
                "Each selected branch will have its own managed environment copy.");
            contentCard.Size = new Size(880, 120);
            contentCard.Location = new Point(35, 20);

            var lblBranches = ModernControls.CreateFieldLabel("Select Branches to Manage:");
            lblBranches.Location = new Point(35, 160);
            lblBranches.Size = new Size(300, 25);

            chkMainBranch = ModernControls.CreateModernCheckBox("🌟 Main Branch (Stable)");
            chkMainBranch.Location = new Point(50, 195);
            chkMainBranch.Size = new Size(400, 35);

            chkBetaBranch = ModernControls.CreateModernCheckBox("🧪 Beta Branch (Testing)");
            chkBetaBranch.Location = new Point(50, 240);
            chkBetaBranch.Size = new Size(400, 35);

            chkAlternateBranch = ModernControls.CreateModernCheckBox("🔄 Alternate Branch");
            chkAlternateBranch.Location = new Point(50, 285);
            chkAlternateBranch.Size = new Size(400, 35);

            chkAlternateBetaBranch = ModernControls.CreateModernCheckBox("🔬 Alternate Beta Branch");
            chkAlternateBetaBranch.Location = new Point(50, 330);
            chkAlternateBetaBranch.Size = new Size(400, 35);

            _contentPanel?.Controls.AddRange(new Control[] {
                contentCard, lblBranches, chkMainBranch, chkBetaBranch, chkAlternateBranch, chkAlternateBetaBranch
            });

            SetupBranchEventHandlers();
        }

        private void SetupBranchEventHandlers()
        {
            if (chkMainBranch != null) chkMainBranch.CheckedChanged += BranchSelection_Changed;
            if (chkBetaBranch != null) chkBetaBranch.CheckedChanged += BranchSelection_Changed;
            if (chkAlternateBranch != null) chkAlternateBranch.CheckedChanged += BranchSelection_Changed;
            if (chkAlternateBetaBranch != null) chkAlternateBetaBranch.CheckedChanged += BranchSelection_Changed;
        }

        private void BranchSelection_Changed(object? sender, EventArgs e)
        {
            UpdateSelectedBranches();
            UpdateNavigationButtons();
            
            FormDiagnostics.LogUserInteraction("BranchSelectionChanged", "CreateManagedEnvironmentForm", 
                $"Selected: {_config.SelectedBranches.Count}");
        }

        private void LoadSteamInformationForWizard()
        {
            try
            {
                FormDiagnostics.StartPerformanceTracking("LoadSteamInformation");
                _logger.LogInformation("Loading Steam information...");

                var steamLibraries = _steamService.GetSteamLibraryPaths();
                if (steamLibraries.Count > 0)
                {
                    _config.SteamLibraryPath = steamLibraries[0];
                    
                    _availableGames = _steamService.GetSteamGames(_config.SteamLibraryPath);
                    var scheduleIGame = _availableGames.FirstOrDefault(g => g.AppId == "3164500");
                    
                    if (scheduleIGame != null)
                    {
                        _config.GameInstallPath = scheduleIGame.InstallPath;
                        _logger.LogInformation("Schedule I game found at: {Path}", _config.GameInstallPath);
                        
                        var (detectedBranch, buildId) = _steamService.GetBranchAndBuildIdFromManifest(_config.GameInstallPath);
                        if (!string.IsNullOrEmpty(detectedBranch))
                        {
                            _config.InstalledBranch = detectedBranch;
                            _config.SetBuildIdForBranch(detectedBranch, buildId ?? "");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Schedule I game not found in selected Steam library");
                    }
                }
                else
                {
                    _logger.LogWarning("No Steam libraries found");
                }

                FormDiagnostics.EndPerformanceTracking("LoadSteamInformation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Steam information");
                FormDiagnostics.EndPerformanceTracking("LoadSteamInformation");
            }
        }


        #endregion
    }
}
