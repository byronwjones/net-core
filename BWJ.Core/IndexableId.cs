using System;

namespace BWJ.Core;
public static class IndexableId
{
    private static DateTime EPOCH = new(2020, 1, 1);

    public static string GetUniqueId()
    {
        var now = DateTime.UtcNow;
        var indexPart = ((long)Math.Ceiling((now - EPOCH).TotalMilliseconds)).ToString("x10");
        var randomPart = Guid.NewGuid().ToString("N").Substring(10);
        var id = $"{indexPart}{randomPart}";
        return id;
    }
}
