using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Logic
{
    public class Player
    {
        public string Name { get; private set; }
        public string DisplayName { get; private set; }

        public string Side { get; private set; }

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
                Side = template.Side,
                Name = template.Name,
                DisplayName = content.TranslationManager.Lookup(template.DisplayName),
                Money = (uint) template.StartMoney
            };
        }
    }

    public class Team
    {
        public string Name { get; }
        public string Owner { get; }
        public bool IsSingleton { get; }

        public Team(string name, string owner, bool isSingleton)
        {
            Name = name;
            Owner = owner;
            IsSingleton = isSingleton;
        }
    }
}
