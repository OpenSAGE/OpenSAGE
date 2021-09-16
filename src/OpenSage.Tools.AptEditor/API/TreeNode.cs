
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Tools.AptEditor.API
{
    enum NodeType
    {
        Logical,

        AptFile,
        ConstFile,
        ImageMap,
        GeometryMap,


    }

    enum NodeValueType
    {
        Null,

    }

    record OperationState(int Code, string Info);
    record PackedValue(NodeValueType Type, string Value);

    class SerializeUtils
    {

        public TreeNode GetNewNode(int id, NodeType type)
        {
            throw new NotImplementedException();
        }

    }

    class TreeNode
    {
        public int ID { get; }
        public NodeType Type { get; }
        public int Parent { get; private set; }
        public IEnumerable<int> Children => _children.AsReadOnly();

        private readonly object _obj;
        private readonly List<int> _children;

        public Func<NodeType, OperationState> AddNode { get; private set; }
        public Func<int, OperationState> RemoveNode { get; private set; }

        public Func<IEnumerable<string>> GetFields { get; private set; }
        public Func<int, PackedValue> GetValue { get; private set; }
        public Func<int, PackedValue, OperationState> SetValue { get; private set; }

        private TreeNode(int id, object obj, NodeType type, 
                    Func<NodeType, OperationState> addNode,
                    Func<int, OperationState> removeNode,
                    Func<IEnumerable<string>> getFields,
                    Func<int, PackedValue> getValue,
                    Func<int, PackedValue, OperationState> setValue
            )
        {
            _obj = obj;
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


    class TreeList
    {
        private List<TreeNode?> _list;


        public TreeList()
        {
            _list = new() { null };
        }

        public int FirstUsableID()
        {
            return -1;
        }

        public OperationState AddNode(int parentNode, NodeType type)
        {
            if (parentNode < 0 || parentNode >= _list.Count)
                return new(-1, "Invalid Parent Node");
            else if (parentNode != 0 && _list[parentNode] == null)
                return new(-1, "Invalid Parent Node");
            else if (parentNode == 0)
            {
                var newId = FirstUsableID();
                
            }
            return null;
        }

        public OperationState RemoveNode(int node)
        {

            return null;
        }

        public OperationState GetFields(int node)
        {

            return null;
        }

        public OperationState Get(int node, int field)
        {

            return null;
        }

        public OperationState Set(int node, int field, PackedValue value)
        {

            return null;
        }
    }
}
