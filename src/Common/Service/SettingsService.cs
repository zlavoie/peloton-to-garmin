using Common.Dto;
using Common.Dto.Garmin;
using Common.Observe;
using Common.Service.Interfaces;
using Common.Stateful;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Common.Service;

public class SettingsService : ISettingsService
{
	private static readonly object _lock = new();
	private static readonly string PelotonApiAuthKey = "PelotonApiAuth";
	private static readonly string GarminApiAuthKey = "GarminApiAuth";
	private static readonly string GarminDeviceInfoKey = "GarminDeviceInfo";

	private static readonly ILogger _logger = LogContext.ForClass<SettingsService>();
	private readonly IMemoryCache _cache;
	private readonly IConfiguration _configurationLoader;
	private readonly IFileHandling _fileHandler;
	private readonly ISettingsRepository _settingRepository;

	public SettingsService(
		IMemoryCache cache, 
		IConfiguration configurationLoader, 
		IFileHandling fileHandler,
		ISettingsRepository settingRepository
	)
	{
		_cache = cache;
		_configurationLoader = configurationLoader;
		_fileHandler = fileHandler;
		_settingRepository = settingRepository;
	}

	public async Task<Settings> GetSettingsAsync()
	{
		using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(SettingsService)}.{nameof(GetSettingsAsync)}");

		return await _settingRepository.GetSettingsAsync();
	}

	public async Task<Settings> UpdateSettingsAsync(Settings updatedSettings)
	{
		using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(SettingsService)}.{nameof(UpdateSettingsAsync)}");

		Settings originalSettings = await _settingRepository.GetSettingsAsync();

		if (updatedSettings.Garmin.Password is null)
			updatedSettings.Garmin.Password = originalSettings.Garmin.Password;

		if (updatedSettings.Peloton.Password is null)
			updatedSettings.Peloton.Password = originalSettings.Peloton.Password;

		//ToDo: Need to look at the cache clearing here, may be able to abstract and use simpler keys
		ClearPelotonApiAuthentication(originalSettings.Peloton.Email);
		ClearPelotonApiAuthentication(updatedSettings.Peloton.Email);

		ClearGarminAuthentication(originalSettings.Garmin.Email);
		ClearGarminAuthentication(originalSettings.Garmin.Password);

		ClearCustomDeviceInfoAsync(originalSettings.Garmin.Email);
		ClearCustomDeviceInfoAsync(updatedSettings.Garmin.Email);

		await _settingRepository.UpdateSettingsAsync(updatedSettings);

		return await _settingRepository.GetSettingsAsync();
	}

	public PelotonApiAuthentication GetPelotonApiAuthentication(string pelotonEmail)
	{
		using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(SettingsService)}.{nameof(GetPelotonApiAuthentication)}");

		lock (_lock)
		{
			string key = $"{PelotonApiAuthKey}:{pelotonEmail}";
			return _cache.Get<PelotonApiAuthentication>(key);
		}
	}

	public void SetPelotonApiAuthentication(PelotonApiAuthentication authentication)
	{
		using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(SettingsService)}.{nameof(SetPelotonApiAuthentication)}");

		lock (_lock)
		{
			string key = $"{PelotonApiAuthKey}:{authentication.Email}";
			_cache.Set(key, authentication, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) });
		}
	}

	public void ClearPelotonApiAuthentication(string pelotonEmail)
	{
		using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(SettingsService)}.{nameof(ClearPelotonApiAuthentication)}");

		lock (_lock)
		{
			string key = $"{PelotonApiAuthKey}:{pelotonEmail}";
			_cache.Remove(key);
		}
	}

	public GarminApiAuthentication GetGarminAuthentication(string garminEmail)
	{
		using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(SettingsService)}.{nameof(GetGarminAuthentication)}");

		lock (_lock)
		{
			string key = $"{GarminApiAuthKey}:{garminEmail}";
			return _cache.Get<GarminApiAuthentication>(key);
		}
	}

	public void SetGarminAuthentication(GarminApiAuthentication authentication)
	{
		using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(SettingsService)}.{nameof(SetGarminAuthentication)}");

		lock (_lock)
		{
			string key = $"{GarminApiAuthKey}:{authentication.Email}";
			int expiration = authentication.OAuth2Token?.Expires_In - (60 * 60) ?? 0; // expire an hour early
			int finalExpiration = expiration <= 0 ? 45 * 60 : expiration; // default to 45min
			_cache.Set(key, authentication, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(finalExpiration) });
		}
	}

	public void ClearGarminAuthentication(string garminEmail)
	{
		using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(SettingsService)}.{nameof(ClearGarminAuthentication)}");

		lock (_lock)
		{
			string key = $"{GarminApiAuthKey}:{garminEmail}";
			_cache.Remove(key);
		}
	}

	public Task<AppConfiguration> GetAppConfigurationAsync()
	{
		var appConfiguration = new AppConfiguration();
		ConfigurationSetup.LoadConfigValues(_configurationLoader, appConfiguration);

		return Task.FromResult(appConfiguration);
	}

	public async Task<GarminDeviceInfo> GetCustomDeviceInfoAsync(string garminEmail)
	{
		using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(SettingsService)}.{nameof(GetCustomDeviceInfoAsync)}");

		Settings settings = await _settingRepository.GetSettingsAsync();

		if (string.IsNullOrEmpty(settings.Format.DeviceInfoPath))
			return null;

		lock (_lock)
		{
			string key = $"{GarminDeviceInfoKey}:{garminEmail}";
			return _cache.GetOrCreate(key, (cacheEntry) => 
			{
				cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
				if (_fileHandler.TryDeserializeXml(settings.Format.DeviceInfoPath, out GarminDeviceInfo userProvidedDeviceInfo))
					return userProvidedDeviceInfo;

				return null;
			});
		}
	}

	private void ClearCustomDeviceInfoAsync(string garminEmail)
	{
		using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(SettingsService)}.{nameof(ClearCustomDeviceInfoAsync)}");

		lock (_lock)
		{
			string key = $"{GarminDeviceInfoKey}:{garminEmail}";
			_cache.Remove(key);
		}
	}
}
