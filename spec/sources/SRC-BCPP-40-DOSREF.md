---
id: SRC-BCPP-40-DOSREF
title: Borland C++ Version 4.0 DOS Reference
superseded_by: []
author: Borland International
date: "1993"
location: https://www.bitsavers.org/pdf/borland/borland_C++/Borland_C++_Version_4.0_DOS_Reference_Oct93.pdf
xxh3: c260532ba6e06f19d9fb15fefd1aef97
licence: All rights reserved by Borland International; only short quotations are used.
---

## Use

The DOS reference of the compiler family whose run time `DSUN.EXE` links (the file holds
`Borland C++ - Copyright 1991 Borland Intl.`). The scan is 6,075,111 bytes. Its section
"Overlays (VROOMM) for DOS" (pages 20 to 26) and its entry for the global variable `_ovrbuffer`
(page 130) describe the overlay manager from the programmer's side: the overlay buffer lies
between the stack and the far heap, `_ovrbuffer` sets its size in paragraphs, and "The default
overlay buffer size is twice the size of the largest overlay." That agrees with the buffer
FND-EXE-568 finds the manager allocating when the word at `57E0:3570` is 0, and is a lead that the
word is `_ovrbuffer`.

The manual does not describe the executable's FBOV pack or its segment table, so it does not
answer Q-EXE-024.

## Known errors

- It documents version 4.0 (1993); `DSUN.EXE` names a 1991 run time, and whether its overlay
  manager behaves as this version describes has not been checked beyond FND-EXE-568.
