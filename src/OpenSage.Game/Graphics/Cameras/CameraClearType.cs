namespace OpenSage.Graphics.Cameras
{
    /// <summary>
    /// Specifies what should be cleared when a <see cref="CameraComponent"/> is rendered.
    /// </summary>
    public enum CameraClearType
    {
        /// <summary>
        /// Clear the depth buffer and clear the color to <see cref="CameraComponent.BackgroundColor"/>.
        /// </summary>
        DepthAndColor = 0,

        /// <summary>
        /// Clear the depth buffer and draw the skybox specified in <see cref="Settings.SceneSettings.SkyboxMaterial"/>.
        /// </summary>
        DepthAndSkybox = 1
    }
}
