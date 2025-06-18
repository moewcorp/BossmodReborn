namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class Unseal(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> players = new(48);
    private readonly List<Actor> targets = new(6);
    private bool? isClose;
    private DateTime activation;
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (actor.Role == Role.Tank)
        {
            return Utils.ZeroOrOne(ref _aoe);
        }
        return [];
    }

    public override void Update()
    {
        if (isClose is not bool close)
            return;
        targets.Clear();
        List<Actor>[] squareActors = [new(16), new(16), new(16)];

        var primaryPos = Module.PrimaryActor.Position;
        var count = players.Count;
        if (count == 0)
        {
            foreach (var a in Module.WorldState.Actors.Actors.Values)
            {
                if (a.OID == default)
                {
                    players.Add(a);
                }
            }
        }

        for (var i = 0; i < count; ++i)
        {
            var a = players[i];
            if (a.IsDead)
                continue;
            for (var j = 0; j < 3; j++)
            {
                if (a.Position.InSquare(FTB4Magitaur.SquarePositions[j], 10f, FTB4Magitaur.SquareDirs[j]))
                {
                    squareActors[j].Add(a);
                    break;
                }
            }
        }

        SelectTargetsFromSquare(squareActors[0]);
        SelectTargetsFromSquare(squareActors[1]);
        SelectTargetsFromSquare(squareActors[2]);

        void SelectTargetsFromSquare(List<Actor> actors)
        {
            var count = actors.Count;
            Span<(Actor actor, float distSq)> distances = new (Actor, float)[count];

            for (var i = 0; i < count; ++i)
            {
                var p = actors[i];
                var distSq = (p.Position - primaryPos).LengthSq();
                distances[i] = (p, distSq);
            }

            var counttargets = Math.Min(2, count);
            for (var i = 0; i < counttargets; ++i)
            {
                var selIdx = i;
                for (var j = i + 1; j < count; ++j)
                {
                    var distJSq = distances[j].distSq;
                    var distSelIdx = distances[selIdx].distSq;
                    var isBetter = close ? distJSq < distSelIdx : distJSq > distSelIdx;
                    if (isBetter)
                        selIdx = j;
                }

                if (selIdx != i)
                {
                    (distances[selIdx], distances[i]) = (distances[i], distances[selIdx]);
                }

                targets.Add(distances[i].actor);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id is (uint)AID.Unseal1 or (uint)AID.Unseal2)
        {
            isClose = id == (uint)AID.Unseal2;
            activation = Module.CastFinishAt(spell, 8f);
            _aoe = new(FTB4Magitaur.CircleMinusSquares, Arena.Center, default, activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Attack1 or (uint)AID.Attack2)
        {
            ++NumCasts;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (isClose is bool close)
        {
            var target = close ? "closest" : "farthest";
            hints.Add($"Targets two {target} players per square!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(targets, Colors.Vulnerable);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (isClose is not bool close || actor.IsDead)
            return;
        if (actor.Role == Role.Tank)
        {
            var inSquare = -1;
            for (var i = 0; i < 3; i++)
            {
                if (actor.Position.InSquare(FTB4Magitaur.SquarePositions[i], 10f, FTB4Magitaur.SquareDirs[i]))
                {
                    inSquare = i;
                    break;
                }
            }
            if (inSquare < 0)
            {
                hints.Add("Get on a square if needed!", false);
                return;
            }
            List<Actor> squareActors = new(16);
            var primaryPos = Module.PrimaryActor.Position;
            var count = players.Count;
            for (var i = 0; i < count; ++i)
            {
                var a = players[i];
                if (a.IsDead)
                {
                    continue;
                }
                if (a.Position.InSquare(FTB4Magitaur.SquarePositions[inSquare], 10f, FTB4Magitaur.SquareDirs[inSquare]))
                {
                    squareActors.Add(a);
                }
            }

            if (squareActors.Count < 2)
                return;

            var targetsOnSquare = new List<Actor>(2);

            var countS = squareActors.Count;
            Span<(Actor actor, float distSq)> distances = new (Actor, float)[countS];

            for (var i = 0; i < countS; ++i)
            {
                var p = squareActors[i];
                var distSq = (p.Position - primaryPos).LengthSq();
                distances[i] = (p, distSq);
            }

            var counttargets = Math.Min(2, countS);
            for (var i = 0; i < counttargets; ++i)
            {
                var selIdx = i;
                for (var j = i + 1; j < countS; ++j)
                {
                    var distJSq = distances[j].distSq;
                    var distSelIdx = distances[selIdx].distSq;
                    var isBetter = close ? distJSq < distSelIdx : distJSq > distSelIdx;
                    if (isBetter)
                        selIdx = j;
                }

                if (selIdx != i)
                {
                    (distances[selIdx], distances[i]) = (distances[i], distances[selIdx]);
                }

                targetsOnSquare.Add(distances[i].actor);
            }

            var countT = targetsOnSquare.Count;
            var isTarget = false;
            var anyNonTank = false;
            for (var i = 0; i < countT; ++i)
            {
                var t = targetsOnSquare[i];
                if (t == actor)
                {
                    isTarget = true;
                }
                else if (t.Role != Role.Tank)
                {
                    anyNonTank = true;
                }
            }

            hints.Add(close ? "Bait close!" : "Bait far!", !isTarget && anyNonTank);
        }
        else
        {
            var count = targets.Count;
            for (var i = 0; i < count; ++i)
            {
                if (targets[i] == actor)
                {
                    hints.Add(close ? "Stay away!" : "Go close!");
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (isClose != null)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            var count = targets.Count;
            var mask = new BitMask();
            for (var i = 0; i < count; ++i)
            {
                if (Raid.FindSlot(targets[i].InstanceID) is var slot2 && slot2 >= 0)
                {
                    mask[slot2] = true;
                }
            }
            hints.AddPredictedDamage(mask, activation, AIHints.PredictedDamageType.Tankbuster);
        }
    }
}
