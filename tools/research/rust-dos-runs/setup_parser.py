"""Fixed SOUND_DS.EXE parser capability attempt; Q-CONFIG-006/011, Q-SOUND-010.

Only two fingerprinted licensed files enter a private session drive. No
arguments, input, guest writes, screenshots, driver use or gameplay is admitted.
Reports are original-derived and remain beneath the approved GAME_DIR.
"""
import json
import os
from pathlib import Path
import re
import shutil
import struct
import sys

import xxhash
from dinorefurb_dosbox_session import (
    RUST_DOS_PINNED_SHA256, RustDosConfig, RustDosSession, RustDosSettings,
    SoundBlaster, Target,
)

ROOT = Path(__file__).resolve().parents[3]
INPUTS = {"SOUND_DS.EXE": (204593, "236c2dc23c071eca421eb5b427caee57"),
          "SOUND.INI": (54669, "aecc7469098e0c188e0e3d056c5fd530")}
# FND-SOUND-024/033 and FND-EXE-491; no arbitrary address or target input.
PHASES = {"parser": (0x1AF6, 0x0001), "returned": (0x1ABD, 0x0021)}
GUARDS = ((0x1AF6, 0x0040), (0x1AF6, 0x0090), (0x1AF6, 0x00C1),
          (0x1AF6, 0x00F0), (0x1AF6, 0x011F),
          (0x1000, 0x4127), (0x1C08, 0x0A25))


def address(loaded, segment, offset):
    if any(type(value) is not int for value in (loaded, segment, offset)):
        raise ValueError("invalid setup address type")
    actual = loaded + segment - 0x1000
    if not 0 <= loaded <= 0xFFFF or not 0 <= actual <= 0xFFFF or not 0 <= offset <= 0xFFFF:
        raise ValueError("setup relocation outside real mode")
    if actual * 16 + offset >= 0x100000:
        raise ValueError("setup address beyond real-mode memory")
    return f"{actual:04X}:{offset:04X}"


def checked_read(client, segment, offset, size):
    if any(type(value) is not int for value in (segment, offset, size)):
        raise ValueError("invalid setup read type")
    if not 0 <= segment <= 0xFFFF or not 0 <= offset < 0x10000 or not 1 <= size <= 16:
        raise ValueError("unsupported setup read")
    if offset + size > 0x10000 or segment * 16 + offset + size > 0x100000:
        raise ValueError("setup read wraps")
    data = client.read_memory(f"{segment:04X}:{offset:04X}", size).data
    if len(data) != size:
        raise ValueError("short setup field read")
    return data


def observe_fields(client, stop, loaded, phase):
    if phase not in PHASES:
        raise ValueError("unsupported setup phase")
    expected = address(loaded, *PHASES[phase])
    ds = loaded + 0x1E36 - 0x1000
    if (stop.get("reason") != "breakpoint" or stop.get("cs_ip") != expected
            or int(stop.get("registers", {}).get("ds", "-1"), 16) != ds):
        raise ValueError("setup observation needs declared stop and DS")
    # Validate the whole named span before issuing any read.
    address(loaded, 0x1E36, 0xECCD)
    registers = stop["registers"]
    ax = int(registers.get("ax", "-1"), 16)
    if not 0 <= ax <= 0xFFFF:
        raise ValueError("setup observation needs a word AX")
    frame = None
    if phase == "parser":
        ss, sp = (int(registers.get(name, "-1"), 16) for name in ("ss", "sp"))
        ip, cs = struct.unpack("<HH", checked_read(client, ss, sp, 4))
        if f"{cs:04X}:{ip:04X}" != address(loaded, *PHASES["returned"]):
            raise ValueError("setup parser has a different far caller")
        frame = {"return_cs": cs, "return_ip": ip}
    counts = struct.unpack("<hhh", checked_read(client, ds, 0xEC74, 6))
    pointers = struct.unpack("<6H", checked_read(client, ds, 0x188C, 12))
    return {"phase": phase, "incoming_frame": frame,
            "counts": dict(zip(("main", "digital", "music_only"), counts)),
            "groups": {name: {"offset": pointers[i], "segment": pointers[i + 1]}
                       for name, i in (("main", 0), ("music_only", 2), ("digital", 4))},
            "stream": dict(zip(("offset", "segment"),
                               struct.unpack("<HH", checked_read(client, ds, 0x1888, 4)))),
            "mode": struct.unpack("<H", checked_read(client, ds, 0xECCC, 2))[0],
            "ax": ax}


def wait_same_operation(session, operation, attempts=6):
    for _ in range(attempts):
        observed = session.observe(operation, timeout=30)
        if not observed.pending:
            return observed
        print("pending setup operation", operation.op, flush=True)
    session.pause()
    for _ in range(3):
        observed = session.observe(operation, timeout=30)
        if not observed.pending:
            raise RuntimeError("setup observation budget ended at a paused stop")
    raise RuntimeError("setup continuation still pending after pause; closing owned session")


def verified_input(path, identity):
    size, digest = identity
    with path.open("rb") as stream:
        body = stream.read(size + 1)
    if (len(body), xxhash.xxh3_128_hexdigest(body)) != identity:
        raise ValueError("setup input differs from the fixed licensed identity")


def main():
    if len(sys.argv) != 2 or not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", sys.argv[1]):
        raise SystemExit("usage: setup_parser.py <new-run-name>")
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                              Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    for name, identity in INPUTS.items():
        verified_input(game / name, identity)
    run = game / "captures/rust-dos-setup-parser" / sys.argv[1]
    if run.exists():
        raise ValueError("setup run directory exists; choose a new name")

    def prepare(destination):
        for name, identity in INPUTS.items():
            shutil.copy2(game / name, destination / name)
            verified_input(destination / name, identity)

    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe",
        emulator_sha256=RUST_DOS_PINNED_SHA256, run_directory=run,
        target=Target("SOUND_DS.EXE", ()), prepare_drive=prepare,
        emulator_config=RustDosConfig(deterministic=True, cycles="15000",
                                     sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1),
                                     ultrasound=None), readiness_timeout=60.0,
    )
    report = {"target": "SOUND_DS.EXE", "inputs": INPUTS, "phases": [],
              "limits": "fixed utility parser capability only; no gameplay, input, guest writes, frames, drivers or rule/RNG replay claim"}
    with RustDosSession(settings) as session:
        entry = session.entry
        report["entry"] = entry
        if entry.get("cs_ip", "").split(":")[-1] != "0000":
            raise ValueError("unexpected setup entry offset")
        loaded = int(entry["registers"]["cs"], 16)
        for segment, offset in GUARDS:
            session.client.set_breakpoint(address(loaded, segment, offset))
        for phase in PHASES:
            operation = session.continue_(until=address(loaded, *PHASES[phase]))
            observed = wait_same_operation(session, operation)
            report["last_stop"] = observed.stop
            (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
            fields = observe_fields(session.client, observed.stop, loaded, phase)
            report["phases"].append(fields)
            (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
            print(json.dumps(fields), flush=True)
        print("setup parser boundary observed; closing owned session", flush=True)


if __name__ == "__main__":
    main()
