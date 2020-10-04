using System;

namespace OpenSage.Scripting
{
    partial class ScriptConditions
    {
        [ScriptCondition(ScriptConditionType.SkirmishPlayerFaction, "Skirmish/Player/Player is faction", "Player {0} is Faction Name: {1}")]
        public static bool SkirmishPlayerFaction(ScriptExecutionContext context, [ScriptArgumentType(ScriptArgumentType.PlayerName)] string playerName, [ScriptArgumentType(ScriptArgumentType.FactionName)] string factionName)
        {
            if (playerName.Equals(ScriptArgumentPlaceholders.LocalPlayer, StringComparison.OrdinalIgnoreCase))
            {
                return context.Scene.LocalPlayer.Side.Equals(factionName, StringComparison.OrdinalIgnoreCase);
            }

            foreach (var player in context.Scene.Players)
            {
                if (player.Name.Equals(playerName, StringComparison.Ordinal))
                {
                    return player.Side.Equals(factionName, StringComparison.Ordinal);
                }
            }

            return false;
        }
    }
}
