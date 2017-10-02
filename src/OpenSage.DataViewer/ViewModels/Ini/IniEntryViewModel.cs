namespace OpenSage.DataViewer.ViewModels.Ini
{
    public abstract class IniEntryViewModel : FileSubObjectViewModel
    {
        public override void Deactivate()
        {
            Game.Scene = null;

            Game.ContentManager.Unload();
        }
    }
}
