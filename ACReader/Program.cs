using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using AssettoCorsaSharedMemory;
using Newtonsoft.Json;

namespace ACReader
{
    [SupportedOSPlatform("windows")]
    class Program
    {
        static DateTime lastPhysicsDump = DateTime.MinValue;
        static StaticInfo? currentStaticInfo;
        static Graphics? currentGraphics;
        static string? currentSessionId;
        private static string GetSessionType(AC_SESSION_TYPE session)
        {
            return session switch
            {
                AC_SESSION_TYPE.AC_PRACTICE => "Practice",
                AC_SESSION_TYPE.AC_QUALIFY => "Qualifying",
                AC_SESSION_TYPE.AC_RACE => "Race",
                AC_SESSION_TYPE.AC_HOTLAP => "Hotlap",
                AC_SESSION_TYPE.AC_TIME_ATTACK => "TimeAttack",
                AC_SESSION_TYPE.AC_DRIFT => "Drift",
                AC_SESSION_TYPE.AC_DRAG => "Drag",
                _ => "Unknown"
            };
        }
                private static string GetAcFlag(AC_FLAG_TYPE flag)
        {
            return flag switch
            {
                AC_FLAG_TYPE.AC_BLUE_FLAG => "Blue_flag",
                AC_FLAG_TYPE.AC_YELLOW_FLAG => "Yellow_flag",
                AC_FLAG_TYPE.AC_BLACK_FLAG => "Blcak_flag",
                AC_FLAG_TYPE.AC_WHITE_FLAG => "White_flag",
                AC_FLAG_TYPE.AC_CHECKERED_FLAG => "Checked_flag",
                AC_FLAG_TYPE.AC_PENALTY_FLAG => "Penalty_flag",
                AC_FLAG_TYPE.AC_NO_FLAG => "No_flag",
                _ => "AC_NO_FLAG"
            };
        }
            private static AcTelemetryLogstash ConvertToLogstash(AcTelemetry telemetry)
            {
                return new AcTelemetryLogstash
                {
                    timestamp = telemetry.timestamp,
                    session = new AcTelemetryLogstash.Session
                    {
                        short_comment = telemetry.session.short_comment,
                        driver = telemetry.session.driver,
                        car = telemetry.session.car,
                        session_id = telemetry.session.session_id,
                        session_type = GetSessionType(currentGraphics.HasValue ? currentGraphics.Value.Session : AC_SESSION_TYPE.AC_UNKNOWN),
                        flag_type = GetAcFlag(currentGraphics.HasValue ? currentGraphics.Value.Flag : AC_FLAG_TYPE.AC_NO_FLAG),

                        completed_laps = telemetry.session.completed_laps,
                        position = telemetry.session.position,
                        icurrent_time = telemetry.session.icurrent_time,
                        session_time_left = telemetry.session.session_time_left,
                        current_sector_index = telemetry.session.current_sector_index,
                        number_of_laps = telemetry.session.number_of_laps,
                        penalty_time = telemetry.session.penalty_time,
                        track = telemetry.session.track,

                        lap_time = telemetry.session.lap_time,
                        best_lap_time = telemetry.session.best_lap_time,
                        is_valid_lap = telemetry.session.is_valid_lap,
                        track_spline_length = telemetry.session.track_spline_length,
                    },
                    metrics = new AcTelemetryLogstash.Metrics
                    {
                        // Physics + shared
                        fuel_used_total = telemetry.metrics.fuel_used_total,
                        fuel_per_100km = telemetry.metrics.fuel_per_100km,

                        packet_id = telemetry.metrics.packet_id,
                        heading = telemetry.metrics.heading,
                        pit_limiter_on = telemetry.metrics.pit_limiter_on,
                        kers_charge = telemetry.metrics.kers_charge,
                        kers_input = telemetry.metrics.kers_input,
                        turbo_boost = telemetry.metrics.turbo_boost,
                        ballast = telemetry.metrics.ballast,
                        final_ff = telemetry.metrics.final_ff,
                        performance_meter = telemetry.metrics.performance_meter,
                        engine_brake = telemetry.metrics.engine_brake,
                        ers_recovery_level = telemetry.metrics.ers_recovery_level,
                        ers_power_level = telemetry.metrics.ers_power_level,
                        ers_heat_charging = telemetry.metrics.ers_heat_charging,
                        ers_is_charging = telemetry.metrics.ers_is_charging,
                        kers_current_kj = telemetry.metrics.kers_current_kj,
                        drs_enabled = telemetry.metrics.drs_enabled,
                        tyre_temp_i = telemetry.metrics.tyre_temp_i,
                        is_ai_controlled = telemetry.metrics.is_ai_controlled,
                        local_velocity = telemetry.metrics.local_velocity,
                        velocity = telemetry.metrics.velocity,
                        tyre_dirty_level = telemetry.metrics.tyre_dirty_level,

                        // Graphics
                        current_time = telemetry.metrics.current_time,
                        last_time = telemetry.metrics.last_time,
                        best_time = telemetry.metrics.best_time,
                        split = telemetry.metrics.split,
                        distance_traveled = telemetry.metrics.distance_traveled,
                        is_in_pit = telemetry.metrics.is_in_pit,
                        last_sector_time = telemetry.metrics.last_sector_time,
                        tyre_compound = telemetry.metrics.tyre_compound,
                        replay_time_multiplier = telemetry.metrics.replay_time_multiplier,
                        normalized_car_position = telemetry.metrics.normalized_car_position,
                        car_coordinates = telemetry.metrics.car_coordinates,
                        ideal_line_on = telemetry.metrics.ideal_line_on,
                        is_in_pit_lane = telemetry.metrics.is_in_pit_lane,
                        surface_grip = telemetry.metrics.surface_grip,
                        mandatory_pit_done = telemetry.metrics.mandatory_pit_done,

                        // StaticInfo
                        ac_version = telemetry.metrics.ac_version,
                        max_power = telemetry.metrics.max_power,
                        max_torque = telemetry.metrics.max_torque,
                        max_rpm = telemetry.metrics.max_rpm,
                        kers = telemetry.metrics.kers,
                        ers = telemetry.metrics.ers,

                        // Shared metrics
                        number_of_tyres_out = telemetry.metrics.number_of_tyres_out,
                        tyre_wear_fl = telemetry.metrics.tyre_wear_fl,
                        tyre_wear_fr = telemetry.metrics.tyre_wear_fr,
                        tyre_wear_rl = telemetry.metrics.tyre_wear_rl,
                        tyre_wear_rr = telemetry.metrics.tyre_wear_rr,
                        tire_temp_core_fl = telemetry.metrics.tire_temp_core_fl,
                        tire_temp_core_fr = telemetry.metrics.tire_temp_core_fr,
                        tire_temp_core_rl = telemetry.metrics.tire_temp_core_rl,
                        tire_temp_core_rr = telemetry.metrics.tire_temp_core_rr,
                        tire_pressure_fl = telemetry.metrics.tire_pressure_fl,
                        tire_pressure_fr = telemetry.metrics.tire_pressure_fr,
                        tire_pressure_rl = telemetry.metrics.tire_pressure_rl,
                        tire_pressure_rr = telemetry.metrics.tire_pressure_rr,

                        abs = telemetry.metrics.abs,
                        brake_temp = telemetry.metrics.brake_temp,
                        wind_speed = telemetry.metrics.wind_speed,
                        tc = telemetry.metrics.tc,
                        gear = telemetry.metrics.gear,
                        drs = telemetry.metrics.drs_enabled,
                        aid_auto_shift = telemetry.metrics.aid_auto_shift,
                        fuel_level = telemetry.metrics.fuel_level,
                        throttle_pos = telemetry.metrics.throttle_pos,
                        clutch_pos = telemetry.metrics.clutch_pos,
                        brake_pos = telemetry.metrics.brake_pos,
                        engine_rpm = telemetry.metrics.engine_rpm,
                        speed = telemetry.metrics.speed,
                        road_temp = telemetry.metrics.road_temp,
                        air_temp = telemetry.metrics.air_temp,
                        air_density = telemetry.metrics.air_density,

                        ride_height_fl = telemetry.metrics.ride_height_fl,
                        ride_height_fr = telemetry.metrics.ride_height_fr,
                        ride_height_rl = telemetry.metrics.ride_height_rl,
                        ride_height_rr = telemetry.metrics.ride_height_rr,

                        suspension_travel_fl = telemetry.metrics.suspension_travel_fl,
                        suspension_travel_fr = telemetry.metrics.suspension_travel_fr,
                        suspension_travel_rl = telemetry.metrics.suspension_travel_rl,
                        suspension_travel_rr = telemetry.metrics.suspension_travel_rr,

                        tire_radius_fl = telemetry.metrics.tire_radius_fl,
                        tire_radius_fr = telemetry.metrics.tire_radius_fr,
                        tire_radius_rl = telemetry.metrics.tire_radius_rl,
                        tire_radius_rr = telemetry.metrics.tire_radius_rr,

                        tire_load_fl = telemetry.metrics.tire_load_fl,
                        tire_load_fr = telemetry.metrics.tire_load_fr,
                        tire_load_rl = telemetry.metrics.tire_load_rl,
                        tire_load_rr = telemetry.metrics.tire_load_rr,

                        tire_temp_inner_fl = telemetry.metrics.tire_temp_inner_fl,
                        tire_temp_inner_fr = telemetry.metrics.tire_temp_inner_fr,
                        tire_temp_inner_rl = telemetry.metrics.tire_temp_inner_rl,
                        tire_temp_inner_rr = telemetry.metrics.tire_temp_inner_rr,

                        tire_temp_middle_fl = telemetry.metrics.tire_temp_middle_fl,
                        tire_temp_middle_fr = telemetry.metrics.tire_temp_middle_fr,
                        tire_temp_middle_rl = telemetry.metrics.tire_temp_middle_rl,
                        tire_temp_middle_rr = telemetry.metrics.tire_temp_middle_rr,

                        tire_temp_outer_fl = telemetry.metrics.tire_temp_outer_fl,
                        tire_temp_outer_fr = telemetry.metrics.tire_temp_outer_fr,
                        tire_temp_outer_rl = telemetry.metrics.tire_temp_outer_rl,
                        tire_temp_outer_rr = telemetry.metrics.tire_temp_outer_rr,

                        tire_slip_ratio_fl = telemetry.metrics.tire_slip_ratio_fl,
                        tire_slip_ratio_fr = telemetry.metrics.tire_slip_ratio_fr,
                        tire_slip_ratio_rl = telemetry.metrics.tire_slip_ratio_rl,
                        tire_slip_ratio_rr = telemetry.metrics.tire_slip_ratio_rr,

                        tire_slip_angle_fl = telemetry.metrics.tire_slip_angle_fl,
                        tire_slip_angle_fr = telemetry.metrics.tire_slip_angle_fr,
                        tire_slip_angle_rl = telemetry.metrics.tire_slip_angle_rl,
                        tire_slip_angle_rr = telemetry.metrics.tire_slip_angle_rr,

                        camber_fl = telemetry.metrics.camber_fl,
                        camber_fr = telemetry.metrics.camber_fr,
                        camber_rl = telemetry.metrics.camber_rl,
                        camber_rr = telemetry.metrics.camber_rr,

                        steering_angle = telemetry.metrics.steering_angle,
                        brake_bias = telemetry.metrics.brake_bias,

                        cg_height = telemetry.metrics.cg_height,
                        cg_accel_longitudinal = telemetry.metrics.cg_accel_longitudinal,
                        cg_accel_lateral = telemetry.metrics.cg_accel_lateral,
                        cg_accel_vertical = telemetry.metrics.cg_accel_vertical,

                        chassis_pitch_angle = telemetry.metrics.chassis_pitch_angle,
                        chassis_roll_angle = telemetry.metrics.chassis_roll_angle,
                        chassis_yaw_rate = telemetry.metrics.chassis_yaw_rate,
                        chassis_pitch_rate = telemetry.metrics.chassis_pitch_rate,
                        chassis_roll_rate = telemetry.metrics.chassis_roll_rate,

                        wheel_angular_speed_fl = telemetry.metrics.wheel_angular_speed_fl,
                        wheel_angular_speed_fr = telemetry.metrics.wheel_angular_speed_fr,
                        wheel_angular_speed_rl = telemetry.metrics.wheel_angular_speed_rl,
                        wheel_angular_speed_rr = telemetry.metrics.wheel_angular_speed_rr,

                        car_damage_front = telemetry.metrics.car_damage_front,
                        car_damage_rear = telemetry.metrics.car_damage_rear,
                        car_damage_left = telemetry.metrics.car_damage_left,
                        car_damage_right = telemetry.metrics.car_damage_right
                    }
                };
            }


        static void Main(string[] args)

        {
            var config = AppConfig.Load();

            AssettoCorsa ac = new AssettoCorsa();

            // Интервалы обновления
            ac.StaticInfoInterval = 10000; // 10 сек
            ac.GraphicsInterval = 10000;   // 10 сек
            ac.PhysicsInterval = 1000;     // 1 сек

            // Подписка на события
            ac.StaticInfoUpdated += Ac_StaticInfoUpdated;
            ac.GraphicsUpdated += Ac_GraphicsUpdated;
            ac.PhysicsUpdated += async (s, e) => await Ac_PhysicsUpdated(s!, e, config);
            ac.GameStatusChanged += Ac_GameStatusChanged;

            ac.Start();

            //Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            ac.Stop();
            
        }
        
        private static bool sessionActive = false;
        private static bool isGameActive = false;
        private static void Ac_GameStatusChanged(object? sender, GameStatusEventArgs e)
        {
            Console.WriteLine($"Game status changed: {e.GameStatus}");
            if (e.GameStatus == AC_STATUS.AC_LIVE)
            {
                // Сгенерировать новый session_id
                currentSessionId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                sessionActive = true; // <-- включаем флаг
                isGameActive = true;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[SESSION] New session started: {currentSessionId}");
                Console.ResetColor();
            }
            else
            {
                sessionActive = false;
                isGameActive = false;

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("[SESSION] Session ended, waiting for next one...");
                Console.ResetColor();
            }
        }

        private static void Ac_StaticInfoUpdated(object? sender, StaticInfoEventArgs e)
        {
            if (!sessionActive)
                return;
            currentStaticInfo = e.StaticInfo;
            var json = JsonConvert.SerializeObject(currentStaticInfo, Formatting.Indented);
        }

        private static void Ac_GraphicsUpdated(object? sender, GraphicsEventArgs e)
        {
            if (!sessionActive)
                return;
            currentGraphics = e.Graphics;
            var dict = FlattenObject(currentGraphics) ?? new Dictionary<string, object?>();
            var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
        }

        private static async Task Ac_PhysicsUpdated(object? sender, PhysicsEventArgs e, AppConfig config)
        {
            
            if (!isGameActive)
            {
                return;
            }
            if (!sessionActive)
            {
                return;
            }
            if ((DateTime.Now - lastPhysicsDump).TotalMilliseconds < 1000)
                return;

            lastPhysicsDump = DateTime.Now;

            if (currentStaticInfo == null || currentGraphics == null)
                return;

            var log = TelemetryParser.Parse(
                staticInfo: currentStaticInfo,
                graphics: currentGraphics!,
                physics: e.Physics,
                sessionId: currentSessionId ?? "unknown");

            await SendToLogstashAsync(ConvertToLogstash(log), config);

            var json = JsonConvert.SerializeObject(log, Formatting.Indented);

        }

        /// <summary>
        /// Рекурсивно преобразует объект в Dictionary, включая массивы и вложенные структуры
        /// </summary>
        private static Dictionary<string, object?> FlattenObject(object? obj)
        {
            if (obj == null) return new Dictionary<string, object?>();

            var dict = new Dictionary<string, object?>();
            var type = obj.GetType();

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = field.GetValue(obj);
                if (value == null)
                {
                    dict[field.Name] = null;
                }
                else if (field.FieldType.IsArray)
                {
                    var arr = value as Array;
                    var list = new List<object?>();
                    foreach (var item in arr!)
                    {
                        if (item.GetType().IsPrimitive || item.GetType() == typeof(string))
                            list.Add(item);
                        else
                            list.Add(FlattenObject(item));
                    }
                    dict[field.Name] = list;
                }
                else if (field.FieldType.IsPrimitive || field.FieldType == typeof(string))
                {
                    dict[field.Name] = value;
                }
                else
                {
                    dict[field.Name] = FlattenObject(value);
                }
            }

            return dict;
        }

        /// <summary>
        /// Отправка JSON в Logstash по UDP с логированием результата
        /// </summary>
        private static async Task SendToLogstashAsync(AcTelemetryLogstash log, AppConfig config)
        {
            try
            {
                using var udpClient = new UdpClient();
                var json = JsonConvert.SerializeObject(log);
                var bytes = Encoding.UTF8.GetBytes(json);

                int sent = await udpClient.SendAsync(bytes, bytes.Length, config.logstashHost, config.logstashPort);

            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}
