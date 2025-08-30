# Schedule I Development Environment Manager

A Windows desktop tool (WinForms) to create and manage a local, “managed” development environment for the Steam game Schedule I (AppID 3164500). It detects Steam libraries and branches, guides you through copying per-branch installations, and provides a dashboard to launch, update, and maintain branch builds with clear progress and logging.

## Highlights
- Steam integration: Detects Steam install and libraries, reads `appmanifest_*.acf`, and prioritizes C: drive.
- Branch management: Tracks branches (main, beta, alternate, etc.), build IDs, and update status.
- Guided workflow: Prompts for Steam branch switches and verifies via manifest/build ID before copying.
- Managed environment: Copies per-branch game files into a dedicated folder structure you control.
- Dark-themed UI: Consistent, accessible dark styling across all forms and dialogs.
- File logging: Persistent logs written to `%LocalAppData%\Schedule I Developer Env\logs`.
- Config persistence: Settings saved to `%LocalAppData%\Schedule I Developer Env\config\dev_environment_config.json`.

## Main Forms & Dialogs
- MainForm: Setup screen shown when no managed environment is configured. Detects Steam, libraries, and guides initial setup.
- ManagedEnvironmentLoadedForm: Primary dashboard when a managed environment exists. Lists branches, size/file counts, local vs Steam build IDs, launch/delete/open actions, and status strip with refresh.
- CreateManagedEnvironmentForm: Wizard for creating the managed environment directory and selecting branches to copy.
- BranchSelectionDialog (Forms/BranchSelectionDialog.cs): Choose which branches to add; shows info for selected branch and supports select-all/none.
- BranchSwitchPromptForm: Guides switching branches in Steam between copy operations; includes safety note to wait for downloads to complete.
- CopyProgressForm: Console-like real‑time copy log with progress bar, status text, and completion gating.
- SteamLibrarySelectionDialog: Appears when multiple Steam libraries are detected; lets you pick the one containing Schedule I.

## Architecture Overview
- Models
  - DevEnvironmentConfig: Paths, selected branches, branch → build ID map, timestamps, version.
  - BranchInfo: Per-branch paths, size/file counts, local/Steam build IDs, status, and display helpers.
  - SteamGameInfo: Steam app metadata and resolved install/library paths.
- Services
  - SteamService: Finds Steam install/libraries, parses manifests, resolves current branch/build ID, and validates branch names.
  - BranchManagementService: Aggregates `BranchInfo`, computes directory stats, compares build IDs, and exposes operations (launch/delete/update metadata).
  - ConfigurationService: Reads/writes config JSON under `%LocalAppData%` and exposes convenience accessors.
  - FileOperationsService: Launches executables, deletes directories, opens Explorer, counts files and sizes, file-in-use checks.
  - FileLoggingService (+ Provider/Factory): Writes timestamped log lines to rotating files under `%LocalAppData%`.

## Key User Flows
- First‑run setup
  - App detects absence of a managed environment and shows MainForm.
  - If multiple Steam libraries exist, SteamLibrarySelectionDialog appears to choose the correct library.
  - CreateManagedEnvironmentForm then collects managed environment path and desired branches.
- Branch copy loop
  - For each selected branch: show CopyProgressForm, prompt to switch the branch in Steam via BranchSwitchPromptForm, verify via manifest/build ID, then copy into the managed environment.
- Managed dashboard
  - ManagedEnvironmentLoadedForm lists all branches (installed and available), indicates current Steam branch, and exposes actions to launch, refresh status, open folder, and remove branches.

## Design Notes and History (memories/)
The `memories/` directory documents design decisions and UI/UX refinements. Useful references include:
- memories/managed-environment-creation-workflow-memory.md: End‑to‑end creation flow, CopyProgressForm and BranchSwitchPromptForm roles, verification steps.
- memories/managed-environment-detection-and-setup-ui-memory.md: Startup logic that decides between setup vs. dashboard.
- memories/steam-manifest-branch-detection-memory.md and memories/buildid-parsing-fix-memory.md: Robust Steam manifest parsing and build ID handling.
- memories/multiple-steam-library-detection-memory.md and memories/steam-information-loading-and-button-enabling-fix-memory.md: Multi‑library handling and safer UI enable/disable.
- memories/sophisticated-branch-management-interface-memory.md and memories/managed-environment-as-main-screen-memory.md: Dashboard layout, status strip, refresh, and action panels.
- memories/copy-progress-form-ui-improvement-memory.md and memories/copy-operation-warning-and-ui-improvements-memory.md: Progress UX improvements and safety prompts.
- memories/dark-theme-implementation-memory.md, memories/ui-spacing-and-sizing-fixes-memory.md, memories/button-styling-standardization-memory.md: Dark theme and visual consistency.
- memories/updated-folder-structure-and-logging-memory.md and memories/logger-warning-fix-memory.md: Logging and app data folder structure.

Browse the full directory for other targeted improvements: branch timing fixes, alignment tweaks, setup flow refinements, etc.

## Requirements
- Windows 10/11
- Steam installed (Schedule I, AppID 3164500)
- .NET 8 SDK (for building) or the published single‑file runtime if using releases
- Visual Studio 2022 (recommended) or `dotnet` CLI

## Setup and Usage
- First launch
  - Run the app. If no managed environment is configured, MainForm appears.
  - Select your Steam library (if prompted), confirm the game path, and choose a managed environment folder.
  - Pick branches to include (e.g., main‑branch, beta‑branch). The app guides you to switch branches in Steam as each one is copied.
- Daily workflow
  - Launch the app to view ManagedEnvironmentLoadedForm.
  - Review branch statuses (UpToDate / UpdateAvailable / NotInstalled) and current Steam branch.
  - Use actions to launch a branch, open its folder, refresh status, or delete a managed copy.

## Screenshots
The following screenshots illustrate the primary flows and UI. They are shown in chronological order:

![Screenshot 001158](assets/Screenshot%202025-08-30%20001158.png)
_First‑run setup screen (MainForm) prompting to configure a managed environment._

![Screenshot 001210](assets/Screenshot%202025-08-30%20001210.png)
_SteamLibrarySelectionDialog: selecting the Steam library that contains Schedule I when multiple libraries are detected._

![Screenshot 001241](assets/Screenshot%202025-08-30%20001241.png)
_CreateManagedEnvironmentForm: choosing the managed environment folder and initial options._

![Screenshot 001258](assets/Screenshot%202025-08-30%20001258.png)
_BranchSelectionDialog: selecting which branches (main/beta/alternate) to include in the managed environment._

![Screenshot 001543](assets/Screenshot%202025-08-30%20001543.png)
_BranchSwitchPromptForm: guidance to switch branches in Steam before each copy step, with caution to wait for downloads._

![Screenshot 001554](assets/Screenshot%202025-08-30%20001554.png)
_CopyProgressForm: console‑style progress view while copying files into the branch folder._

![Screenshot 001845](assets/Screenshot%202025-08-30%20001845.png)
_ManagedEnvironmentLoadedForm: branch dashboard showing statuses, file counts, sizes, and available actions._

## Build and Publish
- Visual Studio
  - Right‑click the project > Publish > choose profile `SingleFile-win-x64` > Publish.
  - Output: `bin\\Release\\publish\\win-x64-single\\Schedule I Developement Environement Manager.exe`
- CLI
  - `dotnet publish -c Release -r win-x64`
  - Produces a single‑file, self‑contained EXE (see `.csproj` for settings). Change `RuntimeIdentifier` to `win-x86` or `win-arm64` if needed.

Notes
- Single‑file packaging occurs on publish, not normal build.
- Native libraries may extract to a temp folder for compatibility.
- Trimming is disabled by default for WinForms/WPF safety.

## Configuration and Logs
- Config file: `%LocalAppData%\\Schedule I Developer Env\\config\\dev_environment_config.json`
- Logs folder: `%LocalAppData%\\Schedule I Developer Env\\logs` (files named like `dd-MM-yy HH-mm.log`)

## Contributing
- See CONTRIBUTING.md for guidelines (environment setup, branching, PRs, and code style) and CODE_OF_CONDUCT.md for community standards.
- Open with Visual Studio 2022, ensure .NET 8 SDK is installed.
- Code style mirrors current patterns: WinForms with explicit layout and a consistent dark theme.
- Please keep UI strings and behaviors aligned with the design decisions captured under `memories/`.

## License
This project is licensed under the MIT License. See `LICENSE` for details.
