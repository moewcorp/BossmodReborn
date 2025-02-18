﻿using BossMod.QuestBattle;

namespace BossMod.Shadowbringers.Quest.Role.TheHuntersLegacy;

public enum OID : uint
{
    Boss = 0x29EE,
    Helper = 0x233C
}

public enum AID : uint
{
    BalamBlaster = 17137, // Boss->self, 4.5s cast, range 30+R 270-degree cone
    BalamBlasterRear = 17138, // Boss->self, 4.5s cast, range 30+R 270-degree cone
    ElectricWhisker = 17126, // Boss->self, 3.5s cast, range 8+R 90-degree cone
    RoaringThunder = 17135, // Boss->self, 4.0s cast, range 8-30 donut
    StreakLightning = 17148, // 233C->location, 2.5s cast, range 3 circle
    AlternatingCurrent1 = 17150, // Helper->self, 4.0s cast, range 60 width 5 rect
    RumblingThunderStack = 17134, // Helper->player, 6.0s cast, range 5 circle
    Thunderbolt1 = 17140, // Helper->players/29EC, 6.0s cast, range 5 circle
    StreakLightning1 = 17147, // Helper->location, 2.5s cast, range 3 circle
}

class Thunderbolt(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Thunderbolt1), 5);

abstract class BB(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(38.05f, 135.Degrees()));
class BalamBlaster(BossModule module) : BB(module, AID.BalamBlaster);
class BalamBlasterRear(BossModule module) : BB(module, AID.BalamBlasterRear);

class ElectricWhisker(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ElectricWhisker), new AOEShapeCone(16.05f, 45.Degrees()));
class RoaringThunder(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RoaringThunder), new AOEShapeDonut(8, 30));
class StreakLightning(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.StreakLightning), 3);
class StreakLightning1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.StreakLightning1), 3);
class AlternatingCurrent(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AlternatingCurrent1), new AOEShapeRect(60, 2.5f));
class RumblingThunder(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.RumblingThunderStack), 5, 1);

class RendaRae(WorldState ws) : UnmanagedRotation(ws, 20)
{
    protected override void Exec(Actor? primaryTarget)
    {
        var dot = StatusDetails(primaryTarget, Roleplay.SID.AcidicBite, Player.InstanceID);
        if (dot.Left < 2.5f)
            UseAction(Roleplay.AID.AcidicBite, primaryTarget, 10);

        UseAction(Roleplay.AID.RadiantArrow, primaryTarget, -5);
        UseAction(Roleplay.AID.HeavyShot, primaryTarget);

        if (primaryTarget?.CastInfo?.Interruptible ?? false)
            UseAction(Roleplay.AID.DullingArrow, primaryTarget, 5);

        if (Player.HPMP.MaxHP * 0.8f > Player.HPMP.CurHP)
            UseAction(Roleplay.AID.HuntersPrudence, Player, -15);
    }
}

class RendaRaeAI(BossModule module) : RotationModule<RendaRae>(module);

class RonkanAura(BossModule module) : BossComponent(module)
{
    private Actor? AuraCenter => Module.Enemies(0x1EADA5).FirstOrDefault();

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (AuraCenter is Actor a)
            Arena.ZoneCircle(a.Position, 10, Colors.SafeFromAOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (AuraCenter is Actor a)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(a.Position, 10), activation: WorldState.FutureTime(5));
    }
}

class BalamQuitzStates : StateMachineBuilder
{
    public BalamQuitzStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RendaRaeAI>()
            .ActivateOnEnter<BalamBlaster>()
            .ActivateOnEnter<BalamBlasterRear>()
            .ActivateOnEnter<ElectricWhisker>()
            .ActivateOnEnter<RoaringThunder>()
            .ActivateOnEnter<StreakLightning>()
            .ActivateOnEnter<StreakLightning1>()
            .ActivateOnEnter<AlternatingCurrent>()
            .ActivateOnEnter<RumblingThunder>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<RonkanAura>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68812, NameID = 8397)]
public class BalamQuitz(WorldState ws, Actor primary) : BossModule(ws, primary, new(-247.11f, 688.33f), new ArenaBoundsCircle(19.5f));
