"""Fixed read-only startup observation for Q-CONFIG-014; no general runner API.

FND-SOUND-085/092/093 identify initializer gates and named library fields.
FND-SOUND-094/095 identify callback, request, allocation and table boundaries.
FND-RNG-001/010 identify the seed, generator and setter guards.
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
    RustDosSettings, SoundBlaster, Target,
)

sys.path.insert(0, str(Path(__file__).resolve().parent))
from probe_drive import prepare_drive, ROOT
from startup_window import wait_for_stop
from preferences_history import StartupHistory

# FND-SOUND-092/094/095: only these instruction boundaries are observable.
SITES = {
    (0x4734, 0x0133): "initializer-entry",
    (0x4734, 0x0185): "callback-gate",
    (0x4734, 0x0199): "callback-call",
    (0x2660, 0x052D): "callback-entry",
    (0x2660, 0x054D): "presence-return",
    (0x2660, 0x0561): "metadata-return",
    (0x2660, 0x075C): "allocation-call",
    (0x2660, 0x0764): "allocation-return",
    (0x2660, 0x058F): "track-return",
    (0x2660, 0x05C0): "table-write-before",
    (0x2660, 0x05C5): "table-write-after",
    (0x4734, 0x019D): "callback-return",
    (0x4734, 0x01A6): "callback-join",
    (0x4734, 0x0293): "music-gate",
}


def address(segment, offset, delta):
    if type(delta) is not int:
        raise ValueError("invalid disc startup relocation")
    segment += delta
    if not 0 <= segment <= 0xFFFF or not 0 <= offset <= 0xFFFF:
        raise ValueError("disc startup relocation outside real mode")
    return f"{segment:04X}:{offset:04X}"


def snapshot(client, stop, delta):
    """Read only named fields at one fixed, source-evidenced boundary."""
    points = {address(seg, off, delta): label for (seg, off), label in SITES.items()}
    label = points.get(stop.get("cs_ip", "").upper())
    registers = stop.get("registers", {})
    if (stop.get("reason") != "breakpoint" or label is None
            or registers.get("system", {}).get("mode") != "real"):
        raise ValueError("disc snapshot requires a declared real-mode breakpoint")
    try:
        values = {key: int(registers[key], 16) for key in ("ds", "ss", "sp", "ax", "dx", "si", "es", "bx")}
    except (KeyError, TypeError, ValueError) as error:
        raise ValueError("missing disc snapshot registers") from error
    data = 0x57E0 + delta
    if values["ds"] != data or values["ss"] != data:
        raise ValueError("disc startup DS/SS differs from admitted data segment")

    def read(segment, offset, size):
        if (not 0 <= segment <= 0xFFFF or not 0 <= offset <= 0xFFFF
                or not 0 < size <= 59 or offset + size > 0x10000
                or segment * 16 + offset + size > 0x100000):
            raise ValueError("disc named read crosses supported bounds")
        body = client.read_memory(f"{segment:04X}:{offset:04X}", size).data
        if len(body) != size:
            raise ValueError("short disc named-field read")
        return body

    def raw(segment, offset, size):
        return read(segment, offset, size).hex()

    def word(segment, offset):
        return struct.unpack("<H", read(segment, offset, 2))[0]

    def pointer(offset):
        off, seg = struct.unpack("<HH", read(data, offset, 4))
        return seg, off

    # FND-SOUND-094: the only admitted callback entry is the paired native call.
    if label == "callback-entry":
        off, seg = struct.unpack("<HH", read(values["ss"], values["sp"], 4))
        if f"{seg:04X}:{off:04X}" != address(0x4734, 0x019D, delta):
            raise ValueError("disc callback return does not name the known initializer")
    cfg_seg, cfg_off = pointer(0x3411)  # FND-SOUND-092; sound_config.
    if cfg_seg == cfg_off == 0:
        raise ValueError("disc startup configuration pointer is null")
    cfg = read(cfg_seg, cfg_off, 59)
    table_seg, table_off = pointer(0x3482)  # FND-SOUND-067/095.
    disc = 0x4E71 + delta
    result = {
        "phase": label, "cs_ip": stop["cs_ip"], "registers": values,
        "configuration_pointer": f"{cfg_seg:04X}:{cfg_off:04X}",
        "configuration": cfg.hex(), "configuration_flags": struct.unpack_from("<H", cfg, 0x14)[0],
        "music_enable": word(data, 0x3444), "music_slot": word(data, 0x343E),
        "source_handle": word(data, 0x3440), "alternate_handle": word(data, 0x3442),
        "getter_handle": word(data, 0x3464),
        "retained_first": raw(data, 0x3420, 4), "retained_second": raw(data, 0x3458, 4),
        "configuration_callback": raw(data, 0x3486, 4),
        "table_pointer": f"{table_seg:04X}:{table_off:04X}",
        "disc_presence": word(disc, 0x0C39), "disc_enable": read(disc, 0x0C3F, 1)[0],
        "disc_metadata_ready": read(disc, 0x0C3D, 1)[0],
        "first_track": read(disc, 0x0C42, 1)[0], "last_track": read(disc, 0x0C43, 1)[0],
        "lead_out": raw(disc, 0x0C44, 4),
        "rng_state": raw(data, 0x384A, 4),  # FND-RNG-001.
    }
    if label == "allocation-call":
        result["allocation_size"] = struct.unpack("<I", read(data, values["sp"], 4))[0]
    if label in ("table-write-before", "table-write-after"):
        target_off = table_off + 7 * (result["last_track"] + 1) + 2
        if table_seg == table_off == 0:
            raise ValueError("final disc table pointer is null")
        # FND-SOUND-095: the before point has the actual ES:BX+2 destination.
        if label == "table-write-before" and (values["es"] != table_seg or values["bx"] + 2 != target_off):
            raise ValueError("disc table destination differs from the named current table")
        result["table_write_target"] = f"{table_seg:04X}:{target_off:04X}"
        result["table_write_bytes"] = raw(table_seg, target_off, 4)
    return result


def observe(session, delta, report, save, history=None):
    """No arbitrary stops: bounded startup through the fixed music gate only."""
    sites = {address(seg, off, delta) for seg, off in SITES}
    # FND-RNG-001/010: fail before either a draw or a new seed can execute.
    guards = {address(0x1000, off, delta) for off in (0x0822, 0x0811)}
    for site in sorted(sites | guards):
        session.client.set_breakpoint(site)
    pending_write = None
    seen = set()
    for _ in range(600):
        operation = session.continue_()
        observed, exhausted = wait_for_stop(session, operation, attempts=2)
        report["last_stop"] = observed.stop
        save()
        if exhausted:
            raise RuntimeError("disc startup observation budget exhausted")
        if observed.stop.get("cs_ip", "").upper() in guards:
            raise RuntimeError("disc startup reached the RNG/seed guard")
        if history and observed.stop.get("reason") == "watchpoint":
            history.changed(session.client, observed.stop)
            save()
            continue
        if history:
            history.check(session.client)
        captured = snapshot(session.client, observed.stop, delta)
        phase = captured["phase"]
        if not seen and phase != "initializer-entry":
            raise ValueError("disc startup did not begin at the initializer entry")
        if phase == "initializer-entry" and seen:
            raise ValueError("disc startup reentered initialization")
        if phase == "callback-entry" and "callback-call" not in seen:
            raise ValueError("disc callback entry lacks its observed caller")
        if phase == "callback-return" and "callback-entry" not in seen:
            raise ValueError("disc callback return lacks its observed entry")
        if phase == "table-write-before":
            if pending_write is not None:
                raise ValueError("disc final write already pending")
            pending_write = captured
        if phase == "table-write-after":
            if (pending_write is None or any(captured[key] != pending_write[key]
                    for key in ("table_pointer", "last_track", "table_write_target"))):
                raise ValueError("disc final-write storage pair changed")
            pending_write = None
        seen.add(phase)
        report["events"].append(captured)
        save()
        print(phase, len(report["events"]), flush=True)
        if phase == "music-gate":
            if pending_write is not None or "callback-gate" not in seen or "callback-join" not in seen:
                raise ValueError("disc startup endpoint lacks its observed gate/join")
            if history:
                history.finish(session.client)
                history.close(session.client)
                history.complete()
            report["complete"] = True
            save()
            return
    raise RuntimeError("disc startup exceeds600 named stops")


def main():
    if (len(sys.argv) not in (2, 3) or not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", sys.argv[1])
            or len(sys.argv) == 3 and sys.argv[2] != "history"):
        raise SystemExit("usage: disc_startup.py <new-run-name> [history]")
    history_mode = len(sys.argv) == 3
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    expected = config["original"]["analysisExecutable"]
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR", Path(expected["path"]).parent)).resolve()
    source = (game / "DSUN.EXE").read_bytes()
    if len(source) != expected["byteLength"] or xxhash.xxh3_128_hexdigest(source) != expected["xxh3_128"]:
        raise SystemExit("disc startup executable differs from the approved build")
    run = game / "captures/rust-dos-disc-startup" / sys.argv[1]
    if run.exists():
        raise SystemExit("disc startup run already exists")
    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe", emulator_sha256=RUST_DOS_PINNED_SHA256,
        run_directory=run, target=Target("DSUN.EXE", ("-W0", "-L")),
        prepare_drive=lambda destination: prepare_drive(game, destination),
        emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
            deterministic=True, cycles="15000", sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1),
            ultrasound=None), readiness_timeout=60.0,
    )
    report = {"complete": False, "events": [], "draws": [], "inputs": [], "writes": [],
              "limits": "fixed startup point observations; no continuous preservation, heap extent or audible-output proof"}
    with RustDosSession(settings) as session:
        history = None
        def save():
            (run / "disc-startup.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        try:
            report["entry"] = session.entry
            if session.entry["cs_ip"].split(":")[1].upper() != "0000":
                raise ValueError("unexpected disc startup program entry")
            delta = int(session.entry["registers"]["cs"], 16) - 0x1000
            if history_mode:
                history = StartupHistory(delta)
                report["value_history"] = history.report
                history.arm(session.client, session.entry)
            seed = session.client.read_memory(address(0x57E0, 0x384A, delta), 4).data
            if len(seed) != 4:
                raise ValueError("short initial RNG state")
            report["initial_rng_state"] = seed.hex()
            observe(session, delta, report, save, history)
        except Exception as error:
            report["error"] = str(error)
            save()
            raise
        finally:
            try:
                if history:
                    history.close(session.client)
            finally:
                save()


if __name__ == "__main__":
    main()
