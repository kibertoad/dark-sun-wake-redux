# archive_pending_metadata

A `UINT8` at offset `0x2C` of a runtime archive record in
BLD-GOG-EN-1.1. Zero makes archive preparation skip its file requests;
nonzero requests serialization of the current header and directory followed
by the handle commit service. The preparation routine clears this byte only
after its own required checks succeed. Its failure return retains the byte
and does not roll back earlier writes [FND-COMBAT-096, FND-COMBAT-124].

Directory construction and modification mark this byte nonzero; one producer
does so even after a failed payload transfer [FND-COMBAT-121,
FND-COMBAT-122, FND-COMBAT-123]. It is a pending metadata indicator,
not proof of successful payload storage, a new gameplay snapshot or valid
directory contents. Setting it in a controlled experiment does not establish
that an ordinary Quit reaches that state [FND-SAVE-028, FND-SAVE-031].
