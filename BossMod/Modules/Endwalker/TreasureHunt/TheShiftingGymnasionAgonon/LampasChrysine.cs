namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.LampasChrysine;

public enum OID : uint
{
    Boss = 0x3D40, //R=6
    GymnasiouLampas = 0x3D4D, //R=2.001
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 32287, // Boss->player, no cast, single-target

    AetherialLight = 32293, // Boss->self, 1.3s cast, single-target
    AetherialLight2 = 32294, // Helper->self, 3.0s cast, range 40 60-degree cone
    unknown = 32236, // Boss->self, no cast, single-target, seems to be connected to Aetherial Light
    Lightburst = 32289, // Boss->self, 3.3s cast, single-target
    Lightburst2 = 32290, // Helper->player, 5.0s cast, single-target
    Shine = 32291, // Boss->self, 1.3s cast, single-target
    Shine2 = 32292, // Helper->location, 3.0s cast, range 5 circle
    Summon = 32288, // Boss->self, 1.3s cast, single-target, spawns bonus loot adds
    Telega = 9630 // GymnasiouLampas->self, no cast, single-target, bonus loot add despawn
}

class Shine(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Shine2), 5);

class AetherialLight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherialLight2), new AOEShapeCone(40, 30.Degrees()), 4)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return ActiveCasters.Select((c, i) => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), (NumCasts > 2 && i < 2) ? Colors.Danger : Colors.AOE));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID == AID.AetherialLight2)
            ++NumCasts;
    }
}

class Lightburst(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Lightburst2));
class Summon(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Summon), "Calls bonus adds");

class LampasChrysineStates : StateMachineBuilder
{
    public LampasChrysineStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Shine>()
            .ActivateOnEnter<AetherialLight>()
            .ActivateOnEnter<Lightburst>()
            .ActivateOnEnter<Summon>()
            .Raw.Update = () => module.Enemies(LampasChrysine.All).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12021)]
public class LampasChrysine(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouLampas];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.GymnasiouLampas), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GymnasiouLampas => 1,
                _ => 0
            };
        }
    }
}
