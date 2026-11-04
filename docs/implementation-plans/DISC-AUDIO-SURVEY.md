# Disc-audio container Survey

Research tooling for Q-SOUND-008 and FMT-SOUND-005. Existing FND-SOUND-002
and FND-CONFIG-008 establish the track names, prefix signatures and configured
cue consumer, but do not validate page framing throughout the forty files.

Add a bounded read-only Ogg page inspector under tools/research. Follow the
Xiph Ogg framing specification and Vorbis I identification/header ordering.
Validate every page capture/version, flags, bounds, non-reflected CRC,
single-stream identity, sequence, BOS/EOS and packet continuation/lacing.
Check the first packet's Vorbis version, positive channels/rate, block-size
ordering and framing bit; require header packet signatures in order 1/3/5
and audio packet type bits afterward. Keep setup, comments and audio payloads
opaque; do not decode samples or claim full codec conformance or playback.
Bound file size to 16 MiB and a packet to 1 MiB; refuse unsupported chained
or multiplexed streams rather than resynchronize over errors.

The CLI resolves only the approved licensed GAME_DIR and fingerprints each
Track02..41 against the committed build manifest before inspection. Emit only
counts, declared rate/channels/block sizes, final granule and file identities;
no payload, comment text or decoded samples. Retain output in GAME_DIR.
Mapping and configured consumer are checked by read-only cue/config inspection;
never launch or control the owner's DOSBox.

Synthetic controls cover valid packet/page spanning, exact-multiple-of-255
termination, independent bitwise CRC control, corrupt/truncated pages, wrong
serial/sequence/continuation, missing/early EOS, header ordering and invalid
identification fields. CI needs no licensed file. Run Test.ps1 before a
tooling-only commit. In a separate research batch run all forty inputs,
record the actual results, reconcile cue/config evidence, update the bounded
format description and close Q-SOUND-008 only if its whole question is answered.
Exit: no unclassified page or byte, all inspected inputs match the manifest,
and spec/queue/parity changes pass the research gate without a playback claim.
