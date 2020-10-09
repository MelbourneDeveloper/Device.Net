// *************************************
//         NOT EDIT THIS FILE          *
// *************************************
import 'package:json_annotation/json_annotation.dart';
import 'package:meta/meta.dart';
import 'package:flutnet_thermometer_bridge/flutnet/data/flutnet_error_code.dart';
import 'package:flutnet_thermometer_bridge/flutnet/service_model/platform_operation_exception.dart';

part 'flutnet_exception.g.dart';


/// An annotation for the code generator to know that this class needs the
/// the star denotes the source file name.
@immutable
@JsonSerializable(nullable: true, explicitToJson: true, anyMap: true)
class FlutnetException extends PlatformOperationException {

	FlutnetException({
		this.code,
		String message,
	}) : super(
					message: message,
				);

	@JsonKey(name: "Code", nullable: false)
	final FlutnetErrorCode code;


	factory FlutnetException.fromJson(Map<dynamic, dynamic> json) => _$FlutnetExceptionFromJson(json);

	Map<String, dynamic> toJson() => _$FlutnetExceptionToJson(this);


}
