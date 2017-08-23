using System;
using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public abstract class ObjectDrawModule : ObjectModule
    {
        internal static ObjectDrawModule ParseDrawModule(IniParser parser) => ParseModule(parser, DrawModuleParseTable);

        internal static readonly Dictionary<string, Func<IniParser, ObjectDrawModule>> DrawModuleParseTable = new Dictionary<string, Func<IniParser, ObjectDrawModule>>
        {
            { "W3DDefaultDraw", W3dDefaultDraw.Parse},
            { "W3DLaserDraw", W3dLaserDraw.Parse },
            { "W3DModelDraw", W3dModelDraw.ParseModel },
            { "W3DOverlordTankDraw", W3dOverlordTankDraw.Parse },
            { "W3DPoliceCarDraw", W3dPoliceCarDraw.Parse },
            { "W3DProjectileStreamDraw", W3dProjectileStreamDraw.Parse },
            { "W3DScienceModelDraw", W3dScienceModelDraw.Parse },
            { "W3DSupplyDraw", W3dSupplyDraw.Parse },
            { "W3DTankDraw", W3dTankDraw.Parse },
            { "W3DTankTruckDraw", W3dTankTruckDraw.Parse },
            { "W3DTruckDraw", W3dTruckDraw.Parse },
        };
    }
}
