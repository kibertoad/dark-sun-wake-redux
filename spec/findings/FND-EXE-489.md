---
id: FND-EXE-489
title: Sound utility readiness tests returned BX bits while coordinate samples consume returned CX and DX
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1A7C:00EA..1A7C:0164
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:20DF..1000:21A9
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-487 and FND-EXE-488's readiness interface 1A7C:00EA
reserves 0020 local bytes, saves SI and DI and writes word
SS:BP-10 as three. It passes output SS:BP-20, input SS:BP-10
and selector 0033 to 1000:20DF, then removes ten argument bytes.
The remaining input-record words and output-record bytes have no local
initializer before this call.

FND-EXE-353's shared wrapper captures a separate initialized segment
record and forwards selector, input pair, output pair and segment record
to 2110. Selector 0033 takes its ordinary local thunk path, not the
0025/0026 special branch. Input word zero supplies AX three; the
other register inputs BX, CX, DX, SI and DI are read from the
otherwise uninitialized input record. The captured segment record supplies
DS and ES separately. No native interrupt is executed by this research.

On ordinary continuation the wrapper writes returned AX, BX, CX and
DX to output words zero, two, four and six, returned SI and DI
to words eight and ten, masked carry to word twelve and full flags
to word fourteen. It updates the separate segment record with returned
ES and DS. FND-EXE-353 retains the carry-set error-helper and native
preservation obligations. These stores do not admit the native results or
retroactively initialize the incoming register record.

The readiness caller reads word SS:BP-1E, output word two, twice.
Each read supplies a word argument to local far-returning 0149; the
other incoming word is zero for the first call and one for the second.
Each call uses push-CS/near-call and the caller removes four incoming
bytes. It retains the first returned AX in SI and second in DI.
The 0149 helper sets DX one, loads the second argument's low byte
into CL, shifts DX left by CL and tests the first incoming word against
that mask. It returns AX one for nonzero intersection, zero otherwise,
restores BP and returns far without incoming cleanup. It makes no call
or interrupt and does not locally change SI, DI, DS or ES.
Only shift counts zero and one are admitted by this selected caller;
other callers and processor-dependent shift-count behavior remain open.

The readiness caller first tests DI against SI, then tests each held
word for zero. It returns one unless both are zero. It restores DI,
SI, SP and BP and returns far without incoming cleanup. Under stable
nonaliased output state the result is one exactly when returned BX has
bit zero or bit one set. The two tests are separate reads, not a held
BX snapshot. Neither the boolean result nor the bit names establish
native device semantics, readiness lifetime or successful input delivery.

FND-EXE-488's coordinate helpers use the same selector and initialized
input word but separate native calls. Their BP-1C and BP-1A reads
map to output words four and six, returned CX and DX respectively.
Each adds eight at word width before its consumer's signed division.
The samples are distinct from this readiness call and from each other.
The helpers do not test the output carry word or native return AX.

## Interpretation

This binds three local consumers to explicit wrapper-produced register
fields and resolves the readiness boolean's two-bit test. It does not
establish admitted native register inputs or device behavior. Q-EXE-007
retains actual DS and native segment/input provenance, unwritten fields,
interrupt and error-helper preservation, aliases and state lifetime, count
and table producers, other callers and 08E5. No complete readiness
reading, successful native input or launch exclusion is claimed.

## Alternatives

Treating readiness as returned AX ignores the caller's output-word-two
reads. Treating coordinate values as untouched locals ignores the wrapper's
CX/DX stores. Treating the segment record as initializing input registers
confuses two separate records. Treating bit presence as admitted device
behavior assumes native effects not observed here.

## How to reproduce

At revision fa2fd91 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Use Capstone 5.0.7 in sixteen-bit mode, MZ header size 1400 and
modeled load segment 1000. Decode shipped BCAA..BD24 at IP 00EA,
CS 1A7C, and 34DF..35A9 at IP 20DF, CS 1000. Bind
selector 0033, input SS:caller BP-10 and output SS:caller BP-20;
follow FND-EXE-353's separately captured segment record. Track output
word offsets two/four/six, incoming bit indices zero/one, local mask tests,
the readiness combination and register/frame preservation. Licensed bytes
stay outside Git; no original process, DOSBox, generated thunk or emulated call runs.
