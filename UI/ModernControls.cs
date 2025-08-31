using System.Drawing;
using System.Windows.Forms;

namespace ScheduleIDevelopementEnvironementManager.UI
{
    /// <summary>
    /// Factory class for creating consistently styled modern UI controls
    /// </summary>
    public static class ModernControls
    {
        /// <summary>
        /// Creates a modern action button with specified style
        /// </summary>
        public static Button CreateActionButton(string text, ModernUITheme.ButtonStyle style)
        {
            var button = new Button
            {
                Text = text,
                Font = ModernUITheme.Typography.ButtonMedium,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            
            // Apply style-specific colors
            switch (style)
            {
                case ModernUITheme.ButtonStyle.Primary:
                    button.BackColor = ModernUITheme.Colors.AccentPrimary;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderColor = ModernUITheme.Colors.AccentSecondary;
                    break;
                    
                case ModernUITheme.ButtonStyle.Secondary:
                    button.BackColor = ModernUITheme.Colors.BackgroundTertiary;
                    button.ForeColor = ModernUITheme.Colors.TextPrimary;
                    button.FlatAppearance.BorderColor = ModernUITheme.Colors.BorderPrimary;
                    break;
                    
                case ModernUITheme.ButtonStyle.Success:
                    button.BackColor = ModernUITheme.Colors.AccentSuccess;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderColor = Color.FromArgb(12, 150, 100);
                    break;
                    
                case ModernUITheme.ButtonStyle.Warning:
                    button.BackColor = ModernUITheme.Colors.AccentWarning;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderColor = Color.FromArgb(200, 130, 8);
                    break;
                    
                case ModernUITheme.ButtonStyle.Danger:
                    button.BackColor = ModernUITheme.Colors.AccentDanger;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderColor = Color.FromArgb(200, 55, 55);
                    break;
                    
                case ModernUITheme.ButtonStyle.Info:
                    button.BackColor = ModernUITheme.Colors.AccentInfo;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderColor = Color.FromArgb(45, 105, 200);
                    break;
            }
            
            button.FlatAppearance.BorderSize = 1;
            return button;
        }
        
        /// <summary>
        /// Creates a modern heading label
        /// </summary>
        public static Label CreateHeadingLabel(string text, bool large = false)
        {
            return new Label
            {
                Text = text,
                Font = large ? ModernUITheme.Typography.HeadingLarge : ModernUITheme.Typography.HeadingMedium,
                ForeColor = ModernUITheme.Colors.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true
            };
        }
        
        /// <summary>
        /// Creates a field label for form inputs
        /// </summary>
        public static Label CreateFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = ModernUITheme.Typography.BodyMedium,
                ForeColor = ModernUITheme.Colors.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true
            };
        }
        
        /// <summary>
        /// Creates a status label with optional color
        /// </summary>
        public static Label CreateStatusLabel(string text, Color? foreColor = null)
        {
            return new Label
            {
                Text = text,
                Font = ModernUITheme.Typography.BodySmall,
                ForeColor = foreColor ?? ModernUITheme.Colors.TextSecondary,
                BackColor = Color.Transparent,
                AutoSize = true
            };
        }
        
        /// <summary>
        /// Creates a modern text input control
        /// </summary>
        public static TextBox CreateModernTextBox(bool readOnly = false, string placeholder = "")
        {
            var textBox = new TextBox
            {
                Font = ModernUITheme.Typography.BodyMedium,
                BackColor = readOnly ? ModernUITheme.Colors.BackgroundSecondary : ModernUITheme.Colors.BackgroundTertiary,
                ForeColor = ModernUITheme.Colors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = readOnly
            };
            
            if (!string.IsNullOrEmpty(placeholder))
            {
                textBox.Text = placeholder;
                textBox.ForeColor = ModernUITheme.Colors.TextMuted;
            }
            
            return textBox;
        }

        /// <summary>
        /// Creates a modern checkbox control
        /// </summary>
        public static CheckBox CreateModernCheckBox(string text, bool isChecked = false)
        {
            return new CheckBox
            {
                Text = text,
                Font = ModernUITheme.Typography.BodyMedium,
                ForeColor = ModernUITheme.Colors.TextPrimary,
                BackColor = Color.Transparent,
                Checked = isChecked,
                UseVisualStyleBackColor = false,
                AutoSize = true
            };
        }
        
        /// <summary>
        /// Creates a section panel with optional title
        /// </summary>
        public static Panel CreateSectionPanel(string title, Size size)
        {
            var panel = new Panel
            {
                Size = size,
                BackColor = ModernUITheme.Colors.BackgroundSecondary,
                BorderStyle = BorderStyle.None
            };
            
            if (!string.IsNullOrEmpty(title))
            {
                var titleLabel = CreateHeadingLabel(title);
                titleLabel.Location = new Point(15, 10);
                titleLabel.Size = new Size(size.Width - 30, 25);
                panel.Controls.Add(titleLabel);
            }
            
            return panel;
        }
        
        /// <summary>
        /// Creates an information card with title and content
        /// </summary>
        public static Panel CreateInfoCard(string title, string content)
        {
            var card = new Panel
            {
                BackColor = ModernUITheme.Colors.BackgroundTertiary,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var titleLabel = CreateHeadingLabel(title, false);
            titleLabel.Location = new Point(15, 5);
            titleLabel.ForeColor = ModernUITheme.Colors.AccentPrimary;
            card.Controls.Add(titleLabel);
            
            if (!string.IsNullOrEmpty(content))
            {
                var contentLabel = new Label
                {
                    Text = content,
                    Font = ModernUITheme.Typography.BodySmall,
                    ForeColor = ModernUITheme.Colors.TextSecondary,
                    BackColor = Color.Transparent,
                    Location = new Point(15, 35),
                    AutoSize = true
                };
                card.Controls.Add(contentLabel);
            }
            
            return card;
        }
        
        /// <summary>
        /// Creates a modern progress bar
        /// </summary>
        public static ProgressBar CreateModernProgressBar()
        {
            return new ProgressBar
            {
                Style = ProgressBarStyle.Continuous,
                BackColor = ModernUITheme.Colors.BackgroundSecondary,
                ForeColor = ModernUITheme.Colors.AccentPrimary
            };
        }
    }
}