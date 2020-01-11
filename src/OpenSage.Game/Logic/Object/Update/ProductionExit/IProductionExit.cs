using System.Numerics;

namespace OpenSage.Logic.Object
{
    interface IProductionExit
    {
        Vector3 GetUnitCreatePoint();
        Vector3? GetNaturalRallyPoint();
    }
}
