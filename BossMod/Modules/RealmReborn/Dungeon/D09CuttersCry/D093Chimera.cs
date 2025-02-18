﻿namespace BossMod.RealmReborn.Dungeon.D09CuttersCry.D093Chimera;

public enum OID : uint
{
    Boss = 0x64C, // R=3.7
    Cacophony = 0x64D, // R=1.0, spawn during fight
    CeruleumSpring = 0x65C, // R=0.5
    IceVoidzone = 0x1E8713 // EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast

    LionsBreath = 1101, // Boss->self, no cast, range 6+R ?-degree cleave
    RamsBreath = 1102, // Boss->self, 2.0s cast, range 6+R 120-degree cone
    DragonsBreath = 1103, // Boss->self, 2.0s cast, range 6+R 120-degree cone
    RamsVoice = 1104, // Boss->self, 3.0s cast, range 6+R circle
    DragonsVoice = 1442, // Boss->self, 4.5s cast, range 8-30 donut
    RamsKeeper = 1106, // Boss->location, 3.0s cast, range 6 voidzone
    Cacophony = 1107, // Boss->self, no cast, visual, summons orb
    ChaoticChorus = 1108 // Cacophony->self, no cast, range 6 aoe
}

class LionsBreath(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.LionsBreath), new AOEShapeCone(9.7f, 60.Degrees())); // TODO: verify angle

abstract class Breath(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(9.7f, 60.Degrees()));
class RamsBreath(BossModule module) : Breath(module, AID.RamsBreath);
class DragonsBreath(BossModule module) : Breath(module, AID.DragonsBreath);

class RamsVoice(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RamsVoice), 9.7f);
class DragonsVoice(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DragonsVoice), new AOEShapeDonut(8, 30));
class RamsKeeper(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.RamsKeeper), m => m.Enemies(OID.IceVoidzone).Where(e => e.EventState != 7), 0.8f);

class ChaoticChorus(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ChaoticChorus))
{
    private readonly AOEShape _shape = new AOEShapeCircle(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: timings
        return Module.Enemies(OID.Cacophony).Where(c => !c.IsDead).Select(c => new AOEInstance(_shape, c.Position, c.Rotation));
    }
}

class D093ChimeraStates : StateMachineBuilder
{
    public D093ChimeraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LionsBreath>()
            .ActivateOnEnter<RamsBreath>()
            .ActivateOnEnter<DragonsBreath>()
            .ActivateOnEnter<RamsVoice>()
            .ActivateOnEnter<DragonsVoice>()
            .ActivateOnEnter<RamsKeeper>()
            .ActivateOnEnter<ChaoticChorus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 12, NameID = 1590)]
public class D093Chimera(WorldState ws, Actor primary) : BossModule(ws, primary, new(-170, -200), new ArenaBoundsCircle(30));
