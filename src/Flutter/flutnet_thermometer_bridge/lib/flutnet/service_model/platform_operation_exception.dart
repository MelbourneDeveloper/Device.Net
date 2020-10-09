// *************************************
//         NOT EDIT THIS FILE          *
// *************************************
import 'package:json_annotation/json_annotation.dart';
import 'package:meta/meta.dart';
import 'package:flutnet_thermometer_bridge/flutnet/data/flutnet_exception.dart';

part 'platform_operation_exception.g.dart';


/// An annotation for the code generator to know that this class needs the
/// the star denotes the source file name.
@immutable
@JsonSerializable(nullable: true, explicitToJson: true, anyMap: true)
class PlatformOperationException extends Object implements Exception {

	PlatformOperationException({
		this.message,
	});

	@JsonKey(name: "Message", nullable: true)
	final String message;


	factory PlatformOperationException.fromJson(Map<dynamic, dynamic> json) => _$PlatformOperationExceptionFromJson(json);

	Map<String, dynamic> toJson() => _$PlatformOperationExceptionToJson(this);

	static final Map<String, PlatformOperationException Function(Map<String, dynamic>)> 	_typeToPlatformOperationException = {
		'Flutnet.Data.FlutnetException': (Map<String, dynamic> json) => FlutnetException.fromJson(json),
		'Flutnet.ServiceModel.PlatformOperationException': (Map<String, dynamic> json) => PlatformOperationException.fromJson(json),
	};


	/// Dynamic deserialization
	factory PlatformOperationException.fromJsonDynamic(Map<String, dynamic> json) {

		// Nothing to do
		if (json == null || json.isEmpty) return null;

		try {
			String typeKey = json.keys.first;
			var fromJson = 	_typeToPlatformOperationException.containsKey(typeKey)
			 ? 	_typeToPlatformOperationException[typeKey] 
			 : null;

			Map<String, dynamic> payload = json[typeKey];

			///! REAL DESERIALIZATION PROCESS
			return fromJson(payload);

		} catch (e) {
		  throw new Exception('Error during lib deserialization process: $json');
		}
	}


	@override
	String toString() {
		return toJson().toString();
	}


}
