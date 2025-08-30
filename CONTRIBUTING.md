# Contributing to Schedule I Development Environment Manager

Thanks for your interest in contributing! This document describes how to set up your environment, our workflow, and expectations for code quality and reviews.

## Quick Start
- Prereqs: Windows 10/11, Visual Studio 2022, .NET 8 SDK, Steam installed.
- Clone the repo and open the solution: `Schedule I Developement Environement Manager.sln`.
- Build and run the WinForms app. On first run, you’ll see the setup flow.

## Development Environment
- IDE: Visual Studio 2022 (latest), with .NET desktop development workload.
- Target Framework: `net8.0-windows`.
- Logging/Config Locations:
  - Logs: `%LocalAppData%\\Schedule I Developer Env\\logs`
  - Config: `%LocalAppData%\\Schedule I Developer Env\\config\\dev_environment_config.json`

## Branching and Commits
- Main branch: stable; keep green. Feature work should be on topic branches.
- Branch naming: `feature/<short-desc>`, `fix/<short-desc>`, or `docs/<short-desc>`.
- Commits: small, focused, imperative mood. Reference issues when applicable.

## Coding Guidelines
- Style: Follow existing code style and patterns in the repository.
- UI: Maintain the dark theme, spacing, and control conventions seen in existing forms.
- Naming: Use descriptive names; avoid one-letter variables.
- Logging: Prefer the provided file logging; log errors/warnings with context.
- Exceptions: Catch, log, and show user-friendly messages where appropriate.
- Trimming: Remains disabled for WinForms/WPF compatibility.

## Testing and Validation
- Manual checks: Walk through the setup flow and managed dashboard.
- Steam integration: If available, validate library detection and manifest parsing.
- Smoke tests: Create a managed environment, copy at least one branch, and launch it.

## Submitting Changes
- Open a Pull Request with:
  - Summary of changes and rationale
  - Screenshots for UI changes (place files under `assets/` if possible)
  - Notes on testing performed
- Keep PRs small; larger changes should be split by area (services, UI, etc.).

## Reporting Issues
- Use clear titles and reproduction steps.
- Include logs from `%LocalAppData%\\Schedule I Developer Env\\logs` when relevant.
- Provide screenshots or the exact error messages.

## Code of Conduct
- By participating, you agree to uphold our CODE_OF_CONDUCT.md.
- Report unacceptable behavior by opening an issue or contacting the maintainers privately (add an email contact in your fork or deployment).

Happy hacking!

