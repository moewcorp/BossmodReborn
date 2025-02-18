﻿namespace BossMod.Stormblood.Quest.MSQ.TheMeasureOfHisReach;

public enum OID : uint
{
    Boss = 0x1C48,
    Whitefang = 0x1C5A,
    Helper = 0x233C
}

public enum AID : uint
{
    HowlingIcewind = 8397, // 1C4F->self, 2.5s cast, range 44+R width 4 rect
    Dragonspirit = 8450, // 1C5A/1C5B->self, 3.0s cast, range 6+R circle
    HowlingMoonlight = 8398, // 1C59->self, 7.0s cast, range 22+R circle
    HowlingBloomshower = 8399, // 1C4F->self, 2.5s cast, range 8+R ?-degree cone
}

class Moonlight(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HowlingMoonlight), 10)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // hits everyone (proximity damage)
        foreach (var c in Casters)
            hints.PredictedDamage.Add((Raid.WithSlot(false, false).Mask(), c.Activation));
    }
}
class Icewind(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HowlingIcewind), new AOEShapeRect(44, 2));
class Dragonspirit(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Dragonspirit), 7.5f);
class Bloomshower(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HowlingBloomshower), new AOEShapeDonutSector(4, 8, 45.Degrees()));

class HakuroWhitefangStates : StateMachineBuilder
{
    public HakuroWhitefangStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Icewind>()
            .ActivateOnEnter<Moonlight>()
            .ActivateOnEnter<Dragonspirit>()
            .ActivateOnEnter<Bloomshower>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68088, NameID = 5975)]
public class HakuroWhitefang(WorldState ws, Actor primary) : BossModule(ws, primary, new(504, -133), new ArenaBoundsCircle(20));
