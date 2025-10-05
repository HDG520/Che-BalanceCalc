using ChemistryPlus.Models;

namespace ChemistryPlus;

public static class ChemicalParser
{
    public static MoleculeFormula ParseFormula(string name)
    {
        var elemCounts = MoleculeFactory.ParseFormula(name);
        return new MoleculeFormula(name, elemCounts);
    }

    public static Reaction ParseReaction(string input)
    {
        var parts = input.Split(Symbols.Equals, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) throw new Exception("反应式格式错误");
        var left = parts[0].Split(Symbols.Plus).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        var right = parts[1].Split(Symbols.Plus).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

        var reacList = left.Select(ParseFormula).ToList();
        var prodList = right.Select(ParseFormula).ToList();

        return new Reaction(reacList, prodList);
    }
}