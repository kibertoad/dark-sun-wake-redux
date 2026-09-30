# Dark Sun reporter acceptance audit

## Source and scope

2026-09-30. BLD-GOG-EN-1.1, the stable owned DSUN.EXE baseline in
`docs/GHIDRA.md`: 634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. The recorded path and current size/metadata
were checked before querying. Each reporter also checks its SHA-256 input guard.
No original process, DOSBox, emulated function or native observation was used.
No spec or parity status changed.

Current adopted reporter/checker: toolkit `926e287a4134512d59fe021efe6507c933da03f1`. Website rules: `3b4e6fcfca887620cdf13c8a8e62f9ca53133d60`. Template: `b9f542549840cf7ce2d254a8f9f7bf0f502daaa7`. The initial candidate was `2b688e66ba34ee25d9882ea4938f95b5461b0ba4`; revised upstream changes are now fully adopted. Python 3.14 and Capstone 5.0.7; Standard remains v1.

Configurations and raw JSON reports live under `GAME_DIR/analysis/reporter-audit/`
and are not committed. `run.mjs` records the initial cases; `verify.mjs` checks
controls and conditional continuations; `verify-adopted.mjs` repeats the dispatch
check with the actual adopted source. The case names below identify their JSON
configurations and corresponding `.report.json` files in that local directory.
The summary here records tool acceptance, not new claims about the game.

## Ten selected requests

| Gap | Case and expected control | Observed result and disposition |
| --- | --- | --- |
| 14 | `gap14-gate` / `gap14-code`, established entry and the two reads in FND-CONFIG-108 | Adopted effect traces stop before the known reads. The current adopted source finds both as conditionalAccesses with named stops/untraced callees, correct widths and unresolved effects; it retains undecoded ranges and rejects an offset-one bad control. Close the bounded discovery request, without claiming values or unconditional execution. |
| 13 | `gap13-selector` / `gap13-setup`, eleven selector calls in FND-CONFIG-101 and six setup calls in FND-CONFIG-111 | Named overlay-domain scans recover all eleven and six encodings. Entry CFG confirms seven and two respectively; the remaining four in each inventory stay raw candidates because of unresolved dispatch routes. The FND-CONFIG-128 same-segment query recovers its later relative-call candidate, also unverified. Positive controls cannot be accepted for these missing paths. Keep open. |
| 18 | `gap18-normalization`, FND-CONFIG-137's verified selection continuation after the byte reader returns | Both adopted and candidate dispatch reports map the recorded normalized/extended pair to position 12 and raw target 3351, with decoded transformation, range gates and the evidenced 99-word layout. Out-of-domain examples remain unresolved without an invented selection. Close this reporter request; no general expression-reader or native-input claim. |
| 21 | `gap21-caller` / `gap21-helper`, FND-CONFIG-143 and FND-CONFIG-145 | Candidate preserves SS-based formation versus DS-based consumption and explicit unresolved aliases. The helper can finish only under a declared returning-copy model; caller loops and mixed-segment filename dependencies in FND-CONFIG-181 remain unverified. String/repeat operations are unsupported. Keep open. |
| 35 | `gap35-register` / `gap35-consumer`, FND-CONFIG-179 and FND-CONFIG-180 | Actual callee reads retain a far window, word identifier, far callback and word mask at distinct consumed offsets/widths. Setter continuations require explicit return/preservation assumptions; the full caller reaches a path limit. Candidate fixes far-frame handling, but the full caller-to-forwarding acceptance remains incomplete. Keep open. |
| 27 | `gap27-loader`, FND-SCRIPT-019's bypasses, loader writes and transfer failure | Early exits stay distinct. With the near-entry return frame correctly configured, unread calls and bounded age loops still prevent complete ordered transfer/rollback summaries. No unchanged-state or transactionality conclusion is accepted. Keep open. |
| 26 | `gap26-initializer` / `gap26-frame`, FND-CONFIG-156 and FND-CONFIG-190 | Widths and producer-specific contracts remain visible, but initializer loops/alias-sensitive return provenance and frame-pointer carry rotation stop the required caller chains. No success conclusion is inferred from a nonzero byte. Keep open. |
| 36 | `gap36-bracket`, FND-CONFIG-187 and FND-CONFIG-188 | Byte and word widths remain distinct, with missing producers explicit. Callee boundaries and alias-sensitive returns prevent full propagation into normalized wrapper results; modeled calls invalidate memory rather than preserve an unwritten neighbor. Keep open. |
| 32 | `gap32-metadata` / `gap32-reload`, FND-CONFIG-165 and FND-CONFIG-171 | The metadata read appears before the null guard; all wrapper paths reach the same runtime request and conditional zero result. Candidate also reports the full far-indirect reload and a changed guard identity after an unread bracket. Modeled continuation/path limits leave the wider layered contract partial. Keep open. |
| 42 | `gap42-wrapper` / `gap42-runtime`, FND-CONFIG-209 through FND-CONFIG-211 | Candidate gets past XCHG; multiplication, carry/counter operations, split-word request representation, repeated clearing and lower heap dependencies remain unsupported/unresolved. Requested quantity, header extent and native capacity are not equated. Keep open. |

Conditional queries explicitly assume a returning call with balanced encoded
frame and the named preserved registers. They invalidate all memory and flags;
AX/DX results remain unknown. These are conditional local continuations, not
evidence of callee preservation, successful release or actual failure. The
audit never uses a modeled return to remove a missing-producer condition.

## Related requests

Gap 19: `gap19-reader` and `gap19-advance` check the three conversion sites
in FND-CONFIG-138. The candidate reports AL-to-AX, 16-bit effective size and
explicit decoder-mnemonic mismatch. Prefixed controls use entirely synthetic
instructions. The current adopted source passes all three recorded conversion controls. Close this conversion-reporting request; the advancement body still stops at an out-of-region call.

Gap 39: `gap39-frame` reaches the DS-based indexed stores in FND-CONFIG-198
under explicitly conditional guard/request returns, retaining BP-derived offset
provenance. The first loop stops at the repeated-instruction bound before the
second loop's reads. Stack reservation is not accepted as DS buffer capacity.
Keep open pending that full case.

Gap 40: `gap40-cleanup` exposes path-specific writes and unknown frame bytes,
but prior image/handle calls and the path cap prevent exhausting all cleanup
predecessors in FND-CONFIG-199. Keep open; no native defect is inferred.

## Upstream refinements and validation

- [Toolkit PR 16](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/pull/16)
  preserves entry-CFG operand observations after unresolved callees, independent
  unknown flag producers, push-CS/near-call frames and explicit modeled widths,
  far-indirect guard provenance, effective-width conversions, XCHG and low-result
  IMUL. Output exhaustion names its 32 MiB bound. Tests contain constructed bytes.
- [Template PR 28](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/28)
  pins that exact source, guide, license and acceptance suite.
- [Standards/protocol PR 27](https://github.com/kibertoad/refurbished-dinosaurs/pull/27)
  clarifies operand-versus-effect reports, independent unresolved flag producers,
  encoded far frames and the contents of a durable case-verification record.

Toolkit: 55 Python tests; 121 Node tests (120 passed, one case-sensitive-file-system
skip on Windows); 17 .NET tests and repository policy pass; build has no warnings
or errors. Template canonical gate: 55 Python, 39 Node and 56 .NET tests pass.
Website: pnpm check passes. Actual source cases above were separate local static
checks; upstream synthetic success did not determine gap closure.

All three PRs' applicable CI checks pass. Dark Sun's existing pinned gate also
passes: 45 Python, 41 Node and 700 .NET tests, with repository, documentation,
configuration and upstream-pin checks passing.

## Revised main adoption rerun

The local verification script now imports this checkout's pinned reporter and checks the reviewed conditionalAccesses schema: the two known reads name their stop dependencies rather than masquerading as traced effects. Incorrect controls still fail. Dispatch and all three effective-width conversions pass again. Conditional continuations for the other cases were rerun with unchanged unresolved/cap limits; they do not close those requests. No original process or emulated call ran. Earlier validation counts above describe the initial PR batch.

## Goal batch: numeric table and pointer controls

2026-09-30, current pinned tooling. Local verify-simple.mjs and gap12-tags/targets/extra configurations and reports are in GAME_DIR/analysis/reporter-audit. FND-CONFIG-096's evidenced two four-byte tags and separate two-word target table match the known tags and canonical targets. An attempted third entry is rejected with limit 2 derived from the loop, and no ASCII fallback is used. The tool requires the researcher to supply the evidenced count/limit; it does not infer the loop. Gap 12 is closed.

FND-CONFIG-144's two pushed far-pointer segment operands retain raw tokens, declared fixup membership, decoded descriptors 113/116 and mapped segments/offsets. These controls pass; gap 16 remains open pending the segment-load and stored-pointer cases. No new game claims or runtime observations.

## Goal batch: bounded maps and JVM diagnostics

Template PR [32](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/32), commit 686f643, supplies shared memory-block selection and diagnostic policy. Local tools use the same implementation, with the configured script category retained. Gap 2's actual retained Ghidra 12.1.3 mapped project reports total 3,546, page 3538/count 16 emits eight blocks with partial scope, exact HEADER emits one block, missing name and oversized default queries fail. Logs gap2-page.log and gap2-controls.log remain GAME_DIR/analysis/reporter-audit. The project was opened read-only with no analysis or original execution; no bytes/code were exported.

Gap 38's scratch-Git test proves ignored root/nested JVM basenames, force-staged diagnostic rejection (including case variants), and ordinary-log acceptance. Java acceptance uses an entirely constructed 3,546-block map and verifies empty/clipped/capped pages, exact/ambiguous/missing names, invalid ranges and arguments. Both behaviors are implemented and verified locally; the upstream requests stay open pending PR merge. Root gate passes 92 Python, 43 Node and 700 .NET tests; template passes 92 Python, 41 Node and 56 .NET tests and a zero-warning/error build.

## Goal batch: actual inventories and committed identity

verify-inventory.mjs under GAME_DIR/analysis/reporter-audit checks the retained original/mapped Ghidra exports against the adopted shared join. Canonicalization uses the existing documented mapped-import formula, retaining explicit exclusions: 97 rows outside source ranges are excluded before the join. The shared exporter was compiled/executed on the read-only retained mapped project, without new analysis, and its 2,723-row output exactly matches the historical export. The join accepts 1,284 original resident and 869 mapped overlay starts, producing the committed 2,153-row TSV exactly. Conflicting view ownership, duplicate/invalid starts, invalid sizes and unmapped starts are rejected. Gap 3's view-specific coverage request passes and closes. Counts remain analyzer-discovered starts, not whole-program coverage.

Template PR [33](https://github.com/kibertoad/refurbished-dinosaurs-template/pull/33), commit 85175d9, adds committed inventory-check identity/destination verification. The actual installed 2,153-row TSV passes; synthetic CLI and schema checks reject wrong prefixes/destinations, numeric aliases, malformed columns/counts and unevidenced legacy paths, while optional empty text cells remain valid. Gap 1 is delivered/verified locally pending upstream merge. Gap 5's portable destination and legacy disc prefix checks pass, but validation against the separately owned disc executable is still pending; no installed-source result is substituted for that case. Raw exports/canonicalization summaries/configurations remain GAME_DIR-only.

Root gate: 92 Python, 44 Node and 700 .NET tests pass. Template canonical gate: 92 Python, 40 Node and 56 .NET tests pass, build zero warnings/errors.

## Merged template PR 32 acceptance

Reviewed and adopted the final merge c048c63523a1b061b5325f6b05055d98819780d7, including review-time requested-count headers, bounded ambiguity indices and policy-driven heap-dump protections. The revised synthetic tests pass without skips. The actual retained mapped project again reports the expected tail page and exact-name result, with requested count explicit; its read-only log is gap2-merged-adoption.log under GAME_DIR. Both entire requests 2 and 38 now have upstream delivery and local acceptance evidence and are removed from gaps.md. Earlier pending-merge notes above are historical.
