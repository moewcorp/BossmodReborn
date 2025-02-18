namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN3QueensGuard;

class OptimalPlaySword(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OptimalPlaySword), 10);
class OptimalPlayShield(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OptimalPlayShield), new AOEShapeDonut(5, 60));

class PawnOff(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PawnOffReal), 20);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Knight, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9838)]
public class DRN3QueensGuard : BossModule
{
    private readonly List<Actor> _warrior;
    private readonly List<Actor> _soldier;
    private readonly List<Actor> _gunner;

    public Actor? Knight() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Warrior() => _warrior.FirstOrDefault();
    public Actor? Soldier() => _soldier.FirstOrDefault();
    public Actor? Gunner() => _gunner.FirstOrDefault();
    public readonly List<Actor> GunTurrets;

    public DRN3QueensGuard(WorldState ws, Actor primary) : base(ws, primary, new(244, -162), new ArenaBoundsCircle(25))
    {
        _warrior = Enemies(OID.Warrior);
        _soldier = Enemies(OID.Soldier);
        _gunner = Enemies(OID.Gunner);
        GunTurrets = Enemies(OID.GunTurret);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(Knight());
        Arena.Actor(Warrior());
        Arena.Actor(Soldier());
        Arena.Actor(Gunner());
        Arena.Actors(GunTurrets);
    }
}
