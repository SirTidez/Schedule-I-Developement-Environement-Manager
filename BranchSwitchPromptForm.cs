using System;
using System.Windows.Forms;
using System.Drawing;
using ScheduleIDevelopementEnvironementManager.UI;
using Microsoft.Extensions.Logging;

namespace ScheduleIDevelopementEnvironementManager
{
    public partial class BranchSwitchPromptForm : Form
    {
        private readonly ILogger? _logger;
        
        // Modern UI Controls
        private Panel? _mainPanel;
        private Label? _titleLabel;
        private Panel? _branchInfoPanel;
        private Label? _currentBranchLabel;
        private Label? _nextBranchLabel;
        private Panel? _buttonPanel;
        private Button? _btnConfirm;
        private Button? _btnCancel;

        public string CurrentBranch { get; private set; }
        public string NextBranch { get; private set; }
        public bool UserConfirmed { get; private set; } = false;

        public BranchSwitchPromptForm(string currentBranch, string nextBranch, ILogger? logger = null)
        {
            _logger = logger;
            CurrentBranch = currentBranch;
            NextBranch = nextBranch;
            
            // Initialize diagnostics if logger available
            if (_logger != null)
            {
                FormDiagnostics.Initialize(_logger);
                FormDiagnostics.LogFormInitialization("BranchSwitchPromptForm");
            }
            
            InitializeModernComponent();
            
            if (_logger != null)
            {
                FormDiagnostics.LogFormLoadComplete("BranchSwitchPromptForm");
            }
        }

        private void InitializeModernComponent()
        {
            if (_logger != null)
                FormDiagnostics.StartPerformanceTracking("BranchSwitchPrompt_Initialization");
            
            this.Text = "🔄 Branch Switch Assistant - Schedule I Development Manager";
            this.Size = new Size(700, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ModernUITheme.Colors.BackgroundPrimary;
            
            // Load the application icon
            this.Icon = MainForm.LoadApplicationIcon();

<<<<<<< Updated upstream
            // Apply dark theme to the form
            ApplyDarkTheme();

            // Main message
            lblMessage = new Label
            {
                Text = "Please switch to the next branch in Steam:",
                Location = new Point(20, 20),
                Size = new Size(450, 40),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Current branch info
            lblCurrentBranch = new Label
            {
                Text = $"Current Branch: {CurrentBranch}",
                Location = new Point(20, 80),
                Size = new Size(450, 25),
                Font = new Font(this.Font.FontFamily, 9),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Next branch info
            lblNextBranch = new Label
            {
                Text = $"Next Branch: {NextBranch}",
                Location = new Point(20, 110),
                Size = new Size(450, 25),
                Font = new Font(this.Font.FontFamily, 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Blue
            };

            // Instructions
            var lblInstructions = new Label
            {
                Text = "1. Open Steam\n2. Right-click on 'Schedule I' in your library\n3. Select 'Properties'\n4. Go to 'Betas' tab\n5. Select the branch from the dropdown\n6. Close the properties window\n7. Click OK when ready to continue",
                Location = new Point(20, 150),
                Size = new Size(450, 100),
                Font = new Font(this.Font.FontFamily, 8),
                TextAlign = ContentAlignment.TopLeft
            };

            // Important note about waiting for download completion
            var lblDownloadNote = new Label
            {
                Text = "⚠️ IMPORTANT: After switching branches, ensure that Steam has fully downloaded and installed the new branch before clicking OK. The game should show as 'Ready to Play' in your Steam library.",
                Location = new Point(20, 270),
                Size = new Size(450, 60),
                Font = new Font(this.Font.FontFamily, 8, FontStyle.Bold),
                TextAlign = ContentAlignment.TopLeft,
                ForeColor = Color.Orange
            };

            // OK button
            btnOk = new Button
            {
                Text = "OK - Branch Switched",
                Location = new Point(150, 360), // Moved down from 220 to 360
                Size = new Size(120, 30),
                DialogResult = DialogResult.OK
            };

            btnOk.Click += BtnOk_Click;

            // Cancel button
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(290, 360), // Moved down from 220 to 360
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            // Apply dark theme to all controls
            ApplyDarkThemeToControl(lblMessage);
            ApplyDarkThemeToControl(lblCurrentBranch);
            ApplyDarkThemeToControl(lblNextBranch);
            ApplyDarkThemeToControl(lblInstructions);
            ApplyDarkThemeToControl(lblDownloadNote);
            ApplyDarkThemeToControl(btnOk);
            ApplyDarkThemeToControl(btnCancel);

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblMessage,
                lblCurrentBranch,
                lblNextBranch,
                lblInstructions,
                lblDownloadNote,
                btnOk,
                btnCancel
            });
=======
            CreateModernLayout();
            SetupModernEventHandlers();
>>>>>>> Stashed changes

            // Set default button
            this.AcceptButton = _btnConfirm;
            this.CancelButton = _btnCancel;
            
            if (_logger != null)
                FormDiagnostics.EndPerformanceTracking("BranchSwitchPrompt_Initialization");
        }

        private void CreateModernLayout()
        {
            _mainPanel = new Panel();
            _mainPanel.Size = new Size(650, 570);
            _mainPanel.Location = new Point(25, 25);
            _mainPanel.BackColor = ModernUITheme.Colors.BackgroundPrimary;

            // Header section
            var headerPanel = ModernControls.CreateSectionPanel("", new Size(650, 90));
            headerPanel.Location = new Point(0, 0);
            headerPanel.BackColor = ModernUITheme.Colors.BackgroundSecondary;

            _titleLabel = ModernControls.CreateHeadingLabel("🔄 Steam Branch Switch Required", true);
            _titleLabel.Location = new Point(15, 15);
            _titleLabel.Size = new Size(620, 30);
            _titleLabel.ForeColor = ModernUITheme.Colors.AccentWarning;

            var subtitleLabel = ModernControls.CreateStatusLabel("Follow the instructions below to switch branches in Steam", ModernUITheme.Colors.TextSecondary);
            subtitleLabel.Location = new Point(15, 50);
            subtitleLabel.Size = new Size(620, 25);

            headerPanel.Controls.AddRange(new Control[] { _titleLabel, subtitleLabel });

            // Branch info panel
            _branchInfoPanel = ModernControls.CreateInfoCard("Branch Information", "");
            _branchInfoPanel.Size = new Size(600, 110);
            _branchInfoPanel.Location = new Point(25, 110);

            _currentBranchLabel = ModernControls.CreateFieldLabel($"📍 Current Branch: {CurrentBranch}");
            _currentBranchLabel.Location = new Point(15, 40);
            _currentBranchLabel.Size = new Size(570, 25);
            _currentBranchLabel.Font = ModernUITheme.Typography.BodyLarge;

            _nextBranchLabel = ModernControls.CreateFieldLabel($"🎯 Target Branch: {NextBranch}");
            _nextBranchLabel.Location = new Point(15, 70);
            _nextBranchLabel.Size = new Size(570, 25);
            _nextBranchLabel.Font = ModernUITheme.Typography.BodyLarge;
            _nextBranchLabel.ForeColor = ModernUITheme.Colors.AccentPrimary;

            _branchInfoPanel.Controls.AddRange(new Control[] { _currentBranchLabel, _nextBranchLabel });

            // Instructions card
            var instructionsCard = ModernControls.CreateInfoCard("📋 Step-by-Step Instructions", 
                "1. 🚀 Open Steam application\n" +
                "2. 📚 Right-click on 'Schedule I' in your library\n" +
                "3. ⚙️ Select 'Properties' from the context menu\n" +
                "4. 🌿 Navigate to the 'Betas' tab\n" +
                "5. 📋 Select the target branch from the dropdown\n" +
                "6. ✅ Close the properties window\n" +
                "7. ⏳ Wait for Steam to download and install\n" +
                "8. 🎯 Click 'Continue' when ready");
            instructionsCard.Size = new Size(600, 180);
            instructionsCard.Location = new Point(25, 240);

            // Important warning panel
            var warningCard = ModernControls.CreateInfoCard("⚠️ Important Notice", 
                "After switching branches in Steam, ensure the game has fully downloaded and installed " +
                "the new branch before continuing. The game should show as 'Ready to Play' in your Steam library. " +
                "Proceeding before the download completes may cause branch detection issues.");
            warningCard.Size = new Size(600, 80);
            warningCard.Location = new Point(25, 440);
            warningCard.BackColor = ModernUITheme.Colors.BackgroundHover;

            // Button panel
            _buttonPanel = new Panel();
            _buttonPanel.Size = new Size(600, 60);
            _buttonPanel.Location = new Point(25, 530);
            _buttonPanel.BackColor = Color.Transparent;

            _btnConfirm = ModernControls.CreateActionButton("✅ Continue - Branch Switched", ModernUITheme.ButtonStyle.Success);
            _btnConfirm.Location = new Point(365, 15);
            _btnConfirm.Size = new Size(220, 40);
            _btnConfirm.DialogResult = DialogResult.OK;

            _btnCancel = ModernControls.CreateActionButton("❌ Cancel", ModernUITheme.ButtonStyle.Danger);
            _btnCancel.Location = new Point(15, 15);
            _btnCancel.Size = new Size(100, 40);
            _btnCancel.DialogResult = DialogResult.Cancel;

            _buttonPanel.Controls.AddRange(new Control[] { _btnConfirm, _btnCancel });

            // Add all panels to main panel
            _mainPanel.Controls.AddRange(new Control[] { 
                headerPanel, _branchInfoPanel, instructionsCard, warningCard, _buttonPanel 
            });

            // Add main panel to form
            this.Controls.Add(_mainPanel);

            if (_logger != null)
                FormDiagnostics.LogBulkThemeApplication("BranchSwitchPromptForm", 12, 12);
        }

        private void SetupModernEventHandlers()
        {
            if (_logger != null)
                FormDiagnostics.LogUserInteraction("SetupEventHandlers", "BranchSwitchPromptForm");
            
            if (_btnConfirm != null)
            {
                _btnConfirm.Click += BtnConfirm_Click;
                if (_logger != null)
                    FormDiagnostics.LogUserInteraction("EventHandlerAttached", "ConfirmButton");
            }
            
            if (_btnCancel != null)
            {
                _btnCancel.Click += BtnCancel_Click;
                if (_logger != null)
                    FormDiagnostics.LogUserInteraction("EventHandlerAttached", "CancelButton");
            }
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            if (_logger != null)
                FormDiagnostics.LogUserInteraction("ConfirmButtonClick", "BranchSwitchPromptForm", "Branch switch confirmed by user");
            UserConfirmed = true;
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            if (_logger != null)
                FormDiagnostics.LogUserInteraction("CancelButtonClick", "BranchSwitchPromptForm", "Branch switch cancelled by user");
            UserConfirmed = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                UserConfirmed = false;
            }
            base.OnFormClosing(e);
        }

    }
}
