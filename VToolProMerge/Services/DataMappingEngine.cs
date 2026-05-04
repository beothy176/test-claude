using System.Collections.Generic;
using System.Linq;
using VToolProMerge.Models;

namespace VToolProMerge.Services;

public class DataMappingEngine
{
    /// <summary>
    /// Resolve giá trị cho 1 placeholder theo MappingRule.
    /// Bản hiện tại tra trong dictionary đầu vào, chưa nối DB/Excel thật.
    /// </summary>
    public string? ResolveValue(MappingRule rule, IReadOnlyDictionary<string, object?> data)
    {
        if (data.TryGetValue(rule.SourceField, out var v) && v != null)
            return v.ToString();
        return rule.DefaultValue;
    }

    public Dictionary<string, string?> ApplyMapping(
        IEnumerable<MappingRule> rules,
        IReadOnlyDictionary<string, object?> data)
    {
        var result = new Dictionary<string, string?>();
        foreach (var r in rules)
            result[r.Placeholder] = ResolveValue(r, data);
        return result;
    }

    public IEnumerable<MappingRule> ValidateRequiredFields(
        IEnumerable<MappingRule> rules,
        IReadOnlyDictionary<string, object?> data)
    {
        return rules.Where(r => r.Required &&
            (!data.TryGetValue(r.SourceField, out var v) || v == null
             || string.IsNullOrWhiteSpace(v.ToString())));
    }

    public IEnumerable<string> GetMissingFields(
        IEnumerable<string> placeholdersInTemplate,
        IEnumerable<MappingRule> rules)
    {
        var mapped = rules.Select(r => r.Placeholder.Trim('[', ']')).ToHashSet();
        return placeholdersInTemplate.Where(p => !mapped.Contains(p));
    }
}
