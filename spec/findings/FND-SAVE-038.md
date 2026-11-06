---
id: FND-SAVE-038
title: Failed Save returns its retained flag after reverse conversion whose resource reads prepare their supplying or indexed-probed archive
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070713..0x0007089C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071659..0x00071869
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:04D5..38FF:07B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1..38FF:09D6
tool: connected evidence review at 2c8fa5e8; retained overlay_listing.py report from FND-COMBAT-122 and FND-SAVE-023; FND-COMBAT-120/151 and FND-CONFIG-151/165/208 interfaces
environment: null
---

## Observation

This answers the explicit preparation and result-propagation parts of
RULE-SAVE-001/Q-SAVE-007's failed-writer return suffix. It reuses the
connected complete local readings rather than repeating searches. Source:
installed DSUN.EXE,634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Overlay187 offsets are relative to6FE30.

### Release precedes conversion; the returned flag is separate

After the resource loop, +09F2 pushes the current bitmap pointer DS:55B8
to444C:0092. It stores returned DX:AX back into that pointer field. The
wrapper's normal zero pair is FND-CONFIG-165; it is not proof of release
or of an unchanged heap. This call precedes the later script conversion.

The retained success byte at BP-1 then gates the optional STXT operations.
Zero skips them and joins+0A5B. That join passes the saved conversion
flag's low byte to+1829 and, after its returning continuation, loads AL
from BP-1 at+0A65 before restoring the frame and returning. There is no
own test of the converter's result or of global error DS:9D99 at that point.

Under an intact caller frame, a retained zero success byte therefore
still returns AL0 after a successful conversion or error-word reset.
Converter/guard nonreturn, changed locals and other child effects remain
distinct. FND-SAVE-037 supplies the outer wrapper's subsequent close/copy
bypass; this finding does not repeat that wrapper reading.

### GPLI requests do not imply preparing the active working record

FND-COMBAT-151's converter separately queries GPLI/1's size through
38FF:05B5 and requests its data through04AB. Each lookup starts from the
then-current active pointer. FND-COMBAT-120 distinguishes the fallback
cursor from global DS:9D9F: traversing another provider does not itself
select that provider globally. The two requests need not select the same
record if state changes between them.

FND-CONFIG-151's plain size lookup returns an entry's stored length
without its own archive preparation or payload read. The data request
prepares the eventual local supplying record before its seek/read. Its
argument is not unconditionally the working record merely because that
record was globally active at the start.

There is an important additional preparation opportunity: indexed lookup
can call08FB on a probed record before reading its index pair. A later
lookup failure can then fall back to another record (FND-COMBAT-120).
Consequently the final provider alone cannot exclude earlier preparation
of an indexed working record.

For a particular working record W, the precise local cases are:

| W's lookup path | Preparation attributable to these GPLI requests |
| --- | --- |
| No GPLI type in W at both requests | Missing-type fallback supplies no own08FB call for W |
| Plain GPLI type but no requested number in W | Plain-number miss supplies no own08FB call for W |
| Indexed GPLI path in W reaching index-pair I/O | Can prepare W before a later miss/fallback |
| W supplies the admitted data entry | Data transfer requires preparation of W first |

These are conditional lookup paths under valid stable records, directories,
frames and admitted children. They are not a census of every writer or
alias. Shipped GPLI/1 occurs in GPLDATA.GFF (FND-SAVE-023), but that file
fact alone does not establish W's current directory or either live provider.

### Error-word reset is not pending-byte clearing

The matched signature helper writes zero to DS:9D99
(FND-CONFIG-208); these lookup wrappers call it anew. That error word
is separate from W's pending byte+2C and from the writer's BP-1 result.
An ensuing error0 therefore proves neither that metadata was prepared nor
that the numbered writer returns success. FND-COMBAT-124 identifies the
actual preparation checks and pending-byte success clear.

The converter's own data stores are its local match state, chained-record
entry/script pairs and selected trigger entry/script pairs; normal completion
also requests release of its table buffer (FND-COMBAT-151). The writer
suffix adds the bitmap-pointer store described above. Composing these as
preserving W requires the named table, bitmap, chain, trigger, stack and
release-metadata spans not to alias W's record/directory, and the reached
guards, allocators, releases and services to respect that binding. No native
failure or such complete span admission is asserted here.

## Interpretation

Reverse conversion is not an unconditional working-archive flush or a
replacement for the writer's retained failure result. The remaining interval
audit can check W's actual GPLI lookup path and preparation events, then the
named release/write spans and later message/callback work. It need not
retain an unidentified flush-all or global-error-as-success mechanism in
this suffix. Q-SAVE-007 remains open for actual failed state and preservation,
other producers and shutdown/file bindings; no status or parity is promoted.

## Alternatives

- A fallback provider is not automatically the new global active record.
- A final non-W provider does not exclude earlier indexed preparation of W.
- A successful signature check clears an error word, not pending metadata.
- Normal converter completion does not replace the caller's saved failure byte.
- Absence of an explicit flush of W does not exclude aliased or child writes.

## How to reproduce

Reuse GAME_DIR/analysis/hit-incoming-20261104/:
restore-save-remove-write-caller.txt, restore-script-reverse.txt and
restore-resource-fallback.txt, using the fingerprint and current locked
evidence Python. overlay_listing.py regenerates descriptor187, 70713 7089C
and71659 71869. resident_listing.py regenerates the listed lookup ranges;
reuse FND-CONFIG-151/208 for the size/signature wrappers and
FND-COMBAT-120 for indexed preparation/fallback. Check bitmap release before
the STXT gate, shared conversion join, final AL load, local provider cursor
and the separate error/dirty fields. Preserve storage and external-effect
conditions. Keep all original-derived reports local; no original code ran.
