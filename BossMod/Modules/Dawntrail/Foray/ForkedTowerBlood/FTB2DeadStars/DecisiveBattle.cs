namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB2DeadStars;

sealed class DecisiveBattleStatus(BossModule module) : BossComponent(module)
{
    public readonly Actor?[] AssignedBoss = new Actor?[PartyState.MaxPartySize];
    public bool Active;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        ref readonly var assignedSlot = ref AssignedBoss[slot];
        if (slot < PartyState.MaxPartySize && assignedSlot != null)
        {
            var target = WorldState.Actors.Find(actor.TargetID);
            if (target != null && target != assignedSlot && target.OID is (uint)OID.Boss or (uint)OID.Phobos or (uint)OID.Nereid)
                hints.Add($"Target {assignedSlot?.Name}!");
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.DecisiveBattle && Raid.FindSlot(source.InstanceID) is var slot && slot is >= 0 and <= PartyState.MaxPartySize)
        {
            AssignedBoss[slot] = WorldState.Actors.Find(tether.Target);
            Active = true;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        ref var assignedSlot = ref AssignedBoss[slot];
        if (slot < AssignedBoss.Length && assignedSlot != null)
        {
            var count = hints.PotentialTargets.Count;
            for (var i = 0; i < count; ++i)
            {
                var enemy = hints.PotentialTargets[i];
                if (enemy.Actor != assignedSlot)
                    enemy.Priority = AIHints.Enemy.PriorityInvincible;
            }
        }
    }
}

sealed class DecisiveBattleAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private readonly List<Polygon> circles = new(3);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DecisiveBattleNereid or (uint)AID.DecisiveBattleTriton or (uint)AID.DecisiveBattlePhobos)
        {
            circles.Add(new(spell.LocXZ, 35f, 64));
            if (circles.Count == 3)
            {
                var center = Arena.Center;
                var exclusiveArea = ComputeNonOverlappingArea(circles, center);

                var donut = new DonutV(center, 30f, 100f, 64);
                var clipper = new PolygonClipper();
                exclusiveArea = clipper.Difference(new(exclusiveArea), new(donut.ToPolygon(center)));

                AOEShapeCustom shape = new([.. circles], InvertForbiddenZone: true) { Polygon = exclusiveArea };
                _aoe = new(shape, center, default, Module.CastFinishAt(spell), Colors.SafeFromAOE);
            }
        }
    }

    private static RelSimplifiedComplexPolygon ComputeNonOverlappingArea(List<Polygon> circles, WPos center)
    {
        var clipper = new PolygonClipper();
        var unionOperand = new PolygonClipper.Operand();
        for (var i = 0; i < 3; ++i)
        {
            unionOperand.AddPolygon(circles[i].ToPolygon(center));
        }
        var union = clipper.Simplify(unionOperand);

        // Compute all pairwise intersections
        var overlaps = new List<RelSimplifiedComplexPolygon>(3);
        for (var i = 0; i < 3; ++i)
        {
            var c1 = new PolygonClipper.Operand(circles[i].ToPolygon(center));
            for (var j = i + 1; j < 3; ++j)
            {
                var c2 = new PolygonClipper.Operand(circles[j].ToPolygon(center));
                overlaps.Add(clipper.Intersect(c1, c2));
            }
        }

        // Subtract overlaps from union
        var result = union;
        for (var i = 0; i < 3; ++i)
        {
            result = clipper.Difference(new(result), new(overlaps[i]));
        }
        return result;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DecisiveBattleTriton)
        {
            ++NumCasts;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe is AOEInstance aoe)
        {
            hints.Add("Stand inside AOE of assigned boss!", !aoe.Check(actor.Position));
        }
    }
}
