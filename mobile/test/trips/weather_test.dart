import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/features/trips/data/weather.dart';

void main() {
  group('weatherFromNotes', () {
    test('takes the text after "weather:"', () {
      expect(
        weatherFromNotes('By train; weather: Light rain, 24°C'),
        'Light rain, 24°C',
      );
      expect(weatherFromNotes('Weather: Sunny, 30°C'), 'Sunny, 30°C');
    });

    test('stops at the next part of the notes', () {
      expect(
        weatherFromNotes('weather: Cloudy, 21°C; hotel: Kandy Hills'),
        'Cloudy, 21°C',
      );
    });

    test('null when the notes say nothing about the weather', () {
      expect(weatherFromNotes(null), isNull);
      expect(weatherFromNotes('By train'), isNull);
      expect(weatherFromNotes('By road; weather:   '), isNull);
    });
  });

  group('weatherKindOf', () {
    test('sun, cloud, rain and storm', () {
      expect(weatherKindOf('Sunny, 30°C'), WeatherKind.sun);
      expect(weatherKindOf('Clear sky'), WeatherKind.sun);
      expect(weatherKindOf('Partly cloudy, 26°C'), WeatherKind.cloud);
      expect(weatherKindOf('Overcast'), WeatherKind.cloud);
      expect(weatherKindOf('Light rain, 24°C'), WeatherKind.rain);
      expect(weatherKindOf('Showers'), WeatherKind.rain);
      expect(weatherKindOf('Thunderstorm'), WeatherKind.storm);
    });

    test('the worst weather wins', () {
      expect(weatherKindOf('Rain with thunder'), WeatherKind.storm);
      expect(weatherKindOf('Sunny then rain'), WeatherKind.rain);
    });

    test('no icon for missing or unknown weather', () {
      expect(weatherKindOf(null), isNull);
      expect(weatherKindOf('24°C'), isNull);
    });
  });
}
