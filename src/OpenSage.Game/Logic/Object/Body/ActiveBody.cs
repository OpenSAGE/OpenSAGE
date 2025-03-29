#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;
using OpenSage.Terrain;
using OpenSage.Utilities;

namespace OpenSage.Logic.Object;

public class ActiveBody : BodyModule
{
    private const float YellowDamagePercent = 0.25f;

    private readonly ActiveBodyModuleData _moduleData;
    private readonly List<uint> _particleSystemIds = [];

    private float _currentHealth;
    private float _currentSubdualDamage;
    private float _previousHealth;
    private float _maxHealth;
    private float _initialHealth;
    private BodyDamageType _currentDamageState;
    private LogicFrame _nextDamageFXFrame;
    private DamageType? _lastDamageFXDone;
    private DamageInfo _lastDamageInfo;
    private LogicFrame? _lastDamageFrame;
    private LogicFrame? _lastHealingFrame;
    private bool _frontCrushed;
    private bool _backCrushed;
    private bool _lastDamageCleared;
    private bool _indestructible;

    private BitArray<ArmorSetCondition> _currentArmorSetFlags = new();
    private ArmorTemplateSet? _currentArmorSet;
    private Armor _currentArmor = Armor.NoArmor;
    private DamageFX? _currentDamageFX;

    internal ActiveBody(GameObject gameObject, IGameEngine gameEngine, ActiveBodyModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;

        _currentDamageState = BodyDamageType.Pristine;

        _currentHealth = _previousHealth = _initialHealth = moduleData.InitialHealth;
        _maxHealth = moduleData.MaxHealth;

        // Force an initially-valid armor setup.
        ValidateArmorAndDamageFX();

        // Start us in the right state.
        SetCorrectDamageState();
    }

    public override DamageInfoOutput AttemptDamage(in DamageInfoInput damageInput)
    {
        ValidateArmorAndDamageFX();

        var damageOutput = new DamageInfoOutput();

        if (_indestructible)
        {
            return damageOutput;
        }

        // We cannot damage again objects that are already dead.
        var obj = GameObject;
        if (obj.IsEffectivelyDead)
        {
            return damageOutput;
        }

        var damager = GameEngine.GameLogic.GetObjectById(damageInput.SourceID);

        var alreadyHandled = false;
        var allowModifier = true;

        var amount = _currentArmor.AdjustDamage(damageInput.DamageType, damageInput.Amount);

        switch (damageInput.DamageType)
        {
            case DamageType.Healing:
                {
                    if (!damageInput.Kill)
                    {
                        // Healing and Damage are separate, so this shouldn't happen.
                        damageOutput = AttemptHealing(damageInput);
                    }
                    return damageOutput;
                }

            case DamageType.KillPilot:
                {
                    // This type of damage doesn't actually damage the unit, but it
                    // does kill its pilot, in the case of a vehicle.
                    if (obj.IsKindOf(ObjectKinds.Vehicle))
                    {
                        // Handle special case for combat bike. We actually kill
                        // the bike by forcing the rider to leave the bike. That
                        // way the bike will automatically scuttle and be unusable.
                        var contain = obj.Contain;
                        if (contain != null && contain.IsRiderChangeContain)
                        {
                            var ai = obj.AIUpdate;

                            if (ai.IsMoving)
                            {
                                // Bike is moving, so just blow it up instead.
                                damager?.ScoreTheKill(obj);
                                obj.Kill();
                            }
                            else
                            {
                                // Removing the rider will scuttle the bike.
                                var rider = contain.ContainedItems[0];
                                ai.AIEvacuateInstantly(true, CommandSourceTypes.FromAI);

                                // Kill the rider.
                                damager?.ScoreTheKill(rider);
                                rider.Kill();
                            }
                        }
                        else
                        {
                            // Make it unmanned, so units can easily check the
                            // ability to "take control of it".
                            obj.SetDisabled(DisabledType.Unmanned);
                            GameEngine.GameLogic.DeselectObject(obj, PlayerMaskType.All, true);

                            obj.AIUpdate?.AIIdle(CommandSourceTypes.FromAI);

                            // Convert it to the neutral team so it renders gray
                            // giving visual representation that it is unmanned.
                            obj.Team = GameEngine.Game.PlayerManager.NeutralPlayer.DefaultTeam;
                        }

                        // We don't care which team sniped the vehicle... we use
                        // this information to flag whether or not we captured a
                        // vehicle.
                        GameEngine.Game.PlayerManager.NeutralPlayer.AcademyStats.RecordVehicleSniped();
                    }

                    alreadyHandled = true;
                    allowModifier = false;
                    break;
                }

            case DamageType.KillGarrisoned:
                {
                    // Original comment (which suggests that this code and the code
                    // mentioned in DumbProjectileBehavior) could be refactored:
                    //
                    // This code is very misleading (but in a good way). One would think this is
                    // an excellent place to add the hook to kill garrisoned troops. And that is
                    // a correct assumption. Unfortunately, the vast majority of garrison slayings
                    // are performed in DumbProjectileBehavior::projectileHandleCollision(), so my
                    // hope is that this message will save you some research time!

                    var killsToMake = MathUtility.FloorToInt(damageInput.Amount);
                    var contain = obj.Contain;
                    if (contain != null
                        && contain.ContainCount > 0
                        && contain.IsGarrisonable
                        && !contain.IsImmuneToClearBuildingAttacks)
                    {
                        var numKilled = 0;

                        // Garrisonable buildings subvert the normal process here.
                        foreach (var thingToKill in contain.ContainedItems)
                        {
                            if (numKilled >= killsToMake)
                            {
                                break;
                            }

                            if (!thingToKill.IsEffectivelyDead)
                            {
                                damager?.ScoreTheKill(thingToKill);
                                thingToKill.Kill();
                                numKilled++;
                                thingToKill.Owner.AcademyStats.RecordClearedGarrisonedBuilding();
                            }
                        }
                    }

                    alreadyHandled = true;
                    allowModifier = false;
                    break;
                }

            case DamageType.Status:
                {
                    // Damage amount is millisecond time we set the status given in
                    // DamageStatusType.
                    var framesToStatusFor = LogicFrameSpan.FromMilliseconds(amount, GameEngine.SageGame);
                    obj.DoStatusDamage(damageInput.DamageStatusType, framesToStatusFor);
                    alreadyHandled = true;
                    allowModifier = false;
                    break;
                }
        }

        if (damageInput.DamageType.IsSubdualDamage())
        {
            if (!CanBeSubdued)
            {
                return damageOutput;
            }

            var wasSubdued = IsSubdued;
            InternalAddSubdualDamage(amount);
            var nowSubdued = IsSubdued;

            alreadyHandled = true;
            allowModifier = false;

            if (wasSubdued != nowSubdued)
            {
                OnSubdualChange(nowSubdued);
            }

            obj.NotifySubdualDamage(amount);
        }

        if (allowModifier && damageInput.DamageType != DamageType.Unresistable)
        {
            // Apply the damage scalar (extra bonuses, like strategy center
            // defensive battle plan). And remember not to adjust unresistable
            // damage, just like the armor code can't.
            amount *= DamageScalar;
        }

        // Sanity check the damage value. We can't apply negative damage.
        if (amount > 0.0f || damageInput.Kill)
        {
            var oldState = _currentDamageState;

            // If the object is going to die, make sure we damage all remaining health.
            if (damageInput.Kill)
            {
                amount = _currentHealth;
            }

            if (!alreadyHandled)
            {
                // Do the damage simplistic damage subtraction.
                InternalChangeHealth(-amount);
            }

            // Record the actual damage done from this, and when it happened.
            damageOutput.ActualDamageDealt = amount;
            damageOutput.ActualDamageClipped = _previousHealth - _currentHealth;

            // Then store the whole DamageInfo for easy lookup.
            var currentDamageInfo = new DamageInfo
            {
                Request = damageInput,
                Result = damageOutput,
            };
            if (_lastDamageFrame < GameEngine.GameLogic.CurrentFrame - 1)
            {
                SetLastDamageInfo(currentDamageInfo);
            }
            else
            {
                // Multiple damages applied in the last two frames. We prefer
                // the one that tells us who the attacker is.
                var srcObj1 = GameEngine.GameLogic.GetObjectById(_lastDamageInfo.Request.SourceID);
                var srcObj2 = GameEngine.GameLogic.GetObjectById(damageInput.SourceID);
                if (srcObj2 != null)
                {
                    if (srcObj1 != null)
                    {
                        if (srcObj2.IsKindOf(ObjectKinds.Vehicle)
                            || srcObj2.IsKindOf(ObjectKinds.Infantry)
                            || srcObj2.IsFactionStructure)
                        {
                            SetLastDamageInfo(currentDamageInfo);
                        }
                    }
                    else
                    {
                        SetLastDamageInfo(currentDamageInfo);
                    }
                }
            }

            // Notify the player that they have been attacked by this player.
            if (_lastDamageInfo.Request.SourceID.IsValid)
            {
                var srcObj = GameEngine.GameLogic.GetObjectById(_lastDamageInfo.Request.SourceID);
                if (srcObj != null)
                {
                    var srcPlayer = srcObj.Owner;
                    obj.Owner.SetAttackedBy(srcPlayer.Id);
                }
            }

            // If our health has gone down, then run the damage module callback.
            if (_currentHealth < _previousHealth)
            {
                foreach (var module in obj.FindBehaviors<IDamageModule>())
                {
                    module.OnDamage(currentDamageInfo);
                }
            }

            if (_currentDamageState != oldState)
            {
                foreach (var module in obj.FindBehaviors<IDamageModule>())
                {
                    module.OnBodyDamageStateChange(
                        currentDamageInfo,
                        oldState,
                        _currentDamageState);
                }

                // Original comment:
                // @todo: This really feels like it should be in the TransitionFX lists.
                switch (_currentDamageState)
                {
                    case BodyDamageType.Damaged:
                        GameEngine.AudioSystem.PlayAudioEvent(
                            obj,
                            obj.Definition.SoundOnDamaged?.Value);
                        break;

                    case BodyDamageType.ReallyDamaged:
                        GameEngine.AudioSystem.PlayAudioEvent(
                            obj,
                            obj.Definition.SoundOnReallyDamaged?.Value);
                        break;
                }
            }

            // Should we play our fear sound?
            if ((_previousHealth / _maxHealth) > YellowDamagePercent
                && (_currentHealth / _maxHealth) < YellowDamagePercent
                && _currentHealth > 0)
            {
                // 25% chance to play.
                if (GameEngine.GameLogic.Random.Next(0, 99) < 25)
                {
                    GameEngine.AudioSystem.PlayAudioEvent(
                        obj.Translation,
                        // TODO(Port): Set PlayerIndex on sound.
                        // fearSound.setPlayerIndex( obj->getControllingPlayer()->getPlayerIndex() );
                        obj.Definition.VoiceFear?.Value);
                }
            }

            // Check to see if we died.
            if (_currentHealth <= 0 && _previousHealth > 0)
            {
                // Give our killer credit for killing us, if there is one.
                damager?.ScoreTheKill(obj);
                obj.OnDie(damageInput);
            }
        }

        DoDamageFX(damageInput, damageOutput);

        // Damaged repulsable civilians scare (repulse) other civilians.
        if (GameEngine.AssetStore.AIData.Current.EnableRepulsors
            && obj.IsKindOf(ObjectKinds.CanBeRepulsed))
        {
            obj.SetObjectStatus(ObjectStatus.Repulsor, true);
        }

        // Retaliate, even if I'm dead. We'll still get my nearby friends to
        // get revenge! Also only retaliate if we're controlled by a human
        // player and the thing that attacked me is an enemy.
        var controllingPlayer = obj.Owner;
        if (controllingPlayer != null
            && controllingPlayer.IsLogicalRetaliationModeEnabled
            && controllingPlayer.IsHuman
            && ShouldRetaliateAgainstAggressor(obj, damager))
        {
            // TODO(Port): Do retaliation. Requires some partition filter
            // and AI stuff that we don't have yet.
        }

        return damageOutput;
    }

    public override DamageInfoOutput AttemptHealing(in DamageInfoInput damageInput)
    {
        ValidateArmorAndDamageFX();

        if (damageInput.DamageType != DamageType.Healing)
        {
            // Healing and Damage are separate, so this shouldn't happen.
            return AttemptDamage(damageInput);
        }

        var obj = GameObject;

        var damageOutput = new DamageInfoOutput();

        // Sorry, once yer dead, yer dead.
        // Special case for bridges, cause the system now thinks they're dead.
        // Original comment:
        // @todo we need to figure out what has changed so we don't have to hack this
        if (!obj.IsKindOf(ObjectKinds.Bridge)
            && !obj.IsKindOf(ObjectKinds.BridgeTower)
            && obj.IsEffectivelyDead)
        {
            return damageOutput;
        }

        var amount = _currentArmor.AdjustDamage(
            damageInput.DamageType,
            damageInput.Amount);

        // Sanity check the damage value. Can't apply negative healing.
        if (amount > 0.0f)
        {
            var oldState = _currentDamageState;

            // Do the damage simplistic damage ADDITION.
            InternalChangeHealth(amount);

            // Record the actual damage done from this, and when it happened.
            damageOutput.ActualDamageDealt = amount;
            damageOutput.ActualDamageClipped = _previousHealth - _currentHealth;

            // Then copy the whole DamageInfo struct for easy lookup.
            var currentDamageInfo = new DamageInfo
            {
                Request = damageInput,
                Result = damageOutput
            };
            SetLastDamageInfo(currentDamageInfo);
            _lastHealingFrame = GameEngine.GameLogic.CurrentFrame;

            // If our health has gone UP, then run the damage module callback.
            if (_currentHealth > _previousHealth)
            {
                foreach (var module in obj.FindBehaviors<IDamageModule>())
                {
                    module.OnHealing(currentDamageInfo);
                }
            }

            if (_currentDamageState != oldState)
            {
                foreach (var module in obj.FindBehaviors<IDamageModule>())
                {
                    module.OnBodyDamageStateChange(
                        currentDamageInfo,
                        oldState,
                        _currentDamageState);
                }
            }
        }

        DoDamageFX(damageInput, damageOutput);

        return damageOutput;
    }

    public override float EstimateDamage(in DamageInfoInput damageInput)
    {
        ValidateArmorAndDamageFX();

        // Subdual damage can't affect you if you can't be subdued.
        if (damageInput.DamageType.IsSubdualDamage() && !CanBeSubdued)
        {
            return 0.0f;
        }

        switch (damageInput.DamageType)
        {
            case DamageType.KillGarrisoned:
                var contain = GameObject.Contain;

                var canKillGarrisoned = contain != null
                    && contain.ContainCount > 0
                    && contain.IsGarrisonable
                    && !contain.IsImmuneToClearBuildingAttacks;

                return canKillGarrisoned
                    ? 1.0f
                    : 0.0f;

            case DamageType.Sniper:
                if (GameObject.IsKindOf(ObjectKinds.Structure)
                    && GameObject.TestStatus(ObjectStatus.UnderConstruction))
                {
                    // If we're a pathfinder shooting a stinger site under
                    // construction... don't. Special case code.
                    return 0.0f;
                }
                break;
        }

        return _currentArmor.AdjustDamage(
            damageInput.DamageType,
            damageInput.Amount);
    }

    public override float Health => _currentHealth;

    public override float MaxHealth => _maxHealth;

    /// <summary>
    /// Simple setting of the health value. It does _not_ track any transition
    /// states for the event of "damage" or the event of "death".
    /// </summary>
    public override void SetMaxHealth(float maxHealth, MaxHealthChangeType healthChangeType)
    {
        var prevMaxHealth = _maxHealth;
        _maxHealth = maxHealth;
        _initialHealth = maxHealth;

        switch (healthChangeType)
        {
            case MaxHealthChangeType.PreserveRatio:
                // 400/500 (80%) + 100 becomes 480/600 (80%)
                // 200/500 (40%) - 100 becomes 160/400 (40%)
                var ratio = _currentHealth / prevMaxHealth;
                var newHealth = maxHealth * ratio;
                InternalChangeHealth(newHealth - _currentHealth);
                break;

            case MaxHealthChangeType.AddCurrentHealthToo:
                // Add the same amount that we are adding to the max health.
                // This could kill you if max health is reduced
                // (if we ever have that ability to add buffer health like in D&D)
                // 400/500 (80%) + 100 becomes 500/600 (83%)
                // 200/500 (40%) - 100 becomes 100/400 (25%)
                InternalChangeHealth(maxHealth - prevMaxHealth);
                break;

            case MaxHealthChangeType.SameCurrentHealth:
                // Do nothing
                break;

            case MaxHealthChangeType.FullyHeal:
                // Set current to the new Max.
                // 400/500 (80%) + 100 becomes 600/600 (100%)
                // 200/500 (40%) - 100 becomes 400/400 (100%)
                InternalChangeHealth(_maxHealth - _currentHealth);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(healthChangeType));
        }

        // When max health is getting clipped to a lower value, if our current health
        // value is now outside of the max health range we will set it back down to the
        // new cap. Note that we are _not_ going through any healing or damage methods here
        // and are doing a direct set.
        if (_currentHealth > maxHealth)
        {
            InternalChangeHealth(maxHealth - _currentHealth);
        }
    }

    public override float InitialHealth => _initialHealth;

    /// <summary>
    /// Simple setting of the initial health value. It does _not_ track any transition
    /// states for the event of "damage" or the event of "death".
    /// </summary>
    public override void SetInitialHealth(int initialPercent)
    {
        // Save the current health as the previous health.
        _previousHealth = _currentHealth;

        var factor = initialPercent / 100.0f;
        var newHealth = factor * _initialHealth;

        // Change the health to the requested percentage.
        InternalChangeHealth(newHealth - _currentHealth);
    }

    public override float PreviousHealth => _previousHealth;

    public override LogicFrameSpan SubdualDamageHealRate => _moduleData.SubdualDamageHealRate;

    public override bool HasAnySubdualDamage => _currentSubdualDamage > 0;

    public override float CurrentSubdualDamageAmount => _currentSubdualDamage;

    public override BodyDamageType DamageState
    {
        get => _currentDamageState;
        set
        {
            var ratio = value switch
            {
                BodyDamageType.Pristine => 1.0f,
                BodyDamageType.Damaged => GameEngine.AssetStore.GameData.Current.UnitDamagedThreshold,
                BodyDamageType.ReallyDamaged => GameEngine.AssetStore.GameData.Current.UnitReallyDamagedThreshold,
                BodyDamageType.Rubble => 0.0f,
                _ => throw new ArgumentOutOfRangeException(nameof(value))
            };

            var desiredHealth = Math.Max(
                _maxHealth * ratio - 1, // -1 because < not <= in CalculateDamageState
                0.0f);

            InternalChangeHealth(desiredHealth - _currentHealth);
            SetCorrectDamageState();
        }
    }

    public override void SetAflame(bool setting)
    {
        // All this does now is act like a major body state change. It is called
        // after Aflame has been set or cleared as an object status.
        UpdateBodyParticleSystems();
    }

    public override void OnVeterancyLevelChanged(VeterancyLevel oldLevel, VeterancyLevel newLevel, bool provideFeedback = false)
    {
        if (oldLevel == newLevel)
        {
            return;
        }

        if (oldLevel < newLevel)
        {
            if (provideFeedback)
            {
                var veterancyChanged = newLevel switch
                {
                    VeterancyLevel.Veteran => GameObject.Definition.SoundPromotedVeteran?.Value,
                    VeterancyLevel.Elite => GameObject.Definition.SoundPromotedElite?.Value,
                    VeterancyLevel.Heroic => GameObject.Definition.SoundPromotedHero?.Value,
                    _ => throw new ArgumentOutOfRangeException(nameof(newLevel))
                };
                GameEngine.AudioSystem.PlayAudioEvent(GameObject, veterancyChanged);
            }

            // Also mark the UI dirty, in case the object is selected or contained.
            // TODO(Port): Implement this.
            //    var obj = GameObject;
            //    var draw = TheInGameUI->getFirstSelectedDrawable();
            //    if (draw != null)
            //    {
            //        var checkOwner = draw.GameObject;
            //        if (checkOwner == obj)
            //        {
            //            // Our selected object has been promoted!
            //            TheControlBar->markUIDirty();
            //        }
            //        else
            //        {
            //            var containedBy = obj.ContainedBy;
            //            if (containedBy && TheInGameUI->getSelectCount() == 1)
            //            {
            //                var checkOwner = draw.GameObject;
            //                if (checkOwner == containedBy)
            //                {
            //                    //But only if the contained by object is containing me!
            //                    TheControlBar->markUIDirty();
            //                }
            //            }
            //        }
            //    }
        }

        var oldBonus = GameEngine.AssetStore.GameData.Current.HealthBonus[(int)oldLevel];
        var newBonus = GameEngine.AssetStore.GameData.Current.HealthBonus[(int)newLevel];
        var mult = newBonus / oldBonus;

        // change the max
        SetMaxHealth(_maxHealth * mult, MaxHealthChangeType.PreserveRatio);

        switch (newLevel)
        {
            case VeterancyLevel.Regular:
                ClearArmorSetFlag(ArmorSetCondition.Veteran);
                ClearArmorSetFlag(ArmorSetCondition.Elite);
                ClearArmorSetFlag(ArmorSetCondition.Hero);
                break;

            case VeterancyLevel.Veteran:
                SetArmorSetFlag(ArmorSetCondition.Veteran);
                ClearArmorSetFlag(ArmorSetCondition.Elite);
                ClearArmorSetFlag(ArmorSetCondition.Hero);
                break;

            case VeterancyLevel.Elite:
                ClearArmorSetFlag(ArmorSetCondition.Veteran);
                SetArmorSetFlag(ArmorSetCondition.Elite);
                ClearArmorSetFlag(ArmorSetCondition.Hero);
                break;

            case VeterancyLevel.Heroic:
                ClearArmorSetFlag(ArmorSetCondition.Veteran);
                ClearArmorSetFlag(ArmorSetCondition.Elite);
                SetArmorSetFlag(ArmorSetCondition.Hero);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(newLevel));
        }
    }

    public override void SetArmorSetFlag(ArmorSetCondition armorSetType)
    {
        _currentArmorSetFlags.Set(armorSetType, true);
    }

    public override void ClearArmorSetFlag(ArmorSetCondition armorSetType)
    {
        _currentArmorSetFlags.Set(armorSetType, false);
    }

    public override bool TestArmorSetFlag(ArmorSetCondition armorSetType)
    {
        return _currentArmorSetFlags.Get(armorSetType);
    }

    public override DamageInfo? LastDamageInfo => _lastDamageInfo;

    public override LogicFrame LastDamageFrame => _lastDamageFrame ?? LogicFrame.Zero;

    public override LogicFrame LastHealingFrame => _lastHealingFrame ?? LogicFrame.Zero;

    public override ObjectId ClearableLastAttacker => _lastDamageCleared ? ObjectId.Invalid : _lastDamageInfo.Request.SourceID;

    public override void ClearLastAttacker()
    {
        _lastDamageCleared = true;
    }

    public override bool FrontCrushed
    {
        get => _frontCrushed;
        set => _frontCrushed = value;
    }

    public override bool BackCrushed
    {
        get => _backCrushed;
        set => _backCrushed = value;
    }

    public override void InternalChangeHealth(float delta)
    {
        // Save the current health as the previous health.
        _previousHealth = _currentHealth;

        // Change the health by the delta. It can be positive or negative.
        _currentHealth += delta;

        // Clamp the new health.
        _currentHealth = Math.Clamp(_currentHealth, 0.0f, _maxHealth);

        // Recalculate the damage state.
        var oldState = _currentDamageState;
        SetCorrectDamageState();

        // If our state has changed...
        if (_currentDamageState != oldState)
        {
            // ... show a visual change in the model for the damage state. We
            // do not show visual changes for damage states when things are
            // under construction, because we just don't have all the art
            // states for that during buildup animation.
            if (!GameObject.TestStatus(ObjectStatus.UnderConstruction))
            {
                EvaluateVisualCondition();
            }
        }

        // Mark the bit according to our health. If our AI is dead but our
        // health improves, it will still re-flag this bit in the AIDeadState
        // every frame.
        GameObject.IsEffectivelyDead = _currentHealth <= 0;
    }

    public override bool IsIndestructible
    {
        get => _indestructible;
        set
        {
            _indestructible = value;

            // For bridges, we mirror this state on its towers.
            if (GameObject.IsKindOf(ObjectKinds.Bridge))
            {
                var bb = GameObject.FindBehavior<BridgeBehavior>();
                if (bb != null)
                {
                    foreach (var bridgeTowerType in Enum.GetValues<BridgeTowerType>())
                    {
                        var towerId = bb.GetTowerId(bridgeTowerType);
                        var tower = GameEngine.GameLogic.GetObjectById(towerId);

                        if (tower?.BodyModule != null)
                        {
                            tower.BodyModule.IsIndestructible = value;
                        }
                    }
                }
            }
        }
    }

    public override void EvaluateVisualCondition()
    {
        GameObject.Drawable?.ReactToBodyDamageStateChange(_currentDamageState);

        // Destroy any particle systems that were attached to our body for the
        // old state and create new particle systems for the new state.
        UpdateBodyParticleSystems();
    }

    public override void UpdateBodyParticleSystems()
    {
        // TODO(Port): Implemement this.
    }

    private bool CanBeSubdued => _moduleData.SubdualDamageCap > 0;

    private bool IsSubdued => _maxHealth <= _currentSubdualDamage;

    private void SetCorrectDamageState()
    {
        _currentDamageState = CalculateDamageState(_currentHealth, _maxHealth);

        // Original comment:
        // @todo srj -- bleah, this is an icky way to do it. oh well.
        if (_currentDamageState == BodyDamageType.Rubble
            && GameObject.IsKindOf(ObjectKinds.Structure))
        {
            var rubbleHeight = GameObject.Definition.StructureRubbleHeight;

            if (rubbleHeight <= 0.0f)
            {
                rubbleHeight = GameEngine.AssetStore.GameData.Current.DefaultStructureRubbleHeight;
            }

            // There's an original comment that says this was changed to a
            // Z only version, to keep it from disappearing from PartitionManager
            // for a frame (which didn't previously happen).
            GameObject.SetGeometryInfoZ(rubbleHeight);

            // Have to tell pathfind as well, as rubble pathfinds differently.
            GameEngine.AI.Pathfinder.RemoveObjectFromPathfindMap(GameObject);
            GameEngine.AI.Pathfinder.AddObjectToPathfindMap(GameObject);

            // Here we make sure nobody collides with us, ever again.
            // This allows projectiles shot from infantry that are inside
            // rubble to get out of said rubble safely.
            GameObject.SetObjectStatus(ObjectStatus.NoCollisions, true);
        }
    }

    private void DoDamageFX(in DamageInfoInput damageInput, in DamageInfoOutput damageOutput)
    {
        // Just the visual aspect of damage can be overridden in some cases.
        // Unresistable is the default to mean no override, as we are out of bits.
        var damageTypeToUse = damageInput.DamageFXOverride != DamageType.Unresistable
            ? damageInput.DamageFXOverride
            : damageInput.DamageType;

        if (_currentDamageFX == null)
        {
            return;
        }

        var now = GameEngine.GameLogic.CurrentFrame;

        if (damageTypeToUse == _lastDamageFXDone && _nextDamageFXFrame > now)
        {
            return;
        }

        var source = GameEngine.GameLogic.GetObjectById(damageInput.SourceID);

        _lastDamageFXDone = damageTypeToUse;
        _nextDamageFXFrame = now + _currentDamageFX.GetDamageFXThrottleTime(damageTypeToUse, source);

        _currentDamageFX.DoDamageFX(
            damageTypeToUse,
            damageOutput.ActualDamageDealt,
            source,
            GameObject,
            GameEngine);
    }

    private void InternalAddSubdualDamage(float delta)
    {
        _currentSubdualDamage += delta;
        _currentSubdualDamage = Math.Min(_currentSubdualDamage, _moduleData.SubdualDamageCap);
    }

    private void OnSubdualChange(bool isNowSubdued)
    {
        if (!GameObject.IsKindOf(ObjectKinds.Projectile))
        {
            var me = GameObject;

            if (isNowSubdued)
            {
                me.SetDisabled(DisabledType.Subdued);

                me.Contain?.OrderAllPassengersToIdle(CommandSourceTypes.FromAI);
            }
            else
            {
                me.ClearDisabled(DisabledType.Subdued);

                if (me.IsKindOf(ObjectKinds.FSInternetCenter))
                {
                    // Any unit inside an internet center is a hacker! Order
                    // them to start hacking again.
                    me.Contain?.OrderAllPassengersToHackInternet(CommandSourceTypes.FromAI);
                }
            }
        }
        else if (isNowSubdued)
        {
            // There's no coming back from being jammed, and projectiles can't
            // even heal, but this makes it clear.
            GameObject.FindBehavior<IProjectileUpdate>()?.ProjectileNowJammed();
        }
    }

    private bool ShouldRetaliateAgainstAggressor(GameObject obj, GameObject? damager)
    {
        // TODO(Port): Implement this.
        return false;
    }

    private void SetLastDamageInfo(in DamageInfo damageInfo)
    {
        _lastDamageInfo = damageInfo;
        _lastDamageCleared = false;
        _lastDamageFrame = GameEngine.GameLogic.CurrentFrame;
    }

    protected internal override void OnDestroy()
    {
        DeleteAllParticleSystems();
    }

    private void DeleteAllParticleSystems()
    {
        // TODO(Port): Implement this.
    }

    private void ValidateArmorAndDamageFX()
    {
        var set = BitArrayMatchFinder.FindBest(
            CollectionsMarshal.AsSpan(GameObject.Definition.ArmorSets),
            _currentArmorSetFlags);

        if (set != null && set != _currentArmorSet)
        {
            _currentArmor = new Armor(set.Armor.Value);
            _currentDamageFX = set.DamageFX?.Value;
            _currentArmorSet = set;
        }
    }

    private BodyDamageType CalculateDamageState(float health, float maxHealth)
    {
        var ratio = health / maxHealth;

        var gameData = GameEngine.AssetStore.GameData.Current;
        if (ratio > gameData.UnitDamagedThreshold)
        {
            return BodyDamageType.Pristine;
        }
        else if (ratio > gameData.UnitReallyDamagedThreshold)
        {
            return BodyDamageType.Damaged;
        }
        else if (ratio > 0.0f)
        {
            return BodyDamageType.ReallyDamaged;
        }
        else
        {
            return BodyDamageType.Rubble;
        }
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistSingle(ref _currentHealth);

        // ZH changed added this field, but didn't bump the version.
        if (reader.SageGame >= SageGame.CncGeneralsZeroHour)
        {
            reader.PersistSingle(ref _currentSubdualDamage);
        }

        reader.PersistSingle(ref _previousHealth);
        reader.PersistSingle(ref _maxHealth);
        reader.PersistSingle(ref _initialHealth);
        reader.PersistEnum(ref _currentDamageState);
        reader.PersistLogicFrame(ref _nextDamageFXFrame);
        reader.PersistEnumOptional(ref _lastDamageFXDone);

        reader.PersistObject(ref _lastDamageInfo);
        reader.PersistLogicFrameOptional(ref _lastDamageFrame);
        reader.PersistLogicFrameOptional(ref _lastHealingFrame);

        reader.PersistBoolean(ref _frontCrushed);
        reader.PersistBoolean(ref _backCrushed);

        reader.PersistBoolean(ref _lastDamageCleared);
        reader.PersistBoolean(ref _indestructible);

        reader.PersistList(
        _particleSystemIds,
        static (StatePersister persister, ref uint item) =>
        {
            persister.PersistUInt32Value(ref item);
        });

        reader.PersistBitArray(ref _currentArmorSetFlags);
    }
}

public class ActiveBodyModuleData : BodyModuleData
{
    internal static ActiveBodyModuleData Parse(IniParser parser)
    {
        var result = parser.ParseBlock(FieldParseTable);

        // BFME and later allows InitialValue to be omitted, in which case
        // it is set to MaxHealth.
        if (parser.SageGame >= SageGame.Bfme && !result._initialHealthSet)
        {
            result.InitialHealth = result.MaxHealth;
        }

        return result;
    }

    internal static readonly IniParseTable<ActiveBodyModuleData> FieldParseTable = new()
    {
        { "MaxHealth", (parser, x) => x.MaxHealth = parser.ParseFloat() },
        { "InitialHealth", (parser, x) => { x.InitialHealth = parser.ParseFloat(); x._initialHealthSet = true; } },
        { "MaxHealthDamaged", (parser, x) => x.MaxHealthDamaged = parser.ParseFloat() },
        { "MaxHealthReallyDamaged", (parser, x) => x.MaxHealthReallyDamaged = parser.ParseFloat() },
        { "RecoveryTime", (parser, x) => x.RecoveryTime = parser.ParseFloat() },

        { "SubdualDamageCap", (parser, x) => x.SubdualDamageCap = parser.ParseFloat() },
        { "SubdualDamageHealRate", (parser, x) => x.SubdualDamageHealRate = parser.ParseTimeMillisecondsToLogicFrames() },
        { "SubdualDamageHealAmount", (parser, x) => x.SubdualDamageHealAmount = parser.ParseFloat() },
        { "GrabObject", (parser, x) => x.GrabObject = parser.ParseAssetReference() },
        { "GrabOffset", (parser, x) => x.GrabOffset = parser.ParsePoint() },
        { "DamageCreationList", (parser, x) => x.DamageCreationLists.Add(DamageCreationList.Parse(parser)) },
        { "GrabFX", (parser, x) => x.GrabFX = parser.ParseAssetReference() },
        { "GrabDamage", (parser, x) => x.GrabDamage = parser.ParseInteger() },
        { "CheerRadius", (parser, x) => x.CheerRadius = parser.ParseInteger() },
        { "DodgePercent", (parser, x) => x.DodgePercent = parser.ParsePercentage() },
        { "UseDefaultDamageSettings", (parser, x) => x.UseDefaultDamageSettings = parser.ParseBoolean() },
        { "EnteringDamagedTransitionTime", (parser, x) => x.EnteringDamagedTransitionTime = parser.ParseInteger() },
        { "HealingBuffFx", (parser, x) => x.HealingBuffFx = parser.ParseAssetReference() },
        { "BurningDeathBehavior", (parser, x) => x.BurningDeathBehavior = parser.ParseBoolean() },
        { "BurningDeathFX", (parser, x) => x.BurningDeathFX = parser.ParseAssetReference() },
        { "DamagedAttributeModifier", (parser, x) => x.DamagedAttributeModifier = parser.ParseAssetReference() },
        { "ReallyDamagedAttributeModifier", (parser, x) => x.ReallyDamagedAttributeModifier = parser.ParseAssetReference() }
    };

    private bool _initialHealthSet;

    public float MaxHealth { get; internal set; }
    public float InitialHealth { get; internal set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public float SubdualDamageCap { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public LogicFrameSpan SubdualDamageHealRate { get; private set; }

    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public float SubdualDamageHealAmount { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public float MaxHealthDamaged { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public float RecoveryTime { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public string? GrabObject { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public Point2D GrabOffset { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public List<DamageCreationList> DamageCreationLists { get; private set; } = [];

    [AddedIn(SageGame.Bfme)]
    public float MaxHealthReallyDamaged { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public string? GrabFX { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int GrabDamage { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int CheerRadius { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public Percentage DodgePercent { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public bool UseDefaultDamageSettings { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public int EnteringDamagedTransitionTime { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public string? HealingBuffFx { get; private set; }

    [AddedIn(SageGame.Bfme2)]
    public bool BurningDeathBehavior { get; private set; }

    [AddedIn(SageGame.Bfme2)]
    public string? BurningDeathFX { get; private set; }

    [AddedIn(SageGame.Bfme2Rotwk)]
    public string? DamagedAttributeModifier { get; private set; }

    [AddedIn(SageGame.Bfme2Rotwk)]
    public string? ReallyDamagedAttributeModifier { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new ActiveBody(gameObject, gameEngine, this);
    }
}

[AddedIn(SageGame.Bfme)]
public sealed class DamageCreationList
{
    internal static DamageCreationList Parse(IniParser parser)
    {
        return new DamageCreationList()
        {
            Object = parser.ParseAssetReference(),
            ObjectKind = parser.ParseEnum<ObjectKinds>(),
            Unknown = parser.ParseString()
        };
    }

    public string? Object { get; private set; }
    public ObjectKinds ObjectKind { get; private set; }
    public string? Unknown { get; private set; }
}
