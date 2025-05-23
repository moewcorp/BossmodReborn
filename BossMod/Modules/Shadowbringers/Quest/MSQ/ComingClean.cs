﻿using BossMod.QuestBattle;

namespace BossMod.Shadowbringers.Quest.MSQ.ComingClean; // quest name: Full Steam Ahead

public enum OID : uint
{
    Boss = 0x295C, // R1.0
    Ranjit = 0x295D, // R1.0
    SerpentHead = 0x295F, // R1.0
    LightningVoidzone = 0x1E9685, // R0.5
    Helper = 0x233C, // R0.5
}

public enum AID : uint
{
    ShatteredSky = 16405, // Boss->self, 5.0s cast, single-target
    ShatteredSky1 = 16429, // 233C->self, 6.0s cast, range 45 circle
    HotPursuit = 16406, // Boss->self, 3.0s cast, single-target
    HotPursuit1 = 16430, // 233C->location, 3.0s cast, range 5 circle
    NexusOfThunderVisual = 16404, // Boss->self, 3.0s cast, single-target
    NexusOfThunder = 16427, // 233C->self, 7.0s cast, range 60+R width 5 rect
    Wrath = 16425, // 295E->self, no cast, range 100 circle
    CoiledLevin = 16424, // 295E->self, 3.0s cast, single-target
    CoiledLevin1 = 16428, // 233C->self, 7.0s cast, range 6 circle
    UnbridledWrath = 16426, // 295E->self, no cast, range 100 circle
    HiddenCurrent = 16403, // Boss->location, no cast, ???
    VeilOfGukumatz1 = 16423, // 2998->self, no cast, single-target
    VeilOfGukumatz2 = 16422, // 295D->self, no cast, single-target
    VeilOfGukumatz3 = 16402, // Boss->self, no cast, single-target
    UnceremoniousBeheading = 16412, // 295D->self, 3.5s cast, range 10 circle
    HiddenCurrent1 = 16411, // 295D->location, no cast, ???
    MercilessLeftVisual = 16415, // 295D->self, 4.0s cast, single-target
    MercilessLeft = 33202, // 233C->self, 4.0s cast, range 40 120-degree cone
    MercilessRight = 16431, // 233C->self, 4.0s cast, range 40 120-degree cone
    KatunCycle = 16413, // 295D->self, 5.5s cast, range 5-40 donut
    HotPursuit2 = 16410, // 295D->self, 3.0s cast, single-target
    AgelessSerpent = 16417, // 295D->self, no cast, single-target
    SerpentRising = 16433, // 295F->self, no cast, single-target
    Evisceration = 16419, // 295D->self, 2.0s cast, range 40 120-degree cone
    Spiritcall = 16420, // 295D->self, no cast, range 100 circle
    SnakingFlame = 16432, // 295F->player, 40.0s cast, width 4 rect charge
}

public enum SID : uint
{
    Smackdown = 2068,
    DownForTheCount = 783 // 295E->player, extra=0xEC7
}

class KatunCycle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.KatunCycle), new AOEShapeDonut(5, 40));

abstract class Cleaves(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40, 60.Degrees()));
class MercilessLeft(BossModule module) : Cleaves(module, AID.MercilessLeft);
class MercilessRight(BossModule module) : Cleaves(module, AID.MercilessRight);
class Evisceration(BossModule module) : Cleaves(module, AID.Evisceration);

class UnceremoniousBeheading(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.UnceremoniousBeheading), 10);
class HotPursuit(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HotPursuit1), 5);
class NexusOfThunder(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.NexusOfThunder), new AOEShapeRect(60, 2.5f));
class CoiledLevin(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CoiledLevin1), 6);
class LightningVoidzone(BossModule module) : Components.Voidzone(module, 6, m => m.Enemies(OID.LightningVoidzone).Where(x => x.EventState != 7));

class ThancredAI(BossModule module) : RotationModule<AutoThancred>(module);

class AutoThancred(WorldState ws) : UnmanagedRotation(ws, 3)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (World.Client.DutyActions[0].CurCharges > 0)
        {
            UseAction(World.Client.DutyActions[0].Action, primaryTarget);
            return;
        }

        if (primaryTarget == null)
            return;

        var distance = Player.DistanceToHitbox(primaryTarget);

        if (distance <= 3)
        {
            UseAction(Roleplay.AID.Smackdown, Player, -100);

            if (Player.FindStatus(SID.Smackdown) != null)
                UseAction(Roleplay.AID.RoughDivide, primaryTarget, -100);
        }

        if (Player.HPMP.CurHP * 2 < Player.HPMP.MaxHP)
            UseAction(Roleplay.AID.SoothingPotion, Player, -100);

        switch (ComboAction)
        {
            case Roleplay.AID.BrutalShell:
                UseAction(Roleplay.AID.SolidBarrel, primaryTarget);
                break;
            case Roleplay.AID.KeenEdge:
                UseAction(Roleplay.AID.BrutalShell, primaryTarget);
                break;
            default:
                UseAction(Roleplay.AID.KeenEdge, primaryTarget);
                break;
        }
    }
}

class RanjitStates : StateMachineBuilder
{
    public RanjitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HotPursuit>()
            .ActivateOnEnter<ThancredAI>()
            .ActivateOnEnter<NexusOfThunder>()
            .ActivateOnEnter<CoiledLevin>()
            .ActivateOnEnter<LightningVoidzone>()
            .ActivateOnEnter<KatunCycle>()
            .ActivateOnEnter<MercilessLeft>()
            .ActivateOnEnter<MercilessRight>()
            .ActivateOnEnter<UnceremoniousBeheading>()
            .ActivateOnEnter<Evisceration>()
            .Raw.Update = () => module.Enemies(OID.Ranjit) is var boss && boss.Count != 0 && boss[0].FindStatus(SID.DownForTheCount) != null || module.WorldState.CurrentCFCID != 680;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69155, NameID = 8374)]
public class Ranjit(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-203, 395), 19.5f, 20)]);

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Ranjit));
        Arena.Actor(PrimaryActor);
    }
}
