using FluentMigrator;

namespace Casbin.Adapter.Linq2Db.Migration;

[Migration(20220110031234)]
public class CreateCasbinRules : AutoReversingMigration
{
    public override void Up()
    {
        Create
            .Table("casbin_rules")
            .WithColumn(nameof(ICasbinRule<long>.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(ICasbinRule.PType)).AsString().NotNullable().Indexed()
            .WithColumn(nameof(ICasbinRule.V0)).AsString().Nullable().Indexed()
            .WithColumn(nameof(ICasbinRule.V1)).AsString().Nullable().Indexed()
            .WithColumn(nameof(ICasbinRule.V2)).AsString().Nullable().Indexed()
            .WithColumn(nameof(ICasbinRule.V3)).AsString().Nullable().Indexed()
            .WithColumn(nameof(ICasbinRule.V4)).AsString().Nullable().Indexed()
            .WithColumn(nameof(ICasbinRule.V5)).AsString().Nullable().Indexed();
    }
}
