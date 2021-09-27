using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.FileFormats.Apt
{
    [AttributeUsage(
    AttributeTargets.Field |
    AttributeTargets.Property,
    AllowMultiple = false)]
    public class DataStorageListAttribute : Attribute
    {
        public readonly Func<object, string> Add;
        public readonly Func<object, object, string> Remove;
        public readonly Func<object, object, object, string> Overwrite;

        public DataStorageListAttribute()
        {

        }

        public DataStorageListAttribute(
            Func<object, string> add,
            Func<object, object, string> remove,
            Func<object, object, object, string> overwrite
            )
        {
            Add = add;
            Remove = remove;
            Overwrite = overwrite;
        }
    }
}
