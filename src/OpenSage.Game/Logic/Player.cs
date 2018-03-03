using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Logic
{
    // TODO: Abstract this out, or try to include all factions from every game?
    public enum Side
    {
        Civilian,
        Observer,
        America,
        China,
        GLA
    }

    public class Player
    {
        public string Name { get; private set; }
        public Side Side { get; private set; }

        public uint Money { get; set; }

        // TODO: Should this be derived from the player's buildings so that it doesn't get out of sync?
        public uint Energy { get; set; }

        public IReadOnlyCollection<GameObject> SelectedUnits => _selectedUnits;

        // TODO: Does the order matter? Is it ever visible in UI?
        private HashSet<GameObject> _selectedUnits;

        public Player()
        {
            _selectedUnits = new HashSet<GameObject>();
        }

        public void SelectUnits(IEnumerable<GameObject> units, bool additive = false)
        {
            if (additive)
            {
                _selectedUnits.UnionWith(units);
            }
            else
            {
                _selectedUnits = units.ToSet();
            }
        }

        public static Player FromTemplate(PlayerTemplate template, ContentManager content)
        {
            // TODO: Use rest of the properties from the template
            return new Player
            {
                Side = ParseSide(template.Side),
                Name = content.TranslationManager.Lookup(template.DisplayName),
                Money = (uint) template.StartMoney
            };
        }

        private static Side ParseSide(string side)
        {
            // TODO: Validate.
            return (Side) Enum.Parse(typeof(Side), side);
        }
    }
}
