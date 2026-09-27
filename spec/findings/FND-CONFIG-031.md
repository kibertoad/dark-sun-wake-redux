---
id: FND-CONFIG-031
title: Message window child registration remains the setup failure path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:02B3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:060D
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x1E5F4..0x6A737
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident routines; DarkSunWakeRedux.Inspect ui-catalog
environment: null
---

## Observation

The resident registration routine at `3A8E:02B3` iterates a window's
children by their four-byte tags. Its dispatch table includes `BUTN` and
`APFM`. The `BUTN` branch calls resident `2EBE:0008`, and the `APFM`
branch calls resident `2F96:000B`; either branch returns failure from
registration when its helper returns `0xFFFF`. After the child loop succeeds,
registration clears the window's callback pointers at offsets `0xF9` and
`0xFD`, then returns zero. The installed `WIND/10501` has two children in
order: `BUTN/10309` and `APFM/11270` (FND-CONFIG-030).

The following callback-storage step writes only offset `0xF5`
(FND-CONFIG-030). The resident activation routine at `3A8E:060D` can return
failure for a zero window pointer or a nonzero result from either optional
callback at offsets `0xFD` and `0xF9`. It does not use the return values of
its child activation calls as failure gates. The two callback fields were
cleared by successful registration before this activation call.

## Interpretation

For a newly registered `WIND/10501` whose callback fields are not changed by
intervening calls, activation's own callback failure gates are inactive.
Its two child registration helpers remain possible failure paths. The
window's zero-pointer and position/bounds gates precede them
(FND-CONFIG-030).

## Alternatives

The `BUTN` and `APFM` helpers' failure conditions and the state in which
this message window is registered have not been read completely. This
finding does not prove that either helper succeeds, or that the message
wait runs in every live state. Other calls within activation have effects
that this reading has not established, although their return values do not
feed its own failure return.

## How to reproduce

Disassemble shipped `DSUN.EXE` file offsets `0x0002FD93..0x000300CE`
and `0x000300ED..0x000303C2` in 16-bit mode. Resolve the five registration
dispatch tags and branch offsets at resident `3A8E:05EF..060C`, and the
four activation tags at `3A8E:08E3..08FA`. Follow the `BUTN` and `APFM`
branches, the registration callback clears, and activation's two callback
tests. Run the read-only `DarkSunWakeRedux.Inspect ui-catalog` on the
approved installed `RESOURCE.GFF` and select `WIND/10501`.
