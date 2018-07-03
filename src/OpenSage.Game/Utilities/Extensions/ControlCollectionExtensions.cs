using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Utilities.Extensions
{
    public static class ControlCollectionExtensions
    {
        /// <summary>
        /// Helper to find Controls startet with a string sequence
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="startsWith"></param>
        /// <param name="searchAllChildren"></param>
        /// <returns></returns>
        public static T[] FindControlsStratsWith<T>(this ControlCollection collection, string startsWith, bool searchAllChildren = true) where T : class
        {
            var result = new List<Control>();
            if (searchAllChildren)
            {
                foreach (var control in collection)
                {
                    result.AddRange(FindChildControls(control.Controls, startsWith));
                }
            }
            
            result.AddRange(collection.Where(x => x.Name.StartsWith(startsWith)).ToList());
            
            return Array.ConvertAll<Control, T>(result.ToArray(), it=> it as T);
        }

        /// <summary>
        /// Helper to find controls by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="searchAllChildren"></param>
        /// <returns></returns>
        public static T[] FindControlsByType<T>(this ControlCollection collection, bool searchAllChildren = true) where T : class
        {
            var result = new List<Control>();
            if (searchAllChildren)
            {
                foreach (var control in collection)
                {
                    result.AddRange(FindChildControls<T>(control.Controls));
                }
            }

            result.AddRange(collection.Where(x => x.GetType() == typeof(T)).ToList());

            return Array.ConvertAll<Control, T>(result.ToArray(), it => it as T);
        }

        private static List<Control> FindChildControls(ControlCollection collection, string startsWith)
        {
            var result = new List<Control>();
            foreach (var control in collection)
            {
                result.AddRange(FindChildControls(control.Controls, startsWith));
            }
            result.AddRange(collection.Where(x => !string.IsNullOrEmpty(x.Name) && x.Name.StartsWith(startsWith)).ToList());
            return result;
        }

        private static List<Control> FindChildControls<T>(ControlCollection collection)
        {
            var result = new List<Control>();
            foreach (var control in collection)
            {
                result.AddRange(FindChildControls<T>(control.Controls));
            }
            result.AddRange(collection.Where(x => x.GetType() == typeof(T)).ToList());
            return result;
        }

    }
}
