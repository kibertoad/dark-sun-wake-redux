"""Walk-run driver, derived from input_watch.py and pass_counts.py.

usage (repository root, evidence Python, -I):
  walk_drive.py <config.json>   (configs in counts/walkruns-*.json)
config:
  name, duration (ms after restore), start: "regen:<run>:<state>" | "<run dir>:<state>",
  watch: [[run addr, n], ...]                      change watchpoints, every change logged
  breaks: [{addr, label, stack: nwords, reads: [[addr, n]], cap, from_ms,
            match: [word index, hex value] (count only hits whose stack word matches),
            exit: bool (once breakpoint at the far return, with the same reads)}]
  events: [{at_ms, key, ...extra key fields} | {at_ms, type} | {at_ms, move: [x, y]} |
           {at_ms, click: [x, y, button]} | {at_ms, shot} | {at_ms, read: [[addr, n]]} |
           {at_ms, save} | {at_ms, trace_ms, lin: [start, len]}]
Times are emulated ms after the restore. Output: D:/rust-dos-games/work/walkruns/<name>/log.json.
Nothing is written to game memory.
"""
import hashlib, json, re, struct, sys, time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "tools/research/rust-dos-runs"))
import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_MEM
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
start = cfg["start"]
if start.startswith("regen:"):
    _, rn, nm = start.split(":")
    source = game / "captures/rust-dos-regenerated" / rn
    meta = json.loads((source / "regeneration.json").read_text(encoding="utf-8"))
    delta = mapping(meta.get("load_mapping_stop", meta["restored_stop"]))
else:
    d, nm = start.rsplit(":", 1)
    source = Path(d)
    delta = (0x05BA - 0x1000) & 0xFFFF
assert delta & 0xFFFF == (0x05BA - 0x1000) & 0xFFFF
state = read_saved_state(source, nm)
verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
admit_state(state)
run = Path("D:/rust-dos-games/work/walkruns") / cfg["name"]
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


log = {"config": cfg, "events": [], "hits": {}, "skipped": {}}


def save():
    (run / "log.json").write_text(json.dumps(log, indent=1) + "\n", encoding="utf-8")


def out(*a):
    print(*a, flush=True)


# --- trace access scan ---------------------------------------------------------------
md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True
dcache = {}
STR = re.compile(r"^(rep\w* )?(lods|movs|cmps|scas|stos|ins|outs)([bwd])")


def decode(hexbytes):
    r = dcache.get(hexbytes)
    if r is not None:
        return r
    b = bytes.fromhex(hexbytes.replace(" ", ""))
    ins = next(md.disasm(b, 0, 1), None)
    if ins is None:
        r = ("none", ())
    else:
        text = f"{ins.mnemonic} {ins.op_str}"
        m = STR.match(ins.mnemonic + " ") or STR.match(ins.mnemonic)
        segov = None
        for p in ins.prefix:
            segov = {0x26: "es", 0x2E: "cs", 0x36: "ss", 0x3E: "ds", 0x64: "fs", 0x65: "gs"}.get(p, segov)
        if m or re.match(r"^(rep\w* )?(lods|movs|cmps|scas|stos)", ins.mnemonic):
            mn = ins.mnemonic.split()[-1]
            size = {"b": 1, "w": 2, "d": 4}.get(mn[-1], 1)
            kind = mn[:4]
            rep = ins.mnemonic.startswith("rep") or any(p in (0xF2, 0xF3) for p in ins.prefix)
            r = ("str", (kind, size, rep, segov or "ds"), text)
        else:
            ops = []
            for op in ins.operands:
                if op.type == X86_OP_MEM:
                    base = ins.reg_name(op.mem.base) if op.mem.base else None
                    idx = ins.reg_name(op.mem.index) if op.mem.index else None
                    seg = ins.reg_name(op.mem.segment) if op.mem.segment else ("ss" if base in ("bp", "ebp") else "ds")
                    ops.append((seg, base, idx, op.mem.disp, op.size, op.access))
            if ins.mnemonic == "xlatb":
                ops.append((segov or "ds", "bx", "al", 0, 1, 1))
            r = ("mem", tuple(ops), text)
    dcache[hexbytes] = r
    return r


def reg(e, name):
    if name == "al":
        return int(e["ax"], 16) & 0xFF
    return int(e[name[-2:]], 16) if len(name) == 3 else int(e[name], 16)


def scan_entry(e, lo, hi, found):
    if e.get("code32"):
        return
    asm = e.get("asm", "")
    if asm.startswith("HLE INT 21h"):
        m = re.search(r"AX=([0-9A-F]{4})", asm)
        if m and int(m.group(1), 16) >> 8 in (0x3F, 0x40):
            a = (int(e["ds"], 16) << 4) + int(e["dx"], 16)
            if a < hi and a + int(e["cx"], 16) > lo:
                found.append({"icount": e["icount"], "at": f"{e['cs']}:{e['ip']}", "asm": asm, "kind": "dos-io"})
        return
    if asm.startswith("HLE"):
        return
    d = decode(e["bytes"])
    if d[0] == "mem":
        for seg, base, idx, disp, size, acc in d[1]:
            off = disp
            if base:
                off += reg(e, base)
            if idx:
                off += reg(e, idx)
            a = (int(e[seg], 16) << 4) + (off & 0xFFFF)
            if a < hi and a + max(size, 1) > lo:
                found.append({"icount": e["icount"], "at": f"{e['cs']}:{e['ip']}", "asm": d[2],
                              "access": acc, "seg": e[seg], "off": f"{off & 0xFFFF:04X}", "kind": "mem"})
    elif d[0] == "str":
        kind, size, rep, segov = d[1]
        n = int(e["cx"], 16) if rep else 1
        down = int(e["flags"], 16) & 0x400
        spans = []
        if kind in ("lods", "movs", "cmps", "outs"):
            spans.append(("src", (int(e[segov], 16) << 4), int(e["si"], 16)))
        if kind in ("stos", "movs", "cmps", "scas", "ins"):
            spans.append(("dst", (int(e["es"], 16) << 4), int(e["di"], 16)))
        for role, base, off in spans:
            if n == 0:
                continue
            first = off - (n - 1) * size if down else off
            a, b = base + (first & 0xFFFF), base + (first & 0xFFFF) + n * size
            if a < hi and b > lo:
                found.append({"icount": e["icount"], "at": f"{e['cs']}:{e['ip']}", "asm": d[2], "role": role,
                              "count": n, "kind": "string"})


with RustDosSession(settings) as s:
    assert s.stop["reason"] == "load"
    start_ms = int(s.stop["emu_ns"]) // 1000000
    log["restore_ms"] = start_ms
    log["restore_stop"] = s.stop["cs_ip"]
    log["version"] = s.client.version()
    rd = lambda a, n: s.client.read_memory(a, n).data

    def now():
        return int(s.stop["emu_ns"]) // 1000000 - start_ms

    def now_ns():
        return int(s.stop["emu_ns"]) - start_ms * 1000000

    watched = {}
    for addr, n in cfg.get("watch", []):
        s.client.set_watchpoint(addr, n)
        watched[addr] = (n, rd(addr, n))
    log["watch_initial"] = {a: v[1].hex() for a, v in watched.items()}
    out("initial", log["watch_initial"])
    bspecs = {b["addr"]: b for b in cfg.get("breaks", [])}
    armed = set()
    exits = {}   # run addr of a pending exit breakpoint -> (spec, entry event)
    nstops = [0]

    def arm(t):
        for a, b in bspecs.items():
            if a not in armed and t >= b.get("from_ms", 0) and log["hits"].get(b["label"], 0) < b.get("cap", 200):
                s.client.set_breakpoint(a, once=False)
                armed.add(a)

    def regs_of(stop):
        return {k.lower(): (int(v, 16) if isinstance(v, str) else v) for k, v in stop["registers"].items()}

    def handle(stop):
        t = now()
        regs = regs_of(stop)
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
            ev = {"t": t, "ns": now_ns(), "kind": "write", "changed": changed, "run": stop["cs_ip"], "static": where}
            for a, n in cfg.get("watch_reads", []):
                ev[a] = rd(a, n).hex()
            log["events"].append(ev)
            return True
        a = stop["cs_ip"]
        if stop.get("reason") == "breakpoint" and a in exits:
            spec, entry = exits.pop(a)
            s.client.remove_breakpoints(a)
            ev = {"t": t, "ns": now_ns(), "kind": "exit", "label": spec["label"], "run": a,
                  "entry_ns": entry["ns"], "regs": {k: f"{regs[k]:04X}" for k in ("ax", "dx", "sp")}}
            for ra, n in spec.get("reads", []):
                ev[ra] = rd(ra, n).hex()
            log["events"].append(ev)
            out("exit", t, spec["label"], a)
            return True
        if stop.get("reason") == "breakpoint" and a in bspecs:
            spec = bspecs[a]
            lab = spec["label"]
            nw = max(spec.get("stack", 6), 2)
            w = struct.unpack(f"<{nw}H", rd(f"{regs['ss']:04X}:{regs['sp']:04X}", 2 * nw))
            mt = spec.get("match")
            if mt and w[mt[0]] != int(mt[1], 16):
                log["skipped"][lab] = log["skipped"].get(lab, 0) + 1
                return True
            log["hits"][lab] = log["hits"].get(lab, 0) + 1
            rseg = w[1]
            ev = {"t": t, "ns": now_ns(), "kind": "hit", "label": lab, "run": a, "static": where,
                  "ret": f"{rseg:04X}:{w[0]:04X}", "ret_static": f"{static_of(rseg)}:{w[0]:04X}" if static_of(rseg) else None,
                  "stack": [f"{x:04X}" for x in w],
                  "regs": {k: f"{regs[k]:04X}" for k in ("ax", "bx", "cx", "dx", "si", "di", "bp", "ds", "es")}}
            for ra, n in spec.get("reads", []):
                ev[ra] = rd(ra, n).hex()
            if spec.get("chain"):
                ch, bp = [], regs["bp"]
                for _ in range(6):
                    if not bp:
                        break
                    nbp, off, seg = struct.unpack("<HHH", rd(f"{regs['ss']:04X}:{bp:04X}", 6))
                    ch.append(f"{static_of(seg)}:{off:04X}" if static_of(seg) else f"run {seg:04X}:{off:04X}")
                    bp = nbp
                ev["chain"] = ch
            if spec.get("code_before") and not static_of(rseg):
                ev["code_before"] = rd(f"{rseg:04X}:{(w[0] - 24) & 0xFFFF:04X}", 24).hex()
            log["events"].append(ev)
            if spec.get("exit"):
                ra = f"{rseg:04X}:{w[0]:04X}"
                exits[ra] = (spec, ev)
                s.client.set_breakpoint(ra, once=False)
            if log["hits"][lab] >= spec.get("cap", 200):
                s.client.remove_breakpoints(a)
                armed.discard(a)
                bspecs_done.add(a)
            out("hit", t, lab, ev["ret"], ev["stack"][2:5])
            return True
        log["events"].append({"t": t, "kind": "other", "stop": stop})
        out("other stop", t, json.dumps(stop)[:400])
        return False

    bspecs_done = set()
    ended = [False]
    tracing = {"on": False}

    def drain():
        tr = tracing
        while True:
            page = s.client.get("/api/trace", since=tr["cursor"], limit=60000, format="json")
            ents = page["entries"]
            tr["dropped"] += page.get("dropped", 0)
            for e in ents:
                scan_entry(e, tr["lo"], tr["hi"], tr["found"])
            if ents:
                tr["first"] = tr["first"] or (ents[0]["icount"], now())
                tr["last"] = (ents[-1]["icount"], now())
            tr["entries"] += len(ents)
            tr["cursor"] = page["next"]
            if not page.get("truncated"):
                break

    def trace_on(lo, ln):
        st = s.client.request("POST", "/api/trace", body={"count": 0}).body
        tracing.update({"on": True, "lo": lo, "hi": lo + ln, "found": [], "entries": 0, "dropped": 0,
                        "cursor": st.get("total_recorded", 0), "first": None, "last": None, "t0": now(), "w0": time.time()})

    def trace_off():
        s.client.request("POST", "/api/trace", body={"count": 0})
        drain()
        tr = tracing
        tr["on"] = False
        ev = {"t": now(), "kind": "trace", "from_t": tr["t0"], "lin": [f"{tr['lo']:X}", tr["hi"] - tr["lo"]],
              "entries": tr["entries"], "dropped": tr["dropped"], "first": tr["first"], "last": tr["last"],
              "nfound": len(tr["found"]), "found": tr["found"][:2000], "wall_s": round(time.time() - tr["w0"], 1)}
        log["events"].append(ev)
        out("trace", tr["t0"], now(), tr["entries"], "entries", len(tr["found"]), "found, dropped", tr["dropped"])

    def run_to(target_ms):
        while not ended[0]:
            for a in list(bspecs_done):
                bspecs.pop(a, None)
            bspecs_done.clear()
            arm(now())
            if now() >= target_ms:
                return
            if nstops[0] > cfg.get("max_stops", 3000):
                raise RuntimeError("too many stops")
            nxt = [b.get("from_ms", 0) for a, b in bspecs.items() if a not in armed and b.get("from_ms", 0) > now()]
            tgt = min([target_ms] + nxt)
            if tracing["on"]:
                tgt = min(tgt, now() + 40)
                s.client.request("POST", "/api/trace", body={"count": 900000})
            ob, exhausted = wait_for_stop(s, s.continue_(until_ms=start_ms + tgt))
            if tracing["on"]:
                drain()
            if exhausted:
                log["events"].append({"t": now(), "kind": "exhausted", "stop": ob.stop})
                out("exhausted", now(), json.dumps(ob.stop)[:300])
                ended[0] = True
                return
            if ob.stop.get("reason") == "time":
                continue
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

    def trace_window(ms, lo, ln):
        hi = lo + ln
        found, total, chunks, dropped = [], 0, 0, 0
        st = s.client.request("POST", "/api/trace", body={"enabled": False}).body
        cursor = st.get("total", None)
        t_end = now() + ms
        tw0 = time.time()
        while now() < t_end and not ended[0]:
            st = s.client.request("POST", "/api/trace", body={"count": 900000}).body
            if cursor is None:
                cursor = st.get("total", 0)
            run_to(min(t_end, now() + 50))
            while True:
                page = s.client.get("/api/trace", since=cursor, limit=60000, format="json")
                ents = page["entries"]
                dropped += page.get("dropped", 0)
                for e in ents:
                    scan_entry(e, lo, hi, found)
                total += len(ents)
                cursor = page["next"]
                if not page.get("truncated"):
                    break
            chunks += 1
            s.client.request("POST", "/api/trace", body={"count": 0})
        ev = {"t": now(), "kind": "trace", "ms": ms, "lin": [f"{lo:X}", ln], "entries": total, "chunks": chunks,
              "dropped": dropped, "found": found[:500], "nfound": len(found), "wall_s": round(time.time() - tw0, 1),
              "last_status": st}
        log["events"].append(ev)
        out("trace", now(), total, "entries", len(found), "found", "dropped", dropped)
        save()

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
            s.save_state(c["save"], check_files=True)
            log["events"].append({"t": t, "kind": "save_state", "name": c["save"]})
            out("saved", c["save"])
        if "trace_on" in c:
            trace_on(int(c["trace_on"][0], 16), c["trace_on"][1])
        if "trace_off" in c:
            trace_off()
        if "trace_ms" in c:
            lo, ln = c["lin"]
            trace_window(c["trace_ms"], int(lo, 16), ln)
            continue
        op = None
        if "key" in c:
            ev = {k: v for k, v in c.items() if k not in ("at_ms", "read", "shot", "save")}
            op = s.send_input("key", ev)
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
        save()
    if not ended[0]:
        run_to(cfg["duration"])
    log["end_ms"] = now()
    try:
        png = s.client.screenshot()
        (run / "final.png").write_bytes(png)
        log["final_md5"] = hashlib.md5(png).hexdigest()
        out("final shot", log["final_md5"])
        log["final_watch"] = {a: rd(a, n).hex() for a, (n, _) in watched.items()}
        log["final_reads"] = {a: rd(a, n).hex() for a, n in cfg.get("final_reads", [])}
        out("final", log["final_watch"], log["final_reads"])
    except Exception as e:
        log["final_error"] = repr(e)
        out("final error", repr(e))
    save()
print("done", log["end_ms"], log["hits"], log["skipped"])
