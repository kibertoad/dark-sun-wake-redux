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

For FND-SOUND-028's remaining source-span question, add an optional --spans
mode to the existing SOUND.INI profiler. Keep its default output unchanged.
Reuse its identity and structural admission, then measure the five string
fields' byte lengths and terminator-inclusive lengths per source record,
and each of the five alternate families' count values and primary/alternate
key occurrences. Report duplicates, missing values and long strings as
metadata, not as simulated native outcomes. Bound every numeric count token
and the record-key census; reject unsupported numeric syntax explicitly.
Never emit a source name or driver value. Synthetic fixtures cover empty,
boundary and over-slot strings, repeated/missing keys, signed/hex counts,
count-token limits and unknown keys. Exit: tooling gate passes, then a
separate research batch measures all installed records twice and compares
source spans against FND-SOUND-028, keeping native retention separate.

For Q-CONFIG-006's remaining installed token/line admission, extend the same
profiler with optional --inputs metadata. Existing default and --spans output
stay unchanged. Reuse the exact installed identity and source structural
admission, then measure LF-delimited physical lines, the first bracket-to-
semicolon copied span, first tag length, terminal LF and unexpected control
bytes. Keep brackets in source-ignored comment/separator lines visible by
line number; do not silently equate the source grammar with native delivery.
For the fixed known numeric keys, report source-spelling length/base/value
and length of its initial digit/minus/x alphabet prefix, without emitting
source text, unknown tag names or numeric token strings. This is source
measurement, not numeric-conversion or native-parser replay. Bound total
lines to 4096 and numeric spelling to 64 bytes; reject unknown numeric syntax
or limits explicitly. Synthetic controls cover decimal/hex/sign boundaries,
hex letters outside the extractor alphabet, comments containing brackets,
CRLF versus LF, missing final LF, line/tag/copy widths, controls and caps.
Exit: tooling gate and synthetic controls pass; a separate research batch
measures the installed file twice and compares spans to the existing static
helper reading, retaining nested context, storage and conversion conditions.

For Q-CONFIG-006's remaining nested source consumption, add optional --contexts
to the same hash-locked profiler. Existing modes remain unchanged. Validate a
bounded source profile against FND-SOUND-028's consecutive-key bands: Flags
and Jumpers consume their declared following keys, and each alternate family
has its preceding positive count and exactly count-minus-one consecutive
alternate keys. This is conditional source conformance, not a replay of native
file/heap behavior. Restrict counts to short decimal spellings, alternate
families to at most ten total slots, flags to the recorded five flag names and
jumpers to the eight recorded native names; reject unsupported patterns with
no source text in diagnostics. Require terminal LF and no source-ignored
brackets/control-byte discrepancy. Cap lines/record keys at 4096/512, preserve
line numbers, count all outer and nested keys separately and emit only metadata.
Synthetic controls cover zero lists, wrong/truncated nested keys, wrong alternate
family/count/order, repeated bands, unknown outer keys, profiles beyond counts
or caps and missing LF. Exit: selected tooling gate passes; a separate research
batch measures all installed records twice and compares source-band accounting
with the native group/record loop reading. Native context/state retention and
partial-failure paths remain distinct even when this source profile conforms.
