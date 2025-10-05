using ChemistryPlus.Models;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ChemistryPlus;

public static class Balancer
{
    /// <summary>
    /// Balances a chemical reaction using linear algebra to solve the system of conservation of atoms.
    /// </summary>
    /// <param name="reaction">The reaction object containing reactants and products.</param>
    /// <returns>An array of integer coefficients for the balanced equation.</returns>
    /// <exception cref="Exception">Throws if the equation cannot be balanced.</exception>
    public static int[] Balance(Reaction reaction)
    {
        // 1. Collect all unique elements from both reactants and products.
        var allElements = new HashSet<string>();
        foreach (var molecule in reaction.Reactants.Concat(reaction.Products))
        {
            foreach (var element in molecule.ElementCounts.Keys)
            {
                allElements.Add(element);
            }
        }
        var elementList = allElements.ToList();

        if (elementList.Count == 0)
        {
            return []; // No elements, nothing to balance.
        }

        var numElements = elementList.Count;
        var numMolecules = reaction.Reactants.Count + reaction.Products.Count;

        // 2. Construct the stoichiometry matrix A (m x n), where m = numElements, n = numMolecules.
        var matrixA = DenseMatrix.OfArray(new double[numElements, numMolecules]);
        for (var i = 0; i < numElements; i++)
        {
            var element = elementList[i];
            
            // Reactants have positive counts
            for (var j = 0; j < reaction.Reactants.Count; j++)
            {
                reaction.Reactants[j].ElementCounts.TryGetValue(element, out var count);
                matrixA[i, j] = count;
            }

            // Products have negative counts to set up the equation A*x = 0
            for (var j = 0; j < reaction.Products.Count; j++)
            {
                reaction.Products[j].ElementCounts.TryGetValue(element, out var count);
                matrixA[i, reaction.Reactants.Count + j] = -count;
            }
        }

        // 3. Find the null space (kernel) of the matrix. For a uniquely balanceable
        //    equation, the null space should be one-dimensional.
        Vector<double>[] nullspace = matrixA.Kernel();

        if (nullspace.Length == 0)
        {
            throw new Exception("The chemical equation has no valid solution (it may be fundamentally unbalanced).");
        }
        if (nullspace.Length > 1)
        {
            // This indicates multiple independent reactions or a linearly dependent system.
            throw new Exception("The chemical equation is indeterminate or represents multiple independent reactions.");
        }

        var basisVector = nullspace[0];

        // 4. Convert the floating-point basis vector into the smallest possible integer ratio.
        var integerCoefficients = ToSmallestIntegers(basisVector);

        // 5. Ensure all coefficients are positive for the final result.
        // The null space vector has opposite signs for reactants vs. products.
        // A standard balanced equation uses positive integers for all species.
        return integerCoefficients.Select(Math.Abs).ToArray();
    }

    /// <summary>
    /// Converts a vector of doubles into a vector of the smallest possible integers
    /// that maintain the same ratio.
    /// </summary>
    private static int[] ToSmallestIntegers(Vector<double> v, double tolerance = 1e-6, int maxDenominator = 10000)
    {
        // Normalize the vector by the element with the largest absolute value.
        // This improves numerical stability and bounds all values between -1 and 1.
        var maxAbs = v.Select(Math.Abs).Max();
        if (maxAbs < tolerance)
        {
            return new int[v.Count]; // It's a zero vector.
        }
        var normalized = v / maxAbs;

        // Find the smallest integer denominator that makes all elements of the
        // normalized vector integers (within the tolerance).
        for (var d = 1; d <= maxDenominator; d++)
        {
            var scaled = normalized * d;
            if (scaled.ForAll(x => Math.Abs(x - Math.Round(x)) < tolerance))
            {
                // Found a valid integer ratio.
                var rounded = scaled.Select(x => (int)Math.Round(x)).ToArray();

                // Simplify the ratio by dividing by the Greatest Common Divisor (GCD).
                var nonZeroAbs = rounded.Where(x => x != 0).Select(Math.Abs).ToArray();
                if (nonZeroAbs.Length > 0)
                {
                    var gcd = GcdOfArray(nonZeroAbs);
                    for (var i = 0; i < rounded.Length; i++)
                    {
                        rounded[i] /= gcd;
                    }
                }
                
                // Ensure the first non-zero coefficient is positive for consistency.
                int firstNonZeroIndex = Array.FindIndex(rounded, x => x != 0);
                if (firstNonZeroIndex != -1 && rounded[firstNonZeroIndex] < 0)
                {
                    for (int i = 0; i < rounded.Length; i++)
                    {
                        rounded[i] *= -1;
                    }
                }

                return rounded;
            }
        }

        throw new Exception("Failed to find a simple integer ratio for the coefficients. The reaction may be complex or invalid.");
    }

    private static int Gcd(int a, int b)
    {
        return b == 0 ? a : Gcd(b, a % b);
    }

    private static int GcdOfArray(int[] arr)
    {
        if (arr.Length == 0) return 1;
        return arr.Aggregate(Gcd);
    }
}