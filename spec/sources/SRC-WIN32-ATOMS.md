---
id: SRC-WIN32-ATOMS
title: Microsoft Win32 local atom API contracts
superseded_by: []
author: Microsoft
date: "2024-11-20"
location: https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-addatoma
xxh3: null
licence: null
---

## Use

Read on 2026-10-08 for Q-EXE-009; also used for Q-EXE-001. These are published external API
contracts for the names independently identified in FND-EXE-043 and
FND-EXE-170, not observations of an executed system library.

- [AddAtomA](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-addatoma),
  updated 2024-11-20: accepts a terminated string of at most 255 bytes.
  Names differing only in case identify the same local atom. Registration
  preserves the case of the first name; adding an existing string returns
  its existing identifier and increments its reference count. Failure is zero.
- [FindAtomA](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-findatoma),
  updated 2024-02-22: searches the local table without case sensitivity.
  Success returns the matching atom identifier; failure returns zero.
- [GetAtomNameA](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getatomnamea),
  updated 2024-11-20: receives an atom, output buffer and character capacity.
  Its success result is the copied character count excluding the terminator;
  failure returns zero. Integer atoms produce a decimal name beginning with
  a number sign. The contract does not say that every byte of the supplied
  capacity is initialized after a shorter result.

Together these contracts predict that the initializer's all-65 lookup
prefix can match its 65/97 encoded registration prefix, with the same
unchanged tail, while retrieval retains the first registered case pattern.
This is an inference from the external contracts and FND-EXE-043's producer,
not a native observation or proof of the stored record's lifetime.

The initialized extent needed by FND-EXE-170's fixed thirty-two-byte decode
must come from the actual returned name and its admitted identifier. A
nonzero result alone does not prove that extent. A known unchanged
initializer-produced name has the extent in FND-EXE-169, but existing table
contents, intervening mutations, identifier admission and storage ownership
remain Q-EXE-009. Naming the API result does not settle those dependencies.

## Known errors

None identified in these pages. They describe the published Win32 contract,
not the exact implementation or state of a system library used during an
original run. Source-only API predictions do not raise original evidence
statuses or supply a complete_reading declaration. No original is run.
