using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ScheduleIDevelopementEnvironementManager.Forms
{
    public partial class CustomLaunchCommandDialog : Form
    {
        private Label? lblTitle;
        private Label? lblBranchName;
        private Label? lblCommandPath;
        private Label? lblWorkingDirectory;
        private Label? lblArguments;
        private Label? lblDescription;
        private TextBox? txtCommandPath;
        private TextBox? txtWorkingDirectory;
        private TextBox? txtArguments;
        private Button? btnBrowseCommand;
        private Button? btnBrowseWorkingDir;
        private Button? btnOK;
        private Button? btnCancel;
        private Button? btnTest;
        private CheckBox? chkUseShellExecute;
        private RichTextBox? rtbDescription;

        public string BranchName { get; private set; }
        public string CommandPath { get; private set; } = string.Empty;
        public string WorkingDirectory { get; private set; } = string.Empty;
        public string Arguments { get; private set; } = string.Empty;
        public bool UseShellExecute { get; private set; } = true;

        // Dark theme colors
        private static readonly Color BackgroundDark = Color.FromArgb(25, 25, 28);
        private static readonly Color BackgroundMedium = Color.FromArgb(45, 45, 48);
        private static readonly Color BackgroundLight = Color.FromArgb(65, 65, 68);
        private static readonly Color AccentBlue = Color.FromArgb(0, 122, 204);
        private static readonly Color AccentGreen = Color.FromArgb(16, 185, 129);
        private static readonly Color AccentRed = Color.FromArgb(239, 68, 68);
        private static readonly Color TextPrimary = Color.FromArgb(255, 255, 255);
        private static readonly Color TextSecondary = Color.FromArgb(156, 163, 175);

        public CustomLaunchCommandDialog(string branchName, string existingCommand = "")
        {
            BranchName = branchName;
            InitializeComponent();
            ParseExistingCommand(existingCommand);
        }

        private void InitializeComponent()
        {
            this.Text = $"Custom Launch Command - {BranchName}";
            this.Size = new Size(600, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = BackgroundDark;

            CreateControls();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            // Title
            lblTitle = new Label
            {
                Text = $"🎮 Custom Launch Command for {BranchName}",
                Location = new Point(20, 20),
                Size = new Size(550, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };

            // Branch name display
            lblBranchName = new Label
            {
                Text = $"Branch: {BranchName}",
                Location = new Point(20, 55),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = TextSecondary,
                BackColor = Color.Transparent
            };

            // Command path
            lblCommandPath = new Label
            {
                Text = "Executable/Script Path: *",
                Location = new Point(20, 90),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };

            txtCommandPath = new TextBox
            {
                Location = new Point(20, 115),
                Size = new Size(450, 25),
                Font = new Font("Segoe UI", 9),
                BackColor = BackgroundLight,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnBrowseCommand = new Button
            {
                Text = "📁",
                Location = new Point(480, 115),
                Size = new Size(30, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            btnBrowseCommand.FlatAppearance.BorderColor = AccentBlue;

            // Working directory
            lblWorkingDirectory = new Label
            {
                Text = "Working Directory (optional):",
                Location = new Point(20, 155),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };

            txtWorkingDirectory = new TextBox
            {
                Location = new Point(20, 180),
                Size = new Size(450, 25),
                Font = new Font("Segoe UI", 9),
                BackColor = BackgroundLight,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnBrowseWorkingDir = new Button
            {
                Text = "📁",
                Location = new Point(480, 180),
                Size = new Size(30, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            btnBrowseWorkingDir.FlatAppearance.BorderColor = AccentBlue;

            // Arguments
            lblArguments = new Label
            {
                Text = "Command Arguments (optional):",
                Location = new Point(20, 220),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };

            txtArguments = new TextBox
            {
                Location = new Point(20, 245),
                Size = new Size(490, 25),
                Font = new Font("Segoe UI", 9),
                BackColor = BackgroundLight,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Shell execute option
            chkUseShellExecute = new CheckBox
            {
                Text = "Use Shell Execute (recommended for .bat, .ps1, etc.)",
                Location = new Point(20, 285),
                Size = new Size(350, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent,
                Checked = true
            };

            // Description
            lblDescription = new Label
            {
                Text = "Examples and Tips:",
                Location = new Point(20, 315),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TextPrimary,
                BackColor = Color.Transparent
            };

            rtbDescription = new RichTextBox
            {
                Location = new Point(20, 340),
                Size = new Size(490, 80),
                Font = new Font("Segoe UI", 8),
                BackColor = BackgroundMedium,
                ForeColor = TextSecondary,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                Text = "Examples:\n" +
                       "• Executable: C:\\Path\\To\\Game.exe\n" +
                       "• Batch file: C:\\Scripts\\launch_with_mods.bat\n" +
                       "• PowerShell: powershell.exe -File \"C:\\Scripts\\setup.ps1\"\n" +
                       "• Steam URL: steam://rungameid/3164500\n\n" +
                       "Leave Working Directory empty to use the branch folder as default."
            };

            // Action buttons
            btnOK = new Button
            {
                Text = "✅ Save",
                Location = new Point(320, 430),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnOK.FlatAppearance.BorderColor = AccentGreen;

            btnCancel = new Button
            {
                Text = "❌ Cancel",
                Location = new Point(410, 430),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderColor = AccentRed;

            btnTest = new Button
            {
                Text = "🧪 Test",
                Location = new Point(230, 430),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = AccentBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            btnTest.FlatAppearance.BorderColor = AccentBlue;

            // Add all controls to form
            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblBranchName, lblCommandPath, txtCommandPath, btnBrowseCommand,
                lblWorkingDirectory, txtWorkingDirectory, btnBrowseWorkingDir,
                lblArguments, txtArguments, chkUseShellExecute,
                lblDescription, rtbDescription,
                btnOK, btnCancel, btnTest
            });
        }

        private void SetupEventHandlers()
        {
            if (btnBrowseCommand != null) btnBrowseCommand.Click += BtnBrowseCommand_Click;
            if (btnBrowseWorkingDir != null) btnBrowseWorkingDir.Click += BtnBrowseWorkingDir_Click;
            if (btnTest != null) btnTest.Click += BtnTest_Click;
            if (btnOK != null) btnOK.Click += BtnOK_Click;
            if (txtCommandPath != null) txtCommandPath.TextChanged += TxtCommandPath_TextChanged;
        }

        private void BtnBrowseCommand_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Select Executable or Script",
                Filter = "All Executable Files|*.exe;*.bat;*.cmd;*.ps1;*.com;*.scr|" +
                        "Executable Files (*.exe)|*.exe|" +
                        "Batch Files (*.bat;*.cmd)|*.bat;*.cmd|" +
                        "PowerShell Scripts (*.ps1)|*.ps1|" +
                        "All Files (*.*)|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtCommandPath!.Text = dialog.FileName;
                
                // Auto-set working directory to the directory containing the executable
                if (string.IsNullOrWhiteSpace(txtWorkingDirectory!.Text))
                {
                    txtWorkingDirectory.Text = Path.GetDirectoryName(dialog.FileName) ?? "";
                }
            }
        }

        private void BtnBrowseWorkingDir_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select Working Directory",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtWorkingDirectory!.Text = dialog.SelectedPath;
            }
        }

        private void BtnTest_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCommandPath?.Text))
            {
                MessageBox.Show("Please specify a command path first.", "Test Command", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var command = BuildCommand();
                var result = MessageBox.Show(
                    $"This will test the launch command:\n\n" +
                    $"Command: {command}\n\n" +
                    $"Do you want to proceed with the test?",
                    "Test Launch Command",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    ExecuteCommand(command);
                    MessageBox.Show("Test command executed. Check if it launched correctly.", 
                        "Test Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing command: {ex.Message}", "Test Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCommandPath?.Text))
            {
                MessageBox.Show("Please specify a command path.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Build and store the command
            CommandPath = txtCommandPath?.Text?.Trim() ?? "";
            WorkingDirectory = txtWorkingDirectory?.Text?.Trim() ?? "";
            Arguments = txtArguments?.Text?.Trim() ?? "";
            UseShellExecute = chkUseShellExecute?.Checked ?? true;
        }

        private void TxtCommandPath_TextChanged(object? sender, EventArgs e)
        {
            btnOK!.Enabled = !string.IsNullOrWhiteSpace(txtCommandPath?.Text);
            btnTest!.Enabled = !string.IsNullOrWhiteSpace(txtCommandPath?.Text);
        }

        private void ParseExistingCommand(string existingCommand)
        {
            if (string.IsNullOrWhiteSpace(existingCommand)) return;

            try
            {
                // Try to parse the existing command
                // Format: "path|workingDir|arguments|useShellExecute"
                var parts = existingCommand.Split('|');
                
                if (parts.Length > 0 && txtCommandPath != null)
                {
                    txtCommandPath.Text = parts[0];
                }
                if (parts.Length > 1 && txtWorkingDirectory != null)
                {
                    txtWorkingDirectory.Text = parts[1];
                }
                if (parts.Length > 2 && txtArguments != null)
                {
                    txtArguments.Text = parts[2];
                }
                if (parts.Length > 3 && bool.TryParse(parts[3], out var useShell) && chkUseShellExecute != null)
                {
                    chkUseShellExecute.Checked = useShell;
                }
            }
            catch
            {
                // If parsing fails, treat as simple command path
                if (txtCommandPath != null)
                {
                    txtCommandPath.Text = existingCommand;
                }
            }
        }

        public string BuildCommand()
        {
            var commandPath = txtCommandPath?.Text?.Trim() ?? "";
            var workingDir = txtWorkingDirectory?.Text?.Trim() ?? "";
            var arguments = txtArguments?.Text?.Trim() ?? "";
            var useShell = chkUseShellExecute?.Checked ?? true;

            // Format: "path|workingDir|arguments|useShellExecute"
            return $"{commandPath}|{workingDir}|{arguments}|{useShell}";
        }

        public static void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            try
            {
                var parts = command.Split('|');
                var commandPath = parts.Length > 0 ? parts[0] : "";
                var workingDir = parts.Length > 1 ? parts[1] : "";
                var arguments = parts.Length > 2 ? parts[2] : "";
                var useShell = parts.Length > 3 ? bool.Parse(parts[3]) : true;

                if (string.IsNullOrWhiteSpace(commandPath)) return;

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = commandPath,
                    UseShellExecute = useShell
                };

                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    startInfo.Arguments = arguments;
                }

                if (!string.IsNullOrWhiteSpace(workingDir) && Directory.Exists(workingDir))
                {
                    startInfo.WorkingDirectory = workingDir;
                }

                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing command: {ex.Message}", "Launch Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
