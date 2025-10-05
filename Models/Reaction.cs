namespace ChemistryPlus.Models;

/// <summary>
/// 反应方程式
/// </summary>
/// <param name="Reactants"></param>
/// <param name="Products"></param>
public record struct Reaction(List<MoleculeFormula> Reactants, List<MoleculeFormula> Products);