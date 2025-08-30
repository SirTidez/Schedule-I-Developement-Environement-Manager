using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace ScheduleIDevelopementEnvironementManager
{
    public partial class CopyProgressForm : Form
    {
        private RichTextBox txtConsoleLog = null!;
        private ProgressBar progressBar = null!;
        private Label lblStatus = null!;
        private Button btnClose = null!;
        private bool isCopyComplete = false;

        public CopyProgressForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Copy Operation Progress";
            this.Size = new Size(900, 650); // Increased width from 700 to 900, height from 550 to 650
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Load the application icon
            this.Icon = MainForm.LoadApplicationIcon();

            // Apply dark theme to the form
            ApplyDarkTheme();

            // Status label
            lblStatus = new Label
            {
                Text = "Preparing to copy files...",
                Location = new Point(20, 20),
                Size = new Size(850, 25), // Increased width from 650 to 850
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            // Console log textbox with horizontal and vertical scrolling
            txtConsoleLog = new RichTextBox
            {
                Location = new Point(20, 60),
                Size = new Size(850, 420), // Increased width from 650 to 850, height from 320 to 420
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                ScrollBars = RichTextBoxScrollBars.Both, // Changed from Vertical to Both for horizontal and vertical scrolling
                WordWrap = false // Disable word wrap to enable horizontal scrolling
            };

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(20, 500), // Moved down from 400 to 500
                Size = new Size(850, 23), // Increased width from 650 to 850
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            // Cancel button (initially disabled)
            btnClose = new Button
            {
                Text = "Cancel",
                Location = new Point(400, 550), // Moved down from 440 to 550, centered for wider form
                Size = new Size(100, 30),
                Enabled = false
            };

            btnClose.Click += BtnClose_Click;

            // Apply dark theme to controls (except console output)
            ApplyDarkThemeToControl(lblStatus);
            ApplyDarkThemeToControl(progressBar);
            ApplyDarkThemeToControl(btnClose);

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblStatus,
                txtConsoleLog,
                progressBar,
                btnClose
            });
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        public void LogMessage(string message)
        {
            if (txtConsoleLog.InvokeRequired)
            {
                txtConsoleLog.Invoke(new Action<string>(LogMessage), message);
                return;
            }

            txtConsoleLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            txtConsoleLog.ScrollToCaret();
        }

        public void UpdateProgress(int value)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action<int>(UpdateProgress), value);
                return;
            }

            progressBar.Value = Math.Min(value, 100);
        }

        public void UpdateStatus(string status)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action<string>(UpdateStatus), status);
                return;
            }

            lblStatus.Text = status;
        }

        public void SetCopyComplete()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(SetCopyComplete));
                return;
            }

            isCopyComplete = true;
            btnClose.Enabled = true;
            btnClose.Text = "Close"; // Change from Cancel to Close when complete
            lblStatus.Text = "Copy operation completed successfully!";
            lblStatus.ForeColor = Color.Green;
        }

        public void SetCopyFailed(string errorMessage)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(SetCopyFailed), errorMessage);
                return;
            }

            isCopyComplete = true;
            btnClose.Enabled = true;
            btnClose.Text = "Close"; // Change from Cancel to Close when failed
            lblStatus.Text = $"Copy operation failed: {errorMessage}";
            lblStatus.ForeColor = Color.Red;
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

                case Button button:
                    button.BackColor = Color.FromArgb(0, 122, 204); // Professional blue
                    button.ForeColor = Color.White;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = Color.FromArgb(0, 100, 180);
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 140, 230);
                    button.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 100, 180);
                    break;

                case ProgressBar progressBar:
                    progressBar.BackColor = Color.FromArgb(30, 30, 30);
                    progressBar.ForeColor = Color.FromArgb(0, 122, 204);
                    break;
            }

            // Recursively apply to child controls
            foreach (Control childControl in control.Controls)
            {
                ApplyDarkThemeToControl(childControl);
            }
        }

        #endregion
    }
}
