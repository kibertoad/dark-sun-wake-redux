# Building and releasing installers

## Local package builds

Create an SDK-free Windows package with:

```powershell
./tools/Publish-Windows.ps1
```

Build the versioned Windows installer with the pinned Inno Setup 7.1.0 compiler:

```powershell
./tools/Build-WindowsInstaller.ps1 -Version 0.1.0
```

Build Linux x64 and macOS arm64/x64 installers on their native hosts:

```powershell
./tools/Build-LinuxInstaller.ps1 -Version 0.1.0
./tools/Build-MacInstaller.ps1 -Version 0.1.0 -Runtime osx-arm64
./tools/Build-MacInstaller.ps1 -Version 0.1.0 -Runtime osx-x64
```

All portable packages and installed applications include the project `NOTICE`
and canonical MIT `LICENSE`. The Windows Setup wizard displays both before
installation. Release installers are unsigned until a project adds its own
platform-specific signing configuration.

The Windows installer accepts `/ORIGINAL="C:\path\to\original"` for unattended
source selection and `/NOEXTRACT=1` to explicitly skip extraction. The interactive
installer streams progress into the Setup log and allows another source to be
selected if verification fails.

## GitHub release workflow

Run the manual-only `Release installers` workflow, enter a semantic version such
as `0.1.0`, and select `windows` or `all`. The default builds Windows x64 only.
`all` additionally requires Linux x64, macOS arm64, and macOS x64 artifacts. A
tag and GitHub Release are created only after tests and every selected build
succeed.

## Git publication authorization

Local commits are not published until the repository owner has approved an
exact remote and branch after the checks in `AGENTS.md` have been completed.
The approval record below is intentionally separate from GitHub release access:
release or administrative permission alone does not authorize an automated push.

### Current owner verification

On 2026-09-20, read-only checks established the following non-secret facts:

| Fact | Verified value |
|---|---|
| Canonical fetch/push URL | `https://github.com/kibertoad/dark-sun-wake-redux.git` |
| GitHub repository identity | `kibertoad/dark-sun-wake-redux` (private) |
| GitHub-reported repository owner | `kibertoad` |
| Authenticated GitHub account | `kibertoad` |
| Effective repository permission | `ADMIN` |
| Branch proposed for publication | `main` |

The repository owner explicitly approved ordinary fast-forward pushes of
`main` to this exact URL on 2026-09-20 after reviewing the table. The approval
is limited to the verified repository, authenticated account, and branch above;
any changed remote, push URL, account, or non-fast-forward operation requires a
fresh verification and approval.

### Approved destinations

Add a row only after that explicit owner confirmation. Each row must include
the canonical URL, `OWNER/REPOSITORY`, branch, verification date, approving
owner, publishing account, effective permission, and permitted scope. Do not
record tokens, credentials, cookies, or private-key material.

| Canonical URL | Repository | Branch | Verified | Approving owner | Account / permission | Scope |
|---|---|---|---|---|---|---|
| `https://github.com/kibertoad/dark-sun-wake-redux.git` | `kibertoad/dark-sun-wake-redux` | `main` | 2026-09-20 | repository owner | `kibertoad` / `ADMIN` | ordinary fast-forward pushes from local `main` |

## Continuous integration

Pull requests and manual runs build, test, and smoke-test the assetless project
on Windows, Linux, and both macOS architectures. Installer jobs verify the
installed filesystem layout; Windows additionally validates the generated Start
menu shortcut and uninstall cleanup.
