using System.Data.Common;
using Casbin.Adapter.Linq2Db.Entities;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using LinqToDB.DataProvider;

namespace Casbin.Adapter.Linq2Db;

public class CasbinDbContext<TKey> : DataConnection where TKey : IEquatable<TKey>
{
    public const string DefaultTableName = "casbin_rules";

    private readonly LinqToDBConnectionOptions<CasbinDbContext<TKey>>? _options;
    private Lazy<ITable<CasbinRule<TKey>>> _casbinRules;

    public ITable<CasbinRule<TKey>> CasbinRules => _casbinRules.Value;

    public CasbinDbContext(LinqToDBConnectionOptions<CasbinDbContext<TKey>> options) : base(options)
    {
        _options = options;

        Init();
    }

    public CasbinDbContext(IDataProvider provider, DbConnection connection) : base(provider, connection) => Init();

    private void Init()
    {
        InlineParameters = true;

        MappingSchema.GetFluentMappingBuilder()
            .Entity<CasbinRule<TKey>>()
            .HasTableName(DefaultTableName)
            .HasIdentity(x => x.Id)
            .HasPrimaryKey(x => x.Id)
            .HasColumn(rule => rule.PType)
            .HasColumn(rule => rule.V0)
            .HasColumn(rule => rule.V1)
            .HasColumn(rule => rule.V2)
            .HasColumn(rule => rule.V3)
            .HasColumn(rule => rule.V4)
            .HasColumn(rule => rule.V5);

        _casbinRules = new Lazy<ITable<CasbinRule<TKey>>>(this.GetTable<CasbinRule<TKey>>);
    }
}
