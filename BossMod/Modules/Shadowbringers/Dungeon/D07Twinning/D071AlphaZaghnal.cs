namespace BossMod.Shadowbringers.Dungeon.D07Twinning.D071AlphaZaghnal;

public enum OID : uint
{
    Boss = 0x27D1, // R6.0
    IronCage = 0x27D3, // R0.5, hitbox helper for cages
    BetaZaghnal = 0x27D2, // R3.75
    Voidzone = 0x1E88F5, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 6497, // BetaZaghnal->player, no cast, single-target

    Augurium = 15717, // Boss->self, 4.0s cast, range 12 120-degree cone
    BeastlyRoar = 15716, // Boss->self, 4.0s cast, range 50 circle
    BeastRampant = 15712, // Boss->self, no cast, single-target

    ForlornImpact1 = 15713, // Boss->self, no cast, range 50 width 6 rect
    ForlornImpact2 = 15718, // Boss->self, no cast, range 50 width 6 rect
    ForlornImpact3 = 15719, // Boss->self, no cast, range 50 width 6 rect
    ForlornImpact4 = 15720, // Boss->self, no cast, range 50 width 6 rect

    BeastPassant = 15714, // Boss->self, no cast, single-target
    Pounce = 15724, // BetaZaghnal->player, no cast, single-target

    PounceErrant = 15711, // Boss->players, no cast, range 10 circle
    ChargeEradicated = 15715, // Boss->player, 5.0s cast, range 8 circle
}

public enum IconID : uint
{
    Target1 = 50, // player
    Target2 = 51, // player
    Target3 = 52, // player
    Target4 = 53, // player
    Spreadmarker = 90 // player
}

class BeastlyRoar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BeastlyRoar));
class Augurium(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Augurium), new AOEShapeCone(12, 60.Degrees()));

class PounceErrant(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.PounceErrant), 10, 4.6f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (IsSpreadTarget(actor))
        {
            var spread = Spreads.FirstOrDefault();
            var cages = Module.Enemies(OID.IronCage);
            var count = cages.Count;
            if (count == 0)
                return;
            var forbidden = new List<Func<WPos, float>>(count);
            for (var i = 0; i < count; ++i)
                forbidden.Add(ShapeDistance.Circle(Module.Enemies(OID.IronCage)[i].Position, 11));
            hints.AddForbiddenZone(ShapeDistance.Union(forbidden), spread.Activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (!Spreads.Any(x => x.Target == pc))
            return;
        var cages = Module.Enemies(OID.IronCage);
        for (var i = 0; i < cages.Count; ++i)
        {
            var a = cages[i];
            Arena.AddCircle(a.Position, a.HitboxRadius, Colors.Danger);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Spreads.Any(x => x.Target == actor))
            hints.Add("Spread, avoid intersecting cage hitboxes!");
    }
}

class ChargeEradicated(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ChargeEradicated), 8, 4, 4);
class ChargeEradicatedVoidzone(BossModule module) : Components.PersistentVoidzone(module, 8, m => m.Enemies(OID.Voidzone).Where(x => x.EventState != 7));

class ForlornImpact(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly AOEShapeRect rect = new(50, 3);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurrentBaits.Count != 0 && (AID)spell.Action.ID is AID.ForlornImpact1 or AID.ForlornImpact2 or AID.ForlornImpact3 or AID.ForlornImpact4)
            CurrentBaits.RemoveAt(0);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID is >= IconID.Target1 and <= IconID.Target4)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, rect, WorldState.FutureTime(7.2f)));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var bait = ActiveBaitsOn(actor).FirstOrDefault();
        if (bait != default)
        {
            var cages = Module.Enemies(OID.IronCage);
            var count = cages.Count;
            if (count == 0)
                return;
            var forbidden = new List<Func<WPos, float>>(count);
            for (var i = 0; i < cages.Count; ++i)
            {
                var a = cages[i];
                forbidden.Add(ShapeDistance.Cone(bait.Source.Position, 100, bait.Source.AngleTo(a), Angle.Asin(3.5f / (a.Position - bait.Source.Position).Length())));
            }
            hints.AddForbiddenZone(ShapeDistance.Union(forbidden), bait.Activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (!ActiveBaits.Any(x => x.Target == pc))
            return;
        var cages = Module.Enemies(OID.IronCage);
        for (var i = 0; i < cages.Count; ++i)
        {
            var a = cages[i];
            Arena.AddCircle(a.Position, a.HitboxRadius, Colors.Danger);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Any(x => x.Target == actor))
            hints.Add("Bait away, avoid intersecting cage hitboxes!");
    }
}

class D071AlphaZaghnalStates : StateMachineBuilder
{
    public D071AlphaZaghnalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BeastlyRoar>()
            .ActivateOnEnter<Augurium>()
            .ActivateOnEnter<PounceErrant>()
            .ActivateOnEnter<ChargeEradicated>()
            .ActivateOnEnter<ChargeEradicatedVoidzone>()
            .ActivateOnEnter<ForlornImpact>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 655, NameID = 8162)]
public class D071AlphaZaghnal(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, 285), new ArenaBoundsSquare(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.BetaZaghnal));
    }
}
