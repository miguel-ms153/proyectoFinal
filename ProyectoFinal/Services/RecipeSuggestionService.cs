using ProyectoFinal.Models;

namespace ProyectoFinal.Services;

public static class RecipeSuggestionService
{
    public static List<RecipeSuggestion> GetSuggestions(IEnumerable<ProductItem> products)
    {
        var names = products
            .Where(p => !p.IsConsumed && !p.IsWasted && p.DaysUntilExpiration is >= 0 and <= 3)
            .Select(p => p.Name.Trim().ToLowerInvariant())
            .ToList();

        var suggestions = new List<RecipeSuggestion>();
        bool Has(params string[] words) => names.Any(name => words.Any(word => name.Contains(word)));

        if (Has("leche", "yogur", "yogurt", "platano", "banana", "fruta"))
            suggestions.Add(new RecipeSuggestion { Title = "Batido nutritivo", Description = "Una opción rápida para aprovechar lácteos y frutas maduras.", Ingredients = "Usa: " + Pick(names, "leche", "yogur", "yogurt", "platano", "banana", "fruta"), AccentColor = "#E4F3FF" });

        if (Has("tomate", "lechuga", "zanahoria", "pepino", "verdura"))
            suggestions.Add(new RecipeSuggestion { Title = "Ensalada fresca", Description = "Combina tus vegetales y añade un aliño al gusto.", Ingredients = "Usa: " + Pick(names, "tomate", "lechuga", "zanahoria", "pepino", "verdura"), AccentColor = "#E5F5E9" });

        if (Has("pan", "queso", "huevo"))
            suggestions.Add(new RecipeSuggestion { Title = "Tostadas especiales", Description = "Ideal para una merienda sencilla antes del vencimiento.", Ingredients = "Usa: " + Pick(names, "pan", "queso", "huevo"), AccentColor = "#FFF0D5" });

        if (Has("arroz", "pollo", "carne", "verdura"))
            suggestions.Add(new RecipeSuggestion { Title = "Arroz salteado", Description = "Aprovecha sobras y verduras en un solo plato.", Ingredients = "Usa: " + Pick(names, "arroz", "pollo", "carne", "verdura"), AccentColor = "#F0E8FB" });

        if (suggestions.Count == 0 && names.Count > 0)
            suggestions.Add(new RecipeSuggestion { Title = "Plato de aprovechamiento", Description = "Prepara una receta con los ingredientes que vencen primero.", Ingredients = "Usa: " + string.Join(", ", names.Select(ToTitle)), AccentColor = "#E3F4E9" });

        return suggestions.Take(3).ToList();
    }

    private static string Pick(IEnumerable<string> names, params string[] words) => string.Join(", ", names.Where(name => words.Any(word => name.Contains(word))).Distinct().Select(ToTitle));
    private static string ToTitle(string value) => string.IsNullOrWhiteSpace(value) ? value : char.ToUpper(value[0]) + value[1..];
}
