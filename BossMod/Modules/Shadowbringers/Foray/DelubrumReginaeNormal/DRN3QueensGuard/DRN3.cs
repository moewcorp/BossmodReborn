namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN3QueensGuard;

class OptimalPlaySword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlaySword, 10f);
class OptimalPlayShield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlayShield, new AOEShapeDonut(5f, 60f));
class BloodAndBoneQueenShotUnseen(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.BloodAndBoneKnight, (uint)AID.BloodAndBoneSoldier, (uint)AID.BloodAndBoneWarrior, (uint)AID.QueensShotUnseen]);
class RapidSeverShotInTheDark(BossModule module) : Components.SingleTargetCasts(module, [(uint)AID.RapidSeverKnight, (uint)AID.RapidSeverSoldier, (uint)AID.RapidSeverWarrior, (uint)AID.ShotInTheDark]);
class Enrages(BossModule module) : Components.CastHints(module, [(uint)AID.BloodAndBoneKnightEnrage, (uint)AID.BloodAndBoneSoldierEnrage, (uint)AID.BloodAndBoneWarriorEnrage, (uint)AID.QueensShotEnrage], "Enrage!", true);

class PawnOff(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PawnOffReal, 20f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.Knight, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9838)]
public class DRN3QueensGuard : QueensGuard
{
    private readonly Actor? _warrior;
    private readonly Actor? _soldier;
    private readonly Actor? _gunner;

    public DRN3QueensGuard(WorldState ws, Actor primary) : base(ws, primary)
    {
        _warrior = Enemies((uint)OID.Warrior) is var warrior && warrior.Count != 0 ? warrior[0] : null;
        _soldier = Enemies((uint)OID.Soldier) is var soldier && soldier.Count != 0 ? soldier[0] : null;
        _gunner = Enemies((uint)OID.Gunner) is var gunner && gunner.Count != 0 ? gunner[0] : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_warrior);
        Arena.Actor(_soldier);
        Arena.Actor(_gunner);
    }
}
