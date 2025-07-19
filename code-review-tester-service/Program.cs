var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "I'm ALIVE!");

app.MapGet("/tests/", (string? level) => 
{
    string[] availableLevels = ["Easy", "Medium", "Hard", "Extra Hard"];
    return availableLevels;
});

app.Run();
