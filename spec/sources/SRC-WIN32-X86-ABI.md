---
id: SRC-WIN32-X86-ABI
title: Microsoft Win32 x86 flat-mode and calling-convention contract
superseded_by: []
author: Microsoft
date: "2024-12-13"
location: https://learn.microsoft.com/en-us/windows-hardware/drivers/debugger/x86-architecture
xxh3: null
licence: null
---

## Use

Read on 2026-10-08 for Q-EXE-009's segment and callee-preservation
obligations in FND-EXE-167 and FND-EXE-198. Microsoft describes Win32
execution as 32-bit flat mode. Its sample register display has matching
DS, ES and SS selectors, but that sample is not the shipped interpreter's
state and does not prove its descriptors or intervening segment writes.

The page's ordinary calling-convention contract preserves registers except
EAX, ECX and EDX; ESP adjustment depends on the convention. It places
up-to-32-bit results in EAX. It distinguishes caller stack cleanup for
__cdecl from callee cleanup for __stdcall and its described __fastcall.
This is an external contract, not proof that every local optimized helper
or indirect target follows one of those conventions. Direct argument reads,
stack effects and register writers still decide a particular local contract.

Flat-mode and ordinary ABI predictions therefore support a conditional
interpretation, not native segment identity, pointer validity, storage
separation or survival across an unread callee. The DOS guest's segmented
state is separate from this host-processor contract.

## Known errors

None identified in the cited page. Its generic description does not admit
the actual loaded process, historic libraries, descriptor bases, exceptional
transfers or custom register-input helpers. Source-only expectations do not
raise original evidence statuses or supply a complete_reading declaration.
