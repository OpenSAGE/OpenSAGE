using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

[AddedIn(SageGame.Bfme)]
public class BuildingBehavior : UpdateModule
{
    private readonly BuildingBehaviorModuleData _moduleData;
    private bool _initial;
    private bool _isNight;
    private bool _fire;
    private bool _isGlowing;

    internal BuildingBehavior(GameObject gameObject, GameContext context, BuildingBehaviorModuleData moduleData)
        : base(gameObject, context)
    {
        _moduleData = moduleData;

        _initial = true;
        _isNight = false;
        _fire = false;
        _isGlowing = false;
    }

    internal override void Update(BehaviorUpdateContext context)
    {
        // TODO: get proper values for these
        var night = false;
        var fire = false;
        var glowing = false;

        if (night != _isNight || _initial)
        {
            var nightWindow = _moduleData.NightWindowName;
            if (night)
            {
                GameObject.Drawable.ShowSubObject(nightWindow);
            }
            else
            {
                GameObject.Drawable.HideSubObject(nightWindow);
            }

            _isNight = night;
        }

        if (fire != _fire || _initial)
        {
            var fireWindow = _moduleData.FireWindowName;
            if (fire)
            {
                GameObject.Drawable.ShowSubObject(fireWindow);
                foreach (var fireName in _moduleData.FireNames)
                {
                    GameObject.Drawable.ShowSubObject(fireName);
                }
            }
            else
            {
                GameObject.Drawable.HideSubObject(fireWindow);

                foreach (var fireName in _moduleData.FireNames)
                {
                    GameObject.Drawable.HideSubObject(fireName);
                }
            }

            _fire = fire;
        }

        if (glowing != _isGlowing || _initial)
        {
            var glowWindow = _moduleData.GlowWindowName;
            if (glowing)
            {
                GameObject.Drawable.ShowSubObject(glowWindow);
            }
            else
            {
                GameObject.Drawable.HideSubObject(glowWindow);
            }

            _isGlowing = glowing;
        }

        if (_initial)
        {
            _initial = false;
        }
    }
}


public sealed class BuildingBehaviorModuleData : BehaviorModuleData
{
    internal static BuildingBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<BuildingBehaviorModuleData> FieldParseTable = new IniParseTable<BuildingBehaviorModuleData>
    {
        { "NightWindowName", (parser, x) => x.NightWindowName = parser.ParseString() },
        { "FireWindowName", (parser, x) => x.FireWindowName = parser.ParseString() },
        { "GlowWindowName", (parser, x) => x.GlowWindowName = parser.ParseString() },
        { "FireName", (parser, x) => x.FireNames.Add(parser.ParseString()) },
    };

    public string NightWindowName { get; private set; }
    public string FireWindowName { get; private set; }
    public string GlowWindowName { get; private set; }
    public List<string> FireNames { get; private set; } = new List<string>();

    internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
    {
        return new BuildingBehavior(gameObject, context, this);
    }
}
