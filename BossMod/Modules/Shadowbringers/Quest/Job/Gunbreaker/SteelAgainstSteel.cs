namespace BossMod.Shadowbringers.Quest.Job.Gunbreaker.SteelAgainstSteel;

public enum OID : uint
{
    Boss = 0x2A45,
    Fustuarium = 0x2AD8, // R0.5
    CullingBlade = 0x2AD3, // R0.5
    IndustrialForce = 0x2BCE, // R0.5
    TerminusEst = 0x2A46, // R1.0
    CaptiveBolt = 0x2AD7, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    CullingBlade1 = 17553, // CullingBlade->self, 3.5s cast, range 60 30-degree cone
    TheOrder = 17568, // Boss->self, 4.0s cast, single-target
    TerminusEst1 = 17567, // TerminusEst->self, no cast, range 40+R width 4 rect
    CaptiveBolt = 17561, // CaptiveBolt->self, 7.0s cast, range 50+R width 10 rect
    AetherochemicalGrenado = 17575, // 2A47->location, 4.0s cast, range 8 circle
    Exsanguination1 = 17563, // 2AD4->self, 5.0s cast, range 2-7 180-degree donut segment
    Exsanguination2 = 17564, // 2AD5->self, 5.0s cast, range 7-12 180-degree donut segment
    Exsanguination3 = 17565, // 2AD6->self, 5.0s cast, range 12-17 180-degree donut segment
    DiffractiveLaser = 17574, // 2A48->self, 3.0s cast, range 45+R width 4 rect
    SnakeShot = 17569, // Boss->self, 4.0s cast, range 20 240-degree cone
    ScaldingTank1 = 17558, // Fustuarium->2A4A, 6.0s cast, range 6 circle
    ToTheSlaughter = 17559, // Boss->self, 4.0s cast, range 40 180-degree cone
}

class ScaldingTank(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ScaldingTank1), 6);
class ToTheSlaughter(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ToTheSlaughter), new AOEShapeCone(40, 90.Degrees()));
class Exsanguination1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Exsanguination1), new AOEShapeDonutSector(2, 7, 90.Degrees()));
class Exsanguination2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Exsanguination2), new AOEShapeDonutSector(7, 12, 90.Degrees()));
class Exsanguination3(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Exsanguination3), new AOEShapeDonutSector(12, 17, 90.Degrees()));
class CaptiveBolt(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CaptiveBolt), new AOEShapeRect(50, 5), 4);
class AetherochemicalGrenado(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AetherochemicalGrenado), 8);
class DiffractiveLaser(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DiffractiveLaser), new AOEShapeRect(45, 2));
class SnakeShot(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SnakeShot), new AOEShapeCone(20, 120.Degrees()));
class CullingBlade(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CullingBlade1), new AOEShapeCone(60, 15.Degrees()));

class TerminusEst(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? Caster;
    private readonly List<Actor> Actors = [];
    private static readonly AOEShapeRect rect = new(40, 2);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TerminusEst)
            Actors.Add(actor);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Caster is Actor c)
            foreach (var t in Actors)
                yield return new(rect, t.Position, t.Rotation, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // check if we already have terminuses out, because he can use this spell for a diff mechanic
        if ((AID)spell.Action.ID == AID.TheOrder && Actors.Count > 0)
            Caster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TerminusEst1)
        {
            Actors.Remove(caster);
            // reset for next iteration
            if (Actors.Count == 0)
                Caster = null;
        }
    }
}

class VitusQuoMessallaStates : StateMachineBuilder
{
    public VitusQuoMessallaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CullingBlade>()
            .ActivateOnEnter<TerminusEst>()
            .ActivateOnEnter<CaptiveBolt>()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<SnakeShot>()
            .ActivateOnEnter<Exsanguination1>()
            .ActivateOnEnter<Exsanguination2>()
            .ActivateOnEnter<Exsanguination3>()
            .ActivateOnEnter<ToTheSlaughter>()
            .ActivateOnEnter<ScaldingTank>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68802, NameID = 8872)]
public class VitusQuoMessalla(WorldState ws, Actor primary) : BossModule(ws, primary, new(-266, -507), new ArenaBoundsCircle(19.5f));
