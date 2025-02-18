namespace BossMod.Global.MaskedCarnivale.Stage28;

public enum OID : uint
{
    Boss = 0x2CD2, //R=2.6
    UndeadSerf1 = 0x2CD4, // R=0.5
    UndeadSerf2 = 0x2CD3, // R=0.5
    UndeadSoldier = 0x2CD5, // R=0.5
    UndeadWarrior = 0x2CD6, // R=1.9
    UndeadGravekeeper = 0x2CD7, // R=0.75
    NecrobaneVoidzone = 0x1EA9FA,
    MagitekExplosive = 0x2CEC, //R=0.8
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 19052, // Boss->player, no cast, single-target
    AutoAttack2 = 6499, // UndeadSerf1/UndeadWarrior/UndeadGravekeeper->player, no cast, single-target
    AutoAttack3 = 19068, // UndeadSoldier->player, no cast, single-target

    DoomImpending = 19051, // Boss->self, 8.0s cast, range 80 circle, heal to full before cast ends
    MarchOfTheDraugar = 19057, // Boss->self, 3.0s cast, single-target, summons 4 different sets of adds
    DeathThroes = 19061, // UndeadSerf2->player, no cast, single-target, pull 20 between hitboxes, player becomes unable to move
    DeathThroesRecover = 19062, // UndeadSerf2->self, no cast, range 100 circle
    Necrobane = 19059, // Boss->location, 4.0s cast, range 6 circle
    MegaDeath = 19055, // Boss->self, 6.0s cast, range 80 circle, deadly if not in voidzone
    HelblarShriek = 19084, // Boss->self, 4.0s cast, range 50 circle, high dmg if in Necrobane voidzone
    FireIII = 19069, // UndeadSoldier->player, 5.0s cast, single-target
    FuneralPyre = 19056, // Boss->self, 6.0s cast, range 40 circle, deadly if in Necrobane voidzone
    ButterflyFloat = 19065, // UndeadWarrior->player, 3.0s cast, single-target
    GlassPunch = 19063, // UndeadWarrior->self, no cast, range 6+R 90-degree cone
    Brainstorm = 19054, // Boss->self, 4.0s cast, range 60 circle, forced march debuffs
    BilrostSquallVisual = 19081, // Boss->self, 9.0s cast, single-target
    BilrostSquall = 19082, // Helper->location, 9.0s cast, range 10 circle
    NailInTheCoffin = 19066, // UndeadGravekeeper->player, no cast, single-target
    VengefulSoul = 19067, // UndeadGravekeeper->location, 3.0s cast, range 6 circle
    Cackle = 19053, // Boss->player, 4.0s cast, single-target, high dmg, interruptible
    Catapult = 19064 // UndeadWarrior->location, 3.0s cast, range 6 circle
}

public enum SID : uint
{
    RightFace = 1961, // Boss->player, extra=0x0
    LeftFace = 1960, // Boss->player, extra=0x0
    ForwardMarch = 1958, // Boss->player, extra=0x0
    AboutFace = 1959 // Boss->player, extra=0x0
}

class DoomImpending(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.DoomImpending), "Heal to full before cast ends!");
class MarchOfTheDraugar(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.MarchOfTheDraugar), "Summons adds! (Kill with fire!)");
class NecrobaneVoidzone(BossModule module) : Components.PersistentInvertibleVoidzoneByCast(module, 6, m => m.Enemies(OID.NecrobaneVoidzone).Where(z => z.EventState != 7), ActionID.MakeSpell(AID.MegaDeath));
class Necrobane(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Necrobane), 6);
class HelblarShriek(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HelblarShriek));
class FuneralPyre(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FuneralPyre));
class Catapult(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Catapult), 6);
class VengefulSoul(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.VengefulSoul), 6);
class BilrostSquall(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BilrostSquall), 10);
class Cackle(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.Cackle));

class Brainstorm(BossModule module) : Components.StatusDrivenForcedMarch(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<BilrostSquall>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will spawn upto 4 waves of adds which are weak to fire.\nA way to quickly heal yourself to full is mandatory and a ranged fire\nability such as Mustard Bomb and Flying Sardine for interrupts\nare highly recommended.");
    }
}

class Stage28States : StateMachineBuilder
{
    public Stage28States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Brainstorm>()
            .ActivateOnEnter<Cackle>()
            .ActivateOnEnter<BilrostSquall>()
            .ActivateOnEnter<VengefulSoul>()
            .ActivateOnEnter<Catapult>()
            .ActivateOnEnter<FuneralPyre>()
            .ActivateOnEnter<HelblarShriek>()
            .ActivateOnEnter<Necrobane>()
            .ActivateOnEnter<NecrobaneVoidzone>()
            .ActivateOnEnter<MarchOfTheDraugar>()
            .ActivateOnEnter<DoomImpending>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 697, NameID = 9233)]
public class Stage28 : BossModule
{
    public Stage28(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }
    private static readonly uint[] adds = [(uint)OID.UndeadSerf1, (uint)OID.UndeadSerf2, (uint)OID.UndeadGravekeeper, (uint)OID.UndeadSoldier, (uint)OID.UndeadWarrior];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(adds));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.UndeadSerf1 or OID.UndeadSerf2 or OID.UndeadSoldier or OID.UndeadGravekeeper or OID.UndeadWarrior => 1,
                _ => 0
            };
        }
    }
}
