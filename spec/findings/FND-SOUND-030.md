---
id: FND-SOUND-030
title: Setup byte reader separates unsigned buffered bytes from failure and retains distinct refill flags
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2EB3..1000:2F69
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2F81..1000:306F
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3720..1000:37F1
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0BF9..1AF6:0C8F
tool: tools/research/exec-census/resident_listing.py at 62ef9c8f; Python 3.12.10, Capstone 5.0.7
environment: null
---

## Observation

Q-CONFIG-006 retains byte delivery beneath FND-SOUND-025's line reader.
This reads that reader's actual far-call target, its buffered refill and
the direct-read branch. Offsets and masks below are hexadecimal; byte
value bounds are decimal. All stream accesses require a valid retained
far pointer and backing storage; these are conditional local contracts.

The line reader calls 1000:2F81 with the far pointer at DS:1888/188A.
That helper returns FFFF for a zero far pointer. Otherwise it first tests
the signed remaining-count word at stream offset 00. A positive count
bypasses all flag gates: it decrements the count, takes the current far
buffer pointer from 0C/0E, increments its offset word only, and returns
the selected byte zero-extended to AX. There is no segment carry or
per-byte text conversion on this branch. NUL is a successful byte zero,
distinct from FFFF; bytes 128 through 255 also remain unsigned.

For a nonpositive count, a negative count, either flag in mask 0110,
or absence of flag 0001 takes the failure path, ORing 0010 into stream
flags at offset 02 and returning FFFF. Otherwise it ORs 0080 into those
flags. A nonzero buffer-size word at 06 chooses refill 1000:2EF1.

The refill optionally calls 2EB3 when flag 0200 is set, then resets the
current pointer 0C/0E from base pointer 08/0A before calling 3720 with
the signed byte handle at 04, base pointer and size word 06. It stores
the child's AX at count offset 00 before testing it as signed:

| Child result | Count and flags after the refill | Refill return |
| --- | --- | --- |
| Positive | Count retained; clear 0020 | Zero |
| Zero | Count zero; clear 0180 and set 0020 | FFFF |
| Negative | Replace count with zero; OR 0010 | FFFF |

On zero refill return, the byte reader re-enters its decrement/load
branch, so the first returned buffered byte is consumed exactly once.
On nonzero refill return it returns FFFF directly, preserving the
refill's distinct zero-result flags rather than taking its own 0010 path.
2EF1 removes its own four argument bytes on return; its caller adds no
second cleanup. 2EB3 scans twenty entries at DS:DBA8 with stride 14,
calling 292B only when an entry's flags contain both bits of 0300.
The effects of that child are not established by this reading.

With buffer size zero the byte reader requests one byte into DS:EDFA
through 3720, optionally after the same 2EB3 call. A nonzero result
continues to the byte, except CR is discarded and another request made
when stream flag 0040 is clear. It clears 0020 before returning the byte
unsigned. Notably this branch tests nonzero, not positive: FFFF from
3720 does not itself take the zero-result path. A zero result instead
calls 27FC with the handle; result one clears 0180 and sets 0020, while
any other result takes the byte reader's 0010 failure path. Both return
FFFF. The effects and DOS-dependent outcomes of 27FC remain open.

The inspected 3720 body validates its handle against DS:DD38, can return
zero from handle-table/count gates, and calls 06FF for actual reading.
Its optional 4000 flag branch removes CR and handles byte 1A, including
further 06FF and 05CC calls. Its returned count is therefore not generally
the raw DOS count. This finding does not establish those children's
effects, table initialization or actual stream mode.

## Interpretation

Under these storage and child-result conditions the byte reader supplies
unsigned bytes or FFFF to the line reader. FND-SOUND-025's line reader
compares the full AX result to FFFF, so an ordinary NUL is not that
failure sentinel. Buffered refill zero and negative outcomes leave
different flag states even though both report FFFF. This removes the
local byte extraction and refill-result dispatch uncertainty from
Q-CONFIG-006, without admitting native file delivery or successful setup.

## Alternatives

Assuming all branches reject a negative read result contradicts the
unbuffered nonzero test. Assuming the byte helper always strips CR
contradicts its buffered fast branch; the lower reader may already have
translated input. Treating the direct path's scratch byte as fresh after
any nonzero child return would require a child-write contract not proved
here. Stream construction, handle/base/size/flag producers, buffer and
line spans, offset wrapping, aliases and preservation, flush effects,
lower read/translation/seek/error contracts and OS outcomes remain
Q-CONFIG-006. No original was run and no complete reading or status
promotion is claimed.

## How to reproduce

Use the approved installed SOUND_DS.EXE, length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Run the committed resident_listing.py
with -I and the four location ranges above, in their listed order.
Retain output under GAME_DIR/analysis/disc-adv-survey-20261104/ as
setup-file-byte-refill.txt and setup-file-byte-refill-repeat.txt.
Both UTF-8 reports have SHA-256
2c94c0bb0a368291df193914779e914ca06d2293a68c56059e00add284296e21.
The range beginning 2EB3 includes the reached refill entry 2EF1;
the byte reader ends at its far return 306E. Follow incoming calls from
the established line reader and both returns from refill, keeping the
positive-count bypass, signed result tests and direct-path nonzero test
distinct. Keep all original-derived listings in the local evidence store.
