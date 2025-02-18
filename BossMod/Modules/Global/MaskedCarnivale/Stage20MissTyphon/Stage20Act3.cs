namespace BossMod.Global.MaskedCarnivale.Stage20.Act3;

public enum OID : uint
{
    Boss = 0x272C, //R=4.5
    Ultros = 0x272D, //R=5.1
    Tentacle = 0x272E //R=7.2
}

public enum AID : uint
{
    Fungah = 14705, // Boss->self, no cast, range 8+R ?-degree cone, knockback 20 away from source
    Fireball = 14706, // Boss->location, 3.5s cast, range 8 circle
    Snort = 14704, // Boss->self, 7.0s cast, range 50+R circle
    Fireball2 = 14707, // Boss->player, no cast, range 8 circle
    Tentacle = 14747, // Tentacle->self, 3.0s cast, range 8 circle
    Wallop = 14748, // Tentacle->self, 3.5s cast, range 50+R width 10 rect, knockback 20 away from source
    Clearout = 14749, // Tentacle->self, no cast, range 13+R ?-degree cone, knockback 20 away from source
    AquaBreath = 14745, // Ultros->self, 2.5s cast, range 8+R 90-degree cone
    Megavolt = 14746, // Ultros->self, 3.0s cast, range 6+R circle
    ImpSong = 14744 // Ultros->self, 6.0s cast, range 50+R circle
}

class AquaBreath(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AquaBreath), new AOEShapeCone(13.1f, 45.Degrees()));
class Megavolt(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Megavolt), 11.1f);
class Tentacle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Tentacle), 8);
class Wallop(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Wallop), new AOEShapeRect(57.2f, 5));
class WallopKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Wallop), 20, kind: Kind.AwayFromOrigin); //knockback actually delayed by 0.8s
class Fireball(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Fireball), 8);
class ImpSong(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.ImpSong), showNameInHint: true);
class Snort(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Snort), "Use Diamondback!");
class SnortKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Snort), 30, kind: Kind.AwayFromOrigin);  //knockback actually delayed by 0.7s

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"This act is act 1+2 combined with tentacles on top.\nThe Final Sting combo (Off-guard->Bristle->Moonflute->Final Sting) makes\nthis act including the achievement much easier. {Module.PrimaryActor.Name} is weak to fire.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Requirement for achievement: Don't kill any tentacles in this act", false);
    }
}

class Stage20Act3States : StateMachineBuilder
{
    public Stage20Act3States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Tentacle>()
            .ActivateOnEnter<Megavolt>()
            .ActivateOnEnter<AquaBreath>()
            .ActivateOnEnter<ImpSong>()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<WallopKB>()
            .ActivateOnEnter<Snort>()
            .ActivateOnEnter<SnortKB>()
            .ActivateOnEnter<Fireball>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 630, NameID = 3046, SortOrder = 3)]
public class Stage20Act3 : BossModule
{
    public Stage20Act3(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.Ultros), Colors.Object);
        Arena.Actors(Enemies(OID.Tentacle), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Tentacle => 2, // this ruins the achievement if boss is still alive when tentacles spawn, TODO: consider making this a setting
                OID.Ultros => 1,
                _ => 0
            };
        }
    }
}
