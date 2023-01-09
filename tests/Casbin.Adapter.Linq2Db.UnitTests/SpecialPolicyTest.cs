using Casbin.Adapter.Linq2Db.UnitTests.Extensions;
using Casbin.Adapter.Linq2Db.UnitTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using NetCasbin;
using NetCasbin.Model;
using Shouldly;
using Xunit;

namespace Casbin.Adapter.Linq2Db.UnitTests;

public class SpecialPolicyTest : IClassFixture<ModelProvideFixture>, IClassFixture<TestHostFixture<SpecialPolicyTest>>
{
    private readonly TestHostFixture<SpecialPolicyTest> _testHostFixture;

    public SpecialPolicyTest(TestHostFixture<SpecialPolicyTest> testHostFixture) => _testHostFixture = testHostFixture;

    [Fact]
    public void TestCommaPolicy()
    {
        // Arrange
        var context = _testHostFixture.Services.GetRequiredService<CasbinDbContext<int>>();
        
        context.Clear();
        
        var adapter = new Linq2DbAdapter<int>(context);
        var enforcer = new Enforcer(Model.CreateDefaultFromText(@"
[request_definition]
r = _

[policy_definition]
p = rule, a1, a2

[policy_effect]
e = some(where (p.eft == allow))

[matchers]
m = eval(p.rule)
"), adapter);

        // Act/Assert
        enforcer.AddFunction("equal", (a1, a2) => a1 == a2);

        enforcer.AddPolicy("equal(p.a1, p.a2)", "a1", "a1");
        enforcer.Enforce("_").ShouldBeTrue();

        enforcer.LoadPolicy();
        enforcer.Enforce("_").ShouldBeTrue();

        enforcer.RemovePolicy("equal(p.a1, p.a2)", "a1", "a1");
        enforcer.AddPolicy("equal(p.a1, p.a2)", "a1", "a2");
        enforcer.Enforce("_").ShouldBeFalse();

        enforcer.LoadPolicy();
        enforcer.Enforce("_").ShouldBeFalse();
    }
}
