#nullable enable

namespace OpenSage.Logic.Object;

public class ExperienceTracker(GameObject parent, GameEngine engine) : IPersistableObject
{
    private readonly GameEngine _engine = engine;

    /// <summary>
    /// Object I am owned by.
    /// </summary>
    private readonly GameObject _parent = parent;

    /// <summary>
    /// Level of experience.
    /// </summary>
    private VeterancyLevel _currentLevel = VeterancyLevel.Regular;

    // TODO: Generals used two different names for the same thing (m_currentLevel and getVeterancyLevel).
    // We could unify them.

    /// <summary>
    /// What level am I?
    /// </summary>
    public VeterancyLevel VeterancyLevel => _currentLevel;

    private int _currentExperience;
    /// <summary>
    /// Number of experience points.
    /// </summary>
    public int CurrentExperience => _currentExperience;

    private ObjectId _experienceSink;
    /// <summary>
    /// ID of object I have pledged my experience point gains to.
    /// This is used by objects like missiles which grant the experience to the vehicle that fired them.
    /// </summary>
    public ObjectId ExperienceSink
    {
        get => _experienceSink;
        set => _experienceSink = value;
    }

    private float _experienceScalar = 1.0f;
    /// <summary>
    /// Scales any experience gained by this multiplier.
    /// </summary>
    public float ExperienceScalar
    {
        get => _experienceScalar;
        set => _experienceScalar = value;
    }

    /// <summary>
    /// Can I gain experience?
    /// </summary>
    public bool IsTrainable => _parent.Definition.IsTrainable;

    /// <summary>
    /// Either I am trainable, or I have a sink set up.
    /// </summary>
    public bool IsAcceptingExperiencePoints => IsTrainable || ExperienceSink.IsValid;

    public int? ExperienceRequiredForNextLevel
    {
        get
        {
            if (_currentLevel == VeterancyLevel.Last)
            {
                return null;
            }

            return _parent.Definition.ExperienceRequired[_currentLevel + 1];
        }
    }

    public int GetExperienceValue(GameObject killer)
    {
        if (killer.GetRelationship(_parent) == RelationshipType.Allies)
        {
            // Friendly fire does not give experience.
            return 0;
        }

        return _parent.Definition.ExperienceValue[_currentLevel];
    }

    // TODO: This is not how Generals actually does this.
    // This is a temporary hack until we rewrite / refactor Scene25D.
    public bool ShowRankUpAnimation { get; set; } = false;

    private void OnVeterancyLevelChanged(VeterancyLevel oldLevel, VeterancyLevel newLevel, bool provideFeedback = true)
    {
        _parent.OnVeterancyLevelChanged(oldLevel, newLevel, provideFeedback);
        ShowRankUpAnimation = true;
    }

    /// <summary>
    /// Set the veterancy level to this level.
    /// If we've already at this exact level, do nothing.
    /// </summary>
    public void SetVeterancyLevel(VeterancyLevel newLevel, bool provideFeedback = true)
    {
        if (newLevel == _currentLevel)
        {
            return;
        }

        var oldLevel = _currentLevel;
        _currentLevel = newLevel;
        _currentExperience = _parent.Definition.ExperienceRequired[_currentLevel];
        // Original code checks if parent is null here, but I don't think it can ever be null
        OnVeterancyLevelChanged(oldLevel, newLevel, provideFeedback);
    }

    /// <summary>
    /// Set veterancy level to at least this level.
    /// If we've already reached or exceeded this level, do nothing.
    /// </summary>
    public void SetMinVeterancyLevel(VeterancyLevel newLevel)
    {
        if (newLevel <= _currentLevel)
        {
            return;
        }

        var oldLevel = _currentLevel;
        _currentExperience = _parent.Definition.ExperienceRequired[_currentLevel];
        // Original code checks if parent is null here, but I don't think it can ever be null
        OnVeterancyLevelChanged(oldLevel, newLevel);
    }

    public void AddExperiencePoints(int experienceGain, bool canScaleForBonus = true)
    {
        if (ExperienceSink.IsValid)
        {
            // I have been set up to give my experience to someone else
            var sinkObject = _engine.Scene3D.GameObjects.GetObjectById(ExperienceSink);

            if (sinkObject != null)
            {
                sinkObject.ExperienceTracker.AddExperiencePoints((int)(experienceGain * ExperienceScalar), canScaleForBonus);
                return;
            }
        }

        if (!IsTrainable)
        {
            // If we're not trainable, no points are given.
            return;
        }

        var oldLevel = _currentLevel;
        var amountToGain = experienceGain;
        if (canScaleForBonus)
        {
            amountToGain = (int)(amountToGain * ExperienceScalar);
        }

        _currentExperience += amountToGain;

        var levelIndex = 0;

        // While we can gain more levels and we have enough experience to gain the next level...
        while (((levelIndex + 1) < (int)VeterancyLevel.Count) && _currentExperience >= _parent.Definition.ExperienceRequired[(VeterancyLevel)(levelIndex + 1)])
        {
            levelIndex++;
        }

        _currentLevel = (VeterancyLevel)levelIndex;

        // If this experience gain resulted in a level up, notify the parent object.
        if (oldLevel != _currentLevel)
        {
            OnVeterancyLevelChanged(oldLevel, _currentLevel);
        }
    }

    public bool GainExpForLevel(int levelsToGain, bool canScaleForBonus = true)
    {
        var newLevel = _currentLevel + levelsToGain;

        if (newLevel > VeterancyLevel.Last)
        {
            newLevel = VeterancyLevel.Last;
        }

        if (newLevel > _currentLevel)
        {
            var experienceNeeded = _parent.Definition.ExperienceRequired[newLevel] - _currentExperience;
            AddExperiencePoints(experienceNeeded, canScaleForBonus);
            return true;
        }

        return false;
    }

    public bool CanGainExpForLevel(int levelsToGain)
    {
        var newLevel = _currentLevel + levelsToGain;

        if (newLevel > VeterancyLevel.Last)
        {
            newLevel = VeterancyLevel.Last;
        }

        return newLevel > _currentLevel;
    }

    // TODO: This is like 95% identical to AddExperiencePoints.
    public void SetExperienceAndLevel(int experienceIn, bool provideFeedback = true)
    {
        if (ExperienceSink.IsValid)
        {
            var sinkObject = _engine.Scene3D.GameObjects.GetObjectById(ExperienceSink);

            if (sinkObject != null)
            {
                sinkObject.ExperienceTracker.SetExperienceAndLevel(experienceIn, provideFeedback);
                return;
            }
        }

        if (!IsTrainable)
        {
            return;
        }

        var oldLevel = _currentLevel;
        _currentExperience = experienceIn;

        var levelIndex = 0;
        while (((levelIndex + 1) < (int)VeterancyLevel.Count) && _currentExperience >= _parent.Definition.ExperienceRequired[(VeterancyLevel)(levelIndex + 1)])
        {
            levelIndex++;
        }

        _currentLevel = (VeterancyLevel)levelIndex;

        if (oldLevel != _currentLevel)
        {
            OnVeterancyLevelChanged(oldLevel, _currentLevel, provideFeedback);
            // Comment from original code: "paradox! this may be a level lost!"
            // What does that mean?
        }
    }

    public void Persist(StatePersister persister)
    {
        persister.PersistVersion(1);

        persister.PersistEnum(ref _currentLevel);
        persister.PersistInt32(ref _currentExperience);
        persister.PersistObjectId(ref _experienceSink);
        persister.PersistSingle(ref _experienceScalar);
    }
}
