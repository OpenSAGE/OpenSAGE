using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public class SpecialPowerCreate : CreateModule
    {
        private readonly GameObject _gameObject;

        public SpecialPowerCreate(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        protected override void OnBuildCompleteImpl()
        {
            foreach (var specialPowerModule in _gameObject.FindBehaviors<SpecialPowerModule>())
            {
                specialPowerModule.ResetCountdown();
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    /// <summary>
    /// Forces the object's SpecialPower to start charging upon creation of the object. Required
    /// by special powers that have <see cref="SpecialPower.PublicTimer"/> set to <code>true</code>.
    /// </summary>
    public sealed class SpecialPowerCreateModuleData : CreateModuleData
    {
        internal static SpecialPowerCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpecialPowerCreateModuleData> FieldParseTable = new IniParseTable<SpecialPowerCreateModuleData>();

        internal override SpecialPowerCreate CreateModule(GameObject gameObject, GameContext context)
        {
            return new SpecialPowerCreate(gameObject);
        }
    }
}
