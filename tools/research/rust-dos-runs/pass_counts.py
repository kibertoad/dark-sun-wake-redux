"""Per-hit log of breakpoints and a watchpoint in the BUG-COMBAT-001 state, binned by pass.

usage: pass_counts.py <config.json>   (configs in counts/; pass_bins.py <log.json> bins a log by pass)
Restores play-ready of v175-play-ready-b-20261019 as indirect_targets.py does, sends the inputs,
and logs every stop (time after restore, site, far return, three argument words) with a cap of
50 hits per site. Overlay sites are given as [descriptor, offset] and are placed at each hit of a
pass marker by reading the overlay's loaded segment from its trampoline stub. Writes log.json
in D:/rust-dos-games/work/qc034/<name>. Nothing is written to game memory.
"""
import json, os, struct, sys
from pathlib import Path

RUNS = Path(__file__).resolve().parent
sys.path.insert(0, str(RUNS))
import xxhash
from dinorefurb_dosbox_session import (RUST_DOS_PINNED_SHA256, Media, RustDosConfig, RustDosSession,
                                       RustDosSettings, SoundBlaster, Target, read_saved_state)
from probe_drive import ROOT
from regenerate_checkpoints import admit_state, mapping
from restore_opener import verify_checkpoint_executable
from startup_window import wait_for_stop
from trampoline_census import layout, resident_fits

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
cfg = json.loads(Path(sys.argv[1]).read_text())
CAP = cfg.get("cap", 50)
config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
game = Path(config["original"]["analysisExecutable"]["path"]).parent
body = (game / "DSUN.EXE").read_bytes()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3)
header, image_end, trampolines, overlays = layout(body)
ov_code = {d: c for c, e, d in overlays}
source = game / "captures/rust-dos-regenerated/v175-play-ready-b-20261019"
meta = json.loads((source / "regeneration.json").read_text(encoding="utf-8"))
delta = mapping(meta.get("load_mapping_stop", meta["restored_stop"]))
assert delta & 0xFFFF == (0x05BA - 0x1000) & 0xFFFF
state = read_saved_state(source, "play-ready")
verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
admit_state(state)
run = Path("D:/rust-dos-games/work/qc034") / cfg["name"]
if run.exists():
    raise SystemExit("run exists")
settings = RustDosSettings(
    emulator=ROOT / "artifacts/rust-dos/rust-dos.exe",
    emulator_sha256=RUST_DOS_PINNED_SHA256, run_directory=run,
    target=Target("DSUN.EXE", ("-W0", "-L")), start_state=state,
    emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
        deterministic=True, cycles="15000",
        sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1), ultrasound=None),
    readiness_timeout=60.0)


def run_addr(static):
    seg, off = (int(x, 16) for x in static.split(":"))
    return f"{(seg + delta) & 0xFFFF:04X}:{off:04X}"


def static_of(seg):
    s = (seg - delta) & 0xFFFF
    return f"{s:04X}" if 0x1000 <= s < 0x1000 + (image_end - header + 15) // 16 else None


def stub_of(d):
    # The resident stub segment of overlay d (its trampolines' segment).
    return next(t["segment"] for t in trampolines if t["descriptor"] == d)


log = {"config": cfg, "events": [], "overlay_segments": [], "reads": []}
hits = {}


def save():
    (run / "log.json").write_text(json.dumps(log, indent=1) + "\n", encoding="utf-8")


with RustDosSession(settings) as s:
    assert s.stop["reason"] == "load" and s.stop["cs_ip"] == state.stop["cs_ip"]
    start_ms = int(s.stop["emu_ns"]) // 1000000
    log["restore_ms"] = start_ms
    rd = lambda a, n: s.client.read_memory(a, n).data

    def now():
        return int(s.stop["emu_ns"]) // 1000000 - start_ms

    def fits(mem, d, off):
        fo = ov_code[d] + off
        return resident_fits(mem, body[fo:fo + len(mem)], 0x05BA)

    sites = {}   # run address -> label
    stubs = {}   # run address of a trampoline site -> True
    for label, static in cfg.get("counts", {}).items():
        sites[run_addr(static)] = label
    for label, static in cfg.get("trampolines", {}).items():
        sites[run_addr(static)] = label
        stubs[run_addr(static)] = True
    ov_sites = {label: tuple(v) for label, v in cfg.get("overlay_counts", {}).items()}
    placed = {}  # label -> run address
    armed = [False]

    def arm():
        for a in sites:
            s.client.set_breakpoint(a, once=False)
        armed[0] = True

    def place_overlays(jump_seg):
        # At a pass marker hit whose stub is a far jump: the overlay's loaded segment.
        for label, (d, off) in ov_sites.items():
            addr = f"{jump_seg:04X}:{off:04X}"
            ok = fits(rd(addr, 12), d, off)
            entry = {"t": now(), "overlay": d, "segment": f"{jump_seg:04X}", "label": label, "verified": ok}
            if ok and placed.get(label) != addr and hits.get(label, 0) < CAP:
                if label in placed:
                    s.client.remove_breakpoints(placed[label])
                    sites.pop(placed[label], None)
                placed[label] = addr
                sites[addr] = label
                s.client.set_breakpoint(addr, once=False)
                entry["placed"] = addr
                log["overlay_segments"].append(entry)
            elif not ok:
                log["overlay_segments"].append(entry)

    watched = {}
    for addr, n in cfg.get("watch", []):
        s.client.set_watchpoint(addr, n)
        watched[addr] = (n, rd(addr, n))

    def handle(stop):
        t = int(stop["emu_ns"]) // 1000000 - start_ms
        regs = {k.lower(): (int(v, 16) if isinstance(v, str) else v) for k, v in stop["registers"].items()}
        ss, sp, bp = regs["ss"], regs["sp"], regs["bp"]
        if stop.get("reason") == "watchpoint":
            changed = {}
            for addr, (n, old) in watched.items():
                new = rd(addr, n)
                if new != old:
                    changed[addr] = f"{old.hex()}->{new.hex()}"
                    watched[addr] = (n, new)
            if not changed:
                return
            cs, ip = (int(x, 16) for x in stop["cs_ip"].split(":"))
            ev = {"t": t, "kind": "write", "changed": changed, "run": stop["cs_ip"],
                  "static": f"{static_of(cs)}:{ip:04X}" if static_of(cs) else None,
                  "regs": {k: f"{regs[k]:04X}" for k in ("ax", "bx", "cx", "dx", "si", "di", "bp", "sp", "ds", "es")},
                  "stack": rd(f"{ss:04X}:{sp:04X}", 48).hex()}
            chain = []
            for _ in range(6):
                if not bp:
                    break
                nbp, off, seg = struct.unpack("<HHH", rd(f"{ss:04X}:{bp:04X}", 6))
                chain.append({"bp": f"{bp:04X}", "ret": f"{seg:04X}:{off:04X}",
                              "static": f"{static_of(seg)}:{off:04X}" if static_of(seg) else None,
                              "args": rd(f"{ss:04X}:{(bp + 6) & 0xFFFF:04X}", 8).hex(),
                              "code_before": None if static_of(seg) else rd(f"{seg:04X}:{(off - 24) & 0xFFFF:04X}", 24).hex()})
                bp = nbp
            ev["chain"] = chain
            log["events"].append(ev)
            return
        a = stop.get("cs_ip")
        label = sites.get(a)
        if stop.get("reason") != "breakpoint" or label is None:
            log["unexpected"] = stop
            save()
            raise RuntimeError(f"unexpected stop {stop}")
        hits[label] = hits.get(label, 0) + 1
        w = struct.unpack("<5H", rd(f"{ss:04X}:{sp:04X}", 10))
        roff, rseg = w[0], w[1]
        ret = f"{static_of(rseg)}:{roff:04X}" if static_of(rseg) else f"run {rseg:04X}:{roff:04X}"
        ev = {"t": t, "kind": "hit", "site": label, "at": a, "ret": ret, "args": [f"{x:04X}" for x in w[2:]]}
        if a in stubs:
            sb = rd(a, 5)
            ev["stub"] = sb.hex()
        if label in ov_sites:
            d, off = ov_sites[label]
            ev["verified"] = fits(rd(a, 12), d, off)
        if cfg.get("code_before") and not static_of(rseg):
            ev["code_before"] = rd(f"{rseg:04X}:{(roff - 24) & 0xFFFF:04X}", 24).hex()
        if label in cfg.get("regs_for", []):
            ev["regs"] = {k: f"{regs[k]:04X}" for k in ("ax", "bx", "cx", "dx", "si", "di")}
        log["events"].append(ev)
        if label == cfg.get("marker") and ev.get("stub", "")[:2] == "ea":
            place_overlays(int(ev["stub"][8:10] + ev["stub"][6:8], 16))
        if hits[label] >= CAP:
            s.client.remove_breakpoints(a)

    def run_to(target_ms):
        cf = cfg.get("count_from", 0)
        if not armed[0] and target_ms > start_ms + cf:
            if now() < cf:
                run_to(start_ms + cf)
            arm()
        while True:
            ob, exhausted = wait_for_stop(s, s.continue_(until_ms=target_ms))
            if exhausted:
                raise RuntimeError("bounded continuation exhausted")
            if ob.stop.get("reason") == "time":
                return
            handle(ob.stop)

    events = sorted(cfg.get("inputs", []) + [{"at_ms": r["at_ms"], "read": r["addrs"]} for r in cfg.get("reads", [])],
                    key=lambda c: c["at_ms"])
    for c in events:
        run_to(start_ms + c["at_ms"])
        if "read" in c:
            log["reads"].append({"t": now(), **{a: rd(a, n).hex() for a, n in c["read"]}})
            continue
        move = s.send_input("mouse", {"action": "move", "x": c["x"], "y": c["y"], "coords": "virtual"})
        for _ in range(50):
            if not s.observe(move, timeout=0).pending:
                break
            run_to(int(s.stop["emu_ns"]) // 1000000 + 50)
        if c["button"] == "none":
            continue
        op = s.send_input("mouse", {"action": "click", "button": c["button"], "hold_ms": 200})
        for _ in range(50):
            if not s.observe(op, timeout=0).pending:
                break
            run_to(int(s.stop["emu_ns"]) // 1000000 + 50)
    run_to(start_ms + cfg["duration"])
    log["hits"] = hits
    log["end_ms"] = now()
    save()
print(json.dumps({"hits": hits, "events": len(log["events"]), "overlay_segments": log["overlay_segments"][:6]}))
