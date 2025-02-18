﻿namespace BossMod.RealmReborn.Alliance.A31AngraMainyu;

class DoubleVision(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DoubleVision));
class MortalGaze1(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.MortalGaze1));
class Level100Flare1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Level100Flare1), 10);
class Level150Death1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Level150Death1), 10);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3231)]
public class A31AngraMainyu(WorldState ws, Actor primary) : BossModule(ws, primary, new(-145, 300), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.FinalHourglass));
        Arena.Actors(Enemies(OID.GrimReaper));
        Arena.Actors(Enemies(OID.AngraMainyusDaewa));
    }
}
