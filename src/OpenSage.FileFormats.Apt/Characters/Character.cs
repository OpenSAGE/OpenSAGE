using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt.Characters
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

    public abstract class Playable : Character
    {
        [DataStorageList(typeof(Frame))]
        public List<Frame> Frames { get; protected set; }
    }

    //base class for all characters used in apt
    public abstract class Character : IMemoryStorage
    {
        public const uint SIGNATURE = 0x09876543;
        public AptFile Container { get; protected set; }

        public static Character Create(BinaryReader reader, AptFile container, Dictionary<long, Character> readCharacters = null)
        {
            // Movie does contain itself so, do a (fancier?) check
            // theoretically, the dictionary should have a max count of 1
            bool flag = false;
            Character character = null;
            var cur_pos = reader.BaseStream.Position;
            if (readCharacters == null)
                readCharacters = new();
            flag = readCharacters.TryGetValue(cur_pos, out character);
            if (flag && character != null)
                return character;

            var type = reader.ReadUInt32AsEnum<CharacterType>();
            var sig = reader.ReadUInt32();

            if (sig != SIGNATURE)
                throw new InvalidDataException();

            switch (type)
            {
                case CharacterType.Movie:
                    character = Movie.Parse(reader, container, readCharacters);
                    break;
                case CharacterType.Shape:
                    character = Shape.Parse(reader);
                    break;
                case CharacterType.Text:
                    character = Text.Parse(reader);
                    break;
                case CharacterType.Font:
                    character = Font.Parse(reader);
                    break;
                case CharacterType.Button:
                    character = Button.Parse(reader);
                    break;
                case CharacterType.Sprite:
                    character = Sprite.Parse(reader);
                    break;
                case CharacterType.Sound:
                    throw new NotImplementedException("Not used in any known game");
                case CharacterType.Image:
                    character = Image.Parse(reader);
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

            if (character == null)
                throw new InvalidOperationException();

            character.Container = container;
            readCharacters[cur_pos] = character;

            return character;
        }

        public abstract void Write(BinaryWriter writer, BinaryMemoryChain pool);
    }


}
