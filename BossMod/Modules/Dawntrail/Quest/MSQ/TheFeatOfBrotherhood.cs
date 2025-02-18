namespace BossMod.Dawntrail.Quest.MSQ.TheFeatOfBrotherhood;

public enum OID : uint
{
    Boss = 0x4206, // R5.0    
    EphemeralSword = 0x4228, // R2.5
    BallOfFire = 0x44B6, // R1.0-2.5
    OathOfFire = 0x4229, // R2.500
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->tank, no cast, single-target
    Teleport = 37199, // Boss->location, no cast, single-target

    RoaringStarVisual = 39304, // Boss->self, 8.0s cast, single-target
    RoaringStarRaidwide = 39305, // Helper->self, 8.0s cast, range 40 circle
    RoaringStarRect = 39302, // BallOfFire->self, 6.0s cast, range 50 width 10 rect

    CoiledStrikeVisual1 = 37186, // Boss->self, 5.0+1,0s cast, single-target
    CoiledStrikeVisual2 = 37187, // Boss->self, 5.0+1,0s cast, single-target
    CoiledStrike = 37188, // Helper->self, 6.0s cast, range 30 150-degree cone

    CelestialFlameVisual = 37175, // Boss->self, 4.0s cast, single-target
    CelestialFlame = 37206, // EphemeralSword->self, no cast, range 40 circle
    SublimeHeat = 37176, // EphemeralSword->self, 5.0s cast, range 10 circle
    Burn = 39299, // Helper->self, 5.0s cast, range 46 width 5 rect

    FallenStar = 37205, // Helper->everyone, 5.0s cast, range 6 circle, spread

    FirstLight = 39298, // Helper->location, 6.0s cast, range 6 circle
    SteelfoldStrikeVisual1 = 37182, // Boss->location, 5.0+1,0s cast, single-target
    SteelfoldStrikeVisual2 = 39300, // Boss->location, no cast, single-target
    SteelfoldStrikeVisual3 = 39532, // Helper->self, 6.0s cast, range 30 width 8 cross
    SteelfoldStrike = 37183, // Helper->self, 6.0s cast, range 30 width 8 cross

    LayOfTheSunA1 = 37207, // Boss->WukLamat, 4.9+3,1s cast, range 6 circle
    LayOfTheSunA2 = 37208, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunA3 = 37209, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunA4 = 37210, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunA5 = 37211, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunA6 = 37212, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunA7 = 37213, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunA8 = 37214, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunA9 = 37215, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunB1 = 40065, // Boss->WukLamat, 4.9+3,1s cast, range 6 circle
    LayOfTheSunB2 = 40066, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunB3 = 40067, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunB4 = 40068, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunB5 = 40069, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunB6 = 40070, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunB7 = 40071, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunB8 = 40072, // Helper->WukLamat, no cast, range 6 circle
    LayOfTheSunB9 = 40073, // Helper->WukLamat, no cast, range 6 circle

    OathbindVisual = 37216, // Boss->self, 11.0s cast, single-target
    Oathbind = 40064, // Helper->Urianger/WukLamat/Alphinaud/Thancred, no cast, single-target
    OathbindPull = 37217, // Helper->Urianger/WukLamat/Alphinaud/Thancred, no cast, single-target, pull 10 between centers
    Oathbind2 = 39810, // Helper->self, no cast, single-target
    NobleTrail = 37218, // Boss->location, 50.0s cast, width 20 rect charge

    DualPyresVisual1 = 37177, // Boss->self, 7.0s cast, single-target
    DualPyresVisual2 = 37178, // Boss->self, no cast, single-target
    DualPyresVisual3 = 37181, // Boss->self, 7.0s cast, single-target
    DualPyresVisual4 = 37309, // Boss->self, no cast, single-target
    DualPyres1 = 37310, // Helper->self, 8.0s cast, range 30 180-degree cone
    DualPyres2 = 37311, // Helper->self, 10.5s cast, range 30 180-degree cone
    DualPyres3 = 37179, // Helper->self, 8.0s cast, range 30 180-degree cone
    DualPyres4 = 37180, // Helper->self, 10.5s cast, range 30 180-degree cone

    InnerWakeVisual = 37200, // Boss->self, 2.5+2,5s cast, single-target
    InnerWake = 37201, // Helper->self, 5.0s cast, range 10 circle
    OuterWakeVisual = 37202, // Boss->self, 2.5+2,5s cast, single-target
    OuterWake = 37203, // Helper->self, 5.0s cast, range 6-40 donut

    BattleBreaker = 37192, // Boss->self, 5.0s cast, range 40 width 40 rect, raidwide, knockback 10 forward
    SagaOfDawnAndDutyVisual = 37193, // Boss->location, no cast, ???
    SagaOfDawnAndDuty = 37196, // Helper->self, no cast, range 60 circle
    Shockwave = 39711, // Helper->self, no cast, range 60 circle
    HeavyImpact = 37197, // Helper->self, no cast, range 60 circle
    HeartOfTural = 37195, // WukLamat->self, 8.0s cast, range 20 width 40 rect
}

class DualPyresSteelfoldStrike(BossModule module) : Components.GenericAOEs(module)
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
            case (uint)AID.DualPyres1:
            case (uint)AID.DualPyres2:
            case (uint)AID.DualPyres3:
            case (uint)AID.DualPyres4:
                AddAOE(cone);
                if (_aoes.Count == 2)
                    _aoes.SortBy(x => x.Activation);
                break;
            case (uint)AID.SteelfoldStrike:
                AddAOE(cross);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.DualPyres1:
                case (uint)AID.DualPyres2:
                case (uint)AID.DualPyres3:
                case (uint)AID.DualPyres4:
                case (uint)AID.SteelfoldStrike:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

class RoaringStarRect(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeRect rect = new(50f, 5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E46)
            _aoes.Add(new(rect, actor.Position, actor.Rotation, WorldState.FutureTime(8.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RoaringStarRect)
            _aoes.Clear();
    }
}

class SublimeHeat(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(9);
    private static readonly AOEShapeCircle circle = new(10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 6 ? 6 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
            aoes[i] = _aoes[i];
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.CelestialFlame)
            _aoes.Add(new(circle, caster.Position, default, WorldState.FutureTime(7.5d)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.SublimeHeat)
            _aoes.RemoveAt(0);
    }
}

class NobleTrail(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe != null && Module.PrimaryActor.IsTargetable ? [_aoe.Value] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NobleTrail)
        {
            var dir = spell.LocXZ - caster.Position;
            _aoe = new(new AOEShapeRect(dir.Length(), 10f), caster.Position, Angle.FromDirection(dir), Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NobleTrail)
            _aoe = null;
    }
}

class LayOfTheSun(BossModule module) : Components.UniformStackSpread(module, 6f, 0, 8)
{
    private int numCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is >= 37207 and <= 37215 or >= 40065 and <= 40073)
        {
            ++numCasts;
            if (numCasts == 9)
            {
                Stacks.Clear();
                numCasts = 0;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LayOfTheSunA1 or (uint)AID.LayOfTheSunB1)
            AddStack(WorldState.Actors.Find(spell.TargetID)!, Module.CastFinishAt(spell));
    }
}

class RoaringStar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.RoaringStarRaidwide));
class CoiledStrike(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CoiledStrike), new AOEShapeCone(30f, 75f.Degrees()));
class Burn(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Burn), new AOEShapeRect(46f, 2.5f), 8);
class FallenStar(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FallenStar), 6f);
class FirstLight(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FirstLight), 6f);
class InnerWake(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.InnerWake), 10f);
class OuterWake(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OuterWake), new AOEShapeDonut(6f, 40f));
class BattleBreaker(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BattleBreaker));
class HeartOfTuralRaidwides(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HeartOfTural), "Raidwides x7");

class HeartOfTural : Components.SimpleAOEs
{
    public HeartOfTural(BossModule module) : base(module, ActionID.MakeSpell(AID.HeartOfTural), new AOEShapeRect(20f, 20f, InvertForbiddenZone: true)) { Color = Colors.SafeFromAOE; }

    private const string hint = "Wait in safe area!";
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Casters.Count == 0)
            return;
        if (Casters[0].Check(actor.Position))
            hints.Add(hint);
        else
            hints.Add(hint, false);
    }
}

class TheFeatOfBrotherhoodStates : StateMachineBuilder
{
    public TheFeatOfBrotherhoodStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DualPyresSteelfoldStrike>()
            .ActivateOnEnter<RoaringStarRect>()
            .ActivateOnEnter<RoaringStar>()
            .ActivateOnEnter<CoiledStrike>()
            .ActivateOnEnter<SublimeHeat>()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<FallenStar>()
            .ActivateOnEnter<FirstLight>()
            .ActivateOnEnter<LayOfTheSun>()
            .ActivateOnEnter<InnerWake>()
            .ActivateOnEnter<OuterWake>()
            .ActivateOnEnter<NobleTrail>()
            .ActivateOnEnter<BattleBreaker>()
            .ActivateOnEnter<HeartOfTural>()
            .ActivateOnEnter<HeartOfTuralRaidwides>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70444, NameID = 12734)]
public class TheFeatOfBrotherhood(WorldState ws, Actor primary) : BossModule(ws, primary, new(353.5f, 596.4f), new ArenaBoundsSquare(19.75f, -77.5f.Degrees()))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.OathOfFire));
    }
}
