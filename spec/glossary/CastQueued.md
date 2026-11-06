# CastQueued

An event: an admitted publication through overlay 179 `+12B1` adds a cast
queue record. It carries the four arguments in call order: two slots, a
`DATA` number and a byte. For a retained kind-zero record, the queue runner
passes its second, first and third arguments as the first three arguments
to overlay 193 `+003C`; this does not guarantee that every publication is
later consumed or that its dispatch kind remains zero
[FND-PARTY-328, FND-RNG-040].
