namespace OpenSage.Logic.Object.Helpers
{
    internal sealed class ObjectWeaponStatusHelper : ObjectHelperModule
    {
        private protected override void RunUpdate(BehaviorUpdateContext context)
        {
            context.GameObject.UpdateWeaponModelConditionFlags();
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }
}
