﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class FoeSplitter(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.FoeSplitter), new AOEShapeCone(9, 45.Degrees())); // TODO: verify angle
class ThunderousDischarge(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.ThunderousDischargeAOE));
class ThousandTonzeSwing(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ThousandTonzeSwing), 20);
class Whack(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WhackAOE), new AOEShapeCone(40, 30.Degrees()));
class DevastatingBoltOuter(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DevastatingBoltOuter), new AOEShapeDonut(25, 30));
class DevastatingBoltInner(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DevastatingBoltInner), new AOEShapeDonut(12, 17));
class Electrocution(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Electrocution), 3);

// TODO: ManaFlame component - show reflect hints
[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9759, PlanLevel = 80)]
public class DRS7 : BossModule
{
    private readonly List<Actor> _monks;
    private readonly List<Actor> _ballsEarth;
    private readonly List<Actor> _ballsFire;

    public DRS7(WorldState ws, Actor primary) : base(ws, primary, Border.DefaultBounds.Center, Border.DefaultBounds)
    {
        _monks = Enemies(OID.StygimolochMonk);
        _ballsEarth = Enemies(OID.BallOfEarth);
        _ballsFire = Enemies(OID.BallOfFire);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(_monks);
        Arena.Actors(_ballsEarth, Colors.Object);
        Arena.Actors(_ballsFire, Colors.Object);
    }
}
