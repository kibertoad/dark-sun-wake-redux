"""Fixed native startup seed observation and start-window call-site checkpoint."""
import json
import os
from pathlib import Path
import re
import struct
import sys

from dinorefurb_dosbox_session import (
    Media, RUST_DOS_PINNED_SHA256, RustDosConfig, RustDosSession,
    RustDosSettings, SoundBlaster, Target,
)

sys.path.insert(0, str(Path(__file__).resolve().parent))
from probe_drive import prepare_drive, ROOT


def seed_caller(frame, trampoline, call, loaded):
    if len(frame) != 6 or len(trampoline) != 5 or len(call) != 5:
        raise ValueError("incomplete seed caller observation")
    offset, segment, argument = struct.unpack("<HHH", frame)
    opcode, target, overlay_segment = struct.unpack("<BHH", trampoline)
    if opcode != 0xEA or target != 0x0141 or segment != overlay_segment or offset != 0x0663:
        raise ValueError("seed caller is not the declared startup overlay return")
    if call != struct.pack("<BHH", 0x9A, 0x0811, loaded):
        raise ValueError("startup caller does not call the relocated seed setter")
    return f"{segment:04X}:{offset:04X}", argument


def checkpoint_summary(checkpoint):
    return {"name": checkpoint.name, "checked": checkpoint.checkpoint,
            "directory": str(checkpoint.directory)}


def wait_for_stop(session, operation, attempts=10):
    for attempt in range(attempts):
        observed = session.observe(operation, timeout=30)
        if not observed.pending:
            return observed, False
        print("pending", operation.op, attempt + 1, flush=True)
    session.pause()
    # pause returns a reply mapping; observe the interrupted continuation.
    while True:
        observed = session.observe(operation, timeout=30)
        if not observed.pending:
            return observed, True


def main():
    if len(sys.argv) != 2 or not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", sys.argv[1]):
        raise SystemExit("usage: startup_window.py <new-run-name>")
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                              Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    run = game / "captures/rust-dos-startup-window" / sys.argv[1]
    if run.exists():
        raise SystemExit("run directory exists; choose a new name")
    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe",
        emulator_sha256=RUST_DOS_PINNED_SHA256, run_directory=run,
        target=Target("DSUN.EXE", ("-W0", "-L")),
        prepare_drive=lambda destination: prepare_drive(game, destination),
        emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
                                     deterministic=True, cycles="15000",
                                     sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1),
                                     ultrasound=None), readiness_timeout=60.0,
    )
    with RustDosSession(settings) as session:
        entry = session.entry
        if entry["cs_ip"].split(":")[1].upper() != "0000":
            raise RuntimeError("unexpected DSUN entry offset")
        loaded = int(entry["registers"]["cs"], 16)

        def address(segment, offset):
            actual = segment - 0x1000 + loaded
            if not 0 <= actual <= 0xFFFF:
                raise RuntimeError("translated segment outside real mode")
            return f"{actual:04X}:{offset:04X}"

        seed_address = address(0x57E0, 0x384A)
        def seed():
            return struct.unpack("<I", session.client.read_memory(seed_address, 4).data)[0]

        destination = address(0x277B, 0x02E4)
        guard = address(0x1000, 0x0822)
        report = {"entry": entry, "initial_rng_state": seed(), "draws": [],
                  "destination": destination, "generator_guard": guard,
                  "target": "DSUN.EXE -W0 -L", "inputs": [], "writes": []}
        session.client.set_breakpoint(guard)
        setter = address(0x1000, 0x0811)
        session.client.set_breakpoint(setter)

        def save_report():
            (run / "startup-window.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")

        def reach(target, stage):
            operation = session.continue_(until=target)
            observed, exhausted = wait_for_stop(session, operation)
            report[stage] = observed.stop
            if exhausted:
                report["limit"] = "wall-clock observation limit; no destination claim"
            save_report()
            if observed.stop.get("reason") != "breakpoint" or observed.stop.get("cs_ip") != target:
                raise RuntimeError(f"{stage} not reached; stop is not admitted for continuation")
            return observed.stop

        stopped = reach(setter, "seed_entry")
        registers = stopped["registers"]
        sp = int(registers["sp"], 16)
        if sp > 0xFFFA:
            raise RuntimeError("seed frame would wrap the stack offset")
        if registers["ds"] != seed_address.split(":")[0]:
            raise RuntimeError("seed setter DS does not name the supported RNG state")
        frame = session.client.read_memory(f"{registers['ss']}:{sp:04X}", 6).data
        return_offset, return_segment = struct.unpack("<HH", frame[:4])
        if return_offset != 0x0663:
            raise RuntimeError("unexpected seed return offset")
        trampoline = session.client.read_memory(address(0x56B2, 0x0020), 5).data
        call = session.client.read_memory(f"{return_segment:04X}:{return_offset - 5:04X}", 5).data
        return_address, argument = seed_caller(frame, trampoline, call, loaded)
        report["seed_argument"] = argument
        report["seed_before"] = seed()
        report["seed_caller"] = {"overlay": 180, "return": return_address,
                                 "validated_trampoline_and_call": True}
        save_report()
        reach(return_address, "seed_return")
        report["seed_after"] = seed()
        save_report()
        if report["seed_after"] != argument:
            raise RuntimeError("seed setter result differs from its word argument")
        operation = session.continue_(until=destination)
        observed, exhausted = wait_for_stop(session, operation)
        if exhausted:
            report["limit"] = "wall-clock observation limit; no destination claim"
        report["stop"] = observed.stop
        report["final_rng_state"] = seed()
        reached = (observed.stop.get("reason") == "breakpoint"
                   and observed.stop.get("cs_ip") == destination)
        if reached:
            registers = observed.stop["registers"]
            report["opener_argument_word"] = struct.unpack(
                "<H", session.client.read_memory(f"{registers['ss']}:{registers['sp']}", 2).data)[0]
            if report["final_rng_state"] != report["seed_after"]:
                raise RuntimeError("RNG state changed after the observed seed call")
            checkpoint = session.save_state("opener", check_files=True)
            report["checkpoint"] = checkpoint_summary(checkpoint)
        save_report()
        print(json.dumps({"reached": reached, "stop": observed.stop.get("cs_ip"),
                          "reason": observed.stop.get("reason"),
                          "initial_seed": report["initial_rng_state"],
                          "final_seed": report["final_rng_state"],
                          "argument": report.get("opener_argument_word")}), flush=True)
        if not reached:
            raise RuntimeError("start-window call not reached; stop is not admitted for continuation")


if __name__ == "__main__":
    main()
