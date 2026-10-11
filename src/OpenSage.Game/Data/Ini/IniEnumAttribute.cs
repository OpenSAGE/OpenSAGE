using System;

namespace OpenSage.Data.Ini;

/// <summary>
/// AllowMultiple so that you can specify a different AddedIn for
/// different values.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class IniEnumAttribute : Attribute
{
    public string[] Names { get; }

    public bool HasAddedIn { get; private set; }
    SageGame _addedIn;
    public SageGame AddedIn
    {
        get => _addedIn;
        set
        {
            _addedIn = value;
            HasAddedIn = true;
        }
    }

    public IniEnumAttribute(string name)
    {
        Names = [name];
    }

    public IniEnumAttribute(string name, params string[] otherNames)
    {
        Names = [name, .. otherNames];
    }
}
