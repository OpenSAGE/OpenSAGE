﻿using System;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public sealed class WeaponTemplateSet
{
    public const int NumWeaponSlots = 5;

    internal static WeaponTemplateSet Parse(IniParser parser)
    {
        return parser.ParseBlock(FieldParseTable);
    }

    private static readonly IniParseTable<WeaponTemplateSet> FieldParseTable = new IniParseTable<WeaponTemplateSet>
    {
        { "Conditions", (parser, x) => x.Conditions = parser.ParseEnumBitArray<WeaponSetConditions>() },
        { "Weapon", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.Weapon = parser.ParseWeaponTemplateReference()) },
        { "PreferredAgainst", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.PreferredAgainst = parser.ParseEnumBitArray<ObjectKinds>()) },
        { "AutoChooseSources", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.AutoChooseSources = parser.ParseEnumBitArray<CommandSourceType>()) },
        { "ShareWeaponReloadTime", (parser, x) => x.ShareWeaponReloadTime = parser.ParseBoolean() },
        { "WeaponLockSharedAcrossSets", (parser, x) => x.WeaponLockSharedAcrossSets = parser.ParseBoolean() },
        { "OnlyAgainst", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.PreferredAgainst = parser.ParseEnumBitArray<ObjectKinds>()) },
        { "OnlyInCondition", (parser, x) => x.ParseWeaponSlotProperty(parser, s => s.OnlyInCondition = parser.ParseEnumBitArray<ModelConditionFlag>()) },
        { "ReadyStatusSharedWithinSet", (parser, x) => x.ReadyStatusSharedWithinSet = parser.ParseBoolean() },
        { "DefaultWeaponChoiceCritera", (parser, x) => x.DefaultWeaponChoiceCriteria = parser.ParseEnum<WeaponChoiceCriteria>() }
    };

    public ObjectDefinition ObjectDefinition { get; internal set; }

    public BitArray<WeaponSetConditions> Conditions { get; private set; } = new BitArray<WeaponSetConditions>();
    public WeaponSetSlot[] Slots { get; } = new WeaponSetSlot[NumWeaponSlots];
    public bool ShareWeaponReloadTime { get; private set; }
    public bool WeaponLockSharedAcrossSets { get; private set; }
    public WeaponChoiceCriteria DefaultWeaponChoiceCriteria { get; private set; }

    [AddedIn(SageGame.Bfme2)]
    public bool ReadyStatusSharedWithinSet { get; private set; }

    private void ParseWeaponSlotProperty(IniParser parser, Action<WeaponSetSlot> parseValue)
    {
        var weaponSlot = parser.ParseEnum<WeaponSlot>();

        ref var slot = ref Slots[(int)weaponSlot];
        if (slot == null)
        {
            slot = new WeaponSetSlot();
        }

        parseValue(slot);
    }
}

[AddedIn(SageGame.Bfme2)]
public enum WeaponChoiceCriteria
{
    PreferMostDamage,
    PreferLongestDamage,
    PreferGrabOverDamage,

    [IniEnum("PREFER_LEAST_MOVEMENT")]
    PreferLeastMovement,

    [IniEnum("SELECT_AT_RANDOM")]
    SelectAtRandom,

    UseWeaponSetDefaultCriteria,
}
