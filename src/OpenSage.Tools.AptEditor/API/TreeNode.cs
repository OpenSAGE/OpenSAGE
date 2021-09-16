
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSage.Tools.AptEditor.API
{
    public enum NodeType
    {
        Logical,

        AptFile,
        ConstFile,
        ImageMap,
        GeometryMap,


    }

    public enum NodeValueType
    {
        Null,
        Json,

    }

    public record OperationState(int Code, string Info);
    public record PackedValue(NodeValueType Type, string Value);

    public class SerializeUtils
    {

        public static TreeNode GetNewNode(int id, NodeType type)
        {
            throw new NotImplementedException();
        }

        public static TreeNode GetNewNode(int id, NodeType type, object obj)
        {
            throw new NotImplementedException();
        }

    }

    public class TreeNode
    {
        public int ID { get; }
        public NodeType Type { get; }
        public object Obj { get; }
        public int Parent { get; private set; }
        public IEnumerable<int> Children => _children.AsReadOnly();

        private readonly List<int> _children;

        public Func<object, NodeType, OperationState> AddNode { get; private set; }
        public Func<object, int, OperationState> RemoveNode { get; private set; }

        public Func<object, IEnumerable<string>> GetFields { get; private set; }
        public Func<object, int, PackedValue> GetValue { get; private set; }
        public Func<object, int, PackedValue, OperationState> SetValue { get; private set; }

        public TreeNode(int id, object obj, NodeType type,
                    Func<object, NodeType, OperationState> addNode,
                    Func<object, int, OperationState> removeNode,
                    Func<object, IEnumerable<string>> getFields,
                    Func<object, int, PackedValue> getValue,
                    Func<object, int, PackedValue, OperationState> setValue
            )
        {
            Obj = obj;
            Type = type;
            ID = id;
            _children = new();
            AddNode = addNode;
            RemoveNode = removeNode;
            GetFields = getFields;
            GetValue = getValue;
            SetValue = setValue;
        }
    }


    public class TreeList
    {
        private List<TreeNode?> _list;


        public TreeList()
        {
            _list = new() { null };
        }

        private int FirstUsableID()
        {
            var ans = _list.Count;
            while (ans > 1 && _list[ans - 1] == null)
                --ans;
            return ans;
        }

        private void SetNodeToId(int id, TreeNode node)
        {
            if (id < _list.Count)
                _list[id] = node;
            else
                _list.Add(node);
        }

        private bool IsInvalidNode(int id)
        {
            return id < 1 || id >= _list.Count || _list[id] == null;
        }

        public OperationState AddNode(int parentNode, NodeType type, object obj)
        {
            if (parentNode != 0  && IsInvalidNode(parentNode))
                return new(-1, "Invalid parent node");
            else
            {
                var newId = FirstUsableID();
                TreeNode newNode;

                if (obj == null)
                    newNode = SerializeUtils.GetNewNode(newId, type);
                else
                    newNode = SerializeUtils.GetNewNode(newId, type, obj);

                if (newNode == null)
                    return new(-2, "Failed to create new node");
                else
                {
                    SetNodeToId(newId, newNode);
                    return new(newId, "Successful");
                }
            }
            return new(-3, "This line of code should not be run");
        }

        public OperationState RemoveNode(int node)
        {
            if (IsInvalidNode(node))
                return new(-1, "Invalid node ID");
            Stack<int> s = new();
            s.Push(node);
            while (s.Count > 0) // remove all child nodes
            {
                var nid = s.Pop();
                var n = _list[nid];
                _list[nid] = null;
                foreach (var c in n.Children)
                    s.Push(c);
            }
            return new(0, "Successful"); ;
        }

        public OperationState GetFields(int node)
        {
            if (IsInvalidNode(node))
                return new(-1, "Invalid node ID");
            var n = _list[node];
            var resRaw = n!.GetFields(n.Obj);
            if (resRaw == null)
                return new(0, "[]");
            return new(resRaw.Count(), JsonSerializer.Serialize(resRaw));
        }

        public OperationState Get(int node, int field)
        {
            if (IsInvalidNode(node))
                return new(-1, "Invalid node ID");
            var n = _list[node];
            var resRaw = n!.GetValue(n.Obj, field);
            if (resRaw == null)
                resRaw = new(NodeValueType.Null, string.Empty);
            return new(0, JsonSerializer.Serialize(resRaw));
        }

        public OperationState Set(int node, int field, PackedValue value)
        {
            if (IsInvalidNode(node))
                return new(-1, "Invalid node ID");
            var n = _list[node];
            var resRaw = n!.SetValue(n.Obj, field, value);
            if (resRaw == null)
                resRaw = new(-2, "The operation did not report its state");
            return new(0, JsonSerializer.Serialize(resRaw));
        }
    }
}
