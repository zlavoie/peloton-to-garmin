using Common.Dto;
using System.Threading.Tasks;

namespace Common.Service.Interfaces
{
	public interface ISettingsRepository
	{
		Task<Settings> GetSettingsAsync();

		Task<bool> UpdateSettingsAsync(Settings updatedSettings);
	}
}
