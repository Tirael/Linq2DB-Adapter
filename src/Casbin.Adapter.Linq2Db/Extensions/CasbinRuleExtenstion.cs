using NetCasbin.Model;
using NetCasbin.Persist;

namespace Casbin.Adapter.Linq2Db.Extensions;

public static class CasbinRuleExtenstion
{
    internal static IReadOnlyList<string> ToList(this ICasbinRule rule) =>
        new List<string> { rule.PType }
            .AddFieldIfNotEmpty(rule.V0)
            .AddFieldIfNotEmpty(rule.V1)
            .AddFieldIfNotEmpty(rule.V2)
            .AddFieldIfNotEmpty(rule.V3)
            .AddFieldIfNotEmpty(rule.V4)
            .AddFieldIfNotEmpty(rule.V5)
            .AsReadOnly();

    private static List<string> AddFieldIfNotEmpty(this List<string> @this, string rule)
    {
        if (!string.IsNullOrEmpty(rule))
            @this.Add(rule);

        return @this;
    }

    internal static IQueryable<TCasbinRule> ApplyQueryFilter<TCasbinRule>(this IQueryable<TCasbinRule> query, string policyType, int fieldIndex,
        IEnumerable<string>? fieldValues)
        where TCasbinRule : ICasbinRule
    {
        if (fieldIndex > 5)
            throw new ArgumentOutOfRangeException(nameof(fieldIndex));

        var fieldValuesList = fieldValues as IList<string> ?? fieldValues?.ToArray();
        var fieldValueCount = fieldValuesList?.Count;

        if (fieldValueCount is null or 0)
            return query;

        var lastIndex = fieldIndex + fieldValueCount - 1;

        if (lastIndex > 5)
            throw new ArgumentOutOfRangeException(nameof(lastIndex));

        query = query.Where(p => p.PType == policyType);
        
        if (fieldIndex is 0 && lastIndex >= 0)
        {
            var field = fieldValuesList?[fieldIndex];

            if (!string.IsNullOrWhiteSpace(field))
                query = query.Where(p => p.V0 == field);
        }

        if (fieldIndex <= 1 && lastIndex >= 1)
        {
            var field = fieldValuesList?[1 - fieldIndex];

            if (!string.IsNullOrWhiteSpace(field))
                query = query.Where(p => p.V1 == field);
        }

        if (fieldIndex <= 2 && lastIndex >= 2)
        {
            var field = fieldValuesList?[2 - fieldIndex];

            if (!string.IsNullOrWhiteSpace(field))
                query = query.Where(p => p.V2 == field);
        }

        if (fieldIndex <= 3 && lastIndex >= 3)
        {
            var field = fieldValuesList?[3 - fieldIndex];

            if (!string.IsNullOrWhiteSpace(field))
                query = query.Where(p => p.V3 == field);
        }

        if (fieldIndex <= 4 && lastIndex >= 4)
        {
            var field = fieldValuesList?[4 - fieldIndex];

            if (!string.IsNullOrWhiteSpace(field))
                query = query.Where(p => p.V4 == field);
        }

        if (lastIndex is 5) // and fieldIndex <= 5
        {
            var field = fieldValuesList?[5 - fieldIndex];

            if (!string.IsNullOrWhiteSpace(field))
                query = query.Where(p => p.V5 == field);
        }

        return query;
    }

    internal static IQueryable<TCasbinRule> ApplyQueryFilter<TCasbinRule>(this IQueryable<TCasbinRule> query, Filter? filter)
        where TCasbinRule : ICasbinRule
    {
        if (filter is null)
            return query;

        if (filter.P is null && filter.G is null)
            return query;

        if (filter.P is not null && filter.G is not null)
        {
            var queryP = query.ApplyQueryFilter(PermConstants.DefaultPolicyType, 0, filter.P);
            var queryG = query.ApplyQueryFilter(PermConstants.DefaultGroupingPolicyType, 0, filter.G);
            return queryP.Union(queryG);
        }

        if (filter.P is not null)
            query = query.ApplyQueryFilter(PermConstants.DefaultPolicyType, 0, filter.P);

        if (filter.G is not null)
            query = query.ApplyQueryFilter(PermConstants.DefaultGroupingPolicyType, 0, filter.G);

        return query;
    }

    internal static TCasbinRule Parse<TCasbinRule>(string policyType, IList<string>? ruleStrings)
        where TCasbinRule : ICasbinRule, new()
    {
        var rule = new TCasbinRule { PType = policyType };

        if (ruleStrings is null)
            return rule;

        for (var i = 0; i < ruleStrings.Count; i++)
        {
            rule.SetValue($"V{i}", ruleStrings[i]);
        }

        return rule;
    }
    
    private static void SetValue<T>(this T @this, string propertyName, object? value)
    {
        var propertyInfo = @this?
            .GetType()
            .GetProperty(propertyName);
 
        if (propertyInfo is null) 
            return;
 
        var type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
     
        propertyInfo.SetValue(@this, value ?? Convert.ChangeType(value, type), null);
    }
}
