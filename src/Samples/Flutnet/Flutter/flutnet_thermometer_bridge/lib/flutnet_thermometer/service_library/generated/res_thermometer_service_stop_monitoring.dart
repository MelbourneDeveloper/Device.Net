// *************************************
//         NOT EDIT THIS FILE          *
// *************************************
import 'package:json_annotation/json_annotation.dart';
import 'package:meta/meta.dart';

part 'res_thermometer_service_stop_monitoring.g.dart';


/// An annotation for the code generator to know that this class needs the
/// the star denotes the source file name.
@immutable
@JsonSerializable(nullable: true, explicitToJson: true, anyMap: true)
class ResThermometerServiceStopMonitoring {

	ResThermometerServiceStopMonitoring();


	factory ResThermometerServiceStopMonitoring.fromJson(Map<dynamic, dynamic> json) => _$ResThermometerServiceStopMonitoringFromJson(json);

	Map<String, dynamic> toJson() => _$ResThermometerServiceStopMonitoringToJson(this);

}
