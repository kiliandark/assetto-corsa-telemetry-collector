using System;
using AssettoCorsaSharedMemory;

namespace ACReader
{
    public static class TelemetryParser
    {
        // Теперь метод принимает sessionId
        private static float _lastX = 0f;
        private static float _lastZ = 0f;
        private static bool _hasLastCoords = false;
        private static float _totalDistance = 0f;
        private static float _fuelStart = -1f;
        public static AcTelemetry Parse(
            StaticInfo? staticInfo,
            Graphics? graphics,
            Physics physics,
            string sessionId)
        {
            // Если сессия неизвестна — создаем новый ID
            long sessionIdLong = long.TryParse(sessionId, out var sid)
                ? sid
                : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
             // Инициализация топлива при старте
            if (_fuelStart < 0 && physics.Fuel > 0)
                _fuelStart = physics.Fuel;

            // --- Расчет дистанции ---
            if (graphics?.CarCoordinates != null && graphics?.CarCoordinates.Length == 3)
            {
                float x = graphics?.CarCoordinates[0] ?? 0f;
                float z = graphics?.CarCoordinates[2] ?? 0f;

                if (_hasLastCoords)
                {
                    float dx = x - _lastX;
                    float dz = z - _lastZ;
                    float step = MathF.Sqrt(dx * dx + dz * dz);

                    // фильтруем телепортации
                    if (step < 100f)
                        _totalDistance += step;
                }

                _lastX = x;
                _lastZ = z;
                _hasLastCoords = true;
            }

            // --- Расход топлива ---
            float fuelUsed = (_fuelStart >= 0f) ? MathF.Max(0f, _fuelStart - physics.Fuel) : 0f;

            // Множитель ассиста
            float fuelRateMultiplier = (staticInfo?.AidFuelRate ?? 1f);

            // distance в метрах
            float distance = _totalDistance;

            // расход топлива на 100 км
            float fuelPer100km = (distance > 1f && fuelUsed > 0f) 
                ? fuelUsed * 100000f / distance / 1000f * 2.5f * fuelRateMultiplier
                : 0f;

            var telemetry = new AcTelemetry
            {
                timestamp = DateTime.Now,

                session = new Session
                {
                    short_comment = $"Tires Aid: {staticInfo?.AidTireRate ?? 0} (SM)",
                    driver = staticInfo?.PlayerName ?? "unknown",
                    car = staticInfo?.CarModel ?? "unknown",
                    session_id = sessionIdLong, // теперь из Program.cs
                    session_type = graphics?.Status.ToString() == "HOTLAP" ? "HOTLAP" : "SESSION",
                    flag_type = graphics?.Flag.ToString() == "Black_flag" ? "Black_flag" : "Flag",
                    track = staticInfo?.Track ?? "unknown",

                    completed_laps = graphics?.CompletedLaps ?? 0f,
                    position = graphics?.Position ?? 0f,
                    icurrent_time = (graphics?.iCurrentTime ?? 0f) / 1000f, // ms -> sec
                    session_time_left = (graphics?.SessionTimeLeft ?? 0f) / 1000f,
                    current_sector_index = Math.Clamp(graphics?.CurrentSectorIndex ?? 0, 0, 2), // корректный сектор
                    number_of_laps = (graphics?.CompletedLaps ?? 0) + 1,
                    penalty_time = graphics?.PenaltyTime ?? 0f,

                    lap_time = (graphics?.iLastTime ?? 0) / 1000f,
                    best_lap_time = (graphics?.iBestTime ?? 0) / 1000f,
                    is_valid_lap = physics.IsValidLap == 0 ? 1 : 0,
                    track_spline_length = staticInfo?.TrackSPlineLength ?? 0f,
                },
                metrics = new Metrics
                {
                    // Physics
                    packet_id = physics.PacketId,
                    drs = physics.Drs,
                    heading = physics.Heading,
                    pit_limiter_on = physics.PitLimiterOn,
                    kers_charge = physics.KersCharge,
                    kers_input = physics.KersInput,
                    turbo_boost = physics.TurboBoost,
                    ballast = physics.Ballast,
                    final_ff = physics.FinalFF,
                    performance_meter = physics.PerformanceMeter,
                    engine_brake = physics.EngineBrake,
                    ers_recovery_level = physics.ErsRecoveryLevel,
                    ers_power_level = physics.ErsPowerLevel,
                    ers_heat_charging = physics.ErsHeatCharging,
                    ers_is_charging = physics.ErsisCharging,
                    kers_current_kj = physics.KersCurrentKJ,
                    drs_enabled = physics.DrsEnabled > 0,
                    tyre_temp_i = physics.TyreTempI,
                    is_ai_controlled = physics.IsAIControlled,
                    local_velocity = physics.LocalVelocity,
                    velocity = physics.Velocity,
                    tyre_dirty_level = physics.TyreDirtyLevel,

                    // Graphics moved to Metrics
                    current_time = graphics?.CurrentTime ?? "",
                    last_time = graphics?.LastTime ?? "",
                    best_time = graphics?.BestTime ?? "",
                    split = graphics?.Split ?? "",
                    distance_traveled = graphics?.DistanceTraveled ?? 0f,
                    is_in_pit = graphics?.IsInPit ?? 0,
                    last_sector_time = graphics?.LastSectorTime ?? 0,
                    tyre_compound = graphics?.TyreCompound ?? "",
                    replay_time_multiplier = graphics?.ReplayTimeMultiplier ?? 0f,
                    normalized_car_position = graphics?.NormalizedCarPosition ?? 0f,
                    car_coordinates = graphics?.CarCoordinates ?? new float[3],
                    ideal_line_on = graphics?.IdealLineOn ?? 0,
                    is_in_pit_lane = graphics?.IsInPitLane ?? 0,
                    surface_grip = graphics?.SurfaceGrip ?? 0f,
                    mandatory_pit_done = graphics?.MandatoryPitDone ?? 0,

                    // Shared Metrics
                    tire_pressure_fl = physics.WheelsPressure.SafeGet(0),
                    tire_pressure_fr = physics.WheelsPressure.SafeGet(1),
                    tire_pressure_rl = physics.WheelsPressure.SafeGet(2),
                    tire_pressure_rr = physics.WheelsPressure.SafeGet(3),
                    number_of_tyres_out = physics.NumberOfTyresOut,
                    tire_temp_core_fl = physics.TyreCoreTemperature.SafeGet(0),
                    tire_temp_core_fr = physics.TyreCoreTemperature.SafeGet(1),
                    tire_temp_core_rl = physics.TyreCoreTemperature.SafeGet(2),
                    tire_temp_core_rr = physics.TyreCoreTemperature.SafeGet(3),
                    tyre_wear_fl = physics.TyreWear.SafeGet(0),
                    tyre_wear_fr = physics.TyreWear.SafeGet(1),
                    tyre_wear_rl = physics.TyreWear.SafeGet(2),
                    tyre_wear_rr = physics.TyreWear.SafeGet(3),
                    fuel_level = physics.Fuel,
                    wind_speed = graphics?.WindSpeed ?? 0f,
                    gear = physics.Gear,
                    brake_temp = physics.BrakeTemp.Average(),
                    abs = physics.Abs,
                    tc = physics.TC,
                    aid_auto_shift = physics.AutoShifterOn > 0,
                    throttle_pos = physics.Gas * 100f,
                    clutch_pos = physics.Clutch * 100f,
                    brake_pos = physics.Brake * 100f,
                    engine_rpm = physics.Rpms,
                    speed = physics.SpeedKmh,
                    road_temp = physics.RoadTemp,
                    air_temp = physics.AirTemp,
                    air_density = physics.AirDensity,
                    ride_height_fl = physics.RideHeight.SafeGet(0) * 1000f,
                    ride_height_fr = physics.RideHeight.SafeGet(1) * 1000f,
                    ride_height_rl = physics.RideHeight.SafeGet(2) * 1000f,
                    ride_height_rr = physics.RideHeight.SafeGet(3) * 1000f,
                    suspension_travel_fl = physics.SuspensionTravel.SafeGet(0) * 1000f,
                    suspension_travel_fr = physics.SuspensionTravel.SafeGet(1) * 1000f,
                    suspension_travel_rl = physics.SuspensionTravel.SafeGet(2) * 1000f,
                    suspension_travel_rr = physics.SuspensionTravel.SafeGet(3) * 1000f,
                    tire_radius_fl = (float)(staticInfo?.TyreRadius.SafeGet(0) * 1000 ?? 0),
                    tire_radius_fr = (float)(staticInfo?.TyreRadius.SafeGet(1) * 1000 ?? 0),
                    tire_radius_rl = (float)(staticInfo?.TyreRadius.SafeGet(2) * 1000 ?? 0),
                    tire_radius_rr = (float)(staticInfo?.TyreRadius.SafeGet(3) * 1000 ?? 0),
                    tire_load_fl = physics.WheelLoad.SafeGet(0),
                    tire_load_fr = physics.WheelLoad.SafeGet(1),
                    tire_load_rl = physics.WheelLoad.SafeGet(2),
                    tire_load_rr = physics.WheelLoad.SafeGet(3),
                    tire_temp_inner_fl = physics.TyreCoreTemperature.SafeGet(0),
                    tire_temp_inner_fr = physics.TyreCoreTemperature.SafeGet(1),
                    tire_temp_inner_rl = physics.TyreCoreTemperature.SafeGet(2),
                    tire_temp_inner_rr = physics.TyreCoreTemperature.SafeGet(3),
                    tire_temp_middle_fl = physics.TyreTempM.SafeGet(0),
                    tire_temp_middle_fr = physics.TyreTempM.SafeGet(1),
                    tire_temp_middle_rl = physics.TyreTempM.SafeGet(2),
                    tire_temp_middle_rr = physics.TyreTempM.SafeGet(3),
                    tire_temp_outer_fl = physics.TyreTempO.SafeGet(0),
                    tire_temp_outer_fr = physics.TyreTempO.SafeGet(1),
                    tire_temp_outer_rl = physics.TyreTempO.SafeGet(2),
                    tire_temp_outer_rr = physics.TyreTempO.SafeGet(3),
                    tire_slip_ratio_fl = physics.WheelSlip.SafeGet(0) * 100f,
                    tire_slip_ratio_fr = physics.WheelSlip.SafeGet(1) * 100f,
                    tire_slip_ratio_rl = physics.WheelSlip.SafeGet(2) * 100f,
                    tire_slip_ratio_rr = physics.WheelSlip.SafeGet(3) * 100f,
                    tire_slip_angle_fl = (float)(physics.SteerAngle * (180 / Math.PI)),
                    tire_slip_angle_fr = (float)(physics.SteerAngle * (180 / Math.PI)),
                    tire_slip_angle_rl = (float)(physics.SteerAngle * (180 / Math.PI)),
                    tire_slip_angle_rr = (float)(physics.SteerAngle * (180 / Math.PI)),
                    camber_fl = (float)(physics.CamberRad.SafeGet(0) * (180 / Math.PI)),
                    camber_fr = (float)(physics.CamberRad.SafeGet(1) * (180 / Math.PI)),
                    camber_rl = (float)(physics.CamberRad.SafeGet(2) * (180 / Math.PI)),
                    camber_rr = (float)(physics.CamberRad.SafeGet(3) * (180 / Math.PI)),
                    steering_angle = (float)(physics.SteerAngle * (180 / Math.PI)),
                    brake_bias = physics.BrakeBias * 100f,
                    cg_height = physics.CgHeight * 1000f,
                    cg_accel_longitudinal = physics.AccG.SafeGet(0),
                    cg_accel_lateral = physics.AccG.SafeGet(1),
                    cg_accel_vertical = physics.AccG.SafeGet(2),
                    chassis_pitch_angle = (float)(physics.Pitch * (180 / Math.PI)),
                    chassis_roll_angle = (float)(physics.Roll * (180 / Math.PI)),
                    chassis_yaw_rate = (float)(physics.LocalAngularVelocity.SafeGet(1) * (180 / Math.PI)),
                    chassis_pitch_rate = (float)(physics.LocalAngularVelocity.SafeGet(0) * (180 / Math.PI)),
                    chassis_roll_rate = (float)(physics.LocalAngularVelocity.SafeGet(2) * (180 / Math.PI)),
                    wheel_angular_speed_fl = physics.WheelAngularSpeed.SafeGet(0),
                    wheel_angular_speed_fr = physics.WheelAngularSpeed.SafeGet(1),
                    wheel_angular_speed_rl = physics.WheelAngularSpeed.SafeGet(2),
                    wheel_angular_speed_rr = physics.WheelAngularSpeed.SafeGet(3),
                    car_damage_front = physics.CarDamage.SafeGet(0),
                    car_damage_rear = physics.CarDamage.SafeGet(1),
                    car_damage_left = physics.CarDamage.SafeGet(2),
                    car_damage_right = physics.CarDamage.SafeGet(3),
                    drs_available = physics.DrsAvailable > 0,

                    fuel_used_total = fuelUsed,
                    fuel_per_100km = fuelPer100km,

                    // StaticInfo
                    ac_version = staticInfo?.ACVersion ?? "",
                    max_power = staticInfo?.MaxPower ?? 0f,
                    max_torque = staticInfo?.MaxTorque ?? 0f,
                    max_rpm = staticInfo?.MaxRpm ?? 0,
                    kers = staticInfo?.HasKERS ?? 0f,
                    ers = staticInfo?.HasERS ?? 0f,
                }
            };

            return telemetry;
        }
    }

    public class AcTelemetry
    {
        public DateTime timestamp { get; set; }
        public required Session session { get; set; }
        public required Metrics metrics { get; set; }
    }

    public class Session
    {
        public required string short_comment { get; set; }
        public required string driver { get; set; }
        public required string car { get; set; }
        public long session_id { get; set; }
        public required string session_type { get; set; }
        public required string flag_type { get; set; }

        public float completed_laps { get; set; }
        public float position { get; set; }
        public float icurrent_time { get; set; }
        public float session_time_left { get; set; }
        public float current_sector_index { get; set; }
        public float number_of_laps { get; set; }
        public float penalty_time { get; set; }

        public required string track { get; set; }

        public float lap_time { get; set; }
        public float best_lap_time { get; set; }
        public int is_valid_lap { get; set; }
        public float track_spline_length { get; set; }
    }

    public class Metrics
    {

        public float fuel_used_total { get; set; }
        public float fuel_per_100km { get; set; }

        public int packet_id { get; set; }
        public float heading { get; set; }
        public int pit_limiter_on { get; set; }
        public float kers_charge { get; set; }
        public float kers_input { get; set; }
        public float turbo_boost { get; set; }
        public float ballast { get; set; }
        public float final_ff { get; set; }
        public float performance_meter { get; set; }
        public int engine_brake { get; set; }
        public int ers_recovery_level { get; set; }
        public int ers_power_level { get; set; }
        public int ers_heat_charging { get; set; }
        public int ers_is_charging { get; set; }
        public float kers_current_kj { get; set; }
        public bool drs_enabled { get; set; }
        public float[] tyre_temp_i { get; set; } = new float[4];
        public int is_ai_controlled { get; set; }

        public float[] local_velocity { get; set; } = new float[3];
        public float[] velocity { get; set; } = new float[3];
        public float[] tyre_dirty_level { get; set; } = new float[4];

        // Graphics moved
        public string current_time { get; set; } = "";
        public string last_time { get; set; } = "";
        public string best_time { get; set; } = "";
        public string split { get; set; } = "";
        public float distance_traveled { get; set; }
        public int is_in_pit { get; set; }
        public int last_sector_time { get; set; }
        public string tyre_compound { get; set; } = "";
        public float replay_time_multiplier { get; set; }
        public float normalized_car_position { get; set; }
        public float[] car_coordinates { get; set; } = new float[3];
        public int ideal_line_on { get; set; }
        public int is_in_pit_lane { get; set; }
        public float surface_grip { get; set; }
        public int mandatory_pit_done { get; set; }

        // StaticInfo
        public string ac_version { get; set; } = "";
        public float max_power { get; set; }
        public int max_rpm { get; set; }
        public float max_torque { get; set; }
        public float kers { get; set; }
        public float ers { get; set; }
 

        public float number_of_tyres_out { get; set; }

        
        public float tyre_wear_fl { get; set; }
        public float tyre_wear_fr { get; set; }
        public float tyre_wear_rl { get; set; }
        public float tyre_wear_rr { get; set; }

        
        public float tire_temp_core_fl { get; set; }
        public float tire_temp_core_fr { get; set; }
        public float tire_temp_core_rl { get; set; }
        public float tire_temp_core_rr { get; set; }


        public float tire_pressure_fl { get; set; }
        public float tire_pressure_fr { get; set; }
        public float tire_pressure_rl { get; set; }
        public float tire_pressure_rr { get; set; }


        public float abs { get; set; }
        public float brake_temp { get; set; }
        public float wind_speed { get; set; }
        public float tc { get; set; }
        public float gear { get; set; }
        public bool drs { get; set; }
        public bool aid_auto_shift { get; set; }
        public float fuel_level { get; set; }
        public float throttle_pos { get; set; }
        public float clutch_pos { get; set; }
        public float brake_pos { get; set; }
        public float engine_rpm { get; set; }
        public float speed { get; set; }
        public float road_temp { get; set; }
        public float air_temp { get; set; }
        public float air_density { get; set; }
        public float ride_height_fl { get; set; }
        public float ride_height_fr { get; set; }
        public float ride_height_rl { get; set; }
        public float ride_height_rr { get; set; }
        public float suspension_travel_fl { get; set; }
        public float suspension_travel_fr { get; set; }
        public float suspension_travel_rl { get; set; }
        public float suspension_travel_rr { get; set; }
        public float tire_radius_fl { get; set; }
        public float tire_radius_fr { get; set; }
        public float tire_radius_rl { get; set; }
        public float tire_radius_rr { get; set; }
        public float tire_load_fl { get; set; }
        public float tire_load_fr { get; set; }
        public float tire_load_rl { get; set; }
        public float tire_load_rr { get; set; }
        public float tire_temp_inner_fl { get; set; }
        public float tire_temp_inner_fr { get; set; }
        public float tire_temp_inner_rl { get; set; }
        public float tire_temp_inner_rr { get; set; }
        public float tire_temp_middle_fl { get; set; }
        public float tire_temp_middle_fr { get; set; }
        public float tire_temp_middle_rl { get; set; }
        public float tire_temp_middle_rr { get; set; }
        public float tire_temp_outer_fl { get; set; }
        public float tire_temp_outer_fr { get; set; }
        public float tire_temp_outer_rl { get; set; }
        public float tire_temp_outer_rr { get; set; }
        public float tire_slip_ratio_fl { get; set; }
        public float tire_slip_ratio_fr { get; set; }
        public float tire_slip_ratio_rl { get; set; }
        public float tire_slip_ratio_rr { get; set; }
        public float tire_slip_angle_fl { get; set; }
        public float tire_slip_angle_fr { get; set; }
        public float tire_slip_angle_rl { get; set; }
        public float tire_slip_angle_rr { get; set; }
        public float camber_fl { get; set; }
        public float camber_fr { get; set; }
        public float camber_rl { get; set; }
        public float camber_rr { get; set; }
        public float steering_angle { get; set; }
        public float brake_bias { get; set; }
        public float cg_height { get; set; }
        public float cg_accel_longitudinal { get; set; }
        public float cg_accel_lateral { get; set; }
        public float cg_accel_vertical { get; set; }
        public float chassis_pitch_angle { get; set; }
        public float chassis_roll_angle { get; set; }
        public float chassis_yaw_rate { get; set; }
        public float chassis_pitch_rate { get; set; }
        public float chassis_roll_rate { get; set; }
        public float wheel_angular_speed_fl { get; set; }
        public float wheel_angular_speed_fr { get; set; }
        public float wheel_angular_speed_rl { get; set; }
        public float wheel_angular_speed_rr { get; set; }
        public float car_damage_front { get; set; }
        public float car_damage_rear { get; set; }
        public float car_damage_left { get; set; }
        public float car_damage_right { get; set; }
        public bool drs_available { get; set; }
    }

    internal static class SafeArrayExtensions
    {
        public static float SafeGet(this float[]? arr, int index)
        {
            if (arr == null || index < 0 || index >= arr.Length)
                return 0f;
            return arr[index];
        }
    }
}
