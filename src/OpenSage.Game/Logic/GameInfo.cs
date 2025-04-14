
#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenSage.Content.Translation;

namespace OpenSage.Logic;

public enum PlayerTemplateIndex
{
    Random = -1,
    Observer = -2,
    Min = Observer
}

/// <summary>
/// Maintains information about the contents of a game slot throughout the game.
/// </summary>
public class GameSlot
{
    public SlotState State { get; private set; }
    public bool IsHuman => State == SlotState.Player;
    public bool IsAI => State == SlotState.EasyAI || State == SlotState.MediumAI || State == SlotState.BrutalAI;
    public bool IsOccupied => IsHuman || IsAI;

    public bool HasMap { get; set; }
    private bool _isAccepted;
    public bool IsAccepted
    {
        get => _isAccepted;
        set
        {
            // Only human players can unaccept
            if (!value && IsHuman)
            {
                _isAccepted = value;
            }
        }
    }
    public bool IsMuted { get; set; }
    public int Color { get; set; }
    public int StartPos { get; set; }
    public int TeamNumber { get; set; }

    private PlayerTemplateIndex _playerTemplate;
    public PlayerTemplateIndex PlayerTemplate
    {
        get => _playerTemplate;
        set
        {
            _playerTemplate = value;
            if (_playerTemplate <= PlayerTemplateIndex.Min)
            {
                StartPos = -1;
            }
        }
    }

    public int OriginalColor { get; private set; }
    public int OriginalStartPos { get; private set; }
    public PlayerTemplateIndex OriginalPlayerTemplate { get; private set; }

    // TODO: These are from Generals, but do they really belong here?
    // Generals has separate sub classes for LAN and GameSpy game slots.
    public string Name { get; set; } = string.Empty;
    public uint IP { get; set; } = 0;
    public ushort Port { get; set; } = 0;
    // TODO(Port): This is FirewallBehaviorType in Generals. Either port it or replace this with something else.
    public int NatBehavior { get; set; } = 1;
    public uint LastFrameInGame { get; set; } = 0;

    private bool _isDisconnected = false;
    public bool IsDisconnected
    {
        get
        {
            if (!IsHuman)
            {
                return false;
            }
            return _isDisconnected;
        }
        set => _isDisconnected = value;
    }

    public virtual void Reset()
    {
        State = SlotState.Closed;
        IsAccepted = false;
        HasMap = true;
        Color = -1;
        StartPos = -1;
        PlayerTemplate = PlayerTemplateIndex.Random;
        TeamNumber = -1;
        NatBehavior = 1;
        LastFrameInGame = 0;
        IsDisconnected = false;
        Port = 0;
        IsMuted = false;
        OriginalPlayerTemplate = PlayerTemplateIndex.Random;
        OriginalStartPos = -1;
        OriginalColor = -1;
        // This doesn't reset the name or the IP address. Bug or feature?
    }

    public void SaveOffOriginalInfo()
    {
        OriginalPlayerTemplate = PlayerTemplate;
        OriginalStartPos = StartPos;
        OriginalColor = Color;
    }

    public bool IsPlayerWithName(string name)
    {
        return IsHuman && name == Name;
    }

    public bool IsPlayerWithIP(uint ip)
    {
        return IsHuman && ip == IP;
    }

    public void SetState(SlotState state, string name, uint ip)
    {
        // C++ has a seemingly completely redundant check here which checks if IsAI is true,
        // and then checks each AI state individually, with code identical to how IsAI is calculated.
        // I've decided to omit that check.

        if (state == SlotState.Player)
        {
            Reset();
            State = state;
            Name = name;
        }
        else
        {
            State = state;
            IsAccepted = true;
            HasMap = true;
            Name = (state switch
            {
                SlotState.Open => "GUI:Open",
                SlotState.EasyAI => "GUI:EasyAI",
                SlotState.MediumAI => "GUI:MediumAI",
                SlotState.BrutalAI => "GUI:HardAI",
                _ => "GUI:Closed",
            }).Translate();
        }
        IP = ip;
    }

    public string GetApparentPlayerTemplateDisplayName(IGame game, GameInfo gameInfo)
    {
        var settings = game.AssetStore.MultiplayerSettings.Current;

        // TODO: I don't exactly understand what this check accomplishes, and why it's copy-pasted to several methods in this class.
        if (settings.ShowRandomPlayerTemplate && OriginalPlayerTemplate == PlayerTemplateIndex.Random && !gameInfo.IsSlotLocalAlly(this))
        {
            return "GUI:Random".Translate();
        }

        if (OriginalPlayerTemplate == PlayerTemplateIndex.Observer)
        {
            return "GUI:Observer".Translate();
        }

        if (PlayerTemplate < 0)
        {
            return "GUI:Random".Translate();
        }

        var playerTemplate = game.AssetStore.PlayerTemplates.GetByIndex((int)OriginalPlayerTemplate);
        return playerTemplate?.DisplayName ?? "Unknown";
    }

    public PlayerTemplateIndex GetApparentPlayerTemplate(IGame game, GameInfo gameInfo)
    {
        var settings = game.AssetStore.MultiplayerSettings.Current;

        // TODO: I don't exactly understand what this check accomplishes, and why it's copy-pasted to several methods in this class.
        if (settings.ShowRandomPlayerTemplate && OriginalPlayerTemplate == PlayerTemplateIndex.Random && !gameInfo.IsSlotLocalAlly(this))
        {
            return OriginalPlayerTemplate;
        }

        return PlayerTemplate;
    }

    public int GetApparentColor(IGame game, GameInfo gameInfo)
    {
        if (OriginalPlayerTemplate == PlayerTemplateIndex.Observer)
        {
            // TODO(Port): In C++ this is initialised with a value from MultiplayerSettings.
            // However, it seems like that value is never initialised?
            // Let's treat this and the other special colors as black for now.
            return 0;
        }

        var settings = game.AssetStore.MultiplayerSettings.Current;

        if (settings.ShowRandomColor && !gameInfo.IsSlotLocalAlly(this))
        {
            return OriginalColor;
        }

        return Color;
    }

    public int GetApparentStartPos(IGame game, GameInfo gameInfo)
    {
        var settings = game.AssetStore.MultiplayerSettings.Current;

        if (settings.ShowRandomStartPos && !gameInfo.IsSlotLocalAlly(this))
        {
            return OriginalStartPos;
        }

        return StartPos;
    }

    public bool IsAlliedTo(GameSlot other)
    {
        // We are allied to ourselves
        if (other == this)
        {
            return true;
        }

        // If the slot is in the same team as the local player, we are allies
        // Team -1 means "no team", so don't count that
        if (TeamNumber == other.TeamNumber && TeamNumber != -1)
        {
            return true;
        }

        // If we're an observer, we are allies with everyone
        if (OriginalPlayerTemplate == PlayerTemplateIndex.Observer)
        {
            return true;
        }

        return false;
    }

    public bool IsEnemyTo(GameSlot other)
    {
        return !IsAlliedTo(other);
    }
}

public enum SlotState
{
    Open,
    Closed,
    EasyAI,
    MediumAI,
    BrutalAI,
    Player
}

public abstract class BaseGameInfo<TSlot> where TSlot : GameSlot
{
    private IGame _game;

    /// <summary>
    /// Highest possible (zero-based) player index.
    /// </summary>
    public const int MaxPlayer = 7;
    public const int MaxSlots = MaxPlayer + 1;

    protected int _preorderMask;
    protected int _crcInterval;
    protected bool _inGame;
    protected bool _inProgress;
    protected bool _surrendered;
    protected int _gameId;
    protected TSlot[] _slots = new TSlot[MaxSlots];
    public IReadOnlyList<TSlot> Slots => _slots;

    protected uint _localIP;

    protected string _mapName = "NOMAP";
    protected int _mapCRC;
    protected int _mapSize;
    protected int _mapMask;
    protected int _seed;
    protected bool _useStats;
    protected BankAccount _startingCash;
    protected ushort _superweaponRestriction;
    /// <summary>
    /// If true, only USA, China and GLA are allowed as playable factions (and the ones from Zero Hour are not).
    /// </summary>
    protected bool _oldFactionsOnly;

    public BaseGameInfo(IGame game)
    {
        _game = game;
        Reset();
    }

    public void Init()
    {
        Reset();
    }

    [MemberNotNull(nameof(_startingCash))]
    public void Reset()
    {
        // TODO(Port): In Generals this is either 1 or 100, depending on build settings.
        _crcInterval = 100;
        _inGame = false;
        _inProgress = false;
        _gameId = 0;
        _mapName = "NOMAP";
        _mapMask = 0;
        _seed = Environment.TickCount;
        _useStats = true;
        _surrendered = false;
        _oldFactionsOnly = false;
        _mapCRC = 0;
        _mapSize = 0;
        _superweaponRestriction = 0;
        // TODO(Port): Is this right? In Generals this is read from TheGlobalData->m_defaultStartingCash.
        // Does that change when the user changes the starting cash in the UI? Or does it use the default value from INI?
        _startingCash = BankAccount.FromAmount(_game, PlayerIndex.Invalid, _game.AssetStore.GameData.Current.DefaultStartingCash);
    }

    public bool IsSlotLocalAlly(GameSlot slot)
    {
        var localIndex = GetLocalSlotNum();
        var localSlot = GetSlotByIndex(localIndex);

        if (localSlot == null)
        {
            return false;
        }

        return localSlot.IsAlliedTo(slot);
    }

    public int GetSlotIndex(GameSlot slot)
    {
        return Array.IndexOf(_slots, slot);
    }

    // Equivalent to getSlot and getConstSlot
    public GameSlot? GetSlotByIndex(int index)
    {
        if (index < 0 || index >= MaxSlots)
        {
            return null;
        }
        return _slots[index];
    }

    public int GetLocalSlotNum()
    {
        if (!_inGame)
        {
            return -1;
        }

        return Array.FindIndex(_slots, (slot) => slot.IsPlayerWithIP(_localIP));
    }

    public GameSlot? LocalSlot
    {
        get
        {
            var localIndex = GetLocalSlotNum();
            if (localIndex == -1)
            {
                return null;
            }
            return GetSlotByIndex(localIndex);
        }
    }

    public bool IsPlayerPreorder(int index)
    {
        // TODO(Port)
        return false;
    }
}

// The C++ base class isn't abstract, but it seems unusable by itself (because it never initialises the slots).
public abstract class GameInfo(IGame game) : BaseGameInfo<GameSlot>(game)
{

}

public sealed class SkirmishGameInfo : GameInfo, IPersistableObject
{
    public SkirmishGameInfo(IGame game) : base(game)
    {
        for (var i = 0; i < MaxSlots; i++)
        {
            _slots[i] = new GameSlot();
        }
    }

    public void Persist(StatePersister persister)
    {
        throw new NotImplementedException();
    }

    public static SkirmishGameInfo FromPlayerSettings(IGame game, PlayerSetting[] players)
    {
        var gameInfo = new SkirmishGameInfo(game);
        gameInfo.Reset();

        // TODO: This is not very efficient, but our assets system doesn't support retrieving the index of an asset by name.
        var playerTemplates = game.AssetStore.PlayerTemplates.ToList();

        for (var i = 0; i < players.Length; i++)
        {
            var player = players[i];
            var slot = gameInfo._slots[i];
            slot.SetState(player.Owner switch
            {
                PlayerOwner.Player => SlotState.Player,
                PlayerOwner.EasyAi => SlotState.EasyAI,
                PlayerOwner.MediumAi => SlotState.MediumAI,
                PlayerOwner.HardAi => SlotState.BrutalAI,
                _ => SlotState.Closed,
            }, player.Name, 0);
            slot.Color = player.Color.ToInt32();
            slot.StartPos = player.StartPosition ?? -1;
            slot.TeamNumber = player.Team;
            var playerTemplateIndex = playerTemplates.FindIndex(x => x.Name == player.SideName);
            slot.PlayerTemplate = playerTemplateIndex == -1 ? PlayerTemplateIndex.Random : (PlayerTemplateIndex)playerTemplateIndex;
        }

        return gameInfo;
    }
}
