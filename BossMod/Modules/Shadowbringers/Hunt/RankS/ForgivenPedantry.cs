namespace BossMod.Shadowbringers.Hunt.RankS.ForgivenPedantry;

public enum OID : uint
{
    Boss = 0x298A, // R=5.5
}

public enum AID : uint
{
    AutoAttackSanctifiedScathe = 17439, // Boss->player, no cast, single-target
    LeftCheek1 = 17446, // Boss->self, 5.0s cast, range 60 180-degree cone
    LeftCheek2 = 17447, // Boss->self, no cast, range 60 180-degree cone
    RightCheek1 = 17448, // Boss->self, 5.0s cast, range 60 180-degree cone
    RightCheek2 = 17449, // Boss->self, no cast, range 60 180-degree cone
    TerrifyingGlance = 17955, // Boss->self, 3.0s cast, range 50 circle, gaze
    TheStake = 17443, // Boss->self, 4.0s cast, range 18 circle
    SecondCircle = 17441, // Boss->self, 3.0s cast, range 40 width 8 rect
    CleansingFire = 17442, // Boss->self, 4.0s cast, range 40 circle
    FeveredFlagellation = 17440, // Boss->players, 4.0s cast, range 15 90-degree cone, tankbuster
    SanctifiedShock = 17900, // Boss->player, no cast, single-target, stuns target before WitchHunt
    WitchHunt = 17444, // Boss->players, 3.0s cast, width 10 rect charge
    WitchHunt2 = 17445, // Boss->players, no cast, width 10 rect charge, targets main tank
}

class LeftRightCheek(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(60, 90.Degrees());
    private readonly List<AOEInstance> _aoes = new(2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        List<AOEInstance> aoes = new(count);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes.Add(count > 1 ? aoe with { Color = Colors.Danger } : aoe);
            else if (i == 1)
                aoes.Add(aoe with { Risky = false });
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LeftCheek1 or AID.RightCheek1)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 3.1f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID is AID.LeftCheek1 or AID.RightCheek1)
            _aoes.RemoveAt(0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID is AID.LeftCheek2 or AID.RightCheek2)
            _aoes.RemoveAt(0);
    }
}

class TerrifyingGlance(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.TerrifyingGlance));
class TheStake(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TheStake), 18);
class SecondCircle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SecondCircle), new AOEShapeRect(40, 4));
class CleansingFire(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CleansingFire));

class FeveredFlagellation(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.FeveredFlagellation), new AOEShapeCone(15, 45.Degrees()))
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { }
    public override void OnEventCast(Actor caster, ActorCastEvent spell) //tankbuster resolves on cast event, which can be delayed by moving out of tankbuster range
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class WitchHunt(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(default, 5);
    private bool witchHunt1done;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SanctifiedShock)
            CurrentBaits.Add(new(Module.PrimaryActor, WorldState.Actors.Find(spell.MainTargetID)!, rect));
        else if ((AID)spell.Action.ID == AID.WitchHunt2)
        {
            CurrentBaits.Clear();
            witchHunt1done = false;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WitchHunt)
        {
            CurrentBaits.Clear();
            CurrentBaits.Add(new(Module.PrimaryActor, WorldState.Actors.Find(Module.PrimaryActor.TargetID)!, rect));
        }
    }

    public override void Update()
    {
        if (CurrentBaits.Count == 0)
            return;
        for (var i = 0; i < CurrentBaits.Count; ++i)
        {
            var b = CurrentBaits[i];
            CurrentBaits[i] = b with { Shape = rect with { LengthFront = (b.Target.Position - b.Source.Position).Length() } };
        }

        if (witchHunt1done) //updating WitchHunt2 target incase of sudden tank swap
            CurrentBaits[0] = CurrentBaits[0] with { Target = WorldState.Actors.Find(Module.PrimaryActor.TargetID)! };
    }
}

class ForgivenPedantryStates : StateMachineBuilder
{
    public ForgivenPedantryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LeftRightCheek>()
            .ActivateOnEnter<TerrifyingGlance>()
            .ActivateOnEnter<TheStake>()
            .ActivateOnEnter<SecondCircle>()
            .ActivateOnEnter<CleansingFire>()
            .ActivateOnEnter<FeveredFlagellation>()
            .ActivateOnEnter<WitchHunt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8910)]
public class ForgivenPedantry(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
