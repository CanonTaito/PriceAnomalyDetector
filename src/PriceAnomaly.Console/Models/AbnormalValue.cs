namespace PriceAnomaly.Console.Models;

public sealed record AbnormalValue(string FileName, string Date, decimal Value, decimal Median);
