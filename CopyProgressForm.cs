using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using ScheduleIDevelopementEnvironementManager.UI;
using Microsoft.Extensions.Logging;

namespace ScheduleIDevelopementEnvironementManager
{
    public partial class CopyProgressForm : Form
    {
        private readonly ILogger? _logger;
        private bool isCopyComplete = false;

        // Modern UI Controls
        private Panel? _mainPanel;
        private Panel? _headerPanel;
        private Label? _titleLabel;
        private Label? _statusLabel;
        private Panel? _progressPanel;
        private ProgressBar? _modernProgressBar;
        private Label? _progressLabel;
        private Label? _progressPercent;
        private Panel? _logPanel;
        private RichTextBox? _consoleLog;
        private Panel? _buttonPanel;
        private Button? _btnClose;
        private Button? _btnMinimize;

        public CopyProgressForm(ILogger? logger = null)
        {
            _logger = logger;
            
            // Initialize diagnostics if logger available
            if (_logger != null)
            {
                FormDiagnostics.Initialize(_logger);
                FormDiagnostics.LogFormInitialization("CopyProgressForm");
            }
            
            InitializeModernComponent();
            
            if (_logger != null)
            {
                FormDiagnostics.LogFormLoadComplete("CopyProgressForm");
            }
        }

        private void InitializeModernComponent()
        {
            if (_logger != null)
                FormDiagnostics.StartPerformanceTracking("CopyProgress_Initialization");
            
            this.Text = "📦 File Copy Operation - Schedule I Development Manager";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.BackColor = ModernUITheme.Colors.BackgroundPrimary;
            
            // Load the application icon
            this.Icon = MainForm.LoadApplicationIcon();

            CreateModernLayout();
            SetupModernEventHandlers();
            
            if (_logger != null)
                FormDiagnostics.EndPerformanceTracking("CopyProgress_Initialization");
        }

        private void CreateModernLayout()
        {
            // Main container
            _mainPanel = new Panel();
            _mainPanel.Size = new Size(950, 650);
            _mainPanel.Location = new Point(25, 25);
            _mainPanel.BackColor = ModernUITheme.Colors.BackgroundPrimary;

            // Header section
            _headerPanel = ModernControls.CreateSectionPanel("", new Size(950, 90));
            _headerPanel.Location = new Point(0, 0);
            _headerPanel.BackColor = ModernUITheme.Colors.BackgroundSecondary;

            _titleLabel = ModernControls.CreateHeadingLabel("📦 File Copy Operation", true);
            _titleLabel.Location = new Point(15, 15);
            _titleLabel.Size = new Size(600, 30);
            _titleLabel.ForeColor = ModernUITheme.Colors.AccentPrimary;

            _statusLabel = ModernControls.CreateStatusLabel("🔄 Preparing to copy files...", ModernUITheme.Colors.AccentInfo);
            _statusLabel.Location = new Point(15, 50);
            _statusLabel.Size = new Size(920, 25);

            _headerPanel.Controls.AddRange(new Control[] { _titleLabel, _statusLabel });

            // Progress section
            _progressPanel = ModernControls.CreateSectionPanel("Progress", new Size(950, 110));
            _progressPanel.Location = new Point(0, 100);
            _progressPanel.BackColor = ModernUITheme.Colors.BackgroundSecondary;

            _progressLabel = ModernControls.CreateFieldLabel("🎯 Files processed: 0 / 0");
            _progressLabel.Location = new Point(15, 40);
            _progressLabel.Size = new Size(600, 25);

            _progressPercent = ModernControls.CreateFieldLabel("0%");
            _progressPercent.Location = new Point(850, 40);
            _progressPercent.Size = new Size(85, 25);
            _progressPercent.TextAlign = ContentAlignment.MiddleRight;
            _progressPercent.ForeColor = ModernUITheme.Colors.AccentPrimary;
            _progressPercent.Font = ModernUITheme.Typography.BodyLarge;

            _modernProgressBar = new ProgressBar();
            _modernProgressBar.Location = new Point(15, 75);
            _modernProgressBar.Size = new Size(920, 20);
            _modernProgressBar.Minimum = 0;
            _modernProgressBar.Maximum = 100;
            _modernProgressBar.Value = 0;
            _modernProgressBar.Style = ProgressBarStyle.Continuous;

            _progressPanel.Controls.AddRange(new Control[] { _progressLabel, _progressPercent, _modernProgressBar });

            // Log section
            _logPanel = ModernControls.CreateSectionPanel("📝 Operation Log", new Size(950, 370));
            _logPanel.Location = new Point(0, 220);

            _consoleLog = new RichTextBox();
            _consoleLog.Location = new Point(15, 40);
            _consoleLog.Size = new Size(920, 315);
            _consoleLog.Font = ModernUITheme.Typography.MonospaceBody;
            _consoleLog.ReadOnly = true;
            _consoleLog.BackColor = ModernUITheme.Colors.LogBackground;
            _consoleLog.ForeColor = ModernUITheme.Colors.LogText;
            _consoleLog.ScrollBars = RichTextBoxScrollBars.Both;
            _consoleLog.WordWrap = false;
            _consoleLog.BorderStyle = BorderStyle.None;

            _logPanel.Controls.Add(_consoleLog);

            // Button panel
            _buttonPanel = new Panel();
            _buttonPanel.Size = new Size(950, 70);
            _buttonPanel.Location = new Point(0, 580);
            _buttonPanel.BackColor = ModernUITheme.Colors.BackgroundSecondary;

            _btnMinimize = ModernControls.CreateActionButton("🔽 Minimize", ModernUITheme.ButtonStyle.Secondary);
            _btnMinimize.Location = new Point(15, 18);
            _btnMinimize.Size = new Size(120, 35);
            _btnMinimize.Enabled = true;

            _btnClose = ModernControls.CreateActionButton("❌ Cancel", ModernUITheme.ButtonStyle.Danger);
            _btnClose.Location = new Point(825, 18);
            _btnClose.Size = new Size(110, 35);
            _btnClose.Enabled = false;

            _buttonPanel.Controls.AddRange(new Control[] { _btnMinimize, _btnClose });

            // Add all panels to main panel
            _mainPanel.Controls.AddRange(new Control[] { 
                _headerPanel, _progressPanel, _logPanel, _buttonPanel 
            });

            // Add main panel to form
            this.Controls.Add(_mainPanel);

            if (_logger != null)
                FormDiagnostics.LogBulkThemeApplication("CopyProgressForm", 15, 15);
        }

        private void SetupModernEventHandlers()
        {
            if (_logger != null)
                FormDiagnostics.LogUserInteraction("SetupEventHandlers", "CopyProgressForm");
            
            if (_btnClose != null)
            {
                _btnClose.Click += BtnClose_Click;
                if (_logger != null)
                    FormDiagnostics.LogUserInteraction("EventHandlerAttached", "CloseButton");
            }
            
            if (_btnMinimize != null)
            {
                _btnMinimize.Click += BtnMinimize_Click;
                if (_logger != null)
                    FormDiagnostics.LogUserInteraction("EventHandlerAttached", "MinimizeButton");
            }
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            if (_logger != null)
                FormDiagnostics.LogUserInteraction("CloseButtonClick", "CopyProgressForm", isCopyComplete ? "Operation completed" : "User cancelled");
            this.Close();
        }

        private void BtnMinimize_Click(object? sender, EventArgs e)
        {
            if (_logger != null)
                FormDiagnostics.LogUserInteraction("MinimizeButtonClick", "CopyProgressForm");
            this.WindowState = FormWindowState.Minimized;
        }

        public void LogMessage(string message)
        {
            if (_consoleLog?.InvokeRequired == true)
            {
                _consoleLog.Invoke(new Action<string>(LogMessage), message);
                return;
            }

            if (_consoleLog != null)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                _consoleLog.AppendText($"[{timestamp}] {message}\n");
                _consoleLog.ScrollToCaret();
                
                if (_logger != null)
                    FormDiagnostics.LogUserInteraction("LogMessage", "CopyProgressForm", message.Length.ToString());
            }
        }

        public void UpdateProgress(int value, int currentFiles = 0, int totalFiles = 0)
        {
            if (_modernProgressBar?.InvokeRequired == true)
            {
                _modernProgressBar.Invoke(new Action<int, int, int>(UpdateProgress), value, currentFiles, totalFiles);
                return;
            }

            if (_modernProgressBar != null)
            {
                _modernProgressBar.Value = Math.Min(value, 100);
                
                if (_progressPercent != null)
                    _progressPercent.Text = $"{value}%";
                
                if (_progressLabel != null && totalFiles > 0)
                    _progressLabel.Text = $"🎯 Files processed: {currentFiles} / {totalFiles}";
                
                if (_logger != null)
                    FormDiagnostics.LogUserInteraction("UpdateProgress", "CopyProgressForm", $"{value}% ({currentFiles}/{totalFiles})");
            }
        }

        public void UpdateStatus(string status)
        {
            if (_statusLabel?.InvokeRequired == true)
            {
                _statusLabel.Invoke(new Action<string>(UpdateStatus), status);
                return;
            }

            if (_statusLabel != null)
            {
                _statusLabel.Text = status;
                
                if (_logger != null)
                    FormDiagnostics.LogUserInteraction("UpdateStatus", "CopyProgressForm", status);
            }
        }

        public void SetCopyComplete()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(SetCopyComplete));
                return;
            }

            isCopyComplete = true;
            
            if (_btnClose != null)
            {
                _btnClose.Enabled = true;
                _btnClose.Text = "✅ Close";
                // Update button style to success
                _btnClose.BackColor = ModernUITheme.Colors.AccentSuccess;
            }
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = "✅ Copy operation completed successfully!";
                _statusLabel.ForeColor = ModernUITheme.Colors.AccentSuccess;
            }
            
            LogMessage("🎉 Copy operation completed successfully!");
            
            if (_logger != null)
                FormDiagnostics.LogUserInteraction("SetCopyComplete", "CopyProgressForm", "Operation completed successfully");
        }

        public void SetCopyFailed(string errorMessage)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(SetCopyFailed), errorMessage);
                return;
            }

            isCopyComplete = true;
            
            if (_btnClose != null)
            {
                _btnClose.Enabled = true;
                _btnClose.Text = "❌ Close";
                // Keep danger style for failed operations
            }
            
            if (_statusLabel != null)
            {
                _statusLabel.Text = $"❌ Copy operation failed: {errorMessage}";
                _statusLabel.ForeColor = ModernUITheme.Colors.AccentDanger;
            }
            
            LogMessage($"❌ Copy operation failed: {errorMessage}");
            
            if (_logger != null)
                FormDiagnostics.LogUserInteraction("SetCopyFailed", "CopyProgressForm", errorMessage);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!isCopyComplete)
            {
                var result = MessageBox.Show(
                    "Copy operation is still in progress. Are you sure you want to close?",
                    "Confirm Close",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
            base.OnFormClosing(e);
        }

    }
}
