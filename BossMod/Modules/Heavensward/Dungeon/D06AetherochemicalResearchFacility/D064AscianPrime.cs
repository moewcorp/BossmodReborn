namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D064AscianPrime;

public enum OID : uint
{
    Boss = 0x3DA7, // R3.8
    LahabreasShade = 0x3DAB, // R3.5
    IgeyorhmsShade = 0x3DAA, // R3.5
    FrozenStar = 0x3DA8, // R1.5
    BurningStar = 0x3DA9, // R1.5
    ArcaneSphere = 0x3DAC, // R7.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target

    AncientCircle = 31901, // Helper->self, no cast, range 10-20 donut, player targeted donut AOE, kind of a stack
    AncientDarkness = 31903, // Helper->self, no cast, range 6 circle

    AncientEruptionVisual = 31908, // Boss->self, 5.0s cast, single-target
    AncientEruption = 31909, // Helper->location, 5.0s cast, range 5 circle

    AncientFrost = 31904, // Helper->players, no cast, range 6 circle

    Annihilation = 31927, // Boss->location, no cast, single-target
    AnnihilationAOE = 33024, // Helper->self, 6.3s cast, range 40 circle
    AnnihilationEnrage = 31928, // Boss->location, no cast, single-target, if Arcane Sphere doesn't get destroyed in time
    AnnihilationEnrageAOE = 33025, // Helper->self, 6.3s cast, range 40 circle

    ArcaneRevelation1 = 31912, // Boss->location, no cast, single-target, teleport
    ArcaneRevelation2 = 31913, // Boss->self, 3.0s cast, single-target
    BurningChains = 31905, // Helper->player, no cast, single-target

    ChillingCrossVisual = 31922, // IgeyorhmsShade->self, 6.0s cast, single-target
    ChillingCross1 = 31923, // Helper->self, 6.0s cast, range 40 width 5 cross
    ChillingCross2 = 31924, // Helper->self, 6.0s cast, range 40 width 5 cross

    CircleOfIcePrimeVisual1 = 31898, // FrozenStar->self, no cast, single-target
    CircleOfIcePrimeVisual2 = 31899, // FrozenStar->self, no cast, single-target
    CircleOfIcePrime = 33021, // Helper->self, 2.0s cast, range 5-40 donut
    FireSpherePrimeVisual1 = 31896, // BurningStar->self, no cast, single-target
    FireSpherePrimeVisual2 = 31897, // BurningStar->self, no cast, single-target
    FireSpherePrime = 33022, // Helper->self, 2.0s cast, range 16 circle

    DarkBlizzardIIIVisual = 31914, // IgeyorhmsShade->self, 6.0s cast, single-target
    DarkBlizzardIII1 = 31915, // Helper->self, 6.0s cast, range 41 20-degree cone
    DarkBlizzardIII2 = 31916, // Helper->self, 6.0s cast, range 41 20-degree cone
    DarkBlizzardIII3 = 31917, // Helper->self, 6.0s cast, range 41 20-degree cone
    DarkBlizzardIII4 = 31918, // Helper->self, 6.0s cast, range 41 20-degree cone
    DarkBlizzardIII5 = 31919, // Helper->self, 6.0s cast, range 41 20-degree cone

    DarkFireIIVisual = 31920, // LahabreasShade->self, 6.0s cast, single-target
    DarkFireII = 31921, // Helper->players, 6.0s cast, range 6 circle

    Dualstar = 31894, // Boss->self, 4.0s cast, single-target

    EntropicFlame = 31907, // Helper->player, no cast, single-target
    EntropicFlame1 = 32126, // Boss->self, no cast, single-target
    EntropicFlame2 = 31906, // Boss->self, 5.0s cast, single-target
    EntropicFlame3 = 32555, // Helper->self, no cast, range 50 width 8 rect, line stack

    FusionPrime = 31895, // Boss->self, 3.0s cast, single-target

    HeightOfChaos = 31911, // Boss->player, 5.0s cast, range 5 circle

    ShadowFlare1 = 31910, // Boss->self, 5.0s cast, range 40 circle
    ShadowFlare2 = 31925, // IgeyorhmsShade->self, 5.0s cast, range 40 circle
    ShadowFlare3 = 31926, // LahabreasShade->self, 5.0s cast, range 40 circle

    UniversalManipulationTeleport = 31419, // Boss->location, no cast, single-target
    UniversalManipulation = 31900, // Boss->self, 5.0s cast, range 40 circle
    UniversalManipulation2 = 33044 // Boss->player, no cast, single-target
}

public enum IconID : uint
{
    AncientCircle = 384, // player
    DarkWhispers = 139, // player
    AncientFrost = 161 // player
}

public enum TetherID : uint
{
    BurningChains = 9 // player->player
}

class AncientCircle(BossModule module) : Components.DonutStack(module, ActionID.MakeSpell(AID.AncientCircle), (uint)IconID.AncientCircle, 10f, 20f, 8f, 4, 4);

class DarkWhispers(BossModule module) : Components.UniformStackSpread(module, default, 6f, alwaysShowSpreads: true)
{
    // regular spread component won't work because this is self targeted
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DarkWhispers)
            AddSpread(actor, WorldState.FutureTime(5d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Spreads.Count != 0 && spell.Action.ID == (uint)AID.AncientDarkness)
            Spreads.Clear();
    }
}

class AncientFrost(BossModule module) : Components.StackWithIcon(module, (uint)IconID.AncientFrost, ActionID.MakeSpell(AID.AncientFrost), 6f, 5f, 4, 4);
class ShadowFlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShadowFlare1));
class ShadowFlareLBPhase(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShadowFlare2), "Raidwide x2");
class Annihilation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AnnihilationAOE));
class UniversalManipulation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.UniversalManipulation), "Raidwide + Apply debuffs for later");

class HeightOfChaos(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.HeightOfChaos), new AOEShapeCircle(5f), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count != 0)
            hints.Add("Tankbuster cleave");
    }
}

class AncientEruption(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AncientEruption), 5f);

abstract class ChillingCross(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCross(40f, 2.5f));
class ChillingCross1(BossModule module) : ChillingCross(module, AID.ChillingCross1);
class ChillingCross2(BossModule module) : ChillingCross(module, AID.ChillingCross2);

abstract class DarkBlizzard(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(41f, 10f.Degrees()));
class DarkBlizzardIIIAOE1(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII1);
class DarkBlizzardIIIAOE2(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII2);
class DarkBlizzardIIIAOE3(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII3);
class DarkBlizzardIIIAOE4(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII4);
class DarkBlizzardIIIAOE5(BossModule module) : DarkBlizzard(module, AID.DarkBlizzardIII5);

class DarkFireII(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.DarkFireII), 6f);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, ActionID.MakeSpell(AID.BurningChains));
class EntropicFlame(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.EntropicFlame), ActionID.MakeSpell(AID.EntropicFlame3), 5.2f);

class Stars(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(5f, 40f);
    private static readonly AOEShapeCircle circle = new(16f);
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly WPos _frozenStarShortTether = new(230f, 86f), _frozenStarLongTether = new(230f, 92f);
    private static readonly WPos _donut = WPos.ClampToGrid(new(230f, 79f)), _circle1 = WPos.ClampToGrid(new(241f, 79f)), _circle2 = WPos.ClampToGrid(new(219f, 79f));

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        List<AOEInstance> aoes = new(count);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if ((aoe.Activation - _aoes[0].Activation).TotalSeconds <= 1d)
                aoes.Add(aoe);
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        void AddAOEs(bool reverse)
        {
            AddAOE(circle, _circle1, !reverse);
            AddAOE(circle, _circle2, !reverse);
            AddAOE(donut, _donut, reverse);
            if (reverse)
                _aoes.Reverse();
            void AddAOE(AOEShape shape, WPos pos, bool first) => _aoes.Add(new(shape, pos, default, WorldState.FutureTime(first ? 11.8d : 14.8f)));
        }

        if (actor.OID == (uint)OID.FrozenStar)
        {
            if (actor.Position == _frozenStarLongTether)
                AddAOEs(false);
            else if (actor.Position == _frozenStarShortTether)
                AddAOEs(true);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.CircleOfIcePrime or (uint)AID.FireSpherePrime)
            _aoes.RemoveAt(0);
    }
}

class D064AscianPrimeStates : StateMachineBuilder
{
    public D064AscianPrimeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AncientCircle>()
            .ActivateOnEnter<DarkWhispers>()
            .ActivateOnEnter<AncientFrost>()
            .ActivateOnEnter<ShadowFlare>()
            .ActivateOnEnter<ShadowFlareLBPhase>()
            .ActivateOnEnter<Annihilation>()
            .ActivateOnEnter<HeightOfChaos>()
            .ActivateOnEnter<DarkBlizzardIIIAOE1>()
            .ActivateOnEnter<DarkBlizzardIIIAOE2>()
            .ActivateOnEnter<DarkBlizzardIIIAOE3>()
            .ActivateOnEnter<DarkBlizzardIIIAOE4>()
            .ActivateOnEnter<DarkBlizzardIIIAOE5>()
            .ActivateOnEnter<AncientEruption>()
            .ActivateOnEnter<ChillingCross1>()
            .ActivateOnEnter<ChillingCross2>()
            .ActivateOnEnter<Stars>()
            .ActivateOnEnter<DarkFireII>()
            .ActivateOnEnter<UniversalManipulation>()
            .ActivateOnEnter<EntropicFlame>()
            .ActivateOnEnter<BurningChains>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3823, SortOrder = 11)]
public class D064AscianPrime(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly ArenaBoundsComplex arena = new([new Polygon(new(230f, 79f), 20.26f, 24)], [new Rectangle(new(230f, 98f), 5, 1.5f)], [new Rectangle(new(228.95f, 97f), 6.55f, 1.7f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ArcaneSphere));
    }
}
