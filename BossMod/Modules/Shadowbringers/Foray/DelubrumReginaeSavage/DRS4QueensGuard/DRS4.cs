namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

class OptimalPlaySword(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OptimalPlaySword), 10f);
class OptimalPlayShield(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OptimalPlayShield), new AOEShapeDonut(5f, 60f));
class OptimalPlayCone(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OptimalPlayCone), new AOEShapeCone(60f, 135f.Degrees()));

// note: apparently there is no 'front unseen' status
class QueensShotUnseen(BossModule module) : Components.CastWeakpoint(module, ActionID.MakeSpell(AID.QueensShotUnseen), new AOEShapeCircle(60f), 0, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);
class TurretsTourUnseen(BossModule module) : Components.CastWeakpoint(module, ActionID.MakeSpell(AID.TurretsTourUnseen), new AOEShapeRect(50f, 2.5f), 0, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);

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
    public Actor? Warrior() => _warrior.Count > 0 ? _warrior[0] : null;
    public Actor? Soldier() => _soldier.Count > 0 ? _soldier[0] : null;
    public Actor? Gunner() => _gunner.Count > 0 ? _gunner[0] : null;
    public readonly List<Actor> GunTurrets;
    public readonly List<Actor> AuraSpheres;
    public readonly List<Actor> SpiritualSpheres;

    public DRS4(WorldState ws, Actor primary) : base(ws, primary, new(244f, -162f), new ArenaBoundsCircle(25f))
    {
        _warrior = Enemies((uint)OID.Warrior);
        _soldier = Enemies((uint)OID.Soldier);
        _gunner = Enemies((uint)OID.Gunner);
        GunTurrets = Enemies((uint)OID.GunTurret);
        AuraSpheres = Enemies((uint)OID.AuraSphere);
        SpiritualSpheres = Enemies((uint)OID.SpiritualSphere);
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
