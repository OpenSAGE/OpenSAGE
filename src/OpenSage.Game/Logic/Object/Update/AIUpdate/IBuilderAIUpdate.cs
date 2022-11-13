namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Should be implemented by any class which inherits <see cref="AIUpdate"/> and is used by a <see cref="ObjectKinds.Dozer"/> unit.
    /// </summary>
    /// <remarks>
    /// This interface is implemented by <see cref="DozerAIUpdate"/> and <see cref="WorkerAIUpdate"/>, since the GLA
    /// worker is also a supply unit.
    /// </remarks>
    internal interface IBuilderAIUpdate
    {
        void SetBuildTarget(GameObject gameObject);
    }
}
