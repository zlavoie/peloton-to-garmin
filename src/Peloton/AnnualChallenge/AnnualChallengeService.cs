using Common.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Peloton.AnnualChallenge;

public interface IAnnualChallengeService
{
	Task<ServiceResult<AnnualChallengeProgress>> GetAnnualChallengeProgressAsync(int userId);
}

public class AnnualChallengeService : IAnnualChallengeService
{
	private const string AnnualChallengeId = "5101dfaa50d441d682a432acd313c706";

	private IPelotonApi _pelotonApi;

	public AnnualChallengeService(IPelotonApi pelotonApi)
	{
		_pelotonApi = pelotonApi;
	}

	public async Task<ServiceResult<AnnualChallengeProgress>> GetAnnualChallengeProgressAsync(int userId)
	{
		var result = new ServiceResult<AnnualChallengeProgress>();
		result.Result = new AnnualChallengeProgress();

		PelotonChallenges joinedChallenges = await _pelotonApi.GetJoinedChallengesAsync(userId);
		if (joinedChallenges == null || joinedChallenges.Challenges.Length <= 0)
			return result;

		PelotonChallenge annualChallenge = joinedChallenges.Challenges.FirstOrDefault(c => c.Challenge_Summary.Id == AnnualChallengeId || c.Challenge_Summary.Title.StartsWith("The Annual"));
		if (annualChallenge is null)
			return result;

		PelotonUserChallengeDetail annualChallengeProgressDetail = await _pelotonApi.GetUserChallengeDetailsAsync(userId, AnnualChallengeId);
		if (annualChallengeProgressDetail is null)
			return result;

		PelotonChallengeTier[] tiers = annualChallengeProgressDetail.Challenge_Detail.Tiers;
		ChallengeProgress progress = annualChallengeProgressDetail.Progress;

		DateTime now = DateTime.UtcNow;
		DateTime startTimeUtc = DateTimeOffset.FromUnixTimeSeconds(annualChallengeProgressDetail.Challenge_Summary.Start_Time).UtcDateTime;
		DateTime endTimeUtc = DateTimeOffset.FromUnixTimeSeconds(annualChallengeProgressDetail.Challenge_Summary.End_Time).UtcDateTime;

		result.Result.HasJoined = true;
		result.Result.EarnedMinutes = progress.Metric_Value;
		result.Result.Tiers = tiers.Where(t => t.Metric_Value > 0).Select(t => 
		{
			double requiredMinutes = t.Metric_Value;
			double actualMinutes = progress.Metric_Value;
			OnTrackDetails onTrackDetails = CalculateOnTrackDetails(now, startTimeUtc, endTimeUtc, actualMinutes, requiredMinutes);

			return new Tier()
			{
				BadgeUrl = t.detailed_badge_image_url,
				Title = t.Title,
				RequiredMinutes = requiredMinutes,
				HasEarned = onTrackDetails.HasEarned,
				PercentComplete= onTrackDetails.PercentComplete,
				IsOnTrackToEarndByEndOfYear = onTrackDetails.IsOnTrackToEarnByEndOfYear,
				MinutesBehindPace = onTrackDetails.MinutesBehindPace,
				MinutesAheadOfPace = onTrackDetails.MinutesBehindPace * -1,
				MinutesNeededPerDay = onTrackDetails.MinutesNeededPerDay,
				MinutesNeededPerWeek = onTrackDetails.MinutesNeededPerDay * 7,
				MinutesNeededPerDayToFinishOnTime = onTrackDetails.MinutesNeededPerDayToFinishOnTime,
				MinutesNeededPerWeekToFinishOnTime = onTrackDetails.MinutesNeededPerDayToFinishOnTime * 7
			};
		}).ToList();

		return result;
	}

	public static OnTrackDetails CalculateOnTrackDetails(DateTime now, DateTime startTimeUtc, DateTime endTimeUtc, double earnedMinutes, double requiredMinutes)
	{
		TimeSpan totalTime = endTimeUtc - startTimeUtc;
		double totalDays = Math.Ceiling(totalTime.TotalDays);

		double minutesNeededPerDay = requiredMinutes / totalDays;

		TimeSpan elapsedTime = now - startTimeUtc;
		double elapsedDays = Math.Ceiling(elapsedTime.TotalDays);

		double neededMinutesToBeOnTrack = elapsedDays * minutesNeededPerDay;

		double remainingDays = Math.Ceiling((endTimeUtc - now).TotalDays);
		double minutesNeededPerDayToFinishOnTime = (requiredMinutes - earnedMinutes) / remainingDays;

		return new OnTrackDetails()
		{
			IsOnTrackToEarnByEndOfYear = earnedMinutes >= neededMinutesToBeOnTrack,
			MinutesBehindPace = neededMinutesToBeOnTrack - earnedMinutes,
			MinutesNeededPerDay = minutesNeededPerDay,
			HasEarned = earnedMinutes >= requiredMinutes,
			PercentComplete = earnedMinutes / requiredMinutes,
			MinutesNeededPerDayToFinishOnTime = minutesNeededPerDayToFinishOnTime
		};
	}

	public record OnTrackDetails
	{
		public bool IsOnTrackToEarnByEndOfYear { get; init; }
		public double MinutesBehindPace { get; init; }
		public double MinutesNeededPerDay { get; init; }
		public bool HasEarned { get; init; }
		public double PercentComplete { get; init; }
		public double MinutesNeededPerDayToFinishOnTime { get; init; }
	}
}
