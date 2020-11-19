// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'flutnet_exception.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

FlutnetException _$FlutnetExceptionFromJson(Map json) {
  return FlutnetException(
    code: _$enumDecode(_$FlutnetErrorCodeEnumMap, json['Code']),
    message: json['Message'] as String,
  );
}

Map<String, dynamic> _$FlutnetExceptionToJson(FlutnetException instance) =>
    <String, dynamic>{
      'Message': instance.message,
      'Code': _$FlutnetErrorCodeEnumMap[instance.code],
    };

T _$enumDecode<T>(
  Map<T, dynamic> enumValues,
  dynamic source, {
  T unknownValue,
}) {
  if (source == null) {
    throw ArgumentError('A value must be provided. Supported values: '
        '${enumValues.values.join(', ')}');
  }

  final value = enumValues.entries
      .singleWhere((e) => e.value == source, orElse: () => null)
      ?.key;

  if (value == null && unknownValue == null) {
    throw ArgumentError('`$source` is not one of the supported values: '
        '${enumValues.values.join(', ')}');
  }
  return value ?? unknownValue;
}

const _$FlutnetErrorCodeEnumMap = {
  FlutnetErrorCode.OperationNotImplemented: 'OperationNotImplemented',
  FlutnetErrorCode.OperationArgumentCountMismatch:
      'OperationArgumentCountMismatch',
  FlutnetErrorCode.InvalidOperationArguments: 'InvalidOperationArguments',
  FlutnetErrorCode.OperationArgumentParsingError:
      'OperationArgumentParsingError',
  FlutnetErrorCode.OperationFailed: 'OperationFailed',
  FlutnetErrorCode.OperationCanceled: 'OperationCanceled',
  FlutnetErrorCode.EnvironmentNotInitialized: 'EnvironmentNotInitialized',
  FlutnetErrorCode.AppKeyErrorBadFormat: 'AppKeyErrorBadFormat',
  FlutnetErrorCode.AppKeyErrorApplicationIdMismatch:
      'AppKeyErrorApplicationIdMismatch',
  FlutnetErrorCode.AppKeyErrorUnsupportedLibraryVersion:
      'AppKeyErrorUnsupportedLibraryVersion',
  FlutnetErrorCode.TrialCallsExceeded: 'TrialCallsExceeded',
};
