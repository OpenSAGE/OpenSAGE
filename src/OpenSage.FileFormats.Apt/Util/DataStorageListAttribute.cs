using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public readonly bool IsReadonly;
        public readonly Type Type;
        public IReadOnlyDictionary<string, Type> AcceptableTypes;

        public readonly MethodInfo ListAdd;
        public readonly MethodInfo ListInsert;
        public readonly MethodInfo ListRemove;
        public readonly MethodInfo ListGet;

        public object New(Type type)
        {
            return type.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
        }

        public object Add(object listRaw, int index = -1, string type = null)
        {
            var t = Type;
            if (!string.IsNullOrWhiteSpace(type) && AcceptableTypes.TryGetValue(type, out var at))
                t = at;
            return Add(listRaw, index, t);
        }
        public object Add(object listRaw, int index = -1, Type t = null)
        {
            if (IsReadonly)
                throw new InvalidOperationException("Readonly List");

            if (t == null || !AcceptableTypes.Values.Contains(t))
                t = Type;
            object objRaw = New(t);
            if (index < 0)
                ListAdd.Invoke(listRaw, new[] { objRaw });
            else
                ListInsert.Invoke(listRaw, new[] { index, objRaw });
            return objRaw;
        }

        public bool Insert(object listRaw, object objRaw, int index = -1)
        {
            if (IsReadonly)
                throw new InvalidOperationException("Readonly List");

            if (index < 0)
                ListAdd.Invoke(listRaw, new[] { objRaw });
            else
                ListInsert.Invoke(listRaw, new[] { index, objRaw });

            return true;
        }

        public object Remove(object listRaw, int index)
        {
            if (IsReadonly)
                throw new InvalidOperationException("Readonly List");

            var obj = ListGet.Invoke(null, new[] { listRaw, index });
            ListRemove.Invoke(listRaw, new object[] { index });
            return obj; 
        }


        public DataStorageListAttribute(
            Type type,
            Type[] acceptableTypes = null, 
            bool isReadonly = false
            )
        {
            IsReadonly = isReadonly;
            Type = type;
            ListAdd = typeof(IList<object>).GetMethod(nameof(IList<object>.Add)).MakeGenericMethod(new[] { type });
            ListInsert = typeof(IList<object>).GetMethod(nameof(IList<object>.Insert)).MakeGenericMethod(new[] { type });
            ListRemove = typeof(IList<object>).GetMethod(nameof(IList<object>.RemoveAt)).MakeGenericMethod(new[] { type });
            ListGet = typeof(Enumerable).GetMethod(nameof(Enumerable.ElementAt)).MakeGenericMethod(new[] { type });

            Dictionary<string, Type> dtemp = new();
            if (acceptableTypes != null)
                foreach (var at in acceptableTypes)
                {
                    dtemp[at.ToString()] = at;
                }
            AcceptableTypes = dtemp;
        }
    }
}
