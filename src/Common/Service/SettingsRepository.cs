using Common.Database;
using Common.Dto;
using Common.Observe;
using Common.Service.Interfaces;
using Serilog;
using System.Threading.Tasks;

namespace Common.Service
{
	public class SettingsRepository : ISettingsRepository
	{
		private static readonly ILogger _logger = LogContext.ForClass<SettingsService>();

		private readonly ISettingsDb _db;

		public SettingsRepository(ISettingsDb db)
		{
			_db = db;
		}

		public async Task<Settings> GetSettingsAsync()
		{
			using System.Diagnostics.Activity tracing = Tracing.Trace($"{nameof(SettingsService)}.{nameof(GetSettingsAsync)}");

			return (await _db.GetSettingsAsync(1)) ?? new Settings(); // hardcode to admin user for now
		}

		public async Task<bool> UpdateSettingsAsync(Settings updatedSettings)
		{
			return await _db.UpsertSettingsAsync(1, updatedSettings); // hardcode to admin user for now
		}
	}
}
