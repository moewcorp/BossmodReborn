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
    private readonly List<Actor> chests = new(4);
    private readonly List<Circle> closedChests = [];
    private readonly List<Circle> openChests = [];
    private AOEShapeCustom? closedAOE;

    public static bool IsOld(Actor actor) => actor.FindStatus((uint)SID.Old) != null;

    public override void Update()
    {
        if (closedAOE == null)
        {
            var helpers = Module.Enemies((uint)OID.Helper);
            var countH = helpers.Count;

            for (var i = 0; i < countH; ++i)
            {
                var c = helpers[i];
                if (c.NameID == 6274)
                {
                    chests.Add(c);
                }
            }
            var count = chests.Count;
            for (var i = 0; i < count; ++i)
                closedChests.Add(new(chests[i].Position, 2.5f));
            closedAOE = new AOEShapeCustom([.. closedChests]);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = openChests.Count;
        var aoes = new AOEInstance[count + 1];
        for (var i = 0; i < count; ++i)
        {
            aoes[i] = new(circle, openChests[i].Center);
        }
        if (closedAOE is AOEShapeCustom aoe)
            aoes[count] = new(aoe with { InvertForbiddenZone = !IsOld(actor) && active }, Arena.Center, color: IsOld(actor) || !active ? default : Colors.SafeFromAOE);
        return aoes;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        var count = chests.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = chests[i];
            if (c.Position.AlmostEqual(actor.Position, 5f))
            {
                if (state == 0x00040008u)
                {
                    var countC = closedChests.Count;
                    for (var j = 0; j < countC; ++j)
                    {
                        if (c.Position == closedChests[j].Center)
                        {
                            closedChests.RemoveAt(j);
                            openChests.Add(new(c.Position, 2.5f));
                            break;
                        }
                    }
                }
                else if (state == 0x00100020)
                {
                    var countO = openChests.Count;
                    for (var j = 0; j < countO; ++j)
                    {
                        if (c.Position == openChests[j].Center)
                        {
                            openChests.RemoveAt(j);
                            closedChests.Add(new(c.Position, 2.5f));
                            break;
                        }
                    }
                }
                closedAOE = new AOEShapeCustom([.. closedChests]);
                return;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Seduce)
            active = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Seduce)
            active = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var isOld = IsOld(actor);
        if (isOld && active || !active)
        {
            var aoes = ActiveAOEs(slot, actor);
            ref readonly var aoe = ref aoes[0];
            if (aoe.Color != Colors.SafeFromAOE && aoe.Check(actor.Position))
                hints.Add("GTFO from chests!");
        }
        else if (!isOld && active)
            hints.Add("Get morphed!");
    }
}

class SeduceCoriolisKick(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(13f);
    public AOEInstance? AOE;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Seduce)
            AOE = new(circle, D022RubyPrincess.ArenaCenter.Quantized(), default, Module.CastFinishAt(spell, 8f));
        else if (spell.Action.ID == (uint)AID.CoriolisKick)
            AOE = new(circle, spell.LocXZ, default, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CoriolisKick)
            AOE = null;
    }
}

class AbyssalVolcano(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalVolcano, 7f);

class GeothermalFlatulence(BossModule module) : Components.StandardChasingAOEs(module, 4f, (uint)AID.GeothermalFlatulenceFirst, (uint)AID.GeothermalFlatulenceRest, 3, 0.8f, 10, true, (uint)IconID.ChasingAOE)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Targets[slot])
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 18f), Activation);
        }
    }
}

class Tornadogenesis(BossModule module) : Components.Cleave(module, (uint)AID.Tornadogenesis, new AOEShapeCone(9.6f, 60f.Degrees()))
{
    private readonly SeduceCoriolisKick _aoe = module.FindComponent<SeduceCoriolisKick>()!;
    private readonly GeothermalFlatulence _aoes = module.FindComponent<GeothermalFlatulence>()!;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.AOE == null && _aoes.Chasers.Count == 0)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.AOE == null && _aoes.Chasers.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_aoe.AOE == null && _aoes.Chasers.Count == 0)
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
    public static readonly WPos ArenaCenter = new(-0.046f, -208.362f);
    private static readonly ArenaBoundsComplex arena = new([new Circle(ArenaCenter, 20)], [new Rectangle(new(-0.4f, -187.4f), 20, 2.5f), new Rectangle(new(-20, -208), 1.5f, 20f)]);
}
