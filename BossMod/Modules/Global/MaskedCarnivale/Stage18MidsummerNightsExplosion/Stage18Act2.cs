namespace BossMod.Global.MaskedCarnivale.Stage18.Act2;

public enum OID : uint
{
    Boss = 0x2725, //R=3.0
    Keg = 0x2726 //R=0.65
}

public enum AID : uint
{
    WildCharge = 15055, // Boss->players, 3.5s cast, width 8 rect charge
    Explosion = 15054, // Keg->self, 2.0s cast, range 10 circle
    Fireball = 15051, // Boss->location, 4.0s cast, range 6 circle
    RipperClaw = 15050, // Boss->self, 4.0s cast, range 5+R 90-degree cone
    TailSmash = 15052, // Boss->self, 4.0s cast, range 12+R 90-degree cone
    BoneShaker = 15053 // Boss->self, no cast, range 50 circle, harmless raidwide
}

class Explosion(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Explosion), 10);
class Fireball(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Fireball), 6);
class RipperClaw(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RipperClaw), new AOEShapeCone(8, 45.Degrees()));
class TailSmash(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TailSmash), new AOEShapeCone(15, 45.Degrees()));

class WildCharge(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.WildCharge), 4)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count > 0 && !Module.Enemies(OID.Keg).All(e => e.IsDead))
            hints.Add("Aim charge at a keg!");
    }
}

class KegExplosion(BossModule module) : Components.GenericStackSpread(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in Module.Enemies(OID.Keg).Where(x => !x.IsDead))
            Arena.AddCircle(p.Position, 10);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var p in Module.Enemies(OID.Keg).Where(x => !x.IsDead))
            if (actor.Position.InCircle(p.Position, 10))
                hints.Add("In keg explosion radius!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Same as last stage. Make the manticores run to the kegs and their attacks\nwill make them blow up. Their attacks will also do friendly fire damage\nto each other.\nThe Ram's Voice and Ultravibration combo can be used to kill manticores.");
    }
}

class Stage18Act2States : StateMachineBuilder
{
    public Stage18Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<RipperClaw>()
            .ActivateOnEnter<TailSmash>()
            .ActivateOnEnter<WildCharge>()
            .Raw.Update = () => module.Enemies(Stage18Act2.Kegs).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 628, NameID = 8116, SortOrder = 2)]
public class Stage18Act2 : BossModule
{
    public Stage18Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
        ActivateComponent<KegExplosion>();
    }
    public static readonly uint[] Kegs = [(uint)OID.Boss, (uint)OID.Keg];

    protected override bool CheckPull() => Enemies(Kegs).Any(e => e.InCombat);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Boss));
        Arena.Actors(Enemies(OID.Keg), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
