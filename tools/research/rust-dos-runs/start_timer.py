"""Q-TIME-005: program-start run watching the sound library's driver installs and timer slots.

usage (repository root, evidence Python, -I): start_timer.py <config.json>   (configs in counts/qt005-*.json)
config: name, duration_ms, keys: [[at_ms, key], ...], table_every_ms, wait_cap, extra_reads
Nothing is written to game memory. Output: D:/rust-dos-games/work/qt005/<name>/log.json
"""
import json, os, struct, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "tools/research/rust-dos-runs"))
import xxhash
from dinorefurb_dosbox_session import (RUST_DOS_PINNED_SHA256, Media, RustDosConfig, RustDosSession,
                                       RustDosSettings, SoundBlaster, Target)
from probe_drive import prepare_drive
from startup_window import wait_for_stop

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
cfg = json.loads(Path(sys.argv[1]).read_text())
config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
game = Path(config["original"]["analysisExecutable"]["path"]).parent
body = (game / "DSUN.EXE").read_bytes()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3)
run = Path("D:/rust-dos-games/work/qt005") / cfg["name"]
if run.exists():
    raise SystemExit("run exists")
run.parent.mkdir(parents=True, exist_ok=True)
sb = SoundBlaster(base=0x220, irq=5, dma=1)


def prepare(d):
    n = prepare_drive(game, d)
    if "cfg14" in cfg:
        # Non-shipped launch: rewrite word 0x14 of the run's own copy of SOUND.CFG (never the installation).
        p = Path(d) / "SOUND.CFG"
        b = bytearray(p.read_bytes())
        assert len(b) == 59 and struct.unpack_from("<H", b, 0x14)[0] == 11
        struct.pack_into("<H", b, 0x14, cfg["cfg14"])
        p.write_bytes(bytes(b))
    return n


settings = RustDosSettings(
    emulator=ROOT / "artifacts/rust-dos/rust-dos.exe",
    emulator_sha256=RUST_DOS_PINNED_SHA256, run_directory=run,
    target=Target("DSUN.EXE", tuple(cfg.get("args", ["-W0", "-L"]))),
    prepare_drive=prepare,
    emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
                                  deterministic=True, cycles="15000", sound_blaster=sb, ultrasound=None),
    readiness_timeout=60.0)

log = {"config": cfg, "settings": {"sound_blaster": repr(sb), "ultrasound": None, "cycles": "15000",
                                   "deterministic": True, "args": cfg.get("args", ["-W0", "-L"])},
       "events": [], "hits": {}}


def save():
    (run / "log.json").write_text(json.dumps(log, indent=1) + "\n", encoding="utf-8")


def out(*a):
    print(*a, flush=True)


with RustDosSession(settings) as s:
    entry = s.entry
    loaded = int(entry["registers"]["cs"], 16)
    log["entry"] = entry
    log["version"] = s.client.version()
    out("entry", entry["cs_ip"], "loaded", hex(loaded), log["version"])

    def seg(S):
        return (S - 0x1000 + loaded) & 0xFFFF

    def A(S, off):
        return f"{seg(S):04X}:{off:04X}"

    rd = lambda a, n: s.client.read_memory(a, n).data
    DS, TL = 0x57E0, 0x4842

    def emu_ns():
        return int(s.stop.get("emu_ns", 0)) if getattr(s, "stop", None) else 0

    def static(cs):
        st = (cs - loaded + 0x1000) & 0xFFFF
        return f"{st:04X}" if 0x1000 <= st < 0x5A00 else None

    def table():
        b = rd(A(TL, 0), 0x124)
        states = struct.unpack_from("<17H", b, 0x6C)
        cbs = struct.unpack_from("<34H", b, 0x06)
        per = struct.unpack_from("<17I", b, 0xD2)
        slots = []
        for i in range(17):
            if states[i]:
                cbseg = cbs[2 * i + 1]
                slots.append({"slot": i, "state": states[i],
                              "cb": f"{static(cbseg) or 'run ' + format(cbseg, '04X')}:{cbs[2 * i]:04X}",
                              "period_us": per[i]})
        return {"count": struct.unpack_from("<H", b, 2)[0], "current_us": struct.unpack_from("<I", b, 0x116)[0],
                "slots": slots, "tick": struct.unpack("<I", rd(A(DS, 0x145E), 4))[0]}

    def regs():
        return {k.lower(): int(v, 16) for k, v in s.stop["registers"].items() if isinstance(v, str)}

    def stackw(r, n):
        return struct.unpack(f"<{n}H", rd(f"{r['ss']:04X}:{r['sp']:04X}", 2 * n))

    B = {}

    def brk(S, off, label, nwords, extra=None, cap=100, exit_=False):
        B[A(S, off)] = {"label": label, "n": nwords, "extra": extra, "cap": cap, "exit": exit_}

    # sound library
    brk(0x4734, 0x0128, "set_music_word(3444)", 3)
    brk(0x4734, 0x0133, "sound_init_local", 2, extra="cfg")
    brk(0x4734, 0x0293, "music_decision(4734:0293)", 2, extra="flags")
    brk(0x4734, 0x039B, "after_music_block(4734:039B)", 2, extra="flags")
    brk(0x4734, 0x03A6, "digital_gate(4734:03A6)", 2, extra="flags")
    brk(0x49E9, 0x018F, "digital_load(49E9:018F)", 2, exit_=True)
    brk(TL, 0x0A23, "drv_install_image(0A23)", 4, exit_=True)
    brk(TL, 0x0B73, "drv_fn65(0B73)", 7, exit_=True)
    brk(TL, 0x0B79, "drv_install(0B79)", 7, exit_=True)
    brk(TL, 0x0B9F, "descriptor(0B9F)", 2, extra="desc")
    brk(TL, 0x0BA8, "fn67_lookup(0BA8)", 2)
    brk(TL, 0x0BE6, "after_slot(0BE6)", 2)
    brk(TL, 0x077F, "slot_alloc(077F)", 4, exit_=True)
    brk(TL, 0x093D, "set_period_us(093D)", 5, cap=60)
    brk(TL, 0x0992, "set_rate_hz(0992)", 5, cap=60)
    brk(TL, 0x08B1, "slot_start(08B1)", 3, cap=60)
    brk(TL, 0x08F7, "slot_stop(08F7)", 3, cap=60)
    brk(TL, 0x084A, "slot_release(084A)", 3, cap=60)
    brk(TL, 0x0C2B, "drv_uninstall(0C2B)", 3, cap=20)
    brk(0x1000, 0x12FA, "wait_ms(12FA)", 3, cap=cfg.get("wait_cap", 40), exit_=True)
    exits = {}
    nstops = [0]
    t0 = [None]

    def now_ms():
        return (emu_ns() - t0[0]) / 1e6

    def handle():
        st = s.stop
        a = st.get("cs_ip")
        r = regs()
        if a in exits:
            spec, ev0 = exits.pop(a)
            s.client.remove_breakpoints(a)
            ev = {"t_ms": now_ms(), "kind": "exit", "label": spec["label"], "entry_t_ms": ev0["t_ms"],
                  "dur_ms": now_ms() - ev0["t_ms"], "ax": f"{r['ax']:04X}", "dx": f"{r['dx']:04X}",
                  "tick": struct.unpack("<I", rd(A(DS, 0x145E), 4))[0]}
            if spec["label"].startswith(("slot_alloc", "drv_install")):
                ev["table"] = table()
            log["events"].append(ev)
            out("exit", round(ev["t_ms"], 3), spec["label"], ev["ax"], round(ev["dur_ms"], 4))
            return
        spec = B.get(a)
        if spec is None:
            log["events"].append({"t_ms": now_ms(), "kind": "other", "stop": st})
            out("other", st)
            raise RuntimeError("unexpected stop")
        lab = spec["label"]
        log["hits"][lab] = log["hits"].get(lab, 0) + 1
        w = stackw(r, spec["n"])
        ev = {"t_ms": now_ms(), "kind": "hit", "label": lab, "stack": [f"{x:04X}" for x in w],
              "ret": f"{static(w[1]) or 'run ' + format(w[1], '04X')}:{w[0]:04X}",
              "regs": {k: f"{r[k]:04X}" for k in ("ax", "bx", "cx", "dx", "si", "di", "es", "ds")}}
        if spec["extra"] == "cfg":
            off, sg = struct.unpack("<HH", rd(A(DS, 0x3411), 4))
            ev["cfg_ptr"] = f"{sg:04X}:{off:04X}"
            ev["cfg"] = rd(f"{sg:04X}:{off:04X}", 0x3B).hex()
        if spec["extra"] in ("cfg", "flags"):
            ev["DS:3444"] = struct.unpack("<H", rd(A(DS, 0x3444), 2))[0]
            ev["DS:14E3"] = rd(A(DS, 0x14E3), 1)[0]
            ev["DS:13F7"] = rd(A(DS, 0x13F7), 1)[0]
            ev["DS:3417"] = rd(A(DS, 0x3417), 1)[0]
            ev["DS:343E"] = struct.unpack("<H", rd(A(DS, 0x343E), 2))[0]
        if spec["extra"] == "desc":
            d = rd(f"{r['es']:04X}:{r['di']:04X}", 0x18)
            ev["descriptor"] = f"{r['es']:04X}:{r['di']:04X}"
            ev["desc_bytes"] = d.hex()
            ev["desc_word14"] = struct.unpack_from("<H", d, 0x14)[0]
        if lab.startswith(("slot_", "set_", "drv_")):
            ev["table"] = table()
        if lab.startswith("wait_ms"):
            ev["ms"] = w[2]
            ev["tick"] = struct.unpack("<I", rd(A(DS, 0x145E), 4))[0]
            ev["current_us"] = struct.unpack("<I", rd(A(TL, 0x116), 4))[0]
            ev["slots"] = table()["slots"]
        log["events"].append(ev)
        out("hit", round(ev["t_ms"], 3), lab, ev["ret"], ev["stack"][2:])
        if spec["exit"]:
            ra = f"{w[1]:04X}:{w[0]:04X}"
            exits[ra] = (spec, ev)
            s.client.set_breakpoint(ra, once=False)
        if log["hits"][lab] >= spec["cap"]:
            s.client.remove_breakpoints(a)
            B.pop(a)

    for a in B:
        s.client.set_breakpoint(a, once=False)
    t0[0] = emu_ns()
    log["t0_emu_ns"] = t0[0]
    keys = sorted(cfg.get("keys", []))
    every = cfg.get("table_every_ms", 2000)
    next_table = 0
    marks = sorted(set([k[0] for k in keys] + list(range(0, cfg["duration_ms"] + 1, every))))
    for m in marks:
        while now_ms() < m:
            nstops[0] += 1
            if nstops[0] > cfg.get("max_stops", 4000):
                raise RuntimeError("too many stops")
            ob, exhausted = wait_for_stop(s, s.continue_(until_ms=int((t0[0] / 1e6) + m) + 1))
            if exhausted:
                log["events"].append({"t_ms": now_ms(), "kind": "exhausted"})
                save()
                raise SystemExit("exhausted")
            if ob.stop.get("reason") == "time":
                break
            handle()
            save()
        if m % every == 0:
            tb = table()
            log["events"].append({"t_ms": now_ms(), "kind": "table", **tb})
            out("table", round(now_ms()), tb["current_us"], [(x["slot"], x["state"], x["cb"], x["period_us"]) for x in tb["slots"]], tb["tick"])
        for k in keys:
            if k[0] == m:
                op = s.send_input("key", {"key": k[1]})
                log["events"].append({"t_ms": now_ms(), "kind": "input", "key": k[1]})
        save()
    if cfg.get("shot"):
        (run / "final.png").write_bytes(s.client.screenshot())
    log["end_ms"] = now_ms()
    save()
print("done", log["hits"])
