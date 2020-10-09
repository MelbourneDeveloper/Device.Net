// *************************************
//         NOT EDIT THIS FILE          *
// *************************************
import 'package:json_annotation/json_annotation.dart';
import 'package:meta/meta.dart';

part 'temperature_changed_args.g.dart';


/// An annotation for the code generator to know that this class needs the
/// the star denotes the source file name.
@immutable
@JsonSerializable(nullable: true, explicitToJson: true, anyMap: true)
class TemperatureChangedArgs {

	TemperatureChangedArgs({
		this.celsius,
		this.fahrenheit,
	});

	@JsonKey(name: "Celsius", nullable: false)
	final double celsius;

	@JsonKey(name: "Fahrenheit", nullable: false)
	final double fahrenheit;


	factory TemperatureChangedArgs.fromJson(Map<dynamic, dynamic> json) => _$TemperatureChangedArgsFromJson(json);

	Map<String, dynamic> toJson() => _$TemperatureChangedArgsToJson(this);

}
