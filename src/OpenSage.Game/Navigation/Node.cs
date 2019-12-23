using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenSage.Navigation
{
    enum Passability
    {
        Passable,
        Impassable,
        ImpassableForTeams,
        ImpassableForAir,
    }

    class Node
    {
        readonly int _x, _y;
        Passability _passability;
    }
}
