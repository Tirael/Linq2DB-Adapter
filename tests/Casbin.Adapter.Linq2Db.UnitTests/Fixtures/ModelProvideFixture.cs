using NetCasbin.Model;

namespace Casbin.Adapter.Linq2Db.UnitTests.Fixtures;

public class ModelProvideFixture
{
    private readonly string _rbacModelText = File.ReadAllText("examples/rbac_model.conf");

    public Model GetNewRbacModel() => Model.CreateDefaultFromText(_rbacModelText);
}
