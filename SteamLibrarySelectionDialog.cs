using System.ComponentModel;
using ScheduleIDevelopementEnvironementManager.Services;
using ScheduleIDevelopementEnvironementManager.UI;
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
        
        // Modern UI Controls
        private Panel? _libraryContainer;
        private List<Panel> _libraryCards;
        private Panel? _selectedCard;

        public string SelectedLibraryPath => _selectedLibraryPath;

        public SteamLibrarySelectionDialog(SteamService steamService, ILogger logger)
        {
            _steamService = steamService;
            _logger = logger;
            _libraryPaths = new List<string>();
            _libraryCards = new List<Panel>();
            
            // Initialize diagnostics system
            FormDiagnostics.Initialize(_logger);
            FormDiagnostics.LogFormInitialization("SteamLibrarySelectionDialog");
            
            InitializeModernForm();
            LoadLibraries(); // Load libraries synchronously
            
            FormDiagnostics.LogFormLoadComplete("SteamLibrarySelectionDialog");
        }

        private void InitializeModernForm()
        {
            FormDiagnostics.StartPerformanceTracking("ModernLibraryDialog_Initialization");
            
            this.Text = "🏛️ Steam Library Selection - Choose Development Library";
            this.Size = new Size(900, 620); // Increased height to accommodate new spacing
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = ModernUITheme.Colors.BackgroundPrimary;
            
            // Load the application icon
            this.Icon = MainForm.LoadApplicationIcon();

            CreateModernCardLayout();
            SetupModernEventHandlers();
            
            FormDiagnostics.EndPerformanceTracking("ModernLibraryDialog_Initialization");
        }

        private void CreateModernCardLayout()
        {
            FormDiagnostics.LogUserInteraction("CreateModernCardLayout", "SteamLibrarySelectionDialog");
            FormDiagnostics.StartPerformanceTracking("CardLayout_Creation");
            
            // Main container
            var mainPanel = new Panel();
            mainPanel.Size = new Size(850, 550);
            mainPanel.Location = new Point(25, 25);
            mainPanel.BackColor = ModernUITheme.Colors.BackgroundPrimary;

            // Header section
            var headerPanel = ModernControls.CreateSectionPanel("", new Size(850, 90));
            headerPanel.Location = new Point(0, 0);
            headerPanel.BackColor = ModernUITheme.Colors.BackgroundSecondary;

            // Title and description
            var titleLabel = ModernControls.CreateHeadingLabel("🏛️ Steam Library Selection", true);
            titleLabel.Location = new Point(15, 15);
            titleLabel.Size = new Size(650, 30);

            var descLabel = ModernControls.CreateStatusLabel("Multiple Steam libraries detected. Select the library containing Schedule I.", ModernUITheme.Colors.TextSecondary);
            descLabel.Location = new Point(15, 50);
            descLabel.Size = new Size(820, 25);

            headerPanel.Controls.AddRange(new Control[] { titleLabel, descLabel });

            // Library cards container (scrollable)
            _libraryContainer = new Panel();
            _libraryContainer.Size = new Size(850, 350);
            _libraryContainer.Location = new Point(0, 110);
            _libraryContainer.BackColor = ModernUITheme.Colors.BackgroundPrimary;
            _libraryContainer.AutoScroll = true;

            // Status panel
            var statusPanel = new Panel();
            statusPanel.Size = new Size(850, 90);
            statusPanel.Location = new Point(0, 470);
            statusPanel.BackColor = ModernUITheme.Colors.BackgroundSecondary;

            // Status label
            lblStatus = ModernControls.CreateStatusLabel("🔍 Scanning Steam libraries...", ModernUITheme.Colors.AccentInfo);
            lblStatus.Location = new Point(15, 25);
            lblStatus.Size = new Size(550, 25);

            // Action buttons
            btnSelect = ModernControls.CreateActionButton("✅ Select Library", ModernUITheme.ButtonStyle.Primary);
            btnSelect.Location = new Point(580, 25);
            btnSelect.Size = new Size(150, 40);
            btnSelect.Enabled = false;

            btnCancel = ModernControls.CreateActionButton("❌ Cancel", ModernUITheme.ButtonStyle.Secondary);
            btnCancel.Location = new Point(750, 25);
            btnCancel.Size = new Size(85, 40);

            statusPanel.Controls.AddRange(new Control[] { lblStatus, btnSelect, btnCancel });

            // Add all panels to main panel
            mainPanel.Controls.AddRange(new Control[] { headerPanel, _libraryContainer, statusPanel });

            // Add main panel to form
            this.Controls.Add(mainPanel);

            // Set dialog result buttons
            this.AcceptButton = btnSelect;
            this.CancelButton = btnCancel;

            FormDiagnostics.LogBulkThemeApplication("SteamLibrarySelectionDialog", 8, 8);
            FormDiagnostics.EndPerformanceTracking("CardLayout_Creation");
        }

        private void SetupModernEventHandlers()
        {
            FormDiagnostics.LogUserInteraction("SetupEventHandlers", "SteamLibrarySelectionDialog");
            
            if (btnSelect != null)
            {
                btnSelect.Click += BtnSelect_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "SelectButton");
            }
            
            if (btnCancel != null)
            {
                btnCancel.Click += BtnCancel_Click;
                FormDiagnostics.LogUserInteraction("EventHandlerAttached", "CancelButton");
            }
        }

        private void LoadLibraries()
        {
            try
            {
                FormDiagnostics.StartPerformanceTracking("LoadLibraries");
                _logger.LogInformation("Loading Steam libraries for modern card-based selection dialog");
                
                if (lblStatus != null)
                {
                    lblStatus.Text = "🔍 Scanning Steam libraries...";
                    lblStatus.ForeColor = ModernUITheme.Colors.AccentInfo;
                }

                var rawLibraryPaths = _steamService.GetSteamLibraryPaths();
                
                _logger.LogInformation("Raw Steam library paths found: {Count}", rawLibraryPaths.Count);
                for (int i = 0; i < rawLibraryPaths.Count; i++)
                {
                    _logger.LogDebug("Raw library path {Index}: '{Path}'", i, rawLibraryPaths[i]);
                }
                
                // Remove duplicates by normalizing for comparison but keep original paths
                var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _libraryPaths = rawLibraryPaths
                    .Where(path => 
                    {
                        var normalizedPath = Path.GetFullPath(path.TrimEnd('\\', '/'));
                        return seenPaths.Add(normalizedPath);
                    })
                    .ToList();
                    
                _logger.LogInformation("Deduplicated Steam library paths: {Count} (removed {Duplicates} duplicates)", 
                    _libraryPaths.Count, rawLibraryPaths.Count - _libraryPaths.Count);
                for (int i = 0; i < _libraryPaths.Count; i++)
                {
                    _logger.LogDebug("Deduplicated library path {Index}: '{Path}'", i, _libraryPaths[i]);
                }
                
                if (_libraryPaths.Count == 0)
                {
                    if (lblStatus != null)
                    {
                        lblStatus.Text = "❌ No Steam libraries found.";
                        lblStatus.ForeColor = ModernUITheme.Colors.AccentDanger;
                    }
                    return;
                }

                // Create library cards
                CreateLibraryCards();

                if (lblStatus != null)
                {
                    lblStatus.Text = $"✅ Found {_libraryPaths.Count} Steam library(ies). Select one to continue.";
                    lblStatus.ForeColor = ModernUITheme.Colors.AccentSuccess;
                }
                
                FormDiagnostics.EndPerformanceTracking("LoadLibraries");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Steam libraries");
                if (lblStatus != null)
                {
                    lblStatus.Text = $"❌ Error loading libraries: {ex.Message}";
                    lblStatus.ForeColor = ModernUITheme.Colors.AccentDanger;
                }
                FormDiagnostics.EndPerformanceTracking("LoadLibraries");
            }
        }

        private void CreateLibraryCards()
        {
            FormDiagnostics.LogUserInteraction("CreateLibraryCards", "SteamLibrarySelectionDialog", _libraryPaths.Count);
            
            if (_libraryContainer == null) return;
            
            _libraryContainer.Controls.Clear();
            _libraryCards.Clear();

            int yPosition = 20;
            
            for (int i = 0; i < _libraryPaths.Count; i++)
            {
                var libraryPath = _libraryPaths[i];
                var card = CreateLibraryCard(libraryPath, i, yPosition);
                _libraryCards.Add(card);
                _libraryContainer.Controls.Add(card);
                
                yPosition += 120; // Card height + spacing
            }
            
            // Auto-select first library (usually C: drive)
            if (_libraryCards.Count > 0)
            {
                SelectLibraryCard(_libraryCards[0]);
            }
        }

        private Panel CreateLibraryCard(string libraryPath, int index, int yPosition)
        {
            // Use the original path as-is for both display and service calls
            var displayPath = libraryPath;
            var servicePath = libraryPath;
            
            var drive = Path.GetPathRoot(libraryPath);
            var isCDrive = drive?.ToUpper() == "C:\\";
            var isRecommended = isCDrive;
            
            // Get drive info
            var driveInfo = "";
            var spaceInfo = "";
            try
            {
                var driveInfoObj = new DriveInfo(drive ?? "C:");
                var freeSpaceGB = driveInfoObj.AvailableFreeSpace / (1024 * 1024 * 1024);
                var totalSpaceGB = driveInfoObj.TotalSize / (1024 * 1024 * 1024);
                spaceInfo = $"{freeSpaceGB:F1} GB free of {totalSpaceGB:F1} GB";
                driveInfo = $"Drive: {driveInfoObj.DriveType}";
            }
            catch
            {
                spaceInfo = "Space info unavailable";
                driveInfo = "Drive info unavailable";
            }

            var card = new Panel();
            card.Size = new Size(820, 100);
            card.Location = new Point(15, yPosition);
            card.BackColor = ModernUITheme.Colors.BackgroundSecondary;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Cursor = Cursors.Hand;
            card.Tag = index;

            // Recommended badge
            if (isRecommended)
            {
                var badge = ModernControls.CreateStatusLabel("⭐ RECOMMENDED", ModernUITheme.Colors.AccentWarning);
                badge.Location = new Point(670, 10);
                badge.Size = new Size(130, 20);
                badge.Font = ModernUITheme.Typography.ButtonSmall;
                card.Controls.Add(badge);
            }

            // Main path label
            var pathLabel = ModernControls.CreateFieldLabel($"📁 {displayPath}");
            pathLabel.Location = new Point(15, 10);
            pathLabel.Size = new Size(640, 25);
            pathLabel.Font = ModernUITheme.Typography.BodyLarge;
            card.Controls.Add(pathLabel);

            // Drive info
            var driveLabel = ModernControls.CreateStatusLabel(driveInfo, ModernUITheme.Colors.TextSecondary);
            driveLabel.Location = new Point(15, 35);
            driveLabel.Size = new Size(250, 20);
            card.Controls.Add(driveLabel);

            // Space info
            var spaceLabel = ModernControls.CreateStatusLabel($"💾 {spaceInfo}", ModernUITheme.Colors.TextSecondary);
            spaceLabel.Location = new Point(15, 60);
            spaceLabel.Size = new Size(350, 20);
            card.Controls.Add(spaceLabel);

            // Game count (if available)
            var gameCountLabel = ModernControls.CreateStatusLabel("🎮 Scanning games...", ModernUITheme.Colors.TextSecondary);
            gameCountLabel.Location = new Point(400, 45);
            gameCountLabel.Size = new Size(250, 20);
            card.Controls.Add(gameCountLabel);

            // Selection indicator
            var selectionIndicator = new Panel();
            selectionIndicator.Size = new Size(5, 100);
            selectionIndicator.Location = new Point(0, 0);
            selectionIndicator.BackColor = Color.Transparent;
            card.Controls.Add(selectionIndicator);

            // Add click handler
            card.Click += (s, e) => SelectLibraryCard(card);
            foreach (Control control in card.Controls)
            {
                control.Click += (s, e) => SelectLibraryCard(card);
            }

            // Async load game count
            _ = Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Starting game scan for library: '{Path}'", servicePath);
                    
                    // Check if the library path exists
                    if (!Directory.Exists(servicePath))
                    {
                        _logger.LogWarning("Library path does not exist: '{Path}'", servicePath);
                        this.Invoke((Action)(() =>
                        {
                            if (this.IsDisposed) return;
                            gameCountLabel.Text = "🎮 Path not found";
                            gameCountLabel.ForeColor = ModernUITheme.Colors.AccentDanger;
                        }));
                        return;
                    }
                    
                    var games = _steamService.GetSteamGames(servicePath);
                    var scheduleIGame = games.FirstOrDefault(g => g.AppId == "3164500");
                    
                    _logger.LogInformation("Game scan completed for '{Path}': {Count} games found, Schedule I: {Found}", 
                        servicePath, games.Count, scheduleIGame != null);
                    
                    this.Invoke((Action)(() =>
                    {
                        if (this.IsDisposed) return; // Check if form is disposed
                        
                        gameCountLabel.Text = scheduleIGame != null ? 
                            "🎯 Schedule I detected!" : 
                            $"🎮 {games.Count} games found";
                        gameCountLabel.ForeColor = scheduleIGame != null ? 
                            ModernUITheme.Colors.AccentSuccess : 
                            ModernUITheme.Colors.TextSecondary;
                    }));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scanning games in library: '{Path}'", servicePath);
                    this.Invoke((Action)(() =>
                    {
                        if (this.IsDisposed) return; // Check if form is disposed
                        gameCountLabel.Text = "🎮 Games: Error scanning";
                        gameCountLabel.ForeColor = ModernUITheme.Colors.AccentDanger;
                    }));
                }
            });

            return card;
        }

        private void SelectLibraryCard(Panel card)
        {
            FormDiagnostics.LogUserInteraction("SelectLibraryCard", "SteamLibrarySelectionDialog", card.Tag);
            
            // Deselect previous card
            if (_selectedCard != null)
            {
                _selectedCard.BackColor = ModernUITheme.Colors.BackgroundSecondary;
                var prevIndicator = _selectedCard.Controls[_selectedCard.Controls.Count - 1];
                prevIndicator.BackColor = Color.Transparent;
            }

            // Select new card
            _selectedCard = card;
            card.BackColor = ModernUITheme.Colors.BackgroundHover;
            var indicator = card.Controls[card.Controls.Count - 1];
            indicator.BackColor = ModernUITheme.Colors.AccentPrimary;

            // Update selected path
            var index = (int)card.Tag!;
            _selectedLibraryPath = _libraryPaths[index];
            
            // Enable select button
            if (btnSelect != null)
            {
                btnSelect.Enabled = true;
                FormDiagnostics.LogButtonStateChange("SelectButton", true, "Library card selected");
            }
        }

        private void BtnSelect_Click(object? sender, EventArgs e)
        {
            FormDiagnostics.LogUserInteraction("SelectButtonClick", "SteamLibrarySelectionDialog", _selectedLibraryPath);
            
            if (!string.IsNullOrEmpty(_selectedLibraryPath))
            {
                _logger.LogInformation("User selected Steam library: {Path}", _selectedLibraryPath);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            FormDiagnostics.LogUserInteraction("CancelButtonClick", "SteamLibrarySelectionDialog");
            _logger.LogInformation("User cancelled library selection");
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Control declarations for modern card-based layout
        private Label? lblStatus;
        private Button? btnSelect;
        private Button? btnCancel;
    }
}
