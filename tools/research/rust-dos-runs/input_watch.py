"""Input and watchpoint driver: restore play-ready of v175-play-ready-b-20261019, send keys and mouse input,
take screenshots, read memory, and log every watchpoint stop on DS:627C and DS:1462.

usage: input_watch.py <config.json>   (configs in counts/qsave006-*.json)   (run from the repository root with the evidence Python, -I)
config: {"name": ..., "duration": ms, "watch": [[addr, n], ...], "breaks": [run addr, ...],
         "events": [{"at_ms": N, "key": "f3"} | {"at_ms": N, "type": "abc"} |
                    {"at_ms": N, "move": [x, y]} | {"at_ms": N, "click": [x, y, "left"]} |
                    {"at_ms": N, "shot": "name"} | {"at_ms": N, "read": [[addr, n], ...]} |
                    {"at_ms": N, "save": "state"}]}
Times are emulated ms after the restore. Run output goes to D:/rust-dos-games/work/qsave006/<name>.
Nothing is written to game memory.
"""
import hashlib, json, struct, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "tools/research/rust-dos-runs"))
import xxhash
from dinorefurb_dosbox_session import (RUST_DOS_PINNED_SHA256, Media, RustDosConfig, RustDosSession,
                                       RustDosSettings, SoundBlaster, Target, read_saved_state)
from regenerate_checkpoints import admit_state, mapping
from restore_opener import verify_checkpoint_executable
from startup_window import wait_for_stop

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
cfg = json.loads(Path(sys.argv[1]).read_text())
config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
game = Path(config["original"]["analysisExecutable"]["path"]).parent
body = (game / "DSUN.EXE").read_bytes()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3)
if cfg.get("start"):
    d, nm = cfg["start"].rsplit(":", 1)
    source = Path(d)
    state = read_saved_state(source, nm)
    delta = (0x05BA - 0x1000) & 0xFFFF
else:
    source = game / "captures/rust-dos-regenerated/v175-play-ready-b-20261019"
    meta = json.loads((source / "regeneration.json").read_text(encoding="utf-8"))
    delta = mapping(meta.get("load_mapping_stop", meta["restored_stop"]))
    assert delta & 0xFFFF == (0x05BA - 0x1000) & 0xFFFF
    state = read_saved_state(source, "play-ready")
verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
admit_state(state)
run = Path("D:/rust-dos-games/work/qsave006") / cfg["name"]
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


def static_of(seg):
    s = (seg - delta) & 0xFFFF
    return f"{s:04X}" if 0x1000 <= s < 0x5A00 else None


log = {"config": cfg, "events": []}


def save():
    (run / "log.json").write_text(json.dumps(log, indent=1) + "\n", encoding="utf-8")


def out(*a):
    print(*a, flush=True)


with RustDosSession(settings) as s:
    assert s.stop["reason"] == "load"
    start_ms = int(s.stop["emu_ns"]) // 1000000
    log["restore_ms"] = start_ms
    rd = lambda a, n: s.client.read_memory(a, n).data

    def now():
        return int(s.stop["emu_ns"]) // 1000000 - start_ms

    watched = {}
    for addr, n in cfg.get("watch", []):
        s.client.set_watchpoint(addr, n)
        watched[addr] = (n, rd(addr, n))
    log["watch_initial"] = {a: v[1].hex() for a, v in watched.items()}
    out("initial", log["watch_initial"])
    breaks = set(cfg.get("breaks", []))
    for a in breaks:
        s.client.set_breakpoint(a, once=False)
    nstops = [0]

    def chain_of(regs):
        ss, bp = regs["ss"], regs["bp"]
        chain = []
        for _ in range(6):
            if not bp:
                break
            nbp, off, seg = struct.unpack("<HHH", rd(f"{ss:04X}:{bp:04X}", 6))
            chain.append({"bp": f"{bp:04X}", "ret": f"{seg:04X}:{off:04X}",
                          "static": f"{static_of(seg)}:{off:04X}" if static_of(seg) else None,
                          "code_before": None if static_of(seg) else rd(f"{seg:04X}:{(off - 24) & 0xFFFF:04X}", 24).hex()})
            bp = nbp
        return chain

    def handle(stop):
        t = now()
        regs = {k.lower(): (int(v, 16) if isinstance(v, str) else v) for k, v in stop["registers"].items()}
        cs, ip = (int(x, 16) for x in stop["cs_ip"].split(":"))
        where = f"{static_of(cs)}:{ip:04X}" if static_of(cs) else None
        nstops[0] += 1
        if stop.get("reason") == "watchpoint":
            changed = {}
            for addr, (n, old) in watched.items():
                new = rd(addr, n)
                if new != old:
                    changed[addr] = f"{old.hex()}->{new.hex()}"
                    watched[addr] = (n, new)
            ev = {"t": t, "kind": "write", "changed": changed, "run": stop["cs_ip"], "static": where,
                  "code_at": rd(f"{cs:04X}:{(ip - 8) & 0xFFFF:04X}", 12).hex(),
                  "chain": chain_of(regs) if changed else None}
            out("write", t, changed, stop["cs_ip"], where)
            log["events"].append(ev)
            return True
        if stop.get("reason") == "breakpoint" and stop["cs_ip"] in breaks:
            ev = {"t": t, "kind": "break", "run": stop["cs_ip"], "static": where,
                  "regs": {k: f"{regs[k]:04X}" for k in ("ax", "bx", "cx", "dx", "si", "di", "ds", "es")},
                  "chain": chain_of(regs)}
            out("break", t, stop["cs_ip"], where)
            log["events"].append(ev)
            return True
        log["events"].append({"t": t, "kind": "other", "stop": stop})
        out("other stop", t, json.dumps(stop)[:400])
        return False

    ended = [False]

    def run_to(target_ms):
        while not ended[0]:
            if now() >= target_ms:
                return
            if nstops[0] > cfg.get("max_stops", 400):
                raise RuntimeError("too many stops")
            ob, exhausted = wait_for_stop(s, s.continue_(until_ms=start_ms + target_ms))
            if exhausted:
                log["events"].append({"t": now(), "kind": "exhausted", "stop": ob.stop})
                out("exhausted", now(), json.dumps(ob.stop)[:300])
                ended[0] = True
                return
            if ob.stop.get("reason") == "time":
                return
            if not handle(ob.stop):
                ended[0] = True
                return

    def deliver(op):
        for _ in range(60):
            if not s.observe(op, timeout=0).pending:
                return True
            run_to(now() + 50)
            if ended[0]:
                return False
        return False

    for c in sorted(cfg["events"], key=lambda c: c["at_ms"]):
        run_to(c["at_ms"])
        if ended[0]:
            break
        t = now()
        if "read" in c:
            r = {a: rd(a, n).hex() for a, n in c["read"]}
            log["events"].append({"t": t, "kind": "read", **r})
            out("read", t, r)
        if "shot" in c:
            png = s.client.screenshot()
            (run / f"{c['shot']}.png").write_bytes(png)
            md5 = hashlib.md5(png).hexdigest()
            log["events"].append({"t": t, "kind": "shot", "name": c["shot"], "md5": md5})
            out("shot", t, c["shot"], md5)
        if "save" in c:
            st = s.save_state(c["save"], check_files=True)
            log["events"].append({"t": t, "kind": "save_state", "name": c["save"]})
            out("saved", c["save"])
        op = None
        if "key" in c:
            op = s.send_input("key", {"key": c["key"]})
        elif "type" in c:
            op = s.send_input("type", {"text": c["type"]})
        elif "move" in c:
            op = s.send_input("mouse", {"action": "move", "x": c["move"][0], "y": c["move"][1], "coords": "virtual"})
        elif "click" in c:
            x, y, b = c["click"]
            mv = s.send_input("mouse", {"action": "move", "x": x, "y": y, "coords": "virtual"})
            deliver(mv)
            op = s.send_input("mouse", {"action": "click", "button": b, "hold_ms": 200})
        if op is not None:
            ok = deliver(op)
            log["events"].append({"t": now(), "kind": "input", "input": {k: v for k, v in c.items() if k != "at_ms"},
                                  "sent_at": t, "delivered": ok})
            out("input", t, now(), {k: v for k, v in c.items() if k != "at_ms"}, ok)
    if not ended[0]:
        run_to(cfg["duration"])
    log["end_ms"] = now()
    try:
        png = s.client.screenshot()
        (run / "final.png").write_bytes(png)
        log["final_md5"] = hashlib.md5(png).hexdigest()
        out("final shot", log["final_md5"])
        log["final_watch"] = {a: rd(a, n).hex() for a, (n, _) in watched.items()}
        out("final", log["final_watch"])
    except Exception as e:
        log["final_error"] = repr(e)
        out("final error", repr(e))
    save()
print("done", log["end_ms"])
