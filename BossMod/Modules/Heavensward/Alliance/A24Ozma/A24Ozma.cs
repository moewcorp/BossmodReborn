﻿namespace BossMod.Heavensward.Alliance.A24Ozma;

class MeteorImpact(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MeteorImpact), 20);
class HolyKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Holy), 3);
class Holy(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Holy));
class ExecrationAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ExecrationAOE), new AOEShapeRect(40.5f, 5));

class AccelerationBomb(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4896)]
public class A24Ozma(WorldState ws, Actor primary) : BossModule(ws, primary, new(280, -410), arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Donut(new(280, -410), 18, 25), new Rectangle(new(280, -386), 5, 12), new Rectangle(new(260, -422), 5, 12, -120.Degrees()),
    new Rectangle(new(300, -422), 5, 12, 120.Degrees())]);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.SingularityFragment));
        Arena.Actors(Enemies(OID.SingularityEcho));
        Arena.Actors(Enemies(OID.SingularityRipple));
        Arena.Actors(Enemies(OID.Ozmasphere));
        Arena.Actors(Enemies(OID.Ozmashade));
    }
}
