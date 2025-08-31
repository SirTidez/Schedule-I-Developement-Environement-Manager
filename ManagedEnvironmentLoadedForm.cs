using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScheduleIDevelopementEnvironementManager.Models;
using ScheduleIDevelopementEnvironementManager.Services;
using ScheduleIDevelopementEnvironementManager.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScheduleIDevelopementEnvironementManager
{
    public partial class ManagedEnvironmentLoadedForm : Form
    {
        private readonly SteamService _steamService;
        private readonly ConfigurationService _configService;
        private readonly BranchManagementService _branchManagementService;
        private readonly FileOperationsService _fileOperationsService;
        private readonly ILogger<ManagedEnvironmentLoadedForm> _logger;
        private DevEnvironmentConfig _config;
        private List<BranchInfo> _branches = new();
        private BranchInfo? _selectedBranch;

        // Advanced UI Controls
        private Label? lblTitle;
        private Label? lblEnvironmentPath;
        private Label? lblCurrentSteamBranch;
        private DataGridView? dgvBranches;
        private StatusStrip? statusStrip;
        private ToolStripStatusLabel? statusLabel;
        private ToolStripStatusLabel? lastRefreshLabel;
        private ToolStripStatusLabel? connectionStatusLabel;
        private Panel? pnlBranchActions;
        private Button? btnLaunch;
        private Button? btnDelete;
        private Button? btnAddBranch;
        private Button? btnRefresh;
        private Button? btnOpenFolder;
        private Button? btnSettings;
        private Button? btnExport;
        private Button? btnExit;
        private System.Windows.Forms.Timer? refreshTimer;
        private Panel? pnlBranchDetails;
        private Label? lblBranchDetails;
        private RichTextBox? rtbBranchInfo;

        // Color scheme for sophisticated dark theme
        private static readonly Color BackgroundDark = Color.FromArgb(25, 25, 28);
        private static readonly Color BackgroundMedium = Color.FromArgb(45, 45, 48);
        private static readonly Color BackgroundLight = Color.FromArgb(65, 65, 68);
        private static readonly Color AccentBlue = Color.FromArgb(0, 122, 204);
        private static readonly Color AccentGreen = Color.FromArgb(16, 185, 129);
        private static readonly Color AccentRed = Color.FromArgb(239, 68, 68);
        private static readonly Color AccentOrange = Color.FromArgb(245, 158, 11);
        private static readonly Color TextPrimary = Color.FromArgb(255, 255, 255);
        private static readonly Color TextSecondary = Color.FromArgb(156, 163, 175);

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
            services.AddSingleton<FileOperationsService>();
            services.AddSingleton<BranchManagementService>();
            
            var serviceProvider = services.BuildServiceProvider();
            _steamService = serviceProvider.GetRequiredService<SteamService>();
            _configService = serviceProvider.GetRequiredService<ConfigurationService>();
            
            // Set managed environment path for config and logging
            if (!string.IsNullOrEmpty(config.ManagedEnvironmentPath))
            {
                fileLoggingFactory.SetManagedEnvironmentPath(config.ManagedEnvironmentPath);
                _configService.SetManagedEnvironmentPath(config.ManagedEnvironmentPath);
            }
            _fileOperationsService = serviceProvider.GetRequiredService<FileOperationsService>();
            _branchManagementService = serviceProvider.GetRequiredService<BranchManagementService>();
            _logger = serviceProvider.GetRequiredService<ILogger<ManagedEnvironmentLoadedForm>>();
            
            _config = config;
            
            InitializeAdvancedForm();
            _ = LoadBranchDataAsync();
        }

        private void InitializeAdvancedForm()
        {
            this.Text = "Schedule I - Development Environment Manager";
            this.Size = new Size(1600, 575);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            
            // Load the application icon
            this.Icon = MainForm.LoadApplicationIcon();

            // Apply sophisticated dark theme
            ApplySophisticatedDarkTheme();

            CreateAdvancedControls();
            SetupAdvancedEventHandlers();
            SetupRefreshTimer();
        }

        private void CreateAdvancedControls()
        {
            // Header Section
            CreateHeaderSection();
            
            // Main Content Area
            CreateBranchDataGrid();
            
            // Branch Actions Panel
            CreateBranchActionsPanel();
            
            // Branch Details Panel
            CreateBranchDetailsPanel();
            
            // Status Strip
            CreateAdvancedStatusStrip();
        }

        private void CreateHeaderSection()
        {
            // Main Title
            lblTitle = new Label
            {
                Text = "🎮 Development Environment Manager",
                Location = new Point(20, 20),
                Size = new Size(600, 35),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };

            // Environment Path
            lblEnvironmentPath = new Label
            {
                Text = $"Environment: {_config.ManagedEnvironmentPath}",
                Location = new Point(20, 60),
                Size = new Size(700, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = TextSecondary,
                BackColor = Color.Transparent
            };

            // Current Steam Branch
            lblCurrentSteamBranch = new Label
            {
                Text = "Current Steam Branch: Loading...",
                Location = new Point(20, 85),
                Size = new Size(400, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = AccentBlue,
                BackColor = Color.Transparent
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblEnvironmentPath, lblCurrentSteamBranch });
        }

        private void CreateBranchDataGrid()
        {
            dgvBranches = new DataGridView
            {
                Location = new Point(20, 120),
                Size = new Size(1220, 300),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                BorderStyle = BorderStyle.FixedSingle,
                BackgroundColor = BackgroundLight,
                GridColor = Color.FromArgb(80, 80, 80),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = BackgroundLight,
                    ForeColor = TextPrimary,
                    SelectionBackColor = AccentBlue,
                    SelectionForeColor = Color.White,
                    Font = new Font("Segoe UI", 9)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = BackgroundMedium,
                    ForeColor = TextPrimary,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(50, 50, 53),
                    ForeColor = TextPrimary
                }
            };

            // Define columns
            var statusColumn = new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                Width = 60,
                ReadOnly = true
            };

            var branchColumn = new DataGridViewTextBoxColumn
            {
                Name = "Branch",
                HeaderText = "Branch Name",
                Width = 200,
                ReadOnly = true
            };

            var sizeColumn = new DataGridViewTextBoxColumn
            {
                Name = "Size",
                HeaderText = "Size",
                Width = 120,
                ReadOnly = true
            };

            var filesColumn = new DataGridViewTextBoxColumn
            {
                Name = "Files",
                HeaderText = "Files",
                Width = 120,
                ReadOnly = true
            };

            var modifiedColumn = new DataGridViewTextBoxColumn
            {
                Name = "Modified",
                HeaderText = "Modified",
                Width = 120,
                ReadOnly = true
            };

            var buildIdColumn = new DataGridViewTextBoxColumn
            {
                Name = "BuildId",
                HeaderText = "Build ID",
                Width = 140,
                ReadOnly = true
            };

            var launchCommandColumn = new DataGridViewTextBoxColumn
            {
                Name = "LaunchCommand",
                HeaderText = "Launch",
                Width = 80,
                ReadOnly = true
            };

            var statusDescColumn = new DataGridViewTextBoxColumn
            {
                Name = "StatusDesc",
                HeaderText = "Status Description",
                Width = 380,
                ReadOnly = true
            };

            dgvBranches.Columns.AddRange(new DataGridViewColumn[]
            {
                statusColumn, branchColumn, sizeColumn, filesColumn, 
                modifiedColumn, buildIdColumn, launchCommandColumn, statusDescColumn
            });

            this.Controls.Add(dgvBranches);
        }

        private void CreateBranchActionsPanel()
        {
            pnlBranchActions = new Panel
            {
                Location = new Point(20, 440),
                Size = new Size(1220, 60),
                BackColor = BackgroundMedium,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Action buttons with sophisticated styling - evenly distributed across full width panel
            btnLaunch = CreateStyledButton("🚀 Launch", new Point(10, 15), new Size(120, 30), AccentGreen);
            btnDelete = CreateStyledButton("🗑️ Delete", new Point(140, 15), new Size(120, 30), AccentRed);
            btnAddBranch = CreateStyledButton("➕ Add Branch", new Point(270, 15), new Size(130, 30), AccentBlue);
            btnRefresh = CreateStyledButton("🔄 Refresh", new Point(410, 15), new Size(120, 30), AccentBlue);
            btnOpenFolder = CreateStyledButton("📁 Open Folder", new Point(540, 15), new Size(130, 30), AccentBlue);
            btnSettings = CreateStyledButton("⚙️ Settings", new Point(680, 15), new Size(120, 30), AccentBlue);
            btnExport = CreateStyledButton("📋 Export", new Point(810, 15), new Size(120, 30), AccentBlue);
            btnExit = CreateStyledButton("❌ Exit", new Point(940, 15), new Size(120, 30), AccentRed);

            // Initially disable action buttons until a branch is selected
            btnLaunch.Enabled = false;
            btnDelete.Enabled = false;
            btnOpenFolder.Enabled = false;

            pnlBranchActions.Controls.AddRange(new Control[]
            {
                btnLaunch, btnDelete, btnAddBranch, btnRefresh, 
                btnOpenFolder, btnSettings, btnExit
            });

            this.Controls.Add(pnlBranchActions);
        }

        private Button CreateStyledButton(string text, Point location, Size size, Color accentColor)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                BackColor = accentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderColor = Color.FromArgb(
                Math.Max(0, accentColor.R - 30),
                Math.Max(0, accentColor.G - 30),
                Math.Max(0, accentColor.B - 30));
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Min(255, accentColor.R + 20),
                Math.Min(255, accentColor.G + 20),
                Math.Min(255, accentColor.B + 20)
            );
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(
                Math.Max(0, accentColor.R - 20),
                Math.Max(0, accentColor.G - 20),
                Math.Max(0, accentColor.B - 20)
            );

            return button;
        }

        private void CreateBranchDetailsPanel()
        {
            pnlBranchDetails = new Panel
            {
                Location = new Point(1270, 120),
                Size = new Size(300, 380),
                BackColor = BackgroundMedium,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblBranchDetails = new Label
            {
                Text = "Branch Details",
                Location = new Point(10, 10),
                Size = new Size(280, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };

            rtbBranchInfo = new RichTextBox
            {
                Location = new Point(10, 40),
                Size = new Size(280, 330),
                BackColor = BackgroundDark,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                Text = "Select a branch to view details..."
            };

            pnlBranchDetails.Controls.AddRange(new Control[] { lblBranchDetails, rtbBranchInfo });
            this.Controls.Add(pnlBranchDetails);
        }

        private void CreateAdvancedStatusStrip()
        {
            statusStrip = new StatusStrip
            {
                BackColor = BackgroundDark,
                ForeColor = TextSecondary,
                Font = new Font("Segoe UI", 9)
            };

            statusLabel = new ToolStripStatusLabel
            {
                Text = "Ready",
                ForeColor = TextPrimary
            };

            lastRefreshLabel = new ToolStripStatusLabel
            {
                Text = "Last Refresh: Never",
                ForeColor = TextSecondary,
                Spring = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            connectionStatusLabel = new ToolStripStatusLabel
            {
                Text = "⚫ Disconnected",
                ForeColor = AccentRed
            };

            statusStrip.Items.AddRange(new ToolStripItem[]
            {
                statusLabel,
                new ToolStripStatusLabel { Spring = true },
                lastRefreshLabel,
                connectionStatusLabel
            });

            this.Controls.Add(statusStrip);
        }

        private void SetupAdvancedEventHandlers()
        {
            // DataGridView events
            if (dgvBranches != null)
            {
                dgvBranches.SelectionChanged += DgvBranches_SelectionChanged;
                dgvBranches.CellPainting += DgvBranches_CellPainting;
                dgvBranches.CellDoubleClick += DgvBranches_CellDoubleClick;
                dgvBranches.MouseDown += DgvBranches_MouseDown;
            }

            // Button events
            if (btnLaunch != null) btnLaunch.Click += BtnLaunch_Click;
            if (btnDelete != null) btnDelete.Click += BtnDelete_Click;
            if (btnAddBranch != null) btnAddBranch.Click += BtnAddBranch_Click;
            if (btnRefresh != null) btnRefresh.Click += BtnRefresh_Click;
            if (btnOpenFolder != null) btnOpenFolder.Click += BtnOpenFolder_Click;
            if (btnSettings != null) btnSettings.Click += BtnSettings_Click;
            if (btnExport != null) btnExport.Click += BtnExport_Click;
            if (btnExit != null) btnExit.Click += BtnExit_Click;

            // Form events
            this.FormClosing += ManagedEnvironmentLoadedForm_FormClosing;
        }

        private void SetupRefreshTimer()
        {
            refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 30000 // 30 seconds
            };
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }

        private async Task LoadBranchDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting LoadBranchDataAsync");
                UpdateStatus("Loading branch information...");
                
                _logger.LogInformation("Calling BranchManagementService.GetAllBranchesAsync");
                // Load branch data
                _branches = await _branchManagementService.GetAllBranchesAsync(_config);
                _logger.LogInformation("GetAllBranchesAsync completed, got {Count} branches", _branches.Count);
                
                _logger.LogInformation("Updating DataGridView");
                // Update DataGridView
                UpdateBranchDataGrid();
                _logger.LogInformation("DataGridView updated");
                
                _logger.LogInformation("Updating current Steam branch display");
                // Update current Steam branch display
                await UpdateCurrentSteamBranchAsync();
                _logger.LogInformation("Steam branch display updated");
                
                UpdateStatus($"Loaded {_branches.Count} branches");
                UpdateLastRefreshTime();
                UpdateConnectionStatus(true);
                _logger.LogInformation("LoadBranchDataAsync completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading branch data");
                UpdateStatus($"Error loading branches: {ex.Message}");
                UpdateConnectionStatus(false);
            }
        }

        private void UpdateBranchDataGrid()
        {
            if (dgvBranches == null) return;

            dgvBranches.Rows.Clear();
            
            foreach (var branch in _branches)
            {
                var row = new DataGridViewRow();
                row.CreateCells(dgvBranches);
                row.Cells[0].Value = branch.StatusIcon;
                row.Cells[1].Value = branch.DisplayName;
                row.Cells[2].Value = branch.FormattedSize;
                row.Cells[3].Value = branch.FormattedFileCount;
                row.Cells[4].Value = branch.FormattedLastModified;
                row.Cells[5].Value = string.IsNullOrEmpty(branch.LocalBuildId) ? "---" : branch.LocalBuildId;
                
                // Custom launch command indicator
                var hasCustomCommand = _config.HasCustomLaunchCommand(branch.BranchName);
                row.Cells[6].Value = hasCustomCommand ? "🎯 Custom" : "🚀 Default";
                
                row.Cells[7].Value = branch.StatusDescription;
                row.Tag = branch;
                
                dgvBranches.Rows.Add(row);
            }
        }

        private async Task UpdateCurrentSteamBranchAsync()
        {
            try
            {
                var currentBranch = await _steamService.GetCurrentBranchFromGamePathAsync(_config.GameInstallPath);
                if (lblCurrentSteamBranch != null)
                {
                    if (!string.IsNullOrEmpty(currentBranch))
                    {
                        var displayName = BranchInfo.GetDisplayName(currentBranch);
                        lblCurrentSteamBranch.Text = $"Current Steam Branch: {displayName}";
                        lblCurrentSteamBranch.ForeColor = AccentGreen;
                    }
                    else
                    {
                        lblCurrentSteamBranch.Text = "Current Steam Branch: Unknown";
                        lblCurrentSteamBranch.ForeColor = AccentOrange;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current Steam branch");
                if (lblCurrentSteamBranch != null)
                {
                    lblCurrentSteamBranch.Text = "Current Steam Branch: Error";
                    lblCurrentSteamBranch.ForeColor = AccentRed;
                }
            }
        }

        private void UpdateStatus(string message)
        {
            if (statusLabel != null)
            {
                statusLabel.Text = message;
            }
            _logger.LogInformation(message);
        }

        private void UpdateLastRefreshTime()
        {
            if (lastRefreshLabel != null)
            {
                lastRefreshLabel.Text = $"Last Refresh: {DateTime.Now:HH:mm:ss}";
            }
        }

        private void UpdateConnectionStatus(bool connected)
        {
            if (connectionStatusLabel != null)
            {
                connectionStatusLabel.Text = connected ? "🟢 Connected" : "⚫ Disconnected";
                connectionStatusLabel.ForeColor = connected ? AccentGreen : AccentRed;
            }
        }

        #region Event Handlers

        private void DgvBranches_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvBranches?.SelectedRows.Count > 0)
            {
                _selectedBranch = dgvBranches.SelectedRows[0].Tag as BranchInfo;
                UpdateBranchActionButtons();
                UpdateBranchDetailsPanel();
            }
            else
            {
                _selectedBranch = null;
                UpdateBranchActionButtons();
                ClearBranchDetailsPanel();
            }
        }

        private void UpdateBranchActionButtons()
        {
            bool branchSelected = _selectedBranch != null;
            bool branchInstalled = _selectedBranch?.IsInstalled ?? false;

            if (btnLaunch != null) btnLaunch.Enabled = branchSelected && branchInstalled;
            if (btnDelete != null) btnDelete.Enabled = branchSelected && branchInstalled;
            if (btnOpenFolder != null) btnOpenFolder.Enabled = branchSelected && branchInstalled;
        }

        private void UpdateBranchDetailsPanel()
        {
            if (_selectedBranch == null || rtbBranchInfo == null) return;

            var details = $"Branch: {_selectedBranch.DisplayName}\n\n";
            details += $"Status: {_selectedBranch.StatusDescription}\n\n";
            details += $"Folder: {_selectedBranch.FolderPath}\n\n";
            details += $"Executable: {(_selectedBranch.IsInstalled ? "Found" : "Missing")}\n\n";
            details += $"Size: {_selectedBranch.FormattedSize}\n";
            details += $"Files: {_selectedBranch.FormattedFileCount}\n";
            details += $"Modified: {_selectedBranch.FormattedLastModified}\n\n";
            details += $"Local Build ID: {(_selectedBranch.LocalBuildId ?? "Unknown")}\n";
            details += $"Steam Build ID: {(_selectedBranch.SteamBuildId ?? "Unknown")}\n\n";
            details += $"Steam Branch: {(_selectedBranch.IsCurrentSteamBranch ? "Yes" : "No")}";

            rtbBranchInfo.Text = details;
        }

        private void ClearBranchDetailsPanel()
        {
            if (rtbBranchInfo != null)
            {
                rtbBranchInfo.Text = "Select a branch to view details...";
            }
        }

        private void DgvBranches_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            // Custom painting for status icons and visual enhancements
            if (e.RowIndex >= 0 && e.ColumnIndex == 0) // Status column
            {
                e.PaintBackground(e.CellBounds, true);
                
                if (e.Value != null)
                {
                    var icon = e.Value.ToString();
                    using var brush = new SolidBrush(TextPrimary);
                    var rect = e.CellBounds;
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    
                    e.Graphics?.DrawString(icon, new Font("Segoe UI Emoji", 12), brush, rect, format);
                }
                
                e.Handled = true;
            }
        }

        private async void DgvBranches_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _selectedBranch?.IsInstalled == true)
            {
                await LaunchSelectedBranchAsync();
            }
        }

        private void DgvBranches_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dgvBranches?.HitTest(e.X, e.Y);
                if (hitTest?.RowIndex >= 0)
                {
                    dgvBranches?.ClearSelection();
                    if (dgvBranches != null) dgvBranches.Rows[hitTest.RowIndex].Selected = true;
                    ShowContextMenu(e.Location);
                }
            }
        }

        private void ShowContextMenu(Point location)
        {
            var contextMenu = new ContextMenuStrip();
            contextMenu.BackColor = BackgroundMedium;
            contextMenu.ForeColor = TextPrimary;

            if (_selectedBranch?.IsInstalled == true)
            {
                // Check if custom launch command exists
                var hasCustomCommand = _config.HasCustomLaunchCommand(_selectedBranch.BranchName);
                
                if (hasCustomCommand)
                {
                    contextMenu.Items.Add("🎯 Launch with Custom Command", null, async (s, e) => await LaunchWithCustomCommandAsync());
                    contextMenu.Items.Add("🚀 Launch Game (Default)", null, async (s, e) => await LaunchSelectedBranchAsync());
                }
                else
                {
                    contextMenu.Items.Add("🚀 Launch Game", null, async (s, e) => await LaunchSelectedBranchAsync());
                }
                
                contextMenu.Items.Add("📁 Open Folder", null, async (s, e) => await OpenSelectedBranchFolderAsync());
                contextMenu.Items.Add(new ToolStripSeparator());
                
                // Custom launch command management
                if (hasCustomCommand)
                {
                    contextMenu.Items.Add("⚙️ Edit Custom Launch Command", null, (s, e) => EditCustomLaunchCommand());
                    contextMenu.Items.Add("❌ Remove Custom Launch Command", null, (s, e) => RemoveCustomLaunchCommand());
                }
                else
                {
                    contextMenu.Items.Add("⚙️ Set Custom Launch Command", null, (s, e) => SetCustomLaunchCommand());
                }
                
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add("🗑️ Delete Branch", null, async (s, e) => await DeleteSelectedBranchAsync());
            }
            else
            {
                contextMenu.Items.Add("➕ Install Branch", null, (s, e) => AddBranchAsync());
            }

            if (dgvBranches != null) contextMenu.Show(dgvBranches, location);
        }

        private async void BtnLaunch_Click(object? sender, EventArgs e)
        {
            if (_selectedBranch == null || !_selectedBranch.IsInstalled) return;

            // Check if custom launch command exists
            var hasCustomCommand = _config.HasCustomLaunchCommand(_selectedBranch.BranchName);
            
            if (hasCustomCommand)
            {
                // Show options dialog
                var result = MessageBox.Show(
                    $"This branch has a custom launch command configured.\n\n" +
                    $"Would you like to:\n" +
                    $"• Yes: Launch with custom command\n" +
                    $"• No: Launch with default method\n" +
                    $"• Cancel: Don't launch",
                    "Launch Options",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes:
                        await LaunchWithCustomCommandAsync();
                        break;
                    case DialogResult.No:
                        await LaunchSelectedBranchAsync();
                        break;
                    // Cancel does nothing
                }
            }
            else
            {
                await LaunchSelectedBranchAsync();
            }
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            await DeleteSelectedBranchAsync();
        }

        private void BtnAddBranch_Click(object? sender, EventArgs e)
        {
            AddBranchAsync();
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e)
        {
            await LoadBranchDataAsync();
        }

        private async void BtnOpenFolder_Click(object? sender, EventArgs e)
        {
            await OpenSelectedBranchFolderAsync();
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            ShowSettingsDialog();
        }

        /// <summary>
        /// Shows the settings dialog with configuration management options
        /// </summary>
        private void ShowSettingsDialog()
        {
            var result = MessageBox.Show(
                "Settings Options:\n\n" +
                "Do you want to delete the current configuration?\n\n" +
                "⚠️ WARNING: This will remove the configuration file but keep all managed environment instances intact.\n" +
                "You will need to reconfigure the application after deletion.\n\n" +
                "Click 'Yes' to delete configuration\n" +
                "Click 'No' to cancel",
                "⚙️ Settings - Delete Configuration", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                DeleteConfiguration();
            }
        }

        /// <summary>
        /// Deletes the current configuration file and handles cleanup
        /// </summary>
        private void DeleteConfiguration()
        {
            try
            {
                _logger.LogInformation("User initiated configuration deletion");

                // Get the current config file path
                var configFilePath = _configService.GetConfigFilePath();
                
                if (!File.Exists(configFilePath))
                {
                    _logger.LogWarning("Configuration file not found: {ConfigPath}", configFilePath);
                    MessageBox.Show("Configuration file not found.", "Information", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Show final confirmation
                var finalConfirmResult = MessageBox.Show(
                    $"Are you absolutely sure you want to delete the configuration?\n\n" +
                    $"File to be deleted:\n{configFilePath}\n\n" +
                    $"This action cannot be undone!\n\n" +
                    $"Managed environment instances will remain intact.",
                    "⚠️ Final Confirmation - Delete Configuration", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Error);

                if (finalConfirmResult != DialogResult.Yes)
                {
                    _logger.LogInformation("Configuration deletion cancelled by user");
                    return;
                }

                // Delete the configuration file
                File.Delete(configFilePath);
                _logger.LogInformation("Configuration file deleted successfully: {ConfigPath}", configFilePath);

                // Show success message
                MessageBox.Show(
                    "Configuration deleted successfully!\n\n" +
                    "The application will now exit. Next time you run the application, " +
                    "you'll go through the initial setup process again.\n\n" +
                    "Your managed environment instances remain intact.",
                    "✅ Configuration Deleted", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);

                // Close the application since we no longer have a valid configuration
                _logger.LogInformation("Closing application after configuration deletion");
                Application.Exit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting configuration file");
                MessageBox.Show(
                    $"Error deleting configuration file:\n\n{ex.Message}\n\n" +
                    "Please try again or delete the file manually.",
                    "❌ Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            ExportBranchData();
        }

        private void BtnExit_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        private async void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            await LoadBranchDataAsync();
        }

        private void ManagedEnvironmentLoadedForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
        }


        #endregion

        #region Branch Operations

        private async Task LaunchSelectedBranchAsync()
        {
            if (_selectedBranch == null || !_selectedBranch.IsInstalled) return;

            try
            {
                UpdateStatus($"Launching {_selectedBranch.DisplayName}...");
                
                var success = await _branchManagementService.LaunchBranchAsync(_selectedBranch);
                if (success)
                {
                    UpdateStatus($"Successfully launched {_selectedBranch.DisplayName}");
                }
                else
                {
                    UpdateStatus($"Failed to launch {_selectedBranch.DisplayName}");
                    MessageBox.Show($"Failed to launch {_selectedBranch.DisplayName}. Check the logs for details.",
                        "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error launching branch {BranchName}", _selectedBranch.BranchName);
                UpdateStatus($"Error launching {_selectedBranch.DisplayName}");
                MessageBox.Show($"Error launching {_selectedBranch.DisplayName}: {ex.Message}",
                    "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Task LaunchWithCustomCommandAsync()
        {
            if (_selectedBranch == null || !_selectedBranch.IsInstalled) return Task.CompletedTask;

            try
            {
                var customCommand = _config.GetCustomLaunchCommand(_selectedBranch.BranchName);
                if (string.IsNullOrWhiteSpace(customCommand))
                {
                    MessageBox.Show("No custom launch command is set for this branch.", "Custom Launch", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return Task.CompletedTask;
                }

                UpdateStatus($"Launching {_selectedBranch.DisplayName} with custom command...");
                _logger.LogInformation("Executing custom launch command for branch {BranchName}: {Command}", 
                    _selectedBranch.BranchName, customCommand);

                CustomLaunchCommandDialog.ExecuteCommand(customCommand);
                UpdateStatus($"Custom launch command executed for {_selectedBranch.DisplayName}");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing custom launch command for branch {BranchName}", _selectedBranch.BranchName);
                UpdateStatus($"Error executing custom command for {_selectedBranch.DisplayName}");
                MessageBox.Show($"Error executing custom launch command: {ex.Message}",
                    "Custom Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return Task.CompletedTask;
            }
        }

        private void SetCustomLaunchCommand()
        {
            if (_selectedBranch == null) return;

            try
            {
                var existingCommand = _config.GetCustomLaunchCommand(_selectedBranch.BranchName);
                using var dialog = new CustomLaunchCommandDialog(_selectedBranch.BranchName, existingCommand);
                
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var newCommand = dialog.BuildCommand();
                    _config.SetCustomLaunchCommand(_selectedBranch.BranchName, newCommand);
                    
                    // Save configuration
                    _ = Task.Run(async () => await _configService.SaveConfigurationAsync(_config));
                    
                    _logger.LogInformation("Custom launch command set for branch {BranchName}: {Command}", 
                        _selectedBranch.BranchName, newCommand);
                    
                    UpdateStatus($"Custom launch command set for {_selectedBranch.DisplayName}");
                    MessageBox.Show("Custom launch command saved successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting custom launch command for branch {BranchName}", _selectedBranch.BranchName);
                MessageBox.Show($"Error setting custom launch command: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditCustomLaunchCommand()
        {
            SetCustomLaunchCommand(); // Same dialog, different context
        }

        private void RemoveCustomLaunchCommand()
        {
            if (_selectedBranch == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove the custom launch command for {_selectedBranch.DisplayName}?",
                    "Remove Custom Launch Command",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _config.SetCustomLaunchCommand(_selectedBranch.BranchName, "");
                    
                    // Save configuration
                    _ = Task.Run(async () => await _configService.SaveConfigurationAsync(_config));
                    
                    _logger.LogInformation("Custom launch command removed for branch {BranchName}", _selectedBranch.BranchName);
                    UpdateStatus($"Custom launch command removed for {_selectedBranch.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing custom launch command for branch {BranchName}", _selectedBranch.BranchName);
                MessageBox.Show($"Error removing custom launch command: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteSelectedBranchAsync()
        {
            if (_selectedBranch == null || !_selectedBranch.IsInstalled) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete the {_selectedBranch.DisplayName} branch?\n\n" +
                $"This will permanently delete all files in:\n{_selectedBranch.FolderPath}\n\n" +
                $"Size: {_selectedBranch.FormattedSize}\nFiles: {_selectedBranch.FormattedFileCount}",
                "Confirm Delete Branch",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                try
                {
                    UpdateStatus($"Deleting {_selectedBranch.DisplayName}...");
                    
                    var success = await _branchManagementService.DeleteBranchAsync(_selectedBranch, _config);
                    if (success)
                    {
                        UpdateStatus($"Successfully deleted {_selectedBranch.DisplayName}");
                        await LoadBranchDataAsync(); // Refresh the list
                    }
                    else
                    {
                        UpdateStatus($"Failed to delete {_selectedBranch.DisplayName}");
                        MessageBox.Show($"Failed to delete {_selectedBranch.DisplayName}. Check the logs for details.",
                            "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting branch {BranchName}", _selectedBranch.BranchName);
                    UpdateStatus($"Error deleting {_selectedBranch.DisplayName}");
                    MessageBox.Show($"Error deleting {_selectedBranch.DisplayName}: {ex.Message}",
                        "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task OpenSelectedBranchFolderAsync()
        {
            if (_selectedBranch == null || !_selectedBranch.IsInstalled) return;

            try
            {
                var success = await _fileOperationsService.OpenDirectoryInExplorerAsync(_selectedBranch.FolderPath);
                if (success)
                {
                    UpdateStatus($"Opened folder for {_selectedBranch.DisplayName}");
                }
                else
                {
                    UpdateStatus($"Failed to open folder for {_selectedBranch.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening folder for branch {BranchName}", _selectedBranch.BranchName);
                MessageBox.Show($"Error opening folder: {ex.Message}",
                    "Folder Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void AddBranchAsync()
        {
            try
            {
                var availableBranches = _branchManagementService.GetAvailableBranchesToAdd(_config);
                
                if (availableBranches.Count == 0)
                {
                    MessageBox.Show("All available branches are already installed in your managed environment.",
                        "No Branches Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create services for the dialog
                var services = new ServiceCollection();
                var fileLoggingFactory = new FileLoggingServiceFactory();
                services.AddLogging(builder => builder.AddProvider(new FileLoggingProvider(fileLoggingFactory)));
                services.AddSingleton<SteamService>();
                services.AddSingleton<ConfigurationService>();
                services.AddSingleton<FileOperationsService>();
                services.AddSingleton<BranchManagementService>();
                var serviceProvider = services.BuildServiceProvider();

                var dialogLogger = serviceProvider.GetRequiredService<ILogger<Forms.BranchSelectionDialog>>();
                
                // Show branch selection dialog
                using var dialog = new Forms.BranchSelectionDialog(
                    _branchManagementService, _steamService, dialogLogger, _config);
                
                if (dialog.ShowDialog() == DialogResult.OK && dialog.SelectedBranches.Count > 0)
                {
                    UpdateStatus($"Added {dialog.SelectedBranches.Count} branches to managed environment");
                    
                    // Refresh the branch list to show the newly added branches
                    await LoadBranchDataAsync();
                    
                    MessageBox.Show(
                        $"Successfully added {dialog.SelectedBranches.Count} branch{(dialog.SelectedBranches.Count == 1 ? "" : "es")}.\n\n" +
                        "The branches will be automatically copied when you switch to them in Steam.",
                        "Branches Added", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddBranchAsync");
                UpdateStatus("Error adding branches");
                MessageBox.Show($"Error adding branches: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportBranchData()
        {
            try
            {
                using var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|JSON files (*.json)|*.json",
                    DefaultExt = "csv",
                    FileName = $"ScheduleI_Branches_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var extension = Path.GetExtension(saveDialog.FileName).ToLowerInvariant();
                    
                    if (extension == ".csv")
                    {
                        ExportToCsv(saveDialog.FileName);
                    }
                    else if (extension == ".json")
                    {
                        ExportToJson(saveDialog.FileName);
                    }

                    UpdateStatus($"Exported branch data to {saveDialog.FileName}");
                    MessageBox.Show($"Branch data exported successfully to:\n{saveDialog.FileName}",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting branch data");
                MessageBox.Show($"Error exporting branch data: {ex.Message}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCsv(string fileName)
        {
            using var writer = new StreamWriter(fileName);
            writer.WriteLine("Branch,Status,Size,Files,Modified,BuildID,Description");
            
            foreach (var branch in _branches)
            {
                writer.WriteLine($"\"{branch.DisplayName}\",\"{branch.Status}\",\"{branch.FormattedSize}\"," +
                               $"\"{branch.FormattedFileCount}\",\"{branch.FormattedLastModified}\"," +
                               $"\"{branch.LocalBuildId}\",\"{branch.StatusDescription}\"");
            }
        }

        private void ExportToJson(string fileName)
        {
            var exportData = _branches.Select(b => new
            {
                Branch = b.DisplayName,
                Status = b.Status.ToString(),
                Size = b.FormattedSize,
                Files = b.FormattedFileCount,
                Modified = b.FormattedLastModified,
                BuildId = b.LocalBuildId,
                Description = b.StatusDescription,
                IsInstalled = b.IsInstalled,
                FolderPath = b.FolderPath
            }).ToArray();

            var json = System.Text.Json.JsonSerializer.Serialize(exportData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            
            File.WriteAllText(fileName, json);
        }

        #endregion

        #region Dark Theme

        private void ApplySophisticatedDarkTheme()
        {
            this.BackColor = BackgroundDark;
            this.ForeColor = TextPrimary;
        }

        #endregion
    }
}