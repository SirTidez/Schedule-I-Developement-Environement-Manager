using System.ComponentModel;
using ScheduleIDevelopementEnvironementManager.Services;
using Microsoft.Extensions.Logging;
using System.IO;

namespace ScheduleIDevelopementEnvironementManager
{
    public partial class SteamLibrarySelectionDialog : Form
    {
        private readonly SteamService _steamService;
        private readonly ILogger _logger;
        private List<string> _libraryPaths;
        private string _selectedLibraryPath = string.Empty;

        public string SelectedLibraryPath => _selectedLibraryPath;

        public SteamLibrarySelectionDialog(SteamService steamService, ILogger logger)
        {
            _steamService = steamService;
            _logger = logger;
            _libraryPaths = new List<string>();
            
            InitializeForm();
            LoadLibraries();
        }

        private void InitializeForm()
        {
            this.Text = "Select Steam Library";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            CreateControls();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            // Title label
            var lblTitle = new Label
            {
                Text = "Multiple Steam libraries detected. Please select the library containing Schedule I:",
                Location = new Point(20, 20),
                Size = new Size(540, 40),
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Library list
            lstLibraries = new ListBox
            {
                Location = new Point(20, 70),
                Size = new Size(540, 200),
                Font = new Font(this.Font.FontFamily, 9)
            };

            // Status label
            lblStatus = new Label
            {
                Text = "",
                Location = new Point(20, 280),
                Size = new Size(540, 20),
                ForeColor = Color.Blue
            };

            // Buttons
            btnSelect = new Button
            {
                Text = "Select Library",
                Location = new Point(400, 320),
                Size = new Size(80, 30),
                Enabled = false
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(490, 320),
                Size = new Size(80, 30)
            };

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblTitle, lstLibraries, lblStatus, btnSelect, btnCancel
            });

            // Set dialog result buttons
            this.AcceptButton = btnSelect;
            this.CancelButton = btnCancel;
        }

        private void SetupEventHandlers()
        {
            lstLibraries!.SelectedIndexChanged += LstLibraries_SelectedIndexChanged;
            btnSelect!.Click += BtnSelect_Click;
            btnCancel!.Click += BtnCancel_Click;
        }

        private void LoadLibraries()
        {
            try
            {
                _logger.LogInformation("Loading Steam libraries for selection dialog");
                lblStatus!.Text = "Loading Steam libraries...";

                _libraryPaths = _steamService.GetSteamLibraryPaths();
                
                if (_libraryPaths.Count == 0)
                {
                    lblStatus!.Text = "No Steam libraries found.";
                    lblStatus!.ForeColor = Color.Red;
                    return;
                }

                // Populate the list with library information
                foreach (var libraryPath in _libraryPaths)
                {
                    var drive = Path.GetPathRoot(libraryPath);
                    var isCDrive = drive?.ToUpper() == "C:\\";
                    var displayText = $"{libraryPath} {(isCDrive ? "(C: Drive - Recommended)" : "")}";
                    
                    lstLibraries!.Items.Add(displayText);
                }

                // Select the first library by default (should be C: drive if available)
                if (lstLibraries!.Items.Count > 0)
                {
                    lstLibraries.SelectedIndex = 0;
                }

                lblStatus!.Text = $"Found {_libraryPaths.Count} Steam library(ies). Select one to continue.";
                lblStatus!.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Steam libraries");
                lblStatus!.Text = $"Error loading libraries: {ex.Message}";
                lblStatus!.ForeColor = Color.Red;
            }
        }

        private void LstLibraries_SelectedIndexChanged(object? sender, EventArgs e)
        {
            btnSelect!.Enabled = lstLibraries!.SelectedIndex >= 0;
        }

        private void BtnSelect_Click(object? sender, EventArgs e)
        {
            if (lstLibraries!.SelectedIndex >= 0 && lstLibraries.SelectedIndex < _libraryPaths.Count)
            {
                _selectedLibraryPath = _libraryPaths[lstLibraries.SelectedIndex];
                _logger.LogInformation("User selected Steam library: {Path}", _selectedLibraryPath);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            _logger.LogInformation("User cancelled library selection");
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Control declarations
        private ListBox? lstLibraries;
        private Label? lblStatus;
        private Button? btnSelect;
        private Button? btnCancel;
    }
}
