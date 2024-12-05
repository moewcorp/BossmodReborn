namespace BossMod.Stormblood.TreasureHunt.LostCanalsOfUznair.CanalIcebeast;

public enum OID : uint
{
    Boss = 0x1F15, //R=7.5
    CanalVindthurs = 0x1F0E, // R1.56
    CanalIceHomunculus = 0x1F0F, // R1.6
    Abharamu = 0x1EBF, // R3.42
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Abharamu/CanalVindthurs->player, no cast, single-target

    FrumiousJaws = 9556, // Boss->player, no cast, single-target
    AbsoluteZero = 9558, // Boss->self, 5.0s cast, range 38+R 90-degree cone
    Blizzard = 967, // CanalIceHomunculus->player, 1.0s cast, single-target
    Eyeshine = 9557, // Boss->self, 4.0s cast, range 38+R circle, gaze
    Freezeover = 4486, // CanalVindthurs->location, 3.0s cast, range 3 circle
    PlainPound = 4487, // CanalVindthurs->self, 3.0s cast, range 3+R circle

    AbharamuActivate = 9636, // Abharamu->self, no cast, single-target
    Spin = 8599, // Abharamu->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // Abharamu->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // Abharamu->location, 3.0s cast, range 6 circle
    Telega = 9630 // Abharamu->self, no cast, single-target, bonus adds disappear
}

class Eyeshine(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.Eyeshine));
class AbsoluteZero(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbsoluteZero), new AOEShapeCone(45.5f, 45.Degrees()));
class Freezeover(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Freezeover), 6);
class PlainPound(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PlainPound), new AOEShapeCircle(4.56f));
class RaucousScritch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.Abharamu);

class CanalIcebeastStates : StateMachineBuilder
{
    public CanalIcebeastStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Eyeshine>()
            .ActivateOnEnter<AbsoluteZero>()
            .ActivateOnEnter<Freezeover>()
            .ActivateOnEnter<PlainPound>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 268, NameID = 6650)]
public class CanalIcebeast(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos center = new(0, -420);
    private static readonly ArenaBoundsComplex arena = new([new Polygon(center, 19.51f * CosPI.Pi8th, 8, 22.5f.Degrees()), new Rectangle(center, 27.66f, 5.5f),
    new Rectangle(new(0, -440), 5.5f, 7.5f)], [new Rectangle(new(0, -400), 20, 3.35f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.CanalIceHomunculus).Concat(Enemies(OID.CanalVindthurs)));
        Arena.Actors(Enemies(OID.Abharamu), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Abharamu => 2,
                OID.CanalVindthurs or OID.CanalIceHomunculus => 1,
                _ => 0
            };
        }
    }
}
