using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Apt.Characters;

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
                Debug.WriteLine("Undefined variable: " + name);
                result = Value.Undefined();
            }

            return result;
        }

        /// <summary>
        /// Check wether or not a string is a builtin flash function
        /// </summary>
        /// <param name="name">function name</param>
        /// <returns></returns>
        public bool IsBuiltInFunction(string name)
        {
            switch (name)
            {
                case "stop":
                    return Item is SpriteItem;
                case "setInterval":
                    return Item is SpriteItem;
                case "gotoAndPlay":
                    return Item is SpriteItem;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Execute a builtin function maybe move builtin functions elsewhere
        /// </summary>
        /// <param name="name"><function name/param>
        public void CallBuiltInFunction(string name, List<Value> args, ActionContext context)
        {
            switch (Item)
            {
                case SpriteItem si:
                    switch (name)
                    {
                        case "stop":
                            si.Stop();
                            break;
                        case "setInterval":
                            break;
                        case "gotoAndPlay":
                            si.Goto(args[0].ToString());
                            si.Play();
                            break;
                    }
                    break;
                case RenderItem ri:
                    break;
            }
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
    }
}
