namespace ProyectoFinal.Models;

public sealed class RecipeSuggestion
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Ingredients { get; init; } = string.Empty;
    public string AccentColor { get; init; } = "#E3F4E9";
}
