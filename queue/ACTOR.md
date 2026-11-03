# ACTOR

Next ID: Q-ACTOR-008

## Static

- Q-ACTOR-001. FMT-ACTOR-002, FMT-ACTOR-001: Which record supplies the first hostile Look
  panel's name, level and available actions? Settles it: the code that fills the Look panel from
  an actor record. Tried: the `RDFF` records that hold the captured label, the hostile `OJFF`
  record's words, and the known `RDFF` and `OJFF` lookup paths; object 9,258 takes no `RDFF`
  request and has no `RDFF` resource (FND-ACTOR-009). Blocks: slice 3.
- Q-ACTOR-002. FMT-ACTOR-001, FMT-ACTOR-004: What do `unk_00`, `unk_06`, `unk_08` and `unk_0B`
  do, what do the image numbers `31E0:0EFF` puts in place of `image` show, what do the unknown
  fields and the bits of `unk_00` of the object slot records hold, and how many slots are there?
  Settles it: a reading of the code that uses the 37-byte slot records at `DS:67BB`, fields
  `0x0`, `0x7`, `0xF` and `0x19`, and of the remaining overlay code of overlays 197 and 213 around the
  `OJFF` tag bytes. Tried: FND-PARTY-225 qualifies overlay 190's occurrence
  and answers its local object-number/image-word resolver; that local
  reader is removed from Settles it. Its resource/buffer and cache admission
  remain Q-PARTY-087. Blocks: nothing yet.
- Q-ACTOR-003. FMT-ACTOR-002: What are the remaining fields of the 23-byte
  kind-one and kind-five records beyond their documented fields?
  Settles it: native readers/writers of the remaining record bytes through
  `57E0:19C1` and `57E0:19D1`, including their FNFO bindings and copied,
  restored and released states; distinguish kind-specific meanings. The
  kind-five first-word key/free-marker binding, logical twenty-slot search
  and getter-base publication are FND-ACTOR-016 and are no longer unknown.
  Actual trigger getter admission is Q-SCRIPT-022. Tried: a search for resident instructions with the displacement 43 or 76
  (FND-ACTOR-010); FND-ACTOR-015 reads the chunk chain and the kinds, which leaves these.
  Tried: FND-ACTOR-016 settles the kind-five first-word source relationship
  and local selection/copy/clear bindings; those parts are removed above.
  The independent kind-four payload/reference question is Q-ACTOR-007.
  Blocks: nothing yet.
- Q-ACTOR-007. FMT-ACTOR-002: What is the relationship of the skipped
  fifteen-byte kind-four payloads to the shared type rows? Settles it: a
  controlled comparison of all 1216 known payloads with ALL/4 at record_ref,
  retaining out-of-range references, other-row matches and differing payloads;
  any other admitted reader of the inline bytes remains explicit. The native
  shared-reference/index binding is FND-ACTOR-017 and no longer unknown.
  Tried: split from Q-ACTOR-003 after FND-ACTOR-016 settles an independent
  kind-five field. FND-ACTOR-015 and FND-PARTY-051 already identify the
  skipped copy and retained record_ref; their meaning remains open.
  Tried: FND-ACTOR-017 binds the shared table and retained type index and
  verifies RDFF/600 against row 61 independently. This removes reference
  identity from Settles it; one case does not establish the full payload
  relationship. Complete shared-row fields are Q-ITEM-005.
  Blocks: nothing yet.
- Q-ACTOR-006. FMT-ACTOR-003: What do the `value` words of `MONR` name, what does `549B:00C0`
  hold and which code reads or writes it, and does script request 8 run in play? Settles it: a
  search for accesses to `549B:00C0` with segment word `0x04A0` in the resident image and the
  overlays, and a trace of what `var7[8]` holds when `GPL/27` reaches `0x01C8`. Tried: FND-ACTOR-014
  reads the request and the record layout, which leaves these. Blocks: nothing yet.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-ACTOR-005. RULE-ACTOR-001: Does the game mirror an object whose entity has
  `unk_flags_bit_7` set, and does it show frames other than the first for placed objects?
  Settles it: a capture of a view that holds an entity with bit 7 set, compared as in
  FND-IMAGE-010. Blocks: nothing yet.

## Source

None.

## Blocked

None.
