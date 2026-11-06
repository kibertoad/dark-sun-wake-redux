# Bounded shutdown observation

Research tooling for Q-SAVE-007 and RULE-SAVE-001. The player-visible
question is whether Quit's known cleanup requests affect a saved-game or
working archive. FND-SAVE-027..030 and FND-EXE-398/558/559 identify
the exit worker, archive fields and slot dispatch. Existing load reports
omit shutdown dirty state, and the historical exit driver is not a current
state-admitted observation. Reuse existing session, checkpoint, named-read
and RNG support rather than creating a general memory or execution tool.

The first tooling batch supplies a read-only snapshot helper for the
ordinary1000:0388 entry only. Admit the exact breakpoint, real mode,
configured relocation, equal expected DS/SS, intact six-byte argument
region with the two ordinary zero flags, and bounded named fields. Read
the callback count/table (capacity32), stored stream hook/four records,
slot gate/sixteen argument-target pairs, active archive and linked head
records with their native handle, logical handle, options and dirty byte.
Reject cycles, short reads, unsupported counts, offset/linear wrap and
more than64 archive nodes. The64-node limit is tooling, not game behavior.
No input, continuation, guest write, arbitrary field/range or native launch
is exposed by this helper. A snapshot at exit entry does not establish
state after intervening callbacks or a successful file operation.

Synthetic acceptance uses an instrumented memory client: exact positive
record values and request bounds; wrong stop/segment/mode/flags rejected
before reads; cyclic and oversized lists, excessive callback counts and
short reads fail explicitly. Source-derived reports remain in GAME_DIR.
Run these controls and the canonical tooling/integration gate before commit.

A subsequent tooling batch integrates a fixed runner. It must verify the
approved DSUN fingerprint and saved-map checkpoint, use RustDosSession
with the pinned rust-dos/default muted audio/private drive, validate the
documented F3/Quit UI and input delivery, and retain initial seed/every
admitted RNG draw through Recorder. Unknown draws or states stop. Capture
entry and reached close/preparation/write request states with caller/frame
bindings, then native result and private-drive file hashes where supported.
No memory seeding, injected calls, unrestricted program arguments or
arbitrary breakpoint configuration. Close the owned session in all paths.
Synthetic runner controls must cover admission failures and stop progression.

The per-call extension observes only the declared slot indirect-call and
return boundaries and the four fixed stream selector/count-test boundaries.
At the slot call require the admitted DS/SS, index0..15 and BX=14*index;
record the pushed argument separately from the current table argument and
target. A mismatch is evidence to retain, not silently normalize. Pair the
ordinary return with that call and record register/frame preservation without
assuming it. Stream reads require one of the four fixed offsets; unsupported
records stop rather than admitting arbitrary near pointers. Synthetic controls
cover changed arguments/registers, wrong boundaries/index/stride and signed
counts. These point observations do not establish continuous preservation,
unseen native targets or the dirty-archive case.

The fixed callee-entry extension observes the known type-three cleanup
entry only. Require its return pair to name the dispatcher's ordinary
return, and SP to reflect the four-byte far-call frame below the observed
pushed argument. Record the entry argument separately, preserving a mismatch
as an observation. Reject other targets or unpaired entries instead of
following arbitrary pointers. Test wrong return pairs, stack displacement
and changed arguments. Entry/return observations do not certify all internal
native effects or continuous preservation.

A separate controlled-dirty tooling extension may seed only the evidenced
`archive_pending_metadata` byte of the active archive record, after the
ordinary exit-entry snapshot. This is an explicit exception to the clean
runner's no-seeding contract, exposed through a fixed opt-in stage rather
than arbitrary addresses, bytes or calls. Require a valid bounded snapshot,
active record equal to the linked head, a zero old byte, and option mask
`0x0002` present. Bind the write to that same record's named field through
the session package's paused/quiescent field contract, expected-old digest
and readback guards. Reject inconsistent state instead of repairing it.
Use only the private restored drive; preserve the canonical checkpoint and
licensed inputs. Record the setup write explicitly in the retained report.

Observe the same record at preparation entry and return, native write
requests and results, the final commit request/result and the dirty byte's
resulting value. Retain failures and partial writes; file hashes alone do
not prove that no requests occurred. Synthetic controls must reject wrong
record identity, missing option, nonzero old byte and unsupported stages,
and verify the one-byte guarded write and preparation-result pairing.
The exit is a state-admitted report of this conditional close contract or
a precise capability failure. A seeded case cannot establish an ordinary
producer history, filename/handle binding, durability or whole-entry closure;
those remaining Q-SAVE-007 obligations stay open.

The implemented opt-in invocation is `shutdown_run.py <native-save-source-run>
<new-run-name> dirty-active`; omit the stage for the clean observer. Run
`check_shutdown_dirty.py` alongside the existing shutdown snapshot, call and
runner controls with the locked evidence Python before integration. The
controlled observer pairs the fixed close-worker preparation return and
the final commit service's interrupt request/result. Unsupported callers,
nested preparation and incomplete native results stop with retained evidence.
Synthetic success does not admit this new stage for an original-game run;
the first bounded attempt must record its actual capability and limits.

The XMS linkage extension follows only the known type-three cleanup's
installation query, entry-pointer query and free request/return boundaries
(FND-EXE-394/396/581/584). It records the returned pointer separately from
the pointer actually loaded at the free call, along with its handle and
result. Require the existing paired type-three callee observation; reject
unpaired/reordered services or changed selectors. Preserve pointer/handle
mismatches as observations. Do not place breakpoints at arbitrary returned
native addresses or infer every external effect from a pointer sample.
The pinned emulator's [DOS data layout](https://github.com/dividebysandwich/rust-dos/blob/v1.8.1/src/dos_data.rs)
fixes its XMS entry at0160:1F00; its [XMS driver](https://github.com/dividebysandwich/rust-dos/blob/v1.8.1/src/xms.rs)
installs that entry. Admit that exact target only, observe its entry with
the expected far-return pair and stack displacement, and reject hooks or
other targets. Entry is observed separately from the pointer sample.
Synthetic controls cover ordering, selectors, frame binding and mismatches.
The native exit is a linked request/result report or a precise capability
failure; API identification alone is not an external no-file-effects proof.

The final research batch attempts only admitted runtime parts, records
versions, exact source/checkpoint identity, state/control observations and
request/result/file-change distinctions, and narrows the rule/queue only
as those observations justify. Exit requires a reviewable retained report
or a precise capability failure; no snapshot or successful gate establishes
the rule, parity, universal no-write behavior or complete reading by itself.
