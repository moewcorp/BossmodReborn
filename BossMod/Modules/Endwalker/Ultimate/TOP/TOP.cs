﻿namespace BossMod.Endwalker.Ultimate.TOP;

class SolarRayM(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.SolarRayM), new AOEShapeCircle(5f), true);
class SolarRayF(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.SolarRayF), new AOEShapeCircle(5f), true);
class P4BlueScreen(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BlueScreenAOE));
class P5BlindFaith(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.BlindFaithSuccess), "Intermission");

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 908, PlanLevel = 90)]
public class TOP(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    private Actor? _opticalUnit;
    private Actor? _omegaM;
    private Actor? _omegaF;
    private Actor? _bossP3;
    private Actor? _bossP5;
    private Actor? _bossP6;
    public Actor? BossP1() => PrimaryActor;
    public Actor? OpticalUnit() => _opticalUnit; // we use this to distinguish P1 wipe vs P1 kill - primary actor can be destroyed before P2 bosses spawn
    public Actor? BossP2M() => _omegaM;
    public Actor? BossP2F() => _omegaF;
    public Actor? BossP3() => _bossP3;
    public Actor? BossP5() => _bossP5;
    public Actor? BossP6() => _bossP6;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _opticalUnit ??= StateMachine.ActivePhaseIndex == 0 ? Enemies((uint)OID.OpticalUnit)[0] : null;
        _omegaM ??= StateMachine.ActivePhaseIndex == 1 ? Enemies((uint)OID.OmegaM)[0] : null;
        _omegaF ??= StateMachine.ActivePhaseIndex == 1 ? Enemies((uint)OID.OmegaF)[0] : null;
        _bossP3 ??= StateMachine.ActivePhaseIndex == 2 ? Enemies((uint)OID.BossP3)[0] : null;
        _bossP5 ??= StateMachine.ActivePhaseIndex == 4 ? Enemies((uint)OID.BossP5)[0] : null;
        _bossP6 ??= StateMachine.ActivePhaseIndex >= 4 ? Enemies((uint)OID.BossP6)[0] : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_omegaM);
        Arena.Actor(_omegaF);
        Arena.Actor(_bossP3);
        Arena.Actor(_bossP5);
        Arena.Actor(_bossP6);
    }
}
