using System;
using System.IO;
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


        public static Character Create(BinaryReader reader, AptFile container)
        {
            Character ch = null;

            var type = reader.ReadUInt32AsEnum<CharacterType>();
            var sig = reader.ReadUInt32();

            if (sig != SIGNATURE)
                throw new InvalidDataException();


            switch (type)
            {
                //must be the root object. Movie does contain itself so, do a simple check
                case CharacterType.Movie:
                    if (container.IsEmpty)
                    {
                        container.IsEmpty = false;
                        ch = Movie.Parse(reader, container);
                    }
                    else
                        return null;
                    break;
                case CharacterType.Shape:
                    ch = Shape.Parse(reader);
                    break;
                case CharacterType.Text:
                    break;
                case CharacterType.Font:
                    break;
                case CharacterType.Button:
                    ch = Button.Parse(reader);
                    break;
                case CharacterType.Sprite:
                    ch = Sprite.Parse(reader);
                    break;
                case CharacterType.Sound:
                    throw new NotImplementedException("Not used in any known game");
                case CharacterType.Image:
                    break;
                case CharacterType.Morph:
                    //used only by CahPowers
                    break;
                case CharacterType.StaticText:
                    break;
                case CharacterType.None:
                    throw new NotImplementedException("Not used in any known game");
                case CharacterType.Video:
                    throw new NotImplementedException("Not used in any known game");
                default:
                    throw new NotImplementedException();
            }

            return ch;
        }
    }
}
