"""Fixed SOUND_DS.EXE parser capability attempt; Q-CONFIG-006/011, Q-SOUND-010.

Only two fingerprinted licensed files enter a private session drive. No
arguments, input, guest writes, screenshots, driver use or gameplay is admitted.
Reports are original-derived and remain beneath the approved GAME_DIR.
"""
import hashlib
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
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "exec-census"))
from setup_record_projection import project_source, RECORD_SIZE
INPUTS = {"SOUND_DS.EXE": (204593, "236c2dc23c071eca421eb5b427caee57"),
          "SOUND.INI": (54669, "aecc7469098e0c188e0e3d056c5fd530")}
# FND-SOUND-024/033 and FND-EXE-491; no arbitrary address or target input.
PHASES = {"parser": (0x1AF6, 0x0001), "returned": (0x1ABD, 0x0021),
          "labels": (0x1ABD, 0x0092), "chooser": (0x19CF, 0x009A),
          "rows": (0x19CF, 0x025B)}
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


def observe_records(client, stop, loaded, projection, phase="returned"):
    """Only named group headers/payloads at the existing successful return stop."""
    if phase not in ("returned", "labels"):
        raise ValueError("unsupported setup record phase")
    fields = observe_fields(client, stop, loaded, phase)
    names = {"main", "digital", "music_only"}
    if ((phase == "returned" and fields["ax"] != 1) or set(projection["counts"]) != names
            or set(projection["payloads"]) != names or fields["counts"] != projection["counts"]):
        raise ValueError("setup records need successful source-matching counts")
    registers = stop["registers"]
    ss, sp = (int(registers.get(key, "-1"), 16) for key in ("ss", "sp"))
    if not 0 <= ss <= 0xFFFF or not 0 <= sp <= 0xFFF0 or ss * 16 + sp + 16 > 0xA0000:
        raise ValueError("setup record observation has an unsupported stack window")
    floor = max((loaded + 0x1E36 - 0x1000) * 16 + 0x10000, ss * 16 + sp + 16)
    blocks = {}
    for name in ("main", "digital", "music_only"):
        count = fields["counts"][name]
        if type(count) is not int or count != -1 and not 1 <= count <= 16:
            raise ValueError("setup record count outside bounded profile")
        payload = projection["payloads"][name]
        if type(payload) is not bytes or len(payload) != max(0, count) * RECORD_SIZE:
            raise ValueError("setup projected record extent mismatch")
        pointer = fields["groups"][name]
        segment, offset = pointer["segment"], pointer["offset"]
        if type(segment) is not int or segment <= 0 or offset != 4:
            raise ValueError("setup record pointer outside offset-four profile")
        request = count * RECORD_SIZE & 0xFFFF
        paragraphs = (request + 19) // 16
        begin, end = segment * 16, segment * 16 + paragraphs * 16
        if begin < floor or end > 0xA0000 or request + 4 > 0x10000:
            raise ValueError("setup group extent crosses the observation bounds")
        if any(begin < block["end"] and block["begin"] < end for block in blocks.values()):
            raise ValueError("setup group allocations overlap")
        blocks[name] = {"segment": segment, "begin": begin, "end": end,
                        "paragraphs": paragraphs, "request": request}
    # Check every header before reading any payload. Never follow word two.
    for name, block in blocks.items():
        paragraphs = struct.unpack("<H", checked_read(client, block["segment"], 0, 2))[0]
        if paragraphs != block["paragraphs"]:
            raise ValueError(f"setup allocation size word differs for {name}: {paragraphs}")
    summaries, native = {}, {}
    for name, block in blocks.items():
        expected = projection["payloads"][name]
        summary = {"count": fields["counts"][name], "header_paragraphs": block["paragraphs"],
                   "modeled_payload_capacity": block["paragraphs"] * 16 - 4,
                   "compared_bytes": 0, "equal": None}
        if expected:
            if len(expected) > 4096:
                raise ValueError("setup populated record observation exceeds cap")
            actual = client.read_memory(f"{block['segment']:04X}:0004", len(expected)).data
            if len(actual) != len(expected):
                raise ValueError("short setup record payload read")
            native[name] = actual
            differing = [index for index, (a, b) in enumerate(zip(actual, expected)) if a != b]
            summary.update({"compared_bytes": len(expected), "equal": not differing,
                            "expected_sha256": hashlib.sha256(expected).hexdigest(),
                            "native_sha256": hashlib.sha256(actual).hexdigest(),
                            "differing_bytes": len(differing), "first_differing_offsets": differing[:32]})
        summaries[name] = summary
    if observe_fields(client, stop, loaded, phase) != fields:
        raise ValueError("setup named fields changed during paused record observation")
    return {"groups": summaries, "all_populated_equal": all(
                row["equal"] for row in summaries.values() if row["compared_bytes"])}, native


def expected_labels(projection):
    """Documented short printable names, excluding every slot's unwritten tail."""
    result = {}
    if (set(projection["counts"]) != {"main", "digital", "music_only"}
            or set(projection["payloads"]) != set(projection["counts"])):
        raise ValueError("unsupported label groups")
    for name, count in projection["counts"].items():
        if type(count) is not int or count != -1 and not 1 <= count <= 16:
            raise ValueError("unsupported label count")
        body = projection["payloads"][name]
        if type(body) is not bytes or len(body) != max(0, count) * RECORD_SIZE:
            raise ValueError("label projection size differs")
        rows = []
        for index in range(max(0, count)):
            field = body[index * RECORD_SIZE:index * RECORD_SIZE + 40]
            end = field.find(b"\0")
            if end < 0 or any(not 32 <= value <= 126 for value in field[:end]):
                raise ValueError("label name outside terminated printable profile")
            rows.append(field[:end] + b"\0")
        result[name] = rows
    return result


def observe_labels(client, stop, loaded, projection, baseline):
    """Only the fixed post-label stop; six modeled blocks, header-first reads."""
    expected = expected_labels(projection)
    fields = observe_fields(client, stop, loaded, "labels")
    keys = ("counts", "groups", "mode")
    if any(fields[key] != baseline[key] for key in keys):
        raise ValueError("setup record bindings changed before labels")
    records, record_native = observe_records(client, stop, loaded, projection, "labels")
    ds = loaded + 0x1E36 - 0x1000
    raw = checked_read(client, ds, 0x1898, 12)
    words = struct.unpack("<6H", raw)
    pointers = {name: (words[index], words[index + 1]) for name, index in
                (("digital", 0), ("music_only", 2), ("main", 4))}
    ss, sp = (int(stop["registers"][key], 16) for key in ("ss", "sp"))
    floor = max(ds * 16 + 0x10000, ss * 16 + sp + 16)
    blocks = []
    for name, pointer in fields["groups"].items():
        request = fields["counts"][name] * RECORD_SIZE & 0xFFFF
        start = pointer["segment"] * 16
        blocks.append((start, start + ((request + 19) // 16) * 16))
    labels = {}
    for name, (offset, segment) in pointers.items():
        request = fields["counts"][name] * 80 & 0xFFFF
        paragraphs = (request + 19) // 16
        begin, end = segment * 16, segment * 16 + paragraphs * 16
        if (offset != 4 or segment <= 0 or begin < floor or end > 0xA0000
                or request + 4 > 0x10000
                or any(begin < high and low < end for low, high in blocks)):
            raise ValueError("unsupported or overlapping label extent")
        blocks.append((begin, end))
        labels[name] = {"offset": offset, "segment": segment, "paragraphs": paragraphs}
    for name, block in labels.items():
        value = struct.unpack("<H", checked_read(client, block["segment"], 0, 2))[0]
        if value != block["paragraphs"]:
            raise ValueError(f"label header differs for {name}")
    summaries, payloads = {}, {}
    total = 0
    for name, rows in expected.items():
        wanted, native, differences = b"".join(rows), bytearray(), []
        for index, row in enumerate(rows):
            total += len(row)
            if len(row) > 41 or total > 1312:
                raise ValueError("label prefix observation exceeds cap")
            offset = 4 + index * 80
            data = client.read_memory(f"{labels[name]['segment']:04X}:{offset:04X}", len(row)).data
            if len(data) != len(row):
                raise ValueError("short label prefix read")
            differences.extend(index * 80 + byte for byte, (a, b) in enumerate(zip(data, row)) if a != b)
            native.extend(data)
        summaries[name] = {"count": fields["counts"][name], "header_paragraphs": labels[name]["paragraphs"],
                           "pointer": {"offset": labels[name]["offset"], "segment": labels[name]["segment"]},
                           "compared_bytes": len(wanted), "equal": native == wanted if rows else None,
                           "differing_bytes": len(differences), "first_differing_offsets": differences[:32]}
        if rows:
            summaries[name].update(expected_sha256=hashlib.sha256(wanted).hexdigest(),
                                   native_sha256=hashlib.sha256(native).hexdigest())
            payloads[name] = (wanted, bytes(native))
    if (checked_read(client, ds, 0x1898, 12) != raw
            or observe_fields(client, stop, loaded, "labels") != fields):
        raise ValueError("setup label bindings changed during paused observation")
    return {"records": records, "bindings": fields, "groups": summaries,
            "all_populated_equal": records["all_populated_equal"] and all(
                row["equal"] for row in summaries.values() if row["compared_bytes"])}, payloads, record_native


def word_register(stop, name):
    value = int(stop.get("registers", {}).get(name, "-1"), 16)
    if not 0 <= value <= 0xFFFF:
        raise ValueError("setup needs declared word registers")
    return value


def signed_word(value):
    return value - 0x10000 if value >= 0x8000 else value


def observe_label_display(client, stop, loaded, baseline):
    """Fixed post-label state only; no pointer following or continuation."""
    fields = observe_fields(client, stop, loaded, "labels")
    if fields != baseline:
        raise ValueError("post-label display needs matching named bindings")
    ds = loaded + 0x1E36 - 0x1000
    spans = ((ds, 0xDEB8, 5), (ds, 0xDEBE, 11), (0, 0, 4))
    first = tuple(checked_read(client, *span) for span in spans)
    if (tuple(checked_read(client, *span) for span in spans) != first
            or observe_fields(client, stop, loaded, "labels") != fields):
        raise ValueError("post-label display changed while paused")
    mode, rows, columns, classification, sync, base, video, enabled = struct.unpack("<5B3H", first[1])
    offset, segment = struct.unpack("<HH", first[2])
    return {"vector_zero": {"offset": offset, "segment": segment,
                            "matches_declared_handler": (offset, segment) == (0x0174, loaded)},
            "display": {"mode": mode, "rows": rows, "columns": columns,
                        "classification": classification, "copy_sync": sync, "base_word": base,
                        "video_segment": video, "direct_enabled": enabled,
                        "window": dict(zip(("left", "top", "right", "bottom", "attribute"), first[0]))}}


def observe_chooser(client, stop, loaded, projection, baseline, main_labels):
    """Fixed MainGroup wrapper entry only, before the chooser makes a frame."""
    fields = observe_fields(client, stop, loaded, "chooser")
    if any(fields[key] != baseline[key] for key in ("counts", "groups", "mode")):
        raise ValueError("chooser record bindings differ")
    ss, sp, bp = (word_register(stop, key) for key in ("ss", "sp", "bp"))
    if (bp < 0x400 or sp < 0x1DB4 or bp + 18 > 0x10000
            or ss * 16 + bp + 18 > 0xA0000):
        raise ValueError("unsupported chooser entry frame span")
    frame = checked_read(client, ss, sp, 14)
    ip, cs, initial, title_offset, title_segment, text_offset, text_segment = struct.unpack("<7H", frame)
    wrapper = checked_read(client, ss, bp + 2, 16)
    caller_ip, caller_cs, count, label_offset, label_segment, toff, tseg, index = struct.unpack("<8H", wrapper)
    wanted = b"".join(row[:-1] + b"\n" for row in expected_labels(projection)["main"]) + b"\0"
    if (f"{cs:04X}:{ip:04X}" != address(loaded, 0x19CF, 0x008F)
            or f"{caller_cs:04X}:{caller_ip:04X}" != address(loaded, 0x1429, 0x0296)
            or count != projection["counts"]["main"]
            or {"offset": label_offset, "segment": label_segment} != main_labels
            or (toff, tseg, index) != (title_offset, title_segment, initial)
            or signed_word(initial) >= count or text_segment != ss
            or text_offset != bp - 0x400 or sp + 14 > text_offset
            or len(wanted) > 1024 or text_offset + len(wanted) > bp):
        raise ValueError("unsupported chooser caller or source binding")
    actual = client.read_memory(f"{ss:04X}:{text_offset:04X}", len(wanted)).data
    if len(actual) != len(wanted):
        raise ValueError("short chooser text read")
    if (checked_read(client, ss, sp, 14) != frame
            or checked_read(client, ss, bp + 2, 16) != wrapper
            or observe_fields(client, stop, loaded, "chooser") != fields):
        raise ValueError("chooser entry bindings changed while paused")
    differences = [i for i, (a, b) in enumerate(zip(actual, wanted)) if a != b]
    return {"bindings": fields, "ss": ss, "entry_sp": sp, "initial_index": signed_word(initial),
            "source_count": count, "compared_bytes": len(wanted), "equal": not differences,
            "expected_sha256": hashlib.sha256(wanted).hexdigest(),
            "native_sha256": hashlib.sha256(actual).hexdigest(),
            "differing_bytes": len(differences), "first_differing_offsets": differences[:32]}, (wanted, actual)


def observe_rows(client, stop, loaded, entry):
    """Fixed post-adjustment row endpoint; never follow vector/video pointers."""
    fields = observe_fields(client, stop, loaded, "rows")
    ss, sp, bp = (word_register(stop, key) for key in ("ss", "sp", "bp"))
    if (ss != entry["ss"] or bp != entry["entry_sp"] - 2 or bp < 0x1DB2
            or sp != bp - 0x1DB2
            or any(fields[key] != entry["bindings"][key] for key in ("counts", "groups", "mode"))):
        raise ValueError("unsupported chooser row frame or binding")
    ds = loaded + 0x1E36 - 0x1000
    spans = ((ss, bp - 0x1C, 4), (ds, 0xDEB8, 5), (ds, 0xDEBE, 11), (0, 0, 4))
    first = tuple(checked_read(client, *span) for span in spans)
    if (tuple(checked_read(client, *span) for span in spans) != first
            or observe_fields(client, stop, loaded, "rows") != fields):
        raise ValueError("chooser row fields changed while paused")
    adjusted, iteration = struct.unpack("<hh", first[0])
    mode, rows, columns, classification, sync, base, video, enabled = struct.unpack("<5B3H", first[2])
    offset, segment = struct.unpack("<HH", first[3])
    count = signed_word(word_register(stop, "si"))
    index = signed_word(word_register(stop, "di"))
    return {"row_count": count, "adjusted_count": adjusted, "comparison_iterations": iteration,
            "source_count": entry["source_count"], "matches_source_count": count == entry["source_count"],
            "current_index": index, "index_matches_entry": index == max(0, entry["initial_index"]),
            "vector_zero": {"offset": offset, "segment": segment,
                            "matches_declared_handler": (offset, segment) == (0x0174, loaded)},
            "display": {"mode": mode, "rows": rows, "columns": columns,
                        "classification": classification, "copy_sync": sync, "base_word": base,
                        "video_segment": video, "direct_enabled": enabled,
                        "window": dict(zip(("left", "top", "right", "bottom", "attribute"), first[1]))}}


def observe_notice_return(client, stop, loaded):
    """FND-EXE-483 entry only: four stack bytes, no pointer following."""
    if (stop.get("reason") != "breakpoint"
            or stop.get("cs_ip") != address(loaded, 0x190F, 0)
            or word_register(stop, "ds") != loaded + 0x1E36 - 0x1000):
        raise ValueError("notice return needs declared stop and DS")
    ss, sp = (word_register(stop, key) for key in ("ss", "sp"))
    linear = ss * 16 + sp
    if sp + 4 > 0x10000 or linear < (loaded + 0x1E36 - 0x1000) * 16 + 0xECCE or linear + 4 > 0xA0000:
        raise ValueError("unsupported notice entry stack")
    frame = checked_read(client, ss, sp, 4)
    if checked_read(client, ss, sp, 4) != frame:
        raise ValueError("notice return changed while paused")
    ip, cs = struct.unpack("<HH", frame)
    canonical = cs - loaded + 0x1000
    return {"ss": ss, "sp": sp, "return_ip": ip, "return_cs": cs,
            "canonical_return_segment": canonical if 0 <= canonical <= 0xFFFF else None}


def observe_notice_objects(client, stop, loaded):
    """Only SOUND-102's two fixed objects; never resume the notice."""
    entry = observe_notice_return(client, stop, loaded)
    if (entry["canonical_return_segment"], entry["return_ip"]) != (0x164C, 0x050D):
        raise ValueError("unsupported notice object caller")
    ds = loaded + 0x1E36 - 0x1000
    ss, sp = entry["ss"], entry["sp"]
    frame = checked_read(client, ss, sp, 12)
    expected = (entry["return_ip"], entry["return_cs"], 0xAF36, ds, 0xD244, ds)
    if struct.unpack("<6H", frame) != expected:
        raise ValueError("unsupported notice object arguments")
    selector = checked_read(client, ds, 0xAF36, 2)
    style = checked_read(client, ds, 0xD244, 12)
    payloads = {}
    summaries = {}
    for name, start, cap in (("body", 0xAF38, 512), ("choices", 0xB138, 128)):
        body = bytearray()
        for index in range(cap):
            value = checked_read(client, ds, start + index, 1)[0]
            body.append(value)
            if value == 0:
                break
            if value != 10 and not 32 <= value <= 126:
                raise ValueError("unsupported notice text byte")
        else:
            raise ValueError("notice text exceeds observation cap")
        for index, value in enumerate(body):
            if checked_read(client, ds, start + index, 1)[0] != value:
                raise ValueError("notice prefix changed while paused")
        payloads[name] = bytes(body)
        summaries[name] = {"length": len(body) - 1, "lf_count": body.count(10),
                           "row_lengths": [len(row) for row in body[:-1].split(bytes([10]))],
                           "sha256": hashlib.sha256(body).hexdigest()}
    if (checked_read(client, ds, 0xAF36, 2) != selector
            or checked_read(client, ds, 0xD244, 12) != style
            or checked_read(client, ss, sp, 12) != frame
            or observe_notice_return(client, stop, loaded) != entry):
        raise ValueError("notice object bindings changed while paused")
    return {"entry": entry, "selector": struct.unpack("<H", selector)[0],
            "style_words": list(struct.unpack("<6H", style)), "text": summaries}, payloads


def require_notice_profile(objects):
    if (objects["selector"] != 0 or objects["style_words"] != [15, 1, 7, 0, 15, 2]
            or [(objects["text"][name]["length"], objects["text"][name]["lf_count"])
                for name in ("body", "choices")] != [(99, 2), (8, 0)]):
        raise ValueError("unsupported notice continuation profile")


def observe_notice_poll(client, stop, loaded, objects, texts):
    """EXE-485/486 fixed pre-input endpoint, separately bound wrapped locals."""
    require_notice_profile(objects)
    entry = objects["entry"]
    ds = loaded + 0x1E36 - 0x1000
    ss, sp, bp = (word_register(stop, key) for key in ("ss", "sp", "bp"))
    if (stop.get("reason") != "breakpoint" or stop.get("cs_ip") != address(loaded, 0x190F, 0x054B)
            or word_register(stop, "ds") != ds or ss != entry["ss"]
            or bp != (entry["sp"] - 2) & 0xFFFF or sp != (bp - 0x176A) & 0xFFFF):
        raise ValueError("unsupported notice poll stop or frame")
    names = (("selector", 4), ("body_height", 6), ("choice_length_sum", 10),
             ("spacing_numerator", 20), ("spacing", 22), ("poll_value", 28),
             ("registered_rows", 30), ("body_rows", 32), ("choice_rows", 34), ("accept_flag", 36))
    spans = [(ss, bp + 2, 12)] + [(ss, (bp - delta) & 0xFFFF, 2) for _, delta in names]
    spans += [(ss, (bp - 0x1766) & 0xFFFF, 12), (ds, 0x18F6, 4),
              (ds, 0xAF36, 2), (ds, 0xD244, 12)]
    for start, size in ((0xAF38, 100), (0xB138, 9)):
        spans.extend((ds, start + i, min(16, size - i)) for i in range(0, size, 16))
    first = [checked_read(client, *span) for span in spans]
    if [checked_read(client, *span) for span in spans] != first:
        raise ValueError("notice poll fields changed while paused")
    expected_frame = (entry["return_ip"], entry["return_cs"], 0xAF36, ds, 0xD244, ds)
    if struct.unpack("<6H", first[0]) != expected_frame:
        raise ValueError("notice poll caller arguments changed")
    body = b"".join(first[15:22])
    choices = first[22]
    if set(texts) != {"body", "choices"} or (len(texts["body"]), len(texts["choices"])) != (100, 9):
        raise ValueError("unsupported notice entry prefixes")
    return {"ss": ss, "sp": sp, "bp": bp,
            "locals": {name: struct.unpack("<H", first[i + 1])[0] for i, (name, _) in enumerate(names)},
            "row_words": list(struct.unpack("<6H", first[11])),
            "input_mode": struct.unpack("<H", first[12][:2])[0],
            "registration_count": struct.unpack("<H", first[12][2:])[0],
            "selector_retained": struct.unpack("<H", first[13])[0] == objects["selector"],
            "style_retained": list(struct.unpack("<6H", first[14])) == objects["style_words"],
            "prefixes_retained": body == texts["body"] and choices == texts["choices"]}, {"body": body, "choices": choices}


def verified_input(path, identity):
    size, digest = identity
    with path.open("rb") as stream:
        body = stream.read(size + 1)
    if (len(body), xxhash.xxh3_128_hexdigest(body)) != identity:
        raise ValueError("setup input differs from the fixed licensed identity")
    return body


def main():
    if (len(sys.argv) not in (2, 3) or len(sys.argv) == 3 and sys.argv[2] not in ("--labels", "--chooser-rows", "--notice-objects", "--notice-poll")
            or not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", sys.argv[1])):
        raise SystemExit("usage: setup_parser.py <new-run-name> [--labels|--chooser-rows|--notice-objects|--notice-poll]")
    labels_profile = len(sys.argv) == 3
    rows_profile = labels_profile and sys.argv[2] in ("--chooser-rows", "--notice-objects", "--notice-poll")
    objects_profile = labels_profile and sys.argv[2] in ("--notice-objects", "--notice-poll")
    poll_profile = labels_profile and sys.argv[2] == "--notice-poll"
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                              Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    inputs = {name: verified_input(game / name, identity) for name, identity in INPUTS.items()}
    projection = project_source(inputs["SOUND.INI"].decode("ascii"))
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
    if labels_profile:
        report["limits"] = "fixed parser and post-label endpoint only; no input, guest writes, frames, producers, drivers or gameplay"
    if rows_profile:
        report["limits"] = "fixed MainGroup chooser row endpoint; notice/driver guards; no input, guest writes, frames, event loop or gameplay"
    with RustDosSession(settings) as session:
        entry = session.entry
        report["entry"] = entry
        if entry.get("cs_ip", "").split(":")[-1] != "0000":
            raise ValueError("unexpected setup entry offset")
        loaded = int(entry["registers"]["cs"], 16)
        for segment, offset in GUARDS:
            session.client.set_breakpoint(address(loaded, segment, offset))
        if labels_profile:
            for offset in (0x0052, 0x006D):
                session.client.set_breakpoint(address(loaded, 0x1ABD, offset))
        for phase in ("parser", "returned"):
            operation = session.continue_(until=address(loaded, *PHASES[phase]))
            observed = wait_same_operation(session, operation)
            report["last_stop"] = observed.stop
            (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
            fields = observe_fields(session.client, observed.stop, loaded, phase)
            report["phases"].append(fields)
            (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
            print(json.dumps(fields), flush=True)
        summary, native = observe_records(session.client, observed.stop, loaded, projection)
        report["record_comparison"] = summary
        for name, body in native.items():
            (run / f"{name}-native.bin").write_bytes(body)
            (run / f"{name}-expected.bin").write_bytes(projection["payloads"][name])
        (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        print(json.dumps(summary), flush=True)
        if not summary["all_populated_equal"]:
            raise RuntimeError("setup record contents differ; local payloads and report retained")
        if labels_profile:
            operation = session.continue_(until=address(loaded, *PHASES["labels"]))
            observed = wait_same_operation(session, operation)
            report["last_stop"] = observed.stop
            (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
            labels, payloads, records = observe_labels(session.client, observed.stop, loaded, projection, report["phases"][-1])
            report["label_comparison"] = labels
            for name, body in records.items():
                (run / f"{name}-later-records-native.bin").write_bytes(body)
                (run / f"{name}-later-records-expected.bin").write_bytes(projection["payloads"][name])
            for name, (wanted, actual) in payloads.items():
                (run / f"{name}-labels-expected.bin").write_bytes(wanted)
                (run / f"{name}-labels-native.bin").write_bytes(actual)
            (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
            print(json.dumps(labels), flush=True)
            if not labels["all_populated_equal"]:
                raise RuntimeError("setup labels differ; local prefixes and report retained")
            report["post_label_display"] = observe_label_display(session.client, observed.stop, loaded, labels["bindings"])
            (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
            print(json.dumps(report["post_label_display"]), flush=True)
        if rows_profile:
            session.client.set_breakpoint(address(loaded, 0x190F, 0x0000))
            operation = session.continue_(until=address(loaded, *PHASES["chooser"]))
            observed = wait_same_operation(session, operation)
            report["last_stop"] = observed.stop
            (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
            if observed.stop.get("cs_ip") == address(loaded, 0x190F, 0):
                report["notice_return"] = observe_notice_return(session.client, observed.stop, loaded)
                if objects_profile:
                    report["notice_objects"], texts = observe_notice_objects(session.client, observed.stop, loaded)
                    for name, body in texts.items():
                        (run / f"notice-{name}-native.bin").write_bytes(body)
                (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
                print(json.dumps({"notice_return": report["notice_return"]}), flush=True)
                if objects_profile:
                    print(json.dumps({"notice_objects": report["notice_objects"]}), flush=True)
                if poll_profile:
                    require_notice_profile(report["notice_objects"])
                    for offset in (0x0724, 0x02C6):
                        session.client.set_breakpoint(address(loaded, 0x190F, offset))
                    session.client.remove_breakpoints(address(loaded, 0x190F, 0))
                    operation = session.continue_(until=address(loaded, 0x190F, 0x054B))
                    observed = wait_same_operation(session, operation)
                    report["last_stop"] = observed.stop
                    (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
                    report["notice_poll"], later = observe_notice_poll(session.client, observed.stop, loaded,
                                                                       report["notice_objects"], texts)
                    for name, body in later.items():
                        (run / f"notice-{name}-poll-native.bin").write_bytes(body)
                    (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
                    print(json.dumps({"notice_poll": report["notice_poll"]}), flush=True)
                return
            if objects_profile:
                raise ValueError("notice object profile did not reach its fixed guard")
            chooser, (wanted, actual) = observe_chooser(session.client, observed.stop, loaded, projection,
                                                     labels["bindings"], labels["groups"]["main"]["pointer"])
            report["chooser_entry"] = chooser
            (run / "chooser-text-expected.bin").write_bytes(wanted)
            (run / "chooser-text-native.bin").write_bytes(actual)
            (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
            if not chooser["equal"]:
                raise RuntimeError("chooser text differs; prefixes and report retained")
            operation = session.continue_(until=address(loaded, *PHASES["rows"]))
            observed = wait_same_operation(session, operation)
            report["last_stop"] = observed.stop
            (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
            report["chooser_rows"] = observe_rows(session.client, observed.stop, loaded, chooser)
            (run / "setup-parser.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
            print(json.dumps(report["chooser_rows"]), flush=True)
        print("setup parser boundary observed; closing owned session", flush=True)


if __name__ == "__main__":
    main()
