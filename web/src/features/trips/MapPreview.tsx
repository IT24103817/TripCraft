/** OpenStreetMap embed around a point. No API key and no extra library. */
export function MapPreview({
  latitude,
  longitude,
  label,
}: {
  latitude: number;
  longitude: number;
  label: string;
}) {
  const valid =
    Number.isFinite(latitude) &&
    Number.isFinite(longitude) &&
    Math.abs(latitude) <= 90 &&
    Math.abs(longitude) <= 180;
  if (!valid)
    return <p className="text-sm text-slate-500">Enter a valid latitude and longitude to preview the map.</p>;

  const d = 0.01;
  const bbox = [longitude - d, latitude - d, longitude + d, latitude + d].join(',');
  const src = `https://www.openstreetmap.org/export/embed.html?bbox=${bbox}&layer=mapnik&marker=${latitude},${longitude}`;
  return (
    <iframe
      title={`Map of ${label}`}
      src={src}
      className="h-48 w-full rounded border border-slate-200"
      loading="lazy"
    />
  );
}
