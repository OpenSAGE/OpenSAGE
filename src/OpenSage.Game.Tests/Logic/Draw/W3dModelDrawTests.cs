using System.Collections.Generic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests.Logic.Draw;

public class W3dModelDrawTests
{
    [Theory]
    [InlineData(new[] { ModelConditionFlag.AwaitingConstruction })]
    [InlineData(new[] { ModelConditionFlag.Sold, ModelConditionFlag.Night, ModelConditionFlag.Snow, ModelConditionFlag.ReallyDamaged })]
    [InlineData(new[] { ModelConditionFlag.Damaged, ModelConditionFlag.Snow })]
    [InlineData(new[] { ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Night, ModelConditionFlag.Snow, ModelConditionFlag.ReallyDamaged })]
    public void FindBestFittingConditionState_AmericaBarracks_TestExactMatch(ModelConditionFlag[] providedFlags)
    {
        var flags = new BitArray<ModelConditionFlag>(providedFlags);
        var bestConditionState = W3dModelDraw.FindBestFittingConditionState(AmericaBarracksModelConditionStates(), flags);
        Assert.Contains(bestConditionState.ConditionFlags, f => f.CountIntersectionBits(flags) == flags.NumBitsSet); // .equals wasn't working?
    }

    [Theory]
    [InlineData(new[] { ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed }, new[] { ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.Garrisoned })] // we can DEFINITELY have extra flags - I'm not sure if this is the result that would be returned though
    [InlineData(new[] { ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed }, new[] { ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed })]
    [InlineData(new[] { ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Night, ModelConditionFlag.Snow, ModelConditionFlag.Damaged }, new[] { ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Night, ModelConditionFlag.Snow, ModelConditionFlag.Damaged })]
    [InlineData(new[] { ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Night }, new[] { ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Night })]
    public void FindBestFittingConditionState_AmericaBarracks_TestPartialMatch(ModelConditionFlag[] expectedFlags, ModelConditionFlag[] providedFlags)
    {
        var flags = new BitArray<ModelConditionFlag>(providedFlags);
        var expected = new BitArray<ModelConditionFlag>(expectedFlags);
        var bestConditionState = W3dModelDraw.FindBestFittingConditionState(AmericaBarracksModelConditionStates(), flags);
        Assert.Contains(bestConditionState.ConditionFlags, f => f.CountIntersectionBits(expected) == expected.NumBitsSet); // .equals wasn't working?
    }

    [Theory]
    [InlineData(new[] { ModelConditionFlag.Moving })]
    [InlineData(new[] { ModelConditionFlag.ActivelyConstructing })]
    [InlineData(new[] { ModelConditionFlag.Moving, ModelConditionFlag.Carrying })]
    public void FindBestFittingConditionState_GlaWorker_TestExactMatch(ModelConditionFlag[] providedFlags)
    {
        var flags = new BitArray<ModelConditionFlag>(providedFlags);
        var bestConditionState = W3dModelDraw.FindBestFittingConditionState(GlaWorkerConditionStates(), flags);
        Assert.Contains(bestConditionState.ConditionFlags, f => f.CountIntersectionBits(flags) == flags.NumBitsSet); // .equals wasn't working?
    }

    [Theory]
    [InlineData(new[] { ModelConditionFlag.Carrying }, new[] { ModelConditionFlag.Carrying, ModelConditionFlag.ReallyDamaged })]
    public void FindBestFittingConditionState_GlaWorker_TestPartialMatch(ModelConditionFlag[] expectedFlags, ModelConditionFlag[] providedFlags)
    {
        var flags = new BitArray<ModelConditionFlag>(providedFlags);
        var expected = new BitArray<ModelConditionFlag>(expectedFlags);
        var bestConditionState = W3dModelDraw.FindBestFittingConditionState(GlaWorkerConditionStates(), flags);
        Assert.Contains(bestConditionState.ConditionFlags, f => f.CountIntersectionBits(expected) == expected.NumBitsSet); // .equals wasn't working?
    }

    /// <summary>
    /// Example model condition states take from usa AmericaBarracks ModuleTag_01
    /// </summary>
    /// <returns></returns>
    private static List<ModelConditionState> AmericaBarracksModelConditionStates()
    {
        return [
            CreateModelConditionState(ModelConditionFlag.None),
            CreateModelConditionState(ModelConditionFlag.Damaged),
            CreateModelConditionState(ModelConditionFlag.ReallyDamaged, ModelConditionFlag.Rubble),
            CreateModelConditionState(ModelConditionFlag.Night),
            CreateModelConditionState(ModelConditionFlag.Damaged, ModelConditionFlag.Night),
            CreateModelConditionState(ModelConditionFlag.ReallyDamaged, ModelConditionFlag.Rubble, ModelConditionFlag.Night),
            CreateModelConditionState(ModelConditionFlag.Snow),
            CreateModelConditionState(ModelConditionFlag.Damaged, ModelConditionFlag.Snow),
            CreateModelConditionState(ModelConditionFlag.ReallyDamaged, ModelConditionFlag.Rubble, ModelConditionFlag.Snow),
            CreateModelConditionState(ModelConditionFlag.Snow, ModelConditionFlag.Night),
            CreateModelConditionState(ModelConditionFlag.Damaged, ModelConditionFlag.Snow, ModelConditionFlag.Night),
            CreateModelConditionState(ModelConditionFlag.ReallyDamaged, ModelConditionFlag.Rubble, ModelConditionFlag.Snow, ModelConditionFlag.Night),
            CreateModelConditionState(ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed),
            CreateModelConditionState(ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Damaged),
            CreateModelConditionState(ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.ReallyDamaged),
            CreateModelConditionState(ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Night),
            CreateModelConditionState(ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Night, ModelConditionFlag.Damaged),
            CreateModelConditionState(ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Night, ModelConditionFlag.ReallyDamaged),
            CreateModelConditionState(ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Snow),
            CreateModelConditionState(ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Snow, ModelConditionFlag.Damaged),
            CreateModelConditionState(ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Snow, ModelConditionFlag.ReallyDamaged),
            CreateModelConditionState(ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Night, ModelConditionFlag.Snow),
            CreateModelConditionState(ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Night, ModelConditionFlag.Snow, ModelConditionFlag.Damaged),
            CreateModelConditionState(ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.PartiallyConstructed, ModelConditionFlag.ActivelyBeingConstructed, ModelConditionFlag.Night, ModelConditionFlag.Snow, ModelConditionFlag.ReallyDamaged),
            CreateModelConditionState( // aliased
                [ModelConditionFlag.AwaitingConstruction],
                [ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.Damaged],
                [ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.ReallyDamaged],
                [ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.Night],
                [ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.Night, ModelConditionFlag.Damaged],
                [ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.Night, ModelConditionFlag.ReallyDamaged],
                [ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.Snow],
                [ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.Snow, ModelConditionFlag.Damaged],
                [ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.Snow, ModelConditionFlag.ReallyDamaged],
                [ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.Night, ModelConditionFlag.Snow],
                [ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.Night, ModelConditionFlag.Snow, ModelConditionFlag.Damaged],
                [ModelConditionFlag.AwaitingConstruction, ModelConditionFlag.Night, ModelConditionFlag.Snow, ModelConditionFlag.ReallyDamaged],
                [ModelConditionFlag.Sold],
                [ModelConditionFlag.Sold, ModelConditionFlag.Damaged],
                [ModelConditionFlag.Sold, ModelConditionFlag.ReallyDamaged],
                [ModelConditionFlag.Sold, ModelConditionFlag.Night],
                [ModelConditionFlag.Sold, ModelConditionFlag.Night, ModelConditionFlag.Damaged],
                [ModelConditionFlag.Sold, ModelConditionFlag.Night, ModelConditionFlag.ReallyDamaged],
                [ModelConditionFlag.Sold, ModelConditionFlag.Snow],
                [ModelConditionFlag.Sold, ModelConditionFlag.Snow, ModelConditionFlag.Damaged],
                [ModelConditionFlag.Sold, ModelConditionFlag.Snow, ModelConditionFlag.ReallyDamaged],
                [ModelConditionFlag.Sold, ModelConditionFlag.Night, ModelConditionFlag.Snow],
                [ModelConditionFlag.Sold, ModelConditionFlag.Night, ModelConditionFlag.Snow, ModelConditionFlag.Damaged],
                [ModelConditionFlag.Sold, ModelConditionFlag.Night, ModelConditionFlag.Snow, ModelConditionFlag.ReallyDamaged]
            ),
        ];
    }

    private static List<ModelConditionState> GlaWorkerConditionStates()
    {
        return
        [
            CreateModelConditionState([ModelConditionFlag.Moving], [ModelConditionFlag.ActivelyConstructing, ModelConditionFlag.Moving]),
            CreateModelConditionState(ModelConditionFlag.Attacking),
            CreateModelConditionState(ModelConditionFlag.Moving, ModelConditionFlag.Attacking),
            CreateModelConditionState([ModelConditionFlag.Moving, ModelConditionFlag.Carrying], [ModelConditionFlag.ActivelyConstructing, ModelConditionFlag.Moving, ModelConditionFlag.Carrying]),
            CreateModelConditionState(ModelConditionFlag.Carrying),
            CreateModelConditionState([ModelConditionFlag.Dying], [ModelConditionFlag.Dying, ModelConditionFlag.Carrying]),
            CreateModelConditionState(ModelConditionFlag.Dying, ModelConditionFlag.ExplodedFlailing),
            CreateModelConditionState(ModelConditionFlag.Dying, ModelConditionFlag.ExplodedBouncing),
            CreateModelConditionState(ModelConditionFlag.SpecialCheering),
            CreateModelConditionState([ModelConditionFlag.ActivelyConstructing], [ModelConditionFlag.ActivelyConstructing, ModelConditionFlag.Carrying]),
        ];
    }

    private static ModelConditionState CreateModelConditionState(params ModelConditionFlag[] flags)
    {
        var model = new ModelConditionState();
        model.ConditionFlags.Add(new BitArray<ModelConditionFlag>(flags));
        return model;
    }

    private static ModelConditionState CreateModelConditionState(params ModelConditionFlag[][] flagsAndAliases)
    {
        var model = new ModelConditionState();
        foreach (var flagSet in flagsAndAliases)
        {
            model.ConditionFlags.Add(new BitArray<ModelConditionFlag>(flagSet));
        }

        return model;
    }
}
