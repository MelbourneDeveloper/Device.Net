// *************************************
//         NOT EDIT THIS FILE          *
// *************************************
import 'package:json_annotation/json_annotation.dart';
import 'package:meta/meta.dart';

part 'cmd_thermometer_service_start_monitoring.g.dart';


/// An annotation for the code generator to know that this class needs the
/// the star denotes the source file name.
@immutable
@JsonSerializable(nullable: true, explicitToJson: true, anyMap: true)
class CmdThermometerServiceStartMonitoring {

	CmdThermometerServiceStartMonitoring();


	factory CmdThermometerServiceStartMonitoring.fromJson(Map<dynamic, dynamic> json) => _$CmdThermometerServiceStartMonitoringFromJson(json);

	Map<String, dynamic> toJson() => _$CmdThermometerServiceStartMonitoringToJson(this);

}
