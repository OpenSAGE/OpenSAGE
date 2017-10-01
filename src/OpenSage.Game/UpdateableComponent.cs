namespace OpenSage
{
    public abstract class UpdateableComponent : EntityComponent
    {
        protected internal abstract void Update(GameTime gameTime);
    }
}
