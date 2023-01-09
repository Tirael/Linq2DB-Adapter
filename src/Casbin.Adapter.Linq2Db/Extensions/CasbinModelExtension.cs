using NetCasbin.Model;
using NetCasbin.Persist;

namespace Casbin.Adapter.Linq2Db.Extensions;

internal static class CasbinModelExtension
{
    internal static void LoadPolicyFromCasbinRules<TCasbinRule>(this Model casbinModel, IEnumerable<TCasbinRule> rules) 
        where TCasbinRule : class, ICasbinRule
    {
        foreach (var rule in rules)
        {
            casbinModel.TryLoadPolicyLine(rule.ToList());
        }
    }
    
    internal static IReadOnlyList<TCasbinRule> ReadPolicyFromCasbinModel<TCasbinRule>(this Model casbinModel)
        where TCasbinRule : class, ICasbinRule, new()
    {
        var casbinRules = new List<TCasbinRule>();
        
        if (casbinModel.Model.TryGetValue(PermConstants.DefaultPolicyType, out var policyTypes))
        {
            foreach (var (policyType, assertion) in policyTypes)
            {
                casbinRules.AddRange(assertion.Policy.Select(ruleStrings => CasbinRuleExtenstion.Parse<TCasbinRule>(policyType, ruleStrings)));
            }
        }

        if (casbinModel.Model.TryGetValue(PermConstants.DefaultGroupingPolicyType, out var groupingPolicyTypes))
        {
            foreach (var (policyType, assertion) in groupingPolicyTypes)
            {
                casbinRules.AddRange(assertion.Policy.Select(ruleStrings => CasbinRuleExtenstion.Parse<TCasbinRule>(policyType, ruleStrings)));
            }
        }

        return casbinRules.AsReadOnly();
    }
}
