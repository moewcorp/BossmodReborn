namespace BossMod.Dawntrail.Quest.MSQ.AFatherFirst;

public enum OID : uint
{
    Boss = 0x4176, // R=5.0
    GuloolJaJasShade = 0x4177, // R4.5
    FireVoidzone = 0x1E8D9B, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Teleport = 36400, // Boss/GuloolJaJasShade->location, no cast, single-target
    FancyBladework = 36413, // Boss->self, 5.0s cast, range 60 circle
    DualBlowsVisual1 = 36393, // Boss->self, 7.0s cast, single-target
    DualBlowsVisual2 = 36394, // Boss->self, no cast, single-target
    DualBlowsVisual3 = 36395, // Boss->self, 7.0s cast, single-target
    DualBlowsVisual4 = 36396, // Boss->self, no cast, single-target
    DualBlows1 = 35421, // Helper->self, 8.0s cast, range 30 180-degree cone
    DualBlows2 = 35422, // Helper->self, 10.5s cast, range 30 180-degree cone
    DualBlows3 = 35423, // Helper->self, 8.0s cast, range 30 180-degree cone
    DualBlows4 = 35424, // Helper->self, 10.5s cast, range 30 180-degree cone
    SteeledStrikeVisual1 = 36389, // Boss->location, 4.0s cast, single-target
    SteeledStrikeVisual2 = 36391, // GuloolJaJasShade->location, 4.0s cast, single-target
    SteeledStrikeVisual3 = 37062, // Helper->self, 5.2s cast, range 30 width 8 cross
    SteeledStrikeVisual4 = 37063, // Helper->self, 5.2s cast, range 30 width 8 cross
    SteeledStrike1 = 36390, // Helper->self, 5.2s cast, range 30 width 8 cross
    SteeledStrike2 = 36392, // Helper->self, 5.2s cast, range 30 width 8 cross
    CoiledStrikeVisual1 = 36405, // Boss->self, 5.0+1,0s cast, single-target
    CoiledStrikeVisual2 = 36406, // Boss->self, 5.0+1,0s cast, single-target
    CoiledStrike = 36407, // Helper->self, 6.0s cast, range 30 150-degree cone

    BattleBreaker = 36414, // Boss->self, 5.0s cast, range 40 width 30 rect, knockback 50, dir foward
    PhaseChange = 36415, // Boss->self, no cast, single-target
    MorningStars1 = 39135, // Helper->location, 1.5s cast, range 4 circle
    MorningStars2 = 38819, // Helper->location, 3.0s cast, range 4 circle
    MorningStarsEnd = 38820, // Boss->self, no cast, single-target
    BurningSunTelegraph1 = 36409, // Helper->location, 1.5s cast, range 4 circle
    BurningSunTelegraph2 = 36410, // Helper->location, 1.5s cast, range 6 circle
    BurningSunVisual = 36408, // Boss->self, 6.3+0,7s cast, single-target
    BurningSun1 = 36411, // Helper->location, 1.0s cast, range 4 circle
    BurningSun2 = 36412, // Helper->location, 1.0s cast, range 6 circle
    BrawlEnder = 36397, // Boss->self, 5.0s cast, range 50 circle
    BrawlEnderKB = 36398, // Boss->player, no cast, single-target, knockback 20, source forward (direction player is facing)
    DoublingVisual1 = 38475, // Boss->self, 3.0s cast, single-target
    DoublingVisual2 = 36424, // Boss->self, 3.0s cast, single-target
    DoublingVisual3 = 38814, // GuloolJaJasShade->self, 3.0s cast, single-target
    GloryBlaze = 36417, // GuloolJaJasShade->self, 8.0s cast, range 40 width 6 rect
    TheThrillVisual1 = 38815, // GuloolJaJasShade->location, 5.0+1,5s cast, single-target
    TheThrillVisual2 = 36418, // Boss->location, 5.0+1,5s cast, single-target
    TheThrill1 = 38817, // Helper->self, 6.2s cast, range 3 circle
    TheThrill2 = 36420, // Helper->self, 6.2s cast, range 3 circle
    UnmitigatedExplosion1 = 36419, // Helper->self, no cast, range 45 circle, tower fail
    UnmitigatedExplosion2 = 38816, // Helper->self, no cast, range 45 circle, tower fail

    ContestofWillsPull = 38818, // Helper->self, no cast, range 40 circle, pull 30 between centers
    ContestOfWills1 = 36421, // Boss->self, 2.0+1,0s cast, single-target
    ContestOfWills2 = 36422, // Boss->self, no cast, single-target
    ContestOfWillsEnrage = 36425, // Helper->self, no cast, range 50 circle, quick time event fail
    IndigoBreeze = 34860 // Helper->self, no cast, single-target
}

class DualBlowsSteeledStrike(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCone cone = new(30f, 90f.Degrees());
    private static readonly AOEShapeCross cross = new(30f, 4f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape) => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        switch (spell.Action.ID)
        {
            case (uint)AID.DualBlows1:
            case (uint)AID.DualBlows2:
            case (uint)AID.DualBlows3:
            case (uint)AID.DualBlows4:
                AddAOE(cone);
                if (_aoes.Count == 2)
                    _aoes.SortBy(x => x.Activation);
                break;
            case (uint)AID.SteeledStrike1:
            case (uint)AID.SteeledStrike2:
                AddAOE(cross);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.DualBlows1:
                case (uint)AID.DualBlows2:
                case (uint)AID.DualBlows3:
                case (uint)AID.DualBlows4:
                case (uint)AID.SteeledStrike1:
                case (uint)AID.SteeledStrike2:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

class BurningSun(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(26);
    private static readonly AOEShapeCircle circleSmall = new(4f), circleBig = new(6f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i < 10)
                aoes[i] = count > 9 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BurningSunTelegraph1)
            _aoes.Add(new(circleSmall, spell.LocXZ, default, Module.CastFinishAt(spell, 4f)));
        else if (spell.Action.ID == (uint)AID.BurningSunTelegraph2)
            _aoes.Add(new(circleBig, spell.LocXZ, default, Module.CastFinishAt(spell, 4f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.BurningSun1 or (uint)AID.BurningSun2)
            _aoes.RemoveAt(0);
    }
}

class BrawlEnder(BossModule module) : Components.Knockback(module, stopAtWall: true)
{
    private DateTime activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (activation != default)
            yield return new(actor.Position, 20, activation, default, actor.Rotation, Kind.DirForward);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<FireVoidzone>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BrawlEnder)
            activation = Module.CastFinishAt(spell, 0.9f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BrawlEnder)
            activation = default;
    }
}

abstract class TheThrill(BossModule module, AID aid) : Components.CastTowers(module, ActionID.MakeSpell(aid), 3f);
class TheThrill1(BossModule module) : TheThrill(module, AID.TheThrill1);
class TheThrill2(BossModule module) : TheThrill(module, AID.TheThrill2);

class FireVoidzone(BossModule module) : Components.PersistentVoidzone(module, 6f, m => m.Enemies(OID.FireVoidzone).Where(z => z.EventState != 7));
class FancyBladework(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FancyBladework));
class CoiledStrike(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CoiledStrike), new AOEShapeCone(30f, 75f.Degrees()));
class GloryBlaze(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GloryBlaze), new AOEShapeRect(40f, 3f));
class BattleBreaker(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BattleBreaker));

abstract class MorningStars(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 4f);
class MorningStars1(BossModule module) : MorningStars(module, AID.MorningStars1);
class MorningStars2(BossModule module) : MorningStars(module, AID.MorningStars2);

class AFatherFirstStates : StateMachineBuilder
{
    public AFatherFirstStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DualBlowsSteeledStrike>()
            .ActivateOnEnter<FancyBladework>()
            .ActivateOnEnter<GloryBlaze>()
            .ActivateOnEnter<CoiledStrike>()
            .ActivateOnEnter<MorningStars1>()
            .ActivateOnEnter<MorningStars2>()
            .ActivateOnEnter<FireVoidzone>()
            .ActivateOnEnter<BurningSun>()
            .ActivateOnEnter<TheThrill1>()
            .ActivateOnEnter<TheThrill2>()
            .ActivateOnEnter<BattleBreaker>()
            .ActivateOnEnter<BrawlEnder>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70419, NameID = 12675)]
public class AFatherFirst(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, 49f), new ArenaBoundsRect(14.55f, 19.5f));
