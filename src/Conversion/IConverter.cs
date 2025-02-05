﻿using Common;
using Common.Dto;
using Common.Dto.Garmin;
using Common.Dto.Peloton;
using Common.Helpers;
using Common.Observe;
using Common.Service;
using Common.Stateful;
using Dynastream.Fit;
using Prometheus;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Metrics = Prometheus.Metrics;
using Summary = Common.Dto.Peloton.Summary;

namespace Conversion
{
	public interface IConverter
	{
		Task<ConvertStatus> ConvertAsync(P2GWorkout workoutData);
	}

	public abstract class Converter<T> : IConverter
	{
		private static readonly Histogram WorkoutsConverted = Metrics.CreateHistogram($"{Statics.MetricPrefix}_workouts_converted_duration_seconds", "The histogram of workouts converted.", new HistogramConfiguration()
		{
			LabelNames = new string[] { Common.Observe.Metrics.Label.FileType }
		});

		private static readonly ILogger _logger = LogContext.ForClass<Converter<T>>();

		private static readonly GarminDeviceInfo RowingDevice = new GarminDeviceInfo()
		{
			Name = "Epix", // Max 20 Chars
			ProductID = GarminProduct.EpixGen2,
			UnitId = 3413684246,
			ManufacturerId = 1, // Garmin
			Version = new GarminDeviceVersion()
			{
				VersionMajor = 10,
				VersionMinor = 43,
				BuildMajor = 0,
				BuildMinor = 0,
			}
		};

		private static readonly GarminDeviceInfo CyclingDevice = new GarminDeviceInfo()
		{
			Name = "TacxTrainingAppWin", // Max 20 Chars
			ProductID = GarminProduct.TacxTrainingAppWin,
			UnitId = 1,
			ManufacturerId = 89, // Tacx
			Version = new GarminDeviceVersion()
			{
				VersionMajor = 1,
				VersionMinor = 30,
				BuildMajor = 0,
				BuildMinor = 0,
			}
		};

		private static readonly GarminDeviceInfo DefaultDevice = new GarminDeviceInfo()
		{
			Name = "Forerunner 945", // Max 20 Chars
			ProductID = GarminProduct.Fr945,
			UnitId = 1,
			ManufacturerId = 1, // Garmin
			Version = new GarminDeviceVersion()
			{
				VersionMajor = 19,
				VersionMinor = 2,
				BuildMajor = 0,
				BuildMinor = 0,
			}
		};

		public static readonly float _metersPerMile = 1609.34f;

		public FileFormat Format { get; init; }

		protected readonly ISettingsService _settingsService;
		protected readonly IFileHandling _fileHandler;

		public Converter(ISettingsService settingsService, IFileHandling fileHandler)
		{
			_settingsService = settingsService;
			_fileHandler = fileHandler;
		}

		protected abstract bool ShouldConvert(Format settings);

		protected abstract Task<T> ConvertInternalAsync(P2GWorkout workoutData, Settings settings);

		protected abstract void Save(T data, string path);

		public async Task<ConvertStatus> ConvertAsync(P2GWorkout workoutData)
		{
			using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(IConverter)}.{nameof(ConvertAsync)}.Workout")?
										.WithWorkoutId(workoutData.Workout.Id)
										.WithTag(TagKey.Format, Format.ToString());

			var status = new ConvertStatus();
			var settings = await _settingsService.GetSettingsAsync();

			if (!ShouldConvert(settings.Format))
			{
				status.Result = ConversionResult.Skipped;
				return status;
			}

			// call internal convert method
			T converted = default;
			string workoutTitle = WorkoutHelper.GetUniqueTitle(workoutData.Workout, settings.Format);
			try
			{
				converted = await ConvertInternalAsync(workoutData, settings);
			}
			catch (Exception e)
			{
				_logger.Error(e, "Failed to convert workout data to format {@Format} {@Workout}", Format, workoutTitle);
				status.Result = ConversionResult.Failed;
				status.ErrorMessage = $"Unknown error while trying to convert workout data for {workoutTitle} - {e.Message}";
				tracing?.AddTag("exception.message", e.Message);
				tracing?.AddTag("exception.stacktrace", e.StackTrace);
				tracing?.AddTag("convert.success", false);
				tracing?.AddTag("convert.errormessage", status.ErrorMessage);
				return status;
			}

			// write to output dir
			string path = Path.Join(settings.App.WorkingDirectory, $"{workoutTitle}.{Format}");
			try
			{
				_fileHandler.MkDirIfNotExists(settings.App.WorkingDirectory);
				Save(converted, path);
				status.Result = ConversionResult.Success;
			}
			catch (Exception e)
			{
				status.Result = ConversionResult.Failed;
				status.ErrorMessage = $"Failed to save converted workout {workoutTitle} for upload. - {e.Message}";
				_logger.Error(e, "Failed to write {@Format} file for {@Workout}", Format, workoutTitle);
				tracing?.AddTag("excetpion.message", e.Message);
				tracing?.AddTag("exception.stacktrace", e.StackTrace);
				tracing?.AddTag("convert.success", false);
				tracing?.AddTag("convert.errormessage", status.ErrorMessage);
				return status;
			}

			// copy to local save
			if (settings.Format.SaveLocalCopy)
				CopyToLocalSaveDir(path, workoutTitle, settings);

			// copy to upload dir
			if (settings.Garmin.Upload && settings.Garmin.FormatToUpload == Format)
			{
				try
				{
					string uploadDest = Path.Join(settings.App.UploadDirectory, $"{workoutTitle}.{Format.ToString().ToLower()}");
					_fileHandler.MkDirIfNotExists(settings.App.UploadDirectory);
					_fileHandler.Copy(path, uploadDest, overwrite: true);
					_logger.Debug("Prepped {@Format} for upload: {@Path}", Format, uploadDest);
				}
				catch (Exception e)
				{
					_logger.Error(e, "Failed to copy {@Format} file for {@Workout}", Format, workoutTitle);
					status.Result = ConversionResult.Failed;
					status.ErrorMessage = $"Failed to save file for {Format} and workout {workoutTitle} to Upload directory - {e.Message}";
					tracing?.AddTag("excetpion.message", e.Message);
					tracing?.AddTag("exception.stacktrace", e.StackTrace);
					tracing?.AddTag("convert.success", false);
					tracing?.AddTag("convert.errormessage", status.ErrorMessage);
					return status;
				}
			}

			return status;
		}

		protected void CopyToLocalSaveDir(string sourcePath, string workoutTitle, Settings settings)
		{
			using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(FitConverter)}.{nameof(CopyToLocalSaveDir)}")
										.WithTag(TagKey.Format, FileFormat.Json.ToString());

			string formatString = Format.ToString().ToLower();
			string localSaveDir = Path.GetFullPath(Path.Join(settings.App.OutputDirectory, formatString));

			try
			{
				_fileHandler.MkDirIfNotExists(localSaveDir);

				string backupDest = Path.Join(localSaveDir, $"{workoutTitle}.{formatString}");
				_fileHandler.Copy(sourcePath, backupDest, overwrite: true);
				_logger.Information("[{@Format}] Backed up file {@File}", Format, backupDest);
			}
			catch (Exception e)
			{
				_logger.Error(e, "Failed to backup {@Format} file for {@Workout} to directory {@Path}", Format, workoutTitle, localSaveDir);
			}
		}

		protected System.DateTime GetStartTimeUtc(Workout workout)
		{
			long startTimeInSeconds = workout.Start_Time;
			var dateTime = DateTimeOffset.FromUnixTimeSeconds(startTimeInSeconds);
			return dateTime.UtcDateTime;
		}

		protected System.DateTime GetEndTimeUtc(Workout workout, WorkoutSamples workoutSamples)
		{
			long endTimeSeconds = workout.End_Time ?? workoutSamples.Duration + workout.Start_Time;
			var dateTime = DateTimeOffset.FromUnixTimeSeconds(endTimeSeconds);
			return dateTime.UtcDateTime;
		}

		protected string GetTimeStamp(System.DateTime startTime, long offset = 0)
		{
			return startTime.AddSeconds(offset).ToString("yyyy-MM-ddTHH:mm:ssZ");
		}

		public static float ConvertDistanceToMeters(double value, string unit)
		{
			DistanceUnit distanceUnit = UnitHelpers.GetDistanceUnit(unit);
			switch (distanceUnit)
			{
				case DistanceUnit.Kilometers:
					return (float)value * 1000;
				case DistanceUnit.Miles:
					return (float)value * _metersPerMile;
				case DistanceUnit.Feet:
					return (float)value * 0.3048f;
				case DistanceUnit.FiveHundredMeters:
					return (float)value / 500;
				case DistanceUnit.Meters:
				default:
					return (float)value;
			}
		}

		public static float ConvertWeightToKilograms(double value, string unit)
		{
			WeightUnit weightUnit = UnitHelpers.GetWeightUnit(unit);
			switch (weightUnit)
			{
				case WeightUnit.Pounds:
					return (float)(value * 0.453592);
				case WeightUnit.Kilograms:
				default:
					return (float)value;
			}
		}

		public static float GetTotalDistance(WorkoutSamples workoutSamples)
		{
			Summary distanceSummary = GetDistanceSummary(workoutSamples);
			if (distanceSummary is null) return 0.0f;

			string unit = distanceSummary.Display_Unit;
			return ConvertDistanceToMeters(distanceSummary.Value.GetValueOrDefault(), unit);
		}

		public static float ConvertToMetersPerSecond(double? value, string displayUnit)
		{
			float val = (float)value.GetValueOrDefault();
			if (val <= 0) return 0.0f;

			SpeedUnit unit = UnitHelpers.GetSpeedUnit(displayUnit);

			switch(unit)
			{
				case SpeedUnit.KilometersPerHour:
				case SpeedUnit.MilesPerHour:
					float meters = ConvertDistanceToMeters(val, displayUnit);
					float metersPerMinute = meters / 60;
					float metersPerSecond = metersPerMinute / 60;
					return metersPerSecond;
				case SpeedUnit.MinutesPer500Meters:
					float secondsPer500m = val * 60f;
					float mps = 500 / secondsPer500m;
					return mps;
				default:
					Log.Error("Found unknown speed unit {@Unit}", unit);
					return 0;
			}
		}

		private static Summary GetDistanceSummary(WorkoutSamples workoutSamples)
		{
			if (workoutSamples?.Summaries is null)
			{
				_logger.Verbose("No workout Summaries found.");
				return null;
			}

			System.Collections.Generic.ICollection<Summary> summaries = workoutSamples.Summaries;
			Summary distanceSummary = summaries.FirstOrDefault(s => s.Slug == "distance");
			if (distanceSummary is null)
				_logger.Verbose("No distance slug found.");

			return distanceSummary;
		}

		public static Summary GetCalorieSummary(WorkoutSamples workoutSamples)
		{
			if (workoutSamples?.Summaries is null)
			{
				_logger.Verbose("No workout Summaries found.");
				return null;
			}

			System.Collections.Generic.ICollection<Summary> summaries = workoutSamples.Summaries;
			Summary caloriesSummary = summaries.FirstOrDefault(s => s.Slug == "calories");
			if (caloriesSummary is not null)
				return caloriesSummary;

			// calories may have been provided by Apple Watch
			caloriesSummary = summaries.FirstOrDefault(s => s.Slug == "total_calories");
			if (caloriesSummary is not null)
				return caloriesSummary;

			_logger.Verbose("No calories slug found.");
			return null;
		}

		protected float GetMaxSpeedMetersPerSecond(WorkoutSamples workoutSamples)
		{
			Metric speedSummary = GetSpeedSummary(workoutSamples);
			if (speedSummary is null) return 0.0f;

			double max = speedSummary.Max_Value.GetValueOrDefault();
			return ConvertToMetersPerSecond(max, speedSummary.Display_Unit);
		}

		protected float GetAvgSpeedMetersPerSecond(WorkoutSamples workoutSamples)
		{
			Metric speedSummary = GetSpeedSummary(workoutSamples);
			if (speedSummary is null) return 0.0f;

			double avg = speedSummary.Average_Value.GetValueOrDefault();
			return ConvertToMetersPerSecond(avg, speedSummary.Display_Unit);
		}

		protected float GetAvgGrade(WorkoutSamples workoutSamples)
		{
			Metric gradeSummary = GetGradeSummary(workoutSamples);
			if (gradeSummary is null) return 0.0f;

			return (float)gradeSummary.Average_Value;
		}
		protected float GetMaxGrade(WorkoutSamples workoutSamples)
		{
			Metric gradeSummary = GetGradeSummary(workoutSamples);
			if (gradeSummary is null) return 0.0f;

			return (float)gradeSummary.Max_Value;
		}

		protected Metric GetGradeSummary(WorkoutSamples workoutSamples)
		{
			return GetMetric("incline", workoutSamples);
		}

		protected Metric GetSpeedSummary(WorkoutSamples workoutSamples)
		{
			Metric speed = GetMetric("speed", workoutSamples);

			if (speed is null)
				speed = GetMetric("split_pace", workoutSamples);

			return speed;
		}

		protected byte? GetUserMaxHeartRate(WorkoutSamples workoutSamples) 
		{
			Zone maxZone = GetHeartRateZone(5, workoutSamples);

			return (byte?)maxZone?.Max_Value;
		}

		protected Zone GetHeartRateZone(int zone, WorkoutSamples workoutSamples)
		{
			Metric hrData = GetHeartRateSummary(workoutSamples);

			if (hrData is null) return null;

			return hrData.Zones?.FirstOrDefault(z => z.Slug == $"zone{zone}");
		}

		protected PowerZones CalculatePowerZones(Workout workout)
		{
			ushort? ftpMaybe = workout.Ftp_Info?.Ftp;
			if (ftpMaybe is null || ftpMaybe <= 0) return null;

			ushort ftp = ftpMaybe.Value;
			return new PowerZones()
			{
				Zone1 = new Zone()
				{
					Slug = "zone1",
					Min_Value = 0,
					Max_Value = 0.55 * ftp,
				},
				Zone2 = new Zone()
				{
					Slug = "zone2",
					Min_Value = 0.56 * ftp,
					Max_Value = 0.75 * ftp
				},
				Zone3 = new Zone()
				{
					Slug = "zone3",
					Min_Value = 0.76 * ftp,
					Max_Value = 0.90 * ftp
				},
				Zone4 = new Zone()
				{
					Slug = "zone4",
					Min_Value = 0.91 * ftp,
					Max_Value = 1.05 * ftp
				},
				Zone5 = new Zone()
				{
					Slug = "zone5",
					Min_Value = 1.06 * ftp,
					Max_Value = 1.20 * ftp
				},
				Zone6 = new Zone()
				{
					Slug = "zone6",
					Min_Value = 1.21 * ftp,
					Max_Value = 1.5 * ftp
				},
				Zone7 = new Zone()
				{
					Slug = "zone7",
					Min_Value = 1.51 * ftp,
					Max_Value = int.MaxValue
				}
			};
		}

		protected PowerZones GetTimeInPowerZones(Workout workout, WorkoutSamples workoutSamples)
		{
			Metric powerZoneData = GetOutputSummary(workoutSamples);

			if (powerZoneData is null) return null;

			PowerZones zones = CalculatePowerZones(workout);

			if (zones is null) return null;

			foreach (double? value in powerZoneData.Values)
			{
				if (zones.Zone1.Min_Value <= value
					&& zones.Zone1.Max_Value >= value)
				{
					zones.Zone1.Duration++;
					continue;
				}

				if (zones.Zone2.Min_Value <= value
					&& zones.Zone2.Max_Value >= value)
				{
					zones.Zone2.Duration++;
					continue;
				}

				if (zones.Zone3.Min_Value <= value
					&& zones.Zone3.Max_Value >= value)
				{
					zones.Zone3.Duration++;
					continue;
				}

				if (zones.Zone4.Min_Value <= value
					&& zones.Zone4.Max_Value >= value)
				{
					zones.Zone4.Duration++;
					continue;
				}

				if (zones.Zone5.Min_Value <= value
					&& zones.Zone5.Max_Value >= value)
				{
					zones.Zone5.Duration++;
					continue;
				}

				if (zones.Zone6.Min_Value <= value
					&& zones.Zone6.Max_Value >= value)
				{
					zones.Zone6.Duration++;
					continue;
				}

				if (zones.Zone7.Min_Value <= value
					&& zones.Zone7.Max_Value >= value)
				{
					zones.Zone7.Duration++;
					continue;
				}
			}

			return zones;
		}

		protected Metric GetOutputSummary(WorkoutSamples workoutSamples)
		{
			return GetMetric("output", workoutSamples);
		}

		protected Metric GetHeartRateSummary(WorkoutSamples workoutSamples)
		{
			return GetMetric("heart_rate", workoutSamples);
		}

		protected static Metric GetCadenceSummary(WorkoutSamples workoutSamples, Sport sport)
		{
			if (sport == Sport.Rowing)
				return GetMetric("stroke_rate", workoutSamples);

			return GetMetric("cadence", workoutSamples);
		}

		public static GraphData GetCadenceTargets(WorkoutSamples workoutSamples)
		{
			GraphData targets = workoutSamples.Target_Performance_Metrics?.Target_Graph_Metrics?.FirstOrDefault(w => w.Type == "cadence")?.Graph_Data;

			if (targets is null)
				targets = workoutSamples.Target_Performance_Metrics?.Target_Graph_Metrics?.FirstOrDefault(w => w.Type == "stroke_rate")?.Graph_Data;

			return targets;
		}

		protected Metric GetResistanceSummary(WorkoutSamples workoutSamples)
		{
			return GetMetric("resistance", workoutSamples);
		}

		protected static Metric GetMetric(string slug, WorkoutSamples workoutSamples)
		{
			if (workoutSamples?.Metrics is null)
			{
				_logger.Verbose("No workout Metrics found.");
				return null;
			}

			Metric metric = workoutSamples.Metrics.FirstOrDefault(s => s.Slug == slug);
			if (metric is null)
			{
				System.Collections.Generic.IEnumerable<Metric> alts = workoutSamples.Metrics
					.Where(w => w.Alternatives is object)
					.SelectMany(s => s.Alternatives);
				metric = alts.FirstOrDefault(s => s.Slug == slug);
			}

			if (metric is null)
				_logger.Verbose($"No {slug} found.");

			return metric;
		}

		protected async Task<GarminDeviceInfo> GetDeviceInfoAsync(FitnessDiscipline sport, Settings settings)
		{
			GarminDeviceInfo deviceInfo = null;
			deviceInfo = await _settingsService.GetCustomDeviceInfoAsync(settings.Garmin.Email);

			if (deviceInfo is null)
			{
				if (sport == FitnessDiscipline.Cycling)
					deviceInfo = CyclingDevice;
				else if (sport == FitnessDiscipline.Caesar)
					deviceInfo = RowingDevice;
				else
					deviceInfo = DefaultDevice;
			}

			_logger.Debug("Using device: {@DeviceName}, {@DeviceProdId}, {@DeviceManufacturerId}, {@DeviceVersion}", deviceInfo.Name, deviceInfo.ProductID, deviceInfo.ManufacturerId, deviceInfo.Version);

			return deviceInfo;
		}

		protected ushort? GetCyclingFtp(Workout workout, UserData userData)
		{
			ushort? ftp = null;
			if (workout?.Ftp_Info is object && workout.Ftp_Info.Ftp > 0)
			{
				ftp = workout.Ftp_Info.Ftp;

				if (workout.Ftp_Info.Ftp_Source == CyclingFtpSource.Ftp_Manual_Source)
					ftp = (ushort)Math.Round(ftp.GetValueOrDefault() * .95);
			} 
			
			if ((ftp is null || ftp <= 0) && userData is object)
			{
				if (userData.Cycling_Ftp_Source == CyclingFtpSource.Ftp_Manual_Source)
					ftp = (ushort)Math.Round(userData.Cycling_Ftp * .95);

				if (userData.Cycling_Ftp_Source == CyclingFtpSource.Ftp_Workout_Source)
					ftp = userData.Cycling_Workout_Ftp;
			}

			if (ftp is null || ftp <= 0)
			{
				ftp = userData?.Estimated_Cycling_Ftp;
			}

			return ftp;
		}

		protected static Sport GetGarminSport(Workout workout)
		{
			FitnessDiscipline fitnessDiscipline = workout.Fitness_Discipline;
			switch (fitnessDiscipline)
			{
				case FitnessDiscipline.Cycling:
				case FitnessDiscipline.Bike_Bootcamp:
					return Sport.Cycling;
				case FitnessDiscipline.Running:
					return Sport.Running;
				case FitnessDiscipline.Walking:
					return Sport.Walking;
				case FitnessDiscipline.Cardio:
				case FitnessDiscipline.Circuit:
				case FitnessDiscipline.Strength:
				case FitnessDiscipline.Stretching:
				case FitnessDiscipline.Yoga:
				case FitnessDiscipline.Meditation:
					return Sport.Training;
				case FitnessDiscipline.Caesar:
				case FitnessDiscipline.Caesar_Bootcamp:
					return Sport.Rowing;
				default:
					return Sport.Invalid;
			}
		}
	}
}
