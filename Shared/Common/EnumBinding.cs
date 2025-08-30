using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Shared.Common;

public static class EnumBinding
{
    // Returns list of OptionItem { Text, Value } using DisplayAttribute.Name as Text and Value
    public static List<OptionItem> GetOptions<TEnum>() where TEnum : struct, System.Enum
    {
        return System.Enum.GetValues<TEnum>()
            .Select(e => new OptionItem
            {
                Text = e.GetDisplayName(),
                Value = e.GetDisplayName()
            })
            .ToList();
    }
}
