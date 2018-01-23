using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Apt.Characters;

namespace OpenSage.Gui.Apt.ActionScript
{
    public sealed class ObjectContext
    {
        public IDisplayItem Item { get; private set; }
        public Dictionary<string, Value> Variables { get; set; }

        /// <summary>
        /// Probably should be in Variables aswell, not sure though
        /// </summary>
        public Dictionary<string, Function> Functions { get; set; }

        /// <summary>
        /// Probably should be in Variables aswell, not sure though
        /// </summary>
        public Dictionary<string, Value> Properties { get; set; }

        public List<Value> Constants { get; set; }

        /// <summary>
        /// this ActionScript object is not bound bound to an item, e.g. for global object
        /// </summary>
        public ObjectContext()
        {
            Variables = new Dictionary<string, Value>();
            Functions = new Dictionary<string, Function>();
            Properties = new Dictionary<string, Value>();
            Constants = new List<Value>();
        }

        public Value GetMember(string name)
        {
            Value result;

            if (!Variables.TryGetValue(name, out result))
                result = Value.Undefined();

            return result;
        }

        /// <summary>
        /// this ActionScript object is bound to an item
        /// </summary>
        /// <param name="item"></param>
        /// the item that this context is bound to
        public ObjectContext(IDisplayItem item)
        {
            Variables = new Dictionary<string, Value>();
            Functions = new Dictionary<string, Function>();
            Properties = new Dictionary<string, Value>();
            Constants = new List<Value>();
            Item = item;

            //initialize item dependent properties
            InitializeProperties();
        }

        private void InitializeProperties()
        {
            //TODO: avoid new fancy switch
            switch(Item.Character)
            {
                case Text t:
                    Properties["textColor"] = Value.FromString(t.Color.ToHex());
                    break;
            }
        }
    }
}
