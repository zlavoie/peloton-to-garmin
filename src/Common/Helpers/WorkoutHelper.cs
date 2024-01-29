using Common.Dto;
using Common.Dto.Peloton;
using HandlebarsDotNet;
using System.IO;
using System.Web;

namespace Common.Helpers;

public static class WorkoutHelper
{
	public const char SpaceSeparator = '_';
	public const char InvalidCharacterReplacer = '-';
	public const char Space = ' ';

	private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

	public static string GetTitle(Workout workout, Format settings)
	{
		string rideTitle = workout.Ride?.Title ?? workout.Id;
		string instructorName = workout.Ride?.Instructor?.Name;

		var templateData = new 
		{
			PelotonWorkoutTitle = rideTitle,
			PelotonInstructorName = instructorName
		};

		string template = settings.WorkoutTitleTemplate;
		if (string.IsNullOrWhiteSpace(template))
			template = new Format().WorkoutTitleTemplate;

		HandlebarsTemplate<object, object> compiledTemplate = Handlebars.Compile(settings.WorkoutTitleTemplate);
		string title = compiledTemplate(templateData);

		string cleanedTitle = title.Replace(Space, SpaceSeparator);

		foreach (char c in InvalidFileNameChars)
		{
			cleanedTitle = cleanedTitle.Replace(c, InvalidCharacterReplacer);
		}

		string result = HttpUtility.HtmlDecode(cleanedTitle);
		return result;
	}

	public static string GetUniqueTitle(Workout workout, Format settings)
	{
		return $"{workout.Id}_{GetTitle(workout, settings)}";
	}

	public static string GetWorkoutIdFromFileName(string filePath)
	{
		string fileName = Path.GetFileNameWithoutExtension(filePath);
		string[] parts = fileName.Split(SpaceSeparator);
		return parts[0];
	}
}
