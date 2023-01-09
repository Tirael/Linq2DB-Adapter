using Casbin.Adapter.Linq2Db.UnitTests.Extensions;
using Casbin.Adapter.Linq2Db.UnitTests.Fixtures;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using NetCasbin;
using NetCasbin.Persist;
using Shouldly;
using Xunit;

namespace Casbin.Adapter.Linq2Db.UnitTests;

public class AdapterTest : TestUtil, IClassFixture<ModelProvideFixture>, IClassFixture<TestHostFixture<AdapterTest>>
{
    private readonly ModelProvideFixture _modelProvideFixture;
    private readonly TestHostFixture<AdapterTest> _testHostFixture;

    public AdapterTest(ModelProvideFixture modelProvideFixture, TestHostFixture<AdapterTest> testHostFixture)
    {
        _modelProvideFixture = modelProvideFixture;
        _testHostFixture = testHostFixture;
    }

    [Fact]
    public async Task TestAdapterAutoSave()
    {
        var context = _testHostFixture.Services.GetRequiredService<CasbinDbContext<int>>();

        await InitPolicy(context);

        var adapter = new Linq2DbAdapter<int>(context);
        var enforcer = new Enforcer(_modelProvideFixture.GetNewRbacModel(), adapter);

        #region Load policy test

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data1", "read"),
            AsList("bob", "data2", "write"),
            AsList("data2_admin", "data2", "read"),
            AsList("data2_admin", "data2", "write")
        ));

        context.CasbinRules.Count().ShouldBe(5);

        #endregion

        #region Add policy test

        await enforcer.AddPolicyAsync("alice", "data1", "write");

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data1", "read"),
            AsList("bob", "data2", "write"),
            AsList("data2_admin", "data2", "read"),
            AsList("data2_admin", "data2", "write"),
            AsList("alice", "data1", "write")
        ));

        context.CasbinRules.Count().ShouldBe(6);

        #endregion

        #region Remove poliy test

        await enforcer.RemovePolicyAsync("alice", "data1", "write");

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data1", "read"),
            AsList("bob", "data2", "write"),
            AsList("data2_admin", "data2", "read"),
            AsList("data2_admin", "data2", "write")
        ));

        context.CasbinRules.Count().ShouldBe(5);

        await enforcer.RemoveFilteredPolicyAsync(0, "data2_admin");

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data1", "read"),
            AsList("bob", "data2", "write")
        ));

        context.CasbinRules.Count().ShouldBe(3);

        #endregion

        #region Batch APIs test

        await enforcer.AddPoliciesAsync(new[] { new List<string> { "alice", "data2", "write" }, new List<string> { "bob", "data1", "read" } });

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data1", "read"),
            AsList("bob", "data2", "write"),
            AsList("alice", "data2", "write"),
            AsList("bob", "data1", "read")
        ));

        context.CasbinRules.Count().ShouldBe(5);

        await enforcer.RemovePoliciesAsync(new[] { new List<string> { "alice", "data1", "read" }, new List<string> { "bob", "data2", "write" } });

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data2", "write"),
            AsList("bob", "data1", "read")
        ));

        context.CasbinRules.Count().ShouldBe(3);

        #endregion


        #region IFilteredAdapter test

        enforcer.LoadFilteredPolicy(new Filter { P = new[] { "bob", "data1", "read" } });

        TestGetPolicy(enforcer, AsList(
            AsList("bob", "data1", "read")
        ));

        enforcer.GetModel().Model["g"]["g"].Policy.Count.ShouldBe(0);
        context.CasbinRules.Count().ShouldBe(3);

        await enforcer.LoadFilteredPolicyAsync(new Filter
        {
            P = new[] { string.Empty, "data2", string.Empty }, G = new[] { string.Empty, "data2_admin" }
        });

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data2", "write")
        ));

        TestGetGroupingPolicy(enforcer, AsList(
            AsList("alice", "data2_admin")
        ));

        enforcer.GetModel().Model["g"]["g"].Policy.Count.ShouldBe(1);
        context.CasbinRules.Count().ShouldBe(3);

        #endregion
    }

    [Fact]
    public async Task TestAdapterAutoSaveAsync()
    {
        var context = _testHostFixture.Services.GetRequiredService<CasbinDbContext<int>>();

        await InitPolicy(context);

        var adapter = new Linq2DbAdapter<int>(context);
        var enforcer = new Enforcer(_modelProvideFixture.GetNewRbacModel(), adapter);

        #region Load policy test

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data1", "read"),
            AsList("bob", "data2", "write"),
            AsList("data2_admin", "data2", "read"),
            AsList("data2_admin", "data2", "write")
        ));

        context.CasbinRules.Count().ShouldBe(5);

        #endregion

        #region Add policy test

        await enforcer.AddPolicyAsync("alice", "data1", "write");

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data1", "read"),
            AsList("bob", "data2", "write"),
            AsList("data2_admin", "data2", "read"),
            AsList("data2_admin", "data2", "write"),
            AsList("alice", "data1", "write")
        ));

        context.CasbinRules.Count().ShouldBe(6);

        #endregion

        #region Remove policy test

        await enforcer.RemovePolicyAsync("alice", "data1", "write");

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data1", "read"),
            AsList("bob", "data2", "write"),
            AsList("data2_admin", "data2", "read"),
            AsList("data2_admin", "data2", "write")
        ));

        context.CasbinRules.Count().ShouldBe(5);

        await enforcer.RemoveFilteredPolicyAsync(0, "data2_admin");

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data1", "read"),
            AsList("bob", "data2", "write")
        ));

        context.CasbinRules.Count().ShouldBe(3);

        #endregion

        #region Batch APIs test

        await enforcer.AddPoliciesAsync(new[] { new List<string> { "alice", "data2", "write" }, new List<string> { "bob", "data1", "read" } });

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data1", "read"),
            AsList("bob", "data2", "write"),
            AsList("alice", "data2", "write"),
            AsList("bob", "data1", "read")
        ));

        context.CasbinRules.Count().ShouldBe(5);

        await enforcer.RemovePoliciesAsync(new[] { new List<string> { "alice", "data1", "read" }, new List<string> { "bob", "data2", "write" } });

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data2", "write"),
            AsList("bob", "data1", "read")
        ));

        context.CasbinRules.Count().ShouldBe(3);

        #endregion

        #region IFilteredAdapter test

        await enforcer.LoadFilteredPolicyAsync(new Filter { P = new[] { "bob", "data1", "read" } });

        TestGetPolicy(enforcer, AsList(
            AsList("bob", "data1", "read")
        ));

        enforcer.GetModel().Model["g"]["g"].Policy.Count.ShouldBe(0);
        context.CasbinRules.Count().ShouldBe(3);

        await enforcer.LoadFilteredPolicyAsync(new Filter
        {
            P = new[] { string.Empty, "data2", string.Empty }, G = new[] { string.Empty, "data2_admin" }
        });

        TestGetPolicy(enforcer, AsList(
            AsList("alice", "data2", "write")
        ));

        TestGetGroupingPolicy(enforcer, AsList(
            AsList("alice", "data2_admin")
        ));

        enforcer.GetModel().Model["g"]["g"].Policy.Count.ShouldBe(1);
        context.CasbinRules.Count().ShouldBe(3);

        #endregion
    }

    private static async Task InitPolicy(CasbinDbContext<int> context)
    {
        var tx = await context.BeginTransactionAsync();

        await context.Clear();

        await context.CasbinRules
            .Value(x => x.PType, "p")
            .Value(x => x.V0, "alice")
            .Value(x => x.V1, "data1")
            .Value(x => x.V2, "read")
            .InsertAsync();

        await context.CasbinRules
            .Value(x => x.PType, "p")
            .Value(x => x.V0, "bob")
            .Value(x => x.V1, "data2")
            .Value(x => x.V2, "write")
            .InsertAsync();

        await context.CasbinRules
            .Value(x => x.PType, "p")
            .Value(x => x.V0, "data2_admin")
            .Value(x => x.V1, "data2")
            .Value(x => x.V2, "read")
            .InsertAsync();

        await context.CasbinRules
            .Value(x => x.PType, "p")
            .Value(x => x.V0, "data2_admin")
            .Value(x => x.V1, "data2")
            .Value(x => x.V2, "write")
            .InsertAsync();

        await context.CasbinRules
            .Value(x => x.PType, "g")
            .Value(x => x.V0, "alice")
            .Value(x => x.V1, "data2_admin")
            .InsertAsync();

        await tx.CommitAsync();
    }
}
