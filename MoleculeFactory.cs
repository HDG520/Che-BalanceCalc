using System.Text.RegularExpressions;

namespace ChemistryPlus;

public class MoleculeFactory
{
    /// <summary>
    /// 解析一个化学式字符串（例如 "C3H8", "H2O", "Al2(SO4)3"）到元素计数映射。
    /// </summary>
    /// <param name="formula">化学式字符串</param>
    /// <returns>元素符号 → 原子数</returns>
    public static Dictionary<string, int> ParseFormula(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
            throw new ArgumentException("formula 不能为空");

        // 移除空格
        formula = formula.Trim();

        // 使用递归 / 堆栈处理括号
        var result = ParseSegment(formula);

        return result;
    }

    /// <summary>
    /// 解析一个片段（可能包含括号与乘数）返回元素计数
    /// </summary>
    private static Dictionary<string, int> ParseSegment(string segment)
    {
        // 这个正则匹配：元素符号 + 可选数字，或 “( … )数字” 组合
        // 解释：
        //   (?<group>\((?<inner>[^()]+)\)(?<mult>[0-9]+)?)  匹配 “(inner)mult” 结构
        //   | (?<elem>[A-Z][a-z]?)(?<count>[0-9]*)         匹配 “元素 +数字” 结构
        var pattern = @"(?<group>\((?<inner>[^()]+)\)(?<mult>[0-9]+)?)|(?<elem>[A-Z][a-z]?)(?<count>[0-9]*)";

        var dict = new Dictionary<string, int>();

        var matches = Regex.Matches(segment, pattern);
        foreach (Match match in matches)
        {
            if (match.Groups["group"].Success)
            {
                // 是一个带括号的组
                var inner = match.Groups["inner"].Value;
                var multStr = match.Groups["mult"].Value;
                var mult = 1;
                if (!string.IsNullOrEmpty(multStr))
                    mult = int.Parse(multStr);

                // 解析内部
                var innerDict = ParseSegment(inner);
                // 把内部乘 multiplicity 加入主 dict
                foreach (var kv in innerDict)
                {
                    if (!dict.ContainsKey(kv.Key))
                        dict[kv.Key] = 0;
                    dict[kv.Key] += kv.Value * mult;
                }
            }
            else if (match.Groups["elem"].Success)
            {
                // 普通元素 + 计数
                var elem = match.Groups["elem"].Value;
                var cntStr = match.Groups["count"].Value;
                var cnt = 1;
                if (!string.IsNullOrEmpty(cntStr))
                    cnt = int.Parse(cntStr);

                dict.TryAdd(elem, 0);
                dict[elem] += cnt;
            }
            // 理论上不会走到这里
        }

        return dict;
    }
}