// *************************************
//         NOT EDIT THIS FILE          *
// *************************************
import 'dart:async';
import 'package:flutter/services.dart';
import 'package:flutnet_thermometer_bridge/flutnet/service_model/platform_operation_exception.dart';
import 'package:flutnet_thermometer_bridge/flutnet_bridge.dart';
import 'package:flutnet_thermometer_bridge/flutnet_thermometer/service_library/generated/res_thermometer_service_get_temperature_async.dart';
import 'package:flutnet_thermometer_bridge/flutnet_thermometer/service_library/generated/cmd_thermometer_service_get_temperature_async.dart';
import 'package:flutnet_thermometer_bridge/flutnet_thermometer/service_library/generated/res_thermometer_service_start_monitoring.dart';
import 'package:flutnet_thermometer_bridge/flutnet_thermometer/service_library/generated/cmd_thermometer_service_start_monitoring.dart';
import 'package:flutnet_thermometer_bridge/flutnet_thermometer/service_library/generated/res_thermometer_service_stop_monitoring.dart';
import 'package:flutnet_thermometer_bridge/flutnet_thermometer/service_library/generated/cmd_thermometer_service_stop_monitoring.dart';
import 'package:flutnet_thermometer_bridge/flutnet_thermometer/service_library/temperature_changed_args.dart';



class ThermometerService {

	static const String _type = 'FlutnetThermometer.ServiceLibrary.ThermometerService';

	ThermometerService(
		this.instanceId,
	) : _temperatureChanged = FlutnetBridge()
						.events( instanceId: instanceId, event: 'temperatureChanged')
						.map((_) => TemperatureChangedArgs.fromJson(_));

	final String instanceId;


	// Events ***************************** 
	final Stream<TemperatureChangedArgs> _temperatureChanged;
	Stream<TemperatureChangedArgs> get temperatureChanged => _temperatureChanged;

	// Operations ***************************** 
	static const _kGetTemperatureAsync = 'GetTemperatureAsync()';
	Future<double> getTemperatureAsync() async {

		// Errors occurring on the platform side cause invokeMethod to throw
		// PlatformExceptions.
		try {

			CmdThermometerServiceGetTemperatureAsync _param = CmdThermometerServiceGetTemperatureAsync();
			Map<String, dynamic> _data = await FlutnetBridge().invokeMethod(
				instanceId: instanceId, 
				service: _type, 
				operation: _kGetTemperatureAsync, 
				arguments: _param.toJson(),
			);
			ResThermometerServiceGetTemperatureAsync _res = ResThermometerServiceGetTemperatureAsync.fromJson(_data);
			return _res.returnValue;

		} on PlatformException catch (e) {
			throw Exception("Unable to execute method 'getTemperatureAsync': ${e.code}, ${e.message}");
		} on PlatformOperationException catch (fe) {
			throw fe;
		} on Exception catch (e) {
			throw Exception("Unable to execute method 'getTemperatureAsync': $e");
		}
	}


	static const _kStartMonitoring = 'StartMonitoring()';
	Future<void> startMonitoring() async {

		// Errors occurring on the platform side cause invokeMethod to throw
		// PlatformExceptions.
		try {

			CmdThermometerServiceStartMonitoring _param = CmdThermometerServiceStartMonitoring();
			Map<String, dynamic> _data = await FlutnetBridge().invokeMethod(
				instanceId: instanceId, 
				service: _type, 
				operation: _kStartMonitoring, 
				arguments: _param.toJson(),
			);
			ResThermometerServiceStartMonitoring _res = ResThermometerServiceStartMonitoring.fromJson(_data);

		} on PlatformException catch (e) {
			throw Exception("Unable to execute method 'startMonitoring': ${e.code}, ${e.message}");
		} on PlatformOperationException catch (fe) {
			throw fe;
		} on Exception catch (e) {
			throw Exception("Unable to execute method 'startMonitoring': $e");
		}
	}


	static const _kStopMonitoring = 'StopMonitoring()';
	Future<void> stopMonitoring() async {

		// Errors occurring on the platform side cause invokeMethod to throw
		// PlatformExceptions.
		try {

			CmdThermometerServiceStopMonitoring _param = CmdThermometerServiceStopMonitoring();
			Map<String, dynamic> _data = await FlutnetBridge().invokeMethod(
				instanceId: instanceId, 
				service: _type, 
				operation: _kStopMonitoring, 
				arguments: _param.toJson(),
			);
			ResThermometerServiceStopMonitoring _res = ResThermometerServiceStopMonitoring.fromJson(_data);

		} on PlatformException catch (e) {
			throw Exception("Unable to execute method 'stopMonitoring': ${e.code}, ${e.message}");
		} on PlatformOperationException catch (fe) {
			throw fe;
		} on Exception catch (e) {
			throw Exception("Unable to execute method 'stopMonitoring': $e");
		}
	}


}
