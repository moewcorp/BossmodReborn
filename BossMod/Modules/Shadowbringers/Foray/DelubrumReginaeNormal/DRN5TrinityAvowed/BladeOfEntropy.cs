namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

class BladeOfEntropy(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCone cone = new(40f, 90f.Degrees());
    private readonly PlayerTemperatures _temps = module.FindComponent<PlayerTemperatures>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (slot is < 0 or > 23)
            return [];
        if (_aoe is AOEInstance aoe)
        {
            var id = aoe.ActorID;
            if (id != default && id == _temps.Temperatures[slot])
            {
                aoe.Color = Colors.SafeFromAOE;
                aoe.Shape = cone with { InvertForbiddenZone = true };
            }
            return new Span<AOEInstance>([aoe]);
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var temp = spell.Action.ID switch
        {
            (uint)AID.BladeOfEntropyAC11 or (uint)AID.BladeOfEntropyBC11 => 1u,
            (uint)AID.BladeOfEntropyAH11 or (uint)AID.BladeOfEntropyBH11 => 3u,
            (uint)AID.BladeOfEntropyAC12 or (uint)AID.BladeOfEntropyBC12 => 2u,
            (uint)AID.BladeOfEntropyAH12 or (uint)AID.BladeOfEntropyBH12 => 4u,
            _ => default
        };
        if (temp != default)
            _aoe = new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), ActorID: temp);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BladeOfEntropyAC11:
            case (uint)AID.BladeOfEntropyBC11:
            case (uint)AID.BladeOfEntropyAH11:
            case (uint)AID.BladeOfEntropyBH11:
            case (uint)AID.BladeOfEntropyAC12:
            case (uint)AID.BladeOfEntropyBC12:
            case (uint)AID.BladeOfEntropyAH12:
            case (uint)AID.BladeOfEntropyBH12:
                _aoe = null;
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len == 0)
            return;
        ref readonly var aoe = ref aoes[0];
        var isInside = aoe.Check(actor.Position);
        if (aoe.Color != Colors.SafeFromAOE)
        {
            if (isInside)
                hints.Add(WarningText);
        }
        else
        {
            hints.Add("Get hit by AOE!", !isInside);
        }
    }
}
