# Schedule I Development Environment Manager - Memories

## Build ID and Timestamp Storage Fix (2024-12-28)

### Issue Fixed
The config file was not properly saving build IDs and updated times together. The system was only storing build IDs as strings without individual timestamps for when each branch's build ID was detected.

### Solution Implemented
1. **Created new `BranchBuildInfo` model** (`Models/BranchBuildInfo.cs`):
   - Stores build ID and updated timestamp together
   - Provides array format methods for backward compatibility
   - Includes multiple constructors for flexibility

2. **Updated `DevEnvironmentConfig` structure**:
   - Changed `BranchBuildIds` from `Dictionary<string, string>` to `Dictionary<string, BranchBuildInfo>`
   - Added new methods: `GetBuildInfoForBranch()`, `GetBuildUpdatedTimeForBranch()`
   - Enhanced `SetBuildIdForBranch()` with timestamp support
   - Maintained backward compatibility for existing `GetBuildIdForBranch()` method

3. **Enhanced UI Display**:
   - Updated MainForm.cs to show both build ID and individual updated timestamps
   - Format: `branch-name: buildId (Updated: yyyy-MM-dd HH:mm:ss) [STATUS]`

### Data Structure Change
**Before:**
```json
"branchBuildIds": {
  "main-branch": "12345678",
  "beta-branch": "87654321"
}
```

**After:**
```json
"branchBuildIds": {
  "main-branch": {
    "buildId": "12345678",
    "updatedTime": "2024-12-28T10:30:00"
  },
  "beta-branch": {
    "buildId": "87654321", 
    "updatedTime": "2024-12-28T11:45:00"
  }
}
```

### Benefits
- Individual timestamps for each branch's build ID detection
- Ability to reference build ID and timestamp separately
- Better tracking of when each branch was last updated
- Maintains backward compatibility with existing code
- Proper array-like access through `ToArray()` method

### Files Modified
- `Models/BranchBuildInfo.cs` (new file)
- `Models/DevEnvironmentConfig.cs` (updated data structure and methods)
- `MainForm.cs` (updated UI display)

### Config Version Update and Migration (2024-12-28)

#### Version Update
- Updated `ConfigVersion` from "1.0" to "2.0" to reflect the new data structure

#### Migration System
1. **Added `DevEnvironmentConfigV1` class**: Legacy format for backward compatibility
2. **Added `MigrateFromV1()` method**: Converts old string-based build IDs to new `BranchBuildInfo` objects
3. **Enhanced `ConfigurationService.LoadConfigurationAsync()`**:
   - Automatically detects v1.0 configs (by version or JSON structure)
   - Gracefully migrates old configs to v2.0 format
   - Saves migrated config automatically
   - Preserves all existing data during migration

#### Migration Process
1. **Detection**: Checks `ConfigVersion` field or catches JSON deserialization errors
2. **Migration**: Uses old `LastUpdated` timestamp for all migrated build IDs
3. **Preservation**: All paths, branches, and build IDs are preserved
4. **Auto-save**: Migrated config is automatically saved in new format

### Testing
- All changes compile successfully
- Existing functionality preserved
- New timestamp tracking works correctly
- Migration system tested and functional

## Custom Launch Commands Feature (2024-12-28)

### Feature Overview
Implemented a comprehensive custom launch command system that allows users to set custom executables, batch files, PowerShell scripts, or any other launch method for each individual branch through a right-click context menu.

### Implementation Details

#### 1. Data Model Updates (`Models/DevEnvironmentConfig.cs`)
- **New Property**: `CustomLaunchCommands` - Dictionary mapping branch names to custom launch commands
- **New Methods**:
  - `GetCustomLaunchCommand(string branchName)` - Retrieves custom command for a branch
  - `SetCustomLaunchCommand(string branchName, string command)` - Sets/removes custom command
  - `HasCustomLaunchCommand(string branchName)` - Checks if branch has custom command
- **Migration Support**: Updated v1.0 to v2.0 migration to initialize empty CustomLaunchCommands

#### 2. Custom Launch Command Dialog (`Forms/CustomLaunchCommandDialog.cs`)
- **Purpose**: User-friendly dialog for setting up custom launch commands
- **Features**:
  - File browser for executable/script selection
  - Working directory specification
  - Command arguments input
  - Shell execute option (for .bat, .ps1, etc.)
  - Test functionality to verify commands work
  - Examples and tips for common use cases
  - Dark theme consistent with application

#### 3. Enhanced Context Menu (`ManagedEnvironmentLoadedForm.cs`)
- **Dynamic Menu**: Context menu changes based on whether custom command exists
- **Options Available**:
  - "🎯 Launch with Custom Command" (if custom command exists)
  - "🚀 Launch Game (Default)" (if custom command exists)
  - "⚙️ Set/Edit Custom Launch Command"
  - "❌ Remove Custom Launch Command" (if custom command exists)

#### 4. Enhanced UI Indicators
- **DataGridView Column**: Added "Launch" column showing "🎯 Custom" or "🚀 Default"
- **Launch Button Behavior**: Shows options dialog when custom command exists
- **Visual Feedback**: Clear indicators of which branches have custom commands

### Command Storage Format
Commands are stored as pipe-separated strings:
```
"path|workingDir|arguments|useShellExecute"
```

Example:
```
"C:\Scripts\launch_with_mods.bat|C:\Scripts|--debug|true"
```

### Use Cases Supported
- **Custom Executables**: Alternative game launchers, modded versions
- **Batch Files**: Complex launch scripts with environment setup
- **PowerShell Scripts**: Advanced automation and configuration
- **Steam URLs**: Direct Steam protocol launches
- **Development Tools**: IDEs, debuggers, profilers for that branch

### User Experience
1. **Right-click any branch** in the managed environment
2. **Select "Set Custom Launch Command"** from context menu
3. **Browse for executable/script** or enter path manually
4. **Configure working directory and arguments** as needed
5. **Test the command** before saving
6. **Launch with custom command** via context menu or main launch button

### Files Modified
- `Models/DevEnvironmentConfig.cs` (added CustomLaunchCommands support)
- `Forms/CustomLaunchCommandDialog.cs` (new dialog form)
- `ManagedEnvironmentLoadedForm.cs` (enhanced context menu and launch functionality)

### Configuration File Impact
```json
{
  "customLaunchCommands": {
    "main-branch": "C:\\Scripts\\launch_main.bat||--debug|true",
    "beta-branch": "C:\\Tools\\DebugLauncher.exe|C:\\Tools|--branch beta|false"
  }
}
```
