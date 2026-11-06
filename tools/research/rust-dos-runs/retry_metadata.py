"""Bounded fixed working-file metadata summaries for Q-SAVE-008."""
import hashlib
import struct

from regenerate_checkpoints import checked_read
from private_payload_lock import LIMIT
from save_failure_fields import record


def source_record(meta):
    if (meta.get("retry") is not True or meta.get("passed") is not True or meta.get("writes")!=[]
            or "lock" in meta.get("fault",{})
            or meta.get("fault",{}).get("write_result",{}).get("carry") is not False
            or meta.get("resource_result",{}).get("ax")!="0000"
            or meta.get("numbered_result",{}).get("success_byte")!=1
            or meta.get("copy_entry",{}).get("ownership")!=0
            or meta.get("checkpoint",{}).get("name")!="retry-map"
            or meta.get("checkpoint",{}).get("checked") is not True
            or meta.get("files_after",{}).get("SAVE02.SAV",{}).get("exists") is not True):
        raise ValueError("retry source lacks successful unseeded ownership-zero copy")
    original=meta["fault"]["record"]
    current=meta["post_failed_map"]["record"]
    if current.get("dirty")!=1 or any(current.get(k)!=original.get(k)
            for k in ("pointer","io_handle","options","flags","handle")):
        raise ValueError("retry source lost its same pending working record")
    return current


def summary(header, directory):
    if len(header) != 28 or header[:4] != b"GFFI":
        raise ValueError("unsupported GFF header")
    position, length = struct.unpack_from("<II", header, 12)
    if length != len(directory) or position < 28 or position + length > LIMIT:
        raise ValueError("unsupported GFF directory extent")
    return {"directory_position": position, "directory_length": length,
            "header_sha256": hashlib.sha256(header).hexdigest(),
            "directory_sha256": hashlib.sha256(directory).hexdigest()}


def disk(run, name):
    if name not in ("DARKRUN.GFF", "SAVE02.SAV"):
        raise ValueError("unsupported retry metadata file")
    drive, path = run / "drive-c", run / "drive-c" / name
    if any(p.is_symlink() or p.is_junction() for p in (run.parent,run,drive,path)):
        raise ValueError("retry metadata path contains a link")
    if not path.is_file() or not 28 <= path.stat().st_size <= LIMIT:
        raise ValueError("unsupported retry metadata file size")
    with path.open("rb") as stream:
        header = stream.read(28)
        position, length = struct.unpack_from("<II",header,12)
        if position < 28 or position+length > path.stat().st_size or length > 65535:
            raise ValueError("retry disk directory is outside its bound")
        stream.seek(position)
        directory = stream.read(length)
    return summary(header,directory)


def memory(client,pointer,expected):
    state=record(client,pointer)
    if any(state[k]!=expected[k] for k in ("pointer","io_handle","options","flags","handle")):
        raise ValueError("retry working record changed identity")
    seg,off=(int(x,16) for x in pointer.split(":"))
    header=checked_read(client,seg,off+0x10,28)
    length=struct.unpack_from("<I",header,16)[0]
    if not 0 < length <= 65535 or off+0x31+length > 65536:
        raise ValueError("retry memory directory is outside its bound")
    directory=b"".join(checked_read(client,seg,off+0x31+start,min(261,length-start))
                       for start in range(0,length,261))
    return {"record":state,"metadata":summary(header,directory)}
