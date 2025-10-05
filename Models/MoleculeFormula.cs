using System.Text;

namespace ChemistryPlus.Models;

/// <summary>
/// 分子式
/// </summary>
/// <param name="name"></param>
/// <param name="elementCounts"></param>
public class MoleculeFormula(string name, Dictionary<string, int> elementCounts)
{
    public string DisplayName => ConvertToSubscript(name);
    public string Name => name;
    
    public Dictionary<string, int> ElementCounts => elementCounts;
    
    private static string ConvertToSubscript(string formula)
    {
        var sb = new StringBuilder();
        foreach (var c in formula)
        {
            if (char.IsDigit(c))
                sb.Append($"_{c}");
            else
                sb.Append(c);
        }
        return sb.ToString();
    }
}