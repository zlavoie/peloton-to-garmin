using Common.Stateful;
using System.Threading.Tasks;

namespace Garmin.Auth.Interfaces
{
	public interface IGarminAuthenticationService
	{
		Task<GarminApiAuthentication> GetGarminAuthenticationAsync();
		Task<GarminApiAuthentication> RefreshGarminAuthenticationAsync();
		Task<GarminApiAuthentication> CompleteMFAAuthAsync(string mfaCode);
	}
}
