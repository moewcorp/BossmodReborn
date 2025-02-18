namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class SpikeFlail(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SpikeFlail), new AOEShapeCone(80f, 135f.Degrees()))
{
    public override bool KeepOnPhaseChange => true;
}

class Touchdown(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Touchdown), 24f)
{
    public override bool KeepOnPhaseChange => true;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return Casters.Count != 0 && (Module.FindComponent<DragonBreath>()?.AOE == null || Arena.Bounds != A12Fafnir.FireArena) ? [Casters[0]] : [];
    }
}

class DragonBreath(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.DragonBreath))
{
    public override bool KeepOnPhaseChange => true;
    public AOEInstance? AOE;

    private static readonly AOEShapeDonut donut = new(16f, 30f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OffensivePostureDragonBreath)
        {
            NumCasts = 0;
            AOE = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 1.2f));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.FireVoidzone && state == 0x00040008)
            AOE = null;
    }
}
