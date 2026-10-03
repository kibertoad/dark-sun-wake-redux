# Shared runtime migration

Bounded settings reads, validated backup writes and per-field recovery provenance.

The migration uses published `1.3.0` NuGet packages for the required shared APIs:
portable paths, recoverable persistence, and PCM/audio with streaming WAVE.
Only packages used by this restoration are referenced. Input support is independent
and is not required by this migration.

Game save payloads, version admission, slot naming, defaults, audio routing, fades, voice limits
and control policies remain local. Synthetic checks establish migration behavior, not parity
with the original game or live device behavior. No original assets entered this change.

Self-contained installers use separate committed runtime/mode lock profiles. Ordinary build
locks remain unchanged. Profile maintenance names the target runtime explicitly, and CI
rejects a missing or mismatched packaging lock instead of regenerating it.
