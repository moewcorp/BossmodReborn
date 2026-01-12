namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

// Cutback Blaze Mechanic needs fixing.
// Icon-marked baited cones: origin at boss, aim at marked players, track them until resolve.
sealed class CutbackBlazeBait(BossModule module) : Components.BaitAwayIcon(
    module,
    new AOEShapeCone(60f, 22.5f.Degrees()),                 // tune angle if needed
    (uint)IconID.CutbackBlazeBait,                    // 664
    (uint)AID.CutbackBlaze1,                                 // remove bait when helper cones appear
    activationDelay: 5.0d,                                   // matches 4.3+0.7s
    centerAtTarget: false)                                   // IMPORTANT: cone tip stays on boss
{
    public static readonly AOEShapeCone Cone = new(60f, 22.5f.Degrees());
}

// Working persistent AOEs for Cutback Blaze cones.
sealed class CutbackBlazePersistent(BossModule module) : Components.GenericAOEs(module, (uint)AID.CutbackBlaze1)
{
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CutbackBlaze1:
                // NOTE: constructor signature in BossModReborn is (shape, pos, rot, activation, color, risky, actorID)
                _aoes.Add(new(CutbackBlazeBait.Cone, caster.Position, caster.Rotation, WorldState.CurrentTime, Colors.AOE, true, caster.InstanceID));
                break;

            // Clear when Divers' Dare resolves (event cast is effectively "cast end" for NPCs)
            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _aoes.Clear();
                break;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        var id = actor.InstanceID;
        _aoes.RemoveAll(a => a.ActorID == id);
    }
}
// Spread markers (helper -> player, 5s cast, range 5 circle)
sealed class AlleyOopInfernoSpread(BossModule module)
    : Components.SpreadFromCastTargets(module, (uint)AID.AlleyOopInferno1, 5f);

// Persistent puddles from Alley-oop Inferno (spread resolves -> leaves puddle; burns if you linger).
// Finally Working!
sealed class AlleyOopInfernoPuddles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _puddles = [];
    private static readonly AOEShapeCircle Shape = new(6f); // tweak if needed

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_puddles);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AlleyOopInferno1: // 46471 helper -> player
            {
                // Create a puddle under each affected target at their CURRENT position (reliable)
                foreach (var t in spell.Targets)
                {
                    var target = WorldState.Actors.Find(t.ID);
                    if (target != null)
                        _puddles.Add(new(Shape, target.Position, default, WorldState.CurrentTime, Colors.AOE, true));
                }
                break;
            }

            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _puddles.Clear();
                break;
        }
    }
}

// Show only 30° maelstroms while they are active; once they resolve, show 15° maelstroms.
sealed class AlleyOopMaelstromSequential(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Tracked> _tracked = [];
    private readonly List<AOEInstance> _active = []; // reused each frame, no allocations
    private static readonly AOEShapeCone Shape30 = new(60f, 15f.Degrees());
    private static readonly AOEShapeCone Shape15 = new(60f, 7.5f.Degrees());

    private struct Tracked
    {
        public AOEInstance AOE;
        public uint AID;
        public Tracked(AOEInstance aoe, uint aid) { AOE = aoe; AID = aid; }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _active.Clear();

        // priority: show all wide cones if any are present
        bool have30 = false;
        foreach (var t in _tracked)
        {
            if (t.AID == (uint)AID.AlleyOopMaelstrom)
            {
                have30 = true;
                break;
            }
        }

        uint want = have30 ? (uint)AID.AlleyOopMaelstrom : (uint)AID.AlleyOopMaelstrom2;

        foreach (var t in _tracked)
        {
            if (t.AID == want)
                _active.Add(t.AOE);
        }

        return CollectionsMarshal.AsSpan(_active);
    }
public override void OnEventCast(Actor caster, ActorCastEvent spell)
{
    if (spell.Action.ID is (uint)AID.AlleyOopMaelstrom or (uint)AID.AlleyOopMaelstrom2)
    {
        // event cast = actual resolve; remove the tracked AOE even if OnCastFinished never triggers
        _tracked.RemoveAll(t => t.AOE.ActorID == caster.InstanceID && t.AID == spell.Action.ID);
    }
}
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.AlleyOopMaelstrom => Shape30,   // 46495
            (uint)AID.AlleyOopMaelstrom2 => Shape15,  // 46496
            _ => null
        };
        if (shape == null)
            return;

        var activation = WorldState.FutureTime(spell.NPCRemainingTime);
        var aoe = new AOEInstance(shape, caster.Position, spell.Rotation, activation, Colors.AOE, true, caster.InstanceID);
        _tracked.Add(new(aoe, spell.Action.ID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AlleyOopMaelstrom or (uint)AID.AlleyOopMaelstrom2)
            _tracked.RemoveAll(t => t.AOE.ActorID == caster.InstanceID && t.AID == spell.Action.ID);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        var id = actor.InstanceID;
        _tracked.RemoveAll(t => t.AOE.ActorID == id);
    }
}
