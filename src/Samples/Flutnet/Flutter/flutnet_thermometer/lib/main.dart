import 'dart:async';
import 'package:flutter_thermometer/thermometer.dart';
import 'package:flutter/material.dart';
import 'package:flutnet_thermometer_bridge/flutnet_thermometer/service_library/thermometer_service.dart';
import 'package:flutnet_thermometer_bridge/flutnet_thermometer/service_library/temperature_changed_args.dart';

void main() => runApp(MyApp());

class MyApp extends StatelessWidget {
  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Flutnet Thermometer App',
      theme: ThemeData(
        primarySwatch: Colors.blue,
      ),
      home: MyHomePage(title: 'Flutnet Thermometer App'),
    );
  }
}

class MyHomePage extends StatefulWidget {
  MyHomePage({Key key, this.title}) : super(key: key);

  final String title;

  @override
  _MyHomePageState createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  // Current temperature value
  double _celsius = 0;
  double _farenheit = 0;

  // Xamarin service for monitoring the temperature
  final ThermometerService _thermometerService =
      ThermometerService("thermometer_service");

  // When the temperature changed in Xamarin
  StreamSubscription<TemperatureChangedArgs> _eventSubscription;

  @override
  void initState() {
    super.initState();

    // ******************************************
    // Connect to the temperature changed event
    // ******************************************
    _eventSubscription = _thermometerService.temperatureChanged.listen(
      (TemperatureChangedArgs args) {
        // Receive the temperature from xamarin
        setState(() {
          _celsius = args.celsius;
          _farenheit = args.fahrenheit;
        });
      },
      cancelOnError: false,
    );

    // Start monitoring the temperature
    _thermometerService.startMonitoring();
  }

  @override
  void dispose() {
    //
    // IMPORTANT: Cancel the subscription from the event.
    //
    _eventSubscription.cancel();

    // Start monitoring the temperature
    _thermometerService.stopMonitoring();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.title),
      ),
      body: Column(
        children: [
          Expanded(
            flex: 1,
            child: Container(
              color: Colors.white,
              child: Row(
                children: [
                  Expanded(
                    flex: 3,
                    child: Padding(
                      padding: const EdgeInsets.all(0.0),
                      child: Image.asset(
                        'assets/xamarin_logo.png',
                        height: 40,
                        fit: BoxFit.fitHeight,
                      ),
                    ),
                  ),
                  Expanded(flex: 1, child: Icon(Icons.add)),
                  Expanded(
                    flex: 7,
                    child: Image.asset(
                      'assets/flutnet_logo.png',
                      height: 40,
                      fit: BoxFit.fitHeight,
                    ),
                  ),
                  Expanded(flex: 1, child: Icon(Icons.add)),
                  Expanded(
                    flex: 3,
                    child: FlutterLogo(
                      size: 40,
                    ),
                  ),
                ],
              ),
            ),
          ),
          Expanded(
            flex: 5,
            child: Container(
              padding: EdgeInsets.all(8.0),
              color: Colors.white,
              child: Expanded(
                child: Row(
                  children: [
                    Expanded(
                      child: Thermometer(
                        label: ThermometerLabel.farenheit(),
                        scale: IntervalScaleProvider(20),
                        mercuryColor: Colors.blue,
                        value: _farenheit,
                        minValue: -40,
                        maxValue: 120,
                        setpoint: Setpoint(_farenheit),
                      ),
                    ),
                    Expanded(
                      child: Thermometer(
                        label: ThermometerLabel.celsius(),
                        scale: IntervalScaleProvider(10),
                        mirrorScale: true,
                        value: _celsius,
                        minValue: -30,
                        maxValue: 50,
                        setpoint: Setpoint(_celsius),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
