---
id: FND-PARTY-250
title: Overlay 182 treats word 0x10 as door flags and writes a sign-extended low byte to both paired records
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069450..0x00069815
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2839..28C9:28E3
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at commit dff6b2fb; Python 3.14.7, Capstone 5.0.7
environment: null
---

## Observation

This reads the two inventoried overlay-182 functions at file offsets
`0x69473` and `0x69636`, their preceding classifier and the type accessor
they call. FND-PARTY-248 supplies the 23-byte item record's type word
and FND-ACTOR-005 the 37-byte placed-slot records. The existing message
selection reading is FND-CONFIG-054. No original code was executed.

### Classification and action request

The helper `182 +0C00` returns one exactly when its signed word argument
lies from 2400 through 2949 inclusive; otherwise it returns zero. Both
functions pass the placed-slot record's word +0x1B to this classifier.

`28C9:2839` initially selects 9999 as its return. It may first request
RDFF for a kind-zero slot (FND-ACTOR-005). At its common tail it returns
the 23-byte record's type word +0x0A only when the slot metadata kind
is one. A caller's type result below 9999 thus comes from that kind-one
path, subject to the child request and table identity.

Function `182 +0C23` takes its placed-slot index in the second word
argument. After a type result below 9999 it saves that slot's object
word +0x1B. If slot word +0x16 is not 0x8000 it calls `31E0:1808`,
saves the slot metadata's record index and checks the classifier.
On the accepted range it reads item word +0x10's low byte. It passes
the opposite of bit 0 as the setter's second low-byte argument and zero
as the third argument, with the slot as first argument. It also saves
the original bit 0 for the later comparison. If its own third low-byte
argument is zero it clears DS:655C before that request.

The later message path is enabled only when this function's third
low-byte argument is zero. After `1695:02E7` it rechecks the classifier,
requires the current bit 0 to equal the saved original bit, and requires
DS:655C zero. Its priorities are:

1. Bit 3 set: request the secret-door discovery message, clear only
   that bit with the complete-word mask 0xFFF7, then return.
2. Otherwise bit 1 set: request the locked-door message.
3. Otherwise bit 0 set: request the unable-to-open message.
4. Otherwise return without one of these messages.

These local branches support closed-state use of bit 0, lock-state use
of bit 1 and undiscovered-secret use of bit 3 in this door context.
The clear at `+0DA1` follows the message request even though it does not
test whether a message was actually displayed. This does not assign
these meanings to word +0x10 on other kinds of object.

### Setter gates and partner lookup

Function `182 +0DE6` takes a placed-slot index, a low-byte request and
a low-byte override as its three word arguments. It returns before the
stores unless DS:0DAB is exactly one, the type accessor returns an
unsigned word below 9999, the slot object passes the classifier and,
after `31E0:1808`, slot word +0x16 is signed positive and not 255.

It computes a partner object number from the saved slot object number:
an even value becomes one less, an odd value one more. It searches
placed slots 5 through 319 in increasing order for the first slot with
that object word. If found, it calls `31E0:1808` on the partner and
saves its metadata record index. A miss leaves the source-only path.
It then loads the source slot's metadata record index and the low byte
of its item word +0x10. It does not read that word's incoming high byte.

With override zero, either bit 2 or bit 3 in this old low byte prevents
the subsequent stores and presentation requests. With nonzero override
those bits do not stop this local operation. This establishes their
blocking use here, without assigning a gameplay name to bit 2.

### Written value and later requests

The source's low byte is sign-extended to a 16-bit word. The request
then transforms that word as follows:

| Request low byte | Transformation |
|---|---|
| Bit 1 set | Set both bits 0 and 1. |
| Exactly zero | Clear both bits 0 and 1. |
| Other nonzero value | Set bit 0; retain bit 1. |

The source store is `+0EF3`. If a partner was found, the source word is
reloaded and copied whole to the saved partner index at `+0F19`; the
partner's old flags are not merged. The full-word sign extension means
the high byte is zero when old bit 7 is clear and 0xFF when it is set.
For example an old low byte 0x80 with request zero produces 0xFF80,
regardless of the incoming high byte. This is an arithmetic consequence
of the read width and the 16-bit masks, not a measured shipped case.

These stores occur before the later unchanged-state exits. In particular,
when request bit 0 and old bit 0 are both set, or when both are clear,
the local function returns after the stores. That does not undo them.
The remaining paths select presentation column 3 if request bit 0 or
bit 1 is set, otherwise column 2; call `182 +0FC5` for the source;
request `31E0:271E` with the selected word of the slot's 72-byte
presentation entry; and, if present, request the partner presentation
and call `+0FC5` for the partner. The called routines' effects are not
established by these argument bindings.

## Interpretation

Word +0x10 is context dependent. These accepted object-number paths
use it as door flags, whereas FND-PARTY-248 copies the same word into
item quantity or payload, and FND-PARTY-249 compares it with 313.
The three overlay-182 stores named by Q-PARTY-020 now have local
values, widths and ordering. Their complete admitted user-action,
script, storage and presentation contract remains Q-PARTY-107.

## Alternatives

- Preserving the old high byte is ruled out for `+0EF3`: it reads a
  byte, sign-extends it and stores a word. The partner receives this
  resulting word, not a bitwise combination with its own flags.
- Returning on an unchanged bit-0 condition does not mean no write:
  both record stores precede those comparisons.
- Bit 2's gameplay name and bit 7's door-specific meaning remain
  unresolved; the observed masks and sign extension do not name them.
- The source handler's 0x8000 presentation-word bypass skips local
  record-index and old-bit initialization. Whether admitted calls can
  subsequently enter the message path with those locals uninitialized
  is unresolved. No successful door interaction is claimed for it.
- Slot/type/record preservation across the resource, SCMD, message and
  action calls, all partners' kind/index admission, DS:655C writers,
  setter caller arguments and presentation effects remain unproved.

## How to reproduce

With the locked evidence Python and installed `<dsun>`, run
`overlay_listing.py <dsun> 182 69450 69815` and
`resident_listing.py <dsun> 28C9:2839..28C9:28E3` from
`tools/research/exec-census/`. The tools check the installed executable's
length and XXH3. Compare the complete inventory ranges for 69450,
69473 and 69636. Trace each guard, the three request cases, partner
found/missing and the store-before-return order separately. Interpret
opcode 98 in these 16-bit functions as sign extension of AL to AX;
Capstone 5.0.7 labels it with a 32-bit mnemonic (known toolkit issue 538).
For message text identities reuse FND-CONFIG-054; no original text or
instruction listing is required in Git.
