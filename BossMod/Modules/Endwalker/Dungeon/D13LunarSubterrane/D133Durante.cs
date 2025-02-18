namespace BossMod.Endwalker.Dungeon.D13LunarSubterrane.D133Durante;

public enum OID : uint
{
    Boss = 0x4042, // R=6.0
    AethericCharge = 0x4043, // R=1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 34991, // Boss->location, no cast, single-target

    ArcaneEdge = 35010, // Boss->player, 5.0s cast, single-target
    OldMagic = 35011, // Boss->self, 5.0s cast, range 60 circle

    DuplicitousBatteryTelegraph = 36058, // Helper->location, 3.5s cast, range 5 circle
    DuplicitousBatteryVisual = 36057, // Boss->self, 6.0s cast, single-target
    DuplicitousBattery = 34994, // Helper->location, no cast, range 5 circle

    ForsakenFount = 35003, // Boss->self, 3.0s cast, single-target
    Explosion1 = 35006, // 4043->self, 5.0s cast, range 11 circle
    Explosion2 = 35005, // Helper->self, 12.0s cast, range 9 circle
    FallenGrace = 35882, // Helper->player, 5.0s cast, range 6 circle
    Contrapasso = 35905, // Boss->self, 3.0s cast, range 60 circle
    Splinter = 35004, // AethericCharge->self, no cast, single-target
    HardSlash = 35009, // Boss->self, 5.0s cast, range 50 90-degree cone

    AntipodalAssaultMarker = 14588, // Helper->player, no cast, single-target
    AntipodalAssaultVisual = 35007, // Boss->self, 5.0s cast, single-target, line stack
    AntipodalAssault = 35008, // Boss->location, no cast, width 8 rect charge

    TwilightPhaseVisual1 = 36055, // Boss->self, 6.0s cast, single-target
    TwilightPhaseVisual2 = 34997, // Boss->self, no cast, single-target
    TwilightPhaseVisual3 = 34998, // Boss->self, no cast, single-target
    TwilightPhase = 36056, // Helper->self, 7.3s cast, range 60 width 20 rect

    DarkImpactVisual = 35001, // Boss->location, 7.0s cast, single-target
    DarkImpact = 35002, // Helper->self, 8.0s cast, range 25 circle

    DeathsJourneyVisual = 35872, // Helper->self, 6.5s cast, range 30 30-degree cone
    DeathsJourneyCircle = 34995, // Boss->self, 6.0s cast, range 8 circle
    DeathsJourneyCone = 34996 // Helper->self, 6.5s cast, range 30 30-degree cone
}

class OldMagicArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20, 23);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OldMagic && Arena.Bounds == D133Durante.StartingBounds)
            _aoe = new(donut, Module.Center, default, Module.CastFinishAt(spell));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x0A)
        {
            Arena.Bounds = D133Durante.DefaultBounds;
            _aoe = null;
        }
    }
}

class OldMagic(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.OldMagic));
class ArcaneEdge(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ArcaneEdge));
class Contrapasso(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Contrapasso));

class DuplicitousBattery(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(32);
    private static readonly AOEShapeCircle circle = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        List<AOEInstance> aoes = new(count);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            var isRisky = i < 16;
            aoes.Add(aoe with { Color = isRisky ? Colors.Danger : 0, Risky = isRisky });
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DuplicitousBatteryTelegraph)
            _aoes.Add(new(circle, spell.LocXZ, default, WorldState.FutureTime(6.5f)));

    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID == AID.DuplicitousBattery)
            _aoes.RemoveAt(0);
    }
}

class Explosion1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Explosion1), 11);
class Explosion2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Explosion2), 9);
class FallenGrace(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FallenGrace), 6);
class AntipodalAssault(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.AntipodalAssaultMarker), ActionID.MakeSpell(AID.AntipodalAssault), 5.4f, markerIsFinalTarget: false);
class HardSlash(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HardSlash), new AOEShapeCone(50, 45.Degrees()));
class TwilightPhase(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TwilightPhase), new AOEShapeRect(60, 10));
class DarkImpact(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DarkImpact), 25);
class DeathsJourneyCircle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DeathsJourneyCircle), 8);
class DeathsJourneyCone(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DeathsJourneyCone), new AOEShapeCone(30, 15.Degrees()));

class D133DuranteStates : StateMachineBuilder
{
    public D133DuranteStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OldMagicArenaChange>()
            .ActivateOnEnter<OldMagic>()
            .ActivateOnEnter<ArcaneEdge>()
            .ActivateOnEnter<Contrapasso>()
            .ActivateOnEnter<DuplicitousBattery>()
            .ActivateOnEnter<Explosion1>()
            .ActivateOnEnter<Explosion2>()
            .ActivateOnEnter<FallenGrace>()
            .ActivateOnEnter<AntipodalAssault>()
            .ActivateOnEnter<HardSlash>()
            .ActivateOnEnter<TwilightPhase>()
            .ActivateOnEnter<DarkImpact>()
            .ActivateOnEnter<DeathsJourneyCircle>()
            .ActivateOnEnter<DeathsJourneyCone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 823, NameID = 12584)]
class D133Durante(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -422), StartingBounds)
{
    public static readonly ArenaBounds StartingBounds = new ArenaBoundsCircle(22.5f);
    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsCircle(20);
}
