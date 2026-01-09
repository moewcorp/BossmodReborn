using System.ComponentModel.Design.Serialization;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;

namespace BossMod.Stormblood.Raid.O1NAlteRoite;
public enum OID : uint
{
    Boss = 0x1A6F,
    Helper = 0x233C,
    BallOfFire = 0x1A71, // R1.000, x0 (spawn during fight)
}
class Adds(BossModule module) : Components.Adds(module, (uint)OID.BallOfFire);
public enum AID : uint
{
    Attack = 872, // Boss->player, no cast, single-target
    WyrmTail = 9174, // Boss->player, no cast, single-target
    Flame = 9181, // Boss->self, no cast, ???
    Roar = 9180, // Boss->self, 4.0s cast, range 100 circle
    Burn = 9173, // 1A71->self, 1.0s cast, range 8 circle
    BreathWing = 9182, // Boss->self, 5.0s cast, single-target
    TwinBolt = 9175, // Boss->self, 5.0s cast, single-target
    Clamp = 9186, // Boss->self, 3.0s cast, range 9+R width 10 rect
    FlashFreeze = 9183, // Boss->self, no cast, single-target
    Levinbolt = 9177, // Boss->self, 5.0s cast, single-target
    Blaze = 9185, // Boss->players, 5.0s cast, range 6 circle
    TheClassicalElements = 9184, // Boss->self, 5.0s cast, single-target
    Downburst = 7896, // Boss->self, 5.0s cast, single-target
}
class Roar(BossModule module) : Components.RaidwideCast(module, (uint)AID.Roar);
class Blaze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Blaze, new AOEShapeCircle(6f));
class Burn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burn, new AOEShapeCircle(8f));
sealed class BreathWing(BossModule module) : Components.GenericKnockback(module, (uint)AID.BreathWing)
{
    private DateTime _activation;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
        => _activation != default ? new Knockback[1] { new(Arena.Center, 10f, _activation, kind: Kind.TowardsOrigin) } : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BreathWing)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BreathWing)
            _activation = default;
    }
}


public enum SID : uint
{
    ThinIce = 911, // none->player, extra=0x96
}

public enum TetherID : uint
{
    Tether_chn_raikou1s = 21, // player->player
}

sealed class O1NAlteRoiteStates : StateMachineBuilder
{
    public O1NAlteRoiteStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<Blaze>()
            .ActivateOnEnter<BreathWing>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
IconIDType = null, // replace null with typeof(IconID) if applicable
Contributors = "JoeSparkx",
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 252,
NameID = 5629)]
public class O1NAlteRoite(WorldState ws, Actor primary) : BossModule(ws, primary, new(00, 00), new ArenaBoundsCircle(20));

