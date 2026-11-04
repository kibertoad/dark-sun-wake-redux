# Item property domain census

Research tooling for Q-ITEM-005 and Q-PARTY-108. The player-visible question
is when item word +10 hex can independently change the Look pointer's
classifier gate. No gameplay implementation or native input is added.

Evidence: FND-PARTY-252 gives the helper/caller predicates and replacement
order; FND-PARTY-253 gives Look's nonzero AL gate; FND-INPUT-013 gives the
pointer modes; FND-ACTOR-017/018 and FND-COMBAT-171 identify the complete
102-row canonical ALL/4 resource and one source-equal native boundary.

A fixed fingerprint-checked query reads only canonical ALL/4's 1530 bytes
at OBJEX.GFF 1F291..1F88B after source verification. Enumerate rows lacking
all type-level sufficient nonzero predicates: byte-zero mask 13 hex,
byte-eleven mask 89 hex, and byte-four bit 20 hex clear. Report row indices,
not payloads. Preserve that these are candidates: name/value/quantity,
fallback and secret-door gates and actual item admission remain independent.

Synthetic acceptance covers exact extent, each sufficient bit independently,
unrelated bits, joint masks and a candidate row. Run the affected tooling
gate. Exit: committed bounded query and controls, followed by a separate
research finding giving the static pointer-consumer reading and actual
source-domain result. A negative domain result is valid only after the
synthetic candidate and each bit control pass. No complete-reading or
confidence promotion follows from this census.
