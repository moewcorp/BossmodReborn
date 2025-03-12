namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN3QueensGuard;

class OptimalPlaySword(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OptimalPlaySword), 10f);
class OptimalPlayShield(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OptimalPlayShield), new AOEShapeDonut(5f, 60f));

class PawnOff(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PawnOffReal), 20);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Knight, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9838)]
public class DRN3QueensGuard : BossModule
{
    private readonly List<Actor> _warrior;
    private readonly List<Actor> _soldier;
    private readonly List<Actor> _gunner;

    public Actor? Knight() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Warrior() => _warrior.Count > 0 ? _warrior[0] : null;
    public Actor? Soldier() => _soldier.Count > 0 ? _soldier[0] : null;
    public Actor? Gunner() => _gunner.Count > 0 ? _gunner[0] : null;

    public DRN3QueensGuard(WorldState ws, Actor primary) : base(ws, primary, new(244f, -162f), new ArenaBoundsCircle(25f))
    {
        _warrior = Enemies((uint)OID.Warrior);
        _soldier = Enemies((uint)OID.Soldier);
        _gunner = Enemies((uint)OID.Gunner);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(Knight());
        Arena.Actor(Warrior());
        Arena.Actor(Soldier());
        Arena.Actor(Gunner());
        Arena.Actors(Enemies((uint)OID.GunTurret));
    }
}
