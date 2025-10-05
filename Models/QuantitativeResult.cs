namespace ChemistryPlus.Models;

public struct QuantitativeResult(double[] producedMoles, double[] remainingMoles, double[] conversionRates)
{
    public readonly double[] ProducedMoles = producedMoles;
    public readonly double[] RemainingMoles = remainingMoles;
    public readonly double[] ConversionRates = conversionRates;
}