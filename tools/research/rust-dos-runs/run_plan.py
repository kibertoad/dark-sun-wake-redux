"""Agent run of DSUN.EXE under rust-dos, deterministic and muted, driven by a plan.

usage: run2.py <run-dir> <plan.json>
plan: {"start": null | "<earlier run dir>:<state>", "steps": [{"keys": [...], "until_ms": N,
       "shot": "name", "save": "state-name"}, ...]}
"""
import hashlib, json, os, shutil, struct, sys
from pathlib import Path
from dinorefurb_dosbox_session import (FieldContract, Media, RustDosConfig, RustDosSession, RustDosSettings, Target, WritableField,
                                       RUST_DOS_PINNED_SHA256, read_saved_state)

game = Path(os.environ["GAME_DIR"])  # the installed BLD-GOG-EN-1.1
run = Path(sys.argv[1])
plan = json.loads(Path(sys.argv[2]).read_text())
skip = {"DOSBOX", "analysis", "capture", "game.gog", "game.ins", "cloud_saves", "__support"}

def prepare(c: Path) -> None:
    for p in game.iterdir():
        if p.name in skip or (p.suffix.lower() in (".pdf", ".lnk", ".txt", ".conf", ".ico", ".dll", ".exe",
                                                   ".zip", ".hashdb", ".info", ".script", ".dat", ".msg", ".ini")
                              and p.name.upper() not in ("DSUN.EXE", "SOUND.INI", "DJ.DAT")):
            continue
        if p.is_dir():
            shutil.copytree(p, c / p.name)
        else:
            shutil.copy2(p, c / p.name)
    cfg = bytearray((c / "SOUND.CFG").read_bytes())
    # The sound setup's file names IRQ 5 for the music and the digital driver (words at 2 and 12);
    # the session's rust-dos has its Sound Blaster at IRQ 7 and takes no configuration.
    assert cfg[2:4] == bytes([5, 0]) and cfg[12:14] == bytes([5, 0]), bytes(cfg[:16]).hex()
    cfg[2:4] = cfg[12:14] = bytes([7, 0])
    (c / "SOUND.CFG").write_bytes(bytes(cfg))

config = RustDosConfig(media=(Media("D", game / "game.ins", "iso"),), deterministic=True, cycles="15000")
start = None
if plan.get("start"):
    d, name = plan["start"].rsplit(":", 1)
    start = read_saved_state(Path(d), name)
settings = RustDosSettings(
    emulator=Path("artifacts/rust-dos/rust-dos.exe"),
    emulator_sha256=RUST_DOS_PINNED_SHA256,
    run_directory=run,
    target=Target("DSUN.EXE", ("-W0", "-L")),
    prepare_drive=None if start else prepare,
    emulator_config=config,
    readiness_timeout=60.0,
    start_state=start,
)

def wait(s, op):
    ob = s.observe(op, timeout=600)
    while ob.pending:
        ob = s.observe(op, timeout=600)
    return ob

# Recording draws (the protocol's Recorded runs). The run loads the image's segment S at S - 0A46.
LOAD = 0x0A46
GENERATOR = "05BA:0822"           # rng, 1000:0822 (FND-RNG-001)
RNG_STATE = "4D9A:384A"           # rng_state, 57E0:384A
RANDOM_MOD_RETURN = 0x2834 * 16 + 0x0633   # inside random_mod, 2834:061C (FND-RNG-003)
SCRIPT_RAND_RETURN = 0x172C * 16 + 0x1710  # inside instruction 0x52, 172C:170B (FND-SCRIPT-011)
SCRIPT_RAND_PARAMETER = "0CE6:1719"        # 172C:1719, the parameter n in DX:AX


SCRIPT_ACCUMULATOR = "41CD:031B"         # script_accumulator, 4C13:031B
SCRIPT_RAND_STORED = "0CE6:1741"           # 172C:1741, after the store to script_accumulator
RANDOM_MOD_DIVIDED = "1DEE:0636"          # 2834:0636, the remainder in DX
CHANCE_IN_TEN = "2834:0519"               # chance_in_ten draws through random_mod (FND-RNG-006)


def linear(addr):
    seg, off = addr.split(":")
    return int(seg, 16) * 16 + int(off, 16)


# The routine that holds a code address, from the repository's DSUN.EXE function inventory.
ROUTINES = []
with open(Path(__file__).resolve().parents[3] / "coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv") as inventory:
    next(inventory)
    for row in inventory:
        start, _, ranges = row.rstrip("\n").split("\t")
        if ":" not in start:
            continue  # overlay code, given by file offset; the probe needs only resident routines
        for span in ranges.split():
            a, b = span.split("..")
            ROUTINES.append((linear(a), linear(b), start))


def routine(addr):
    at = linear(addr)
    found = [start for a, b, start in ROUTINES if a <= at < b]
    return found[0] if len(found) == 1 else None


class Unrecorded(Exception):
    pass


def static(seg, off):
    return f"{seg + LOAD:04X}:{off:04X}"


def word(s, addr):
    return struct.unpack("<H", s.client.read_memory(addr, 2).data)[0]


def record_draw(s, ob, until_ms):
    """Records the draw the generator is about to make, and returns the stop to go on from."""
    r = ob.stop["registers"]
    off, seg = struct.unpack("<HH", s.client.read_memory(f"{r['ss']}:{r['sp']}", 4).data)
    state = struct.unpack("<I", s.client.read_memory(RNG_STATE, 4).data)[0]
    after = (state * 0x015A4E35 + 1) & 0xFFFFFFFF
    d = (after >> 16) & 0x7FFF
    at = (seg + LOAD) * 16 + off
    if at == RANDOM_MOD_RETURN:
        bp = int(r["bp"], 16)
        n = word(s, f"{r['ss']}:{bp + 6:04X}")
        coff, cseg = struct.unpack("<HH", s.client.read_memory(f"{r['ss']}:{bp + 2:04X}", 4).data)
        caller = static(cseg, coff)
        if routine(caller) == CHANCE_IN_TEN:
            # The rule is that of chance_in_ten's caller, one frame further up.
            cbp = word(s, f"{r['ss']}:{bp:04X}")
            coff, cseg = struct.unpack("<HH", s.client.read_memory(f"{r['ss']}:{cbp + 2:04X}", 4).data)
            caller = static(cseg, coff)
        rule = plan.get("rules", {}).get(routine(caller))
        if rule is None:
            raise Unrecorded(f"random_mod({n}) from {caller} in {routine(caller)}, which no rule in the plan names")
        n_s = n - 0x10000 if n & 0x8000 else n
        draw = {"rule": rule, "bound": n_s, "result": int(d - int(d / n_s) * n_s)}
        s.client.set_breakpoint(RANDOM_MOD_DIVIDED, once=True)
        ob = wait(s, s.continue_(until_ms=until_ms))
        if ob.stop.get("reason") != "breakpoint" or ob.stop["cs_ip"] != RANDOM_MOD_DIVIDED:
            raise Unrecorded(f"random_mod did not reach {RANDOM_MOD_DIVIDED}: {ob.stop}")
        remainder = int(ob.stop["registers"]["dx"], 16)
        if remainder != draw["result"] & 0xFFFF:
            raise Unrecorded(f"random_mod left {remainder}, the reading gives {draw['result']}")
        ob = wait(s, s.continue_(until_ms=until_ms))
    elif at == SCRIPT_RAND_RETURN:
        s.client.set_breakpoint(SCRIPT_RAND_PARAMETER, once=True)
        ob = wait(s, s.continue_(until_ms=until_ms))
        if ob.stop.get("reason") != "breakpoint" or ob.stop["cs_ip"] != SCRIPT_RAND_PARAMETER:
            raise Unrecorded(f"instruction 0x52 did not reach {SCRIPT_RAND_PARAMETER}: {ob.stop}")
        q = ob.stop["registers"]
        n = (int(q["dx"], 16) << 16) | int(q["ax"], 16)
        n = n - (1 << 32) if n & 0x80000000 else n
        product = d * (n + 1)
        draw = {"rule": "RULE-SCRIPT-006", "bound": n, "result": int(product / 0x8000)}
        s.client.set_breakpoint(SCRIPT_RAND_STORED, once=True)
        ob = wait(s, s.continue_(until_ms=until_ms))
        if ob.stop.get("reason") != "breakpoint" or ob.stop["cs_ip"] != SCRIPT_RAND_STORED:
            raise Unrecorded(f"instruction 0x52 did not reach {SCRIPT_RAND_STORED}: {ob.stop}")
        stored = struct.unpack("<i", s.client.read_memory(SCRIPT_ACCUMULATOR, 4).data)[0]
        if stored != draw["result"]:
            raise Unrecorded(f"instruction 0x52 stored {stored}, the reading gives {draw['result']}")
        ob = wait(s, s.continue_(until_ms=until_ms))
    else:
        raise Unrecorded(f"a draw returning to {static(seg, off)}, which the probe does not know")
    return draw, ob


draws = []
with RustDosSession(settings) as s:
    if "rng_state" in plan:
        # The seed of this run: a guarded write of rng_state (glossary rng_state, FND-RNG-001).
        old = s.client.read_memory(RNG_STATE, 4).data
        contract = FieldContract("dark-sun-wake-runs/1", (WritableField("rng_state", RNG_STATE, 4),))
        s.write(contract, "rng_state", struct.pack("<I", plan["rng_state"]), hashlib.sha256(old).hexdigest())
    if plan.get("watch_draws"):
        s.client.set_breakpoint(GENERATOR)
        print("rng_state", struct.unpack("<I", s.client.read_memory(RNG_STATE, 4).data)[0])
    for k, step in enumerate(plan["steps"]):
        # A deterministic machine takes input only while paused and delivers it once resumed, so
        # queue the input, run to the step's time, and only then see each input delivered.
        inputs = [s.send_input("key", {"key": key}) for key in step.get("keys", [])]
        if "type" in step:
            inputs.append(s.send_input("type", {"text": step["type"]}))
        if "mouse" in step:
            inputs.append(s.send_input("mouse", step["mouse"]))
        if "until_ms" in step:
            ob = wait(s, s.continue_(until_ms=step["until_ms"]))
            while ob.stop.get("reason") == "breakpoint" and ob.stop["cs_ip"] == GENERATOR:
                draw, ob = record_draw(s, ob, step["until_ms"])
                draws.append(draw)
                print("  draw", draw, flush=True)
            print(k, step["until_ms"], ob.stop.get("reason"), ob.stop.get("cs_ip"), flush=True)
        for op in inputs:
            ob = s.observe(op, timeout=5)
            if ob.pending:
                raise SystemExit(f"input of step {k} not delivered by {step.get('until_ms')}")
        if step.get("status"):
            print("  mouse", s.client.status().get("mouse"))
        for addr, n in step.get("read", []):
            print("  mem", addr, s.client.read_memory(addr, n).data.hex())
        if "shot" in step:
            (run / f"{step['shot']}.png").write_bytes(s.client.screenshot())
        if "save" in step:
            st = s.save_state(step["save"], check_files=True)
            print("  saved", step["save"], st.checkpoint)
    if plan.get("watch_draws"):
        print("draws", json.dumps(draws))
