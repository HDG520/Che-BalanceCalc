using ChemistryPlus.Models;

namespace ChemistryPlus;

public static class StoichiometryCalculator
{
    /// <summary>
    /// initialReactantMoles 数组长度 = reaction.Reactants.Count
    /// coeffs 总长度 = reactants + products 模块
    /// </summary>
    public static QuantitativeResult Compute(Reaction reaction, int[] coeffs, double[] initialReactantMoles)
    {
        var r = reaction.Reactants.Count;
        var p = reaction.Products.Count;

        // 找 limiting reagent：对每个反应物 i，计算 initialMoles[i] / coeffs[i]
        var minRatio = double.PositiveInfinity;
        for (var i = 0; i < r; i++)
        {
            if (coeffs[i] <= 0) continue;
            var ratio = initialReactantMoles[i] / coeffs[i];
            if (ratio < minRatio) minRatio = ratio;
        }

        var result = new QuantitativeResult(
            new double[p],
            new double[r],
            new double[r]
        );

        // 计算剩余与生成
        for (var i = 0; i < r; i++)
        {
            var used = coeffs[i] * minRatio;
            result.RemainingMoles[i] = initialReactantMoles[i] - used;
            result.ConversionRates[i] = used / initialReactantMoles[i];
        }
        for (var j = 0; j < p; j++)
        {
            result.ProducedMoles[j] = Math.Abs(coeffs[r + j] * minRatio);
        }

        return result;
    }
}
