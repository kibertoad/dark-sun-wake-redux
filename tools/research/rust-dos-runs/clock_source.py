"""Fixed native observation of startup clock buffers, conversion and seed input."""
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
from probe_drive import ROOT, prepare_drive
from startup_window import wait_for_stop, seed_caller


def source_caller(frame, trampoline, call, loaded):
    if len(frame) != 6 or len(trampoline) != 5:
        raise ValueError("incomplete clock-source caller observation")
    offset, segment, argument = struct.unpack("<HHH", frame)
    opcode, target, overlay = struct.unpack("<BHH", trampoline)
    if (offset != 0x065C or segment != overlay or argument != 0
            or opcode != 0xEA or target != 0x0141
            or call != struct.pack("<BHH", 0x9A, 0x0963, loaded)):
        raise ValueError("clock helper caller is not the declared startup source")


def buffer_offsets(registers):
    bp = int(registers["bp"], 16)
    if registers["ds"] != registers["ss"] or not 8 <= bp <= 0xFFFF:
        raise ValueError("clock near-buffer stack identity or bounds are not admitted")
    return bp - 4, bp - 8


def environment_words(raw, actual_segment, expected_segment):
    if actual_segment != expected_segment or len(raw) != 6:
        raise ValueError("clock environment record identity or width is not admitted")
    low, high, enabled = struct.unpack("<HHH", raw)
    return {"offset": (high << 16) | low, "daylight_enabled": enabled}


def read_environment(read, actual_segment, expected_segment):
    if actual_segment != expected_segment:
        raise ValueError("clock environment segment is not admitted before reading")
    return environment_words(read(actual_segment, 0x397A, 6), actual_segment, expected_segment)


def constructor_caller(frame, selected, data_segment):
    if (len(frame) != 4 or len(selected) != 6
            or struct.unpack("<HH", frame) != (0x0260, data_segment)
            or struct.unpack("<BBHH", selected) != (0xFF, 16, 0x29AA, 0)):
        raise ValueError("constructor caller is not the declared startup record")


def timezone_caller(frame, selected, loaded, data_segment):
    if (len(frame) != 6 or len(selected) != 6
            or struct.unpack("<HHH", frame) != (0x0259, loaded, data_segment)
            or struct.unpack("<BBHH", selected) != (0xFF, 30, 0x3B56, loaded)):
        raise ValueError("timezone caller is not the declared startup record")


def constructor_sizes(raw):
    if len(raw) != 6:
        raise ValueError("incomplete constructor size record")
    payload, source, table = struct.unpack("<HHH", raw)
    if not 2 <= payload <= 32767 or not 8 <= table <= 32768 or table % 8:
        raise ValueError("constructor sizes differ from the admitted scan bounds")
    return {"payload_bytes": payload, "source_segment": source, "table_bytes": table}


def constructor_array(read, actual_segment, expected_segment, payload, payload_bytes, table, table_bytes):
    if actual_segment != expected_segment:
        raise ValueError("constructor segment is not admitted before reading")
    if (not 2 <= payload_bytes <= 32767 or not 8 <= table_bytes <= 32768 or table_bytes % 8
            or not 1 <= payload <= 0x10000 - payload_bytes
            or not 1 <= table <= 0x10000 - table_bytes):
        raise ValueError("constructor spans are null, wrapping or outside the declared size domain")
    if payload < table + table_bytes and table < payload + payload_bytes:
        raise ValueError("constructor payload and table overlap")
    previous = None
    for index in range(min(table_bytes // 2, 1024)):
        raw = read(actual_segment, table + 2 * index, 2)
        if len(raw) != 2:
            raise ValueError("incomplete constructor pointer word")
        pointer = struct.unpack("<H", raw)[0]
        if pointer == 0:
            return {"member_count": index, "terminated": True, "members_inside_payload": True}
        if (not payload <= pointer < payload + payload_bytes
                or (previous is None and pointer != payload)
                or (previous is not None and pointer <= previous)):
            raise ValueError("constructor member is outside its payload or does not progress")
        previous = pointer
    raise ValueError("constructor pointer array lacks a terminator within the scoped read bound")


def main():
    if len(sys.argv) != 2 or not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", sys.argv[1]):
        raise SystemExit("usage: clock_source.py <new-run-name>")
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                              Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    run = game / "captures/rust-dos-clock-source" / sys.argv[1]
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
            mapped = segment - 0x1000 + loaded
            if not 0 <= mapped <= 0xFFFF:
                raise RuntimeError("translated segment outside real mode")
            return f"{mapped:04X}:{offset:04X}"
        report = {"entry": entry, "stops": {}, "draws": [], "inputs": [], "writes": []}
        def record():
            (run / "clock-source.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        def read(segment, offset, size):
            if not 0 <= offset <= 0x10000 - size:
                raise RuntimeError("clock frame read would wrap its offset")
            return session.client.read_memory(f"{segment}:{offset:04X}", size).data
        def reach(offset, stage):
            target = address(0x1000, offset)
            operation = session.continue_(until=target)
            observed, exhausted = wait_for_stop(session, operation)
            report["stops"][stage] = observed.stop
            record()
            if exhausted or observed.stop.get("reason") != "breakpoint" or observed.stop.get("cs_ip") != target:
                raise RuntimeError(f"{stage} not reached; different stop is not admitted")
            return observed.stop["registers"]
        session.client.set_breakpoint(address(0x1000, 0x0822))
        expected_ds = address(0x57E0, 0).split(":")[0]
        expected_data = int(expected_ds, 16)
        def check_constructor(registers, stack_bias):
            if registers["ds"] != expected_ds or registers["ss"] != expected_ds:
                raise RuntimeError("constructor data/stack identity differs before reading")
            frame = read(registers["ss"], int(registers["sp"], 16) + stack_bias, 4)
            selected = read(registers["ds"], 0x39B2, 6)
            constructor_caller(frame, selected, expected_data)
        registers = reach(0x29B7, "environment_payload_allocation_return")
        check_constructor(registers, 4)
        sizes = constructor_sizes(read(registers["ds"], 0x008A, 6))
        payload = int(registers["ax"], 16)
        if not 1 <= payload <= 0x10000 - sizes["payload_bytes"]:
            raise RuntimeError("constructor payload allocation is null or wraps")
        report["constructor"] = {**sizes, "payload_root": payload, "caller_validated": True}
        record()
        registers = reach(0x29DA, "environment_table_allocation_return")
        check_constructor(registers, 4)
        if (constructor_sizes(read(registers["ds"], 0x008A, 6)) != sizes
                or int(registers["di"], 16) != payload or registers["es"] != expected_ds):
            raise RuntimeError("constructor copy sizes, payload or destination identity changed")
        table = int(registers["ax"], 16)
        if not 1 <= table <= 0x10000 - sizes["table_bytes"]:
            raise RuntimeError("constructor table allocation is null or wraps")
        report["constructor"]["table_root"] = table
        record()
        registers = reach(0x2A06, "environment_constructor_return")
        check_constructor(registers, 0)
        if (registers["es"] != expected_ds or registers["si"] != "3994" or registers["di"] != "39BE"
                or constructor_sizes(read(registers["ds"], 0x008A, 6)) != sizes
                or struct.unpack("<H", read(registers["ds"], 0x3920, 2))[0] != table
                or struct.unpack("<H", read(registers["ds"], 0x0088, 2))[0] != table):
            raise RuntimeError("constructor return bounds or root mirror differ")
        report["constructor"]["array"] = constructor_array(
            read, registers["ds"], expected_ds, payload, sizes["payload_bytes"], table, sizes["table_bytes"])
        record()
        registers = reach(0x3B61, "startup_environment_lookup_return")
        if registers["ds"] != expected_ds or registers["ss"] != expected_ds:
            raise RuntimeError("startup environment lookup data/stack identity differs before reading")
        pointer = struct.unpack("<H", read(registers["ss"], int(registers["sp"], 16), 2))[0]
        if pointer != 0x3980 or read(registers["ds"], pointer, 3) != b"TZ\0":
            raise RuntimeError("startup environment lookup name differs")
        report["startup_environment_lookup_result"] = int(registers["ax"], 16)
        record()
        if report["startup_environment_lookup_result"] != 0:
            raise RuntimeError("only the zero-result startup timezone path is admitted")
        registers = reach(0x3CD6, "startup_timezone_return")
        if registers["ds"] != expected_ds or registers["ss"] != expected_ds:
            raise RuntimeError("startup timezone data/stack identity differs before reading")
        frame = read(registers["ss"], int(registers["sp"], 16), 6)
        timezone_caller(frame, read(registers["ds"], 0x39B8, 6), loaded, expected_data)
        report["startup_timezone_caller_validated"] = True
        report["startup_environment"] = read_environment(read, registers["ds"], expected_ds)
        record()
        if report["startup_environment"] != {"offset": 18000, "daylight_enabled": 1}:
            raise RuntimeError("standalone startup timezone globals differ from the default path")
        registers = reach(0x0963, "source_entry")
        frame = read(registers["ss"], int(registers["sp"], 16), 6)
        return_offset, return_segment = struct.unpack("<HH", frame[:4])
        if return_offset != 0x065C:
            raise RuntimeError("unexpected clock source return offset")
        trampoline = session.client.read_memory(address(0x56B2, 0x0020), 5).data
        call = read(f"{return_segment:04X}", return_offset - 5, 5)
        source_caller(frame, trampoline, call, loaded)
        report["source_caller_validated"] = True
        registers = reach(0x0976, "date_return")
        date_offset, time_offset = buffer_offsets(registers)
        pointer = struct.unpack("<H", read(registers["ss"], int(registers["sp"], 16), 2))[0]
        if pointer != date_offset:
            raise RuntimeError("date writer did not receive the declared local buffer")
        date = read(registers["ds"], date_offset, 4)
        registers = reach(0x0980, "time_return")
        if buffer_offsets(registers) != (date_offset, time_offset):
            raise RuntimeError("clock frame moved between service outputs")
        pointer = struct.unpack("<H", read(registers["ss"], int(registers["sp"], 16), 2))[0]
        if pointer != time_offset or read(registers["ds"], date_offset, 4) != date:
            raise RuntimeError("time writer pointer or preceding date buffer changed")
        time = read(registers["ds"], time_offset, 4)
        registers = reach(0x3B61, "environment_lookup_return")
        if registers["ds"] != expected_ds:
            raise RuntimeError("environment lookup uses another data segment")
        pointer = struct.unpack("<H", read(registers["ss"], int(registers["sp"], 16), 2))[0]
        if pointer != 0x3980 or read(registers["ds"], pointer, 3) != b"TZ\0":
            raise RuntimeError("environment lookup name differs from the declared clock name")
        report["environment_lookup_result"] = int(registers["ax"], 16)
        record()
        if report["environment_lookup_result"] != 0:
            raise RuntimeError("only the zero-result startup environment path is admitted")
        registers = reach(0x387D, "environment_initializer_return")
        report["environment"] = read_environment(read, registers["ds"], expected_ds)
        record()
        if report["environment"] != {"offset": 18000, "daylight_enabled": 1}:
            raise RuntimeError("startup environment globals differ from the declared default path")
        registers = reach(0x3933, "daylight_return")
        if registers["ds"] != expected_ds:
            raise RuntimeError("daylight helper returned with another data segment")
        report["daylight_result"] = int(registers["ax"], 16)
        record()
        registers = reach(0x098E, "conversion_return")
        if buffer_offsets(registers) != (date_offset, time_offset):
            raise RuntimeError("clock frame moved during conversion")
        pointers = struct.unpack("<HH", read(registers["ss"], int(registers["sp"], 16), 4))
        if pointers != (date_offset, time_offset):
            raise RuntimeError("converter did not receive the declared date/time buffers")
        if read(registers["ds"], date_offset, 4) != date or read(registers["ds"], time_offset, 4) != time:
            raise RuntimeError("converter changed the measured clock buffers")
        value = (int(registers["dx"], 16) << 16) | int(registers["ax"], 16)
        report["buffers"] = {"date_offset": date_offset, "time_offset": time_offset,
                             "date_words": list(struct.unpack("<HH", date)),
                             "time_words": list(struct.unpack("<HH", time)),
                             "identity_validated": True}
        report["conversion_result"] = value
        record()
        registers = reach(0x0811, "seed_entry")
        frame = read(registers["ss"], int(registers["sp"], 16), 6)
        offset, segment = struct.unpack("<HH", frame[:4])
        if offset != 0x0663:
            raise RuntimeError("unexpected seed caller offset")
        call = read(f"{segment:04X}", offset - 5, 5)
        trampoline = session.client.read_memory(address(0x56B2, 0x0020), 5).data
        _, argument = seed_caller(frame, trampoline, call, loaded)
        if argument != value & 0xFFFF:
            raise RuntimeError("seed argument differs from the conversion low word")
        report["seed_argument"] = argument
        record()
        print(json.dumps({"buffers": report["buffers"], "conversion_result": value,
                          "seed_argument": argument, "environment": report["environment"],
                          "environment_lookup_result": report["environment_lookup_result"],
                          "daylight_result": report["daylight_result"],
                          "constructor": report["constructor"],
                          "startup_environment": report["startup_environment"], "draws": []}), flush=True)


if __name__ == "__main__":
    main()
