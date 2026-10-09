---
id: FMT-EXE-006
title: BAT launch files
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["RAVAGER.BAT", "SOUND.BAT", "CD:*.BAT"]
byte_order: null
size: null
text: true
definition: null
evidence: [FND-EXE-008, FND-EXE-009, FND-EXE-010]
conflicting: []
split_with: []
related: []
---

## Layout

| Key | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|
| line | byte text | command_text | A command, label, comment or blank line terminated by CRLF | supported | FND-EXE-008 |
| EOF | file boundary | end | Immediately after the final CRLF, without a byte-26 marker | supported | FND-EXE-008 |


The seven manifest-listed files are CRLF-delimited command text without NUL
or DOS end-of-file marker bytes. Six contain only ASCII bytes. Disc
`SOUND.BAT` additionally uses bytes 185, 186, 187, 188, 200, 201, 204 and
205 in display commands; their intended code page remains unknown.

Installed `RAVAGER.BAT` names `DSUN` with options `-W0` and `-L`, then exits
the shell. Installed `SOUND.BAT` suppresses command echo, displays a notice,
copies `D:\*.ADV` into the current directory, names `SOUND_DS`, deletes
current-directory `*.ADV`, and exits. Copy and delete output goes to NUL;
there are no conditional result checks or remembered copied-file list.

Disc `AA.BAT`, `AC.BAT`, `CA.BAT` and `CC.BAT` display notices and pause
before naming `C:\DOS\EDIT` with, respectively, `A:\AUTOEXEC.BAT`,
`A:\CONFIG.SYS`, `C:\AUTOEXEC.BAT` and `C:\CONFIG.SYS`.

Disc `SOUND.BAT` selects its ULTRADIR path when that variable is nonempty;
otherwise it tests ARIA. With neither set, it names `SOUND_DS` without
installing a TSR. The ULTRADIR path tests MIDI patch-file presence and
selects `UM200.ini`, `UM206A.ini` or `UM206.ini` for copying to `ssi1.ini`.
The tests, ERRORLEVEL thresholds and ordering are recorded in FND-EXE-008.
The 200 and 206A success paths jump to the undefined `G_SUCCESS` label.
The 206 path reaches the Ultramid load command; its successful fall-through
names `SOUND_DS`, then unloads Ultramid. Error-message paths reach the
shared cleanup that clears ULTRASND and BLASTER and then selects non-TSR
setup. ARIA selects the GM2 bank load command and a threshold-1 branch
whose target token is `INS_GM1:` while the label is `INS_GM1`. Its ordinary
fall-through names `SOUND_DS` and unloads that TSR. The GM1 label loads
the smaller bank then jumps to undefined `RUN_ARIA`. These describe the
shipped command text, not an observed execution or shell error outcome.

The disc's helpers are not studied further. The disc's `SOUND.BAT` is the
version 1.0 copy that the installation replaces (FND-EXE-570), and the
other four open DOS startup files in an editor and are named by no
command of GOG's launch configuration (FND-EXE-010); whether the game
launches any helper remains Q-EXE-007. Their display encoding, how an interpreter
handles the disc `SOUND.BAT`'s undefined labels, and which disc helpers the
disc installer launches are left unread.

The distribution's declared primary task selects a DOSBox configuration
with separate game and setup branches (FND-EXE-010). Its menu admits choices
123 and tests ERRORLEVEL thresholds 3, 2 and 1 in that order, selecting exit,
setup and game labels respectively. After declaring installation and overlay
mounts on C and the disc mount on D, and changing to C, its game label names
bare `ravager`; its setup label names bare `sound`. Those names match the
installed helper stems. The text after them jumps to exit and launcher,
respectively, but resolution and actual continuation remain open. The named
installed helpers themselves both end with EXIT.

## Enumerations and flags

No binary enumeration or byte-order field applies to this text listing.
The meanings of executable command-line options are outside this entry.

## Differences between builds

None known. Installed and disc sound scripts differ within this build.

## Coverage

Complete command-text reads of `RAVAGER.BAT`, `SOUND.BAT`, and the five
`CD:` `.BAT` files of BLD-GOG-EN-1.1 (FND-EXE-008, FND-EXE-009). No complete
reading of callers, external commands or interpreter behavior is claimed.

## Open questions

- Does the shipped game or sound-setup executable launch any batch helpers
  (Q-EXE-007)? Automatic invocation and manual-only use both remain possible.
  FND-EXE-008 describes helper contents; FND-EXE-010 establishes wrapper
  command branches, not executable callers. Direct executable launch
  references traced through their inputs settle it.
  FND-EXE-350 locates literal sound.bat names in both game editions and
  an autoexec.bat suffix in the sound utility. These are consumer-reading
  leads, not launch evidence; their writers and consumers remain Q-EXE-007.
  FND-EXE-360 traces the utility's stack pathname into the interface also
  used for sound.ini. Its deeper callees, segment/input admission and later
  paths still prevent an execution exclusion; the game consumers remain unread.
  FND-EXE-353 connects the prefix-byte read to the wrapper's returned-DX
  store and separates segment/input records; interrupt results, unwritten
  inputs, error continuation and downstream file operations remain open.
  FND-EXE-354 reads the pointer selector's signed gate and post-increment
  bound comparison; record/count admission, the other near callee and
  later pathname operations remain unresolved.
  FND-EXE-370 narrows the utility route to conditional attribute/open service
  selectors. Record bounds, incoming saved-register data, interrupt preservation
  and remaining handle/buffer callees still prevent complete admission.
  FND-EXE-355 follows the selected mode through ordered record stores and
  downstream request/configuration calls; their effects, storage admission
  and later pathname consumers remain unresolved.
  FND-EXE-356 resolves the local returned-bit helper and its unchecked
  interrupt continuation; native results, record admission and remaining
  configuration/cleanup callees stay open.
  FND-EXE-357 reads the configuration helper's distinct failure prefixes
  and buffer publication; its allocation/cleanup callees, preservation and
  state admission remain under Q-EXE-007 before an execution exclusion.
  FND-EXE-358 reads cleanup's distinct result/store paths; remaining helper
  effects, SI/stack preservation and record admission stay under Q-EXE-007.
  FND-EXE-359 resolves the retained-word numeric conversion and local stack
  cleanup; destination admission and other helper contracts remain Q-EXE-007.
  FND-EXE-361 traces formatting's supplied destination through an unchecked
  bounded scan/copy; storage, termination, extents and aliases remain Q-EXE-007.
  FND-EXE-362 reads final cleanup's carry-dependent return and error-state
  mapping; native preservation and table/segment admission remain Q-EXE-007.
  FND-EXE-363 records preliminary cleanup's pre-call mutations and flag-dependent
  result comparison; downstream contracts and state admission remain Q-EXE-007.
  FND-EXE-364 reads the downstream count guard and direct request wrappers;
  byte-processing and native/state admission remain under Q-EXE-007.
  FND-EXE-365 reads byte expansion, batching and return arithmetic;
  native preservation and frame/source/extent admission remain Q-EXE-007.
  FND-EXE-366 reads preliminary cleanup's selected-call iterator; segment,
  table/flag state, preservation and re-entry admission remain Q-EXE-007.
  FND-EXE-367 reads the bit-four buffer helper's segment dispatch and matching
  path; alternate/terminal contracts and shared/link state remain Q-EXE-007.
  FND-EXE-368 reads the alternate buffer path's segment and field updates;
  topology/extent/alias and shared-state admission remain Q-EXE-007.
  FND-EXE-369 reads the terminal helper's pair gate and sentinel/state paths;
  stored bounds/base and native/extent admission remain Q-EXE-007.
  FND-EXE-371 reads buffer quantity, candidate search and exact/split returns;
  growth/unit/extent and topology/shared-state admission remain Q-EXE-007.
  FND-EXE-372 reads growth request ordering, alignment and segment publication;
  lower-helper, preservation and extent/state admission remain Q-EXE-007.
  FND-EXE-373 reads the lower quantity helper's gates and saved-prior return;
  input, bound/state writers, native and frame/extent admission remain Q-EXE-007.
  FND-EXE-374 identifies neighboring shared-state producers and copy arithmetic;
  callers, remaining helpers, headers and extent/state admission remain Q-EXE-007.
  FND-EXE-375 reads the smaller-buffer helper's pre-call stores and retained return;
  callers, field/state writers, native outcomes and extent admission remain Q-EXE-007.
  FND-EXE-376 reads preliminary configuration, adjusted scan count and pair test;
  producers, extents, frames and native/state admission remain Q-EXE-007.
  FND-EXE-377 follows the pathname's byte matcher and position continuation;
  later consumers, reader/position contracts and state admission remain Q-EXE-007.
  FND-EXE-378 reads position-helper request ordering and signed adjustment;
  reader, producers, native preservation and later consumers remain Q-EXE-007.
  FND-EXE-379 reads the reader root's byte/full-word returns and refill loop;
  lower callees, producers, extents and later consumers remain Q-EXE-007.
  FND-EXE-471 reads refill publication and the fixed-count preliminary pass;
  lower reads, producers, extents and later consumers remain Q-EXE-007.
  FND-EXE-472 reads lower count/byte processing and unchecked extra-result use;
  native admission, producers, extents and later consumers remain Q-EXE-007.
  FND-EXE-473 reads the zero-read classifier's flag and native-pair paths;
  native/state admission and later pathname consumers remain Q-EXE-007.
  FND-EXE-474 reads later pathname byte-copy and reverse-search tests;
  continuation, index/frame admission and extents remain Q-EXE-007.
  FND-EXE-475 identifies the second filename and sequential configuration markers;
  later conversion/consumers and segment/string admission remain Q-EXE-007.
  FND-EXE-476 reads field-byte collection, fixed mapping and second conversion store;
  converter, later consumers and input/state admission remain Q-EXE-007.
  FND-EXE-477 reads the converter's decimal-prefix and modular-width behavior;
  runtime table/source admission and later consumers remain Q-EXE-007.
  FND-EXE-478 reads remaining field collection and unequal result validation;
  retained-word consumers and input/preservation admission remain Q-EXE-007.
  FND-EXE-479 reads retained-word publication through repeated table searches;
  table/field admission and later continuation remain Q-EXE-007.
  FND-EXE-480 reads final buffer publication and unchecked optional-call result;
  formatter, source/global extents and optional consumer remain Q-EXE-007.
  FND-EXE-481 reads formatter flush and callback descriptor advancement;
  conversion dispatch, total output and state admission remain Q-EXE-007.
  FND-EXE-482 conditionally binds plain string dispatch and default padding;
  source/table admission and remaining consumer paths remain Q-EXE-007.
  FND-EXE-483 reads optional consumer text collectors and signed traversal;
  producer/frame admission and later consumer paths remain Q-EXE-007.
  FND-EXE-484 reads coordinate arguments and signed spacing arithmetic;
  downstream effects and count/object admission remain Q-EXE-007.
  FND-EXE-485 reads spacing recurrence and row-interface argument provenance;
  interface effects, record consumers and state admission remain Q-EXE-007.
  FND-EXE-486 reads remaining local selector and return paths;
  input/interface effects and state admission remain Q-EXE-007.
  FND-EXE-487 reads row registration and local polling mode selection;
  readiness/table admission and downstream modes remain Q-EXE-007.
  FND-EXE-488 reads downstream byte/coordinate modes and scan sentinel;
  readiness, native input and table/count admission remain Q-EXE-007.
  FND-EXE-489 binds readiness and coordinate consumers to returned registers;
  native input/segment admission and interface effects remain Q-EXE-007.
  FND-EXE-499 identifies both game editions' containing diagnostic and
  its positive consumers; actual DS and deeper call effects remain Q-EXE-007.
  FND-EXE-500 reads selected byte dispatch and character write paths;
  record/native admission and broader caller coverage remain Q-EXE-007.
  FND-EXE-502 reads signed error mapping and positioning request paths;
  state/native admission and remaining output callees remain Q-EXE-007.
  FND-EXE-503 reads local flush paths and aggregate result disposal;
  3DCC, record/state admission and broader coverage remain Q-EXE-007.
  FND-EXE-504 reads 3DCC's guards, expansion and short-write returns;
  DS/SS, record/state admission and broader coverage remain Q-EXE-007.
  FND-EXE-507 reads remaining local byte-dispatch branches and copy/fallback
  helpers; actual state/segments and broader callers remain Q-EXE-007.
  FND-EXE-508 records shipped diagnostic and initial handle-table values;
  runtime segment/writer admission and broader callers remain Q-EXE-007.
  FND-EXE-509 traces early saved-segment, DS/SS and zero-fill producers;
  native preservation and later state/writer admission remain Q-EXE-007.
  FND-EXE-510 reads the startup priority dispatcher and shipped targets;
  callee effects, segment/state admission and broader callers remain Q-EXE-007.
  FND-EXE-511 identifies an early conditional diagnostic-flag writer;
  0705/364E, state admission and remaining callers remain Q-EXE-007.
  FND-EXE-512 reads returned-bit masking and initialization stores/failure;
  native and deeper callee contracts remain Q-EXE-007.
  FND-EXE-513 reads allocation-wrapper segment and traversal obligations;
  deeper allocation and state admission remain Q-EXE-007.
  FND-EXE-514 reads selected-block and shared-head mutation helpers;
  block/storage producers and remaining initialization remain Q-EXE-007.
  FND-EXE-515 reads immediate block publication and sentinel paths;
  0F68 and storage/state admission remain Q-EXE-007.
  FND-EXE-516 reads shared-offset request guards and returns;
  initial offset, segments and storage/state admission remain Q-EXE-007.
  FND-EXE-517 records the shipped offset seed and explicit margin setter;
  caller/writer and runtime storage admission remain Q-EXE-007.
  FND-EXE-518 reads one setter caller's prior global/link mutations;
  alternate cleanup and remaining writer/state admission remain Q-EXE-007.
  FND-EXE-519 reads alternate cleanup and link-helper fall-through;
  block/state admission and remaining callers remain Q-EXE-007.
  FND-EXE-526 reads the other positive setter-call lead and outer caller;
  grow/copy and input/storage admission remain Q-EXE-007.
  FND-EXE-527 reads the grow branch's allocation, word copy and cleanup;
  outer callers and storage/segment admission remain Q-EXE-007.
  FND-EXE-528 reads the positioning dependency's buffered adjustment and
  state-before-native-call ordering; record/native admission remains Q-EXE-007.
  FND-EXE-529 reads the earlier first-priority startup target and its relocated
  segment operands; its callees and startup admission remain Q-EXE-007.
  FND-EXE-530 reads the first startup helper's fixed traversal and bounded
  quantity producer; actual table/segment admission remains Q-EXE-007.
  FND-EXE-531 reads the resident startup request wrapper and pair return;
  its nested callees and shared-state admission remain Q-EXE-007.
  FND-EXE-546 reads its exact-size and split helpers' segment stores and
  returned pairs; remaining producers and callees remain Q-EXE-007.
  FND-EXE-547 reads both block producers' alignment tests and header stores;
  their common request callee and storage admission remain Q-EXE-007.
  FND-EXE-548 reads that request's arithmetic, normalized comparisons and
  captured prior-pair return; 177C and state admission remain Q-EXE-007.
  FND-EXE-549 reads the publisher's current/upper-pair writes and native
  wrapper results; native contracts and initial producers remain Q-EXE-007.
  FND-EXE-550 reads shipped pair/cache zeros and the pre-dispatch upper-word
  writer; native preservation and complete writer coverage remain Q-EXE-007.
  FND-EXE-551 reads startup failure's selected cleanup and native request;
  native contracts and other cleanup callers/targets remain Q-EXE-007.
  FND-EXE-555 reads general cleanup's reverse-priority dispatcher and shipped
  callbacks; target bodies and runtime writers remain Q-EXE-007.
  FND-EXE-556 reads the manager cleanup target and mutable cache callbacks;
  complete writers, driver/native contracts and registration remain Q-EXE-007.
  FND-EXE-557 reads general callback registration's equality guard and ordered
  target/count writes; all callers, writers and input admission remain Q-EXE-007.
  FND-EXE-558 supplies a relocated registration caller and callback sequence;
  complete callers/writers and downstream effects remain Q-EXE-007.
  FND-EXE-559 reads its slot dispatcher and native carry-mapping wrapper;
  indirect targets, native effects and state admission remain Q-EXE-007.
  FND-EXE-391 reads one type-selected cleanup-slot producer and its native
  cleanup target; other producers and input/native admission remain Q-EXE-007.
  FND-EXE-392 traces its preliminary output writers and companion word return;
  native preservation, complete callers and other producers remain Q-EXE-007.
  FND-EXE-393 reads type-two slot publication and its far no-op cleanup;
  query contracts, other producers and state admission remain Q-EXE-007.
  FND-EXE-394 reads both query bodies and their installation probe; native
  record/register contracts, other producers and state admission remain Q-EXE-007.
  FND-EXE-395 reads type-two's other published targets and shared native-request
  helper; native semantics, callers, writers and other slot types remain Q-EXE-007.
  FND-EXE-396 reads type-three publication, its preliminary helper and cleanup;
  native contracts, complete callers/writers and remaining targets remain Q-EXE-007.
  FND-EXE-397 reads its two remaining published callees and shipped request
  defaults; native effects, complete callers/writers and type four remain Q-EXE-007.
  FND-EXE-398 reads type four, its immediate helpers and published targets;
  native contracts, actual callers/writers and storage admission remain Q-EXE-007.
  FND-EXE-399 reads the installed type-one published targets and local-copy
  callees; native mapping, callers/writers and buffer admission remain Q-EXE-007.
  FND-EXE-401 bounds a resident literal search for the shared segment word;
  cross-region/computed writers and segment provenance remain Q-EXE-007.
  FND-EXE-402 extends that literal search into resident native helpers;
  interrupt effects and computed transfers still leave writer coverage open.
  FND-EXE-403 reads the registration caller's local input producers and
  wrapped count; DS provenance and intervening call effects remain Q-EXE-007.
  FND-EXE-404 supplies the first caller helper's byte-input and local result
  dependency; native preservation and subsequent pointer callees remain open.
  FND-EXE-405 binds both pointer callees to existing copy/append readings;
  source/extent/alias admission and manager consumption remain Q-EXE-007.
  FND-EXE-406 reads the manager's record bindings, local gate/slot writes
  and mutable callback continuation; remaining helper/storage contracts stay open.
  FND-EXE-407 supplies both direct manager helper bodies and packed-table
  writers; link termination, segment/input admission and native effects stay open.
  FND-EXE-408 bounds additional link-field candidates and reads one retrying
  writer; its helper, other writers and storage/preservation contracts stay open.
  FND-EXE-409 supplies that helper's outputs, selection and untested final
  tag stores; target effects, complete writers and storage admission stay open.
  FND-EXE-411 reads the two linked-record release writers and ordered
  failure prefixes; cycle freedom, other writers and storage admission stay open.
  FND-EXE-412 reads the remaining literal extension writer and ordered
  accounting/reconnection failures; callers, computed writers and admission stay open.
  FND-EXE-413 reads one quantity-adjustment caller and its signed shrink
  helper; selector production, broader callers and native/storage admission stay open.
  FND-EXE-414 reads the six-record selector and head-loading wrapper;
  identity/state producers, deeper callees and native/storage admission stay open.
  FND-EXE-415 reads the record-zero lookup and distinct second-slot
  callback layout; encoded-word producers and native/storage admission stay open.
  FND-EXE-416 reads linked-node allocation, count publication and repeated
  template requests; state producers and native/storage admission stay open.
  FND-EXE-417 reads dirty flushing, tail inspection and predecessor release;
  stable structure, output freshness and native/storage admission stay open.
  FND-EXE-418 reads record creation, aligned load retry and ordered
  state publication; callers, output freshness and native/storage admission stay open.
  FND-EXE-490 finds no AH=4B request among the sound utility's 69 interrupt
  21 instructions or its stack-thunk callers, and no interrupt 2E. Its load
  image makes no direct launch request; the game editions remain Q-EXE-007.
  FND-EXE-496 and FND-EXE-497 find that its installable drivers request no
  program execution and transfer out only to callbacks it never registers
  or to resident programs outside the build, and FND-EXE-498 and
  FND-EXE-501 find that SBAWE32.ADV, in its use, reaches only code entries.
  FND-EXE-380 identifies shipped limit and first-five-byte initializers;
  startup, actual DS, aliases and later writers must admit their runtime use.
- How does the shipped wrapper resolve the two bare helper commands and
  continue after them (Q-EXE-009)? Resolution to installed batch files is
  consistent with the declared mounts, but command-search order, batch
  chaining and EXIT handling may affect that result and the textual return
  path (FND-EXE-010). Reading the shipped interpreter's resolution and
  continuation code under the declared launch inputs settles the static
  behavior; mutable overlay substitutions remain conditional. The bundled
  source predicts replacement of the wrapper by a bare batch command, not
  return to it, because only CALL retains the active batch
  (SRC-DOSBOX-GOG-0742). The competing return reading remains unsupported;
  FND-EXE-013 and FND-EXE-014 now provide compiled dispatch and a conditional
  batch-cleanup distinction, including the CALL flag interval. Command/mount
  inputs, other flag writers and exceptional/EXIT paths must still decide
  the complete continuation. FND-EXE-015 now records supplied-name, COM,
  EXE and BAT ordering before admitted PATH candidates, conditional on
  normal helpers and a valid input. Its availability predicate crosses an
  unread normalization/drive-object boundary, so installed-BAT resolution
  remains conditional. FND-EXE-016 separately identifies the count-80 PATH
  scan that can advance past NUL; supported-input reachability and subsequent
  effects remain unknown. FND-EXE-017 records initial selector production
  and guarded table access, without settling later normalization, initial
  selector writers or drive-object behavior. FND-EXE-018 reads two direct
  selector writers and their differing success/update predicates; caller
  admission, indirect writers and later virtual effects remain unknown.
  FND-EXE-019 identifies the exact drive-command suffix gates and CRT import
  slots; unchanged bare helper tokens take the local filename-lookup branch,
  conditional on their delivery. FND-EXE-020 records the filename helper's
  byte scan and prefix setup, separating consumed and emitted bounds.
  FND-EXE-021 now reads component transformations, return/failure branches
  and retained output mutations. Prefix provenance, caller contracts and
  complete resolution remain open. FND-EXE-022 records two pointer-installation
  paths and one direct prefix transfer; constructors, concrete virtual targets
  and storage/alias bounds remain unread. FND-EXE-211 classifies its two known
  indexed writers within bounded bodies, retaining unsearched code and callees.
  FND-EXE-023 records a direct record
  append and its conditional growth path; allocation contracts, object
  construction and caller invariants still need evidence. FND-EXE-024
  identifies allocation/release imports and local retry/return boundaries;
  callback state, exceptional effects and caller inputs remain conditional.
  FND-EXE-025 reads the separate failure-object prefix and bitmap fallback,
  without establishing its lifetime or final exceptional outcome.
  FND-EXE-026 links a bounded caller loop to append and initialization;
  earlier selector admission, list/object producers and aliases remain unread.
  FND-EXE-027 traces one earlier list producer and construction-call inputs;
  construction effects, status meanings and dispatch targets remain open.
  FND-EXE-028 resolves the bounded dispatch targets and shared output reread;
  helper effects, aliases and final continuations remain conditional.
  FND-EXE-029 reads the first shared helper as a sentinel-linked search;
  comparison semantics, collection producers and returned-word lifetime remain open.
  FND-EXE-030 reads stored-length and unsigned-byte comparison operations;
  valid storage, producer contracts and downstream effects remain conditional.
  FND-EXE-031 traces one collection producer and direct link-write order;
  field helpers, ownership, initialization and cleanup remain unread.
  FND-EXE-032 reads signed field-publication branches; preceding-word
  producers, auxiliary effects and ownership remain unresolved.
  FND-EXE-033 reads the add-one auxiliary and a distinct previous-value
  exchange-add return; ownership and caller cleanup contracts remain open.
  FND-EXE-034 traces negative-path payload copying and length publication;
  FND-EXE-035 traces capacity rounding and three-word prefix initialization;
  FND-EXE-036 traces the bounded capacity-limit construction/publication path;
  FND-EXE-037 traces its object first-word and payload-field publication order;
  FND-EXE-038 traces temporary input/end production and range-copy branches;
  FND-EXE-039 traces the bounded null-input construction and failure route;
  FND-EXE-040 traces their conditional raw-prefix release boundary;
  FND-EXE-041 traces a mutable final target and its normal-return import boundary;
  FND-EXE-170 traces the bounded encoded shared-record reader;
  FND-EXE-043 traces record selection, initialization and shared-pointer publication;
  FND-EXE-169 bounds the shipped copied tail and local name terminators;
  FND-EXE-167 traces lazy initialization and mode-dependent record publication;
  FND-EXE-200 traces mode admission, flag publication and the bounded wait loop;
  FND-EXE-201 retains negative-mode direct-clear aliases and their sign-byte admission;
  FND-EXE-202 retains gate-neighbor indexed-store and callee-preservation limits;
  FND-EXE-203 reads its direct allocation wrapper and retains imported preservation;
  FND-EXE-204 separates neighboring cursor reads from fixed publications;
  FND-EXE-205 separates scalar initialization from indirect and imported writes;
  FND-EXE-206 validates the shared numeric gate-neighbor search and its exclusions;
  FND-EXE-207 retains the bounded-width physical overlap-start search and exclusions;
  FND-EXE-047 traces imported resource and local-helper mode publication;
  FND-EXE-048 resolves record/wait imports and the zero-result tail return;
  FND-EXE-049 traces saved-link cleanup and its freshly selected mode;
  FND-EXE-050 bounds setup/cleanup return handling in construction callers;
  FND-EXE-051 traces recovered stored-handler prefixes and forwarding branches;
  FND-EXE-052 traces selected-record publication and saved-state indirect transfer;
  FND-EXE-053 traces register-input selection and mutable-local link traversal;
  FND-EXE-054 traces the second selector and its two distinct callback result gates;
  FND-EXE-165 traces nested callback record writers and early-exit status reloads;
  FND-EXE-056 bounds callback selected-record access and full-width field stores;
  FND-EXE-057 traces signature-selected saved-state reads and seven-return preparation;
  FND-EXE-196 traces ordinary signed admission, helper iteration and six-return stores;
  FND-EXE-197 bounds one nested-record source prefix's conditional read extent;
  FND-EXE-198 narrows selected-local frame origin and ordinary nested lifetime;
  FND-EXE-199 narrows zero-mode alias candidates with full-width mode admission;
  SRC-WIN32-X86-ABI supplies an external host flat-mode and ordinary ABI
  contract, without admitting native segments or every local helper's effects;
  FND-EXE-059 bounds the matching byte reader and terminating full-word outputs;
  FND-EXE-060 traces metadata marker branches, cursor returns and relative targets;
  FND-EXE-061 bounds modifier mask classes, marker bypass and local zero callees;
  FND-EXE-062 traces guarded typed reads, base adjustment and cursor return;
  FND-EXE-063 bounds nibble-nine termination, sign fill and full-word output;
  FND-EXE-214 traces matching strides, low-byte virtual results and index scans;
  FND-EXE-215 bounds matching classification, fallback flag preservation and cursor hops;
  FND-EXE-066 traces the second terminal wrapper and initial shared-target tail dispatch;
  FND-EXE-067 traces classification-one counter/link effects, saved return and caller continuation;
  FND-EXE-068 traces guarded context acquisition, converted TLS returns and post-publication zero stores;
  FND-EXE-069 traces initialization index stores, converted guard writes and ignored callback values;
  FND-EXE-070 traces stored-handler state branches, signed cleanup counters and its context getter;
  FND-EXE-071 bounds head-associated target guards, outgoing slots and passed-through returns;
  FND-EXE-072 connects a published head target to unsigned mode and adjusted-payload tail dispatch;
  FND-EXE-073 traces payload-range bitmap clearing, separate guard reads and prefix-adjusted free;
  FND-EXE-074 resolves the pool-associated wait/signal imports and their return predicates;
  FND-EXE-075 traces the pool counter writer, unchecked creation result and once completion;
  FND-EXE-076 locates three physical guard-address candidates outside the decoded reference list;
  FND-EXE-077 classifies those candidates as reads and records their conditional frame/state prefixes;
  FND-EXE-078 bounds virtual-only guard storage and excludes overlapping declared relocation sites;
  FND-EXE-171 grounds startup callees and their empty memory-update route;
  FND-EXE-080 traces pre-dispatch guard publication, reverse callbacks and registration return;
  FND-EXE-081 reads the selected prefix's paired increments and distinct context initializer;
  FND-EXE-082 traces mutable cleanup-cursor reloads, next-word reads and publication;
  FND-EXE-083 traces the first cleanup target and separately admitted paired-pointer constructor;
  FND-EXE-084 traces shared old-value cleanup gates and ordered indirect-resource calls;
  FND-EXE-085 traces reverse-slot release admission and partial-word initialization;
  FND-EXE-086 records distinct status-store and handler gates; FND-EXE-087
  records signature-selected head effects and fresh-mode saved-state transfer;
  FND-EXE-088 records overlap construction and distinct stored handlers.
  FND-EXE-089 records selected-callback release admission and first-word cleanup;
  FND-EXE-090 records its downstream stored-target and low-byte fallback gates.
  FND-EXE-091 records this caller's pre-reader modifier, explicit displacement
  and physically selected zero-return fallback method.
  FND-EXE-092 composes first-field helper admission, destination stores and
  later cursor/marker stages without establishing stream bounds.
  FND-EXE-093 records PATH source advancement, key admission and untested output
  helper completion; FND-EXE-094 records its word-boundary and bounded-copy
  callees without establishing the record-list extent.
  FND-EXE-095 records final output replacement/alias choices and pointer returns;
  FND-EXE-096 records preparation spans, release admission and publication,
  retaining new-prefix and stored-handler admission limits. FND-EXE-097
  composes FND-EXE-035's prefix producer with that caller and recovers the
  saved handler, retaining native frame and shared-tail input uncertainty.
  FND-EXE-098 records consecutive/list reset producers and a mutable fixed
  fallback first word; index admission remains open. FND-EXE-099 physically
  resolves selected byte/word slots and records fresh-table word composition
  plus a byte transfer route. FND-EXE-100 records mapping publishers, object
  selection, source reentry and post-read reset/restoration. FND-EXE-101
  records category/flag state admission and copied-word publication. FND-EXE-102
  records helper depth/callback publication and distinct conditional/normal
  restoration. FND-EXE-103 grounds the conditional wait-target/callback link
  and distinct signed/full-width retry and dispatch gates. FND-EXE-104
  records admission sum exits, callback-list order and budget publication.
  FND-EXE-105 records callback insertion and repeated-match removers;
  FND-EXE-106 records pool links and retained-successor callback traversal;
  FND-EXE-107 records fixed-target insertion/removal wrappers and argument construction;
  FND-EXE-108 records the fixed callback slot selection and shifted-word forwarding;
  FND-EXE-109 records downstream word dispatch and value-two call/clearing order;
  FND-EXE-110 records value-seven priority publication and gated state transfers;
  FND-EXE-111 records value-one byte capture, progress and callback reinsertion;
  FND-EXE-112 records zero-branch old-index reads and post-call count rereads;
  FND-EXE-113 records the first callee byte-write/removal and scheduling prefix;
  FND-EXE-114 records its equality priority and optional state-call continuation;
  FND-EXE-115 records local-F-zero second-record writes and distinct returns;
  FND-EXE-116 records nonzero-F counter/write ordering and fresh current-byte reads;
  FND-EXE-117 records exact-equality byte merging and fresh write-position arithmetic;
  FND-EXE-118 records shared counters and latch-setting tail insertion;
  FND-EXE-119 records gate-absent priority/state calls and local-F suffix admission;
  FND-EXE-120 records post-record nonzero-byte priority and local-F publication;
  FND-EXE-121 records shared bounded slot-byte/mask publishers and return widths;
  FND-EXE-122 records optional first-mode slot admission and pre-call clearing;
  FND-EXE-123 records second-mode position bounds, group gate and load ordering;
  FND-EXE-124 records selected-callee prefix gates, reader calls and recursive fallback;
  FND-EXE-125 records full-width mapped reads and independent boundary-byte assembly;
  FND-EXE-126 records physical full-width targets and the shared four-call return widths;
  FND-EXE-127 records larger full-width zero-mode publication/reentry and full returns;
  FND-EXE-128 records full-width two-level lookup and missing-entry reload contracts;
  FND-EXE-129 records full-width mode/entry-bit selector gates and their precedence;
  FND-EXE-130 records full-width retained-entry publication, reentry and last-entry cleanup;
  FND-EXE-131 records selected-prefix selector admission, physical targets and shared default;
  FND-EXE-132 records prefix width masks, retain-mask branch and selector clearing;
  FND-EXE-133 records all physical byte-indexed mask contributions;
  FND-EXE-134 records distinct word-field and byte-count/full-width mask branches;
  FND-EXE-135 records byte/full-width exact-input gates and retained/fresh lookup ordering;
  FND-EXE-136 records complementary count gates and fresh-word versus full-width comparisons;
  FND-EXE-137 records signed word/full-width shifts and their fresh-count mask gate;
  FND-EXE-138 records the signed byte shift with retained count and lookup inputs;
  FND-EXE-139 records full-width logical shift and fresh-count one/sign mask gates;
  FND-EXE-140 records zero-extended word shifts and fresh-word count/retained-sign gates;
  FND-EXE-141 records zero-extended byte shifts with retained count/lookup and saved-byte sign;
  FND-EXE-142 records complementary full-width shifts and retained sign/fresh tail gates;
  FND-EXE-143 records the complementary word-count boundary and continued sign gates;
  FND-EXE-144 records the complementary byte-count boundary and saved/retained inputs;
  FND-EXE-145 records the full-width result-nibble equality and exact-result gates with retained mask one;
  FND-EXE-146 records word/full-width retained-result widths, opposite nibble gates and shared-tail input producers;
  FND-EXE-147 records retained-byte result comparisons and lookup inputs with opposite nibble gates;
  FND-EXE-148 records guarded full-width admission, fresh byte masks and retained sign inputs;
  FND-EXE-149 records shared word comparison inputs, later result-word reads and saved-byte lookup admission;
  FND-EXE-150 records guarded word admission, full guard width and retained-word/saved-byte inputs;
  FND-EXE-151 records saved original bytes before working increment and register reuse in guarded byte admission;
  FND-EXE-152 records shared byte comparison and saved D/V inputs without guard admission;
  FND-EXE-153 records shared full comparison with fresh byte inputs and a later full result read;
  FND-EXE-154 records retained-word zero-nibble and exact-minimum tests with a fresh-byte lookup;
  FND-EXE-155 records guarded byte equality, saved original inputs and a flipped high-mask XOR;
  FND-EXE-156 records guarded word equality with retained words, fresh byte inputs and later word D;
  FND-EXE-157 records guarded full equality with fresh d/n/v bytes and later full D;
  FND-EXE-158 records strict byte comparison, saved inputs and bounded partial-register mask work;
  FND-EXE-159 records strict word comparison with retained words, fresh bytes and later word D;
  FND-EXE-160 records strict full comparison with retained inputs, fresh n/d/v bytes and later full D;
  FND-EXE-161 records a gated save/restore writer route with AL-only admission and a byte-derived return;
  FND-EXE-162 records its stored callback with signed return gates and a conditional last-pair comparison;
  FND-EXE-163 records the diagnostic first transfer with mutable candidate traversal and distinct saved-state admission;
  FND-EXE-164 composes its candidate reset with register-input selection and full-seven state-transfer admission;
  table/slot producers, runtime writers/other prefix effects and remaining selected callee effects remain open.
  floating numeric contracts, downstream targets, producers,
  indirect targets and actual state admission remain open.
  Native frame admission, callback invocation, selector effects, allocation units, capacity, aliases
  and lifetime remain conditional.

  FND-EXE-166 separately narrows a selected-record reader's direct-call
  and raw address-word domains and records its post-setup argument reload.
  FND-EXE-208 adds the bounded physical near-transfer/interior-flow comparison;
  excluded transfer representations still prevent a complete caller claim.
  FND-EXE-209 supplies distinct metadata writers for two ordinary callback
  record origins; FND-EXE-210 bounds their conditional prefix read extents.
  Selection, preservation and downstream target/stream admission remain open.
  FND-EXE-212 bounds concrete count writers and conditional initial matching pairs;
  it does not admit all record origins or later matching and cleanup behavior.
  FND-EXE-213 follows one count-one nonmatching-signature path through negative
  decoding to conditional saved six; preservation and other routes remain open.
  FND-EXE-216 separates the pair decoders' direct output and stack stores from
  the saved loop counter and cursor under equal segment bases. Pre-loop input,
  source/stack alias admission and normally terminating decodes remain open.
  The selected-record reader's closure dependencies are separated from the
  full wrapper outcome: Q-EXE-011 requires actual segment-state evidence to
  distinguish equal-offset stack and pointer accesses; Q-EXE-012 requires
  caller/interior-target producers beyond the controlled direct domains;
  Q-EXE-013 requires the last writers and lifetime of the actual pointer
  chain and offset-28 word. FND-EXE-166/198 and FND-EXE-208 favor the
  ordinary reader route, while FND-EXE-167/168/200/201 retain intervening
  alias and producer alternatives. The settling evidence for each is in its
  queue item; none is discharged by citation coverage or a finite prefix.
  FND-EXE-217 adds the controlled decoded DS/SS-output domain and opaque-site
  classifications for Q-EXE-011. Its zero modeled matches do not admit initial
  descriptor bases or preservation through missing/opaque and external effects.
  FND-EXE-218 classifies the saved listing's empty effects as decoded NOPs,
  with a separate WAIT control that prevents equating empty p-code with a NOP.
  Actual initial descriptor and external-preservation evidence remains required
  by Q-EXE-011; the classification does not establish storage identity.
  FND-EXE-219 identifies an undecoded short-jump candidate into the reader
  and controlled empty direct-incoming domains for its preceding gap.
  Its admission remains Q-EXE-012; neither padding nor a second caller is established.
  FND-EXE-220 excludes exact shipped four-byte absolute address words for
  every byte address in that gap and reader; other target representations
  and their producers remain Q-EXE-012.
  Preserved-selector-input and alias-modified-input readings remain open:
  the former needs setup, selected-local lifetime and field-writer admission;
  the latter needs a concrete intervening writer (Q-EXE-009). Neither a
  single decoded call site nor the small leaf body establishes the format.
  FND-EXE-167 separates setup's caller-owned record store from the incoming
  slot numerically, conditional on equal DS/SS bases. A disjoint shared base
  and an overlapping shared-base store remain competing readings until its
  producers and segment identities settle them (Q-EXE-009). FND-EXE-168
  separates fresh and decoded-existing publication origins; the reader's
  leading-value test alone cannot settle allocation ownership or stack
  separation. Decoded input, lifetime and indirect-writer admission remain
  Q-EXE-009. SRC-WIN32-ATOMS predicts that case-insensitive lookup can
  match the encoded registration name while preserving its first registered
  case pattern. With an unchanged initializer-produced name, FND-EXE-169's
  extent supplies a possible complete decode input; a nonzero short or
  changed-name result alone does not. Identifier/name admission, runtime
  writes and storage lifetime distinguish those readings (Q-EXE-009).

  FND-EXE-172 independently checks physical rel32 candidates into the
  fixed-bound startup helper, agreeing with FND-EXE-171's decoded domain.
  Unchecked transfer representations and runtime-written code still prevent
  a complete caller declaration (Q-EXE-009).
