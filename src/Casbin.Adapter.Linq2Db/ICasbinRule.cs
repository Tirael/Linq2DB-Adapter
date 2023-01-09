namespace Casbin.Adapter.Linq2Db;

public interface ICasbinRule
{
    public string PType { get; set; }
    public string V0 { get; set; }
    public string V1 { get; set; }
    public string V2 { get; set; }
    public string V3 { get; set; }
    public string V4 { get; set; }
    public string V5 { get; set; }
}

public interface ICasbinRule<TKey> : ICasbinRule where TKey : IEquatable<TKey>
{
    public TKey Id { get; set; }
}
