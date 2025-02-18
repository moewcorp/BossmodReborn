namespace BossMod.Shadowbringers.Alliance.A13Engels;

class DemolishStructureArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect square = new(5, 5, 5, InvertForbiddenZone: true);
    private AOEInstance? _aoe;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DemolishStructure2 && Arena.Bounds == A13MarxEngels.StartingBounds)
            _aoe = new(square, A13MarxEngels.TransitionSpot, Color: Colors.SafeFromAOE);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x0B)
        {
            Arena.Center = A13MarxEngels.SecondArenaCenter;
            Arena.Bounds = A13MarxEngels.StartingBounds;
            _aoe = null; //
        }
    }
}

class MarxSmash1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MarxSmash1), new AOEShapeRect(60, 15));
class MarxSmash2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MarxSmash2), new AOEShapeRect(60, 15));
class MarxSmash3(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MarxSmash3), new AOEShapeRect(60, 15));
class MarxSmash4(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MarxSmash4), new AOEShapeRect(30, 30));
class MarxSmash5(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MarxSmash5), new AOEShapeRect(35, 30));
class MarxSmash6(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MarxSmash6), new AOEShapeRect(60, 10));
class MarxSmash7(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MarxSmash7), new AOEShapeRect(60, 10));

class MarxCrush(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MarxCrush), new AOEShapeRect(15, 15));

class PrecisionGuidedMissile2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.PrecisionGuidedMissile2), 6);
class LaserSight1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LaserSight1), new AOEShapeRect(100, 10));
class GuidedMissile2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GuidedMissile2), 6);
class IncendiaryBombing2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.IncendiaryBombing2), 8);
class IncendiaryBombing1(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.IncendiaryBombing1), 8, 5);
class DiffuseLaser(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DiffuseLaser));
class SurfaceMissile2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SurfaceMissile2), 6);

class GuidedMissile(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6), ActionID.MakeSpell(AID.GuidedMissile2), ActionID.MakeSpell(AID.GuidedMissile3), 5.5f, 1, 4, true);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9147)]
public class A13MarxEngels(WorldState ws, Actor primary) : BossModule(ws, primary, StartingArenaCenter, StartingBounds)
{
    public static readonly WPos TransitionSpot = new(900, 697);
    public static readonly WPos StartingArenaCenter = new(900, 670);
    public static readonly WPos SecondArenaCenter = new(900, 785);
    public static readonly ArenaBoundsSquare StartingBounds = new(30);
}
