﻿namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class Stance(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCone _shapeCone = new(50f, 40f.Degrees());
    private static readonly AOEShapeCone _shapeOut = new(24f, 90f.Degrees());
    private static readonly AOEShapeDonut _shapeIn = new(8f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.SusurrantBreathAOE => _shapeCone,
            (uint)AID.SlitheringStrikeAOE => _shapeOut,
            (uint)AID.StranglingCoilAOE => _shapeIn,
            _ => null
        };
        if (shape != null)
            _aoe = new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SusurrantBreathAOE or (uint)AID.SlitheringStrikeAOE or (uint)AID.StranglingCoilAOE)
            ++NumCasts;
    }
}

class CharringCataclysm(BossModule module) : Components.UniformStackSpread(module, 4f, default, 2, 2)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SusurrantBreathAOE or (uint)AID.SlitheringStrikeAOE or (uint)AID.StranglingCoilAOE)
        {
            // note: dd vs supports is random, select supports arbitrarily
            AddStacks(Module.Raid.WithoutSlot(true, true, true).Where(p => p.Class.IsSupport()), Module.CastFinishAt(spell, 0.7f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.CharringCataclysm)
            Stacks.Clear();
    }
}

class ChillingCataclysm(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ChillingCataclysmAOE))
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCross _shape = new(40f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ChillingCataclysmArcaneSphere)
        {
            var act = WorldState.FutureTime(5.6d);
            var pos = WPos.ClampToGrid(actor.Position);
            _aoes.Add(new(_shape, pos, Angle.AnglesCardinals[1], act));
            _aoes.Add(new(_shape, pos, Angle.AnglesIntercardinals[1], act));
        }
    }
}

class CracklingCataclysm(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CracklingCataclysm), 6f);
