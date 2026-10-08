---
id: SRC-MS-CRT-ASSERT
title: Microsoft CRT assertion failure contract
superseded_by: []
author: Microsoft
date: "2023-02-07"
location: https://learn.microsoft.com/en-us/cpp/c-runtime-library/reference/assert-macro-assert-wassert?view=msvc-170
xxh3: null
licence: null
---

## Use

Read on 2026-10-08 for Q-EXE-009 and FND-EXE-170. Microsoft describes
`_assert` as an internal CRT routine with message, filename and line-number
parameters. Its assertion documentation describes diagnostic output and
Abort, Retry and Ignore choices in dialog mode. Ignore can continue in that
mode; output mode affects behavior and can be overridden.

This is a published external contract, not an observation of the original's
loaded CRT. It is a lead for checking failure effects and continuations,
not evidence that a particular branch returns or that any native response
was selected.

## Known errors

None identified in the page. The documented CRT need not match the historic
system library loaded by the shipped executable. No native run, status
promotion or complete_reading declaration follows from this source.