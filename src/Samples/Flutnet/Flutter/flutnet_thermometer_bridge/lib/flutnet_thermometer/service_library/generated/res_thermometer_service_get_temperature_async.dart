// *************************************
//         NOT EDIT THIS FILE          *
// *************************************
import 'package:json_annotation/json_annotation.dart';
import 'package:meta/meta.dart';

part 'res_thermometer_service_get_temperature_async.g.dart';


/// An annotation for the code generator to know that this class needs the
/// the star denotes the source file name.
@immutable
@JsonSerializable(nullable: true, explicitToJson: true, anyMap: true)
class ResThermometerServiceGetTemperatureAsync {

	ResThermometerServiceGetTemperatureAsync({
		this.returnValue,
	});

	@JsonKey(name: "ReturnValue", nullable: false)
	final double returnValue;


	factory ResThermometerServiceGetTemperatureAsync.fromJson(Map<dynamic, dynamic> json) => _$ResThermometerServiceGetTemperatureAsyncFromJson(json);

	Map<String, dynamic> toJson() => _$ResThermometerServiceGetTemperatureAsyncToJson(this);

}
