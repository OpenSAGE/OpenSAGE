namespace OpenSage.Logic.Object;

public interface IUpgradableScienceModule
{
    void TryUpgrade(Science purchasedScience);
}
