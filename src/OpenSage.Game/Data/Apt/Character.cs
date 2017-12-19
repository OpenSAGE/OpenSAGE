using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt
{
    public enum CharacterType : uint
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

    //base class for all characters used in apt
    public class Character
    {
        private const uint SIGNATURE = 0x09876543;

        static public Character Create(BinaryReader br,Character root=null)
        {
            Character ch = null;

            var type = br.ReadUInt32AsEnum<CharacterType>();
            var sig = br.ReadUInt32();

            if (sig != SIGNATURE)
                throw new InvalidDataException();

           


            switch (type)
            {
                //must be the root object. Movie does contain itself so, do a simple check
                case CharacterType.MOVIE:
                    if (root==null)
                        ch = Movie.Parse(br);
                    else
                        return root;
                    break;
                case CharacterType.SHAPE:
                    break;
                case CharacterType.TEXT:
                    break;
                case CharacterType.FONT:
                    break;
                case CharacterType.BUTTON:
                    break;
                case CharacterType.SPRITE:
                    break;
                case CharacterType.SOUND:
                    break;
                case CharacterType.IMAGE:
                    break;
                case CharacterType.MORPH:
                    break;
                case CharacterType.STATICTEXT:
                    break;
                case CharacterType.NONE:
                    break;
                case CharacterType.VIDEO:
                    break;
                default:
                    throw new NotImplementedException();
            }

            return ch;
        }
    }
}
