﻿namespace BossMod.RealmReborn.Alliance.A23Amon;

class BlizzagaForte(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BlizzagaForte), 10);
class Darkness(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Darkness), new AOEShapeCone(6, 22.5f.Degrees()));

class CurtainCall(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.CurtainCall), 60)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.IceCage).Where(a => !a.IsDead);
}

class ThundagaForte1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ThundagaForte1), 6);
class ThundagaForte2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ThundagaForte2), 6);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 102, NameID = 2821)]
public class A23Amon(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -200), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.KumKum));
        Arena.Actors(Enemies(OID.Kichiknebik));
        Arena.Actors(Enemies(OID.ExperimentalByProduct66));
        Arena.Actors(Enemies(OID.ExperimentalByProduct33));
    }
}
