using System.Numerics;

namespace OpenSage.Logic.Object
{
    interface IProductionExit
    {
        bool CanProduce => true;
        Vector3 GetUnitCreatePoint();
        Vector3? GetNaturalRallyPoint();
        void ProduceUnit() {}
    }
}
