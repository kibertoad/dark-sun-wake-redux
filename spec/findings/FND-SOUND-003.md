---
id: FND-SOUND-003
title: SOUND_DS.EXE holds no VOC signature, no VOC file extension and no BIOS wait call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1 (ReportBytePattern); a PowerShell 5.1.26100.9444 scan of the whole file; repeated with Python 3.14.7 byte searches of the whole file
environment: null
---

## Observation

`SOUND_DS.EXE` is 204,593 bytes. Its MZ header is `0x1400` bytes and its load image ends at file
offset `0x1E5E0`, so 80,209 bytes after the image are not loaded by DOS. The finding is located at the start of
the load image, `1000:0000`, because it is a search of the whole file.

Searches of the whole file, loaded image and trailing bytes alike, find:

- no occurrence of the 19 bytes `Creative Voice File`;
- no `.VOC`, in any mix of upper and lower case (the three letters `voc` occur 15 times, none
  after a dot);
- no `CD 15`, the encoding of `INT 15h`.

Earlier searches in Ghidra of the loaded image alone gave the same three results.

## Interpretation

The sound setup program neither checks a Creative Voice File header nor names a `.VOC` file, and
it has no direct call to the BIOS wait service. Nothing here ties it to playing the game's voice
files; the game plays them itself (FND-SOUND-007, FND-SOUND-008).

## Alternatives

A search for literal bytes misses a header checked byte by byte, a name built at run time and an
interrupt number held in a variable. The setup program may still play a sample from memory to
test the card.

## How to reproduce

Search `SOUND_DS.EXE` for the bytes `43 72 65 61 74 69 76 65 20 56 6F 69 63 65 20 46 69 6C 65`,
for `.voc` ignoring case, and for `CD 15`.
