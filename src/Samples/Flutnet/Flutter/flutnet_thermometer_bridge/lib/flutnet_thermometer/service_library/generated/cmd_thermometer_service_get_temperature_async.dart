// *************************************
//         NOT EDIT THIS FILE          *
// *************************************
import 'package:json_annotation/json_annotation.dart';
import 'package:meta/meta.dart';

part 'cmd_thermometer_service_get_temperature_async.g.dart';


/// An annotation for the code generator to know that this class needs the
/// the star denotes the source file name.
@immutable
@JsonSerializable(nullable: true, explicitToJson: true, anyMap: true)
class CmdThermometerServiceGetTemperatureAsync {

	CmdThermometerServiceGetTemperatureAsync();


	factory CmdThermometerServiceGetTemperatureAsync.fromJson(Map<dynamic, dynamic> json) => _$CmdThermometerServiceGetTemperatureAsyncFromJson(json);

	Map<String, dynamic> toJson() => _$CmdThermometerServiceGetTemperatureAsyncToJson(this);

}
