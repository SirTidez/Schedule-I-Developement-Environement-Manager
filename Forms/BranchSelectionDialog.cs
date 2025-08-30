using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ScheduleIDevelopementEnvironementManager.Models;
using ScheduleIDevelopementEnvironementManager.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScheduleIDevelopementEnvironementManager.Forms
{
    /// <summary>
    /// Dialog for selecting branches to add to the managed environment
    /// </summary>
    public partial class BranchSelectionDialog : Form
    {
        private readonly BranchManagementService _branchManagementService;
        private readonly SteamService _steamService;
        private readonly ILogger<BranchSelectionDialog> _logger;
        private readonly DevEnvironmentConfig _config;
        private List<string> _availableBranches;
        
        // UI Controls
        private Label? lblTitle;
        private Label? lblDescription;
        private CheckedListBox? clbBranches;
        private RichTextBox? rtbBranchInfo;
        private Label? lblSelectedInfo;
        private Button? btnSelectAll;
        private Button? btnSelectNone;
        private Button? btnOK;
        private Button? btnCancel;
        private ProgressBar? progressBar;
        private Label? lblProgress;

        // Color scheme matching the main application
        private static readonly Color BackgroundDark = Color.FromArgb(25, 25, 28);
        private static readonly Color BackgroundMedium = Color.FromArgb(45, 45, 48);
        private static readonly Color BackgroundLight = Color.FromArgb(65, 65, 68);
        private static readonly Color AccentBlue = Color.FromArgb(0, 122, 204);
        private static readonly Color AccentGreen = Color.FromArgb(16, 185, 129);
        private static readonly Color AccentRed = Color.FromArgb(239, 68, 68);
        private static readonly Color TextPrimary = Color.FromArgb(255, 255, 255);
        private static readonly Color TextSecondary = Color.FromArgb(156, 163, 175);

        public List<string> SelectedBranches { get; private set; } = new();

        public BranchSelectionDialog(
            BranchManagementService branchManagementService,
            SteamService steamService,
            ILogger<BranchSelectionDialog> logger,
            DevEnvironmentConfig config)
        {
            _branchManagementService = branchManagementService;
            _steamService = steamService;
            _logger = logger;
            _config = config;
            _availableBranches = _branchManagementService.GetAvailableBranchesToAdd(_config);
            
            InitializeComponent();
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            this.Text = "Add Branches to Managed Environment";
            this.Size = new Size(650, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            
            // Load the application icon
            this.Icon = MainForm.LoadApplicationIcon();

            // Apply dark theme
            ApplyDarkTheme();
            CreateControls();
            SetupEventHandlers();
            LoadAvailableBranches();
        }

        private void CreateControls()
        {
            // Title
            lblTitle = new Label
            {
                Text = "➕ Add New Branches",
                Location = new Point(20, 20),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };

            // Description
            lblDescription = new Label
            {
                Text = "Select the branches you want to add to your managed environment.\n" +
                       "Each branch will be copied from Steam when you switch to it.",
                Location = new Point(20, 60),
                Size = new Size(580, 40),
                Font = new Font("Segoe UI", 9),
                ForeColor = TextSecondary,
                BackColor = Color.Transparent
            };

            // Available branches list
            var lblAvailableBranches = new Label
            {
                Text = "Available Branches:",
                Location = new Point(20, 110),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };

            clbBranches = new CheckedListBox
            {
                Location = new Point(20, 135),
                Size = new Size(280, 200),
                BackColor = BackgroundLight,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9),
                CheckOnClick = true
            };

            // Branch info panel
            var lblBranchInfo = new Label
            {
                Text = "Branch Information:",
                Location = new Point(320, 110),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };

            rtbBranchInfo = new RichTextBox
            {
                Location = new Point(320, 135),
                Size = new Size(280, 200),
                BackColor = BackgroundDark,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                Font = new Font("Consolas", 8),
                Text = "Select a branch to view details..."
            };

            // Selection controls
            lblSelectedInfo = new Label
            {
                Text = "0 branches selected",
                Location = new Point(20, 345),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = TextSecondary,
                BackColor = Color.Transparent
            };

            btnSelectAll = CreateStyledButton("Select All", new Point(20, 370), new Size(80, 30), AccentBlue);
            btnSelectNone = CreateStyledButton("Select None", new Point(110, 370), new Size(90, 30), AccentBlue);

            // Progress controls (initially hidden)
            progressBar = new ProgressBar
            {
                Location = new Point(20, 415),
                Size = new Size(580, 20),
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };

            lblProgress = new Label
            {
                Text = "",
                Location = new Point(20, 440),
                Size = new Size(580, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = TextSecondary,
                BackColor = Color.Transparent,
                Visible = false
            };

            // Action buttons
            btnOK = CreateStyledButton("Add Selected Branches", new Point(350, 470), new Size(150, 35), AccentGreen);
            btnCancel = CreateStyledButton("Cancel", new Point(510, 470), new Size(90, 35), AccentRed);
            
            btnOK.Enabled = false; // Initially disabled until branches are selected

            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblDescription, lblAvailableBranches, clbBranches,
                lblBranchInfo, rtbBranchInfo, lblSelectedInfo,
                btnSelectAll, btnSelectNone, progressBar, lblProgress,
                btnOK, btnCancel
            });
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
                Math.Min(255, accentColor.B + 20));
            
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(
                Math.Max(0, accentColor.R - 20),
                Math.Max(0, accentColor.G - 20),
                Math.Max(0, accentColor.B - 20));

            return button;
        }

        private void SetupEventHandlers()
        {
            if (clbBranches != null)
            {
                clbBranches.SelectedIndexChanged += ClbBranches_SelectedIndexChanged;
                clbBranches.ItemCheck += ClbBranches_ItemCheck;
            }

            if (btnSelectAll != null) btnSelectAll.Click += BtnSelectAll_Click;
            if (btnSelectNone != null) btnSelectNone.Click += BtnSelectNone_Click;
            if (btnOK != null) btnOK.Click += BtnOK_Click;
            if (btnCancel != null) btnCancel.Click += BtnCancel_Click;
        }

        private void LoadAvailableBranches()
        {
            if (clbBranches == null) return;

            clbBranches.Items.Clear();

            if (_availableBranches.Count == 0)
            {
                clbBranches.Items.Add("No branches available to add");
                return;
            }

            foreach (var branchName in _availableBranches)
            {
                var displayName = BranchInfo.GetDisplayName(branchName);
                clbBranches.Items.Add(displayName);
            }

            UpdateSelectedInfo();
        }

        private void ClbBranches_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (clbBranches?.SelectedIndex >= 0 && clbBranches.SelectedIndex < _availableBranches.Count)
            {
                var branchName = _availableBranches[clbBranches.SelectedIndex];
                ShowBranchInfo(branchName);
            }
        }

        private void ClbBranches_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            // Use BeginInvoke to ensure the check state is updated before we read it
            BeginInvoke(() => UpdateSelectedInfo());
        }

        private void ShowBranchInfo(string branchName)
        {
            if (rtbBranchInfo == null) return;

            var description = _steamService.GetBranchDescription(branchName);
            var displayName = BranchInfo.GetDisplayName(branchName);

            var info = $"Branch: {displayName}\n\n";
            info += $"Internal Name: {branchName}\n\n";
            info += $"Description:\n{description}\n\n";
            info += "Status: Ready to install\n\n";
            info += "Note: This branch will be copied from Steam when you first switch to it.";

            rtbBranchInfo.Text = info;
        }

        private void UpdateSelectedInfo()
        {
            if (clbBranches == null || lblSelectedInfo == null || btnOK == null) return;

            var checkedCount = clbBranches.CheckedItems.Count;
            lblSelectedInfo.Text = $"{checkedCount} branch{(checkedCount == 1 ? "" : "es")} selected";
            btnOK.Enabled = checkedCount > 0;
        }

        private void BtnSelectAll_Click(object? sender, EventArgs e)
        {
            if (clbBranches == null) return;

            for (int i = 0; i < clbBranches.Items.Count; i++)
            {
                clbBranches.SetItemChecked(i, true);
            }
        }

        private void BtnSelectNone_Click(object? sender, EventArgs e)
        {
            if (clbBranches == null) return;

            for (int i = 0; i < clbBranches.Items.Count; i++)
            {
                clbBranches.SetItemChecked(i, false);
            }
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            if (clbBranches == null) return;

            // Collect selected branches
            SelectedBranches.Clear();
            for (int i = 0; i < clbBranches.CheckedItems.Count; i++)
            {
                var checkedIndex = clbBranches.CheckedIndices[i];
                if (checkedIndex < _availableBranches.Count)
                {
                    SelectedBranches.Add(_availableBranches[checkedIndex]);
                }
            }

            if (SelectedBranches.Count == 0)
            {
                MessageBox.Show("Please select at least one branch to add.", 
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirm the selection
            var branchNames = SelectedBranches.Select(BranchInfo.GetDisplayName).ToArray();
            var message = $"Add the following branch{(SelectedBranches.Count == 1 ? "" : "es")} to your managed environment?\n\n" +
                         string.Join("\n", branchNames) + "\n\n" +
                         "Note: Branches will be automatically copied when you switch to them in Steam.";

            var result = MessageBox.Show(message, "Confirm Branch Addition", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                AddSelectedBranches();
            }
        }

        private async void AddSelectedBranches()
        {
            try
            {
                // Show progress controls
                if (progressBar != null && lblProgress != null)
                {
                    progressBar.Visible = true;
                    lblProgress.Visible = true;
                    progressBar.Value = 0;
                    progressBar.Maximum = SelectedBranches.Count;
                }

                // Disable controls during operation
                SetControlsEnabled(false);

                // Add each selected branch to configuration
                for (int i = 0; i < SelectedBranches.Count; i++)
                {
                    var branchName = SelectedBranches[i];
                    var displayName = BranchInfo.GetDisplayName(branchName);
                    
                    if (lblProgress != null)
                    {
                        lblProgress.Text = $"Adding {displayName}...";
                    }

                    // Add to selected branches in config
                    if (!_config.SelectedBranches.Contains(branchName))
                    {
                        _config.SelectedBranches.Add(branchName);
                    }

                    // Update progress
                    if (progressBar != null)
                    {
                        progressBar.Value = i + 1;
                    }

                    // Small delay for visual feedback
                    await Task.Delay(200);
                }

                if (lblProgress != null)
                {
                    lblProgress.Text = "Saving configuration...";
                }

                // Save the updated configuration
                var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
                var fileLoggingFactory = new FileLoggingServiceFactory();
                services.AddLogging(builder => builder.AddProvider(new FileLoggingProvider(fileLoggingFactory)));
                services.AddSingleton<ConfigurationService>();
                var serviceProvider = services.BuildServiceProvider();
                var configService = serviceProvider.GetRequiredService<ConfigurationService>();
                await configService.SaveConfigurationAsync(_config);

                if (lblProgress != null)
                {
                    lblProgress.Text = "Complete!";
                }

                _logger.LogInformation("Successfully added {Count} branches: {Branches}", 
                    SelectedBranches.Count, string.Join(", ", SelectedBranches));

                // Show success message
                await Task.Delay(500);
                MessageBox.Show($"Successfully added {SelectedBranches.Count} branch{(SelectedBranches.Count == 1 ? "" : "es")} to your managed environment.\n\n" +
                               "The branches will be copied when you first switch to them in Steam.",
                               "Branches Added", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding selected branches");
                
                if (lblProgress != null)
                {
                    lblProgress.Text = "Error occurred";
                }

                MessageBox.Show($"Error adding branches: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                SetControlsEnabled(true);
                
                // Hide progress controls
                if (progressBar != null && lblProgress != null)
                {
                    progressBar.Visible = false;
                    lblProgress.Visible = false;
                }
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            if (clbBranches != null) clbBranches.Enabled = enabled;
            if (btnSelectAll != null) btnSelectAll.Enabled = enabled;
            if (btnSelectNone != null) btnSelectNone.Enabled = enabled;
            if (btnOK != null) btnOK.Enabled = enabled && clbBranches?.CheckedItems.Count > 0;
            if (btnCancel != null) btnCancel.Enabled = enabled;
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = BackgroundDark;
            this.ForeColor = TextPrimary;
        }
    }
}