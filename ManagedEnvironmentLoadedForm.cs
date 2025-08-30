using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScheduleIDevelopementEnvironementManager.Models;
using ScheduleIDevelopementEnvironementManager.Services;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace ScheduleIDevelopementEnvironementManager
{
    public partial class ManagedEnvironmentLoadedForm : Form
    {
        private readonly SteamService _steamService;
        private readonly ConfigurationService _configService;
        private readonly ILogger<ManagedEnvironmentLoadedForm> _logger;
        private DevEnvironmentConfig _config;

        // UI Controls
        private Label? lblTitle;
        private TextBox? txtConfigInfo;
        private ListBox? lstBranches;
        private Button? btnRefresh;
        private Button? btnExit;
        private Button? btnReconfigure;

        public ManagedEnvironmentLoadedForm(DevEnvironmentConfig config)
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
            _logger = serviceProvider.GetRequiredService<ILogger<ManagedEnvironmentLoadedForm>>();
            
            _config = config;
            
            InitializeForm();
            LoadConfigurationInfo();
        }

        private void InitializeForm()
        {
            this.Text = "Schedule I Development Environment Manager - Managed Environment Loaded";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            
            // Load the application icon
            this.Icon = MainForm.LoadApplicationIcon();

            CreateControls();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            // Title
            lblTitle = new Label
            {
                Text = "Managed Environment Loaded Successfully!",
                Location = new Point(20, 20),
                Size = new Size(750, 30),
                Font = new Font(this.Font.FontFamily, 16, FontStyle.Bold),
                ForeColor = Color.Green,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Configuration Information
            var lblConfigInfo = new Label
            {
                Text = "Configuration Information:",
                Location = new Point(20, 70),
                Size = new Size(200, 20),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            txtConfigInfo = new TextBox
            {
                Location = new Point(20, 95),
                Size = new Size(740, 200),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9)
            };

            // Branches List
            var lblBranches = new Label
            {
                Text = "Managed Branches:",
                Location = new Point(20, 310),
                Size = new Size(200, 20),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            lstBranches = new ListBox
            {
                Location = new Point(20, 335),
                Size = new Size(740, 200),
                Font = new Font("Consolas", 9)
            };

            // Buttons
            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(200, 560),
                Size = new Size(100, 35)
            };

            btnReconfigure = new Button
            {
                Text = "Reconfigure",
                Location = new Point(320, 560),
                Size = new Size(120, 35),
                BackColor = Color.LightBlue
            };

            btnExit = new Button
            {
                Text = "Exit",
                Location = new Point(460, 560),
                Size = new Size(100, 35)
            };

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblConfigInfo, txtConfigInfo, lblBranches, lstBranches,
                btnRefresh, btnReconfigure, btnExit
            });
        }

        private void SetupEventHandlers()
        {
            btnRefresh!.Click += BtnRefresh_Click;
            btnReconfigure!.Click += BtnReconfigure_Click;
            btnExit!.Click += BtnExit_Click;
        }

        private void LoadConfigurationInfo()
        {
            try
            {
                _logger.LogInformation("Loading configuration information for display");
                
                // Build configuration information text
                var configInfo = new StringBuilder();
                configInfo.AppendLine("=== SCHEDULE I DEVELOPMENT ENVIRONMENT CONFIGURATION ===");
                configInfo.AppendLine();
                configInfo.AppendLine($"Steam Library Path: {_config.SteamLibraryPath}");
                configInfo.AppendLine($"Game Installation Path: {_config.GameInstallPath}");
                configInfo.AppendLine($"Managed Environment Path: {_config.ManagedEnvironmentPath}");
                configInfo.AppendLine($"Installed Branch: {_config.InstalledBranch}");
                configInfo.AppendLine();
                configInfo.AppendLine($"Selected Branches ({_config.SelectedBranches.Count}):");
                foreach (var branch in _config.SelectedBranches)
                {
                    configInfo.AppendLine($"  - {branch}");
                }
                configInfo.AppendLine();
                configInfo.AppendLine($"Configuration File: {_configService.GetConfigFilePath()}");
                configInfo.AppendLine($"Last Modified: {File.GetLastWriteTime(_configService.GetConfigFilePath()):yyyy-MM-dd HH:mm:ss}");
                
                txtConfigInfo!.Text = configInfo.ToString();

                // Load branch information
                LoadBranchInformation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration information");
                txtConfigInfo!.Text = $"Error loading configuration: {ex.Message}";
            }
        }

        private void LoadBranchInformation()
        {
            try
            {
                _logger.LogInformation("Loading branch information");
                lstBranches!.Items.Clear();

                if (string.IsNullOrEmpty(_config.ManagedEnvironmentPath) || !Directory.Exists(_config.ManagedEnvironmentPath))
                {
                    lstBranches.Items.Add("ERROR: Managed environment path does not exist or is invalid");
                    return;
                }

                // Get all branch directories
                var branchDirectories = Directory.GetDirectories(_config.ManagedEnvironmentPath);
                
                if (branchDirectories.Length == 0)
                {
                    lstBranches.Items.Add("No branch directories found in managed environment");
                    return;
                }

                foreach (var branchDir in branchDirectories)
                {
                    var branchName = Path.GetFileName(branchDir);
                    var branchInfo = GetBranchInfo(branchDir);
                    lstBranches.Items.Add(branchInfo);
                }

                // Also show selected branches that might not have directories yet
                foreach (var selectedBranch in _config.SelectedBranches)
                {
                    var branchPath = Path.Combine(_config.ManagedEnvironmentPath, selectedBranch);
                    if (!Directory.Exists(branchPath))
                    {
                        lstBranches.Items.Add($"[NOT CREATED] {selectedBranch} - Path: {branchPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading branch information");
                lstBranches!.Items.Add($"Error loading branch information: {ex.Message}");
            }
        }

        private string GetBranchInfo(string branchPath)
        {
            try
            {
                var branchName = Path.GetFileName(branchPath);
                var directoryInfo = new DirectoryInfo(branchPath);
                var fileCount = Directory.GetFiles(branchPath, "*", SearchOption.AllDirectories).Length;
                var totalSize = GetDirectorySize(branchPath);
                var lastModified = directoryInfo.LastWriteTime;

                return $"[{branchName}] - Path: {branchPath} - Files: {fileCount:N0} - Size: {FormatFileSize(totalSize)} - Modified: {lastModified:yyyy-MM-dd HH:mm:ss}";
            }
            catch (Exception ex)
            {
                return $"[{Path.GetFileName(branchPath)}] - Error: {ex.Message}";
            }
        }

        private long GetDirectorySize(string path)
        {
            try
            {
                var dir = new DirectoryInfo(path);
                return dir.GetFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            }
            catch
            {
                return 0;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            try
            {
                _logger.LogInformation("Refresh button clicked, reloading configuration information");
                
                // Reload configuration from file
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var refreshedConfig = await _configService.LoadConfigurationAsync();
                        if (refreshedConfig != null)
                        {
                            _config = refreshedConfig;
                            this.Invoke(() =>
                            {
                                LoadConfigurationInfo();
                                MessageBox.Show("Configuration refreshed successfully!", "Refresh Complete", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error refreshing configuration");
                        this.Invoke(() =>
                        {
                            MessageBox.Show($"Error refreshing configuration: {ex.Message}", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling refresh button click");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReconfigure_Click(object? sender, EventArgs e)
        {
            try
            {
                _logger.LogInformation("Reconfigure button clicked, opening configuration window");
                
                // TODO: Show the configuration management window
                MessageBox.Show("Configuration management functionality will be implemented later.", 
                              "Coming Soon", 
                              MessageBoxButtons.OK, 
                              MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling reconfigure button click");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExit_Click(object? sender, EventArgs e)
        {
            this.Close();
        }
    }
}
