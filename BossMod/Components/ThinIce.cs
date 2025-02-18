namespace BossMod.Components;

// component for ThinIce mechanic
// observation: for SID 911 the distance is 0.1 * status extra
public abstract class ThinIce(BossModule module, float distance, bool createforbiddenzones = false, uint statusID = 911, bool stopAtWall = false, bool stopAfterWall = false) : Knockback(module, stopAtWall: stopAtWall, stopAfterWall: stopAfterWall)
{
    public readonly uint StatusID = statusID;
    public readonly float Distance = distance;
    private static readonly WDir offset = new(0f, 1f);
    public BitMask Mask;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Mask[slot] != default)
            yield return new(actor.Position, Distance, default, default, actor.Rotation, Kind.DirForward);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == StatusID)
            Mask.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == StatusID)
            Mask[Raid.FindSlot(actor.InstanceID)] = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CalculateMovements(slot, actor).Any(e => DestinationUnsafe(slot, actor, e.to)))
            hints.Add("You are risking to slide into danger!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (Mask[pcSlot] != default)
            Arena.AddCircle(pc.Position, Distance, Colors.Vulnerable);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (createforbiddenzones && Mask[slot] != default)
        {
            var pos = actor.Position;
            var ddistance = 2f * Distance;
            var forbidden = new Func<WPos, float>[3]
            {
                ShapeDistance.InvertedDonut(pos, Distance, Distance + 1.2f),
                ShapeDistance.InvertedDonut(pos, ddistance, ddistance + 1.2f),
                ShapeDistance.InvertedRect(pos, offset, 0.5f, 0.5f, 0.5f)
            };
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), DateTime.MaxValue);
        }
    }
}
