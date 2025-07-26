namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class HolyLance(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoePrepare;
    private readonly List<AOEInstance> _aoes = new(12);
    private bool prepare;
    public bool Show;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Show)
        {
            return [];
        }
        if (prepare)
        {
            return Utils.ZeroOrOne(ref _aoePrepare);
        }
        else
        {
            return _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LuminousLanceVisual)
        {
            _aoes.RemoveAt(0);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RuneAxe)
        {
            _aoePrepare = new(FTB4Magitaur.CircleMinusSquares, Arena.Center, default, Module.CastFinishAt(spell));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.LuminousLance)
        {
            var a = actor.Position;
            var count = _aoes.Count;
            var act = count == 0 ? WorldState.FutureTime(13d) : _aoes[0].Activation.AddSeconds(2d * count);
            for (var i = 0; i < 3; ++i)
            {
                var pos = FTB4Magitaur.SquarePositions[i];
                if (a.InSquare(pos, 10f, FTB4Magitaur.SquareDirs[i]))
                {
                    _aoes.Add(new(FTB4Magitaur.Square, pos.Quantized(), FTB4Magitaur.SquareAngles[i], act));
                    return;
                }
            }
            _aoes.Add(new(FTB4Magitaur.CircleMinusSquares, Arena.Center, default, act));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.PreyLancepoint)
        {
            prepare = false;
            _aoePrepare = null;
        }
    }
}

sealed class HolyIV(BossModule module) : Components.GenericStackSpread(module)
{
    public readonly List<(int Order, Actor Actor, DateTime expireAt, int squareIndex)> Status = new(9);
    public int NumCasts;
    public BitMask IsCurrentTarget;
    private int numTargets;
    public int[] InitialPositions = new int[PartyState.MaxPartySize];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.PreyLancepoint)
        {
            var expire = status.ExpireAt;
            var order = (expire - WorldState.CurrentTime).TotalSeconds switch
            {
                < 20d => 0,
                < 30d => 1,
                _ => 2
            };
            var count = Status.Count;
            var id = actor.InstanceID;
            var statuses = CollectionsMarshal.AsSpan(Status);
            for (var i = 0; i < count; ++i)
            {
                ref var s = ref statuses[i];
                if (s.Actor.InstanceID == id) // if players get removed from object table and readded later it triggers another OnStatusGain
                {
                    s.Actor = actor; // replace actor reference to get most recent known position

                    var stacks = CollectionsMarshal.AsSpan(Stacks); // do the same for already created stacks
                    var len = stacks.Length;
                    for (var j = 0; j < len; ++j)
                    {
                        ref var stack = ref stacks[j];
                        if (stack.Target.InstanceID == id)
                        {
                            stack.Target = actor;
                            return;
                        }
                    }
                    return;
                }
            }
            var squareIndex = -1;
            var a = actor.Position;
            for (var i = 0; i < 3; ++i)
            {
                if (a.InSquare(FTB4Magitaur.SquarePositions[i], 10f, FTB4Magitaur.SquareDirs[i]))
                {
                    squareIndex = i;
                    break;
                }
            }
            ++numTargets;
            Status.Add((order, actor, expire, squareIndex));

            var party = Raid.WithSlot(false, true, true);
            var lenP = party.Length;
            Array.Fill(InitialPositions, -1);
            var lastSquare = -1;
            for (var i = 0; i < lenP; ++i)
            {
                ref readonly var player = ref party[i];
                for (var j = 0; j < 3; ++j)
                {
                    if (player.Item2.Position.InSquare(FTB4Magitaur.SquarePositions[j], 10f, FTB4Magitaur.SquareDirs[j]))
                    {
                        InitialPositions[player.Item1] = lastSquare = j;
                        break;
                    }
                }
            }
            for (var i = 0; i < 8; ++i)
            {
                if (InitialPositions[i] == -1) // not in a square, assume last assigned square as correct?
                {
                    InitialPositions[i] = lastSquare;
                }
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (numTargets > 9)
        {
            hints.Add($"Too many targets, mechanic potentially unsolveable!");
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor.IsDead && status.ID == (uint)SID.PreyLancepoint) // if player dies the stack will not happen
        {
            var count = Status.Count;
            var id = actor.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (Status[i].Actor.InstanceID == id)
                {
                    Status.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.HolyIV)
        {
            Stacks.Clear();
            IsCurrentTarget = default;
        }
        else if (id == (uint)AID.LuminousLanceVisual)
        {
            ++NumCasts;
            var order = NumCasts switch
            {
                1 => 0,
                5 => 1,
                9 => 2,
                _ => -1
            };
            if (order >= 0)
            {
                var count = Status.Count;
                for (var i = 0; i < count; ++i)
                {
                    var p = Status[i];
                    if (p.Order == order)
                    {
                        Stacks.Add(new(p.Actor, 6f, 16, 16, p.expireAt));
                        IsCurrentTarget[Raid.FindSlot(p.Actor.InstanceID)] = true;
                    }
                }
            }
        }
    }
}

sealed class HolyIVHints(BossModule module) : Components.GenericAOEs(module)
{
    private readonly HolyIV _status = module.FindComponent<HolyIV>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_status.Stacks.Count != 0)
        {
            var count = _status.Status.Count;
            var index = _status.InitialPositions[slot];

            for (var i = 0; i < count; ++i)
            {
                var s = _status.Status[i];
                if (s.squareIndex == index) // there might be no spread for the square if player died
                {
                    var casts = _status.NumCasts;
                    var isOutsideStack = index == 1 && casts < 3 || index == 0 && casts is > 3 and < 7 || index == 2 && casts is > 7 and < 11;
                    return new AOEInstance[1] { new(isOutsideStack ? FTB4Magitaur.StackOutsideSquare[index] : FTB4Magitaur.StackInsideSquare[index], Arena.Center, default, _status.Stacks[0].Activation) };
                }
            }
        }
        return [];
    }
}
