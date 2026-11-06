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
node tools/upstream.mjs docs --preflight
node tools/upstream.mjs docs --generate
node tools/upstream.mjs links
```

`links` checks that every link into the copy names a heading there and gives
that section's line range, so agents read only those lines; `links --write`
adds or corrects the ranges. The first command checks integrity, and that the CI
action pin, the lock and `package.json` agree, without network access. The
documentation runner also refuses an installed checker of another version, and
verifies before executing it.
It passes the checker the inputs the CI step gives, `scheduled-generation`
included, so it neither writes nor compares `spec/index/` and `PARITY.md`;
`--generate` drops that input, and without `--check` the checker then writes
them, as the scheduled job on the main branch does (`docs/VALIDATION.md`). The canonical
validation gate runs these offline checks. Kaitai and other dependencies required
by the checker must already be installed for applicable entries; a snapshot does
not install them or make the entire build network-independent.

The snapshots are the rules this repository follows, and agents read them
instead of the published pages: `AGENTS.md` ("The local copy of the standard")
says how. The template is updated when the website changes, so nobody checks
for a newer version during routine work. Owner instructions and explicit
repository adaptations still apply.

NoRestore requires installed locked dependencies and never restores.

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

Whole-checkout configuration integration copies Git-visible, non-ignored files
and excludes caches and local output. Narrow checker-option controls use small
synthetic metadata fixtures. Initialize Git before validating a ZIP download. Set
`PWSH` when the PowerShell host is not named `pwsh` on PATH.

Shared reporter contracts and local setup are in
[BOUNDED-EVIDENCE-REPORTERS.md](BOUNDED-EVIDENCE-REPORTERS.md). Shared Ghidra scripts
come from the installed engine; `tools/Get-GhidraScriptPath.ps1` combines their
path with retained project-specific scripts. No original content is committed.

Local documentation checks compare against local `main` when it exists, because
this project's sessions commit without pushing and `origin/main` may predate
already adopted generated files. CI keeps its pull-request base. An explicit
`--base` overrides the local default; comparisons remain enabled.


Optional editing feedback uses `node tools/upstream.mjs docs --preflight`.
Checker 6.1.0 prints a partial pass and names the checks it skips. The wrapper
omits its implicit scheduled-generation flag for this mode; explicit
incompatible options still fail. Preflight never writes generated files and
does not replace complete documentation, citation, schema, queue or staged
acceptance checks. `--generate` cannot be combined with it.


## Research closure recovery adoption, 2026-11-06

Adopted protocol [PR 130](https://github.com/kibertoad/refurbished-dinosaurs/pull/130)
at `34202ed4a313b0c9c702f159ccbc8f5e85fb9f28` and template
[PR 149](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/149)
at `175daaad3c6be12cd389aedd2e3bc859647cecb7`, following the owner's
explicit refresh request. The lock records the protocol source revision and
hashes; checker 6.1.0 and its existing release revision are retained.

The template additions to `docs/RESEARCH-CLOSURE.md`, `start-session` and
`research-item` are adopted with this checkout's `vendor/upstream/` paths.
The closure procedure links the now-pinned recovery section and acknowledges
the existing progress reporter instead of the template's missing-reporter
notice. Assessment results belong in research commit messages, obligation
maps in spec Open questions keyed by queue IDs, timing in the next three
closure-batch commit messages, and the two-batch comparison in the handover.
Adoption changes no evidence status or parity result.

## Claim-scoped closure adoption, 2026-11-06

Following the owner's request to adopt the latest template, checked template
main at `ef82c9784d08c4e98ade56cfd49cdd56130ac139` and adopted its protocol
snapshot `5b398e0eab9a6ff8ad1c35fc003c214d85fe7d47`, the merge of process
[PR 132](https://github.com/kibertoad/refurbished-dinosaurs/pull/132).
The template's other changes since the previously checked `8cf09b0` are
section-range updates and the matching research skill/closure procedure.

The new scope paragraph and research-item instruction are adopted with
this checkout's vendored paths and existing reporter adaptations. Each
blocker names the claim and mechanism it could change; unrelated service
behavior is not absorbed into an entry. Unknown accesses and indirect
targets remain obligations. Layout, acquisition and buffer-lifetime claims
retain their own evidence requirements. No claim, complete-reading
declaration, queue closure or parity result changes through this adoption.
Checker 6.1.0 and its release revision remain the template's exact pins.

## Connected-contract procedure adoption, 2026-11-06

At the owner's request, adopted merged template
[PR 150](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/150)
(`dc06cc87a82065b053fc3318e962e2f5ae69ab15`) and the corrective
[PR 151](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/151)
(`824a3a831b641267d54228c1c10c3d996b5c89fa`). PR 151 was stacked
onto PR 150's branch; its Evidence bundle row is applied explicitly here.

The closure guide and start/research skills require a contract brief in
the existing batch commit-message draft: useful answer, boundary, connected
evidence bundle and falsifiable exit. The bundle stays within the items one
research batch may settle; another entry's dependency gets its own bounded
queue item. Required helpers join the reading rather than automatically
ending a batch. Partial results name the exact remaining condition, and
interrupted work resumes from the objective and queue IDs in the handover.

Adoption preserves local goal, clean-room, validation and reporter
adaptations. Links use this checkout's vendor/upstream paths. The generic
template handover State bullet is recorded in the active goal's handover,
without replacing the unrelated non-goal handover or copying template test
counts. No rule snapshot, package/checker pin, evidence status, complete
reading or parity result changes through this procedure adoption.
