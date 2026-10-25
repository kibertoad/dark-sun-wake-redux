"""Q-COMBAT-035 driver, adapted from tools/research/rust-dos-runs/input_watch.py.

usage: bug_exit.py <config.json>  (configs in counts/qc035-*.json; run from the repository root with the evidence Python, -I)
Restores play-ready of v175-play-ready-b-20261019 (or cfg "start" = "<dir>:<state>"), sends keys and
mouse input, takes screenshots, reads memory, logs every change on the watched ranges with the
instruction that made it (resident address or overlay descriptor+offset matched against DSUN.EXE),
and counts unchanged writes and breakpoint hits per site. Events may switch watchpoints and
breakpoints on and off: {"watch_on": [[a, n]]}, {"watch_off": [a]}, {"break_on": [a]}, {"break_off": [a]}.
Run output goes to D:/rust-dos-games/work/qc035/<name>. Nothing is written to game memory.
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
from trampoline_census import layout, resident_fits

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
cfg = json.loads(Path(sys.argv[1]).read_text())
config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
game = Path(config["original"]["analysisExecutable"]["path"]).parent
body = (game / "DSUN.EXE").read_bytes()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3)
header, image_end, trampolines, overlays = layout(body)
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
run = Path("D:/rust-dos-games/work/qc035") / cfg["name"]
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
    return f"{s:04X}" if 0x1000 <= s < 0x1000 + (image_end - header + 15) // 16 else None


log = {"config": cfg, "events": [], "counts": {}}


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

    ovcache = {}

    def where_of(cs, ip):
        st = static_of(cs)
        if st:
            return f"{st}:{ip:04X}"
        key = (cs, ip)
        if key in ovcache:
            return ovcache[key]
        lo = max(0, ip - 12)
        mem = rd(f"{cs:04X}:{lo:04X}", 24)
        found = []
        for code, end, d in overlays:
            if code + lo + 24 <= end:
                img = body[code + lo:code + lo + 24]
                score = sum(1 for a, b in zip(mem, img) if a == b)
                if score >= 19:
                    found.append(f"ov{d}+{ip:04X}")
        r = "|".join(found) if found else f"run {cs:04X}:{ip:04X}"
        ovcache[key] = r
        return r

    watched = {}

    def watch_on(addr, n):
        s.client.set_watchpoint(addr, n)
        watched[addr] = (n, rd(addr, n))

    for addr, n in cfg.get("watch", []):
        watch_on(addr, n)
    log["watch_initial"] = {a: v[1].hex() for a, v in watched.items()}
    out("initial", log["watch_initial"])
    breaks = set()

    def break_on(a):
        s.client.set_breakpoint(a, once=False)
        breaks.add(a)

    for a in cfg.get("breaks", []):
        break_on(a)
    nstops = [0]

    def chain_of(regs):
        ss, bp = regs["ss"], regs["bp"]
        chain = []
        for _ in range(6):
            if not bp:
                break
            nbp, off, seg = struct.unpack("<HHH", rd(f"{ss:04X}:{bp:04X}", 6))
            chain.append({"bp": f"{bp:04X}", "ret": f"{seg:04X}:{off:04X}", "static": where_of(seg, off)})
            bp = nbp
        return chain

    def count(key, t):
        c = log["counts"].setdefault(key, {"n": 0, "first": t, "last": t})
        c["n"] += 1
        c["last"] = t

    def handle(stop):
        t = now()
        regs = {k.lower(): (int(v, 16) if isinstance(v, str) else v) for k, v in stop["registers"].items()}
        cs, ip = (int(x, 16) for x in stop["cs_ip"].split(":"))
        nstops[0] += 1
        if stop.get("reason") == "watchpoint":
            where = where_of(cs, ip)
            changed = {}
            for addr, (n, old) in watched.items():
                new = rd(addr, n)
                if new != old:
                    changed[addr] = f"{old.hex()}->{new.hex()}"
                    watched[addr] = (n, new)
            if not changed:
                count(f"same {where}", t)
                return True
            ev = {"t": t, "kind": "write", "changed": changed, "run": stop["cs_ip"], "static": where,
                  "chain": chain_of(regs)}
            if cfg.get("quiet_changes") and all(a in cfg["quiet_changes"] for a in changed):
                count(f"change {','.join(changed)} {where}", t)
                return True
            for ca, cv, offs in cfg.get("chase", []):
                if ca in changed and changed[ca].endswith("->" + cv):
                    for o in offs:
                        a = f"{cs:04X}:{o}"
                        if a not in breaks:
                            break_on(a)
                            ev.setdefault("chased", []).append(a)
            out("write", t, changed, stop["cs_ip"], where)
            log["events"].append(ev)
            return True
        if stop.get("reason") == "breakpoint" and stop["cs_ip"] in breaks:
            count(f"break {stop['cs_ip']}", t)
            if cfg.get("log_breaks", True):
                ss, sp = regs["ss"], regs["sp"]
                w = struct.unpack("<7H", rd(f"{ss:04X}:{sp:04X}", 14))
                ev = {"t": t, "kind": "break", "run": stop["cs_ip"], "static": where_of(cs, ip),
                      "stack": [f"{x:04X}" for x in w], "stub": rd(stop["cs_ip"], 5).hex(),
                      "regs": {k: f"{regs[k]:04X}" for k in ("ax", "bx", "cx", "dx", "es")},
                      "code": rd(stop["cs_ip"], 8).hex(),
                      "ret_near": where_of(cs, w[0]), "ret_far": where_of(w[1], w[0]),
                      "chain": chain_of(regs) if cfg.get("break_chain") else None}
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
            if nstops[0] > cfg.get("max_stops", 20000):
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

    try:
        shift = 0
        for c in sorted(cfg["events"], key=lambda c: c["at_ms"]):
            run_to(c["at_ms"] + shift)
            if ended[0]:
                break
            t = now()
            for a, n in c.get("watch_on", []):
                watch_on(a, n)
                log["events"].append({"t": t, "kind": "watch_on", "addr": a, "value": watched[a][1].hex()})
            for a in c.get("watch_off", []):
                s.client.remove_watchpoints(a)
                watched.pop(a, None)
                log["events"].append({"t": t, "kind": "watch_off", "addr": a})
            for a in c.get("break_on", []):
                break_on(a)
                log["events"].append({"t": t, "kind": "break_on", "addr": a})
            for a in c.get("break_off", []):
                s.client.remove_breakpoints(a)
                breaks.discard(a)
                log["events"].append({"t": t, "kind": "break_off", "addr": a})
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
            if c.get("snapshot_counts"):
                log["events"].append({"t": t, "kind": "counts", "counts": json.loads(json.dumps(log["counts"]))})
                out("counts", t, {k: v["n"] for k, v in log["counts"].items()})
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
            if "key_until" in c:
                # press key, wait gap ms, read addr; repeat until it reads value (hex) or tries run out.
                key, addr, n, value, tries, gap = c["key_until"]
                for k in range(tries):
                    t0 = now()
                    op2 = s.send_input("key", {"key": key})
                    ok = deliver(op2)
                    log["events"].append({"t": now(), "kind": "input", "input": {"key": key}, "sent_at": t0,
                                          "delivered": ok, "try": k})
                    run_to(t0 + gap)
                    got = rd(addr, n).hex()
                    out("key_until", key, t0, now(), addr, got)
                    log["events"].append({"t": now(), "kind": "read", addr: got, "try": k})
                    if got == value:
                        break
                shift = now() - c["at_ms"]
                log["events"].append({"t": now(), "kind": "shift", "shift": shift})
            if op is not None:
                ok = deliver(op)
                log["events"].append({"t": now(), "kind": "input", "input": {k: v for k, v in c.items() if k != "at_ms"},
                                      "sent_at": t, "delivered": ok})
                out("input", t, now(), {k: v for k, v in c.items() if k not in ("at_ms", "read")}, ok)
        if not ended[0]:
            run_to(cfg["duration"])
        log["end_ms"] = now()
        png = s.client.screenshot()
        (run / "final.png").write_bytes(png)
        log["final_md5"] = hashlib.md5(png).hexdigest()
        out("final shot", log["final_md5"])
        log["final_watch"] = {a: rd(a, n).hex() for a, (n, _) in watched.items()}
        out("final", log["final_watch"])
    except Exception as e:
        log["error"] = repr(e)
        out("error", repr(e))
    finally:
        save()
out("counts", {k: v for k, v in log["counts"].items()})
print("done", log.get("end_ms"))
