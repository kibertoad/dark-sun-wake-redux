"""Read-only Copy driver directory census; Q-SOUND-007, FND-EXE-491/496.

Usage: python -I disc_adv_envelopes.py INSTALL_DIR BUILD_MANIFEST
Reuses the existing ISO collector; prints metadata only, never driver bytes.
"""
from contextlib import redirect_stdout
import io
from pathlib import Path
import struct
import sys


def envelope(body):
    if not isinstance(body, bytes) or not 8 <= len(body) <= 65536:
        raise ValueError("ADV envelope: unsupported file size")
    if body[2:6] != b"Copy":
        raise ValueError("ADV envelope: Copy signature missing")
    directory = struct.unpack_from("<H", body)[0]
    if not 6 <= directory <= len(body) - 2:
        raise ValueError("ADV envelope: directory outside file")
    at, rows, seen = directory, [], set()
    while at + 2 <= len(body):
        number = struct.unpack_from("<H", body, at)[0]
        if number == 0xFFFF:
            return {"bytes": len(body), "directory": directory,
                    "directory_bytes": at + 2 - directory, "functions": len(rows),
                    "unique_targets": len({row["target"] for row in rows}),
                    "rows": rows, "terminal": at,
                    "limits": "Copy directory only; remaining bytes and driver behavior opaque"}
        if len(rows) >= 256 or at + 4 > len(body):
            raise ValueError("ADV envelope: entry limit or truncated pair")
        target = struct.unpack_from("<H", body, at + 2)[0]
        if number in seen or target >= len(body):
            raise ValueError("ADV envelope: duplicate function or target outside file")
        seen.add(number)
        rows.append({"number": number, "target": target})
        at += 4
    raise ValueError("ADV envelope: missing terminal number")


def main():
    if len(sys.argv) != 3:
        raise SystemExit("usage: disc_adv_envelopes.py INSTALL_DIR BUILD_MANIFEST")
    import json
    import xxhash
    root, manifest = sys.argv[1:]
    # The established collector checks the ISO and reads only root ADV files.
    # Its interrupt census is not evidence for this envelope report.
    sys.argv = [sys.argv[0], str(Path(root) / "game.gog"), manifest]
    sys.path.insert(0, str(Path(__file__).resolve().parent))
    with redirect_stdout(io.StringIO()):
        from disc_adv_exec_census import files, expected
    if len(expected) != 19 or set(files) != set(expected):
        raise ValueError("ADV envelope: manifest/disc census differs from nineteen")
    result = []
    for path in sorted(files):
        body = files[path]
        digest = xxhash.xxh3_128_hexdigest(body)
        if (len(body), digest) != expected[path]:
            raise ValueError(f"ADV envelope: source identity differs: {path}")
        result.append({"path": path, "xxh3": digest, **envelope(body)})
    print(json.dumps({"drivers": result}, indent=2))


if __name__ == "__main__":
    main()
