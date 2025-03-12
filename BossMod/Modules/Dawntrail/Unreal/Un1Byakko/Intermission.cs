namespace BossMod.Dawntrail.Unreal.Un1Byakko;

class VoiceOfThunder : Components.PersistentInvertibleVoidzone
{
    public VoiceOfThunder(BossModule module) : base(module, 2f, GetOrbs)
    {
        InvertResolveAt = WorldState.CurrentTime;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var orbs = GetOrbs(Module);
        if (orbs.Length != 0)
            hints.Add("Touch the orbs!");
    }

    private static Actor[] GetOrbs(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.AramitamaSoul);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class Intermission(BossModule module) : BossComponent(module)
{
    public bool Active;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ArenaFeatures && state is 0x00040008 or 0x00100020)
            Active = state == 0x00040008;
    }
}

class IntermissionOrbAratama(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.IntermissionOrbAratama))
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.IntermissionOrbSpawn:
                AOEs.Add(new(_shape, spell.TargetXZ, default, WorldState.FutureTime(5.1f)));
                break;
            case (uint)AID.IntermissionOrbAratama:
                ++NumCasts;
                AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(spell.TargetXZ, 1f));
                break;
        }
    }
}

class IntermissionSweepTheLeg(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.IntermissionSweepTheLeg), new AOEShapeDonut(5f, 25f));
class ImperialGuard(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ImperialGuard), new AOEShapeRect(44.75f, 2.5f));
class FellSwoop(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.FellSwoop));
