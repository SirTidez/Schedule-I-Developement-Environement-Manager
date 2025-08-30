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
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Load the application icon
            this.Icon = MainForm.LoadApplicationIcon();

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

            // OK button
            btnOk = new Button
            {
                Text = "OK - Branch Switched",
                Location = new Point(150, 220),
                Size = new Size(120, 30),
                DialogResult = DialogResult.OK
            };

            btnOk.Click += BtnOk_Click;

            // Cancel button
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(290, 220),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblMessage,
                lblCurrentBranch,
                lblNextBranch,
                lblInstructions,
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
    }
}
