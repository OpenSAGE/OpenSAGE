using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Serialization;

using OpenSage.FileFormats.Apt;

namespace OpenSage.Tools.AptEditor.Util
{

    public record OperationState(int Code, string Info);

    public class TreeNode
    {
        public Type Type { get; }
        public object? Obj { get; }
        public TreeNode? Parent { get; private set; }

        public string DisplayName { get; private set; }

        private SortedDictionary<string, PropertyInfo> _normalProperties;
        private SortedDictionary<string, PropertyInfo> _nodeProperties;
        private SortedDictionary<string, (PropertyInfo, DataStorageListAttribute)> _listProperties;

        private readonly List<TreeNode> _children;
        private readonly bool _canChangeChildren;
        private readonly DataStorageListAttribute? _attr;

        public IEnumerable<TreeNode> Children => _children.AsReadOnly();
        public IEnumerable<string> Fields => _normalProperties.Keys.AsEnumerable();

        public override string ToString()
        {
            return $"{DisplayName}(Type: {Type}, Children: {Children.Count()}, Fields: {Fields.Count()})";
        }

        public TreeNode? AddNode()
        {
            return null;
        }

        public bool RemoveNode(TreeNode n)
        {
            return false;
        }

        public (bool, TreeNode?) OverwriteNode(TreeNode oldNode, TreeNode newNode)
        {
            return (false, null);
        }

        // general

        public string? GetRaw(string field)
        {
            if (Obj == null)
                return null;
            var succ = _normalProperties.TryGetValue(field, out var desc);
            if (!succ)
                return null;
            else
            {
                var val = desc!.GetValue(Obj);
                string pval = JsonSerializer.Serialize(val);
                return pval;
            }
        }

        public OperationState Get(string field)
        {
            if (Obj == null)
                return new(2, "Null Object");
            var succ = _normalProperties.TryGetValue(field, out var desc);
            if (!succ)
                return new(1, "Invalid field");
            else
            {
                var val = desc!.GetValue(Obj);
                try
                {
                    return new(0, JsonSerializer.Serialize(val));
                    
                }
                catch (JsonException)
                {
                    return new(0, val!.ToString());
                }
            }
        }

        public OperationState Set(string field, string pval)
        {
            if (Obj == null)
                return new(2, "Null Object");
            var succ = _normalProperties.TryGetValue(field, out var desc);
            if (!succ)
                return new(1, "Invalid field");
            else
            {
                var ret = Get(field);
                Type t = desc!.PropertyType;
                MethodInfo method = typeof(JsonSerializer).GetMethod("Deserialize") // TODO bug check
                             .MakeGenericMethod(new Type[] { t });
                var val = method.Invoke(null, new object[] { pval });
                desc!.SetValue(Obj, val);
                return ret;
            }
        }

        public TreeNode(
            IEnumerable<(string, object)> objs,
            DataStorageListAttribute attr,
            TreeNode? parent,
            string? dispName = null,
            Type? t = null,
            Dictionary<object, TreeNode>? record = null
            )
        {
            _canChangeChildren = true;
            _attr = attr;

            Obj = null;
            Type = t == null ? typeof(object) : t;
            Parent = parent;
            DisplayName = string.IsNullOrEmpty(dispName) ? $"Unnamed {Type.Name}" : dispName;

            _normalProperties = new();
            _nodeProperties = new();
            _listProperties = new();

            _children = new();
            foreach (var (dn, o) in objs)
                _children.Add(new(o, this, dn, t, record));
        }

        public TreeNode(
            object? obj,
            TreeNode? parent,
            string? dispName = null,
            Type? t = null,
            Dictionary<object, TreeNode>? record = null
            )
        {
            if (record == null)
                record = new();
            else if (obj != null)
            {
                if (record.TryGetValue(obj, out var nobj))
                {
                    obj = null;
                }
                else
                    record[obj] = this;
            }

            _canChangeChildren = false;
            _attr = null;

            Obj = obj;
            Type = obj == null ? (t == null ? typeof(object) : t) : obj.GetType();
            Parent = parent;
            DisplayName = string.IsNullOrEmpty(dispName) ? $"Unnamed {Type.Name}" : dispName;

            _normalProperties = new();
            _nodeProperties = new();
            _listProperties = new();

            _children = new();

            // enumerate and categorize properties
            foreach (var field in Type.GetProperties())
            {
                object[] attrs = field.GetCustomAttributes(typeof(DataStorageListAttribute), true);
                if (attrs.Length > 0)
                    _listProperties.Add(field.Name, (field, (DataStorageListAttribute) attrs[0]));
                else if (field.PropertyType.GetInterfaces().Contains(typeof(IDataStorage)))
                    _nodeProperties.Add(field.Name, field);
                else
                    _normalProperties.Add(field.Name, field);

            }

            // init subnodes
            foreach (var np in _nodeProperties)
                _children.Add(new(obj == null ? null : np.Value.GetValue(obj), this, np.Key, np.Value.PropertyType, record));

            // init subnode lists
            foreach (var (field, (lp, attr)) in _listProperties)
            {
                List<(string, object)> l = new();
                if (lp.PropertyType.GetInterfaces().Where(a => a.Name.StartsWith("IList")).Count() > 0)
                {
                    var curId = 0;
                    if (obj != null && lp.GetValue(obj) != null)
                        foreach (var elem in (IEnumerable<object>) lp.GetValue(obj)!)
                            l.Add(($"{field} #{curId++}", elem));
                }
                else if (lp.PropertyType.GetInterfaces().Where(a => a.Name.StartsWith("IDictionary")).Count() > 0)
                {
                    if (obj != null && lp.GetValue(obj) != null)
                        foreach (var elem in (IEnumerable<object>) lp.GetValue(obj)!)
                        {
                            var ek = elem.GetType().GetProperty("Key")!.GetValue(elem);
                            var ev = elem.GetType().GetProperty("Value")!.GetValue(elem);
                            l.Add((ek.ToString(), ev)!);
                        }
                }
                var c = new TreeNode(l, attr, this, field, lp.PropertyType.GetGenericArguments().Last(), record);
                _children.Add(c);
            }
        }
    }


    public class TreeList : EditManager
    {
        // bijection & maintenance

        private TreeNode _base;
        private int _newId;

        private Dictionary<int, TreeNode> _mapId2Node;
        private Dictionary<TreeNode, int> _mapNode2Id;

        public TreeList(object obj): base()
        {
            _base = new TreeNode(obj, null, "[Root]", obj.GetType());
            _newId = 1;
            _mapId2Node = new();
            _mapNode2Id = new();

            AddNode2(_base);
        }

        private List<(TreeNode, int)> GetNodeRecord(TreeNode n)
        {
            List<(TreeNode, int)> res = new();
            Queue<TreeNode> q = new();
            q.Enqueue(n);
            while (q.Count > 0)
            {
                var c = q.Dequeue();
                var succ = _mapNode2Id.TryGetValue(c, out var cid);
                if (!succ)
                    cid = -1;
                res.Add((c, cid));
                foreach (var cc in c.Children)
                    q.Enqueue(cc);
            }
            return res;
        }

        private int AddNode(TreeNode c)
        {
            var cid = _newId++;
            _mapNode2Id[c] = cid;
            _mapId2Node[cid] = c;
            return cid;
        }

        private void OverwriteNode(TreeNode n, int id)
        {
            if (id < 1 || id >= _newId || n == null)
                throw new InvalidOperationException();
            EraseNode(n);
            EraseNode(id);
            _mapNode2Id[n] = id;
            _mapId2Node[id] = n;
        }

        private int EraseNode(TreeNode n)
        {
            if (_mapNode2Id.ContainsKey(n))
            {
                var cid = _mapNode2Id[n];
                _mapId2Node.Remove(cid);
                _mapNode2Id.Remove(n);
                return cid;
            }
            return -1;
        }

        private int EraseNode(int cid)
        {
            if (_mapId2Node.ContainsKey(cid))
            {
                var n = _mapId2Node[cid];
                _mapId2Node.Remove(cid);
                _mapNode2Id.Remove(n);
                return cid;
            }
            return -1;
        }

        private void AddNode2(TreeNode n)
        {
            foreach(var (c, cid) in GetNodeRecord(n))
                if (cid < 0)
                    AddNode(c);
        }

        private List<(TreeNode, int)> EraseNode2(TreeNode n)
        {
            var ans = GetNodeRecord(n);
            foreach (var (c, cid) in ans)
                if (cid > 0)
                    EraseNode(c);
            return ans;
        }

        private void OverwriteRecord(List<(TreeNode, int)> r)
        {
            foreach (var (c, cid) in r)
                if (cid > 0)
                    OverwriteNode(c, cid);
                else if (cid < 0)
                    EraseNode(c);
                else
                    throw new InvalidOperationException();
        }

        public int GetID(TreeNode n) { return _mapNode2Id.TryGetValue(n, out var ret) ? ret : -1; }
        public TreeNode? GetNode(int id) { return _mapId2Node.TryGetValue(id, out var ret) ? ret : null; }

        private bool IsInvalidNode(TreeNode? n) { return n == null || !_mapNode2Id.ContainsKey(n); }
        private bool IsInvalidID(int id) { return id < 1 || !_mapId2Node.ContainsKey(id); }

        // operation towards outside

        public OperationState AddNode(int parentNodeId)
        {
            if (IsInvalidID(parentNodeId))
                return new(-1, "Invalid parent node ID");
            else
            {
                var parentNode = GetNode(parentNodeId)!;
                TreeNode? newNode = parentNode.AddNode();

                if (newNode == null)
                    return new(-2, "Internal Failure");
                else
                {
                    // operate
                    AddNode2(newNode);
                    var newId = GetID(newNode);
                    if (newId < 1)
                        throw new NotImplementedException("This should not happen, check bugs");
                    // maintain edit manager
                    var rec = GetNodeRecord(newNode);
                    PushActionNoEdit(new EditAction(
                        () =>
                        {
                            parentNode.OverwriteNode(null, newNode);
                            OverwriteRecord(rec);
                        },
                        () =>
                        {
                            parentNode.RemoveNode(newNode);
                            EraseNode2(newNode);
                        },
                        $"Add New {parentNode.DisplayName}"
                        ));

                    return new(0, $"{{ \"id\": {newId} }}");
                }
            }
            return new(-3, "This line of code should not be run, check bugs");
        }

        public OperationState RemoveNode(int id)
        {
            if (IsInvalidID(id))
                return new(-1, "Invalid node ID");
            var node = GetNode(id);
            var parentNode = node!.Parent;
            if (parentNode == null)
                return new(-3, "Can not remove the root node");
            var succ = parentNode.RemoveNode(node);
            if (succ)
            {
                // operate
                var rec = EraseNode2(node);
                // maintain
                PushActionNoEdit(new EditAction(
                    () =>
                    {
                        parentNode.RemoveNode(node);
                        EraseNode2(node);
                    },
                    () =>
                    {
                        parentNode.OverwriteNode(null, node);
                        OverwriteRecord(rec);
                    },
                    $"Remove {parentNode.DisplayName}.{node.DisplayName}"
                    ));

                return new(0, "Successful");
            }
            else
                return new(-2, "Internal Failure");
        }

        public OperationState OverwriteNode(int id, TreeNode newNode)
        {
            throw new NotImplementedException();
            if (IsInvalidID(id))
                return new(-1, "Invalid node ID");
            var oldNode = GetNode(id)!;
            if (oldNode.Type != newNode.Type)
                return new(-4, "Invalid node type");
            var parentNode = oldNode!.Parent;
            if (parentNode == null)
                return new(-3, "Can not overwrite the root node");

            var (succ, retnNode) = parentNode.OverwriteNode(oldNode, newNode);
            if (succ)
            {
                // operate
                var recOld = GetNodeRecord(oldNode);
                EraseNode2(oldNode);
                AddNode2(newNode);
                // maintain
                // TODO
                var rec = GetNodeRecord(newNode);
                PushActionNoEdit(new EditAction(
                        () =>
                        {
                            parentNode.OverwriteNode(oldNode, newNode);
                            EraseNode2(oldNode);
                            OverwriteRecord(rec);
                        },
                        () =>
                        {
                            parentNode.RemoveNode(newNode);
                            EraseNode2(newNode);
                            OverwriteRecord(recOld);
                        },
                        $"Overwrite {parentNode.DisplayName}"
                        ));

                return new(0, "Successful");
            }
            else
                return new(-2, "Internal Failure");
        }

        public OperationState GetChildren(int id)
        {
            if (IsInvalidID(id))
                return new(-1, "Invalid node ID");
            var n = GetNode(id);
            var resRaw = n!.Children;
            if (resRaw == null)
                return new(-2, "Internal Failure");
            var res = resRaw.Select(n => GetID(n));
            return new(0, JsonSerializer.Serialize(res));
        }

        public OperationState GetFields(int id)
        {
            if (IsInvalidID(id))
                return new(-1, "Invalid node ID");
            var n = GetNode(id);
            var resRaw = n!.Fields;
            if (resRaw == null)
                return new(-2, "Internal Failure");
            return new(0, JsonSerializer.Serialize(resRaw));
        }

        public OperationState GetType(int id)
        {
            if (IsInvalidID(id))
                return new(-1, "Invalid node ID");
            var n = GetNode(id);
            var resRaw = n!.Type;
            if (resRaw == null)
                return new(-2, "Internal Failure");
            return new(0, resRaw.ToString());
        }

        public OperationState GetDisplayName(int id)
        {
            if (IsInvalidID(id))
                return new(-1, "Invalid node ID");
            var n = GetNode(id);
            var resRaw = n!.DisplayName;
            if (resRaw == null)
                return new(-2, "Internal Failure");
            return new(0, resRaw);
        }

        public OperationState Get(int id, string field)
        {
            if (IsInvalidID(id))
                return new(-1, "Invalid node ID");
            var n = GetNode(id);
            var resRaw = n!.Get(field);
            if (resRaw == null)
                resRaw = new(-2, "The operation did not report its state");
            return resRaw;
        }

        public OperationState Set(int id, string field, string value)
        {
            if (IsInvalidID(id))
                return new(-1, "Invalid node ID");
            var n = GetNode(id)!;
            var oldVal = n.GetRaw(field);
            var resRaw = n.Set(field, value);
            if (resRaw == null)
                resRaw = new(-2, "The operation did not report its state");
            // operate
            // do nothing
            // maintain
            if (resRaw.Code == 0)
                PushActionNoEdit(new EditAction(
                    () => n.Set(field, value),
                    () => n.Set(field, oldVal!),
                    $"Edit {field}"
                    ));
            return resRaw;
        }

        // display

        public void Print()
        {
            Stack<(int, string)> q = new();
            q.Push((1, ""));
            while (q.Count > 0)
            {
                var (nid, nstr) = q.Pop();
                Console.WriteLine(nstr + $"#{nid} {GetType(nid).Info} {GetDisplayName(nid).Info}");

                var f = GetFields(nid).Info;
                Console.WriteLine(nstr + " Fields: " + f);
                var farr = JsonSerializer.Deserialize<List<string>>(f);
                foreach (var ff in farr)
                    Console.WriteLine(nstr + $"  {ff}: {Get(nid, ff).Info}");

                var c = GetChildren(nid).Info;
                Console.WriteLine(nstr + " Children: " + c);
                var carr = JsonSerializer.Deserialize<List<int>>(c);
                carr.Reverse();
                foreach (var cc in carr)
                    q.Push((cc, nstr + "   "));
            }
        }
    }
}
