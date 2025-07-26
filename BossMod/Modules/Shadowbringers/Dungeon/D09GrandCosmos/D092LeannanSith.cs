namespace BossMod.Shadowbringers.Dungeon.D09GrandCosmos.D092LeananSith;

public enum OID : uint
{
    Boss = 0x2C04, // R2.4
    EnslavedLove = 0x2C06, // R3.6
    LoversRing = 0x2C05, // R2.04
    LeannanSeed1 = 0x1EAE9E, // R0.5
    LeannanSeed2 = 0x1EAE9F, // R0.5
    LeannanSeed3 = 0x1EAEA0, // R0.5
    LeannanSeed4 = 0x1EAEA1, // R0.5
    DirtTiles = 0x1EAEC6, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // LoversRing->player, no cast, single-target
    Teleport = 18207, // Boss->location, no cast, ???

    StormOfColor = 18203, // Boss->player, 4.0s cast, single-target
    OdeToLostLove = 18204, // Boss->self, 4.0s cast, range 60 circle
    DirectSeeding = 18205, // Boss->self, 4.0s cast, single-target
    GardenersHymn = 18206, // Boss->self, 14.0s cast, single-target

    ToxicSpout = 18208, // LoversRing->self, 8.0s cast, range 60 circle
    OdeToFarWinds = 18210, // Boss->self, 3.0s cast, single-target
    FarWind = 18211, // Helper->location, 5.0s cast, range 8 circle
    FarWindSpread = 18212, // Helper->player, 5.0s cast, range 5 circle
    OdeToFallenPetals = 18768, // Boss->self, 4.0s cast, range 5-60 donut
    IrefulWind = 18209 // EnslavedLove->self, 13.0s cast, range 40+R width 40 rect, knockback 10, source forward
}

public enum SID : uint
{
    Transporting = 404 // none->player, extra=0x15
}

class OdeToLostLove(BossModule module) : Components.RaidwideCast(module, (uint)AID.OdeToLostLove);
class StormOfColor(BossModule module) : Components.SingleTargetCast(module, (uint)AID.StormOfColor);
class FarWindSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FarWindSpread, 5f);
class FarWind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FarWind, 8f);
class OdeToFallenPetals(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OdeToFallenPetals, new AOEShapeDonut(5f, 60f));
class IrefulWind(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.IrefulWind, 10f, kind: Kind.DirForward, stopAtWall: true);

class GreenTiles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<Actor, DateTime> transportingCheckStartTimes = [];
    private const float HalfSize = 5;
    private static readonly WPos[] defaultGreenTiles =
    [
        new(-5f, -75f), new(5f, -75f), new(-15f, -65f), new(15f, -65f),
        new(-15f, -55f), new(5f, -55f), new(-5f, -45f), new(15f, -45f)
    ];
    private Square[] tiles = [];
    private AOEInstance? _aoe;
    public BitMask transporting;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe != null ? new AOEInstance[1] { _aoe.Value with { Risky = transporting[slot] } } : [];

    private bool ShouldActivateAOEs => NumCasts == 1 ? tiles.Length > 0 : tiles.Length > 8;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Transporting)
            transporting[Raid.FindSlot(actor.InstanceID)] = true;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Transporting)
            transporting[Raid.FindSlot(actor.InstanceID)] = false;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x0B)
        {
            return;
        }
        tiles = state switch
        {
            0x00020001u => GenerateTiles(defaultGreenTiles),
            0x00200010u => GenerateRotatedTiles(180f),
            0x01000080u => GenerateRotatedTiles(-90f),
            0x08000400u => GenerateRotatedTiles(90f),
            _ => [],
        };
    }

    public override void Update()
    {
        var isNull = _aoe == null;
        var activate = ShouldActivateAOEs;
        if (activate && isNull)
            _aoe = new(new AOEShapeCustom(tiles), Arena.Center, color: Colors.FutureVulnerable);
        else if (!activate && !isNull)
            _aoe = null;
    }

    private static Square[] GenerateTiles(WPos[] positions)
    {
        var tiles = new Square[8];
        for (var i = 0; i < 8; ++i)
        {
            tiles[i] = new Square(positions[i], HalfSize);
        }
        return tiles;
    }

    private static Square[] GenerateRotatedTiles(float angle)
    {
        var tiles = new Square[8];
        for (var i = 0; i < 8; ++i)
        {
            tiles[i] = new Square(WPos.RotateAroundOrigin(angle, D092LeananSith.ArenaCenter, defaultGreenTiles[i]), HalfSize);
        }
        return tiles;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DirectSeeding)
            ++NumCasts;
        else if (spell.Action.ID == (uint)AID.IrefulWind)
        {
            var knockbackDirection = spell.Rotation.Round(90f);
            var offset = 10f * knockbackDirection.ToDirection();
            var newTiles = new List<Square>(10);
            var pos = caster.Position;

            for (var i = 0; i < 8; ++i)
            {
                var newCenter = tiles[i].Center - offset;
                var newTile = new Square(newCenter, HalfSize);
                newTiles.Add(newTile);

                if ((pos - newCenter).LengthSq() > 625f)
                    newTiles.Add(new(newCenter + offset, HalfSize));
            }
            tiles = [.. newTiles];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe == null)
            return;
        base.AddAIHints(slot, actor, assignment, hints);

        var shape = _aoe.Value;
        var clippedSeeds = GetClippedSeeds(ref shape);

        if (!transporting[slot])
        {
            Actor? closest = null;
            var minDistSq = float.MaxValue;

            var count = clippedSeeds.Count;
            for (var i = 0; i < count; ++i)
            {
                var seed = clippedSeeds[i];
                if (seed.IsTargetable)
                {
                    hints.GoalZones.Add(hints.GoalSingleTarget(clippedSeeds[i], 2.5f, 5f));
                    var distSq = (actor.Position - seed.Position).LengthSq();
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        closest = seed;
                    }
                }
            }
            hints.InteractWithTarget = closest;
        }
        else if (!shape.Check(actor.Position))
            HandleTransportingActor(ref actor, ref hints);
        else
            transportingCheckStartTimes.Remove(actor);
    }

    private List<Actor> GetClippedSeeds(ref AOEInstance shape)
    {
        var seeds = Module.Enemies(D092LeananSith.Seeds);
        var count = seeds.Count;
        var clippedSeeds = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var s = seeds[i];
            if (s.IsTargetable && shape.Check(s.Position))
                clippedSeeds.Add(s);
        }
        return clippedSeeds;
    }

    private void HandleTransportingActor(ref Actor actor, ref AIHints hints)
    {
        if (!transportingCheckStartTimes.TryGetValue(actor, out var checkStartTime))
            transportingCheckStartTimes[actor] = WorldState.CurrentTime;
        else if (WorldState.CurrentTime >= checkStartTime.AddSeconds(0.25d))  // we need to delay the drop or server lag will cause the seed to drop at an old position
        {
            hints.StatusesToCancel.Add(((uint)SID.Transporting, default));
            transportingCheckStartTimes.Remove(actor);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var trans = transporting[slot] != default;
        if (_aoe == null)
            return;
        var aoe = _aoe.Value;
        if (trans)
        {
            if (aoe.Check(actor.Position))
                hints.Add("Drop seed outside of vulnerable area!");
            else
                hints.Add("Drop your seed!");
        }
        else if (!trans)
        {
            if (GetClippedSeeds(ref aoe).Count != 0)
                hints.Add("Pick up seeds in vulnerable area!");
        }
    }
}

class D092LeananSithStates : StateMachineBuilder
{
    public D092LeananSithStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OdeToLostLove>()
            .ActivateOnEnter<StormOfColor>()
            .ActivateOnEnter<FarWindSpread>()
            .ActivateOnEnter<FarWind>()
            .ActivateOnEnter<OdeToFallenPetals>()
            .ActivateOnEnter<IrefulWind>()
            .ActivateOnEnter<GreenTiles>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 692, NameID = 9044)]
public class D092LeananSith(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(19.5f))
{
    public static readonly WPos ArenaCenter = new(default, -60f);
    public static readonly uint[] Seeds = [(uint)OID.LeannanSeed1, (uint)OID.LeannanSeed2, (uint)OID.LeannanSeed3, (uint)OID.LeannanSeed4];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(Seeds), Colors.Object);
        Arena.Actors(Enemies((uint)OID.LoversRing));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.LoversRing => 1,
                _ => 0
            };
        }
    }
}
