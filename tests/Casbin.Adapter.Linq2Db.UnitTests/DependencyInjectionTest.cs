using Casbin.Adapter.Linq2Db.UnitTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using NetCasbin.Persist;
using Shouldly;
using Xunit;

namespace Casbin.Adapter.Linq2Db.UnitTests;

public class DependencyInjectionTest : IClassFixture<TestHostFixture<DependencyInjectionTest>>
{
    private readonly TestHostFixture<DependencyInjectionTest> _testHostFixture;

    public DependencyInjectionTest(TestHostFixture<DependencyInjectionTest> testHostFixture) => _testHostFixture = testHostFixture;

    [Fact]
    public void ShouldResolveCasbinDbContext()
    {
        // Arrange
        var dbContext = _testHostFixture.Services.GetRequiredService<CasbinDbContext<int>>();
        // Act/Assert
        dbContext.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldResolveLinq2DbAdapter()
    {
        // Arrange
        var adapter = _testHostFixture.Services.GetRequiredService<IAdapter>();
        // Act/Assert
        adapter.ShouldNotBeNull();
    }
}
