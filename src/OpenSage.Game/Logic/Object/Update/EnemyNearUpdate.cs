using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the the use of <see cref="ObjectDefinition.VoiceMeetEnemy"/> and also allows the 
    /// object to use the <see cref="ModelConditionFlag.EnemyNear"/> condition state which will be 
    /// triggered when an enemy object is within this object's 
    /// <see cref="ObjectDefinition.VisionRange"/>.
    /// </summary>
    public sealed class EnemyNearUpdateModuleData : UpdateModuleData
    {
        internal static EnemyNearUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<EnemyNearUpdateModuleData> FieldParseTable = new IniParseTable<EnemyNearUpdateModuleData>();
    }
}
