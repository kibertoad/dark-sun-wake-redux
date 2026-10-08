// Classify analyzer body spans; classification does not prove executable ownership.
export function overlayBodyPartitions(image, ranges) {
  if (!Array.isArray(ranges) || ranges.length === 0 || ranges.length > 4096)
    throw new Error("Supply 1..4096 body spans");
  const { bytes, header, end, overlays } = image;
  const zones = [{ start: header, end, kind: "resident-load-image" }];
  const ordered = [...overlays].sort((a, b) => a.start - b.start);
  for (let i = 0; i < ordered.length; i++) {
    const o = ordered[i];
    zones.push({ start: o.start, end: o.end, kind: "overlay-code", descriptor: o.descriptor });
    if (o.storageEnd > o.end) zones.push({ start: o.end, end: o.storageEnd, kind: "overlay-fixups", descriptor: o.descriptor });
    const next = ordered[i + 1]?.start ?? bytes.length;
    if (next > o.storageEnd) {
      const zero = bytes.subarray(o.storageEnd, next).every(b => b === 0);
      zones.push({ start: o.storageEnd, end: next, kind: zero ? "zero-padding" : "other-file-bytes" });
    }
  }
  zones.sort((a, b) => a.start - b.start);
  for (let i = 1; i < zones.length; i++) if (zones[i].start < zones[i - 1].end)
    throw new Error("Source layout zones overlap");
  const at = p => zones.find(z => p >= z.start && p < z.end);
  return { spans: ranges.map(({ start, end: finish, entry }) => {
    if (!Number.isSafeInteger(start) || !Number.isSafeInteger(finish) || start < 0 || finish <= start || finish > bytes.length
        || !Number.isSafeInteger(entry) || entry < 0 || entry >= bytes.length)
      throw new Error("Invalid body span or entry file offset");
    const parts = [];
    for (let p = start; p < finish;) {
      const zone = at(p), next = zone ? Math.min(zone.end, finish)
        : Math.min(finish, zones.find(z => z.start > p)?.start ?? finish);
      parts.push({ start: p, end: next, kind: zone?.kind ?? "other-file-bytes",
        ...(zone?.descriptor === undefined ? {} : { descriptor: zone.descriptor }) });
      p = next;
    }
    const entryZone = at(entry);
    return { start, end: finish, entry, entryKind: entryZone?.kind ?? "other-file-bytes",
      ...(entryZone?.descriptor === undefined ? {} : { entryDescriptor: entryZone.descriptor }), parts };
  }), scope: "Physical layout only; analyzer ownership and native reachability remain unverified" };
}
