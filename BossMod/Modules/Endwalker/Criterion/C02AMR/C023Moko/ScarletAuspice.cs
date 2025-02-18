﻿namespace BossMod.Endwalker.VariantCriterion.C02AMR.C023Moko;

abstract class ScarletAuspice(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6f);
class NScarletAuspice(BossModule module) : ScarletAuspice(module, AID.NScarletAuspice);
class SScarletAuspice(BossModule module) : ScarletAuspice(module, AID.SScarletAuspice);

abstract class BoundlessScarletFirst(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(60f, 5f));
class NBoundlessScarletFirst(BossModule module) : BoundlessScarletFirst(module, AID.NBoundlessScarletAOE);
class SBoundlessScarletFirst(BossModule module) : BoundlessScarletFirst(module, AID.SBoundlessScarletAOE);

abstract class BoundlessScarletRest(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(60f, 15f), 2);
class NBoundlessScarletRest(BossModule module) : BoundlessScarletRest(module, AID.NBoundlessScarletExplosion);
class SBoundlessScarletRest(BossModule module) : BoundlessScarletRest(module, AID.SBoundlessScarletExplosion);

class InvocationOfVengeance(BossModule module) : Components.UniformStackSpread(module, 3, 3, alwaysShowSpreads: true)
{
    public int NumMechanics;
    private readonly List<Actor> _spreadTargets = [];
    private readonly List<Actor> _stackTargets = [];
    private DateTime _spreadResolve;
    private DateTime _stackResolve;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_spreadResolve == default || _stackResolve == default)
            return;
        var orderHint = _spreadResolve > _stackResolve ? $"Stack -> Spread" : $"Spread -> Stack";
        hints.Add($"Debuff order: {orderHint}");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.VengefulFlame:
                _spreadTargets.Add(actor);
                _spreadResolve = status.ExpireAt;
                UpdateStackSpread();
                break;
            case (uint)SID.VengefulPyre:
                _stackTargets.Add(actor);
                _stackResolve = status.ExpireAt;
                UpdateStackSpread();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NVengefulFlame:
            case (uint)AID.SVengefulFlame:
                if (_spreadResolve != default)
                {
                    ++NumMechanics;
                    _spreadTargets.Clear();
                    _spreadResolve = default;
                    UpdateStackSpread();
                }
                break;
            case (uint)AID.NVengefulPyre:
            case (uint)AID.SVengefulPyre:
                if (_stackResolve != default)
                {
                    ++NumMechanics;
                    _stackTargets.Clear();
                    _stackResolve = default;
                    UpdateStackSpread();
                }
                break;
        }
    }

    private void UpdateStackSpread()
    {
        Spreads.Clear();
        Stacks.Clear();
        if (_stackResolve == default || _stackResolve > _spreadResolve)
            AddSpreads(_spreadTargets, _spreadResolve);
        if (_spreadResolve == default || _spreadResolve > _stackResolve)
            AddStacks(_stackTargets, _stackResolve);
    }
}
