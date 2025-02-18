namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D121Lyngbakr;

public enum OID : uint
{
    Boss = 0x3EEB, //R=7.6
    SmallCrystal = 0x1EB882, // R=0.5
    BigCrystal = 0x1EB883, // R=0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 34517, // Boss->player, no cast, single-target

    BodySlam = 33335, // Boss->self, 3.0s cast, range 40 circle
    SonicBloop = 33345, // Boss->player, 5.0s cast, single-target, tankbuster
    ExplosiveFrequency = 33340, // Helper->self, 10.0s cast, range 15 circle
    ResonantFrequency = 33339, // Helper->self, 5.0s cast, range 8 circle
    TidalBreath = 33344, // Boss->self, 5.0s cast, range 40 180-degree cone
    Tidalspout = 33343, // Helper->player, 5.0s cast, range 6 circle
    Upsweep = 33338, // Boss->self, 5.0s cast, range 40 circle
    Floodstide = 33341, // Boss->self, 3.0s cast, single-target        
    Waterspout = 33342 // Helper->player, 5.0s cast, range 5 circle, spread
}

class ExplosiveResonantFrequency(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circleSmall = new(8), circleBig = new(15);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        List<AOEInstance> aoes = new(count);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if ((aoe.Activation - _aoes[0].Activation).TotalSeconds <= 1)
                aoes.Add(aoe);
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ResonantFrequency)
            _aoes.Add(new(circleSmall, spell.LocXZ, default, Module.CastFinishAt(spell), ActorID: caster.InstanceID));
        else if ((AID)spell.Action.ID == AID.ExplosiveFrequency)
            _aoes.Add(new(circleBig, spell.LocXZ, default, Module.CastFinishAt(spell), ActorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ResonantFrequency or AID.ExplosiveFrequency)
        {
            for (var i = 0; i < _aoes.Count; ++i)
            {
                var aoe = _aoes[i];
                if (aoe.ActorID == caster.InstanceID)
                {
                    _aoes.Remove(aoe);
                    break;
                }
            }
        }
    }
}

class SonicBloop(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.SonicBloop));
class Waterspout(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Waterspout), 5);
class TidalBreath(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TidalBreath), new AOEShapeCone(40, 90.Degrees()));
class Tidalspout(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Tidalspout), 6, 4, 4);
class Upsweep(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Upsweep));
class BodySlam(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BodySlam));

class D121LyngbakrStates : StateMachineBuilder
{
    public D121LyngbakrStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SonicBloop>()
            .ActivateOnEnter<TidalBreath>()
            .ActivateOnEnter<Tidalspout>()
            .ActivateOnEnter<Waterspout>()
            .ActivateOnEnter<Upsweep>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<ExplosiveResonantFrequency>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12336, SortOrder = 3)]
public class D121Lyngbakr(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-322, 120), 19.5f * CosPI.Pi40th, 48)], [new Rectangle(new(-322, 99), 20, 2.25f),
    new Rectangle(new(-322, 140), 20, 1.25f)]);
}
