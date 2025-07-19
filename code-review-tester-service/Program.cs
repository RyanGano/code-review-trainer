using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "I'm ALIVE!");

app.MapGet("/tests/", (DifficultyLevel? level) => 
{
    // If no level is provided, return all available levels
    if (level == null)
    {
        return Results.Ok(Enum.GetNames<DifficultyLevel>());
    }

    return Results.Ok($"Test level: {level}");
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
