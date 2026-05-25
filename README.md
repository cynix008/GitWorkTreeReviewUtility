# Git Worktree Review Utility

A .NET 9 console app for creating and removing Git worktrees used during code review.

This utility helps developers review branches without switching away from their current working branch. It creates a separate Git worktree for the branch being reviewed, opens it in a configured editor such as VS Code or Windsurf, and later removes the worktree when the review is complete.

---

## Features

- Select a configured repository from `repos.json`.
- Fetch the configured main branch and selected source branch from `origin`.
- Create a review worktree from the remote source branch.
- Open the created worktree in the configured review tool, usually VS Code or Windsurf.
- Remove review worktrees with `git worktree remove --force`.
- Delete only local branches that start with `review/`.
- Keep the main repository branch unchanged.
- Support dry-run mode.
- Detect dirty worktrees before removal.
- Use colored console output for easier scanning.
- Support Windows and macOS configuration.

---

## Why This Tool Exists

When reviewing pull requests or feature branches, developers often need to check out another branch. Doing that inside the main repository can be disruptive because:

- your current branch may have uncommitted work
- switching branches can fail because of local changes
- long branch names can create messy folder names
- cleanup of temporary review branches and worktrees can be repetitive
- opening the correct review folder manually takes extra steps

This utility solves that by using `git worktree`.

It creates a separate working folder for the review branch while leaving your main repository untouched.

---

## Requirements

### Windows

- .NET 9 SDK
- Git for Windows
- VS Code or Windsurf
- Git Credential Manager configured if using HTTPS remotes

Check your setup:

```powershell
dotnet --version
git --version
where code
where code.cmd
where windsurf
where windsurf.cmd
```

### macOS

- .NET 9 SDK
- Git
- VS Code or Windsurf
- Git credentials configured through Git Credential Manager, SSH, or macOS Keychain

Check your setup:

```bash
dotnet --version
git --version
which code
which windsurf
```

---

## Configuration Files

The app reads repository settings from:

```text
repos.json
```

The project file copies `repos.json` to the output directory during build.

Recommended professional pattern:

```text
repos.json          local machine config, ignored by Git
repos.sample.json   safe example config, committed to GitHub
```

Do not commit your real `repos.json` because it may contain:

- local machine paths
- usernames
- company repository names
- internal folder structures
- editor-specific paths

---

## Create Local Config

Copy the sample config into a real local config file.

### Windows PowerShell

```powershell
copy repos.sample.json repos.json
```

### macOS / Linux

```bash
cp repos.sample.json repos.json
```

Then edit `repos.json` with your real local repository paths.

---

## Example `repos.json` for Windows

### VS Code

```json
{
  "Repositories": [
    {
      "Name": "Example Repo",
      "Path": "C:\\Develop\\example-repo",
      "MainBranch": "main",
      "WorktreeRoot": "C:\\Develop",
      "ReviewCommand": "cmd.exe",
      "ReviewArguments": [
        "/c",
        "code.cmd",
        "-n",
        "."
      ],
      "GitCommandOptions": {
        "TimeoutSeconds": 300,
        "DisableTerminalPrompt": true,
        "EnableTrace": false
      }
    }
  ]
}
```

### Windsurf

```json
{
  "Repositories": [
    {
      "Name": "Example Repo",
      "Path": "C:\\Develop\\example-repo",
      "MainBranch": "main",
      "WorktreeRoot": "C:\\Develop",
      "ReviewCommand": "cmd.exe",
      "ReviewArguments": [
        "/c",
        "windsurf.cmd",
        "."
      ],
      "GitCommandOptions": {
        "TimeoutSeconds": 300,
        "DisableTerminalPrompt": true,
        "EnableTrace": false
      }
    }
  ]
}
```

If `code.cmd` or `windsurf.cmd` is not found, use the full path.

Example:

```json
"ReviewArguments": [
  "/c",
  "C:\\Users\\YourUserName\\AppData\\Local\\Programs\\Microsoft VS Code\\bin\\code.cmd",
  "-n",
  "."
]
```

---

## Example `repos.json` for macOS

### VS Code

```json
{
  "Repositories": [
    {
      "Name": "Example Repo",
      "Path": "/Users/yourname/Develop/example-repo",
      "MainBranch": "main",
      "WorktreeRoot": "/Users/yourname/Develop",
      "ReviewCommand": "code",
      "ReviewArguments": [
        "-n",
        "."
      ],
      "GitCommandOptions": {
        "TimeoutSeconds": 300,
        "DisableTerminalPrompt": true,
        "EnableTrace": false
      }
    }
  ]
}
```

### Windsurf

```json
{
  "Repositories": [
    {
      "Name": "Example Repo",
      "Path": "/Users/yourname/Develop/example-repo",
      "MainBranch": "main",
      "WorktreeRoot": "/Users/yourname/Develop",
      "ReviewCommand": "windsurf",
      "ReviewArguments": [
        "."
      ],
      "GitCommandOptions": {
        "TimeoutSeconds": 300,
        "DisableTerminalPrompt": true,
        "EnableTrace": false
      }
    }
  ]
}
```

---

## Config Fields

### `Name`

Display name shown in the repository selection menu.

```json
"Name": "Example Repo"
```

### `Path`

The path to the main repository.

Windows example:

```json
"Path": "C:\\Develop\\example-repo"
```

macOS example:

```json
"Path": "/Users/yourname/Develop/example-repo"
```

The app runs Git commands from this repository path.

### `MainBranch`

The branch to fetch before creating the worktree.

```json
"MainBranch": "main"
```

This may also be `master`, `develop`, or another branch depending on your repository.

The app does not switch to this branch. It only fetches the latest remote reference.

### `WorktreeRoot`

The folder where review worktrees will be created.

Windows example:

```json
"WorktreeRoot": "C:\\Develop"
```

macOS example:

```json
"WorktreeRoot": "/Users/yourname/Develop"
```

### `ReviewCommand`

The command used to open the created worktree.

Windows example:

```json
"ReviewCommand": "cmd.exe"
```

macOS example:

```json
"ReviewCommand": "code"
```

or:

```json
"ReviewCommand": "windsurf"
```

### `ReviewArguments`

Arguments passed to the review command.

VS Code new-window example:

```json
"ReviewArguments": [
  "-n",
  "."
]
```

The `.` means open the current worktree folder.

On Windows, using `cmd.exe` is often more reliable:

```json
"ReviewCommand": "cmd.exe",
"ReviewArguments": [
  "/c",
  "code.cmd",
  "-n",
  "."
]
```

### `GitCommandOptions`

Controls Git command behavior.

```json
"GitCommandOptions": {
  "TimeoutSeconds": 300,
  "DisableTerminalPrompt": true,
  "EnableTrace": false
}
```

#### `TimeoutSeconds`

Maximum time allowed for a Git command before the app stops waiting.

#### `DisableTerminalPrompt`

When `true`, the app sets:

```text
GIT_TERMINAL_PROMPT=0
```

This prevents Git from silently waiting for username or password input.

Use `true` for normal usage.

Use `false` temporarily if you need Git to prompt for credentials.

#### `EnableTrace`

When `true`, the app sets:

```text
GIT_TRACE=1
GIT_TRACE_PERFORMANCE=1
GIT_CURL_VERBOSE=1
```

Use this only when troubleshooting Git authentication, remote, or network issues.

---

## Running the App

Build the project:

```bash
dotnet build
```

Run the project:

```bash
dotnet run
```

Publish the project:

```bash
dotnet publish -c Release
```

---

## Typical Workflow

1. Start the utility.

```bash
dotnet run
```

2. Select a repository from the menu.

```text
Preconfigured repositories:
1. Example Repo - C:\Develop\example-repo

Select repo: 1
```

3. Enter the branch you want to review.

```text
Enter source branch name: feature/ju
```

4. The app fetches the configured main branch and source branch.

5. The app creates a review worktree.

6. The app opens the new worktree in VS Code or Windsurf.

7. When review is complete, run the utility again and choose the remove option.

---

## Console Output

The console uses color to make important output easier to scan:

- Git commands: cyan
- Success messages: green
- Warnings: yellow
- Errors: red
- Prompts: default console color

---

## Dry-Run Mode

Use menu option `3` to toggle dry-run mode for the current app session.

When dry-run mode is enabled, the header shows:

```text
[DRY RUN ENABLED]
```

The app prints commands that would run, but it does not execute Git commands and does not open VS Code or Windsurf.

Example:

```text
[DRY RUN] git fetch --verbose --progress --prune origin +refs/heads/main:refs/remotes/origin/main
[DRY RUN] git worktree add -b review/my-repo-review-ju C:\Develop\my-repo-review-ju origin/feature/ju
```

Dry-run mode is not written to `repos.json`.

---

## Worktree Naming

Worktree folders use the repository folder name plus a short branch name:

```text
<repo-folder-name>-review-<shortBranch>
```

Example:

```text
modernization-unified-masthead-review-ju
```

If the folder already exists, the app appends a number:

```text
modernization-unified-masthead-review-ju-2
```

Folder names are sanitized for Windows and macOS paths.

---

## Dirty Worktree Protection

Before removing a worktree, the app checks:

```bash
git -C <worktreePath> status --porcelain
```

If uncommitted changes exist, the app shows the changed files and asks for explicit confirmation before running force removal.

Example:

```text
WARNING: This worktree has uncommitted changes.

 M src/App.cs
?? temp.txt

Force remove this worktree and discard these changes? [y/N]:
```

Cancelling at that prompt leaves the worktree untouched.

---

## Removal Safety

Worktree removal uses:

```bash
git worktree remove --force <worktreePath>
```

After removal, the app deletes the local branch only when the branch name starts with:

```text
review/
```

Non-review branches are never deleted by the cleanup step.

Important:

```text
git worktree remove --force
```

can discard uncommitted changes inside the worktree being removed.

Always check the warning before confirming removal.

---

## Git Commands Used

When creating a worktree, the app runs commands similar to:

```bash
git fetch --verbose --progress --prune origin +refs/heads/main:refs/remotes/origin/main
git fetch --verbose --progress --prune origin +refs/heads/feature/ju:refs/remotes/origin/feature/ju
git worktree add -b review/example-repo-review-ju <worktreePath> origin/feature/ju
```

When removing a worktree, the app runs commands similar to:

```bash
git -C <worktreePath> status --porcelain
git worktree remove --force <worktreePath>
git branch -D review/example-repo-review-ju
git worktree prune --verbose
```

---

## Recommended `.gitignore`

Use a `.gitignore` file to avoid committing build output and local config.

```gitignore
bin/
obj/
.vs/
.vscode/
*.user
*.suo

# Local machine config
repos.json

# Safe public sample config
!repos.sample.json
```

For .NET projects, you can generate a starter `.gitignore` with:

```bash
dotnet new gitignore
```

Then add the `repos.json` rule manually.

---

## Troubleshooting

### Git fetch fails or hangs

First verify Git works outside the app.

Windows:

```powershell
cd C:\Develop\example-repo
git fetch --verbose --progress origin
```

macOS:

```bash
cd /Users/yourname/Develop/example-repo
git fetch --verbose --progress origin
```

If this fails, the issue is with Git authentication, network, VPN, or remote configuration.

Check your remote:

```bash
git remote -v
```

If you use HTTPS, verify Git Credential Manager or your credential store is configured.

Windows:

```powershell
git credential-manager --version
git config --global credential.helper manager
```

macOS:

```bash
git credential-manager --version
git config --global credential.helper manager
```

If you use SSH, test:

```bash
ssh -T git@github.com
```

or your organization’s Git host.

---

### VS Code does not open on Windows

Check:

```powershell
where code
where code.cmd
```

If found, use `cmd.exe` and `code.cmd` in `repos.json`.

Example:

```json
"ReviewCommand": "cmd.exe",
"ReviewArguments": [
  "/c",
  "code.cmd",
  "-n",
  "."
]
```

If needed, use the full path to `code.cmd`.

---

### VS Code does not open on macOS

Check:

```bash
which code
code --version
```

If `code` is not found, open VS Code and install the shell command:

```text
Command Palette → Shell Command: Install 'code' command in PATH
```

Then restart your terminal and try again.

---

### Windsurf does not open on Windows

Check:

```powershell
where windsurf
where windsurf.cmd
```

Then update `repos.json` accordingly.

Example:

```json
"ReviewCommand": "cmd.exe",
"ReviewArguments": [
  "/c",
  "windsurf.cmd",
  "."
]
```

---

### Windsurf does not open on macOS

Check:

```bash
which windsurf
windsurf --version
```

If the command is missing, install or enable the Windsurf command-line launcher, then restart your terminal.

---

### Worktree removal fails

Close any editor, terminal, or running process inside the worktree folder.

Then try again.

Windows can block folder deletion if a process is currently using the folder.

Also make sure the utility itself is not running from inside the worktree being deleted.

Recommended layout:

Windows:

```text
C:\Develop
├── WorktreeReviewTool
├── example-repo
└── example-repo-review-ju
```

macOS:

```text
/Users/yourname/Develop
├── WorktreeReviewTool
├── example-repo
└── example-repo-review-ju
```

---

## Publishing to GitHub

Before pushing this project to GitHub, make sure:

```text
repos.json is not committed
repos.sample.json is committed
bin/ is not committed
obj/ is not committed
README.md is updated
LICENSE exists if the project is public
```

Check Git status:

```bash
git status
```

If `repos.json` is staged accidentally, unstage it:

```bash
git restore --staged repos.json
```

Recommended first commit:

```bash
git add .
git commit -m "Initial release of Git Worktree Review Utility"
```

Then connect to your GitHub repository:

```bash
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/git-worktree-review-utility.git
git push -u origin main
```

---

## Development

Build:

```bash
dotnet build
```

Run:

```bash
dotnet run
```

Clean:

```bash
dotnet clean
```

Publish release build:

```bash
dotnet publish -c Release
```

---

## Future Improvements

Possible future improvements include:

- arrow-key menu selection
- repository search/filter
- worktree search/filter
- support for more editors
- optional GUI version
- unit tests for branch name sanitization
- unit tests for Git worktree parsing
- automatic cleanup of stale worktrees
- configurable worktree naming templates

---

## License

# Git Worktree Review Utility

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)