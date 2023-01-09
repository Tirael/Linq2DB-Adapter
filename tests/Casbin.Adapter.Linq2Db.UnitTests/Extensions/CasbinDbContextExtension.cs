using LinqToDB;

namespace Casbin.Adapter.Linq2Db.UnitTests.Extensions;

internal static class CasbinDbContextExtension
{
    internal static Task Clear<TKey>(this CasbinDbContext<TKey> dbContext) where TKey : IEquatable<TKey> => dbContext.CasbinRules.DeleteAsync();
}
