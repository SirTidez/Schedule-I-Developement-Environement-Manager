using System;
using System.Windows.Forms;
using System.Drawing;

namespace ScheduleIDevelopementEnvironementManager
{
    public partial class BranchSwitchPromptForm : Form
    {
        private Label lblMessage = null!;
        private Label lblCurrentBranch = null!;
        private Label lblNextBranch = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;

        public string CurrentBranch { get; private set; }
        public string NextBranch { get; private set; }
        public bool UserConfirmed { get; private set; } = false;

        public BranchSwitchPromptForm(string currentBranch, string nextBranch)
        {
            CurrentBranch = currentBranch;
            NextBranch = nextBranch;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Switch to Next Branch";
            this.Size = new Size(500, 450); // Increased height from 300 to 450
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Load the application icon
            this.Icon = MainForm.LoadApplicationIcon();

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

            // Set default button
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            UserConfirmed = true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                UserConfirmed = false;
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
