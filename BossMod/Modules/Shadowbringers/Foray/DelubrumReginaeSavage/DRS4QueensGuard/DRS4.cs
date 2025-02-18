namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

class OptimalPlaySword(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OptimalPlaySword), 10);
class OptimalPlayShield(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OptimalPlayShield), new AOEShapeDonut(5, 60));
class OptimalPlayCone(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OptimalPlayCone), new AOEShapeCone(60, 135.Degrees()));

// note: apparently there is no 'front unseen' status
class QueensShotUnseen(BossModule module) : Components.CastWeakpoint(module, ActionID.MakeSpell(AID.QueensShotUnseen), new AOEShapeCircle(60), 0, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);
class TurretsTourUnseen(BossModule module) : Components.CastWeakpoint(module, ActionID.MakeSpell(AID.TurretsTourUnseen), new AOEShapeRect(50, 2.5f), 0, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);

class FieryPortent(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.FieryPortent), "Stand still!");
class IcyPortent(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.IcyPortent), "Move!");
class PawnOff(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PawnOffReal), 20);
class Fracture(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Fracture)); // TODO: consider showing reflect hints

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.Knight, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9838, PlanLevel = 80)]
public class DRS4 : BossModule
{
    private readonly List<Actor> _warrior;
    private readonly List<Actor> _soldier;
    private readonly List<Actor> _gunner;

    public Actor? Knight() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Warrior() => _warrior.FirstOrDefault();
    public Actor? Soldier() => _soldier.FirstOrDefault();
    public Actor? Gunner() => _gunner.FirstOrDefault();
    public readonly List<Actor> GunTurrets;
    public readonly List<Actor> AuraSpheres;
    public readonly List<Actor> SpiritualSpheres;

    public DRS4(WorldState ws, Actor primary) : base(ws, primary, new(244, -162), new ArenaBoundsCircle(25))
    {
        _warrior = Enemies(OID.Warrior);
        _soldier = Enemies(OID.Soldier);
        _gunner = Enemies(OID.Gunner);
        GunTurrets = Enemies(OID.GunTurret);
        AuraSpheres = Enemies(OID.AuraSphere);
        SpiritualSpheres = Enemies(OID.SpiritualSphere);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(Knight());
        Arena.Actor(Warrior());
        Arena.Actor(Soldier());
        Arena.Actor(Gunner());
        Arena.Actors(GunTurrets);
        Arena.Actors(AuraSpheres);
        Arena.Actors(SpiritualSpheres);
    }
}
