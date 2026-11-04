"""Fixed F2 entry observation from an admitted map checkpoint; no guest writes."""
import hashlib
import json
import os
from pathlib import Path
import re
import struct
import sys

from dinorefurb_dosbox_session import (
    Media, RUST_DOS_PINNED_SHA256, RustDosConfig, RustDosSession,
    RustDosSettings, SoundBlaster, Target, read_saved_state,
)

sys.path.insert(0, str(Path(__file__).resolve().parent))
from probe_drive import ROOT
from restore_opener import verify_checkpoint_executable
from regenerate_checkpoints import admit_state, mapping, snapshot, ready, checked_read
from checkpoint_rng import Recorder
from startup_window import wait_for_stop, checkpoint_summary
import request_provider
import archive_origin
from overlay_returns import region_target, region_iret, ReaderReturn
import map_restore
from object_restore import ObjectTrace
from script_restore import ScriptTrace
from item_restore import ItemTrace


def fields(client, delta):
    def read(seg, off, size):
        return checked_read(client, seg + delta, off, size)
    def word(seg, off):
        return struct.unpack("<H", read(seg, off, 2))[0]
    def pointer(raw):
        off, seg = struct.unpack("<HH", raw)
        return f"{seg:04X}:{off:04X}"
    target = read(0x57E0, 0xA0FD, 4)
    off, seg = struct.unpack("<HH", target)
    window = None
    if off or seg:
        body = checked_read(client, seg, off, 0xF9)
        if body[:4] != b"WIND":
            raise ValueError("event target lacks the evidenced window tag")
        window = {"number": struct.unpack_from("<I", body, 8)[0],
                  "callback": pointer(body[0xF5:0xF9])}
    return {
        "count": word(0x57E0, 0xA103),
        "head": pointer(read(0x57E0, 0xA105, 4)),
        "event_target": pointer(target), "event_window": window,
        "global_handler": pointer(read(0x57E0, 0xA0F1, 4)),
        "kind_three_handler": pointer(read(0x57E0, 0xA0F5, 4)),
        "saved_handler": pointer(read(0x4C4D, 0x000F, 4)),
        "handler_mirror": pointer(read(0x57E0, 0x61A2, 4)),
        "selected_row": word(0x4C4C, 0), "list_top": word(0x4C4C, 2),
        "save_load_window": pointer(read(0x50AF, 0, 4)),
        "load_mode": read(0x50AF, 4, 1)[0],
        "list_pointer": pointer(read(0x57E0, 0x6278, 4)),
        "exit_byte": read(0x57E0, 0x627C, 1)[0],
    }


def map_ready(state):
    return (ready(state, 19501) and state["mode"] == 1
            and state["slot_kinds"] == [0, 2, 0, 2] and state["leader"] == 1)


def input_map_ready(client, delta, state, ui):
    if (state["mode"] != 1 or state["leader"] != 1 or state["slot_kinds"] != [0, 2, 0, 2]
            or state["mouse_enabled"] != 1 or ui["count"] != 1 or ui["exit_byte"] != 0
            or ui["global_handler"] != f"{0x28C9 + delta:04X}:0CFF"
            or any(ui[name] != "0000:0000" for name in ("event_target", "save_load_window", "list_pointer"))):
        return False
    seg, off = (int(x, 16) for x in ui["head"].split(":"))
    if not (seg or off):
        return False
    body = checked_read(client, seg, off, 0xA0)
    if body[:4] != b"WIND":
        raise ValueError("map head lacks the declared window tag")
    head = {"number": struct.unpack_from("<I", body, 8)[0],
            "flags": struct.unpack_from("<H", body, 0x9E)[0]}
    return (head["number"] == 19501 and head["flags"] & 0x1F == 8
            and (state["window"] is None or state["window"] == head))


def observe_delivery(session, operation, previous):
    return previous if previous is not None and not previous.pending else session.observe(operation, timeout=0)


def native_path_pair(source, destination):
    return (bytes.fromhex(source["bytes_hex"]).lower() == b"c:\\save02.sav"
            and bytes.fromhex(destination["bytes_hex"]).lower() == b"c:\\darkrun.gff")


def archive_fields(client, delta):
    ds = 0x57E0 + delta
    def value(offset, size):
        return int.from_bytes(checked_read(client, ds, offset, size), "little")
    def record(offset):
        off, seg = struct.unpack("<HH", checked_read(client, ds, offset, 4))
        handle = int.from_bytes(checked_read(client, seg, off + 0x28, 4), "little") if off or seg else None
        return {"pointer": f"{seg:04X}:{off:04X}", "handle_field": handle}
    return {"reuse": value(0x167A, 1), "working_handle": value(0x144E, 4),
            "region_handle": value(0x145A, 4), "game_mode": value(0x0DAB, 2),
            "region": value(0x140C, 2), "traversal_mode": value(0x9D9B, 2),
            "counter": value(0x9D8E, 2), "active": record(0x9D9F), "head": record(0x9DA3)}


def declared_return(client, registers, expected):
    off, seg = struct.unpack("<HH", checked_read(client, int(registers["ss"], 16), int(registers["sp"], 16), 4))
    if off != expected:
        raise ValueError("return does not identify the declared reader-preparation caller")
    return f"{seg:04X}:{off:04X}"


def reader_frame(client, registers, segment):
    off, seg, selector = struct.unpack("<HHH", checked_read(client, int(registers["ss"], 16), int(registers["sp"], 16), 6))
    if off != 0xF2F or seg != segment or selector != 0:
        raise ValueError("reader frame differs from the declared selector-zero call")
    return selector


def load_ready(state, ui):
    return ready(state, 18500) and ui["load_mode"] == 1


def event_words(client, registers, bias):
    if bias not in (0, 4):
        raise ValueError("unsupported event frame")
    data = checked_read(client, int(registers["ss"], 16),
                        int(registers["sp"], 16) + bias, 24)
    return list(struct.unpack("<12H", data))


def navigation_matches(call, entry, return_frame, expected_return, callback, key_word=0x5000):
    return (call is not None and call["event_words"] == entry
            and entry[0] == 6 and entry[6] == key_word
            and call["fields"]["global_handler"] == callback
            and return_frame == expected_return)


def list_markers(client, ui):
    if ui["list_top"] != 0 or not 0 <= ui["selected_row"] < 10:
        raise ValueError("marker observation requires an admitted first-page row")
    seg, off = (int(x, 16) for x in ui["list_pointer"].split(":"))
    if not (seg or off) or off + 1250 > 0x10000:
        raise ValueError("null or wrapping ten-record list")
    return [checked_read(client, seg, off + 125 * row, 1)[0] for row in range(10)]


def returned_frame(entry, returned):
    start = int(entry["sp"], 16)
    return (start + 4 <= 0xFFFF and int(returned["ss"], 16) == int(entry["ss"], 16)
            and int(returned["sp"], 16) == start + 4)


def load_root(client, callback):
    seg, off = (int(x, 16) for x in callback.split(":"))
    jump = checked_read(client, seg, off, 5)
    if jump[0] != 0xEA or struct.unpack_from("<H", jump, 1)[0] != 0x0785:
        raise ValueError("Load trampoline differs from declared overlay entry")
    target_seg = struct.unpack_from("<H", jump, 3)[0]
    call = checked_read(client, target_seg, 0x0BDF, 3)
    if call != struct.pack("<BH", 0xE8, (0x05ED - 0x0BE2) & 0xFFFF):
        raise ValueError("numbered-load call differs from declared near call")
    return f"{target_seg:04X}:05ED", f"{target_seg:04X}:0BE2"


def terminated_path(client, segment, offset, bound):
    if bound not in (80, 82) or not (segment or offset):
        raise ValueError("unsupported path storage")
    data = bytearray()
    for index in range(bound):
        value = checked_read(client, segment, offset + index, 1)[0]
        if value == 0:
            return {"length": len(data), "bytes_hex": data.hex()}
        data.append(value)
    raise ValueError("path lacks a terminator within its declared bound")


def copy_arguments(frame):
    if len(frame) != 20:
        raise ValueError("short copy frame")
    words = struct.unpack("<10H", frame)
    if any(words[6:]):
        raise ValueError("copy arguments differ from the internal-buffer contract")
    return words[:6]


def named_file(run, name):
    if name not in ("darkrun.GFF", "SAVE02.SAV", "RGN032.GFF"):
        raise ValueError("unsupported private file name")
    drive = run / "drive-c"
    path = drive / name
    if any(p.is_symlink() or p.is_junction() for p in (run, drive, path)):
        raise ValueError("working file path contains a link")
    if not path.exists():
        return {"exists": False}
    limit = 128 * 1024 * 1024
    if not path.is_file() or path.stat().st_size > limit:
        raise ValueError("unsupported working file")
    digest = hashlib.sha256()
    total = 0
    with path.open("rb") as stream:
        while block := stream.read(min(1024 * 1024, limit + 1 - total)):
            total += len(block)
            if total > limit:
                raise ValueError("working file grew beyond bound")
            digest.update(block)
    return {"exists": True, "length": total, "sha256": digest.hexdigest()}


def working_file(run):
    return named_file(run, "darkrun.GFF")


def private_span(run, selection, identity, name="darkrun.GFF"):
    offset, count = selection["offset"], selection["count"]
    if (not identity.get("exists") or named_file(run,name) != identity or count not in (2, 1920,12544)
            or offset < 0 or offset + count > identity["length"]):
        raise ValueError("private payload span or copied file identity differs")
    with (run / "drive-c" / name).open("rb") as stream:
        stream.seek(offset)
        data = stream.read(count)
    if len(data) != count or named_file(run,name) != identity:
        raise ValueError("private payload span changed or was short")
    return data


def private_row(run, row, identity, name="darkrun.GFF"):
    offset = row["file_row_offset"]
    if not identity.get("exists") or named_file(run,name) != identity or offset < 0 or offset + 12 > identity["length"]:
        raise ValueError("private directory row lies outside the unchanged working file")
    with (run / "drive-c" / name).open("rb") as stream:
        stream.seek(offset)
        data = stream.read(12)
    if len(data) != 12 or named_file(run,name) != identity:
        raise ValueError("private directory row was short or file identity changed")
    return struct.unpack("<III", data) == (row["number"], row["offset"], row["length"])


def settings_request(client, registers, offset):
    if offset not in (0x66F, 0x743):
        raise ValueError("unsupported settings request")
    data = checked_read(client, int(registers["ss"], 16), int(registers["sp"], 16), 12)
    tag, number, out_off, out_seg = struct.unpack("<IIHH", data)
    expected = 0x46455250 if offset == 0x66F else 0x51455247
    if tag != expected or (offset == 0x66F and number != 100):
        raise ValueError("settings request differs from the declared tag/ID")
    return {"tag": "PREF" if offset == 0x66F else "GREQ", "id": number,
            "output": f"{out_seg:04X}:{out_off:04X}"}


def parent_mapping(client, delta, copy_entry_registers, load_entry_registers):
    ss = int(copy_entry_registers["ss"], 16)
    wrapper_bp = int(copy_entry_registers["bp"], 16)
    child = checked_read(client, ss, wrapper_bp + 2, 4)
    parent = checked_read(client, int(load_entry_registers["ss"], 16),
                          int(load_entry_registers["sp"], 16), 4)
    child_off, child_seg = struct.unpack("<HH", child)
    parent_off, parent_seg = struct.unpack("<HH", parent)
    if child_off != 0x659 or parent_off != 0xBE2 or child_seg != parent_seg:
        raise ValueError(f"unexpected parent frames {child_seg:04X}:{child_off:04X}, {parent_seg:04X}:{parent_off:04X}")
    code = checked_read(client, child_seg, child_off, 8)
    expected = bytes([0x83, 0xC4, 4]) + struct.pack("<BHH", 0x9A, 0x52, 0x56E9 + delta)
    if code != expected:
        raise ValueError("parent loaded code differs from declared continuation")
    return f"{child_seg:04X}", f"{parent_seg:04X}:{parent_off:04X}"


def saved_load_return(client, registers):
    data = checked_read(client, int(registers["ss"], 16), int(registers["sp"], 16), 4)
    offset, segment = struct.unpack("<HH", data)
    if offset != 0xBE2:
        raise ValueError("saved return no longer names the declared load caller")
    return f"{segment:04X}:{offset:04X}"


def caller_cleanup_jump(code):
    return code == bytes([0xEB, (0xBF2 - 0xBE4) & 0xFF])


def main():
    if (len(sys.argv) not in (3, 4) or
            any(not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", x) for x in sys.argv[1:]) or
            (len(sys.argv) == 4 and sys.argv[3] not in ("down", "down-return", "load-call", "copy-inputs", "copy-return", "load-return", "saved-copy-return", "saved-reader-entry", "saved-providers", "saved-payloads", "saved-origin", "saved-reader-return", "saved-map-request", "saved-objects", "saved-script-conversion", "saved-items"))):
        raise SystemExit("usage: load_entry.py <source-run> <new-run-name> [down|down-return|load-call|copy-inputs|copy-return|load-return|saved-copy-return|saved-reader-entry|saved-providers|saved-payloads|saved-origin|saved-reader-return|saved-map-request|saved-objects|saved-script-conversion|saved-items]")
    navigation = len(sys.argv) == 4
    observe_objects = navigation and sys.argv[3] == "saved-objects"
    observe_items = navigation and sys.argv[3] == "saved-items"
    observe_scripts = observe_items or navigation and sys.argv[3] == "saved-script-conversion"
    observe_map = observe_objects or observe_scripts or navigation and sys.argv[3] == "saved-map-request"
    observe_reader_return = observe_map or navigation and sys.argv[3] == "saved-reader-return"
    observe_origin = observe_reader_return or navigation and sys.argv[3] == "saved-origin"
    observe_payloads = observe_origin or navigation and sys.argv[3] == "saved-payloads"
    observe_providers = observe_payloads or navigation and sys.argv[3] == "saved-providers"
    saved_source = observe_providers or navigation and sys.argv[3] in ("saved-copy-return", "saved-reader-entry")
    observe_reader = observe_providers or navigation and sys.argv[3] == "saved-reader-entry"
    stage = "copy-return" if saved_source else sys.argv[3] if navigation else "entry"
    observe_return = stage == "down-return"
    observe_load = stage in ("load-call", "copy-inputs", "copy-return", "load-return")
    observe_copy = stage in ("copy-inputs", "copy-return", "load-return")
    observe_copy_return = stage in ("copy-return", "load-return")
    observe_load_return = stage == "load-return"
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    source = game / ("captures/rust-dos-native-save" if saved_source else "captures/rust-dos-regenerated") / sys.argv[1]
    meta = json.loads((source / ("native-save.json" if saved_source else "regeneration.json")).read_text(encoding="utf-8"))
    state = read_saved_state(source, "saved-map" if saved_source else "play-ready")
    verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
    admit_state(state)
    delta = mapping(meta["load_mapping_stop"])
    rng = Recorder(delta)
    run = game / "captures/rust-dos-load-entry" / sys.argv[2]
    if run.exists():
        raise ValueError("run already exists")
    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe",
        emulator_sha256=RUST_DOS_PINNED_SHA256, run_directory=run,
        target=Target("DSUN.EXE", ("-W0", "-L")), start_state=state,
        emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
            deterministic=True, cycles="15000",
            sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1), ultrasound=None),
        readiness_timeout=60.0)
    report = {"source": sys.argv[1], "writes": [], "draws": [],
              "dispatches": [], "calls": [], "observations": [], "passed": False}
    with RustDosSession(settings) as session:
        def record():
            (run / "load-entry.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        try:
            report["restored_stop"] = session.stop
            if (session.stop["reason"] != "load" or session.stop["cs_ip"] != state.stop["cs_ip"]
                    or session.stop["emu_ns"] != state.stop["emu_ns"]):
                raise ValueError("restored instruction/time differs")
            initial = snapshot(session.client, delta)
            report["initial"] = {"state": initial, "fields": fields(session.client, delta)}
            if saved_source:
                if (not meta.get("passed") or initial != meta["saved_state"]
                        or named_file(run, "SAVE02.SAV") != meta["save_after"]
                        or not input_map_ready(session.client, delta, initial, report["initial"]["fields"])):
                    raise ValueError("native saved-source checkpoint differs from its verified candidate")
            elif not map_ready(initial):
                raise ValueError("source is not the admitted two-character map state")
            dispatch = rng.address(0x39D1, 0x0752)
            global_call = rng.address(0x39D1, 0x083B)
            callback = rng.address(0x5736, 0x002F)
            for address in (dispatch, global_call, rng.address(0x1000, 0x0822), rng.address(0x1000, 0x0811)):
                session.client.set_breakpoint(address)
            key = session.send_input("key", {"key": "f2", "hold_ms": 100})
            delivery = None
            key_name = "f2"
            awaiting_return = False
            root = root_return = None
            copy_entry = rng.address(0x4544, 0)
            copy_return = None
            copy_stops = {rng.address(0x4544, off): off for off in (0x4F, 0x67, 0x78, 0x7D, 0x8C, 0x91, 0xA6)}
            report["copy_steps"] = []
            load_stops = {}
            report["load_steps"] = []
            return_watch = None
            report["return_relocations"] = []
            region_call = rng.address(0x56BD, 0xC0)
            region_trap = rng.address(0x56EF, 0)
            manager_iret = rng.address(0x4AE5, 0x559)
            region_return = region_watch = reader_entry = None
            awaiting_region_reload = False
            provider_entry = rng.address(0x38FF, 0x7B1)
            provider_selected = rng.address(0x38FF, 0x94A)
            provider_return = rng.address(0x38FF, 0x9D4)
            target_request = None
            report["resource_requests"] = []
            open_entry = rng.address(0x38FF, 0x66)
            open_io = rng.address(0x38FF, 0x96)
            open_return = rng.address(0x38FF, 0x2B1)
            opening = published = None
            reader_return = None
            reference_payloads = {}
            open_report = "working_open"
            map_prediction = None
            object_trace = None
            script_trace = None
            item_trace = None
            report["region_relocations"] = []
            previous = None
            budget = 4096 if observe_objects else 1024 if observe_load_return or observe_reader_return else 512
            report["observation_limit"] = budget
            for index in range(budget):
                target = int(session.stop["emu_ns"]) // 1000000 + 100
                observed, exhausted = wait_for_stop(session, session.continue_(until_ms=target))
                report["last_stop"] = observed.stop
                report["continuations"] = index + 1
                if exhausted:
                    raise RuntimeError("operation observation budget exhausted")
                stop = observed.stop
                if reader_return is not None:
                    outcome = reader_return.handle(stop)
                    report["reader_return_events"] = reader_return.events
                    if outcome == "returned":
                        if item_trace is not None:
                            report["item_trace"] = item_trace.finish()
                        if script_trace is not None:
                            report["script_trace"] = script_trace.finish()
                        if object_trace is not None:
                            report["object_trace"] = object_trace.finish()
                        if observe_map:
                            if map_prediction is None:
                                raise ValueError("reader returned without an admitted GMAP transfer")
                            current_map = map_restore.state(session.client,delta)
                            map_step = report["map_request"]
                            map_step["reader_return_comparison"] = map_restore.comparison(map_restore.read(session.client,map_step["selected"]["destination"],12544),map_prediction)
                            map_step["reader_return_state"] = current_map
                            map_step["same_binding"] = current_map["map"] == map_step["selected"]["destination"]
                        if set(reference_payloads) != {24,26}:
                            raise ValueError("reader returned without both admitted target transfers")
                        samples = []
                        for step in report["resource_requests"]:
                            number = step["request"]["number"]
                            sample = request_provider.payload(session.client, step["selected"], number, reference_payloads[number])
                            slot = checked_read(session.client, 0x57E0+delta, 0x157E+4*(number-1), 4)
                            off, seg = struct.unpack("<HH", slot)
                            sample.update({"number": number, "source_slot": f"{seg:04X}:{off:04X}",
                                           "same_binding": f"{seg:04X}:{off:04X}" == step["selected"]["destination"]})
                            samples.append(sample)
                        report["reader_return"] = {"stop": stop, "al": int(stop["registers"]["ax"],16)&0xFF,
                                                   "targets": samples, "archive": archive_fields(session.client, delta)}
                        report["passed"] = True
                        break
                    if outcome == "continued":
                        record()
                        continue
                if item_trace is not None and item_trace.handle(stop):
                    record()
                    continue
                if object_trace is not None and object_trace.handle(stop):
                    record()
                    continue
                if script_trace is not None and script_trace.handle(stop):
                    record()
                    continue
                if observe_origin and stop["reason"] == "breakpoint" and stop["cs_ip"] == open_entry:
                    opening = archive_origin.opening(session.client, stop["registers"], delta, terminated_path,region=open_report=="region_open")
                    if opening is not None:
                        report[open_report] = {"entry": stop, "arguments": opening}
                        session.client.remove_breakpoints(open_entry)
                        session.client.set_breakpoint(open_io, once=True)
                        session.client.set_breakpoint(open_return, once=True)
                    record()
                    continue
                if observe_origin and stop["reason"] == "breakpoint" and stop["cs_ip"] == open_io:
                    request_provider.frame(stop["registers"], opening)
                    opening["io_handle"] = int(stop["registers"]["ax"], 16)
                    report[open_report]["io_return"] = stop
                    if opening["io_handle"] == 0xFFFF:
                        raise ValueError("fixed working-file I/O open failed")
                    record()
                    continue
                if observe_origin and stop["reason"] == "breakpoint" and stop["cs_ip"] == open_return:
                    current = archive_fields(session.client, delta)
                    published = archive_origin.publication(session.client, stop["registers"], opening, current,region=open_report=="region_open")
                    report[open_report].update({"return": stop, "publication": published, "archive": current})
                    record()
                    continue
                if observe_providers and stop["reason"] == "breakpoint" and stop["cs_ip"] == provider_entry:
                    if target_request is not None:
                        raise ValueError("nested resource request while observing a target")
                    target_request = request_provider.request(session.client, stop["registers"], 0x38FF + delta)
                    if target_request is None and observe_map and open_report=="region_open":
                        target_request = map_restore.request(session.client,stop["registers"],delta)
                        if target_request is not None:
                            if "map_request" in report:
                                raise ValueError("duplicate GMAP request")
                            report["map_request"] = {"request":target_request,"entry":stop,"state":map_restore.state(session.client,delta)}
                            session.client.set_breakpoint(provider_selected,once=True)
                            session.client.set_breakpoint(provider_return,once=True)
                            record()
                            continue
                    if target_request is not None:
                        expected_number = 24 if not report["resource_requests"] else 26
                        if target_request["number"] != expected_number:
                            raise ValueError("target SAVE requests differ from the admitted order")
                        report["resource_requests"].append({"request": target_request, "entry": stop})
                        session.client.set_breakpoint(provider_selected, once=True)
                        session.client.set_breakpoint(provider_return, once=True)
                    record()
                    continue
                if target_request is not None and target_request.get("kind")=="map" and stop["reason"]=="breakpoint" and stop["cs_ip"]==provider_selected:
                    step = report["map_request"]
                    selection = request_provider.selected(session.client,stop["registers"],target_request)
                    step.update({"selected":selection,"selection_stop":stop,"archive":archive_fields(session.client,delta)})
                    source = next(((key,name) for key,name in (("region_open","RGN032.GFF"),("working_open","darkrun.GFF"))
                                   if key in report and all(selection[k]==report[key]["publication"][k] for k in ("provider","provider_handle","io_handle"))),None)
                    if source is None or selection["count"]!=12544:
                        raise ValueError("GMAP source or length is outside the admitted candidates")
                    key,name = source
                    row = archive_origin.directory(session.client,stop["registers"],target_request,selection,report[key]["publication"],expected_tag=b"GMAP")
                    identity = named_file(run,name)
                    row["private_row_equal"] = private_row(run,row,identity,name)
                    step.update({"directory":row,"source_file":name,"source_identity":identity})
                    if not row["private_row_equal"]:
                        raise ValueError("GMAP directory row differs from its private source")
                    record()
                    continue
                if target_request is not None and target_request.get("kind")=="map" and stop["reason"]=="breakpoint" and stop["cs_ip"]==provider_return:
                    request_provider.frame(stop["registers"],target_request)
                    step = report["map_request"]
                    step["return"] = stop
                    step["reported_count"] = int.from_bytes(checked_read(session.client,0x57E0+delta,0x9D91,4),"little")
                    if int(stop["registers"]["ax"],16) or step["reported_count"]!=12544:
                        raise ValueError("GMAP transfer did not complete its admitted count")
                    expected = private_span(run,step["selected"],step["source_identity"],step["source_file"])
                    actual = map_restore.read(session.client,step["selected"]["destination"],12544)
                    step["payload"] = map_restore.comparison(actual,expected)
                    current = map_restore.state(session.client,delta)
                    step["transfer_state"] = current
                    if not step["payload"]["equal"] or current["map"]!=step["selected"]["destination"]:
                        raise ValueError("GMAP payload or destination binding differs")
                    first = map_restore.read(session.client,current["first_plane"],1568)
                    second = map_restore.read(session.client,current["second_plane"],1568)
                    map_prediction = map_restore.expected_flags(actual,first,second)
                    spans = {"map":(current["map"],12544),"first_plane":(current["first_plane"],1568),"second_plane":(current["second_plane"],1568)}
                    spans.update({f"SAVE/{s['request']['number']}":(s["selected"]["destination"],s["selected"]["count"]) for s in report["resource_requests"]})
                    step["overlaps"] = map_restore.overlaps(spans)
                    session.client.remove_breakpoints(provider_entry)
                    if object_trace is not None:
                        object_trace.capture_before()
                    target_request = None
                    record()
                    continue
                if observe_providers and stop["reason"] == "breakpoint" and stop["cs_ip"] == provider_selected:
                    step = report["resource_requests"][-1]
                    step["selected"] = request_provider.selected(session.client, stop["registers"], target_request)
                    step["selection_stop"] = stop
                    step["archive"] = archive_fields(session.client, delta)
                    if observe_origin:
                        if published is None:
                            raise ValueError("target selection preceded admitted working publication")
                        row = archive_origin.directory(session.client, stop["registers"], target_request, step["selected"], published)
                        row["private_row_equal"] = private_row(run, row, report["copy_return"]["working_after"])
                        step["directory"] = row
                        record()
                        if not row["private_row_equal"]:
                            raise ValueError("selected directory row differs from its private-file row")
                    if target_request["number"] == 26 and not observe_payloads:
                        report["passed"] = True
                        break
                    record()
                    continue
                if observe_providers and stop["reason"] == "breakpoint" and stop["cs_ip"] == provider_return:
                    request_provider.frame(stop["registers"], target_request)
                    step = report["resource_requests"][-1]
                    step["return"] = stop
                    step["error"] = int.from_bytes(checked_read(session.client, 0x57E0+delta, 0x9D99, 2), "little")
                    step["reported_count"] = int.from_bytes(checked_read(session.client, 0x57E0+delta, 0x9D91, 4), "little")
                    if "selected" not in step or int(stop["registers"]["ax"],16) != 0 or step["reported_count"] != step["selected"]["count"]:
                        raise ValueError("target SAVE did not complete the matched full-length request")
                    if observe_payloads:
                        expected = private_span(run, step["selected"], report["copy_return"]["working_after"])
                        reference_payloads[target_request["number"]] = expected
                        step["payload"] = request_provider.payload(session.client, step["selected"], target_request["number"], expected)
                        record()
                        if not step["payload"]["equal"]:
                            raise ValueError("target payload differs from the selected private-file span")
                        if target_request["number"] == 26:
                            if not observe_reader_return:
                                report["passed"] = True
                                break
                            if observe_map:
                                open_report = "region_open"
                                session.client.set_breakpoint(open_entry)
                            else:
                                session.client.remove_breakpoints(provider_entry)
                    target_request = None
                    record()
                    continue
                if stop["reason"] == "breakpoint" and stop["cs_ip"] == rng.address(0x1000, 0x0822):
                    report["draws"].append(rng.record(session, stop))
                    record()
                    continue
                if stop["reason"] == "watchpoint" and region_watch is not None:
                    relocated, trapped = region_target(session.client, report["region_entry"]["stop"]["registers"], delta)
                    report["region_relocations"].append({"before": region_return, "after": relocated, "stop": stop})
                    if relocated != region_return:
                        session.client.remove_breakpoints(region_return)
                        region_return = relocated
                        if not trapped:
                            session.client.set_breakpoint(region_return, once=True)
                    if trapped:
                        session.client.set_breakpoint(region_trap, once=True)
                    record()
                    continue
                if stop["reason"] == "watchpoint" and return_watch is not None:
                    relocated = saved_load_return(session.client, report["load_call"]["stop"]["registers"])
                    report["return_relocations"].append({"stop": stop, "before": root_return, "after": relocated})
                    if relocated != root_return:
                        session.client.remove_breakpoints(root_return)
                        root_return = relocated
                        session.client.set_breakpoint(root_return, once=True)
                    record()
                    continue
                if stop["reason"] == "breakpoint" and stop["cs_ip"] == dispatch:
                    report["dispatches"].append({"stop": stop, "event_words": event_words(session.client, stop["registers"], 4),
                                                  "fields": fields(session.client, delta)})
                elif stop["reason"] == "breakpoint" and stop["cs_ip"] == global_call:
                    report["calls"].append({"stop": stop, "event_words": event_words(session.client, stop["registers"], 0),
                                             "fields": fields(session.client, delta)})
                elif stop["reason"] == "breakpoint" and stop["cs_ip"] == callback and key_name in ("down", "enter"):
                    r = stop["registers"]
                    entry = event_words(session.client, r, 4)
                    frame = checked_read(session.client, int(r["ss"], 16), int(r["sp"], 16), 4)
                    off, seg = struct.unpack("<HH", frame)
                    ret = f"{seg:04X}:{off:04X}"
                    report["callback_entry"] = {"stop": stop, "event_words": entry,
                        "return": ret, "fields": fields(session.client, delta)}
                    delivery = observe_delivery(session, key, delivery)
                    report["navigation_input"] = {"key": key_name, "state": delivery.state, "detail": delivery.detail}
                    if not navigation_matches(report["calls"][-1] if report["calls"] else None,
                            entry, ret, rng.address(0x39D1, 0x083F), callback,
                            0x1C0D if observe_load else 0x5000):
                        raise RuntimeError("navigation callback/frame differs from the observed global call")
                    if observe_load:
                        report["markers_before"] = list_markers(session.client, report["callback_entry"]["fields"])
                        session.client.remove_breakpoints(callback)
                        record()
                        continue
                    if observe_return:
                        report["markers_before"] = list_markers(session.client, report["callback_entry"]["fields"])
                        session.client.remove_breakpoints(callback)
                        session.client.set_breakpoint(ret, once=True)
                        awaiting_return = True
                        record()
                        continue
                    report["passed"] = True
                    break
                elif root is not None and stop["reason"] == "breakpoint" and stop["cs_ip"] == root:
                    if "callback_entry" not in report:
                        raise RuntimeError("numbered-load entry preceded admitted Enter callback")
                    r = stop["registers"]
                    frame = checked_read(session.client, int(r["ss"], 16), int(r["sp"], 16), 6)
                    off, seg, row = struct.unpack("<HHH", frame)
                    if f"{seg:04X}:{off:04X}" != root_return:
                        raise RuntimeError("numbered-load return does not identify the declared caller")
                    report["load_call"] = {"stop": stop, "row_argument": row,
                        "return": root_return, "fields": fields(session.client, delta)}
                    delivery = observe_delivery(session, key, delivery)
                    report["navigation_input"] = {"key": key_name, "state": delivery.state, "detail": delivery.detail}
                    if observe_copy:
                        session.client.remove_breakpoints(root)
                        session.client.set_breakpoint(copy_entry)
                        record()
                        continue
                    report["passed"] = True
                    break
                elif (observe_copy and stop["reason"] == "breakpoint"
                      and stop["cs_ip"] == copy_entry):
                    if "load_call" not in report:
                        raise RuntimeError("copy entry preceded the admitted numbered-load call")
                    r = stop["registers"]
                    frame = checked_read(session.client, int(r["ss"], 16), int(r["sp"], 16), 20)
                    ret_off, ret_seg, src_off, src_seg, dst_off, dst_seg = copy_arguments(frame)
                    trampoline = checked_read(session.client, 0x56EF + delta, 0x89, 5)
                    if trampoline != struct.pack("<BHH", 0xEA, 0x0EBE, ret_seg) or ret_off != 0x0EFC:
                        raise RuntimeError("copy return/trampoline differs from declared wrapper")
                    instruction = checked_read(session.client, ret_seg, 0x0EF7, 5)
                    if instruction != struct.pack("<BHH", 0x9A, 0, 0x4544 + delta):
                        raise RuntimeError("copy call instruction differs from declared resident target")
                    report["copy_inputs"] = {"stop": stop, "source_pointer": f"{src_seg:04X}:{src_off:04X}",
                        "destination_pointer": f"{dst_seg:04X}:{dst_off:04X}",
                        "source": terminated_path(session.client, src_seg, src_off, 80),
                        "destination": terminated_path(session.client, dst_seg, dst_off, 82),
                        "fields": fields(session.client, delta), "buffer_null": True, "capacity": 0}
                    delivery = observe_delivery(session, key, delivery)
                    report["navigation_input"] = {"key": key_name, "state": delivery.state, "detail": delivery.detail}
                    if observe_copy_return:
                        report["working_before"] = working_file(run)
                        if saved_source:
                            if not native_path_pair(report["copy_inputs"]["source"], report["copy_inputs"]["destination"]):
                                raise ValueError("native source/destination differ from the fixed path pair")
                            report["source_before"] = named_file(run, "SAVE02.SAV")
                            if report["source_before"] != meta["save_after"]:
                                raise ValueError("native source file changed before copying")
                            if (run / "drive-c/SAVE02.SAV").samefile(run / "drive-c/darkrun.GFF"):
                                raise ValueError("native source and working file alias")
                        copy_return = f"{ret_seg:04X}:{ret_off:04X}"
                        session.client.remove_breakpoints(copy_entry)
                        for address in (*copy_stops, copy_return):
                            session.client.set_breakpoint(address, once=True)
                        record()
                        continue
                    report["passed"] = True
                    break
                elif observe_copy_return and stop["reason"] == "breakpoint" and stop["cs_ip"] in copy_stops:
                    r = stop["registers"]
                    original = report["copy_inputs"]["stop"]["registers"]
                    if (r["ss"] != original["ss"] or
                            int(r["bp"], 16) != int(original["sp"], 16) - 2):
                        raise RuntimeError("internal stop belongs to another copy frame")
                    at = copy_stops[stop["cs_ip"]]
                    step = {"offset": at, "stop": stop, "ax": r["ax"]}
                    if at in (0x4F, 0x67):
                        output = int(r["bp"], 16) - (0xDA if at == 0x4F else 0x12A)
                        step["normalized"] = terminated_path(session.client, int(r["ss"], 16), output, 80)
                    if at == 0xA6:
                        step["length_result"] = (int(r["dx"], 16) << 16) | int(r["ax"], 16)
                    if saved_source and at == 0x91:
                        step["working_after_open"] = working_file(run)
                    report["copy_steps"].append(step)
                    record()
                    continue
                elif copy_return is not None and stop["reason"] == "breakpoint" and stop["cs_ip"] == copy_return:
                    if not returned_frame(report["copy_inputs"]["stop"]["registers"], stop["registers"]):
                        raise RuntimeError("copy return belongs to another stack frame")
                    report["copy_return"] = {"stop": stop, "ax": stop["registers"]["ax"],
                        "fields": fields(session.client, delta), "working_after": working_file(run)}
                    if saved_source:
                        report["copy_return"]["source_after"] = named_file(run, "SAVE02.SAV")
                    if observe_reader:
                        if not int(stop["registers"]["ax"], 16) & 0xFF:
                            raise RuntimeError("only successful native copying admits reader preparation")
                        report["copy_archive_state"] = archive_fields(session.client, delta)
                        if observe_origin:
                            session.client.set_breakpoint(open_entry)
                        session.client.set_breakpoint(region_call, once=True)
                        copy_return = None
                        record()
                        continue
                    if observe_load_return:
                        if int(stop["registers"]["ax"], 16) & 0xFF:
                            raise RuntimeError("only the observed copy-rejection branch may continue")
                        segment, root_return = parent_mapping(session.client, delta,
                            report["copy_inputs"]["stop"]["registers"], report["load_call"]["stop"]["registers"])
                        report["parent_mapping"] = {"segment": segment, "return": root_return}
                        load_stops = {f"{segment}:{off:04X}": off for off in
                                      (0x659, 0x661, 0x66F, 0x674, 0x743, 0x748, 0x77D)}
                        for address in (*load_stops, root_return):
                            session.client.set_breakpoint(address, once=True)
                        copy_return = None
                        record()
                        continue
                    report["passed"] = True
                    break
                elif observe_reader and stop["reason"] == "breakpoint" and stop["cs_ip"] == region_call:
                    r = stop["registers"]
                    original = report["copy_inputs"]["stop"]["registers"]
                    if r["ss"] != original["ss"] or r["bp"] != original["bp"]:
                        raise ValueError("region call belongs to another wrapper frame")
                    region_return = declared_return(session.client, r, 0xF26)
                    seg = int(region_return.split(":")[0], 16)
                    if checked_read(session.client, seg, 0xF21, 5) != struct.pack("<BHH", 0x9A, 0xC0, 0x56BD + delta):
                        raise ValueError("region call instruction differs from the declared wrapper")
                    args = list(struct.unpack("<4H", checked_read(session.client, int(r["ss"], 16), int(r["sp"], 16)+4, 8)))
                    current = archive_fields(session.client, delta)
                    if args != [current["region"], 0, 1, 0]:
                        raise ValueError("region-preparation arguments differ from declared call")
                    report["region_entry"] = {"stop": stop, "arguments": args, "archive": current}
                    region_watch = f"{r['ss']}:{int(r['sp'],16)+2:04X}"
                    session.client.set_watchpoint(region_watch, 2)
                    session.client.set_breakpoint(region_return, once=True)
                    record()
                    continue
                elif observe_reader and stop["reason"] == "breakpoint" and stop["cs_ip"] == region_trap:
                    original = report["region_entry"]["stop"]["registers"]
                    if not returned_frame(original, stop["registers"]):
                        raise ValueError("header trap belongs to another returned frame")
                    header = 0x56EF + delta
                    if (checked_read(session.client, header, 0, 2) != bytes([0xCD,0x3F]) or
                            int.from_bytes(checked_read(session.client, header, 2, 2),"little") != 0xF26):
                        raise ValueError("header trap differs from the declared saved region return")
                    report["region_trap"] = stop
                    session.client.remove_watchpoints(region_watch)
                    region_watch = None
                    if checked_read(session.client, 0x4AE5+delta, 0x559, 1) != bytes([0xCF]):
                        raise ValueError("overlay handler IRET differs from declared code")
                    session.client.set_breakpoint(manager_iret)
                    awaiting_region_reload = True
                    record()
                    continue
                elif awaiting_region_reload and stop["reason"] == "breakpoint" and stop["cs_ip"] == manager_iret:
                    relocated = region_iret(session.client, stop["registers"], report["region_entry"]["stop"]["registers"], delta)
                    if relocated is None:
                        report["nested_overlay_returns"] = report.get("nested_overlay_returns",0)+1
                        record()
                        continue
                    report["region_reload"] = {"stop": stop, "return": relocated}
                    session.client.remove_breakpoints(manager_iret)
                    region_return = relocated
                    session.client.set_breakpoint(region_return, once=True)
                    awaiting_region_reload = False
                    record()
                    continue
                elif region_return is not None and stop["reason"] == "breakpoint" and stop["cs_ip"] == region_return:
                    if not returned_frame(report["region_entry"]["stop"]["registers"], stop["registers"]):
                        raise ValueError("region return belongs to another frame")
                    seg = int(region_return.split(":")[0], 16)
                    expected = bytes([0x83,0xC4,8,0x6A,0,0x0E]) + struct.pack("<BH",0xE8,(0xA6C-0xF2F)&0xFFFF)
                    if checked_read(session.client, seg, 0xF26, len(expected)) != expected:
                        raise ValueError("reader near call differs from declared wrapper")
                    report["region_return"] = {"stop": stop, "archive": archive_fields(session.client, delta)}
                    if region_watch is not None:
                        session.client.remove_watchpoints(region_watch)
                    region_watch = None
                    reader_entry = f"{seg:04X}:0A6C"
                    session.client.set_breakpoint(reader_entry, once=True)
                    record()
                    continue
                elif reader_entry is not None and stop["reason"] == "breakpoint" and stop["cs_ip"] == reader_entry:
                    r = stop["registers"]
                    if int(r["ds"],16) != 0x57E0 + delta:
                        raise ValueError("reader DS differs from the named resident data mapping")
                    selector = reader_frame(session.client, r, int(reader_entry.split(":")[0],16))
                    report["reader_entry"] = {"stop": stop, "selector": selector, "archive": archive_fields(session.client, delta)}
                    if observe_reader_return:
                        reader_return = ReaderReturn(session.client, r, delta)
                    if observe_items:
                        item_trace = ItemTrace(session.client,delta,r)
                        report["item_trace"] = item_trace.report
                    if observe_objects:
                        object_trace = ObjectTrace(session.client,delta)
                        report["object_trace"] = object_trace.report
                    if observe_scripts:
                        script_trace = ScriptTrace(session.client,delta)
                        report["script_trace"] = script_trace.report
                    if observe_providers:
                        session.client.remove_breakpoints(global_call)
                        session.client.set_breakpoint(provider_entry)
                        record()
                        continue
                    report["passed"] = True
                    break
                elif observe_load_return and stop["reason"] == "breakpoint" and stop["cs_ip"] in load_stops:
                    r = stop["registers"]
                    original = report["load_call"]["stop"]["registers"]
                    if (r["ss"] != original["ss"] or
                            int(r["bp"], 16) != int(original["sp"], 16) - 2):
                        raise RuntimeError("internal stop belongs to another numbered-load frame")
                    at = load_stops[stop["cs_ip"]]
                    step = {"offset": at, "stop": stop, "ax": r["ax"]}
                    if at in (0x66F, 0x743):
                        step["request"] = settings_request(session.client, r, at)
                    report["load_steps"].append(step)
                    if at == 0x77D:
                        if saved_load_return(session.client, original) != root_return:
                            raise RuntimeError("saved return changed before final-helper watch admission")
                        watched_offset = int(original["sp"], 16) + 2
                        checked_read(session.client, int(original["ss"], 16), watched_offset, 2)
                        return_watch = f"{int(original['ss'], 16):04X}:{watched_offset:04X}"
                        session.client.set_watchpoint(return_watch, 2)
                        report["return_watch"] = return_watch
                    record()
                    continue
                elif (observe_load_return and load_stops and stop["reason"] == "breakpoint"
                      and stop["cs_ip"] == root_return):
                    if not returned_frame(report["load_call"]["stop"]["registers"], stop["registers"]):
                        raise RuntimeError("numbered-load return belongs to another stack frame")
                    seg, off = (int(x, 16) for x in root_return.split(":"))
                    code = checked_read(session.client, seg, off, 2)
                    report["returned_caller_instruction"] = code.hex()
                    if not caller_cleanup_jump(code):
                        raise RuntimeError("returned caller instruction differs from the declared cleanup jump")
                    report["load_return"] = {"stop": stop, "ax": stop["registers"]["ax"],
                        "fields": fields(session.client, delta), "state": snapshot(session.client, delta)}
                    report["passed"] = True
                    break
                elif (awaiting_return and stop["reason"] == "breakpoint"
                      and stop["cs_ip"] == rng.address(0x39D1, 0x083F)):
                    if not returned_frame(report["callback_entry"]["stop"]["registers"], stop["registers"]):
                        raise RuntimeError("return belongs to a different callback stack frame")
                    ui = fields(session.client, delta)
                    report["callback_return"] = {"stop": stop, "fields": ui,
                        "markers": list_markers(session.client, ui),
                        "state": snapshot(session.client, delta), "ax": stop["registers"]["ax"]}
                    delivery = observe_delivery(session, key, delivery)
                    report["navigation_input"] = {"key": "down", "state": delivery.state, "detail": delivery.detail}
                    report["passed"] = True
                    break
                elif stop["reason"] != "time":
                    raise RuntimeError("unexpected stop; no continuation admitted")
                delivery = observe_delivery(session, key, delivery)
                report["input" if key_name == "f2" else "navigation_input"] = {"key": key_name, "state": delivery.state, "detail": delivery.detail}
                current = {"state": snapshot(session.client, delta), "fields": fields(session.client, delta)}
                if current != previous:
                    report["observations"].append(current)
                report["polls"] = index + 1
                record()
                if key_name == "f2" and delivery.state == "done" and current == previous and load_ready(current["state"], current["fields"]):
                    (run / "load.png").write_bytes(session.client.screenshot())
                    report["checkpoint"] = checkpoint_summary(session.save_state("load-entry", check_files=True))
                    if not navigation:
                        report["passed"] = True
                        break
                    if current["fields"]["global_handler"] != callback:
                        raise RuntimeError("Load hook differs from the declared callback")
                    session.client.remove_breakpoints(dispatch)
                    session.client.set_breakpoint(callback)
                    if observe_load:
                        root, root_return = load_root(session.client, callback)
                        session.client.set_breakpoint(root)
                    key_name = "enter" if observe_load else "down"
                    key = session.send_input("key", {"key": key_name, "hold_ms": 100})
                    delivery = None
                    previous = None
                    record()
                    continue
                if not delivery.pending and delivery.state != "done":
                    raise RuntimeError("key delivery failed")
                previous = current
            else:
                raise RuntimeError("Load-entry state budget exhausted")
        except Exception as error:
            report["error"] = str(error)
            raise
        finally:
            record()
        print(json.dumps({"passed": report["passed"], "dispatches": len(report["dispatches"]),
                          "draws": len(report["draws"])}), flush=True)


if __name__ == "__main__":
    main()
