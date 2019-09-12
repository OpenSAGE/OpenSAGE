namespace OpenSage
{
    /// <summary>
    /// Represents an abstract subsystem across different SAGE games.
    /// Can map to multiple LoadSubsystem entries.
    /// </summary>
    public enum Subsystem
    {
        /// <summary>
        /// Things that need to be loaded immediately on engine start, before loading any other assets.
        /// </summary>
        Core,

        /// <summary>
        /// Things that need to be loaded in order to play sounds or music.
        /// </summary>
        Audio,

        /// <summary>
        /// Things that need to be loaded in order to load and spawn game objects.
        /// </summary>
        ObjectCreation,

        /// <summary>
        /// Things that need be loaded in order to spawn players or list them in menus.
        /// </summary>
        Players,

        /// <summary>
        /// Things that need to be loaded in order to load and render terrain and roads.
        /// </summary>
        Terrain,

        /// <summary>
        /// Things that need to be loaded in order to load and render particle systems.
        /// </summary>
        ParticleSystems,

        /// <summary>
        /// Things that need to be loaded in order to load and render Wnd-based GUIs.
        /// </summary>
        Wnd,

        /// <summary>
        /// Things that need to be loaded in order to start multiplayer (possibly including Skirmish).
        /// </summary>
        Multiplayer,

        /// <summary>
        /// Things that need to be loaded in order to play the single-player campaign.
        /// </summary>
        LinearCampaign,

        /// <summary>
        /// Things that need to be loaded in order to display the credits.
        /// </summary>
        Credits,
    }
}
