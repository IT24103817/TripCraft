/// The kinds of weather the itinerary shows an icon for.
enum WeatherKind { sun, cloud, rain, storm }

/// The weather part of a day's notes: "By train; weather: Light rain, 24°C" -> "Light rain, 24°C".
/// Null when the notes say nothing about the weather.
String? weatherFromNotes(String? notes) {
  if (notes == null) return null;
  final at = notes.toLowerCase().indexOf('weather:');
  if (at < 0) return null;
  // Everything after "weather:", up to the next "; " part if there is one.
  final text = notes.substring(at + 'weather:'.length).split(';').first.trim();
  return text.isEmpty ? null : text;
}

/// Which icon a weather text gets. Checked from the worst weather down, so "Rain and thunder" is a storm.
/// Null when the text is missing or unknown (no icon is shown then).
WeatherKind? weatherKindOf(String? weather) {
  final text = weather?.toLowerCase() ?? '';
  bool has(List<String> words) => words.any(text.contains);
  if (has(['storm', 'thunder', 'lightning'])) return WeatherKind.storm;
  if (has(['rain', 'drizzle', 'shower'])) return WeatherKind.rain;
  if (has(['cloud', 'overcast', 'fog', 'mist'])) return WeatherKind.cloud;
  if (has(['sun', 'clear', 'fair', 'hot'])) return WeatherKind.sun;
  return null;
}
