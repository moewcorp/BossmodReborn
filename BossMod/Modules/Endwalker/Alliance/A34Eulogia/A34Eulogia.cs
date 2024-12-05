namespace BossMod.Endwalker.Alliance.A34Eulogia;

class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly WPos Center = new(945, -945);
    private static readonly ArenaBoundsSquare squareBounds = new(24);
    private static readonly ArenaBoundsCircle smallerBounds = new(30);
    public static readonly ArenaBoundsCircle BigBounds = new(35);
    private static readonly AOEShapeCustom transitionSquare = new([new Circle(Center, 30)], [new Square(Center, 24)]);
    private static readonly AOEShapeDonut transitionSmallerBounds = new(30, 35);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x1B)
        {
            if (state == 0x00080004)
                Arena.Bounds = BigBounds;
            else if (state == 0x00100001)
                Arena.Bounds = smallerBounds;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Hieroglyphika)
            _aoe = new(transitionSquare, Center, default, Module.CastFinishAt(spell));
        else if ((AID)spell.Action.ID == AID.Whorl)
            _aoe = new(transitionSmallerBounds, Center, default, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Hieroglyphika)
        {
            Arena.Bounds = squareBounds;
            _aoe = null;
        }
        else if ((AID)spell.Action.ID == AID.Whorl)
        {
            Arena.Bounds = smallerBounds;
            _aoe = null;
        }
    }
}

class Sunbeam(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.SunbeamAOE), new AOEShapeCircle(6), true);
class DestructiveBolt(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DestructiveBoltAOE), 6, 8);

class HoD(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(90, 20));
class HandOfTheDestroyerWrath(BossModule module) : HoD(module, AID.HandOfTheDestroyerWrathAOE);
class HandOfTheDestroyerJudgment(BossModule module) : HoD(module, AID.HandOfTheDestroyerJudgmentAOE);

class SoaringMinuet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeCone(40, 135.Degrees()));
class EudaimonEorzea(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.EudaimonEorzeaAOE));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11301, SortOrder = 7)]
public class A34Eulogia(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.Center, ArenaChanges.BigBounds);
