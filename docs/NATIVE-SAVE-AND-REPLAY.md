# Native save and replay format

Define a versioned, bounded, recreation-native format before save files become public.
Record magic, format version, maximum decoded size, state schema, migration policy,
atomic-write behavior, backup/recovery, unknown-field policy, and validation errors.

Replays should store an initial snapshot, deterministic random state and consumption
count, accepted/rejected commands, validation results, and a canonical state hash after
each step or phase boundary. Loading must replay and reject divergence. Keep support for
older versions explicit and tested; never deserialize arbitrary runtime types.

The implemented start-flow foundation uses snapshot schema `3`, an explicit
seed, a sequence number that advances for accepted and rejected commands,
immutable snapshot copies, and a canonical binary state encoding hashed with
XXH3-128. Schema 3 includes each member's psionic disciplines and optional
clerical sphere, the active occupied-slot edit target, and the bounded list of
characters dropped from the party for later ADD. The in-memory replay format
is version `3`; it stores each
semantic command and expected resulting hash, and rejects the first mismatch.
This is not yet the public native file
format: maximum file size, atomic I/O, migrations, RNG consumption, and
gameplay-wide state remain required before persistence is exposed.
