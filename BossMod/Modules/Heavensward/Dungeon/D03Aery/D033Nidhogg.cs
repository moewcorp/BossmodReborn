namespace BossMod.Heavensward.Dungeon.D03Aery.D033Nidhogg;

public enum OID : uint
{
    Boss = 0x39CA, // R12.0
    TheSablePrice = 0x39CB, // R1.0
    Liegedrake = 0x39CD, // R3.6
    Ahleh = 0x39CC, // R5.0
    Helper = 0x233C
}

public enum AID : uint
{
    Attack = 870, // Boss/Liegedrake/Ahleh->player, no cast, single-target

    DeafeningBellow = 30206, // Boss->self, 5.0s cast, range 80 circle
    HotTailVisual = 30196, // Boss->self, 3.0s cast, single-target
    HotTail = 30197, // Helper->self, 3.5s cast, range 68 width 16 rect
    HotWingVisual = 30194, // Boss->self, 3.0s cast, single-target
    HotWing = 30195, // Helper->self, 3.5s cast, range 30 width 68 rect
    HorridRoarVisual = 30201, // Boss->self, no cast, single-target
    HorridRoarSpread = 30202, // Helper->player, 5.0s cast, range 6 circle
    HorridRoar = 30200, // Helper->location, 4.0s cast, range 6 circle
    Cauterize = 30198, // Boss->self, 5.0s cast, range 80 width 22 rect
    Touchdown = 30199, // Boss->self, no cast, range 80 circle
    TheSablePrice = 30203, // Boss->self, 3.0s cast, single-target
    TheScarletPrice = 30205, // Boss->player, 5.0s cast, single-target
    Massacre = 30207, // Boss->location, 6.0s cast, range 80 circle
    HorridBlaze = 30224, // Boss->players, 7.0s cast, range 6 circle
    SableWeave = 30204, // TheSablePrice->player, 15.0s cast, single-target
    Roast = 30209 // Ahleh->self, 4.0s cast, range 30 width 8 rect
}

public enum SID : uint
{
    Fetters = 3324, // none->player, extra=0x0
    DraconianGaze = 703 // none->Boss, extra=0x0
}

class Fetters(BossModule module) : BossComponent(module)
{
    private bool chained;
    private bool chainsactive;
    private Actor? chaintarget;
    private bool casting;

    public override void Update()
    {
        var fetters = chaintarget?.FindStatus(SID.Fetters) != null;
        if (fetters)
            chainsactive = true;
        if (fetters && !chained)
            chained = true;
        if (chaintarget != null && !fetters && !casting)
        {
            chained = false;
            chaintarget = null;
            chainsactive = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (chaintarget != null && chainsactive)
            hints.Add($"Destroy fetters on {chaintarget.Name}!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (chained && actor != chaintarget)
            foreach (var e in hints.PotentialTargets)
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.TheSablePrice => 1,
                    OID.Boss => -1,
                    _ => 0
                };
        var fetters = Module.Enemies(OID.TheSablePrice).FirstOrDefault();
        if (fetters != null && !fetters.IsDead)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(fetters.Position, fetters.HitboxRadius + 3));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SableWeave)
        {
            chaintarget = WorldState.Actors.Find(spell.TargetID)!;
            casting = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SableWeave)
            casting = false;
    }
}

class DeafeningBellow(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DeafeningBellow));

class HotTail(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HotTail), new AOEShapeRect(68, 8));
class HotWing(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HotWing), new AOEShapeRect(30, 34));
class Cauterize(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Cauterize), new AOEShapeRect(80, 11));
class HorridRoarSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HorridRoarSpread), 6);
class HorridRoar(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HorridRoar), 6);
class HorridBlaze(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.HorridBlaze), 6, 4, 4);
class Massacre(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Massacre));
class TheScarletPrice(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.TheScarletPrice));
class Roast(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Roast), new AOEShapeRect(30, 4));

class D033NidhoggStates : StateMachineBuilder
{
    public D033NidhoggStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HotTail>()
            .ActivateOnEnter<HotWing>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<HorridRoarSpread>()
            .ActivateOnEnter<HorridRoar>()
            .ActivateOnEnter<HorridBlaze>()
            .ActivateOnEnter<DeafeningBellow>()
            .ActivateOnEnter<Massacre>()
            .ActivateOnEnter<Fetters>()
            .ActivateOnEnter<TheScarletPrice>()
            .ActivateOnEnter<Roast>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3458)]
public class D033Nidhogg(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(35, -267), 32.5f)], [new Rectangle(new(35, -299.5f), 20, 0.75f), new Rectangle(new(35, -233.5f), 20, 1.25f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.Liegedrake));
        Arena.Actors(Enemies(OID.Ahleh));
        Arena.Actors(Enemies(OID.TheSablePrice), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.TheSablePrice => 2,
                OID.Ahleh or OID.Liegedrake => 1,
                _ => 0
            };
        }
    }
}
