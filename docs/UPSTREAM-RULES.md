# Pinned Standard v1, Methodology and Protocol

`tools/upstream-lock.json` identifies the exact revision, source paths and
SHA-256 digests of the unmodified rule pages and their MIT license in
`vendor/upstream/`. Configuration preserves their bytes. Agents read these local
copies; routine work never checks for newer published rules.

Install tooling with `./tools/Restore-ToolDependencies.ps1`. The checker is the
npm package `@scientific-method/standard-checker`, locked in `package-lock.json`;
shared instruction analysis is the hash-locked Python release in
`tools/evidence/requirements.txt`. CI runs the toolkit's `check-documentation`
action at a full commit SHA, which runs the checker source at that commit. The
lock's `checker` entry records that commit and the package version it carries,
and `package.json` pins exactly that version, so the offline run and CI apply
the same rules. Package
installation does not refresh the rule snapshot. Node 22+ and PowerShell 7+ are
required; configuration tests also require Git.

```sh
node tools/upstream.mjs verify
node tools/upstream.mjs docs --check
node tools/upstream.mjs docs
node tools/upstream.mjs links
```

Verification checks snapshot digests offline, and that the CI action pin, the
lock and `package.json` agree. The documentation runner refuses an installed
checker of another version, and passes CI checker inputs to the installed package; command
line inputs override them. Without `--check`, it regenerates indexes and parity.
`links --write` updates section line ranges. Canonical validation includes these
checks. NoRestore requires installed locked dependencies and never restores.

Only when the owner requests a rules refresh in the current task:

```sh
node tools/upstream.mjs check-upstream
node tools/upstream.mjs refresh --rules <full-40-character-commit> --toolkit <full-40-character-commit>
```

The freshness command compares the pinned rule files with the website repository,
and the pinned checker version with the one on the toolkit's main branch. It
returns 0 for unchanged content, 2 for changed content and 1 for failure, and
writes nothing. Refresh stages all downloads and the checker version the toolkit
commit carries before writing anything. The toolkit commit must be the one the
toolkit tagged `@scientific-method/standard-checker@<version>`: a later commit can
carry the same version with unreleased checker changes, which CI would run and the
published package would not. Refresh requires Standard v1, updates the CI checker
pin and the `package.json` pin, atomically replaces individual files and writes
the lock last. Run `npm install` afterwards to update `package-lock.json`, then
`./tools/Restore-ToolDependencies.ps1`. Digest checks detect an interrupted
refresh. Review changes, regenerate link ranges and run the canonical gate before
committing. The checker's
[changelog](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/blob/main/packages/standard-checker/CHANGELOG.md)
lists what each version checks. No spec claim is promoted as a side effect.

Configuration tests copy Git-visible, non-ignored files and exclude caches and
local output. Initialize a Git checkout before validating a ZIP download. Set
`PWSH` when the PowerShell host is not named `pwsh` on PATH.

Shared reporter contracts and local setup are in
[BOUNDED-EVIDENCE-REPORTERS.md](BOUNDED-EVIDENCE-REPORTERS.md). Shared Ghidra scripts
come from the installed engine; `tools/Get-GhidraScriptPath.ps1` combines their
path with retained project-specific scripts. No original content is committed.
