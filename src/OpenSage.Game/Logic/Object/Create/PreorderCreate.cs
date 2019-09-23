﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of the PREORDER ModelConditionState with this object which in turn is only 
    /// triggered by the presence of registry key 'Preorder' set to '1' in 
    /// HKLM\Software\ElectronicArts\EAGames\Generals.
    /// </summary>
    public sealed class PreorderCreateModuleData : CreateModuleData
    {
        internal static PreorderCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PreorderCreateModuleData> FieldParseTable = new IniParseTable<PreorderCreateModuleData>();
    }
}
