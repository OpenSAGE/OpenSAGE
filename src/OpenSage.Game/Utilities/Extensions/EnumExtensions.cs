using System;
using System.Linq;

namespace OpenSage.Utilities.Extensions
{
    public static class EnumExtensions
    {
        public static string GetName(this Enum value)
        {
            var memberInfo = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            if (!(memberInfo is null))
            {
                var attribute = (DisplayAttribute) memberInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
                if (attribute is null)
                {
                    return value.ToString();
                }
                return attribute.Name;
            }
            return null;
        }
    }
}
