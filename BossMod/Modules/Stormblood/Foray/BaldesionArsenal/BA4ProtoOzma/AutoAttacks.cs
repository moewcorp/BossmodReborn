namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA4ProtoOzma;

class AutoAttacksCube(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly List<Actor> targets = new(3);
    private static readonly AOEShapeRect rect = new(40.5f, 2f);

    // todo: this is a hack, ideally we need to determine who has the current highest enmity on each platform
    // the hack just assumes that people with highest enmity for cube auto attack never changes
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AutoAttackSphere:
                if (targets.Count == 3)
                    targets.Clear();
                targets.Add(WorldState.Actors.Find(spell.MainTargetID)!);
                break;
            case (uint)AID.FlareStarVisual:
                if (caster == Module.PrimaryActor)
                {
                    var activation = WorldState.FutureTime(4.3d);
                    AddBaits(ref activation);
                }
                break;
            case (uint)AID.AutoAttackCube:
                ++NumCasts;
                if (NumCasts % 3 == 0)
                {
                    CurrentBaits.Clear();
                    if (NumCasts <= 42)
                    {
                        var activation = WorldState.FutureTime(2.6d); // time varies wildly depending on current mechanic, taking lowest
                        AddBaits(ref activation);
                    }
                }
                break;
            case (uint)AID.TransfigurationSphere1:
            case (uint)AID.TransfigurationSphere2:
            case (uint)AID.TransfigurationSphere3:
                NumCasts = 0;
                break;
        }
        void AddBaits(ref DateTime activation)
        {
            var count = targets.Count;
            var s = Module.PrimaryActor;
            for (var i = 0; i < count; ++i)
                CurrentBaits.Add(new(s, targets[i], rect, activation));
        }
    }
}

class AutoAttacksPyramid(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private static readonly AOEShapeRect rect = new(100f, 5f);
    private static readonly Angle a120 = 120f.Degrees(), am120 = -120f.Degrees();
    private readonly List<Actor> players = [];
    private bool active;

    // this is just an estimation, targets quickly look random if not in predetermined spots behind platform black hole buffers...
    public override void Update()
    {
        if (active)
        {
            List<Actor> actorsP1 = [];
            List<Actor> actorsP2 = [];
            List<Actor> actorsP3 = [];
            CurrentBaits.Clear();

            var primaryPos = Module.PrimaryActor.Position;
            var countP = players.Count;
            if (countP == 0)
            {
                foreach (var a in Module.WorldState.Actors.Actors.Values)
                    if (a.OID == 0)
                        players.Add(a);
            }

            for (var i = 0; i < countP; ++i)
            {
                var a = players[i];
                var pos = a.Position;
                if (rect.Check(pos, primaryPos, default))
                    actorsP1.Add(a);
                else if (rect.Check(pos, primaryPos, a120))
                    actorsP2.Add(a);
                else if (rect.Check(pos, primaryPos, am120))
                    actorsP3.Add(a);
            }

            var countp1 = actorsP1.Count;
            var countp2 = actorsP2.Count;
            var countp3 = actorsP3.Count;

            SortActors(ref actorsP1);
            SortActors(ref actorsP2);
            SortActors(ref actorsP3);

            var t1 = countp1 != 0 ? actorsP1[countp1 - 1] : null;
            var t2 = countp2 != 0 ? actorsP2[countp2 - 1] : null;
            var t3 = countp3 != 0 ? actorsP3[countp3 - 1] : null;
            AddBait(ref t1);
            AddBait(ref t2);
            AddBait(ref t3);
            void AddBait(ref Actor? t)
            {
                if (t != null)
                    CurrentBaits.Add(new(Module.PrimaryActor, t, circle));
            }
            void SortActors(ref List<Actor> actors)
            => actors.Sort((a, b) =>
                {
                    var distA = (a.Position - primaryPos).LengthSq();
                    var distB = (b.Position - primaryPos).LengthSq();
                    return distA.CompareTo(distB);
                });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ExecrationVisual:
                if (caster == Module.PrimaryActor)
                    active = true;
                break;
            case (uint)AID.TransfigurationSphere1:
            case (uint)AID.TransfigurationSphere2:
            case (uint)AID.TransfigurationSphere3:
                active = false;
                CurrentBaits.Clear();
                break;
        }
    }
}

class AutoAttacksStar(BossModule module) : Components.GenericStackSpread(module)
{
    private bool active;
    private static readonly Angle a120 = 120f.Degrees(), am120 = -120f.Degrees();
    private readonly List<Actor> players = [];
    private static readonly AOEShapeRect rect = new(100f, 5f);

    public override void Update()
    {
        if (active)
        {
            List<Actor> actorsP1 = [];
            List<Actor> actorsP2 = [];
            List<Actor> actorsP3 = [];
            Stacks.Clear();

            var primaryPos = Module.PrimaryActor.Position;
            var countP = players.Count;
            if (countP == 0)
            {
                foreach (var a in Module.WorldState.Actors.Actors.Values)
                    if (a.OID == 0)
                        players.Add(a);
            }

            for (var i = 0; i < countP; ++i)
            {
                var a = players[i];
                var pos = a.Position;
                if (rect.Check(pos, primaryPos, default))
                    actorsP1.Add(a);
                else if (rect.Check(pos, primaryPos, a120))
                    actorsP2.Add(a);
                else if (rect.Check(pos, primaryPos, am120))
                    actorsP3.Add(a);
            }

            var countp1 = actorsP1.Count;
            var countp2 = actorsP2.Count;
            var countp3 = actorsP3.Count;

            SortActors(ref actorsP1);
            SortActors(ref actorsP2);
            SortActors(ref actorsP3);

            var t1 = countp1 != 0 ? actorsP1[0] : null;
            var t2 = countp2 != 0 ? actorsP2[0] : null;
            var t3 = countp3 != 0 ? actorsP3[0] : null;
            AddBait(ref t1);
            AddBait(ref t2);
            AddBait(ref t3);
            void AddBait(ref Actor? t)
            {
                if (t != null)
                    Stacks.Add(new(t, 6f, 2, 24));
            }
            void SortActors(ref List<Actor> actors)
            => actors.Sort((a, b) =>
                {
                    var distA = (a.Position - primaryPos).LengthSq();
                    var distB = (b.Position - primaryPos).LengthSq();
                    return distA.CompareTo(distB);
                });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.MourningStarVisual:
                if (caster == Module.PrimaryActor)
                    active = true;
                break;
            case (uint)AID.TransfigurationSphere1:
            case (uint)AID.TransfigurationSphere2:
            case (uint)AID.TransfigurationSphere3:
                active = false;
                Stacks.Clear();
                break;
        }
    }
}
