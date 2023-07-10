using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TimeTracker.Server.Shared.Helpers;

public static class PermissionHelper
{
    public static bool HasPermit(string permissionJson, string permission)
    {
        try
        {
            var permissions = JObject.Parse(permissionJson);

            if (permissions.TryGetValue(permission, out var permit))
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