using System.Drawing;

namespace ScheduleIDevelopementEnvironementManager.UI
{
    /// <summary>
    /// Modern UI theme system providing consistent colors, typography, and styling
    /// </summary>
    public static class ModernUITheme
    {
        /// <summary>
        /// Professional dark theme color palette
        /// </summary>
        public static class Colors
        {
            // Primary background colors - Dark VS Code inspired
            public static readonly Color BackgroundPrimary = Color.FromArgb(25, 25, 28);     // Main background
            public static readonly Color BackgroundSecondary = Color.FromArgb(35, 35, 40);   // Panel backgrounds
            public static readonly Color BackgroundTertiary = Color.FromArgb(45, 45, 50);    // Card backgrounds
            public static readonly Color BackgroundHover = Color.FromArgb(55, 55, 65);       // Hover states
            
            // Text colors with proper contrast ratios
            public static readonly Color TextPrimary = Color.FromArgb(240, 240, 240);        // Primary text
            public static readonly Color TextSecondary = Color.FromArgb(180, 180, 185);      // Secondary text
            public static readonly Color TextMuted = Color.FromArgb(120, 120, 130);          // Muted text
            public static readonly Color TextDisabled = Color.FromArgb(80, 80, 90);          // Disabled text
            
            // Accent colors for interactive elements
            public static readonly Color AccentPrimary = Color.FromArgb(0, 120, 215);        // Primary blue
            public static readonly Color AccentSecondary = Color.FromArgb(16, 110, 190);     // Darker blue
            public static readonly Color AccentSuccess = Color.FromArgb(16, 185, 129);       // Green success
            public static readonly Color AccentWarning = Color.FromArgb(245, 158, 11);       // Orange warning
            public static readonly Color AccentDanger = Color.FromArgb(239, 68, 68);         // Red error
            public static readonly Color AccentInfo = Color.FromArgb(59, 130, 246);          // Blue info
            
            // Border and separator colors
            public static readonly Color BorderPrimary = Color.FromArgb(65, 65, 75);         // Main borders
            public static readonly Color BorderSecondary = Color.FromArgb(50, 50, 60);       // Subtle borders
            public static readonly Color BorderFocus = AccentPrimary;                        // Focus indicators
            
            // Special purpose colors
            public static readonly Color LogBackground = Color.FromArgb(15, 15, 18);         // Console/log background
            public static readonly Color LogText = Color.FromArgb(0, 255, 0);                // Console text (green)
            public static readonly Color SelectionBackground = Color.FromArgb(51, 153, 255, 40); // Selection highlight (transparent)
        }
        
        /// <summary>
        /// Typography system with consistent font sizing and weights
        /// </summary>
        public static class Typography
        {
            // Heading fonts
            public static readonly Font HeadingLarge = new Font("Segoe UI", 18F, FontStyle.Bold);
            public static readonly Font HeadingMedium = new Font("Segoe UI", 14F, FontStyle.Bold);
            public static readonly Font HeadingSmall = new Font("Segoe UI", 12F, FontStyle.Bold);
            
            // Body fonts
            public static readonly Font BodyLarge = new Font("Segoe UI", 11F, FontStyle.Regular);
            public static readonly Font BodyMedium = new Font("Segoe UI", 10F, FontStyle.Regular);
            public static readonly Font BodySmall = new Font("Segoe UI", 9F, FontStyle.Regular);
            
            // Specialized fonts
            public static readonly Font ButtonLarge = new Font("Segoe UI", 10F, FontStyle.Bold);
            public static readonly Font ButtonMedium = new Font("Segoe UI", 9F, FontStyle.Bold);
            public static readonly Font ButtonSmall = new Font("Segoe UI", 8F, FontStyle.Bold);
            public static readonly Font MonospaceBody = new Font("Consolas", 9F, FontStyle.Regular);
            public static readonly Font MonospaceSmall = new Font("Consolas", 8F, FontStyle.Regular);
        }
        
        /// <summary>
        /// Button style variants for consistent theming
        /// </summary>
        public enum ButtonStyle
        {
            Primary,    // Main action button (blue)
            Secondary,  // Secondary action (gray)
            Success,    // Success action (green)
            Warning,    // Warning action (orange)
            Danger,     // Destructive action (red)
            Info        // Informational action (blue)
        }
        
        /// <summary>
        /// Layout constants for consistent spacing
        /// </summary>
        public static class Layout
        {
            public const int SpacingXSmall = 4;
            public const int SpacingSmall = 8;
            public const int SpacingMedium = 16;
            public const int SpacingLarge = 24;
            public const int SpacingXLarge = 32;
            
            public const int BorderRadius = 4;
            public const int BorderRadiusLarge = 8;
            
            public const int ButtonHeight = 32;
            public const int ButtonHeightLarge = 40;
            public const int InputHeight = 28;
            public const int InputHeightLarge = 36;
        }
    }
}