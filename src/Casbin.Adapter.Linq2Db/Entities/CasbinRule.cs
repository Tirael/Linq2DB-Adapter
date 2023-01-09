namespace Casbin.Adapter.Linq2Db.Entities;

public class CasbinRule<TKey> : ICasbinRule<TKey>
    where TKey : IEquatable<TKey>
{
    public TKey Id { get; set; } = default!;
    public string PType { get; set; } = string.Empty;
    public string V0 { get; set; } = string.Empty;
    public string V1 { get; set; } = string.Empty;
    public string V2 { get; set; } = string.Empty;
    public string V3 { get; set; } = string.Empty;
    public string V4 { get; set; } = string.Empty;
    public string V5 { get; set; } = string.Empty;
}
