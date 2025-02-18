namespace BossMod.Global.MaskedCarnivale.Stage27;

public enum OID : uint
{
    Boss = 0x2CF8, //R=4.5
    Bomb = 0x2CF9, //R=0.8
    MagitekExplosive = 0x2CEC, //R=0.8
    Helper = 0x233C
}

public enum AID : uint
{
    BombsSpawn = 19260, // Boss->self, no cast, single-target
    Fungah = 19256, // Boss->self, no cast, range 8+R 90-degree cone, knockback 15 away from source
    Explosion = 19259, // Bomb->self, no cast, range 8 circle, wipe if in range
    Fireball = 19258, // Boss->location, 4.0s cast, range 8 circle
    Fungahhh = 19257, // Boss->self, no cast, range 8+R 90-degree cone, knockback 15 away from source
    Snort = 19266, // Boss->self, 10.0s cast, range 60+R circle, knockback 15 away from source
    MassiveExplosion = 19261 // MagitekExplosive->self, no cast, range 60 circle, wipe, failed to destroy Magitek Explosive in time
}

class Fireball(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Fireball), 8);
class Snort(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Snort), 15, stopAtWall: true);

class Fungah(BossModule module) : Components.Knockback(module, stopAtWall: true)
{
    private DateTime _activation;
    private readonly List<Actor> _bombs = [];
    private bool otherpatterns;
    private static readonly AOEShapeCone cone = new(12.5f, 45.Degrees());

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_bombs.Count != 0 && _activation != default || otherpatterns)
            yield return new(Module.PrimaryActor.Position, 15, _activation, cone, Direction: Angle.FromDirection(actor.Position - Module.PrimaryActor.Position));
    }

    public override void OnActorCreated(Actor actor)
    {
        var MagitekExplosive = Module.Enemies(OID.MagitekExplosive).FirstOrDefault();
        if ((OID)actor.OID == OID.Bomb)
            _bombs.Add(actor);
        if (_bombs.Count == 8)
            _activation = WorldState.FutureTime(5);
        if (MagitekExplosive != null)
            if (MagitekExplosive.Position.AlmostEqual(new(96, 94), 3) || MagitekExplosive.Position.AlmostEqual(new(92, 100), 3) || MagitekExplosive.Position.AlmostEqual(new(96, 106), 3) || MagitekExplosive.Position.AlmostEqual(new(108, 100), 3))
            {
                _activation = WorldState.FutureTime(5.3f);
                otherpatterns = true;
            }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_bombs.Count != 0 && (AID)spell.Action.ID == AID.Explosion)
            _bombs.Clear();
        if ((AID)spell.Action.ID is AID.Fungah or AID.Fungahhh)
        {
            _activation = default;
            otherpatterns = false;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<Explosion>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class Explosion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _casters = [];
    private static readonly AOEShapeCircle circle = new(8);
    private DateTime _activation;
    private DateTime _snortingeffectends;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_casters.Count > 0 && _snortingeffectends == default)
            foreach (var c in _casters)
                yield return new(circle, c.Position, default, _activation);
        if (_casters.Count > 0 && _snortingeffectends > WorldState.CurrentTime)
            foreach (var c in _casters)
                yield return new(circle, c.Position + Math.Min(15, Module.Arena.IntersectRayBounds(c.Position, (c.Position - Module.PrimaryActor.Position).Normalized()) - c.HitboxRadius / 2) * (c.Position - Module.PrimaryActor.Position).Normalized(), Activation: _activation);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID == OID.Bomb)
        {
            if (animState1 == 1)
            {
                _casters.Add(actor);
                _activation = WorldState.FutureTime(6);
            }
        }
    }

    public override void Update()
    {
        if (_snortingeffectends != default && _snortingeffectends < WorldState.CurrentTime)
            _snortingeffectends = default;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID == AID.Explosion)
            _casters.Remove(caster);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Snort)
            _snortingeffectends = Module.CastFinishAt(spell, 2.5f);
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will spawn Bombs and Magitek Explosives throughout the fight.\nUse Snort to push away Bombs from Magitek Explosives and bait Fireballs\naway from the MEs. Meanwhile destroy the MEs asap because they will blow\nup on their own after about 35s. If any ME detonates you will be wiped.\nThe MEs are weak against water abilities and strong against fire attacks.");
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    private DateTime _activation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!Module.Enemies(OID.MagitekExplosive).All(e => e.IsDead))
            hints.Add($"A {Module.Enemies(OID.MagitekExplosive).FirstOrDefault()!.Name} spawned, destroy it asap.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.Enemies(OID.MagitekExplosive).Any(e => e.IsTargetable))
            hints.Add($"Explosion in ca.: {Math.Max(35 - (WorldState.CurrentTime - _activation).TotalSeconds, 0.0f):f1}s");
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.MagitekExplosive)
            _activation = WorldState.CurrentTime;
    }
}

class Stage27States : StateMachineBuilder
{
    public Stage27States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<Snort>()
            .ActivateOnEnter<Fungah>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 696, NameID = 3046)]
public class Stage27 : BossModule
{
    public Stage27(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.Bomb), Colors.Object);
        Arena.Actors(Enemies(OID.MagitekExplosive), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.MagitekExplosive => 2,
                OID.Boss => 1,
                OID.Bomb => AIHints.Enemy.PriorityPointless,
                _ => 0
            };
        }
    }
}
