using System.Text;
using ChemistryPlus.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChemistryPlus.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public static IReadOnlyCollection<string> Items =>
    [
        // A: 王水溶金
        "Au + HCl + HNO3 -> HAuCl4 + NO + H2O",
        // B: 酸碱中和与沉淀
        "Ba(OH)2 + H2SO4 -> BaSO4 + H2O",
        // C: 碳氢化合物燃烧
        "C3H8 + O2 -> CO2 + H2O",
        // D: 氘（氢的同位素），测试非标准元素符号
        "D2 + O2 -> D2O",
        // E: 稀土元素氧化物与酸反应
        "Er2O3 + HCl -> ErCl3 + H2O",
        // F: 铁的氧化
        "Fe + O2 -> Fe3O4",
        // G: 锗烷燃烧
        "GeH4 + O2 -> GeO2 + H2O",
        // H: 氢气燃烧（基础反应）
        "H2 + O2 -> H2O",
        // I: 碘量法滴定（经典氧化还原）
        "I2 + Na2S2O3 -> NaI + Na2S4O6",
        // J: (没有以J开头的常见元素)
        // K: 氯酸钾分解
        "KClO3 -> KCl + O2",
        // L: 锂与水反应
        "Li + H2O -> LiOH + H2",
        // M: 镁与酸反应（置换反应）
        "Mg + HCl -> MgCl2 + H2",
        // N: 氨气催化氧化（工业制硝酸）
        "NH3 + O2 -> NO + H2O",
        // O: 臭氧分解
        "O3 -> O2",
        // P: 白磷燃烧（测试多原子分子 P4）
        "P4 + O2 -> P4O10",
        // Q: (没有以Q开头的常见元素)
        // R: 铷与水反应
        "Rb + H2O -> RbOH + H2",
        // S: 硫燃烧（测试多原子分子 S8）
        "S8 + O2 -> SO2",
        // T: 钛的氯化（工业制备）
        "TiO2 + C + Cl2 -> TiCl4 + CO",
        // U: 六氟化铀的水解
        "UF6 + H2O -> UO2F2 + HF",
        // V: 五氧化二钒与盐酸反应
        "V2O5 + HCl -> VOCl3 + H2O",
        // W: 钨的氢气还原法 (Wolfram)
        "WO3 + H2 -> W + H2O",
        // X: 氙的氟化物水解
        "XeF6 + H2O -> XeO3 + HF",
        // Y: 氧化钇与硝酸反应
        "Y2O3 + HNO3 -> Y(NO3)3 + H2O",
        // Z: 锌与酸反应
        "Zn + HCl -> ZnCl2 + H2",

        // ---- 特殊情况测试 ----
        // 1. 复杂的氧化还原反应 (系数较大)
        "KMnO4 + HCl -> KCl + MnCl2 + H2O + Cl2",
        // 3. 涉及括号的复杂化合物
        "Ca3(PO4)2 + SiO2 + C -> CaSiO3 + P4 + CO",
        // 4. “火山”实验 (分解反应)
        "(NH4)2Cr2O7 -> Cr2O3 + N2 + H2O"
    ];

    [ObservableProperty] public partial string ReactionInput { get; set; } = string.Empty;

    [ObservableProperty] public partial string WarningMessage { get; set; } = string.Empty;

    [ObservableProperty] public partial string InitialMolesInput { get; set; } = string.Empty;

    [ObservableProperty] public partial string CoefficientsResult { get; set; } = string.Empty;

    [ObservableProperty] public partial string ProductsResult { get; set; } = string.Empty;

    [RelayCommand]
    private void Compute()
    {
        // Clear previous results and warnings at the start
        WarningMessage = string.Empty;
        CoefficientsResult = string.Empty;
        ProductsResult = string.Empty;

        try
        {
            if (string.IsNullOrWhiteSpace(ReactionInput))
            {
                WarningMessage = "请输入反应式";
                return;
            }

            // 1. Parse and balance the reaction. This is the primary action.
            var reaction = ChemicalParser.ParseReaction(ReactionInput);
            var coeffs = Balancer.Balance(reaction);
            GenerateCoefficientsString(reaction, coeffs);

            // 2. Conditionally perform stoichiometry calculation if initial moles are provided.
            if (string.IsNullOrWhiteSpace(InitialMolesInput))
            {
                WarningMessage = "未输入初始摩尔数，仅显示配平结果";
                return;
            }

            var initialMoles = ParseInitialMoles(InitialMolesInput, reaction.Reactants.Count);
            var q = StoichiometryCalculator.Compute(reaction, coeffs, initialMoles);
            var model = new ReactionModel(reaction, q.ProducedMoles);

            GenerateProductsOutput(model);
        }
        catch (Exception ex)
        {
            // On any failure, display the error and ensure results are cleared.
            WarningMessage = "错误: " + ex.Message;
            CoefficientsResult = string.Empty;
            ProductsResult = string.Empty;
        }
    }

    /// <summary>
    ///     Parses the user input string for initial moles of reactants.
    /// </summary>
    /// <returns>An array of initial moles as doubles.</returns>
    /// <exception cref="FormatException">Throws if input is invalid.</exception>
    private double[] ParseInitialMoles(string input, int expectedCount)
    {
        var parts = input.Split(Symbols.Split, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToArray();

        if (parts.Length != expectedCount) throw new FormatException($"请输入 {expectedCount} 个初始摩尔数，用空格或逗号分隔");

        var initialMoles = new double[expectedCount];
        for (var i = 0; i < parts.Length; i++)
            if (!double.TryParse(parts[i], out initialMoles[i]))
                throw new FormatException($"无法将 \"{parts[i]}\" 解析为有效的数值");

        return initialMoles;
    }


    private void GenerateProductsOutput(ReactionModel model)
    {
        // Each product gets a line "X: Y mol"
        var lines = model.Reaction.Products
            .Select((p, i) => $"{p.DisplayName}: {model.ProducedMoles[i]:G6} mol");

        // Join with "\\ " for LaTeX line breaks within a \cases environment
        ProductsResult = $@" \cases{{ {string.Join(@",\\ ", lines)} }}";
    }

    private void GenerateCoefficientsString(Reaction reaction, int[] coeffs)
    {
        var sb = new StringBuilder();
        var coeffIndex = 0;

        // Reactants
        for (var i = 0; i < reaction.Reactants.Count; i++)
        {
            sb.Append($"{FormatCoefficient(coeffs[coeffIndex++])} {{{reaction.Reactants[i].DisplayName}}}");
            if (i < reaction.Reactants.Count - 1) sb.Append(" + ");
        }

        sb.Append($" {Symbols.Equals[0]} "); // Use the equals symbol from your Symbols class

        // Products
        for (var i = 0; i < reaction.Products.Count; i++)
        {
            sb.Append($"{FormatCoefficient(coeffs[coeffIndex++])} {{{reaction.Products[i].DisplayName}}}");
            if (i < reaction.Products.Count - 1) sb.Append(" + ");
        }

        CoefficientsResult = sb.ToString();
    }

    /// <summary>
    ///     Formats a stoichiometric coefficient for display. Coefficients of 1 are omitted.
    /// </summary>
    private static string FormatCoefficient(int coefficient)
    {
        return coefficient > 1 ? coefficient.ToString("G6") : string.Empty;
    }
}