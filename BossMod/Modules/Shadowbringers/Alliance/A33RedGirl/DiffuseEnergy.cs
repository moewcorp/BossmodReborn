namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class DiffuseEnergy(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone cone = new(12f, 60f.Degrees());
    private readonly List<(Actor source, Actor target)> tethers = new(4);
    private readonly Actor?[] sources = new Actor[2];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID != default && tether.ID == (uint)TetherID.ChildsPlay && WorldState.Actors.Find(tether.Target) is Actor t)
        {
            tethers.Add((source, t));
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = iconID switch
        {
            (uint)IconID.RotateCW => -120f.Degrees(),
            (uint)IconID.RotateCCW => 120f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            var count = tethers.Count;
            var isTethered = false;
            var index = -1;
            for (var i = 0; i < count; ++i)
            {
                if (tethers[i].source == actor)
                {
                    isTethered = true;
                    index = i;
                    break;
                }
            }
            Sequences.Add(new(cone, WPos.ClampToGrid(actor.Position), actor.Rotation, increment, WorldState.FutureTime(5d), 2.7d, isTethered ? 2 : 6, actorID: targetID));

            if (!isTethered)
            {
                return;
            }
            // add moving rotations
            var t = tethers[index];
            if (t.target == sources[0])
            {
                AddSequence(new(default, -16.9f));
                return;
            }
            if (t.target == sources[1])
            {
                AddSequence(new(default, 16.9f));
                return;
            }
            void AddSequence(WDir offset)
            {
                Sequences.Add(new(cone, WPos.ClampToGrid(actor.Position + offset), actor.Rotation + 2f * increment, increment, WorldState.FutureTime(13.3d), 2.7d, 3));
                tethers.RemoveAt(index);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChildsPlay1:
                sources[0] = caster;
                break;
            case (uint)AID.ChildsPlay2:
                sources[1] = caster;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DiffuseEnergyFirst or (uint)AID.DiffuseEnergyRest)
        {
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
        }
    }
}
