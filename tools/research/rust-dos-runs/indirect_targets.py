"""Records the targets that memory-indirect calls and jumps take in a rust-dos run.

Usage: python indirect_targets.py <source-run> <state> <new-run-name> <ms> <site> [<site> ...]
           [--clicks clicks.json] [--read SEG:OFF:LEN ...]

Restores checkpoint <state> of captures/rust-dos-regenerated/<source-run> in a new owned session,
as trampoline_census.py does, and sets a breakpoint that stays armed on each <site>, a static
load-image address such as 1000:1D8D. At each stop it decodes the instruction there, forms the
memory operand's address from the stop's registers (DS, or SS for a BP base, unless the
instruction names a segment), and reads the word (near) or double word (far) the transfer takes.
It counts each distinct target per site, with the run time of its first hit, and removes a site's
breakpoint after 200 hits. Targets are given both as run addresses and, for load-image segments,
as static ones. Each --read reads LEN bytes at the static address SEG:OFF once after the restore
and once at the end, and records them in hexadecimal. Writes targets.json under captures/rust-dos-indirect/<new-run-name>. Nothing is
written to the game's memory.
"""
import json
import os
import re
import struct
import sys
from pathlib import Path

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_MEM
from dinorefurb_dosbox_session import (RUST_DOS_PINNED_SHA256, Media, RustDosConfig, RustDosSession,
                                       RustDosSettings, SoundBlaster, Target, read_saved_state)

sys.path.insert(0, str(Path(__file__).resolve().parent))
from probe_drive import ROOT
from regenerate_checkpoints import admit_state, mapping
from restore_opener import verify_checkpoint_executable
from startup_window import wait_for_stop

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
CAP = 200
SEGMENT_NAMES = {"cs", "ds", "es", "ss", "fs", "gs"}


def operand_address(insn, regs):
    """The segment and offset of the instruction's memory operand under the given registers."""
    mem = next(op.mem for op in insn.operands if op.type == X86_OP_MEM)
    base = insn.reg_name(mem.base) if mem.base else None
    index = insn.reg_name(mem.index) if mem.index else None
    offset = mem.disp
    for r in (base, index):
        if r:
            offset += int(regs[r], 16) if isinstance(regs[r], str) else regs[r]
    segment = insn.reg_name(mem.segment) if mem.segment else ("ss" if base == "bp" else "ds")
    value = regs[segment]
    return (int(value, 16) if isinstance(value, str) else value), offset & 0xFFFF


def main():
    args = sys.argv[1:]
    clicks, reads = [], []
    while "--read" in args:
        i = args.index("--read")
        seg, off, length = (int(x, 16) for x in args[i + 1].split(":"))
        reads.append((seg, off, length))
        del args[i:i + 2]
    if "--clicks" in args:
        i = args.index("--clicks")
        clicks = json.loads(Path(args[i + 1]).read_text(encoding="utf-8"))
        del args[i:i + 2]
    if len(args) < 4 or any(not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", x) for x in args[:3]):
        raise SystemExit(__doc__)
    duration = int(args[3])
    if not 0 < duration <= 600000:
        raise ValueError("duration must be 1 to 600000 ms")
    sites = [tuple(int(x, 16) for x in a.split(":")) for a in args[4:]]
    for c in clicks:
        if (c.get("button") not in ("left", "right", "none") or not 0 <= c["x"] < 320 or not 0 <= c["y"] < 200
                or not 0 <= c["at_ms"] < duration):
            raise ValueError("unsupported click")
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    body = (game / "DSUN.EXE").read_bytes()
    if (len(body), xxhash.xxh3_128_hexdigest(body)) != (SIZE, XXH3):
        raise ValueError("not the installed DSUN.EXE")
    cblp, pages = struct.unpack_from("<HH", body, 2)
    header = struct.unpack_from("<H", body, 8)[0] * 16
    image_end = (pages - 1) * 512 + (cblp or 512)
    load_paragraphs = (image_end - header + 15) // 16
    source = game / "captures/rust-dos-regenerated" / args[0]
    meta = json.loads((source / "regeneration.json").read_text(encoding="utf-8"))
    delta = mapping(meta.get("load_mapping_stop", meta["restored_stop"]))
    state = read_saved_state(source, args[1])
    verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
    admit_state(state)
    run = game / "captures/rust-dos-indirect" / args[2]
    if run.exists():
        raise ValueError("run already exists")
    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe",
        emulator_sha256=RUST_DOS_PINNED_SHA256, run_directory=run,
        target=Target("DSUN.EXE", ("-W0", "-L")), start_state=state,
        emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
            deterministic=True, cycles="15000",
            sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1), ultrasound=None),
        readiness_timeout=60.0)
    md = Cs(CS_ARCH_X86, CS_MODE_16)
    md.detail = True
    by_address = {}
    for seg, off in sites:
        lin = header + (seg - 0x1000) * 16 + off
        insn = next(md.disasm(body[lin:lin + 8], off))
        far = insn.bytes[insn.bytes.index(0xFF) + 1] >> 3 & 7 in (3, 5)
        by_address[f"{(seg + delta) & 0xFFFF:04X}:{off:04X}"] = {
            "site": f"{seg:04X}:{off:04X}", "instruction": f"{insn.mnemonic} {insn.op_str}",
            "far": far, "insn": insn, "hits": 0, "targets": {}}

    def static_of(seg):
        s = (seg - delta) & 0xFFFF
        return f"{s:04X}" if 0x1000 <= s < 0x1000 + load_paragraphs else None

    report = {"source_run": args[0], "state": args[1], "duration_ms": duration, "clicks": clicks,
              "delta": f"{delta & 0xFFFF:04X}", "sites": []}

    def record():
        report["sites"] = [{k: v for k, v in e.items() if k != "insn"} for e in by_address.values()]
        (run / "targets.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")

    with RustDosSession(settings) as s:
        if s.stop["reason"] != "load" or s.stop["cs_ip"] != state.stop["cs_ip"]:
            raise RuntimeError("checkpoint restore differs")
        start_ms = int(s.stop["emu_ns"]) // 1000000

        def read_all(name):
            report[name] = {f"{seg:04X}:{off:04X}": s.client.read_memory(
                f"{(seg + delta) & 0xFFFF:04X}:{off:04X}", length).data.hex() for seg, off, length in reads}

        read_all("reads_start")
        for address in by_address:
            s.client.set_breakpoint(address, once=False)

        def run_to(target_ms):
            while True:
                ob, exhausted = wait_for_stop(s, s.continue_(until_ms=target_ms))
                if exhausted:
                    raise RuntimeError("bounded continuation exhausted")
                stop = ob.stop
                if stop.get("reason") == "time":
                    return
                e = by_address.get(stop.get("cs_ip"))
                if stop.get("reason") != "breakpoint" or e is None:
                    report["unexpected_stop"] = stop
                    record()
                    raise RuntimeError(f"unexpected stop {stop}")
                regs = {k.lower(): v for k, v in stop["registers"].items()}
                seg, off = operand_address(e["insn"], regs)
                data = s.client.read_memory(f"{seg:04X}:{off:04X}", 4 if e["far"] else 2).data
                if e["far"]:
                    toff, tseg = struct.unpack("<HH", data)
                else:
                    toff, tseg = struct.unpack("<H", data)[0], int(stop["cs_ip"].split(":")[0], 16)
                key = f"{tseg:04X}:{toff:04X}"
                t = e["targets"].setdefault(key, {"count": 0, "first_ms": int(stop["emu_ns"]) // 1000000 - start_ms,
                                                  "static": (f"{static_of(tseg)}:{toff:04X}"
                                                             if static_of(tseg) else None),
                                                  "pointer": f"{seg:04X}:{off:04X}"})
                t["count"] += 1
                e["hits"] += 1
                if e["hits"] >= CAP:
                    s.client.remove_breakpoints(stop["cs_ip"])

        for c in sorted(clicks, key=lambda c: c["at_ms"]):
            run_to(start_ms + c["at_ms"])
            move = s.send_input("mouse", {"action": "move", "x": c["x"], "y": c["y"], "coords": "virtual"})
            for _ in range(50):
                if not s.observe(move, timeout=0).pending:
                    break
                run_to(int(s.stop["emu_ns"]) // 1000000 + 50)
            if c["button"] == "none":
                continue
            op = s.send_input("mouse", {"action": "click", "button": c["button"], "hold_ms": 200})
            for _ in range(50):
                result = s.observe(op, timeout=0)
                if not result.pending:
                    break
                run_to(int(s.stop["emu_ns"]) // 1000000 + 50)
        run_to(start_ms + duration)
        read_all("reads_end")
        record()
    print(json.dumps({e["site"]: {"hits": e["hits"], "targets": list(e["targets"])}
                      for e in by_address.values()}), flush=True)


if __name__ == "__main__":
    main()
