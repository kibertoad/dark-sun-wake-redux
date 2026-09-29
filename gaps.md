# Upstream gaps observed during restoration work

These are the remaining requests for the restoration template and shared tooling.
Merged template/toolkit changes have been adopted and validated locally; resolved
requests were removed. See [the adoption record](docs/TEMPLATE-ADOPTION.md).
The original item numbers remain stable. These are tooling requests, not new
claims about the original game's rules.

## 38. Keep JVM crash diagnostics out of commit candidates

On 2026-09-29, the session handover named an untracked
`hs_err_pid15480.log` in the repository root. Its header reports a native
allocation failure during JVM startup; the command line is empty and
`java_command` is unknown, so it does not establish which analysis task
failed. A current process inventory contained no Java process. The file
was left untouched because ownership remains uncertain.

The template ignores Ghidra projects and build logs but did not ignore JVM
fatal-error or replay logs. Such diagnostics can contain environment paths,
stack details and memory excerpts; their presence is neither a durable
finding nor proof of a live task. This checkout now ignores
`hs_err_pid*.log` and `replay_pid*.log`.

**Request:** include these exclusions in the shared template and consider
a repository-policy rejection for explicitly staged JVM diagnostic files.
Document how to redirect analysis crash diagnostics into local-only storage.
Keep process ownership and terminal-state verification separate from the
presence of a crash file, and do not attribute an unidentified JVM failure
to a specific research query.

## 2. Make memory-block reports usable for mapped overlays

The local-only `FBOV` mapped image produced 3,546 Ghidra memory blocks.
`ReportMemoryBlocks.java` stopped at its 512-block limit without a report.
The limit is useful for bounded output, but it prevents inspecting this image's
block layout when diagnosing analyzer-discovered functions.

**Request:** let the shared reporter select a named block, address range, or
bounded page of blocks, while retaining an explicit output limit. This would
allow focused inspection without a broad memory-map export.

## 4. Offer an offline rerun for the local test gate

`./tools/Test.ps1` invokes `dotnet test` with restore on every run. In this
restricted workspace, restore failed on NuGet's service or signature endpoint
even after a successful authorized restore had cached the packages. The same
script passed all 700 tests when network access was available.

**Request:** consider an explicit offline rerun option that uses an already
restored lock/assets state. Keep the normal CI path restoring packages from
NuGet.

## 9. Distinguish a window image from copied control data in UI catalogs

`UiWindowResource` currently reports `Window.ImageResourceNumber` from
offset `0x3A` of a `WIND` record. In `WIND/18500`, that word is 10002 because
the record copies edit-box data; the window's own image field at `0xC2` is
zero. The read-only UI catalog therefore appears to assign `BMP/10002` to
the whole window, even though it belongs to the copied edit-box record.

**Request:** have the shared UI catalog expose the true window image field
separately from copied control data, and label the latter as uncertain until
its runtime use is established. A synthetic fixture with different values at
`0x3A` and `0xC2` would guard against this false screen-background claim.
