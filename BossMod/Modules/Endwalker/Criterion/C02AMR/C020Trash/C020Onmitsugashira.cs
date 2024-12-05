namespace BossMod.Endwalker.VariantCriterion.C02AMR.C020Trash2;

abstract class Issen(BossModule module, AID aid) : Components.SingleTargetCast(module, ActionID.MakeSpell(aid));
class NIssen(BossModule module) : Issen(module, AID.NIssen);
class SIssen(BossModule module) : Issen(module, AID.SIssen);

abstract class Huton(BossModule module, AID aid) : Components.SingleTargetCast(module, ActionID.MakeSpell(aid), "Cast speed buff");
class NHuton(BossModule module) : Huton(module, AID.NHuton);
class SHuton(BossModule module) : Huton(module, AID.SHuton);

abstract class JujiShuriken(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(40, 1.5f));
class NJujiShuriken(BossModule module) : JujiShuriken(module, AID.NJujiShuriken);
class SJujiShuriken(BossModule module) : JujiShuriken(module, AID.SJujiShuriken);
class NJujiShurikenFast(BossModule module) : JujiShuriken(module, AID.NJujiShurikenFast);
class SJujiShurikenFast(BossModule module) : JujiShuriken(module, AID.SJujiShurikenFast);

class C020OnmitsugashiraStates : StateMachineBuilder
{
    public C020OnmitsugashiraStates(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NIssen>(!savage)
            .ActivateOnEnter<NHuton>(!savage)
            .ActivateOnEnter<NJujiShuriken>(!savage)
            .ActivateOnEnter<NJujiShurikenFast>(!savage)
            .ActivateOnEnter<SIssen>(savage)
            .ActivateOnEnter<SHuton>(savage)
            .ActivateOnEnter<SJujiShuriken>(savage)
            .ActivateOnEnter<SJujiShurikenFast>(savage)
            .ActivateOnEnter<NMountainBreeze>(!savage) // for yamabiko
            .ActivateOnEnter<SMountainBreeze>(savage);
    }
}
class C020NOnmitsugashiraStates(BossModule module) : C020OnmitsugashiraStates(module, false);
class C020SOnmitsugashiraStates(BossModule module) : C020OnmitsugashiraStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NOnmitsugashira, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12424, SortOrder = 5)]
public class C020NOnmitsugashira(WorldState ws, Actor primary) : C020Trash2(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SOnmitsugashira, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12424, SortOrder = 5)]
public class C020SOnmitsugashira(WorldState ws, Actor primary) : C020Trash2(ws, primary);
