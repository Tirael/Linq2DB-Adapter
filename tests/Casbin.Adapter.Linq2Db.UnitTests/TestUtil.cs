using NetCasbin;
using NetCasbin.Util;
using Shouldly;

namespace Casbin.Adapter.Linq2Db.UnitTests;

public class TestUtil
{
    internal static List<T> AsList<T>(params T[] values) => values.ToList();

    internal static List<string> AsList(params string[] values) => values.ToList();

    internal static void TestEnforce(Enforcer e, string sub, object obj, string act, bool res) => res.ShouldBe(e.Enforce(sub, obj, act));

    internal static void TestEnforceWithoutUsers(Enforcer e, string obj, string act, bool res) => res.ShouldBe(e.Enforce(obj, act));

    internal static void TestDomainEnforce(Enforcer e, string sub, string dom, string obj, string act, bool res) => res.ShouldBe(e.Enforce(sub, dom, obj, act));

    internal static void TestGetPolicy(Enforcer e, List<List<string>> res)
    {
        var policy = e.GetPolicy();
        Utility.Array2DEquals(res, policy).ShouldBeTrue();
    }

    internal static void TestGetFilteredPolicy(Enforcer e, int fieldIndex, List<List<string>> res, params string[] fieldValues)
    {
        var filteredPolicy = e.GetFilteredPolicy(fieldIndex, fieldValues);
        Utility.Array2DEquals(res, filteredPolicy).ShouldBeTrue();
    }

    internal static void TestGetGroupingPolicy(Enforcer e, List<List<string>> res)
    {
        var groupingPolicy = e.GetGroupingPolicy();
        res.ShouldBe(groupingPolicy);
    }

    internal static void TestGetFilteredGroupingPolicy(Enforcer e, int fieldIndex, List<List<string>> res, params string[] fieldValues)
    {
        var filteredGroupingPolicy = e.GetFilteredGroupingPolicy(fieldIndex, fieldValues);
        res.ShouldBe(filteredGroupingPolicy);
    }

    internal static void TestHasPolicy(Enforcer e, List<string> policy, bool res)
    {
        var hasPolicy = e.HasPolicy(policy);
        res.ShouldBe(hasPolicy);
    }

    internal static void TestHasGroupingPolicy(Enforcer e, List<string> policy, bool res)
    {
        var hasGroupingPolicy = e.HasGroupingPolicy(policy);
        res.ShouldBe(hasGroupingPolicy);
    }

    internal static void TestGetRoles(Enforcer e, string name, List<string> res)
    {
        var rolesForUser = e.GetRolesForUser(name);
        var message = $"Roles for {name}: {rolesForUser}, supposed to be {res}";
        Utility.SetEquals(res, rolesForUser).ShouldBeTrue(message);
    }

    internal static void TestGetUsers(Enforcer e, string name, List<string> res)
    {
        var usersForRole = e.GetUsersForRole(name);
        var message = $"Users for {name}: {usersForRole}, supposed to be {res}";
        Utility.SetEquals(res, usersForRole).ShouldBeTrue(message);
    }

    internal static void TestHasRole(Enforcer e, string name, string role, bool res)
    {
        var hasRoleForUser = e.HasRoleForUser(name, role);
        res.ShouldBe(hasRoleForUser);
    }

    internal static void TestGetPermissions(Enforcer e, string name, List<List<string>> res)
    {
        var permissionsForUser = e.GetPermissionsForUser(name);
        var message = $"Permissions for {name}: {permissionsForUser}, supposed to be {res}";
        Utility.Array2DEquals(res, permissionsForUser).ShouldBeTrue();
    }

    internal static void TestHasPermission(Enforcer e, string name, List<string> permission, bool res)
    {
        var hasPermissionForUser = e.HasPermissionForUser(name, permission);
        res.ShouldBe(hasPermissionForUser);
    }

    internal static void TestGetRolesInDomain(Enforcer e, string name, string domain, List<string> res)
    {
        var rolesForUserInDomain = e.GetRolesForUserInDomain(name, domain);
        var message = $"Roles for {name} under {domain}: {rolesForUserInDomain}, supposed to be {res}";
        Utility.SetEquals(res, rolesForUserInDomain).ShouldBeTrue(message);
    }

    internal static void TestGetPermissionsInDomain(Enforcer e, string name, string domain, List<List<string>> res)
    {
        var permissionsForUserInDomain = e.GetPermissionsForUserInDomain(name, domain);
        Utility.Array2DEquals(res, permissionsForUserInDomain)
            .ShouldBeTrue($"Permissions for {name} under {domain}: {permissionsForUserInDomain}, supposed to be {res}");
    }
}
