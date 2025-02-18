﻿namespace BossMod.Shadowbringers.Quest.Job.Dancer.SaveTheLastDanceForMe;

public enum OID : uint
{
    Boss = 0x2AC7, // R2.400, x1
    ShadowySpume = 0x2AC8, // R0.800, x0 (spawn during fight)
    ForebodingAura = 0x2ACB, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    Dread = 17476, // Boss->location, 3.0s cast, range 5 circle
    Anguish = 17487, // ->2ACD, 5.5s cast, range 6 circle
    WhelmingLossFirst = 17480, // AethericShadow->self, 5.0s cast, range 5 circle
    WhelmingLossRest = 17481, // AethericShadow1->self, no cast, range 5 circle
    BitterLove = 15650, // 2AC9->self, 3.0s cast, range 12 120-degree cone
}

class Dread(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Dread), 5);
class BitterLove(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BitterLove), new AOEShapeCone(12, 60.Degrees()));
class WhelmingLoss(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(5))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WhelmingLossFirst)
            Lines.Add(new() { Next = spell.LocXZ, Advance = spell.Rotation.ToDirection() * 5, NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1, ExplosionsLeft = 7, MaxShownExplosions = 3 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WhelmingLossFirst or (uint)AID.WhelmingLossRest)
        {
            var index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index < 0)
                return;
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
class Adds(BossModule module) : Components.Adds(module, (uint)OID.ShadowySpume);
class Anguish(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Anguish), 6);

class ForebodingAura(BossModule module) : Components.PersistentVoidzone(module, 8, m => m.Enemies(OID.ForebodingAura).Where(e => !e.IsDead));

class AethericShadowStates : StateMachineBuilder
{
    public AethericShadowStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ForebodingAura>()
            .ActivateOnEnter<Anguish>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<WhelmingLoss>()
            .ActivateOnEnter<Dread>()
            .ActivateOnEnter<BitterLove>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68790, NameID = 8493)]
public class AethericShadow(WorldState ws, Actor primary) : BossModule(ws, primary, new(73.6f, -743.6f), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.FindStatus(DNC.SID.ClosedPosition) == null && Raid.WithoutSlot(false, false).Exclude(actor).FirstOrDefault() is Actor partner)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(DNC.AID.ClosedPosition), partner, ActionQueue.Priority.VeryHigh);
        }
    }
}

