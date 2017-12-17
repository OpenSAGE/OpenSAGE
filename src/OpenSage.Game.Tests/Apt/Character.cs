using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Game.Tests.Apt
{
    //base class for all characters used in apt
    class Character
    {

        public enum Type
        {
            SHAPE = 1,
            TEXT = 2,
            FONT = 3,
            BUTTON = 4,
            SPRITE = 5,
            SOUND = 6,
            IMAGE = 7,
            MORPH = 8,
            MOVIE = 9,
            STATICTEXT = 10,
            NONE = 11,
            VIDEO = 12
        };

    }
}
