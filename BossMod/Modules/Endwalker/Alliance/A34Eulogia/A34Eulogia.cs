namespace BossMod.Endwalker.Alliance.A34Eulogia;

class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly WPos Center = new(945f, -945f);
    private static readonly ArenaBoundsSquare squareBounds = new(24f);
    private static readonly ArenaBoundsCircle smallerBounds = new(30f);
    public static readonly ArenaBoundsCircle BigBounds = new(35f);
    private static readonly AOEShapeCustom transitionSquare = new([new Square(Center, 30f)], [new Square(Center, 24f)]);
    private static readonly AOEShapeDonut transitionSmallerBounds = new(30f, 35f);
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
        if (spell.Action.ID == (uint)AID.Hieroglyphika)
            _aoe = new(transitionSquare, Center, default, Module.CastFinishAt(spell));
        else if (spell.Action.ID == (uint)AID.Whorl)
            _aoe = new(transitionSmallerBounds, Center, default, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Hieroglyphika)
        {
            Arena.Bounds = squareBounds;
            _aoe = null;
        }
        else if (spell.Action.ID == (uint)AID.Whorl)
        {
            Arena.Bounds = smallerBounds;
            _aoe = null;
        }
    }
}

class Sunbeam(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.SunbeamAOE), new AOEShapeCircle(6f), true);
class DestructiveBolt(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DestructiveBoltAOE), 6f, 8);

class HoD(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(90f, 20f));
class HandOfTheDestroyerWrath(BossModule module) : HoD(module, AID.HandOfTheDestroyerWrathAOE);
class HandOfTheDestroyerJudgment(BossModule module) : HoD(module, AID.HandOfTheDestroyerJudgmentAOE);

class SoaringMinuet(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeCone(40f, 135f.Degrees()));
class EudaimonEorzea(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.EudaimonEorzeaAOE));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11301, SortOrder = 7)]
public class A34Eulogia(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.Center, ArenaChanges.BigBounds);
