using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript.Library;

namespace OpenSage.Gui.Apt.ActionScript
{
    public class ObjectContext
    {
        /// <summary>
        /// The item that this context is connected to
        /// </summary>
        public IDisplayItem Item { get; private set; }

        /// <summary>
        /// Contains functions and member variables
        /// </summary>
        public Dictionary<string, Value> Variables { get; set; }

        public List<Value> Constants { get; set; }

        /// <summary>
        /// this ActionScript object is not bound bound to an item, e.g. for global object
        /// </summary>
        public ObjectContext()
        {
            //Actionscript variables are not case sensitive!
            Variables = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);
            Constants = new List<Value>();
        }

        /// <summary>
        /// this ActionScript object is bound to an item
        /// </summary>
        /// <param name="item"></param>
        /// the item that this context is bound to
        public ObjectContext(IDisplayItem item)
        {
            Variables = Variables = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);
            Constants = new List<Value>();
            Item = item;

            //initialize item dependent properties
            InitializeProperties();
        }

        /// <summary>
        /// return a variable, when not present return Undefined
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public virtual Value GetMember(string name)
        {
            Value result;

            if (!Variables.TryGetValue(name, out result))
            {
                Debug.WriteLine("[WARN] Undefined variable: " + name);
                result = Value.Undefined();
            }

            return result;
        }

        /// <summary>
        /// Check wether or not a string is a builtin flash function
        /// </summary>
        /// <param name="name">function name</param>
        /// <returns></returns>
        public virtual bool IsBuiltInFunction(string name)
        {
            return Builtin.IsBuiltInFunction(name);
        }

        /// <summary>
        /// Execute a builtin function maybe move builtin functions elsewhere
        /// </summary>
        /// <param name="name"><function name/param>
        public virtual void CallBuiltInFunction(string name, Value[] args)
        {
            Builtin.CallBuiltInFunction(name, this, args);
        }

        private void InitializeProperties()
        {
            //TODO: avoid new fancy switch
            switch (Item.Character)
            {
                case Text t:
                    Variables["textColor"] = Value.FromString(t.Color.ToHex());
                    break;
            }
        }

        /// <summary>
        /// used by text
        /// </summary>
        /// <param name="value">value name</param>
        /// <returns></returns>
        public Value ResolveValue(string value,ObjectContext ctx)
        {
            var path = value.Split('.');
            var obj = this;

            if(path.Length>1)
            {
                if (Builtin.IsBuiltInVariable(path.First()))
                {
                    obj = Builtin.GetBuiltInVariable(path.First(),ctx).ToObject();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return obj.GetMember(path[1]);
        }

        public Value GetProperty(int property)
        {
            var type = (PropertyType)property;
            Value result = null;

            switch (type)
            {
                case PropertyType.Target:
                    result = Value.FromString(GetTargetPath());
                    break;
            }

            return result;
        }

        /// <summary>
        /// Calculates the absolute target path
        /// </summary>
        /// <returns>the target</returns>
        internal string GetTargetPath()
        {
            string path;

            if (Item.Parent == null)
                path = "/";
            else
            {
                path = Item.Parent.ScriptObject.GetTargetPath();
                path += Item.Name;
            }

            return path;
        }
    }
}
