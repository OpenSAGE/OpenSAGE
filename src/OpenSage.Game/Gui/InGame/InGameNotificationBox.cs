using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Gui.InGame
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class InGameNotificationBox
    {
        internal static InGameNotificationBox Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<InGameNotificationBox> FieldParseTable = new IniParseTable<InGameNotificationBox>
        {
            { "Font", (parser, x) => x.Font = IniFont.Parse(parser) },
            //was renamed in ROTWK
            { "DefaultMessageFont", (parser, x) => x.Font = IniFont.Parse(parser) },

            { "DefaultMessageColor", (parser, x) => x.DefaultMessageColor = parser.ParseColorRgb() },
            { "DefaultOpenAudio", (parser, x) => x.DefaultOpenAudio = parser.ParseAssetReference() },
            { "NotificationType", (parser, x) => x.NotificationTypes.Add(NotificationType.Parse(parser)) }
        };

        public IniFont Font { get; private set; }
        public ColorRgb DefaultMessageColor { get; private set; }
        public string DefaultOpenAudio { get; private set; }
        public List<NotificationType> NotificationTypes { get; } = new List<NotificationType>();
    }

    public class NotificationType
    {
        internal static NotificationType Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<NotificationType> FieldParseTable = new IniParseTable<NotificationType>
        {
            { "Title", (parser, x) => x.Title = parser.ParseLocalizedStringKey() },
            { "Icon", (parser, x) => x.Icon = parser.ParseAssetReference() }
        };

        public string Name { get; private set; }

        public string Title { get; private set; }
        public string Icon { get; private set; }
    }

    public class IniFont
    {
        internal static IniFont Parse(IniParser parser)
        {
            return new IniFont
            {
                FontName = parser.ParseQuotedString(),
                Size = parser.ParseInteger(),
                Bold = parser.ParseBoolean()
            };
        }

        public string FontName { get; private set; }
        public int Size { get; private set; }
        public bool Bold { get; private set; }
    }
}
