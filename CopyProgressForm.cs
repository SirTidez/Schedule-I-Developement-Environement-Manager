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
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Status label
            lblStatus = new Label
            {
                Text = "Preparing to copy files...",
                Location = new Point(20, 20),
                Size = new Size(650, 25),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            // Console log textbox
            txtConsoleLog = new RichTextBox
            {
                Location = new Point(20, 60),
                Size = new Size(650, 320),
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(20, 400),
                Size = new Size(650, 23),
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            // Close button (initially disabled)
            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(300, 440),
                Size = new Size(100, 30),
                Enabled = false
            };

            btnClose.Click += BtnClose_Click;

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
    }
}
