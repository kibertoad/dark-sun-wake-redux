"""Records which overlay routines a run enters through their trampolines, and from where.

Usage (repository root, evidence Python, GAME_DIR as in README.md):
  trampoline_census.py <source-run> <state> <new-run-name> <ms> [clicks.json]

Restores checkpoint <state> of captures/rust-dos-regenerated/<source-run> in a new owned session,
sets a one-shot execution breakpoint at every FMT-EXE-004 trampoline of the installed DSUN.EXE,
and runs <ms> emulated milliseconds. clicks.json, when given, is a list of
{"at_ms", "x", "y", "button"} left or right clicks in virtual 320 x 200 coordinates, each sent as
a separate completed move and click; button "none" only moves the pointer. At each trampoline stop it reads the far return address at
SS:SP and places the caller: a resident return whose preceding bytes match the load image (with
relocated segment words allowed to differ by the load segment) is named by its static address; any
other return is matched against overlay code by the far call's offset and the bytes before it.
The breakpoint goes after its first stop, so each trampoline is reported once. Makes no guest
writes and saves no state. Writes census.json in the run directory and prints a summary.
"""
import json
import os
from pathlib import Path
import re
import struct
import sys

import xxhash
from dinorefurb_dosbox_session import (
    Media, RUST_DOS_PINNED_SHA256, RustDosConfig, RustDosSession,
    RustDosSettings, SoundBlaster, Target, read_saved_state,
)

sys.path.insert(0, str(Path(__file__).resolve().parent))
from probe_drive import ROOT
from regenerate_checkpoints import admit_state, mapping
from restore_opener import verify_checkpoint_executable
from startup_window import wait_for_stop

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
WINDOW = 24


def layout(body):
    header = struct.unpack_from("<H", body, 8)[0] * 16
    cblp, pages = struct.unpack_from("<HH", body, 2)
    image_end = (pages - 1) * 512 + (cblp or 512)
    table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
    trampolines, overlays = [], []
    for d in range(count):
        seg, _, flags, _ = struct.unpack_from("<HHHH", body, table_offset + 8 * d)
        base = header + seg * 16
        if flags != 3 or body[base:base + 2] != b"\xcd\x3f":
            continue
        payload, size, _, n = struct.unpack_from("<IHHH", body, base + 4)
        code = image_end + 16 + payload
        overlays.append((code, code + size, d))
        for k in range(n):
            at = base + 0x20 + 5 * k
            if body[at:at + 2] != b"\xcd\x3f":
                continue
            target = struct.unpack_from("<H", body, at + 2)[0]
            trampolines.append({"segment": seg + 0x1000, "offset": 0x20 + 5 * k,
                                "descriptor": d, "target": target})
    return header, image_end, trampolines, overlays


def resident_fits(memory, image, delta):
    i = 0
    while i < len(memory):
        if memory[i] == image[i]:
            i += 1
            continue
        if i + 1 < len(memory):
            m = struct.unpack_from("<H", memory, i)[0]
            f = struct.unpack_from("<H", image, i)[0]
            if (m - f) & 0xFFFF == delta & 0xFFFF:
                i += 2
                continue
        return False
    return True


def place_caller(body, header, image_end, overlays, delta, seg, off, memory):
    static = seg - delta
    linear = header + (static - 0x1000) * 16 + off
    if header < linear - WINDOW and linear <= image_end:
        # The file's segment words are relative to segment 0; the load image puts them at 1000.
        if resident_fits(memory, body[linear - WINDOW:linear], delta + 0x1000):
            return {"kind": "resident", "address": f"{static:04X}:{off:04X}"}
    # A far call ends at the return: 9A, offset word, then a segment word the overlay manager set.
    call = memory[-5:-2]
    best = []
    for code, end, d in overlays:
        at = body.find(call, code, end)
        while at != -1:
            ret = at + 5
            if ret - WINDOW >= code:
                score = sum(a == b for a, b in zip(memory[:-2], body[ret - WINDOW:ret - 2]))
                best.append((score, d, ret - code))
            at = body.find(call, at + 1, end)
    best.sort(reverse=True)
    if best and best[0][0] >= WINDOW - 6 and (len(best) == 1 or best[1][0] < best[0][0]):
        return {"kind": "overlay", "descriptor": best[0][1], "return": f"+{best[0][2]:04X}",
                "score": best[0][0]}
    return {"kind": "unplaced", "candidates": [list(x) for x in best[:3]]}


def main():
    if len(sys.argv) not in (5, 6) or any(not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", x) for x in sys.argv[1:4]):
        raise SystemExit("usage: trampoline_census.py <source-run> <state> <new-run-name> <ms> [clicks.json]")
    duration = int(sys.argv[4])
    if not 0 < duration <= 600000:
        raise ValueError("duration must be 1 to 600000 ms")
    clicks = json.loads(Path(sys.argv[5]).read_text(encoding="utf-8")) if len(sys.argv) == 6 else []
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
    header, image_end, trampolines, overlays = layout(body)
    source = game / "captures/rust-dos-regenerated" / sys.argv[1]
    meta = json.loads((source / "regeneration.json").read_text(encoding="utf-8"))
    delta = mapping(meta.get("load_mapping_stop", meta["restored_stop"]))
    state = read_saved_state(source, sys.argv[2])
    verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
    admit_state(state)
    run = game / "captures/rust-dos-census" / sys.argv[3]
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
    by_address = {}
    for t in trampolines:
        by_address[f"{t['segment'] + delta:04X}:{t['offset']:04X}"] = t
    report = {"source_run": sys.argv[1], "state": sys.argv[2], "duration_ms": duration,
              "clicks": clicks, "trampolines": len(trampolines), "hits": [], "inputs": []}

    def record():
        (run / "census.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")

    with RustDosSession(settings) as s:
        if s.stop["reason"] != "load" or s.stop["cs_ip"] != state.stop["cs_ip"]:
            raise RuntimeError("checkpoint restore differs")
        start_ms = int(s.stop["emu_ns"]) // 1000000
        report["restored_stop"] = s.stop
        ds = 0x57E0 + delta

        def view():
            # The view origin DS:1408/140A and the 37-byte records of slots 0 to 3 at DS:67BB.
            origin = struct.unpack("<hh", s.client.read_memory(f"{ds:04X}:1408", 4).data)
            records = s.client.read_memory(f"{ds:04X}:67BB", 4 * 37).data
            return {"origin": origin, "slots": [records[37 * i:37 * i + 37].hex() for i in range(4)]}

        report["view_start"] = view()
        for address in by_address:
            s.client.set_breakpoint(address, once=True)
        pending = sorted(clicks, key=lambda c: c["at_ms"])
        end_ms = start_ms + duration

        def run_to(target_ms):
            while True:
                ob, exhausted = wait_for_stop(s, s.continue_(until_ms=target_ms))
                if exhausted:
                    raise RuntimeError("bounded continuation exhausted")
                stop = ob.stop
                if stop.get("reason") == "time":
                    return
                t = by_address.get(stop.get("cs_ip"))
                if stop.get("reason") != "breakpoint" or t is None:
                    report["unexpected_stop"] = stop
                    record()
                    raise RuntimeError(f"unexpected stop {stop}")
                r = stop["registers"]
                off, seg = struct.unpack("<HH", s.client.read_memory(f"{r['ss']}:{r['sp']}", 4).data)
                memory = s.client.read_memory(f"{seg:04X}:{(off - WINDOW) & 0xFFFF:04X}", WINDOW).data
                caller = place_caller(body, header, image_end, overlays, delta, seg, off, memory)
                report["hits"].append({
                    "trampoline": f"{t['segment']:04X}:{t['offset']:04X}",
                    "routine": f"{t['descriptor']}+{t['target']:04X}",
                    "at_ms": int(stop["emu_ns"]) // 1000000 - start_ms,
                    "return": f"{seg:04X}:{off:04X}", "caller": caller})

        for index, c in enumerate(pending):
            run_to(start_ms + c["at_ms"])
            (run / f"before-{index}.png").write_bytes(s.client.screenshot())
            move = s.send_input("mouse", {"action": "move", "x": c["x"], "y": c["y"], "coords": "virtual"})
            for _ in range(50):
                if not s.observe(move, timeout=0).pending:
                    break
                run_to(int(s.stop["emu_ns"]) // 1000000 + 50)
            if c["button"] == "none":
                report["inputs"].append({**c, "state": "moved"})
                continue
            op = s.send_input("mouse", {"action": "click", "button": c["button"], "hold_ms": 200})
            for _ in range(50):
                result = s.observe(op, timeout=0)
                if not result.pending:
                    break
                run_to(int(s.stop["emu_ns"]) // 1000000 + 50)
            report["inputs"].append({**c, "state": result.state})
            if result.state != "done":
                record()
                raise RuntimeError("click delivery not confirmed")
        run_to(end_ms)
        report["view_end"] = view()
        (run / "final.png").write_bytes(s.client.screenshot())
        record()
    placed = sum(h["caller"]["kind"] != "unplaced" for h in report["hits"])
    print(json.dumps({"hits": len(report["hits"]), "placed": placed, "run": str(run)}), flush=True)


if __name__ == "__main__":
    main()
