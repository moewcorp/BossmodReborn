using BossMod.Dawntrail.Quest.MSQ.TheProtectorAndTheDestroyer.Otis;

namespace BossMod.Dawntrail.Quest.MSQ.TheProtectorAndTheDestroyer.Gwyddrud;

public enum OID : uint
{
    Boss = 0x4349, // R5.0    
    LimitBreakHelper = 0x40B5, // R1.0
    BallOfLevin = 0x434A, // R1.5
    SuperchargedLevin = 0x39C4, // R2.0
    Helper2 = 0x3A5E, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player/WukLamat, no cast, single-target

    CracklingHowlVisual = 38211, // Boss->self, 4.3+0,7s cast, single-target
    CracklingHowl = 38212, // Helper->self, 5.0s cast, range 40 circle
    Teleport = 38213, // Boss->location, no cast, single-target
    VioletVoltageTelegraph = 38220, // Helper->self, 2.5s cast, range 20 180-degree cone
    VioletVoltage = 38221, // Helper->self, no cast, range 20 180-degree cone
    VioletVoltageVisual1 = 38214, // Boss->self, 8.3+0,7s cast, single-target
    VioletVoltageVisual2 = 38216, // Boss->self, no cast, single-target
    VioletVoltageVisual3 = 38217, // Boss->self, no cast, single-target
    VioletVoltageVisual4 = 38215, // Boss->self, 10.3+0,7s cast, single-target
    VioletVoltageVisual5 = 38218, // Boss->self, no cast, single-target
    VioletVoltageVisual6 = 38219, // Boss->self, no cast, single-target
    Gnaw = 38222, // Boss->tank, 5.0s cast, single-target
    RollingThunderVisual = 38223, // Boss->self, 4.3+0,7s cast, single-target
    RollingThunder = 38224, // Helper->self, 5.0s cast, range 20 45-degree cone
    RoaringBoltKB = 38230, // Boss->self, 4.3+0,7s cast, range 20 circle, knockback 12, away from source
    RoaringBolt = 38231, // Helper->location, 7.0s cast, range 6 circle
    GatheringStorm = 38225, // Boss->self, no cast, single-target, limit break phase
    LevinStartMoving = 38226, // BallOfLevin/SuperchargedLevin->LimitBreakHelper, no cast, single-target
    GatheringSurge = 38243, // Boss->self, no cast, single-target
    UntamedCurrent = 38232, // Boss->self, 3.3+0,7s cast, range 40 circle, knockback 15, away from source
    UntamedCurrentAOE1 = 38233, // Helper2->location, 3.1s cast, range 5 circle
    UntamedCurrentAOE2 = 19718, // Helper2->location, 3.2s cast, range 5 circle
    UntamedCurrentAOE3 = 19999, // Helper2->location, 3.0s cast, range 5 circle
    UntamedCurrentAOE4 = 38234, // Helper2->location, 3.1s cast, range 5 circle
    UntamedCurrentAOE5 = 19719, // Helper2->location, 3.3s cast, range 5 circle
    UntamedCurrentAOE6 = 19720, // Helper2->location, 3.2s cast, range 5 circle
    UntamedCurrentAOE7 = 19721, // Helper2->location, 3.3s cast, range 5 circle
    UntamedCurrentAOE8 = 19727, // Helper2->location, 3.2s cast, range 5 circle (outside arena)
    UntamedCurrentAOE9 = 19728, // Helper2->location, 3.3s cast, range 5 circle (outside arena)
    UntamedCurrentAOE10 = 19179, // Helper2->location, 3.1s cast, range 5 circle (outside arena)
    UntamedCurrentSpread = 19181, // Helper->all, 5.0s cast, range 5 circle
    UntamedCurrentStack = 19276, // Helper->Alisaie, 5.0s cast, range 6 circle
}

class Gnaw(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Gnaw);
class CracklingHowl(BossModule module) : Components.RaidwideCast(module, (uint)AID.CracklingHowl);
class UntamedCurrentRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.UntamedCurrent);

class VioletVoltage(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCone cone = new(20f, 90f.Degrees());
    private static readonly Angle a180 = 180f.Degrees();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else if (aoes[0].Rotation.AlmostEqual(aoe.Rotation + a180, Angle.DegToRad))
                aoe.Risky = false;
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VioletVoltageTelegraph)
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 6)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.VioletVoltage)
            _aoes.RemoveAt(0);
    }
}

class RoaringBoltKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RoaringBoltKB, 12f, stopAtWall: true)
{
    private readonly RoaringBolt _aoe = module.FindComponent<RoaringBolt>()!;
    private static readonly Angle a25 = 25f.Degrees();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var count = _aoe.Casters.Count;
        for (var i = 0; i < count; ++i)
            if (_aoe.Casters[i].Check(pos))
                return true;
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;
        var aoes = _aoe.Casters;
        var source = Casters[0];
        var count = aoes.Count;
        if (count != 0)
        {
            var forbidden = new List<Func<WPos, float>>(count);
            for (var i = 0; i < count; ++i)
                forbidden.Add(ShapeDistance.Cone(Arena.Center, 20f, Angle.FromDirection(aoes[i].Origin - Arena.Center), a25));
            hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Module.CastFinishAt(source.CastInfo));
        }
    }
}

class RollingThunder : Components.SimpleAOEs
{
    public RollingThunder(BossModule module) : base(module, (uint)AID.RollingThunder, new AOEShapeCone(20f, 22.5f.Degrees()), 6) { MaxDangerColor = 2; }
}

class RoaringBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RoaringBolt, 6f);
class UntamedCurrentSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.UntamedCurrentSpread, 5f);
class UntamedCurrentStack(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.UntamedCurrentStack, 6f, 5, 5);
class UntamedCurrentAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.UntamedCurrentAOE1, (uint)AID.UntamedCurrentAOE2, (uint)AID.UntamedCurrentAOE3,
(uint)AID.UntamedCurrentAOE4, (uint)AID.UntamedCurrentAOE5, (uint)AID.UntamedCurrentAOE6, (uint)AID.UntamedCurrentAOE7,
(uint)AID.UntamedCurrentAOE8, (uint)AID.UntamedCurrentAOE9, (uint)AID.UntamedCurrentAOE10], 5f);

class GwyddrudStates : StateMachineBuilder
{
    public GwyddrudStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CracklingHowl>()
            .ActivateOnEnter<UntamedCurrentRaidwide>()
            .ActivateOnEnter<UntamedCurrentAOEs>()
            .ActivateOnEnter<UntamedCurrentSpread>()
            .ActivateOnEnter<UntamedCurrentStack>()
            .ActivateOnEnter<RoaringBolt>()
            .ActivateOnEnter<RoaringBoltKB>()
            .ActivateOnEnter<RollingThunder>()
            .ActivateOnEnter<VioletVoltage>()
            .ActivateOnEnter<Gnaw>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70478, NameID = 13170)]
public class Gwyddrud(WorldState ws, Actor primary) : BossModule(ws, primary, OtisOathbroken.ArenaCenter, OtisOathbroken.ArenaBounds)
{
    private static readonly uint[] all = [(uint)OID.Boss, (uint)OID.SuperchargedLevin, (uint)OID.BallOfLevin];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(all));
    }
}
