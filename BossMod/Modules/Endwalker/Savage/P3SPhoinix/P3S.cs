﻿namespace BossMod.Endwalker.Savage.P3SPhoinix;

class HeatOfCondemnation(BossModule module) : Components.TankbusterTether(module, (uint)AID.HeatOfCondemnationAOE, (uint)TetherID.HeatOfCondemnation, 6);
class TrailOfCondemnationAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TrailOfCondemnationAOE, new AOEShapeRect(40, 7.5f));
class SearingBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SearingBreezeAOE, 6);

abstract class Cinderwing(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(60, 90.Degrees()));
class LeftCinderwing(BossModule module) : Cinderwing(module, (uint)AID.LeftCinderwing);
class RightCinderwing(BossModule module) : Cinderwing(module, (uint)AID.RightCinderwing);

class DevouringBrand(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCross cross = new(40, 5);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DevouringBrandAOE)
            _aoe = new(cross, spell.LocXZ, default, Module.CastFinishAt(spell, 2.2f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00080004)
            _aoe = null;
    }
}

class SunBirdLarge(BossModule module) : Components.Adds(module, (uint)OID.SunbirdLarge)
{
    public int FinishedTethers;
    public override void Update()
    {
        var comp = Module.FindComponent<BirdTether>();
        if (comp != null)
            FinishedTethers = comp.NumFinishedChains;
    }
}

class SunBirdSmall(BossModule module) : Components.Adds(module, (uint)OID.SunbirdSmall);
class DarkenedFireAdd(BossModule module) : Components.Adds(module, (uint)OID.DarkenedFire);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 807, NameID = 10720, PlanLevel = 90)]
public class P3S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
