namespace PriceBlotter.Api.Models;

public record PriceItem(int Id, string Name, decimal Price, DateTime UpdatedAt);
