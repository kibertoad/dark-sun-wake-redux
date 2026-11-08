# Fixed disc initialization observation

Q-CONFIG-014 asks which music-level value Preferences can read on admitted
startup paths. FND-SOUND-092/093/094/095 already describe the local profile
gates, callback selection/results, retained-buffer selection and disc-table
write contract. The next missing evidence is actual loaded configuration,
callback response and pointer/metadata admission on the unchanged installed
startup. This observation does not introduce gameplay behavior or establish
audible output. RULE-CONFIG-003 remains supported until its whole reading
qualifies.

Existing start_timer.py uses timed advancement, optional configuration changes
and broad timer observation. startup_window.py provides current session,
source-copy and stop helpers but does not read this callback. Add a separate
fixed disc_startup.py probe using those helpers, not an arbitrary breakpoint,
memory, argument or program runner.

The probe verifies the approved executable and uses probe_drive.py's complete
manifest verification and private unchanged C: copy. Mount the installed
game.ins read-only through RustDosSession with pinned rust-dos and default
muted host output, the documented Sound Blaster settings and deterministic
cycles. Permit no input, configuration mutation, guest writes or checkpoint
restoration. Close the owned session on success or failure; never wait on a
held machine run lock.

Observe only the documented initializer entry, pre-callback gate and call,
known callback entry, presence/metadata returns, table-allocation call/return,
completed per-track iterations, final table write before/after, callback return,
join and later music gate. Stop at that gate, before music-driver work.
Install the generator and seed-setter guards before the first continuation;
an unexpected draw or seed boundary stops rather than proceeding unrecorded.
Record initial seed, no setup writes and every reached stop in GAME_DIR.
No conclusion about an unhit branch follows without its selecting state.

Named reads require real mode and the expected relocated DS/SS at each stop,
nonwrapping offset and linear ranges, exact byte counts and nonnull bounded
configuration backing. Read only the documented config pointer/59-byte config,
music-enable/slot/handle and retained-buffer fields, callback/table pointers,
disc presence/enable/metadata/first/last/lead-out fields and RNG state.
At known callback entry require its exact caller return pair. At allocation
call capture only its four-byte size argument. The final table write may
read its four-byte target before and after the instruction, with admitted
pointer arithmetic; this measures bytes, not allocated extent. A real-mode
readable address is not evidence of heap capacity or disjointness.

Pair final write observations by their actual table pointer and last-track
value. Reject a changed pair instead of comparing different storage. Cap the
whole probe at600 named stops and use the existing bounded same-operation
observer; unknown stops or exhausted budgets fail with a retained report.
No extra continuation is issued because observing an operation timed out.

Synthetic controls cover each declared point, wrong stop/mode/DS/SS/return,
short reads, null or wrapping configuration backing, allocator argument
width, final-target arithmetic and changed-pair refusal, unknown/RNG/seed
stops, bounded repeated iterations and endpoint completion. These controls
do not admit a native run. Exit for tooling: controls and Test.ps1 pass and
the fixed command is documented. A subsequent research batch attempts the
run, records actual runtime parts and narrows only obligations its evidence
settles. Native capacity, unobserved writers and other profiles remain open.

## Value history

For Q-CONFIG-014, add only an optional fixed `history` mode to the existing
fresh-drive observer. Reuse SOUND-085/092/096 and CONFIG-245's named-field
watch mechanism. Start at the checked program-start stop, before its first
continuation, and end at the existing first music gate. Observe the eight
fixed relocated game/library fields used by the Preferences history and
the two-byte internal driver-enable word at DS:3444. Omit the derived card
watch: the initially null configuration pointer is watched only as a four-
byte value and never selects a watch target. Require the admitted initial
null pointer and driver-enable one. No arbitrary ranges, new inputs, guest
writes, seed changes, additional execution targets or continuations.

Reuse changed-value matching, ordered real-mode stops, full named snapshots,
overlap/bounds refusal, unexplained-change rejection and owned-watch cleanup.
Ordinary startup stops retain their existing DS/SS, frame, phase and paired-
write guards. Watch stops may occur between them, with their actual reasons
and post-instruction locations retained. RNG/seed guards still stop before
execution; unknown stops or exhausted observations fail. Successful history
requires both actual pointer publication from zero and driver-enable one to
zero as positive four-/two-byte primary-watch events, the known music-gate
endpoint and cleanup. The report adds optional value_history without changing
the legacy command or report. It covers changed instruction-boundary values,
not unchanged writes, intra-instruction effects, unmonitored configuration,
later startup/map/Preferences history or continuous segment identity.

Acceptance: new synthetic controls for exact fixed startup fields, initial
state and program-entry refusal, pointer changes without target following,
required positive widths, watched events mixed with declared startup stops,
unknown watches and missing endpoint/cleanup; all existing Preferences and
startup controls remain passing. Run Test.ps1 Research before tooling commit.
Exit: bounded `disc_startup.py <new-run-name> history` command and passing
controls. A separate research batch attempts two independent fresh runs,
records only observed capabilities/results and narrows the exact prefix;
it does not join separately restored observations into one history.

Synthetic commands use the checkout-local evidence Python with `-I`:
check_startup_history.py, check_disc_startup.py,
check_preferences_history.py and check_preferences_readback.py in
tools/research/rust-dos-runs/. Native history capability remains unverified
until the separate attempt; arming a range is not positive mutation delivery.
