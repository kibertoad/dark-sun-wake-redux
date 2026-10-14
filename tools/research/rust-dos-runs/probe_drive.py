"""Prepare a manifest-bounded private DSUN drive; never traverse the installation."""
import json
from pathlib import Path, PurePosixPath

import xxhash

ROOT = Path(__file__).resolve().parents[3]
MANIFEST = ROOT / "src/DarkSunWakeRedux.Extractor/source-manifests/gog-en-52095422060333615.json"
MAX_FILE = 128 * 1024 * 1024


def prepare_drive(source, destination, manifest=MANIFEST):
    source, destination = Path(source).resolve(), Path(destination).resolve()
    if not destination.is_dir() or any(destination.iterdir()):
        raise ValueError("probe C: destination must be an existing empty directory")
    document = json.loads(Path(manifest).read_text(encoding="utf-8-sig"))
    rows = document.get("files")
    if not isinstance(rows, list) or not 1 <= len(rows) <= 4096:
        raise ValueError("invalid probe source manifest file count")
    selected, names, total = [], set(), 0
    for row in rows:
        name = row.get("path", "")
        path = PurePosixPath(name)
        if (not name or path.is_absolute() or str(path) != name
                or any(part in (".", "..") or ":" in part or "\\" in part
                       for part in path.parts)):
            raise ValueError("noncanonical manifest path")
        if name.casefold() in names:
            raise ValueError("duplicate manifest path")
        names.add(name.casefold())
        if name.casefold() in ("game.gog", "game.ins") or path.parts[0].casefold() == "music":
            continue
        size, digest = row.get("size"), row.get("xxh3")
        if type(size) is not int or not 0 <= size <= MAX_FILE:
            raise ValueError(f"invalid manifest size: {name}")
        total += size
        if total > 512 * 1024 * 1024:
            raise ValueError("probe source exceeds bounded total size")
        if not isinstance(digest, str) or len(digest) != 32 or any(c not in "0123456789abcdef" for c in digest):
            raise ValueError(f"invalid manifest fingerprint: {name}")
        candidate = source.joinpath(*path.parts)
        for parent in (candidate, *candidate.parents):
            if parent == source:
                break
            if parent.is_symlink() or parent.is_junction():
                raise ValueError(f"linked source path: {name}")
        if not candidate.is_file() or candidate.stat().st_size != size:
            raise ValueError(f"missing or changed probe source: {name}")
        with candidate.open("rb") as stream:
            body = stream.read(size + 1)
        if len(body) != size or xxhash.xxh3_128_hexdigest(body) != digest:
            raise ValueError(f"changed probe source fingerprint: {name}")
        selected.append((path, body))
    # All fingerprints checked before writing any destination file. Retaining
    # verified bytes also avoids copying a changed source after validation.
    for path, body in selected:
        target = destination.joinpath(*path.parts)
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(body)
    return len(selected)
