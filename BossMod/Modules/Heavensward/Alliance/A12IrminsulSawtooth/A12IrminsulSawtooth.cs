﻿namespace BossMod.Heavensward.Alliance.A12IrminsulSawtooth;

class WhiteBreath(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WhiteBreath), new AOEShapeCone(28, 60.Degrees()));
class MeanThrash(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MeanThrash), new AOEShapeCone(12, 60.Degrees()));
class MeanThrashKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.MeanThrash), 10, stopAtWall: true);
class MucusBomb(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.MucusBomb), 10);
class MucusSpray(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MucusSpray2), new AOEShapeDonut(6, 20));
class Rootstorm(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Rootstorm));
class Ambush(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Ambush), 9);
class AmbushKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Ambush), 30, stopAtWall: true, kind: Kind.TowardsOrigin);

class ShockwaveStomp(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.ShockwaveStomp), 70)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.Irminsul).Where(a => !a.IsDead);
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Irminsul, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 120, NameID = 4623)]
public class A12IrminsulSawtooth(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 130), arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Donut(new(0, 130), 8, 35)]);
    private Actor? _sawtooth;

    public Actor? Irminsul() => PrimaryActor;
    public Actor? Sawtooth() => _sawtooth;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _sawtooth ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Sawtooth).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_sawtooth);
        Arena.Actors(Enemies(OID.ArkKed));
    }
}
