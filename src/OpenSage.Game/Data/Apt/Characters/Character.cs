using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.Characters
{
    public enum CharacterType : uint
    {
        Shape = 1,
        Text = 2,
        Font = 3,
        Button = 4,
        Sprite = 5,
        Sound = 6,
        Image = 7,
        Morph = 8,
        Movie = 9,
        StaticText = 10,
        None = 11,
        Video = 12
    };

    //base class for all characters used in apt
    public class Character
    {
        private const uint SIGNATURE = 0x09876543;
        public AptFile Container { get; private set; }


        public static Character Create(BinaryReader br,AptFile c)
        {
            Character ch = null;

            var type = br.ReadUInt32AsEnum<CharacterType>();
            var sig = br.ReadUInt32();

            if (sig != SIGNATURE)
                throw new InvalidDataException();
      

            switch (type)
            {
                //must be the root object. Movie does contain itself so, do a simple check
                case CharacterType.Movie:
                    if (c.IsEmpty)
                    {
                        c.IsEmpty = false;
                        ch = Movie.Parse(br, c);
                    }
                    else
                        return null;
                    break;
                case CharacterType.Shape:
                    ch = Shape.Parse(br); 
                    break;
                case CharacterType.Text:
                    break;
                case CharacterType.Font:
                    break;
                case CharacterType.Button:
                    ch = Button.Parse(br);
                    break;
                case CharacterType.Sprite:
                    ch = Sprite.Parse(br);
                    break;
                case CharacterType.Sound:
                    throw new NotImplementedException("Not used in any known game");
                    break;
                case CharacterType.Image:
                    break;
                case CharacterType.Morph:
                    break;
                case CharacterType.StaticText:
                    break;
                case CharacterType.None:
                    throw new NotImplementedException("Not used in any known game");
                    break;
                case CharacterType.Video:
                    throw new NotImplementedException("Not used in any known game");
                    break;
                default:
                    throw new NotImplementedException();
            }

            return ch;
        }
    }
}
