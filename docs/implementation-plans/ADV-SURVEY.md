# Disc ADV envelope Survey

Research tooling for Q-SOUND-007 and FMT-SOUND-004. Existing FND-EXE-491/492,
FND-EXE-496 and FND-EXE-590 already identify the Copy signature, first-word
function-directory pointer, number/offset pairs and terminal FFFF number,
plus the setup/game loader distinction and selected embedded-resource matches.
Reuse that evidence instead of decoding all driver procedures again.

Add a bounded metadata-only function-directory inspector. Admit at most
65536 bytes, require Copy at offset two, a directory after that signature,
complete little-endian pairs, distinct function numbers and in-file target
offsets, and a terminal number FFFF. Read no target word after the terminal
number; accept aliased targets. Cap the inspection at 256 entries as an
explicit tool limit, not an original-game bound. Keep all other bytes opaque.

Reuse disc_adv_exec_census.py's existing ISO collection in the CLI, suppressing
its unrelated printed census. Check exactly the manifest's nineteen CD:*.ADV
size/hash identities before interpreting any envelope. Do not execute, load
or call a driver; retain metadata reports and any original bytes in GAME_DIR.
Synthetic controls cover short/truncated headers and rows, a two-byte final
sentinel, invalid pointers/targets, duplicate function IDs, aliases and caps.
Run Test.ps1 before a tooling commit.

In a separate research batch inspect all nineteen files twice and compare
reports. Record counts/structure in spec, add the required bounded schema and
promote only the envelope supported by evidence. Remove that part from
Q-SOUND-007; retain actual setup record/name-selection and any unresolved
loader paths. A known signature or complete directory does not establish
driver behavior, complete code mapping, hardware effects or successful load.
Exit: every manifest driver has a classified bounded envelope, schema and
format agree with the findings, and research/queue/parity checks pass.

For the remaining setup-name producers shared with Q-CONFIG-006, extend the
existing resident_listing.py to accept the independently fingerprinted
SOUND_DS.EXE as well as DSUN.EXE. Keep the same load-at-1000 relocation
model and output notation; no unknown-source fallback. Validate requested
same-segment ranges against the actual load image before slicing, refusing
negative/outside/wrapping spans. Synthetic range controls and the tooling
gate precede any new setup reading. Original listings stay in GAME_DIR.

For Q-SOUND-010's actual source-label conditions, add a metadata-only
SOUND.INI profile reader from FMT-CONFIG-002's supported text layout.
Check the installed file's exact size/hash before reporting. Bound text to
128 KiB, lines to 4096 bytes, each group to 256 records and each label to
256 bytes. Validate the three group/record boundaries, declared counts
(including the empty -1 group), one CardName per record and bracketed values;
do not interpret other keys or model the original parser's failure behavior.
Report counts, label length/printability and derived text footprints only,
never names. Classify the FND-SOUND-022 positive small-list profile separately
from empty, larger or other labels; those inputs are not silently admitted.
Synthetic controls cover boundaries, duplicates, count mismatch, negative
groups, non-ASCII/control/long labels and caps. Commit tooling after Test.ps1,
then measure the licensed source twice in a separate research batch. Source
text conformance does not prove native parsing, vector state or child effects.
