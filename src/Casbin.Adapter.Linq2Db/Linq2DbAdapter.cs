using Casbin.Adapter.Linq2Db.Entities;
using Casbin.Adapter.Linq2Db.Extensions;
using LinqToDB;
using LinqToDB.Data;
using NetCasbin.Model;
using NetCasbin.Persist;

namespace Casbin.Adapter.Linq2Db;

public class Linq2DbAdapter<TKey> : Linq2DbAdapter<TKey, CasbinRule<TKey>> where TKey : IEquatable<TKey>
{
    public Linq2DbAdapter(CasbinDbContext<TKey> context) : base(context)
    {
    }
}

public class Linq2DbAdapter<TKey, TCasbinRule> : Linq2DbAdapter<TKey, TCasbinRule, CasbinDbContext<TKey>>
    where TCasbinRule : class, ICasbinRule<TKey>, new()
    where TKey : IEquatable<TKey>
{
    public Linq2DbAdapter(CasbinDbContext<TKey> context) : base(context)
    {
    }
}

public class Linq2DbAdapter<TKey, TCasbinRule, TDbContext> : IAdapter, IFilteredAdapter
    where TDbContext : CasbinDbContext<TKey>
    where TCasbinRule : class, ICasbinRule<TKey>, new()
    where TKey : IEquatable<TKey>
{
    private ITable<TCasbinRule>? _casbinRules;
    private readonly BulkCopyOptions _bulkCopyOptions = new() { KeepIdentity = false };

    protected ITable<TCasbinRule> CasbinRules => _casbinRules ??= GetCasbinRuleTable(DbContext);
    protected TDbContext DbContext { get; }

    public Linq2DbAdapter(TDbContext context) => DbContext = context ?? throw new ArgumentNullException(nameof(context));

    #region virtual method

    protected virtual ITable<TCasbinRule> GetCasbinRuleTable(TDbContext dbContext) => dbContext.GetTable<TCasbinRule>();

    protected virtual IQueryable<TCasbinRule> OnLoadPolicy(Model model, IQueryable<TCasbinRule> casbinRules) => casbinRules;

    protected virtual IEnumerable<TCasbinRule> OnSavePolicy(Model model, IEnumerable<TCasbinRule> casbinRules) => casbinRules;

    protected virtual TCasbinRule OnAddPolicy(string section, string policyType, IEnumerable<string>? rule, TCasbinRule casbinRule) => casbinRule;

    protected virtual IEnumerable<TCasbinRule> OnAddPolicies(string section, string policyType, IEnumerable<IEnumerable<string>> rules,
        IEnumerable<TCasbinRule> casbinRules) => casbinRules;

    protected virtual IQueryable<TCasbinRule> OnRemoveFilteredPolicy(string section, string policyType, int fieldIndex, string[]? fieldValues,
        IQueryable<TCasbinRule> casbinRules) => casbinRules;

    #endregion

    #region Load policy

    public virtual void LoadPolicy(Model model)
    {
        var casbinRules = CasbinRules.AsQueryable();
        casbinRules = OnLoadPolicy(model, casbinRules);
        model.LoadPolicyFromCasbinRules(casbinRules);
        IsFiltered = false;
    }

    public virtual async Task LoadPolicyAsync(Model model)
    {
        var casbinRules = CasbinRules.AsQueryable();
        casbinRules = OnLoadPolicy(model, casbinRules);
        model.LoadPolicyFromCasbinRules(await casbinRules.ToListAsync());
        IsFiltered = false;
    }

    #endregion

    #region Save policy

    public virtual void SavePolicy(Model model)
    {
        var casbinRules = model.ReadPolicyFromCasbinModel<TCasbinRule>();

        if (casbinRules is { Count: 0 })
            return;

        var saveRules = OnSavePolicy(model, casbinRules);

        DbContext.BulkCopy(_bulkCopyOptions, saveRules);
    }

    public virtual async Task SavePolicyAsync(Model model)
    {
        var casbinRules = model.ReadPolicyFromCasbinModel<TCasbinRule>();

        if (casbinRules is { Count: 0 })
            return;

        var saveRules = OnSavePolicy(model, casbinRules);

        await DbContext.BulkCopyAsync(_bulkCopyOptions, saveRules);
    }

    #endregion

    #region Add policy

    public virtual void AddPolicy(string section, string policyType, IList<string>? rule)
    {
        if (rule is null || rule is { Count: 0 })
            return;

        var casbinRule = CasbinRuleExtenstion.Parse<TCasbinRule>(policyType, rule);
        casbinRule = OnAddPolicy(section, policyType, rule, casbinRule);

        DbContext.BulkCopy(_bulkCopyOptions, new[] { casbinRule });
    }

    public virtual async Task AddPolicyAsync(string section, string policyType, IList<string>? rule)
    {
        if (rule is null or { Count: 0 })
            return;

        var casbinRule = CasbinRuleExtenstion.Parse<TCasbinRule>(policyType, rule);
        casbinRule = OnAddPolicy(section, policyType, rule, casbinRule);

        await DbContext.BulkCopyAsync(_bulkCopyOptions, new[] { casbinRule });
    }

    public virtual void AddPolicies(string section, string policyType, IEnumerable<IList<string>>? rules)
    {
        if (rules is null)
            return;

        var rulesArray = rules as IList<string>[] ?? rules.ToArray();

        if (rulesArray is { Length: 0 })
            return;

        var casbinRules = rulesArray.Select(r => CasbinRuleExtenstion.Parse<TCasbinRule>(policyType, r));
        casbinRules = OnAddPolicies(section, policyType, rulesArray, casbinRules);

        DbContext.BulkCopy(_bulkCopyOptions, casbinRules);
    }

    public virtual async Task AddPoliciesAsync(string section, string policyType, IEnumerable<IList<string>>? rules)
    {
        if (rules is null)
            return;

        var rulesArray = rules as IList<string>[] ?? rules.ToArray();

        if (rulesArray is { Length: 0 })
            return;

        var casbinRules = rulesArray.Select(r => CasbinRuleExtenstion.Parse<TCasbinRule>(policyType, r));
        casbinRules = OnAddPolicies(section, policyType, rulesArray, casbinRules);

        await DbContext.BulkCopyAsync(_bulkCopyOptions, casbinRules);
    }

    #endregion

    #region Remove policy

    public virtual void RemovePolicy(string section, string policyType, IList<string>? rule)
    {
        if (rule is null or { Count: 0 })
            return;

        RemovePolicyInMemory(section, policyType, rule);
    }

    public virtual Task RemovePolicyAsync(string section, string policyType, IList<string>? rule) =>
        rule is null or { Count: 0 } ? Task.CompletedTask : RemovePolicyInMemoryAsync(section, policyType, rule);

    public virtual void RemoveFilteredPolicy(string section, string policyType, int fieldIndex, params string[]? fieldValues)
    {
        if (fieldValues is null or { Length: 0 })
            return;

        ApplyRemoveFilteredPolicyAsync(section, policyType, fieldIndex, fieldValues)
            .GetAwaiter()
            .GetResult();
    }

    public virtual async Task RemoveFilteredPolicyAsync(string section, string policyType, int fieldIndex, params string[]? fieldValues)
    {
        if (fieldValues is null or { Length: 0 })
            return;

        await ApplyRemoveFilteredPolicyAsync(section, policyType, fieldIndex, fieldValues);
    }

    public virtual void RemovePolicies(string section, string policyType, IEnumerable<IList<string>>? rules)
    {
        if (rules is null)
            return;

        var rulesArray = rules as IList<string>?[] ?? rules.ToArray();

        if (rulesArray is { Length: 0 })
            return;

        foreach (var rule in rulesArray)
        {
            RemovePolicyInMemory(section, policyType, rule);
        }
    }

    public virtual async Task RemovePoliciesAsync(string section, string policyType, IEnumerable<IList<string>>? rules)
    {
        if (rules is null)
            return;

        var rulesArray = rules as IList<string>?[] ?? rules.ToArray();

        if (rulesArray is { Length: 0 })
            return;

        foreach (var rule in rulesArray)
            await RemovePolicyInMemoryAsync(section, policyType, rule);
    }

    #endregion

    #region IFilteredAdapter

    public bool IsFiltered { get; private set; }

    public void LoadFilteredPolicy(Model model, Filter filter)
    {
        var casbinRules = CasbinRules.ApplyQueryFilter(filter);
        casbinRules = OnLoadPolicy(model, casbinRules);
        model.LoadPolicyFromCasbinRules(casbinRules);
        IsFiltered = true;
    }

    public async Task LoadFilteredPolicyAsync(Model model, Filter filter)
    {
        var casbinRules = CasbinRules.ApplyQueryFilter(filter);
        casbinRules = OnLoadPolicy(model, casbinRules);
        model.LoadPolicyFromCasbinRules(await casbinRules.ToListAsync());
        IsFiltered = true;
    }

    #endregion

    private void RemovePolicyInMemory(string section, string policyType, IEnumerable<string>? rule) =>
        RemoveFilteredPolicy(section, policyType, 0, rule as string[] ?? rule?.ToArray());

    private Task RemovePolicyInMemoryAsync(string section, string policyType, IEnumerable<string>? rule) =>
        RemoveFilteredPolicyAsync(section, policyType, 0, rule as string[] ?? rule?.ToArray());

    private async Task ApplyRemoveFilteredPolicyAsync(string section, string policyType, int fieldIndex, params string[]? fieldValues)
    {
        var query = CasbinRules.ApplyQueryFilter(policyType, fieldIndex, fieldValues);

        query = OnRemoveFilteredPolicy(section, policyType, fieldIndex, fieldValues, query);

        var idsToDelete = query.Select(r => r.Id).ToHashSet();

        await CasbinRules
            .Where(r => idsToDelete.Contains(r.Id))
            .DeleteAsync();
    }
}
