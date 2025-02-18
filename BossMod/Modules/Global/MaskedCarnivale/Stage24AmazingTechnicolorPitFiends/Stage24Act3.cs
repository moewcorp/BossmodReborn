namespace BossMod.Global.MaskedCarnivale.Stage24.Act3;

public enum OID : uint
{
    Boss = 0x2739, //R=3.0
    ArenaMagus = 0x273A, //R=1.0
    VacuumWave = 0x273B //R=1.0
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    PageTear = 15324, // Boss->self, 3.5s cast, range 6+R 90-degree cone
    MagicHammer = 15327, // Boss->location, 3.0s cast, range 8 circle
    GaleCut = 15323, // Boss->self, 3.0s cast, single-target
    HeadDown = 15325, // Boss->player, 5.0s cast, width 8 rect charge, knockback 10, source forward
    VacuumBlade = 15328, // VacuumWave->self, 3.0s cast, range 3 circle
    BoneShaker = 15326, // Boss->self, 3.0s cast, range 50+R circle, raidwide + adds
    Fire = 14266, // ArenaMagus->player, 1.0s cast, single-target
    SelfDetonate = 15329 // ArenaMagus->player, 3.0s cast, single-target
}

class MagicHammer(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MagicHammer), 8);
class PageTear(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PageTear), new AOEShapeCone(8, 45.Degrees()));
class VacuumBlade(BossModule module) : Components.PersistentVoidzone(module, 3, m => m.Enemies(OID.VacuumWave).Where(x => !x.IsDead));

class HeadDown(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.HeadDown), 4);

class HeadDownKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.HeadDown), 10, kind: Kind.DirForward)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<VacuumBlade>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);
}

class BoneShaker(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BoneShaker), "Adds + Raidwide");

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!Module.Enemies(OID.ArenaMagus).All(e => e.IsDead))
            hints.Add($"Kill {Module.Enemies(OID.ArenaMagus).FirstOrDefault()!.Name} fast or wipe!\nUse ranged physical attacks.");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} spawns two adds when casting Boneshaker. These should be a\npriority or they will explode and wipe you. To kill them without touching\nthe electric field use a ranged physical attack such as Fire Angon.\nYou can start the Final Sting combination at about 50% health left.\n(Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

class Stage24Act3States : StateMachineBuilder
{
    public Stage24Act3States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<PageTear>()
            .ActivateOnEnter<MagicHammer>()
            .ActivateOnEnter<VacuumBlade>()
            .ActivateOnEnter<HeadDown>()
            .ActivateOnEnter<HeadDownKB>()
            .ActivateOnEnter<BoneShaker>()
            .ActivateOnEnter<Hints2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 634, NameID = 8125, SortOrder = 3)]
public class Stage24Act3 : BossModule
{
    public Stage24Act3(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.ArenaMagus));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.ArenaMagus => 1, //TODO: ideally Magus should only be attacked with ranged physical abilities
                _ => 0
            };
        }
    }
}
