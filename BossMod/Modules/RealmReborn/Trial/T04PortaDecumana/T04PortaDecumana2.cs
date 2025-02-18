namespace BossMod.RealmReborn.Trial.T04PortaDecumana.Phase2;

public enum OID : uint
{
    Boss = 0x3900, // R=6.0
    Aetheroplasm = 0x3902, // R=1.0
    MagitekBit = 0x3901, // R=0.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 29004, // Boss->player, no cast, single-target
    Teleport = 28628, // Boss->location, no cast, single-target
    TankPurge = 29022, // Boss->self, 5.0s cast, raidwide
    HomingLasers = 29023, // Boss->player, 5.0s cast, single-target, tankbuster

    MagitekRayForward = 29005, // Boss->self, no cast, single-target, visual
    MagitekRayRight = 29006, // Boss->self, no cast, single-target, visual
    MagitekRayLeft = 29007, // Boss->self, no cast, single-target, visual
    MagitekRayAOEForward = 29008, // Helper->self, 2.2s cast, range 40 width 6 rect aoe
    MagitekRayAOERight = 29009, // Helper->self, 2.2s cast, range 40 width 6 rect aoe
    MagitekRayAOELeft = 29010, // Helper->self, 2.2s cast, range 40 width 6 rect aoe

    HomingRay = 29011, // Boss->self, 4.0s cast, single-target, visual
    HomingRayAOE = 29012, // Helper->player, 5.0s cast, range 6 circle spread
    LaserFocus = 29013, // Boss->self, 4.0s cast, single-target, visual
    LaserFocusAOE = 29014, // Helper->player, 5.0s cast, range 6 circle stack

    AethericBoom = 29015, // Boss->self, 4.0s cast, knockback 30
    AetheroplasmSoak = 29016, // Aetheroplasm->self, no cast, range 8 circle aoe
    AetheroplasmCollide = 29017, // Aetheroplasm->self, no cast, raidwide

    BitTeleport = 29018, // MagitekBit->location, no cast, single-target
    AssaultCannon = 29019, // MagitekBit->self, 4.0s cast, range 40 width 4 rect

    CitadelBuster = 29020, // Boss->self, 5.0s cast, range 40 width 12 rect aoe
    Explosion = 29021, // Helper->self, 7.0s cast, raidwide with ? falloff

    LimitBreakRefill = 28542, // Helper->self, no cast, range 40 circle - probably limit break refill
    Ultima = 29024, // Boss->self, 71.0s cast, enrage
}

class TankPurge(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TankPurge));
class HomingLasers(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HomingLasers));

abstract class MagitekRay(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(40f, 3f));
class MagitekRayF(BossModule module) : MagitekRay(module, AID.MagitekRayAOEForward);
class MagitekRayR(BossModule module) : MagitekRay(module, AID.MagitekRayAOERight);
class MagitekRayL(BossModule module) : MagitekRay(module, AID.MagitekRayAOELeft);

class HomingRay(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HomingRayAOE), 6f);
class LaserFocus(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.LaserFocusAOE), 6f, 4, 4);

class AethericBoom(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.AethericBoom), 30f, stopAtWall: true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Casters.Count > 0)
            hints.Add("Prepare to soak the orbs!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Surecast), actor, ActionQueue.Priority.High);
        }
    }
}

class Aetheroplasm(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _orbs = module.Enemies(OID.Aetheroplasm);

    public IEnumerable<Actor> ActiveOrbs => _orbs.Where(x => !x.IsDead && !x.Rotation.AlmostEqual(default, Angle.DegToRad));

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ActiveOrbs.Any())
            hints.Add("Soak the orbs!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // orbs spawn with rotation 0°, checking for a different angle makes sure the AI doesn't run into the wall trying to catch them
        // since orbs are outside of the arena until they start rotating
        Actor[] orbz = [.. ActiveOrbs];
        var len = orbz.Length;
        if (len != 0)
        {
            var orbs = new Func<WPos, float>[len];
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
            for (var i = 0; i < len; ++i)
            {
                var o = orbz[i];
                orbs[i] = ShapeDistance.InvertedRect(o.Position + 0.5f * o.Rotation.ToDirection(), new WDir(0f, 1f), 0.5f, 0.5f, 0.5f);
            }
            hints.AddForbiddenZone(ShapeDistance.Intersection(orbs), DateTime.MaxValue);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var orb in ActiveOrbs)
            Arena.AddCircle(orb.Position, 1f, Colors.Safe);
    }
}

class AssaultCannon(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AssaultCannon), new AOEShapeRect(40f, 2f));
class CitadelBuster(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CitadelBuster), new AOEShapeRect(40f, 6f));

class Explosion(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Explosion), 16f) // TODO: verify falloff
{
    // there is an overlap with another mechanic which has to be resolved first
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<AssaultCannon>()!.Casters.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class Ultima(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Ultima), "Enrage!", true);

class T04PortaDecumana2States : StateMachineBuilder
{
    public T04PortaDecumana2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TankPurge>()
            .ActivateOnEnter<HomingLasers>()
            .ActivateOnEnter<MagitekRayF>()
            .ActivateOnEnter<MagitekRayR>()
            .ActivateOnEnter<MagitekRayL>()
            .ActivateOnEnter<HomingRay>()
            .ActivateOnEnter<LaserFocus>()
            .ActivateOnEnter<AethericBoom>()
            .ActivateOnEnter<Aetheroplasm>()
            .ActivateOnEnter<AssaultCannon>()
            .ActivateOnEnter<CitadelBuster>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Ultima>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 830, NameID = 2137, SortOrder = 2)]
public class T04PortaDecumana2(WorldState ws, Actor primary) : BossModule(ws, primary, new(-704, 480), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, Colors.Enemy, true);
        Arena.Actors(Enemies(OID.Aetheroplasm).Where(x => !x.IsDead), Colors.Object, true);
    }
}
