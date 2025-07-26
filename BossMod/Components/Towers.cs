namespace BossMod.Components;

public class GenericTowers(BossModule module, uint aid = default, bool prioritizeInsufficient = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide) : CastCounter(module, aid)
{
    public struct Tower(WPos position, AOEShape shape, int minSoakers = 1, int maxSoakers = 1, BitMask forbiddenSoakers = default, DateTime activation = default, Angle rotation = default, ulong actorID = default)
    {
        public Tower(WPos position, float radius, int minSoakers = 1, int maxSoakers = 1, BitMask forbiddenSoakers = default, DateTime activation = default, ulong actorID = default) : this(position, new AOEShapeCircle(radius), minSoakers, maxSoakers, forbiddenSoakers, activation, default, actorID) { }
        public WPos Position = position;
        public Angle Rotation = rotation;
        public AOEShape Shape = shape;
        public int MinSoakers = minSoakers;
        public int MaxSoakers = maxSoakers;
        public BitMask ForbiddenSoakers = forbiddenSoakers;
        public DateTime Activation = activation;
        public ulong ActorID = actorID;

        public readonly bool IsInside(WPos pos) => Shape.Check(pos, Position, Rotation);
        public readonly bool IsInside(Actor actor) => IsInside(actor.Position);

        public readonly int NumInside(BossModule module)
        {
            var count = 0;
            var party = module.Raid.WithSlot();
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var indexActor = ref party[i];
                if (!ForbiddenSoakers[indexActor.Item1] && Shape.Check(indexActor.Item2.Position, Position, Rotation))
                    ++count;
            }
            return count;
        }
        public readonly bool CorrectAmountInside(BossModule module) => NumInside(module) is var count && count >= MinSoakers && count <= MaxSoakers;
        public readonly bool InsufficientAmountInside(BossModule module) => NumInside(module) is var count && count < MaxSoakers;
        public readonly bool TooManyInside(BossModule module) => NumInside(module) is var count && count > MaxSoakers;
    }

    public List<Tower> Towers = [];
    public readonly bool PrioritizeInsufficient = prioritizeInsufficient; // give priority to towers with more than 0 but less than min soakers
    public readonly AIHints.PredictedDamageType DamageType = damageType;

    public virtual ReadOnlySpan<Tower> ActiveTowers(int slot, Actor actor) => CollectionsMarshal.AsSpan(Towers);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var towers = ActiveTowers(slot, actor);
        var len = towers.Length;
        if (len == 0)
            return;
        var gtfoFromTower = false;
        for (var i = 0; i < len; ++i)
        {
            var t = towers[i];
            if (t.ForbiddenSoakers[slot] && t.IsInside(actor))
            {
                gtfoFromTower = true;
                break;
            }
        }

        if (gtfoFromTower)
        {
            hints.Add("GTFO from tower!");
        }
        else // Find index of a tower that is not forbidden and the actor is inside
        {
            var soakedIndex = -1;
            for (var i = 0; i < len; ++i)
            {
                var t = towers[i];
                if (!t.ForbiddenSoakers[slot] && t.IsInside(actor))
                {
                    soakedIndex = i;
                    break;
                }
            }

            if (soakedIndex >= 0) // If a suitable tower is found
            {
                var count2 = towers[soakedIndex].NumInside(Module);
                if (count2 < towers[soakedIndex].MinSoakers)
                    hints.Add("Too few soakers in the tower!");
                else if (count2 > towers[soakedIndex].MaxSoakers)
                    hints.Add("Too many soakers in the tower!");
                else
                    hints.Add("Soak the tower!", false);
            }
            else // Check if any tower has insufficient soakers
            {
                var insufficientSoakers = false;
                for (var i = 0; i < len; ++i)
                {
                    var t = towers[i];
                    if (!t.ForbiddenSoakers[slot] && t.InsufficientAmountInside(Module))
                    {
                        insufficientSoakers = true;
                        break;
                    }
                }
                if (insufficientSoakers)
                    hints.Add("Soak the tower!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var towers = ActiveTowers(pcSlot, pc);
        var len = towers.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var t = ref towers[i];
            if (t.ForbiddenSoakers[pcSlot])
                continue;
            var isInside = t.IsInside(pc);
            var numInside = t.NumInside(Module);
            var safe = numInside < t.MaxSoakers || isInside && numInside <= t.MaxSoakers;
            if (safe)
                t.Shape.Outline(Arena, t.Position, t.Rotation, Colors.Safe, 2f);
            else if (isInside && numInside > t.MaxSoakers) // player is inside but tower has more players than needed
                t.Shape.Outline(Arena, t.Position, t.Rotation, default, 2f);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var towers = ActiveTowers(pcSlot, pc);
        var len = towers.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var t = ref towers[i];
            if (t.ForbiddenSoakers[pcSlot] || !t.IsInside(pc) && t.NumInside(Module) >= t.MaxSoakers)
            {
                t.Shape.Draw(Arena, t.Position, t.Rotation);
                continue;
            }
        }
    }

    // default tower styling
    public static void DrawTower(MiniArena arena, ref Tower tower, bool safe)
    {
        tower.Shape.Outline(arena, tower.Position, tower.Rotation, safe ? Colors.Safe : default, 2f);
    }
    public static void DrawTower(MiniArena arena, WPos pos, float radius, bool safe)
    {
        arena.AddCircle(pos, radius, safe ? Colors.Safe : default, 2f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var towers = ActiveTowers(slot, actor);
        var len = towers.Length;
        if (len == 0)
            return;

        var forbiddenInverted = new List<Func<WPos, float>>(len);
        var forbidden = new List<Func<WPos, float>>(len);

        var hasForbiddenSoakers = false;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var t = ref towers[i];
            if (t.ForbiddenSoakers[slot])
            {
                hasForbiddenSoakers = true;
                break;
            }
        }
        if (!hasForbiddenSoakers)
        {
            if (PrioritizeInsufficient)
            {
                List<Tower> insufficientTowers = new(len);
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var t = ref towers[i];
                    if (t.InsufficientAmountInside(Module) && t.NumInside(Module) != 0)
                        insufficientTowers.Add(t);
                }
                Tower? mostRelevantTower = null;
                var pos = actor.Position;
                var countI = insufficientTowers.Count;
                for (var i = 0; i < countI; ++i)
                {
                    ref readonly var t = ref insufficientTowers.Ref(i);
                    var numInside = t.NumInside(Module);
                    if (mostRelevantTower == null || mostRelevantTower is Tower towe && towe.NumInside(Module) is var num && (numInside > num || numInside == num &&
                        (t.Position - pos).LengthSq() < (towe.Position - pos).LengthSq()))
                        mostRelevantTower = t;
                }
                if (mostRelevantTower is Tower tow)
                {
                    forbiddenInverted.Add(tow.Shape.InvertedDistance(tow.Position, tow.Rotation));
                }
            }
            var inTower = false;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var t = ref towers[i];
                if (t.IsInside(actor) && t.CorrectAmountInside(Module))
                {
                    inTower = true;
                    break;
                }
            }

            var missingSoakers = !inTower;
            if (missingSoakers)
            {
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var t = ref towers[i];
                    if (t.InsufficientAmountInside(Module))
                    {
                        missingSoakers = true;
                        break;
                    }
                }
            }
            if (forbiddenInverted.Count == 0)
            {
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var t = ref towers[i];
                    var isInside = t.IsInside(actor);
                    var correctAmount = t.CorrectAmountInside(Module);
                    if (t.InsufficientAmountInside(Module) || isInside && correctAmount)
                    {
                        forbiddenInverted.Add(t.Shape.InvertedDistance(t.Position, t.Rotation));
                    }
                    else if (t.TooManyInside(Module) || !isInside && correctAmount)
                    {
                        forbidden.Add(t.Shape.Distance(t.Position, t.Rotation));
                    }
                }
            }
            var fcount = forbidden.Count;
            if (fcount == 0 || inTower || missingSoakers && forbiddenInverted.Count != 0)
            {
                hints.AddForbiddenZone(ShapeDistance.Intersection(forbiddenInverted), Towers.Ref(0).Activation);
            }
            else if (fcount != 0 && !inTower)
            {
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Towers.Ref(0).Activation);
            }
        }
        else
        {
            for (var i = 0; i < len; ++i)
            {
                ref readonly var t = ref towers[i];
                forbidden.Add(t.Shape.Distance(t.Position, t.Rotation));
            }
            var fcount = forbidden.Count;
            if (fcount != 0)
            {
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Towers.Ref(0).Activation);
            }
        }
        BitMask mask = default;
        var actors = Module.Raid.WithSlot();
        var acount = actors.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var t = ref towers[i];
            for (var j = 0; j < acount; ++j)
            {
                ref readonly var indexActor = ref actors[j];
                if (!t.ForbiddenSoakers[indexActor.Item1] && t.Shape.Check(indexActor.Item2.Position, t.Position, t.Rotation))
                {
                    mask.Set(indexActor.Item1);
                }
            }
        }
        hints.AddPredictedDamage(mask, Towers.Ref(0).Activation, DamageType);
    }
}

public class CastTowers(BossModule module, uint aid, float radius, int minSoakers = 1, int maxSoakers = 1, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide) : GenericTowers(module, aid, damageType: damageType)
{
    public readonly float Radius = radius;
    public readonly int MinSoakers = minSoakers;
    public readonly int MaxSoakers = maxSoakers;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Towers.Add(new(spell.LocXZ, Radius, MinSoakers, MaxSoakers, activation: Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var id = caster.InstanceID;
            var count = Towers.Count;
            var towers = CollectionsMarshal.AsSpan(Towers);
            for (var i = 0; i < count; ++i)
            {
                ref var tower = ref towers[i];
                if (tower.ActorID == id)
                {
                    Towers.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

// for tower mechanics in open world since likely not everyone is in your party
public class GenericTowersOpenWorld(BossModule module, uint aid = default, bool prioritizeInsufficient = false, bool prioritizeEmpty = false) : CastCounter(module, aid)
{
    public sealed class Tower(WPos position, float radius, int minSoakers = 1, int maxSoakers = 1, HashSet<Actor>? allowedSoakers = null, DateTime activation = default, ulong actorID = default)
    {
        public WPos Position = position;
        public float Radius = radius;
        public int MinSoakers = minSoakers;
        public int MaxSoakers = maxSoakers;
        public HashSet<Actor>? AllowedSoakers = allowedSoakers;
        public DateTime Activation = activation;
        public ulong ActorID = actorID;

        public bool IsInside(WPos pos) => pos.InCircle(Position, Radius);
        public bool IsInside(Actor actor) => IsInside(actor.Position);

        public int NumInside(BossModule module)
        {
            var count = 0;
            var allowedSoakers = AllowedSoakers ??= Soakers(module);
            foreach (var a in allowedSoakers)
            {
                if (a.Position.InCircle(Position, Radius))
                    ++count;
            }
            return count;
        }

        public bool CorrectAmountInside(BossModule module) => NumInside(module) is var count && count >= MinSoakers && count <= MaxSoakers;
        public bool InsufficientAmountInside(BossModule module) => NumInside(module) is var count && count < MaxSoakers;
        public bool TooManyInside(BossModule module) => NumInside(module) is var count && count > MaxSoakers;

        public void InitializeAllowedSoakers(BossModule module)
        {
            AllowedSoakers ??= Soakers(module);
        }
    }

    protected static HashSet<Actor> Soakers(BossModule module)
    {
        HashSet<Actor> actors = new(module.WorldState.Actors.Actors.Values.Count);
        foreach (var a in module.WorldState.Actors.Actors.Values)
            if (a.OID == default)
                actors.Add(a);
        return actors;
    }

    protected static HashSet<Actor> TankSoakers(BossModule module)
    {
        HashSet<Actor> actors = new(module.WorldState.Actors.Actors.Values.Count);
        foreach (var a in module.WorldState.Actors.Actors.Values)
            if (a.OID == default && a.Role == Role.Tank)
                actors.Add(a);
        return actors;
    }

    public readonly List<Tower> Towers = [];
    public readonly bool PrioritizeInsufficient = prioritizeInsufficient; // give priority to towers with more than 0 but less than min soakers

    public virtual ReadOnlySpan<Tower> ActiveTowers(int slot, Actor actor) => CollectionsMarshal.AsSpan(Towers);

    public readonly bool PrioritizeEmpty = prioritizeEmpty; // give priority to towers with 0 soakers

    // default tower styling
    public static void DrawTower(MiniArena arena, WPos pos, float radius, bool safe)
    {
        arena.AddCircle(pos, radius, safe ? Colors.Safe : default, 2f);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var towers = ActiveTowers(slot, actor);
        var len = towers.Length;
        if (len == 0)
            return;
        var gtfoFromTower = false;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var t = ref towers[i];
            t.InitializeAllowedSoakers(Module);
            if (!t.AllowedSoakers!.Contains(actor) && t.IsInside(actor))
            {
                gtfoFromTower = true;
                break;
            }
        }

        if (gtfoFromTower)
        {
            hints.Add("GTFO from tower!");
        }
        else // Find index of a tower that is not forbidden and the actor is inside
        {
            var soakedIndex = -1;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var t = ref towers[i];
                if (t.AllowedSoakers!.Contains(actor) && t.IsInside(actor))
                {
                    soakedIndex = i;
                    break;
                }
            }

            if (soakedIndex >= 0) // If a suitable tower is found
            {
                var count2 = towers[soakedIndex].NumInside(Module);
                if (count2 < towers[soakedIndex].MinSoakers)
                    hints.Add("Too few soakers in the tower!");
                else if (count2 > towers[soakedIndex].MaxSoakers)
                    hints.Add("Too many soakers in the tower!");
                else
                    hints.Add("Soak the tower!", false);
            }
            else // Check if any tower has insufficient soakers
            {
                var insufficientSoakers = false;
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var t = ref towers[i];
                    if (t.AllowedSoakers!.Contains(actor) && t.InsufficientAmountInside(Module))
                    {
                        insufficientSoakers = true;
                        break;
                    }
                }
                if (insufficientSoakers)
                    hints.Add("Soak the tower!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var towers = ActiveTowers(pcSlot, pc);
        var len = towers.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var t = ref towers[i];
            t.InitializeAllowedSoakers(Module);
            DrawTower(Arena, t.Position, t.Radius, t.AllowedSoakers!.Contains(pc) && !t.IsInside(pc) && t.NumInside(Module) < t.MaxSoakers || t.IsInside(pc) && t.AllowedSoakers.Contains(pc) && t.NumInside(Module) <= t.MaxSoakers);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var towers = ActiveTowers(slot, actor);
        var len = towers.Length;
        if (len == 0)
            return;
        var forbiddenInverted = new List<Func<WPos, float>>(len);
        var forbidden = new List<Func<WPos, float>>(len);

        var hasForbiddenSoakers = false;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var t = ref towers[i];
            t.InitializeAllowedSoakers(Module);
            if (!t.AllowedSoakers!.Contains(actor))
            {
                hasForbiddenSoakers = true;
                break;
            }
        }
        if (!hasForbiddenSoakers)
        {
            if (PrioritizeEmpty)
            {
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var t = ref towers[i];
                    if (t.NumInside(Module) == 0)
                        forbiddenInverted.Add(ShapeDistance.InvertedCircle(t.Position, t.Radius));
                }
            }
            else if (PrioritizeInsufficient) // less soakers than max
            {
                List<Tower> insufficientTowers = new(len);
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var t = ref towers[i];
                    if (t.InsufficientAmountInside(Module) && t.NumInside(Module) != 0)
                        insufficientTowers.Add(t);
                }
                Tower? mostRelevantTower = null;
                var countI = insufficientTowers.Count;
                for (var i = 0; i < countI; ++i)
                {
                    var t = insufficientTowers[i];
                    if (mostRelevantTower == null || t.NumInside(Module) > mostRelevantTower.NumInside(Module) || t.NumInside(Module) == mostRelevantTower.NumInside(Module) &&
                        (t.Position - actor.Position).LengthSq() < (mostRelevantTower.Position - actor.Position).LengthSq())
                        mostRelevantTower = t;
                }
                if (mostRelevantTower != null)
                    forbiddenInverted.Add(ShapeDistance.InvertedCircle(mostRelevantTower.Position, mostRelevantTower.Radius));
            }
            var inTower = false;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var t = ref towers[i];
                if (t.IsInside(actor) && t.CorrectAmountInside(Module))
                {
                    inTower = true;
                    break;
                }
            }

            var missingSoakers = !inTower;
            if (missingSoakers)
            {
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var t = ref towers[i];
                    if (t.InsufficientAmountInside(Module))
                    {
                        missingSoakers = true;
                        break;
                    }
                }
            }
            if (forbiddenInverted.Count == 0)
            {
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var t = ref towers[i];
                    if (t.InsufficientAmountInside(Module) || t.IsInside(actor) && t.CorrectAmountInside(Module))
                    {
                        forbiddenInverted.Add(ShapeDistance.InvertedCircle(t.Position, t.Radius));
                    }
                    else if (t.TooManyInside(Module) || !t.IsInside(actor) && t.CorrectAmountInside(Module))
                    {
                        forbidden.Add(ShapeDistance.Circle(t.Position, t.Radius));
                    }
                }
            }
            var ficount = forbiddenInverted.Count;
            var fcount = forbidden.Count;
            if (fcount == 0 || inTower || missingSoakers && ficount != 0)
            {
                hints.AddForbiddenZone(ShapeDistance.Intersection(forbiddenInverted), towers[0].Activation);
            }
            else if (fcount != 0 && !inTower)
            {
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), towers[0].Activation);
            }
        }
        else
        {
            for (var i = 0; i < len; ++i)
            {
                ref readonly var t = ref towers[i];
                forbidden.Add(ShapeDistance.Circle(t.Position, t.Radius));
            }

            if (forbidden.Count != 0)
            {
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), towers[0].Activation);
            }
        }
    }
}

public class CastTowersOpenWorld(BossModule module, uint aid, float radius, int minSoakers = 1, int maxSoakers = 1, bool prioritizeInsufficient = false, bool prioritizeEmpty = false) : GenericTowersOpenWorld(module, aid, prioritizeInsufficient, prioritizeEmpty)
{
    public readonly float Radius = radius;
    public readonly int MinSoakers = minSoakers;
    public readonly int MaxSoakers = maxSoakers;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Towers.Add(new(spell.LocXZ, Radius, MinSoakers, MaxSoakers, activation: Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var id = caster.InstanceID;
            var count = Towers.Count;
            var towers = CollectionsMarshal.AsSpan(Towers);
            for (var i = 0; i < count; ++i)
            {
                ref var tower = ref towers[i];
                if (tower.ActorID == id)
                {
                    Towers.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
