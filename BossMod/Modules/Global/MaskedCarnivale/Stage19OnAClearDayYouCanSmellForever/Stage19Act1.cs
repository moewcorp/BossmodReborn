namespace BossMod.Global.MaskedCarnivale.Stage19.Act1;

public enum OID : uint
{
    Boss = 0x2727, //R=5.775
    Voidzone = 0x1EA9F9 //R=0.5
}

public enum AID : uint
{
    Reflect = 15073, // Boss->self, 3.0s cast, single-target, boss starts reflecting all melee attacks
    AutoAttack = 6499, // Boss->player, no cast, single-target
    BadBreath = 15074, // Boss->self, 3.5s cast, range 12+R 120-degree cone
    VineProbe = 15075, // Boss->self, 2.5s cast, range 6+R width 8 rect
    OffalBreath = 15076 // Boss->location, 3.5s cast, range 6 circle, interruptible, voidzone
}

class BadBreath(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BadBreath), new AOEShapeCone(17.775f, 60.Degrees()));
class VineProbe(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.VineProbe), new AOEShapeRect(11.775f, 4));
class OffalBreath(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.OffalBreath));
class OffalBreathVoidzone(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.OffalBreath), m => m.Enemies(OID.Voidzone).Where(e => e.EventState != 7), 1.6f);

class Reflect(BossModule module) : BossComponent(module)
{
    private bool reflect;
    private bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Reflect)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Reflect)
        {
            reflect = true;
            casting = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (casting)
            hints.Add("Boss will reflect all magic damage!");
        else if (reflect)
            hints.Add("Boss reflects all magic damage!"); // TODO: could use an AI hint to never use magic abilities after this is casted
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("At the start of the fight Rebekkah will cast Reflect. This will reflect all\nmagic damage back to you. Useful skills: Sharpened Knife,\nFlying Sardine, Ink Jet (Act 2), Exuviation (Act 2), potentially a Final Sting\ncombo. (Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

class Stage19Act1States : StateMachineBuilder
{
    public Stage19Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Reflect>()
            .ActivateOnEnter<BadBreath>()
            .ActivateOnEnter<VineProbe>()
            .ActivateOnEnter<OffalBreath>()
            .ActivateOnEnter<OffalBreathVoidzone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 629, NameID = 8117, SortOrder = 1)]
public class Stage19Act1 : BossModule
{
    public Stage19Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }
}
