"""Static census of DOS service selection in the disc's Miles .ADV driver files (FND-EXE-492).

Usage: python -I disc_adv_exec_census.py PATH_TO_game.gog PATH_TO_BLD-GOG-EN-1.1.files.yaml

Reads each CD:*.ADV file from the ISO 9660 volume on the MODE2/2352 data track in memory, checks
its size and XXH3-128 against the build manifest, and lists every interrupt instruction byte pair
with the nearest preceding immediate AX/AH writer of the most common aligned decode. Nothing is
written to disk or executed.
"""
import re
import sys
from collections import Counter

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SECTOR, DATA = 2352, 24
image, manifest = sys.argv[1], sys.argv[2]
md = Cs(CS_ARCH_X86, CS_MODE_16)

expected = {}
text = open(manifest, encoding="utf-8").read()
for m in re.finditer(r"- path: (CD:[^\n]+\.ADV)\n(?:\s+format: [^\n]+\n)?\s+size: (\d+)\n\s+xxh3: ([0-9a-f]{32})", text):
    expected[m.group(1)] = (int(m.group(2)), m.group(3))


def sectors(f, lba, count):
    out = bytearray()
    for i in range(count):
        f.seek((lba + i) * SECTOR + DATA)
        out += f.read(2048)
    return bytes(out)


files = {}
with open(image, "rb") as f:
    pvd = sectors(f, 16, 1)
    assert pvd[0] == 1 and pvd[1:6] == b"CD001"
    root_lba = int.from_bytes(pvd[158:162], "little")
    root_len = int.from_bytes(pvd[166:170], "little")
    data = sectors(f, root_lba, (root_len + 2047) // 2048)
    pos = 0
    while pos < root_len:
        n = data[pos]
        if n == 0:
            pos = (pos // 2048 + 1) * 2048
            continue
        lba = int.from_bytes(data[pos + 2:pos + 6], "little")
        size = int.from_bytes(data[pos + 10:pos + 14], "little")
        flags, idlen = data[pos + 25], data[pos + 32]
        name = data[pos + 33:pos + 33 + idlen].decode("latin1").split(";")[0].rstrip(".")
        pos += n
        if not flags & 2 and name.upper().endswith(".ADV"):
            files["CD:" + name] = sectors(f, lba, (size + 2047) // 2048)[:size]

print(f"manifest .ADV entries {len(expected)}, root-directory .ADV files {len(files)}")
for path in sorted(set(expected) | set(files)):
    if path not in files or path not in expected:
        print(f"{path}: {'absent from disc root' if path not in files else 'not in manifest'}")
        continue
    body = files[path]
    digest = xxhash.xxh3_128_hexdigest(body)
    ok = (len(body), digest) == expected[path]
    ints = Counter()
    sites = []
    for m in re.finditer(re.escape(b"\xCD"), body):
        h = m.start()
        if h + 1 >= len(body):
            continue
        votes = Counter()
        for back in range(1, 49):
            s = h - back
            if s < 0:
                break
            chain = list(md.disasm(body[s:h + 2], s))
            if chain and chain[-1].address == h and chain[-1].size == 2:
                votes[tuple((x.address, x.mnemonic, x.op_str) for x in chain)] += 1
        if sum(votes.values()) < 20:
            continue
        num = body[h + 1]
        ints[num] += 1
        if num in (0x21, 0x2E):
            chain = max(votes, key=lambda t: (votes[t], len(t)))
            writer = next((o for a, mn, o in reversed(chain[:-1])
                           if mn == "mov" and re.match(r"a[hx], (0x)?[0-9a-f]+$", o)), None)
            if writer is None:
                ah = "non-immediate:" + "; ".join(f"{mn} {o}" for a, mn, o in chain[-6:-1])
            else:
                v = int(writer.split(", ")[1], 0)
                ah = f"{(v if writer.startswith('ah') else v >> 8):02X}"
            sites.append(f"int {num:02X} at 0x{h:04X} AH={ah}")
    print(f"{path}: size {len(body)} identity {'ok' if ok else 'MISMATCH'}; aligned interrupts "
          f"{dict(sorted((f'{k:02X}', v) for k, v in ints.items()))}; 4B immediates "
          f"{len(re.findall(re.escape(b'\xB4\x4B'), body))}")
    for s in sites:
        print("   ", s)
