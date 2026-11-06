---
id: FND-PARTY-316
title: Timer interrupt restores saved DI through captured stack state and thirteen apparent stack writes are trampoline data
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:040C..4842:04DD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1464..1000:149B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:0020..5702:00F7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 573B:0020..573B:00DE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 575A:0098..575A:009D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5778:0039..5778:003E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57A6:007F..57A6:0084
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57C1:0066..57C1:006B
tool: segment_register_writes.py, trampoline_target.py and resident_listing.py at 009d20f3; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 read-only INST-handler-20261106 with ReportDecompileMatches.java; FND-EXE-281/509/562/593 and FND-TIME-012
environment: null
---

## Observation

Q-PARTY-136 asks whether producer numbers survive intervening helpers
and handlers. Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets below are hexadecimal.

### Timer preservation uses memory rather than live callback DI

FND-TIME-012 identifies interrupt 8 handler 4842:040C and its slot
callbacks. Its entry increments CS:0004, clears direction, then pushes
AX, BX, CX, DX, SI, DI, BP, ES and DS. Let S be SP on handler
entry, after the interrupt frame has been made. With admitted nonwrapping
stack storage, saved DI occupies SS:(S-0C)..(S-0A).

Only post-increment CS:0004 equal to one selects the private-stack
path. It captures SS at CS:03B6 and the post-save SP, S-12,
at CS:03B8. It sets SS from CS and SP to 03B6 before scanning
the slots and invoking the indirect callback at 0498. Thus its ordinary
callback call frames use a different selected stack from the saved DI
word, conditional on actual segment/backing separation and no wrap.
The nesting-count bypass skips these assignments and the entire scan.

After the scan, 04AA reloads SS from CS:03B6 and 04AF reloads
SP from CS:03B8. Both paths then pop DS, ES, BP, DI, SI, DX,
CX and BX, send the interrupt-controller request, and pop AX. If the
two sentinel words still match, the handler decrements its nesting
count and executes IRET. Otherwise it repeatedly enables interrupts
and requests interrupt 3; returning from that request repeats the loop,
not the IRET epilogue.

Under intact saved words, captured SS/SP, interrupt frame and normal
callback continuation, DI and DS are restored regardless of their live
values after a callback. The captured SS/SP pair is only a mechanism:
callback/alias writes to CS:03B6/03B8, changed nesting state allowing
another capture, private-stack exhaustion, original-stack aliases and
nonlocal transfers still need admission. No timer termination, scheduling,
hardware success or unconditional interrupted-frame preservation follows.
Ghidra's bounded return matches recover the sentinel branch but print
ordinary return expressions; the actual IRET and repeated interrupt-3
continuation are established by the instruction reading.

### The exploratory SS list has five code sites, not eighteen switches

segment_register_writes.py reports eighteen SS-writing decodings in
inventory ranges. The independently read startup assignment, the two
temporary list-link assignments from FND-EXE-281, and the two timer
assignments above account for five. The list-link helper performs no
call, interrupt or stack operation between its SS assignments and restores
SS before popping flags; actual header aliases remain a memory question.

The other thirteen decodings lie inside twelve five-byte trampolines:
5702 offsets 004D, 0057, 009D, 00ED and 00F2; 573B offsets
008E, 009D and 00D9; and 575A:0098, 5778:0039, 57A6:007F
and 57C1:0066. The 5702:009D row supplies two such decodings.
trampoline_target.py resolves every named start through its flags-3
descriptor and trampoline index. FMT-EXE-004 and FND-EXE-562/593
establish that the bytes after INT 3F are target data: the manager
rewrites the trampoline and resumes at its start, rather than executing
these apparent POP SS or MOV SS decodings by fall-through.

This excludes those thirteen decodings on the established trampoline
route, not arbitrary entries into changed data. The inventory-range
tool does not search every byte, prove entry ownership, or cover every
dynamic/alternate stream. Its five classified code sites are not a
whole-program absence proof. The startup SS assignment still retains
FND-EXE-509's native register-preservation condition.

## Interpretation

Q-PARTY-136 no longer needs an unspecified live-DI convention for the
timer's normal returning route or meanings for these apparent SS sites.
It needs the concrete saved-DI, captured-stack, callback-memory and
interrupt-frame conditions above. The temporary list switch likewise
has a local restoration contract, with backing aliases still open.
No queue item closes and FMT-PARTY-007 remains supported.

## Alternatives

- Every callback must preserve live DI: the handler reloads its saved word.
- Restoring SS alone restores the interrupted stack: SP is captured and
  reloaded separately, and both stored values must survive.
- Eighteen reported decodings prove eighteen runtime switches: thirteen
  decode trampoline target data on the established route.
- A printed decompiler return proves the sentinel-failure path rejoins:
  the instruction path loops after returning interrupt-3 requests.

## How to reproduce

Run segment_register_writes.py with installed DSUN.EXE and
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv. Resolve the twelve listed
trampoline starts with trampoline_target.py and apply FMT-EXE-004's
entry contract. Reuse FND-EXE-281/509 for the list/startup controls.
Read resident_listing.py range 4842:040C..4842:04DD, tracking each
push, the capture epoch, the two stack reloads and both sentinel exits.
Do not interpret the exploratory DS-write totals as validated instructions.

Retained getter-stack-segment-writers.txt, getter-stack-stub-controls.txt,
getter-timer-stack-continuation.txt and getter-timer-stack-ghidra reports
are under GAME_DIR/analysis/handler-census-20261106/. No original game,
emulator session or emulated call supplied evidence here.
