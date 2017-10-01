using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class ObjectComponent : EntityComponent
    {
        public IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates =>
            Entity.GetComponentsInSelfAndDescendants<DrawableComponent>()
            .SelectMany(x => x.ModelConditionStates)
            .Distinct(new BitArrayEqualityComparer<ModelConditionFlag>())
            .OrderBy(x => x.NumBitsSet);

        public BitArray<ModelConditionFlag> ModelConditionFlags { get; private set; }

        public void SetModelConditionFlags(BitArray<ModelConditionFlag> flags)
        {
            ModelConditionFlags = flags;

            // TODO: Let each drawable use the appropriate TransitionState between ConditionStates.

            foreach (var drawable in Entity.GetComponentsInSelfAndDescendants<DrawableComponent>())
            {
                drawable.UpdateConditionState(flags);
            }
        }

        protected override void Start()
        {
            base.Start();

            SetModelConditionFlags(new BitArray<ModelConditionFlag>());
        }
    }
}
