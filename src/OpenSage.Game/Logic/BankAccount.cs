#nullable enable

using System;

namespace OpenSage.Logic;

// Equivalent to the Money class in Generals
// We can't name this class Money since it would conflict with the Money field
public sealed class BankAccount(IGame game, PlayerIndex playerIndex) : IPersistableObject
{
    public PlayerIndex PlayerIndex { get; set; } = playerIndex;

    public uint Money;

    /// <summary>
    /// Withdraws the specified amount of money from the bank account.
    /// If the amount is greater than the current balance, it withdraws the entire balance.
    /// </summary>
    /// <param name="amountToWithdraw">Returns how much money was actually withdrawn.</param>
    /// <param name="playSound">Should this play a sound?</param>
    /// <returns></returns>
    public uint Withdraw(uint amountToWithdraw, bool playSound = true)
    {
        amountToWithdraw = Math.Min(amountToWithdraw, Money);

        if (amountToWithdraw == 0)
        {
            return 0;
        }

        if (playSound)
        {
            game.Audio.PlayAudioEvent(game.AssetStore.MiscAudio.Current.MoneyWithdrawSound.Value);
            // TODO(Port): Attach player index to the sound event (so that only the owner or replay observer hears it)
        }

        Money -= amountToWithdraw;
        return amountToWithdraw;
    }

    public void Deposit(uint amountToDeposit, bool playSound = true)
    {
        if (amountToDeposit == 0)
        {
            return;
        }

        if (playSound)
        {
            game.Audio.PlayAudioEvent(game.AssetStore.MiscAudio.Current.MoneyDepositSound.Value);
            // TODO(Port): Attach player index to the sound event (so that only the owner or replay observer hears it)
        }

        Money += amountToDeposit;

        if (amountToDeposit > 0)
        {
            game.PlayerList.GetNthPlayer(PlayerIndex)?.AcademyStats.RecordIncome();
        }
    }

    public void Init()
    {
        Money = 0;
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistUInt32(ref Money);
    }

    public static BankAccount FromAmount(IGame game, PlayerIndex playerIndex, uint amount)
    {
        var bankAccount = new BankAccount(game, playerIndex);
        bankAccount.Deposit(amount);
        return bankAccount;
    }
}
