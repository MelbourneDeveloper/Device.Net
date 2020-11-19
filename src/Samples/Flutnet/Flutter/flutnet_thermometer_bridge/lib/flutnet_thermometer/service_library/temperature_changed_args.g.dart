// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'temperature_changed_args.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

TemperatureChangedArgs _$TemperatureChangedArgsFromJson(Map json) {
  return TemperatureChangedArgs(
    celsius: (json['Celsius'] as num).toDouble(),
    fahrenheit: (json['Fahrenheit'] as num).toDouble(),
  );
}

Map<String, dynamic> _$TemperatureChangedArgsToJson(
        TemperatureChangedArgs instance) =>
    <String, dynamic>{
      'Celsius': instance.celsius,
      'Fahrenheit': instance.fahrenheit,
    };
