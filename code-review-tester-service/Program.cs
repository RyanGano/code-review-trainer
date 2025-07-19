using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "I'm ALIVE!");

app.MapGet("/tests/", (string? level) => 
{
    // If no level is provided, return all available levels
    if (string.IsNullOrEmpty(level))
    {
        return Results.Ok(new[] { "Easy", "Medium", "Hard", "Extra Hard" });
    }

    // Validate the level parameter
    if (!Enum.TryParse<DifficultyLevel>(level.Replace(" ", ""), true, out var parsedLevel))
    {
        return Results.BadRequest($"Invalid level '{level}'. Valid levels are: Easy, Medium, Hard, Extra Hard");
    }

    // Convert enum back to display format
    string displayLevel = parsedLevel switch
    {
        DifficultyLevel.ExtraHard => "Extra Hard",
        _ => parsedLevel.ToString()
    };

    return Results.Ok($"Test level: {displayLevel}");
})
.WithName("GetTests");

app.Run();

// Enum for difficulty levels
public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard,
    ExtraHard
}
