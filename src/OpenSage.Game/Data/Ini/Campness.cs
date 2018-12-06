namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public enum Campness
    {
        [IniEnum("CAMPNESS_DEFAULT")]
        Default,

        [IniEnum("CAMPNESS_FORTRESS")]
        Fortress,

        [IniEnum("CAMPNESS_WALL")]
        Wall,

        [IniEnum("CAMPNESS_DEFENSIVE")]
        Defensive,

        [IniEnum("CAMPNESS_RESOURCE")]
        Resource,

        [IniEnum("CAMPNESS_SUMMONED")]
        Summoned,

        [IniEnum("CAMPNESS_ALWAYS")]
        Always,

        [IniEnum("CAMPNESS_TECH")]
        Tech,

        [IniEnum("CAMPNESS_RESOURCE_BUILDING"), AddedIn(SageGame.Bfme2)]
        ResourceBuilding,

        [IniEnum("CAMPNESS_ALWAYS_CAMP"), AddedIn(SageGame.Bfme2)]
        AlwaysCamp,

        [IniEnum("CAMPNESS_DEFENSIVE_TOWER"), AddedIn(SageGame.Bfme2)]
        DefensiveTower,

        [IniEnum("CAMPNESS_TECH_BUILDING"), AddedIn(SageGame.Bfme2)]
        TechBuilding,
    }
}
