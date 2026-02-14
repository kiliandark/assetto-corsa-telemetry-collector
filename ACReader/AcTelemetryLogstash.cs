using System;
using System.Data;
using System.Reflection.Emit;
using AssettoCorsaSharedMemory;

namespace ACReader
{
    public class AcTelemetryLogstash
    {
        public DateTime timestamp { get; set; }
        public Session session { get; set; }    
        public Metrics metrics { get; set; }

        public AcTelemetryLogstash(string sessionType = "")
        {
            timestamp = DateTime.Now;
            session = new Session(sessionType);
            metrics = new Metrics();
        }

        public class Session
        {
            public string short_comment { get; set; }
            public string driver { get; set; }
            public string car { get; set; }
            public long session_id { get; set; }
            public string session_type { get; set; }
            public string flag_type { get; set; }

            public float completed_laps { get; set; }
            public float position { get; set; }
            public float icurrent_time { get; set; }
            public float session_time_left { get; set; }
            public float current_sector_index { get; set; }
            public float number_of_laps { get; set; }
            public float penalty_time { get; set; }

            public string track { get; set; }
            public float lap_time { get; set; }
            public float best_lap_time { get; set; }
            public int is_valid_lap { get; set; }
            public float track_spline_length { get; set; }


            public Session(string sessionType = "")
            {
                short_comment = string.Empty;
                driver = string.Empty;
                car = string.Empty;
                session_id = 0;
                session_type = sessionType;
                flag_type = string.Empty;
                track = string.Empty;
            }
        }

        public class Metrics
        {
            // Physics
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

            // Graphics
            public float fuel_used_total { get; set; }
            public float fuel_per_100km { get; set; }

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
            public float max_torque { get; set; }
            public int max_rpm { get; set; }
            public float kers { get; set; }
            public float ers { get; set; }

            // Shared Metrics
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

            public bool drs_available { get; set; }
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

            // Ride height
            public float ride_height_fl { get; set; }
            public float ride_height_fr { get; set; }
            public float ride_height_rl { get; set; }
            public float ride_height_rr { get; set; }

            // Suspension travel
            public float suspension_travel_fl { get; set; }
            public float suspension_travel_fr { get; set; }
            public float suspension_travel_rl { get; set; }
            public float suspension_travel_rr { get; set; }

            // Tyres
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

            // Slip
            public float tire_slip_ratio_fl { get; set; }
            public float tire_slip_ratio_fr { get; set; }
            public float tire_slip_ratio_rl { get; set; }
            public float tire_slip_ratio_rr { get; set; }
            public float tire_slip_angle_fl { get; set; }
            public float tire_slip_angle_fr { get; set; }
            public float tire_slip_angle_rl { get; set; }
            public float tire_slip_angle_rr { get; set; }

            // Camber
            public float camber_fl { get; set; }
            public float camber_fr { get; set; }
            public float camber_rl { get; set; }
            public float camber_rr { get; set; }

            // Steering
            public float steering_angle { get; set; }

            // Brake bias
            public float brake_bias { get; set; }

            // Chassis
            public float cg_height { get; set; }
            public float cg_accel_longitudinal { get; set; }
            public float cg_accel_lateral { get; set; }
            public float cg_accel_vertical { get; set; }
            public float chassis_pitch_angle { get; set; }
            public float chassis_roll_angle { get; set; }
            public float chassis_yaw_rate { get; set; }
            public float chassis_pitch_rate { get; set; }
            public float chassis_roll_rate { get; set; }

            // Wheel speed
            public float wheel_angular_speed_fl { get; set; }
            public float wheel_angular_speed_fr { get; set; }
            public float wheel_angular_speed_rl { get; set; }
            public float wheel_angular_speed_rr { get; set; }

            // Car damage
            public float car_damage_front { get; set; }
            public float car_damage_rear { get; set; }
            public float car_damage_left { get; set; }
            public float car_damage_right { get; set; }

           public Metrics()
            {
                // Массивы
                tyre_temp_i = new float[4];
                local_velocity = new float[3];
                velocity = new float[3];
                tyre_dirty_level = new float[4];
                car_coordinates = new float[3];

                fuel_used_total = 0;
                fuel_per_100km = 0;


                // Physics / Shared Metrics
                packet_id = 0;
                heading = 0;
                pit_limiter_on = 0;
                kers_charge = 0;
                kers_input = 0;
                turbo_boost = 0;
                ballast = 0;
                final_ff = 0;
                performance_meter = 0;
                engine_brake = 0;
                ers_recovery_level = 0;
                ers_power_level = 0;
                ers_heat_charging = 0;
                ers_is_charging = 0;
                kers_current_kj = 0;
                drs_enabled = false;
                is_ai_controlled = 0;

                number_of_tyres_out = 0;
                tyre_wear_fl = 0;
                tyre_wear_fr = 0;
                tyre_wear_rl = 0;
                tyre_wear_rr = 0;

                tire_temp_core_fl = 0;
                tire_temp_core_fr = 0;
                tire_temp_core_rl = 0;
                tire_temp_core_rr = 0;

                tire_pressure_fl = 0;
                tire_pressure_fr = 0;
                tire_pressure_rl = 0;
                tire_pressure_rr = 0;

                abs = 0;
                brake_temp = 0;
                wind_speed = 0;
                tc = 0;
                gear = 1;
                drs = false;
                aid_auto_shift = false;
                fuel_level = 0;
                throttle_pos = 0;
                clutch_pos = 0;
                brake_pos = 0;
                engine_rpm = 0;
                speed = 0;
                road_temp = 0;
                air_temp = 0;
                air_density = 0;

                // Ride height
                ride_height_fl = 0;
                ride_height_fr = 0;
                ride_height_rl = 0;
                ride_height_rr = 0;

                // Suspension travel
                suspension_travel_fl = 0;
                suspension_travel_fr = 0;
                suspension_travel_rl = 0;
                suspension_travel_rr = 0;

                // Tyres
                tire_radius_fl = 0;
                tire_radius_fr = 0;
                tire_radius_rl = 0;
                tire_radius_rr = 0;
                tire_load_fl = 0;
                tire_load_fr = 0;
                tire_load_rl = 0;
                tire_load_rr = 0;
                tire_temp_inner_fl = 0;
                tire_temp_inner_fr = 0;
                tire_temp_inner_rl = 0;
                tire_temp_inner_rr = 0;
                tire_temp_middle_fl = 0;
                tire_temp_middle_fr = 0;
                tire_temp_middle_rl = 0;
                tire_temp_middle_rr = 0;
                tire_temp_outer_fl = 0;
                tire_temp_outer_fr = 0;
                tire_temp_outer_rl = 0;
                tire_temp_outer_rr = 0;

                // Slip
                tire_slip_ratio_fl = 0;
                tire_slip_ratio_fr = 0;
                tire_slip_ratio_rl = 0;
                tire_slip_ratio_rr = 0;
                tire_slip_angle_fl = 0;
                tire_slip_angle_fr = 0;
                tire_slip_angle_rl = 0;
                tire_slip_angle_rr = 0;

                // Camber
                camber_fl = 0;
                camber_fr = 0;
                camber_rl = 0;
                camber_rr = 0;

                // Steering
                steering_angle = 0;

                // Brake bias
                brake_bias = 0;

                // Chassis
                cg_height = 0;
                cg_accel_longitudinal = 0;
                cg_accel_lateral = 0;
                cg_accel_vertical = 0;
                chassis_pitch_angle = 0;
                chassis_roll_angle = 0;
                chassis_yaw_rate = 0;
                chassis_pitch_rate = 0;
                chassis_roll_rate = 0;

                // Wheel speed
                wheel_angular_speed_fl = 0;
                wheel_angular_speed_fr = 0;
                wheel_angular_speed_rl = 0;
                wheel_angular_speed_rr = 0;

                // Car damage
                car_damage_front = 0;
                car_damage_rear = 0;
                car_damage_left = 0;
                car_damage_right = 0;

                // Graphics
                current_time = string.Empty;
                last_time = string.Empty;
                best_time = string.Empty;
                split = string.Empty;
                distance_traveled = 0;
                is_in_pit = 0;
                last_sector_time = 0;
                tyre_compound = string.Empty;
                replay_time_multiplier = 0;
                normalized_car_position = 0;
                ideal_line_on = 0;
                is_in_pit_lane = 0;
                surface_grip = 0;
                mandatory_pit_done = 0;
                car_coordinates = new float[3];

                // StaticInfo
                ac_version = string.Empty;
                max_power = 0;
                max_torque = 0;
                max_rpm = 0;
                kers = 0;
                ers = 0;
            }

        }
    }
}
