namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD70Taquaru;

public enum OID : uint
{
    Boss = 0x1815, // R5.750, x1
    Voidzone = 0x1E9998, // R0.500, EventObj type, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    Douse = 7091, // Boss->self, 3.0s cast, range 8 circle
    Drench = 7093, // Boss->self, no cast, range 10+R 90-degree cone, 5.1s after pull, 7.1s after every 2nd Electrogenesis, 7.3s after every FangsEnd
    Electrogenesis = 7094, // Boss->location, 3.0s cast, range 8 circle
    FangsEnd = 7092 // Boss->player, no cast, single-target
}

class Douse(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, ActionID.MakeSpell(AID.Douse), GetVoidzones, 0.8f)
{
    public static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Voidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class DousePuddle(BossModule module) : BossComponent(module)
{
    private readonly Actor[] puddles = Douse.GetVoidzones(module);

    private bool BossInPuddle
    {
        get
        {
            var len = puddles.Length;
            for (var i = 0; i < len; ++i)
            {
                if (Module.PrimaryActor.Position.InCircle(puddles[i].Position, 8f + Module.PrimaryActor.HitboxRadius))
                    return true;
            }
            return false;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        // indicate on minimap how far boss needs to be pulled
        if (BossInPuddle)
            Arena.AddCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius, Colors.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.PrimaryActor.TargetID == actor.InstanceID && BossInPuddle)
            hints.Add("Pull boss out of puddle!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.TargetID == actor.InstanceID && BossInPuddle)
        {
            var effPuddleSize = 8 + Module.PrimaryActor.HitboxRadius;
            var tankDist = hints.FindEnemy(Module.PrimaryActor)?.TankDistance ?? 2;
            // yaquaru tank distance seems to be around 2-2.5y, but from testing, 3y minimum is needed to move it out of the puddle, either because of rasterization shenanigans or netcode
            var effTankDist = Module.PrimaryActor.HitboxRadius + tankDist + 1;

            var len = puddles.Length;
            var puddlez = new Func<WPos, float>[len];
            for (var i = 0; i < len; ++i)
                puddlez[i] = ShapeDistance.Circle(puddles[i].Position, effPuddleSize + effTankDist);
            var closest = ShapeDistance.Union(puddlez);
            hints.GoalZones.Add(p => closest(p) > 0f ? 1000f : 0f);
        }
    }
}

class Electrogenesis(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Electrogenesis), 8f);

class DD70TaquaruStates : StateMachineBuilder
{
    public DD70TaquaruStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Douse>()
            .ActivateOnEnter<DousePuddle>()
            .ActivateOnEnter<Electrogenesis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 205, NameID = 5321)]
public class DD70Taquaru(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -220f), new ArenaBoundsCircle(25f));
