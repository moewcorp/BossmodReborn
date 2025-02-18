namespace BossMod.Stormblood.Quest.MSQ.ARequiemForHeroes;

class StormUnbound(BossModule module) : Components.Exaflare(module, 5)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TheStormUnboundCast)
            Lines.Add(new() { Next = spell.LocXZ, Advance = caster.Rotation.ToDirection() * 5, NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1, ExplosionsLeft = 4, MaxShownExplosions = 2 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TheStormUnboundCast or AID.TheStormUnboundRepeat)
        {
            foreach (var l in Lines.Where(l => l.Next.AlmostEqual(caster.Position, 1)))
                AdvanceLine(l, caster.Position);
        }
    }
}

class LightlessSpark2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LightlessSparkAdds), new AOEShapeCone(40, 45.Degrees()));

class ArtOfTheStorm(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ArtOfTheStorm), 8);
class EntropicFlame(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.EntropicFlame), new AOEShapeRect(50, 4));

class FloodOfDarkness(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FloodOfDarkness), 6);
class VeinSplitter(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.VeinSplitter), 10);
class LightlessSpark(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LightlessSpark), new AOEShapeCone(40, 45.Degrees()));
class SwellUnbound(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TheSwellUnbound), new AOEShapeDonut(8, 20));
class Swell(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ArtOfTheSwell), 8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(new AOEShapeDonut(8, 50), Arena.Center);
    }
}

abstract class ArtOfTheSword(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(40, 3));
class ArtOfTheSword1(BossModule module) : ArtOfTheSword(module, AID.ArtOfTheSword1);
class ArtOfTheSword2(BossModule module) : ArtOfTheSword(module, AID.ArtOfTheSword2);
class ArtOfTheSword3(BossModule module) : ArtOfTheSword(module, AID.ArtOfTheSword3);

class DarkAether(BossModule module) : Components.GenericAOEs(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Module.Enemies(OID.DarkAether).Select(e => new AOEInstance(new AOEShapeCircle(1.5f), e.Position, e.Rotation));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in ActiveAOEs(slot, actor))
            hints.AddForbiddenZone(new AOEShapeRect(3, 1.5f, 1.5f), c.Origin, c.Rotation, c.Activation);
    }
}

class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.TheStorm, (uint)OID.TheSwell, (uint)OID.AmeNoHabakiri]);

public class ZenosP2States : StateMachineBuilder
{
    public ZenosP2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FloodOfDarkness>()
            .ActivateOnEnter<VeinSplitter>()
            .ActivateOnEnter<LightlessSpark>()
            .ActivateOnEnter<LightlessSpark2>()
            .ActivateOnEnter<SwellUnbound>()
            .ActivateOnEnter<Swell>()
            .ActivateOnEnter<ArtOfTheSword1>()
            .ActivateOnEnter<ArtOfTheSword2>()
            .ActivateOnEnter<ArtOfTheSword3>()
            .ActivateOnEnter<ArtOfTheStorm>()
            .ActivateOnEnter<EntropicFlame>()
            .ActivateOnEnter<DarkAether>()
            .ActivateOnEnter<StormUnbound>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68721, NameID = 6039, PrimaryActorOID = (uint)OID.BossP2)]
public class ZenosP2(WorldState ws, Actor primary) : BossModule(ws, primary, new(233, -93.25f), new ArenaBoundsCircle(20));
