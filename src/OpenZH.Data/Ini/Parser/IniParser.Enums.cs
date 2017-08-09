using System.Collections.Generic;

namespace OpenZH.Data.Ini.Parser
{
    partial class IniParser
    {
        private static readonly Dictionary<string, Key> KeyMap = new Dictionary<string, Key>
        {
            { "KEY_NONE", Key.None },

            { "KEY_0", Key.D0 },
            { "KEY_1", Key.D1 },
            { "KEY_2", Key.D2 },
            { "KEY_3", Key.D3 },
            { "KEY_4", Key.D4 },
            { "KEY_5", Key.D5 },
            { "KEY_6", Key.D6 },
            { "KEY_7", Key.D7 },
            { "KEY_8", Key.D8 },
            { "KEY_9", Key.D9 },

            { "KEY_F1", Key.F1 },
            { "KEY_F2", Key.F2 },
            { "KEY_F3", Key.F3 },
            { "KEY_F4", Key.F4 },
            { "KEY_F5", Key.F5 },
            { "KEY_F6", Key.F6 },
            { "KEY_F7", Key.F7 },
            { "KEY_F8", Key.F8 },
            { "KEY_F9", Key.F9 },
            { "KEY_F10", Key.F10 },
            { "KEY_F11", Key.F11 },
            { "KEY_F12", Key.F12 },

            { "KEY_LEFT", Key.Left },
            { "KEY_RIGHT", Key.Right },
            { "KEY_UP", Key.Up },
            { "KEY_DOWN", Key.Down },

            { "KEY_A", Key.A },
            { "KEY_B", Key.B },
            { "KEY_C", Key.C },
            { "KEY_D", Key.D },
            { "KEY_E", Key.E },
            { "KEY_F", Key.F },
            { "KEY_G", Key.G },
            { "KEY_H", Key.H },
            { "KEY_I", Key.I },
            { "KEY_J", Key.J },
            { "KEY_K", Key.K },
            { "KEY_L", Key.L },
            { "KEY_M", Key.M },
            { "KEY_N", Key.N },
            { "KEY_O", Key.O },
            { "KEY_P", Key.P },
            { "KEY_Q", Key.Q },
            { "KEY_R", Key.R },
            { "KEY_S", Key.S },
            { "KEY_T", Key.T },
            { "KEY_U", Key.U },
            { "KEY_V", Key.V },
            { "KEY_W", Key.W },
            { "KEY_X", Key.X },
            { "KEY_Y", Key.Y },
            { "KEY_Z", Key.Z },

            { "KEY_LBRACKET", Key.LeftBracket },
            { "KEY_RBRACKET", Key.RightBracket },
            { "KEY_COMMA", Key.Comma },
            { "KEY_PERIOD", Key.Period },
            { "KEY_BACKSLASH", Key.Backslash },
            { "KEY_SLASH", Key.Slash },

            { "KEY_SPACE", Key.Space },
            { "KEY_ENTER", Key.Enter },
            { "KEY_TAB", Key.Tab },
            { "KEY_DEL", Key.Delete },
            { "KEY_ESC", Key.Escape },
            { "KEY_BACKSPACE", Key.Backspace },

            { "KEY_MINUS", Key.Minus },
            { "KEY_EQUAL", Key.Equal },

            { "KEY_KP0", Key.NumPad0 },
            { "KEY_KP1", Key.NumPad1 },
            { "KEY_KP2", Key.NumPad2 },
            { "KEY_KP3", Key.NumPad3 },
            { "KEY_KP4", Key.NumPad4 },
            { "KEY_KP5", Key.NumPad5 },
            { "KEY_KP6", Key.NumPad6 },
            { "KEY_KP7", Key.NumPad7 },
            { "KEY_KP8", Key.NumPad8 },
            { "KEY_KP9", Key.NumPad9 },

            { "KEY_KPSLASH", Key.NumPadDivide }
        };

        private static readonly Dictionary<string, KeyTransition> KeyTransitionMap = new Dictionary<string, KeyTransition>
        {
            { "DOWN", KeyTransition.Down },
            { "UP", KeyTransition.Up }
        };

        private static readonly Dictionary<string, KeyModifiers> KeyModifiersMap = new Dictionary<string, KeyModifiers>
        {
            { "NONE", KeyModifiers.None },
            { "ALT", KeyModifiers.Alt },
            { "CTRL", KeyModifiers.Ctrl },
            { "SHIFT", KeyModifiers.Shift },
            { "SHIFT_ALT", KeyModifiers.ShiftAlt },
            { "SHIFT_CTRL", KeyModifiers.ShiftCtrl },
            { "SHIFT_ALT_CTRL", KeyModifiers.ShiftAltCtrl }
        };

        private static readonly Dictionary<string, CommandMapUsabilityFlags> CommandMapUsabilityFlagsMap = new Dictionary<string, CommandMapUsabilityFlags>
        {
            { "NONE", CommandMapUsabilityFlags.None },
            { "GAME", CommandMapUsabilityFlags.Game },
            { "SHELL", CommandMapUsabilityFlags.Shell },
        };

        private static readonly Dictionary<string, CommandMapCategory> CommandMapCategoryMap = new Dictionary<string, CommandMapCategory>
        {
            { "INTERFACE", CommandMapCategory.Interface },
            { "TEAM", CommandMapCategory.Team },
            { "SELECTION", CommandMapCategory.Selection },
            { "CONTROL", CommandMapCategory.Control },
            { "MISC", CommandMapCategory.Misc }
        };

        private static readonly Dictionary<string, AnimationMode> AnimationModeMap = new Dictionary<string, AnimationMode>
        {
            { "ONCE", AnimationMode.Once },
            { "LOOP", AnimationMode.Loop },
            { "PING_PONG", AnimationMode.PingPong }
        };

        private static readonly Dictionary<string, DamageType> DamageTypeMap = new Dictionary<string, DamageType>
        {
            { "DEFAULT", DamageType.Default },

            { "EXPLOSION", DamageType.Explosion },
            { "CRUSH", DamageType.Crush },
            { "ARMOR_PIERCING", DamageType.ArmorPiercing },
            { "SMALL_ARMS", DamageType.SmallArms },
            { "GATTLING", DamageType.Gattling },
            { "RADIATION", DamageType.Radiation },
            { "FLAME", DamageType.Flame },
            { "LASER", DamageType.Laser },
            { "SNIPER", DamageType.Sniper },
            { "POISON", DamageType.Poison },
            { "HEALING", DamageType.Healing },
            { "UNRESISTABLE", DamageType.Unresistable },
            { "WATER", DamageType.Water },
            { "DEPLOY", DamageType.Deploy },
            { "SURRENDER", DamageType.Surrender },
            { "HACK", DamageType.Hack },
            { "KILL_PILOT", DamageType.KillPilot },
            { "PENALTY", DamageType.Penalty },
            { "FALLING", DamageType.Falling },
            { "MELEE", DamageType.Melee },
            { "DISARM", DamageType.Disarm },
            { "HAZARD_CLEANUP", DamageType.HazardCleanup },
            { "INFANTRY_MISSILE", DamageType.InfantryMissile },
            { "AURORA_BOMB", DamageType.AuroraBomb },
            { "LAND_MINE", DamageType.LandMine },
            { "JET_MISSILES", DamageType.JetMissiles },
            { "STEALTHJET_MISSILES", DamageType.StealthjetMissiles },
            { "MOLOTOV_COCKTAIL", DamageType.MolotovCocktail },
            { "COMANCHE_VULCAN", DamageType.ComancheVulcan },
            { "FLESHY_SNIPER", DamageType.FleshySniper },
            { "PARTICLE_BEAM", DamageType.ParticleBeam }
        };

        private static readonly Dictionary<string, CommandType> CommandTypeMap = new Dictionary<string, CommandType>
        {
            { "PLACE_BEACON", CommandType.PlaceBeacon },
            { "SPECIAL_POWER", CommandType.SpecialPower },
            { "SPECIAL_POWER_FROM_COMMAND_CENTER", CommandType.SpecialPowerFromCommandCenter },
            { "OBJECT_UPGRADE", CommandType.ObjectUpgrade },
            { "PLAYER_UPGRADE", CommandType.PlayerUpgrade },
            { "EXIT_CONTAINER", CommandType.ExitContainer },
            { "EVACUATE", CommandType.Evacuate },
            { "EXECUTE_RAILED_TRANSPORT", CommandType.ExecuteRailedTransport },
            { "COMBATDROP", CommandType.CombatDrop },
            { "GUARD", CommandType.Guard },
            { "GUARD_WITHOUT_PURSUIT", CommandType.GuardWithoutPursuit },
            { "GUARD_FLYING_UNITS_ONLY", CommandType.GuardFlyingUnitsOnly },
            { "ATTACK_MOVE", CommandType.AttackMove },
            { "STOP", CommandType.Stop },
            { "FIRE_WEAPON", CommandType.FireWeapon },
            { "SWITCH_WEAPON", CommandType.SwitchWeapon},
            { "DOZER_CONSTRUCT_CANCEL", CommandType.DozerConstructCancel },
            { "DOZER_CONSTRUCT", CommandType.DozerConstruct },
            { "CANCEL_UNIT_BUILD", CommandType.CancelUnitBuild },
            { "UNIT_BUILD", CommandType.UnitBuild },
            { "PURCHASE_SCIENCE", CommandType.PurchaseScience },
            { "TOGGLE_OVERCHARGE", CommandType.ToggleOvercharge },
            { "SET_RALLY_POINT", CommandType.SetRallyPoint },
            { "SELL", CommandType.Sell },
            { "CANCEL_UPGRADE", CommandType.CancelUpgrade },
            { "CONVERT_TO_CARBOMB", CommandType.ConvertToCarBomb },
            { "HIJACK_VEHICLE", CommandType.HijackVehicle },
            { "HACK_INTERNET", CommandType.HackInternet }
        };

        private static readonly Dictionary<string, CommandButtonOptions> CommandButtonOptionsMap = new Dictionary<string, CommandButtonOptions>
        {
            { "OK_FOR_MULTI_SELECT", CommandButtonOptions.OkForMultiSelect },
            { "CHECK_LIKE", CommandButtonOptions.CheckLike },
            { "NEED_TARGET_ENEMY_OBJECT", CommandButtonOptions.NeedTargetEnemyObject },
            { "NEED_TARGET_NEUTRAL_OBJECT", CommandButtonOptions.NeedTargetNeutralObject },
            { "NEED_TARGET_ALLY_OBJECT", CommandButtonOptions.NeedTargetAllyObject },
            { "CONTEXTMODE_COMMAND", CommandButtonOptions.ContextModeCommand },
            { "OPTION_ONE", CommandButtonOptions.OptionOne },
            { "OPTION_TWO", CommandButtonOptions.OptionTwo },
            { "OPTION_THREE", CommandButtonOptions.OptionThree },
            { "NEED_TARGET_POS", CommandButtonOptions.NeedTargetPos },
            { "NOT_QUEUEABLE", CommandButtonOptions.NotQueueable },
            { "IGNORES_UNDERPOWERED", CommandButtonOptions.IgnoresUnderpowered },
            { "NEED_SPECIAL_POWER_SCIENCE", CommandButtonOptions.NeedSpecialPowerScience },
            { "SCRIPT_ONLY", CommandButtonOptions.ScriptOnly },
            { "NEED_UPGRADE", CommandButtonOptions.NeedUpgrade },
            { "USES_MINE_CLEARING_WEAPONSET", CommandButtonOptions.UsesMineClearingWeaponSet }
        };

        private static readonly Dictionary<string, CommandButtonBorderType> CommandButtonBorderTypeMap = new Dictionary<string, CommandButtonBorderType>
        {
            { "NONE", CommandButtonBorderType.None },
            { "ACTION", CommandButtonBorderType.Action },
            { "BUILD", CommandButtonBorderType.Build },
            { "UPGRADE", CommandButtonBorderType.Upgrade },
            { "SYSTEM", CommandButtonBorderType.System }
        };

        private static readonly Dictionary<string, CommandButtonRadiusCursorType> CommandButtonRadiusCursorTypeMap = new Dictionary<string, CommandButtonRadiusCursorType>
        {
            { "DAISYCUTTER", CommandButtonRadiusCursorType.DaisyCutter },
            { "NAPALMSTRIKE", CommandButtonRadiusCursorType.NapalmStrike },
            { "PARADROP", CommandButtonRadiusCursorType.Paradrop },
            { "CLUSTERMINES", CommandButtonRadiusCursorType.ClusterMines },
            { "EMPPULSE", CommandButtonRadiusCursorType.EmpPulse },
            { "A10STRIKE", CommandButtonRadiusCursorType.A10Strike },
            { "CARPETBOMB", CommandButtonRadiusCursorType.CarpetBomb },
            { "NUCLEARMISSILE", CommandButtonRadiusCursorType.NuclearMissile },
            { "SCUDSTORM", CommandButtonRadiusCursorType.ScudStorm },
            { "ARTILLERYBARRAGE", CommandButtonRadiusCursorType.ArtilleryBarrage },
            { "SPYSATELLITE", CommandButtonRadiusCursorType.SpySatellite },
            { "SPYDRONE", CommandButtonRadiusCursorType.SpyDrone },
            { "RADAR", CommandButtonRadiusCursorType.Radar },
            { "AMBUSH", CommandButtonRadiusCursorType.Ambush },
            { "EMERGENCY_REPAIR", CommandButtonRadiusCursorType.EmergencyRepair },
            { "ANTHRAXBOMB", CommandButtonRadiusCursorType.AnthraxBomb },
            { "GUARD_AREA", CommandButtonRadiusCursorType.GuardArea },
            { "ATTACK_CONTINUE_AREA", CommandButtonRadiusCursorType.AttackContinueArea },
            { "ATTACK_SCATTER_AREA", CommandButtonRadiusCursorType.AttackScatterArea },
            { "FRIENDLY_SPECIALPOWER", CommandButtonRadiusCursorType.FriendlySpecialPower }
        };

        private static readonly Dictionary<string, WeaponSlot> WeaponSlotMap = new Dictionary<string, WeaponSlot>
        {
            { "PRIMARY", WeaponSlot.Primary },
            { "SECONDARY", WeaponSlot.Secondary },
            { "TERTIARY", WeaponSlot.Tertiary }
        };

        public Key ParseKey() => ParseEnum(KeyMap);
        public KeyTransition ParseKeyTransition() => ParseEnum(KeyTransitionMap);
        public KeyModifiers ParseKeyModifiers() => ParseEnum(KeyModifiersMap);
        public CommandMapUsabilityFlags ParseCommandMapUsabilityFlags() => ParseEnumFlags(CommandMapUsabilityFlags.None, CommandMapUsabilityFlagsMap);
        public CommandMapCategory ParseCommandMapCategory() => ParseEnum(CommandMapCategoryMap);

        public AnimationMode ParseAnimationMode() => ParseEnum(AnimationModeMap);

        public DamageType ParseDamageType() => ParseEnum(DamageTypeMap);

        public CommandType ParseCommandType() => ParseEnum(CommandTypeMap);

        public CommandButtonOptions ParseCommandButtonOptions() => ParseEnumFlags(CommandButtonOptions.None, CommandButtonOptionsMap);

        public CommandButtonBorderType ParseCommandButtonBorderType() => ParseEnum(CommandButtonBorderTypeMap);

        public CommandButtonRadiusCursorType ParseCommandButtonRadiusCursorType() => ParseEnum(CommandButtonRadiusCursorTypeMap);

        public WeaponSlot ParseWeaponSlot() => ParseEnum(WeaponSlotMap);
    }
}
