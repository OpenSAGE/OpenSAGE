using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OpenSage.FileFormats;
using OpenSage.FileFormats.Apt;

namespace OpenSage.Tools.AptEditor.Util
{

    public record OperationState(int Code, string Info);

    public class TreeNode
    {
        public Type Type { get; }
        public object? Obj { get; }
        public TreeNode? Parent { get; private set; }

        public string FieldName { get; private set; }

        private SortedDictionary<string, PropertyInfo> _normalProperties;
        private SortedDictionary<string, PropertyInfo> _nodeProperties;
        private SortedDictionary<string, (PropertyInfo, DataStorageListAttribute)> _listProperties;

        private readonly List<TreeNode> _children;
        private readonly bool _canChangeChildren;
        private readonly DataStorageListAttribute? _attr;

        public IEnumerable<TreeNode> Children => _children.AsReadOnly();
        public IEnumerable<string> Fields => _normalProperties.Keys.AsEnumerable();
        public IEnumerable<string> FieldTypes => _normalProperties.Values.Select(x => x.PropertyType.ToString()).AsEnumerable();

        public override string ToString()
        {
            return $"{FieldName}(Type: {Type}, Children: {Children.Count()}, Fields: {Fields.Count()})";
        }

        public TreeNode? AddNode(TreeNode? newNode = null, int index = -1, string? type = null)
        {
            if (!_canChangeChildren)
                return null;
            var attr = _attr!;
            object nobj;
            if (newNode == null)
            {
                nobj = attr.Add(Obj, index, type);
                newNode = new(nobj, this, null, Type);
            }
            else
            {
                nobj = newNode.Obj!;
                attr.Insert(Obj, nobj, index);
            }
            if (index < 0)
                _children.Add(newNode);
            else
                _children.Insert(index, newNode);
            return newNode;
        }

        public int RemoveNode(TreeNode n)
        {
            if (!_canChangeChildren)
                return -1;
            var attr = _attr!;
            var index = _children.IndexOf(n);
            if (index >= 0)
            {
                attr.Remove(Obj, index);
                _children.RemoveAt(index);
            }
            return index;
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
                string pval = SerializeUtilities.Serialize(val);
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
                return new(0, SerializeUtilities.Serialize(val));
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
                MethodInfo method = typeof(SerializeUtilities).GetMethod(nameof(SerializeUtilities.Deserialize))! // TODO bug check
                             .MakeGenericMethod(new Type[] { t });
                var val = method.Invoke(null, new object[] { pval });
                desc!.SetValue(Obj, val);
                return ret;
            }
        }

        public TreeNode(
            IEnumerable<object> objs,
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
            FieldName = string.IsNullOrEmpty(dispName) ? $"Unnamed {Type.Name}" : dispName;

            _normalProperties = new();
            _nodeProperties = new();
            _listProperties = new();

            _children = new();
            foreach (var o in objs)
                _children.Add(new(o, this, dispName, t, record));
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
            FieldName = string.IsNullOrEmpty(dispName) ? $"Unnamed {Type.Name}" : dispName;

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
                else if (field.PropertyType.GetInterfaces().Contains(typeof(IMemoryStorage)))
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
                List<object> l = new();
                if (lp.PropertyType.GetInterfaces().Where(a => a.Name.StartsWith("IList")).Count() > 0)
                {
                    var curId = 0;
                    if (obj != null && lp.GetValue(obj) != null)
                        foreach (var elem in (IEnumerable<object>) lp.GetValue(obj)!)
                            l.Add(elem);
                }
                else if (lp.PropertyType.GetInterfaces().Where(a => a.Name.StartsWith("IDictionary")).Count() > 0)
                {
                    throw new NotImplementedException("Not used in any known structures");
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


    public class TreeViewEditor : EditManager
    {
        // bijection & maintenance

        private TreeNode _base;
        private int _newId;

        private Dictionary<int, TreeNode> _mapId2Node;
        private Dictionary<TreeNode, int> _mapNode2Id;

        public TreeViewEditor(object obj): base()
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

        // node operations

        public OperationState AddNode(int parentNodeId, string? type = null)
        {
            if (IsInvalidID(parentNodeId))
                return new(-1, "Invalid parent node ID");
            else
            {

                // try operating
                var parentNode = GetNode(parentNodeId)!;
                TreeNode? newNode = parentNode.AddNode(type: type);

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
                            parentNode.AddNode(newNode);
                            OverwriteRecord(rec);
                        },
                        () =>
                        {
                            parentNode.RemoveNode(newNode);
                            EraseNode2(newNode);
                        },
                        $"Add New {parentNode.FieldName}"
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
            var index = parentNode.RemoveNode(node);
            if (index >= 0)
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
                        parentNode.AddNode(node, index);
                        OverwriteRecord(rec);
                    },
                    $"Remove {parentNode.FieldName}.{node.FieldName}"
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
            return new(0, SerializeUtilities.Serialize(res));
        }

        public OperationState GetFields(int id)
        {
            if (IsInvalidID(id))
                return new(-1, "Invalid node ID");
            var n = GetNode(id);
            List<IEnumerable<string>> resRaw = new() { n!.Fields, n!.FieldTypes };
            if (resRaw == null)
                return new(-2, "Internal Failure");
            return new(0, SerializeUtilities.Serialize(resRaw));
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

        public OperationState GetFieldName(int id)
        {
            if (IsInvalidID(id))
                return new(-1, "Invalid node ID");
            var n = GetNode(id);
            var resRaw = n!.FieldName;
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

        // operations

        public OperationState StartMerging(string desc)
        {
            if (ForceMergeActions)
                return new(1, "Merging already started");
            ForceMergeActions = true;
            CustomMergedActionsDescription = desc;
            return new(0, "Successful");
        }

        public OperationState EndMerging()
        {
            if (!ForceMergeActions)
                return new(1, "Merging is not started yet");
            ForceMergeActions = false;
            return new(0, "Successful");
        }

        public new OperationState Undo()
        {
            if (ForceMergeActions)
                return new(1, "Please stop merging first");
            string? desc = GetUndoDescription();
            if (desc == null)
                return new(2, "No operation to undo");
            base.Undo();
            return new(0, desc);
        }

        public new OperationState Redo()
        {
            if (ForceMergeActions)
                return new(1, "Please stop merging first");
            string? desc = GetRedoDescription();
            if (desc == null)
                return new(2, "No operation to redo");
            base.Redo();
            return new(0, desc);
        }

        // display

        public void Print()
        {
            Stack<(int, string)> q = new();
            q.Push((1, ""));
            while (q.Count > 0)
            {
                var (nid, nstr) = q.Pop();
                Console.WriteLine(nstr + $"#{nid} {GetType(nid).Info} {GetFieldName(nid).Info}");

                var f = GetFields(nid).Info;
                Console.WriteLine(nstr + " Fields: ");
                var farr = SerializeUtilities.Deserialize<List<List<string>>>(f)!;
                for (int i = 0; i < farr[0].Count; ++i)
                    Console.WriteLine(nstr + $"  {farr[0][i]}: {Get(nid, farr[0][i]).Info} ({farr[1][i]})");

                var c = GetChildren(nid).Info;
                Console.WriteLine(nstr + " Children: " + c);
                var carr = SerializeUtilities.Deserialize<List<int>>(c)!;
                carr.Reverse();
                foreach (var cc in carr)
                    q.Push((cc, nstr + "   "));
            }
        }
    }
}
