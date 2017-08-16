using System;
using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    // TODO: Figure these out properly.
    [Flags]
    public enum RoadType : uint
    {
        None = 0,

        Start = 2,
        End = 4,

        Angled = 8,

        Unknown1 = 16,
        Unknown2 = 32,

        Unknown1_Angled = Unknown1 | Angled,
        Unknown2_Angled = Unknown2 | Angled,

        TightCurve = 64,

        Unknown1_TightCurve = Unknown1 | TightCurve,
        Unknown2_TightCurve = Unknown2 | TightCurve,

        EndCap = 128,

        BroadCurveStart = Start,
        BroadCurveEnd = End,

        AngledStart = Angled | Start,
        AngledEnd = Angled | End,

        TightCurveStart = TightCurve | Start,
        TightCurveEnd = TightCurve | End,

        BroadCurveEndCapStart = BroadCurveStart | EndCap,
        BroadCurveEndCapEnd = BroadCurveEnd | EndCap,

        AngledEndCapStart = AngledStart | EndCap,
        AngledEndCapEnd = AngledEnd | EndCap,

        TightCurveEndCapStart = TightCurveStart | EndCap,
        TightCurveEndCapEnd = TightCurveEnd | EndCap
    }
}
