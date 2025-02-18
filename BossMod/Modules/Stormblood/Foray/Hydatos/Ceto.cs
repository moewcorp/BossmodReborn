﻿namespace BossMod.Stormblood.Foray.Hydatos.Ceto;

public enum OID : uint
{
    Boss = 0x2765, // R5.000, x1
    FaithlessGuard = 0x2767, // R2.000, x0 (spawn during fight)
    LifelessSlave1 = 0x2766, // R2.700, x1
    LifelessSlave2 = 0x2785, // R2.700, x1
    LifelessSlave3 = 0x2784, // R2.700, x1
}

public enum AID : uint
{
    SickleStrike = 15466, // Boss->player, 3.5s cast, single-target
    PetrifactionBoss = 15469, // Boss->self, 450 circle
    AbyssalReaper = 15468, // Boss->self, 4.0s cast, range 18 circle
    PetrifactionAdds = 15475, // LifelessSlave1/LifelessSlave2/LifelessSlave3->self, 4.0s cast, range 50 circle
    CircleOfFlames = 15472, // FaithlessGuard->location, 3.0s cast, range 5 circle
    TailSlap = 15471, // FaithlessGuard->self, 3.0s cast, range 12 120-degree cone
    Petrattraction = 15473, // FaithlessGuard->2783, 3.0s cast, single-target
    CircleBlade = 15470, // FaithlessGuard->self, 3.0s cast, range 7 circle
}

class SickleStrike(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.SickleStrike));
class PetrifactionBoss(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.PetrifactionBoss), range: 50f);
class PetrifactionAdds(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.PetrifactionAdds), range: 50f);
class AbyssalReaper(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AbyssalReaper), 18f);
class CircleOfFlames(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CircleOfFlames), 5f);
class TailSlap(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TailSlap), new AOEShapeCone(12f, 60f.Degrees()));
class Petrattraction(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Petrattraction), 50f, kind: Kind.TowardsOrigin);
class CircleBlade(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CircleBlade), 7);
class Adds(BossModule module) : Components.Adds(module, (uint)OID.FaithlessGuard);

class CetoStates : StateMachineBuilder
{
    public CetoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SickleStrike>()
            .ActivateOnEnter<PetrifactionBoss>()
            .ActivateOnEnter<PetrifactionAdds>()
            .ActivateOnEnter<AbyssalReaper>()
            .ActivateOnEnter<CircleOfFlames>()
            .ActivateOnEnter<TailSlap>()
            .ActivateOnEnter<Petrattraction>()
            .ActivateOnEnter<CircleBlade>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 639, NameID = 7955, Contributors = "xan")]
public class Ceto(WorldState ws, Actor primary) : BossModule(ws, primary, new(747.8959f, -878.8765f), new ArenaBoundsCircle(80f, MapResolution: 1));

