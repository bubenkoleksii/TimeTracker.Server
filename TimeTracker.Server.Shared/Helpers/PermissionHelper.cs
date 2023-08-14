using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TimeTracker.Server.Shared.Helpers;

public static class PermissionHelper
{
    public static bool HasPermit(string? permissionJson, string permission)
    {
        try
        {
            if (permissionJson.ToLower() == "all")
                return true;

            var permissions = JObject.Parse(permissionJson.ToLower());

            if (permissions.TryGetValue(permission.ToLower(), out var permit))
            {
                return permit.Value<bool>();
            }
        }
        catch (JsonReaderException)
        {
            return false;
        }

        return false;
    }
}