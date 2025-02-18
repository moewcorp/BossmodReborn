namespace BossMod.RealmReborn.Quest.MSQ.TheUltimateWeapon;

public enum OID : uint
{
    Boss = 0x3933, // R1.75
    SeaOfPitch = 0x1EB738, // R0.5
    Firesphere = 0x3934, // R1.0
}

public enum AID : uint
{
    AncientFireIII = 29327, // Boss->self, 4.0s cast, range 40 circle
    DarkThunder = 29329, // Lahabrea->self, 4.0s cast, range 1 circle
    EndOfDays = 29331, // Boss->self, 4.0s cast, range 60 width 8 rect
    EndOfDaysAdds = 29762, // PhantomLahabrea->self, 4.0s cast, range 60 width 8 rect
    Nightburn = 29340, // Boss->player, 4.0s cast, single-target
    FiresphereSummon = 29332, // Boss->self, 4.0s cast, single-target
    Burst = 29333, // Firesphere->self, 3.0s cast, range 8 circle
    AncientEruption = 29335, // Lahabrea->self, 4.0s cast, range 6 circle
    FluidFlare = 29760, // Lahabrea->self, 4.0s cast, range 40 60-degree cone
    AncientCross = 29756, // Lahabrea->self, 4.0s cast, range 6 circle
    BurstFlare = 29758, // Lahabrea->self, 5.0s cast, range 60 circle
    GripOfNight = 29337, // Boss->self, 6.0s cast, range 40 150-degree cone
}

class BurstFlare(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.BurstFlare), 10)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // don't add any hints if Burst hasn't gone off yet, it tends to spook AI mode into running into deathwall
        if (Module.Enemies(OID.Firesphere).Any(x => x.CastInfo?.RemainingTime > 0))
            return;
        if (Casters.Count != 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 5), Module.CastFinishAt(Casters[0].CastInfo));
    }
}

class GripOfNight(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GripOfNight), new AOEShapeCone(40, 75.Degrees()));
class AncientCross(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AncientCross), 6, 8);
class AncientEruption(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AncientEruption), 6);
class FluidFlare(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FluidFlare), new AOEShapeCone(40, 30.Degrees()));

class FireSphere(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Burst))
{
    private DateTime? _predictedCast;
    private static readonly AOEShapeCircle circle = new(8);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FiresphereSummon)
            _predictedCast = WorldState.CurrentTime.AddSeconds(12);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Burst)
            _predictedCast = Module.CastFinishAt(spell);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_predictedCast is DateTime dt && dt > WorldState.CurrentTime)
            foreach (var enemy in Module.Enemies(OID.Firesphere))
                yield return new(circle, enemy.Position, default, dt);
    }
}

class Nightburn(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Nightburn));
class AncientFire(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AncientFireIII));

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private bool completed;
    private static readonly AOEShapeDonut donut = new(15, 20);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AncientFireIII && !completed)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0 && state == 0x20001)
        {
            Arena.Bounds = new ArenaBoundsCircle(15);
            completed = true;
        }
    }
}

class DarkThunder(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DarkThunder), 1);
class SeaOfPitch(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.SeaOfPitch).Where(x => x.EventState != 7));

abstract class EoD(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(60, 4));
class EndOfDays(BossModule module) : EoD(module, AID.EndOfDays);
class EndOfDaysAdds(BossModule module) : EoD(module, AID.EndOfDaysAdds);

class LahabreaStates : StateMachineBuilder
{
    public LahabreaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<DarkThunder>()
            .ActivateOnEnter<SeaOfPitch>()
            .ActivateOnEnter<AncientFire>()
            .ActivateOnEnter<EndOfDays>()
            .ActivateOnEnter<Nightburn>()
            .ActivateOnEnter<FireSphere>()
            .ActivateOnEnter<AncientEruption>()
            .ActivateOnEnter<FluidFlare>()
            .ActivateOnEnter<AncientCross>()
            .ActivateOnEnter<BurstFlare>()
            .ActivateOnEnter<GripOfNight>()
            .ActivateOnEnter<EndOfDaysAdds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70058, NameID = 2143)]
public class Lahabrea(WorldState ws, Actor primary) : BossModule(ws, primary, new(-704, 480), new ArenaBoundsCircle(20));

