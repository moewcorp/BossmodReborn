namespace BossMod.Stormblood.Dungeon.D02ShisuiOfTheVioletTides.D022RubyPrincess;

public enum OID : uint
{
    Boss = 0x1B0E, // R1.6
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Tornadogenesis = 8063, // Boss->self, no cast, range 8+R 120-degree cone
    Old = 8062, // Helper->self, no cast, range 4 circle, chest when polymorphing player
    Seduce = 8058, // Boss->self, 7.0s cast, range 50 circle
    CoriolisKick = 8059, // Boss->self, 5.0s cast, range 13 circle
    AbyssalVolcano = 8060, // Boss->self, 3.0s cast, range 7 circle
    GeothermalFlatulenceFirst = 9431, // Helper->location, 3.8s cast, range 4 circle
    GeothermalFlatulenceRest = 8061 // Helper->location, no cast, range 4 circle
}

public enum SID : uint
{
    Old = 1259, // none->player, extra=0x3D
    Seduced = 991 // Boss->player, extra=0xF

}

public enum IconID : uint
{
    ChasingAOE = 1 // player
}

class SeduceOld(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(2.5f);
    private bool active;
    private bool addedCircles;
    private readonly HashSet<Actor> chests = [.. module.Enemies(OID.Helper).Where(x => x.NameID == 6274)];
    private readonly HashSet<Circle> closedChests = [];
    private readonly HashSet<Circle> openChests = [];

    public static bool IsOld(Actor actor) => actor.FindStatus(SID.Old) != null;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in openChests)
            yield return new(circle, c.Center);
        yield return new(new AOEShapeCustom([.. closedChests]) with { InvertForbiddenZone = !IsOld(actor) && active }, Arena.Center, Color: IsOld(actor) || !active ? Colors.AOE : Colors.SafeFromAOE);
    }

    public override void Update()
    {
        if (closedChests.Count == 0 && !addedCircles)
        {
            foreach (var c in chests)
                closedChests.Add(new(c.Position, 2.5f));
            addedCircles = true;
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        var chest = chests.FirstOrDefault(x => x.Position.AlmostEqual(actor.Position, 5));
        if (chest != null)
        {
            if (state == 0x00040008)
            {
                closedChests.RemoveWhere(x => x.Center == chest.Position);
                openChests.Add(new(chest.Position, 2.5f));
            }
            else if (state == 0x00100020)
            {
                closedChests.Add(new(chest.Position, 2.5f));
                openChests.RemoveWhere(x => x.Center == chest.Position);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Seduce)
            active = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Seduce)
            active = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if ((IsOld(actor) && active || !active) && ActiveAOEs(slot, actor).Any(c => c.Check(actor.Position) && c.Color != Colors.SafeFromAOE))
            hints.Add("GTFO from chests!");
        else if (!IsOld(actor) && active)
            hints.Add("Get morphed!");
    }
}

class SeduceCoriolisKick(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(13);
    private AOEInstance _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe != default)
            yield return _aoe with { Origin = Module.PrimaryActor.Position };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Seduce)
            _aoe = new(circle, default, default, Module.CastFinishAt(spell, 8));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CoriolisKick)
            _aoe = default;
    }
}

class AbyssalVolcano(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AbyssalVolcano), 7);

class GeothermalFlatulence(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(4), ActionID.MakeSpell(AID.GeothermalFlatulenceFirst), ActionID.MakeSpell(AID.GeothermalFlatulenceRest), 3, 0.8f, 10, true, (uint)IconID.ChasingAOE)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Actors.Contains(actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 18), Activation);
    }
}

class Tornadogenesis(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Tornadogenesis), new AOEShapeCone(9.6f, 60.Degrees()))
{
    private readonly SeduceCoriolisKick _aoe = module.FindComponent<SeduceCoriolisKick>()!;
    private readonly GeothermalFlatulence _aoes = module.FindComponent<GeothermalFlatulence>()!;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_aoe.ActiveAOEs(slot, actor).Any() && _aoes.Chasers.Count == 0)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_aoe.ActiveAOEs(slot, actor).Any() && _aoes.Chasers.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_aoe.ActiveAOEs(pcSlot, pc).Any() && _aoes.Chasers.Count == 0)
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class D022RubyPrincessStates : StateMachineBuilder
{
    public D022RubyPrincessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SeduceOld>()
            .ActivateOnEnter<SeduceCoriolisKick>()
            .ActivateOnEnter<AbyssalVolcano>()
            .ActivateOnEnter<GeothermalFlatulence>()
            .ActivateOnEnter<Tornadogenesis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 235, NameID = 6241)]
public class D022RubyPrincess(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(-0.046f, -208.362f), 20)], [new Rectangle(new(-0.4f, -187.4f), 20, 2.5f), new Rectangle(new(-20, -208), 20, 1.5f, 90.Degrees())]);
}
