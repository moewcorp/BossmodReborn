namespace BossMod.Shadowbringers.Hunt.RankA.Nariphon;

public enum OID : uint
{
    Boss = 0x2890 // R=6.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    VineHammer = 16969, // Boss->player, no cast, single-target, attacks several random players in a row
    AllergenInjection = 16972, // Boss->player, 5.0s cast, range 6 circle
    RootsOfAtopy = 16971, // Boss->player, 5.0s cast, range 6 circle
    OdiousMiasma = 16970 // Boss->self, 3.0s cast, range 12 120-degree cone
}

public enum SID : uint
{
    PiercingResistanceDownII = 1435 // Boss->player, extra=0x0
}

class OdiousMiasma(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OdiousMiasma), new AOEShapeCone(12, 60.Degrees()));

class AllergenInjection(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.AllergenInjection), new AOEShapeCircle(6), true)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaits.Any(x => x.Target == actor))
            hints.Add("Bait away!");
    }
}

class RootsOfAtopy(BossModule module) : Components.GenericStackSpread(module)
{
    private BitMask _forbidden;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RootsOfAtopy)
            Stacks.Add(new(WorldState.Actors.Find(spell.TargetID)!, 6, 8, 8, activation: Module.CastFinishAt(spell), forbiddenPlayers: _forbidden));
    }
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.PiercingResistanceDownII)
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.PiercingResistanceDownII)
            _forbidden[Raid.FindSlot(actor.InstanceID)] = default;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RootsOfAtopy)
            Stacks.RemoveAt(0);
    }
}

class NariphonStates : StateMachineBuilder
{
    public NariphonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OdiousMiasma>()
            .ActivateOnEnter<RootsOfAtopy>()
            .ActivateOnEnter<AllergenInjection>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8907)]
public class Nariphon(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) { }
