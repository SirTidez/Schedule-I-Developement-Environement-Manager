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
        
        // UI Controls (removed unused legacy fields to fix warnings)
        
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
            this.Size = new Size(700, 670);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = ModernUITheme.Colors.BackgroundPrimary;
            
            // Load the application icon
            this.Icon = MainForm.LoadApplicationIcon();

            // Apply dark theme to the form
            ApplyDarkTheme();
            CreateModernLayout();
            SetupModernEventHandlers();

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
            _branchInfoPanel.Location = new Point(25, 105);

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

        /// <summary>
        /// Applies dark theme to the form
        /// </summary>
        private void ApplyDarkTheme()
        {
            this.BackColor = ModernUITheme.Colors.BackgroundPrimary;
            this.ForeColor = ModernUITheme.Colors.TextPrimary;
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
                    form.BackColor = ModernUITheme.Colors.BackgroundPrimary;
                    form.ForeColor = ModernUITheme.Colors.TextPrimary;
                    break;
                    
                case Button button:
                    button.BackColor = ModernUITheme.Colors.BackgroundSecondary;
                    button.ForeColor = ModernUITheme.Colors.TextPrimary;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = ModernUITheme.Colors.BorderPrimary;
                    break;
                    
                case Label label:
                    label.BackColor = Color.Transparent;
                    label.ForeColor = ModernUITheme.Colors.TextPrimary;
                    break;
                    
                case TextBox textBox:
                    textBox.BackColor = ModernUITheme.Colors.BackgroundSecondary;
                    textBox.ForeColor = ModernUITheme.Colors.TextPrimary;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    break;
                    
                case Panel panel:
                    panel.BackColor = ModernUITheme.Colors.BackgroundSecondary;
                    break;
            }

            // Apply theme to child controls recursively
            foreach (Control childControl in control.Controls)
            {
                ApplyDarkThemeToControl(childControl);
            }
        }


    }
}
