# Dark Sun reporter acceptance audit

## Source and scope

2026-09-30. BLD-GOG-EN-1.1, the stable owned DSUN.EXE baseline in
`docs/GHIDRA.md`: 634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. The recorded path and current size/metadata
were checked before querying. Each reporter also checks its SHA-256 input guard.
No original process, DOSBox, emulated function or native observation was used.
No spec or parity status changed.

Adopted reporter: toolkit `c2b21ee62fc404391e8dcfafd7029185f81241a9`.
Refinement candidate: toolkit `2b688e66ba34ee25d9882ea4938f95b5461b0ba4`,
PR [16](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/pull/16).
Python 3.14 and Capstone 5.0.7. Standard remains v1 with the existing local pin.
The candidate was tested separately; this game has not adopted the unmerged
reporter or proposed rule changes.

Configurations and raw JSON reports live under `GAME_DIR/analysis/reporter-audit/`
and are not committed. `run.mjs` records the initial cases; `verify.mjs` checks
controls and conditional continuations; `verify-adopted.mjs` repeats the dispatch
check with the actual adopted source. The case names below identify their JSON
configurations and corresponding `.report.json` files in that local directory.
The summary here records tool acceptance, not new claims about the game.

## Ten selected requests

| Gap | Case and expected control | Observed result and disposition |
| --- | --- | --- |
| 14 | `gap14-gate` / `gap14-code`, established entry and the two reads in FND-CONFIG-108 | Adopted effect traces stop before the known reads. Candidate entry-CFG operand observations find both with correct widths, retain unknown values/segments, report undecoded ranges and reject an offset-one bad control. Candidate passes; keep open pending adoption. |
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
instructions. Keep the request open until the new reporting events are adopted.

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
