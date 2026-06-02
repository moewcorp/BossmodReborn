namespace BossMod.Dawntrail.Ultimate.DMU;

// TODO improve hints so it glows red if the player will fall out of the map
// TODO only show the players tether not everyones
class GravenImage(BossModule module) : BossComponent(module) {
    private List<(Actor source, Actor player)> tethers = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID._Gen_Tether_chn_elem0f) {
            var target = WorldState.Actors.Find(tether.Target);

            if (target != null) {
                tethers.Add((source, target));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.PulseWave) {
            tethers.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        foreach (var (source, target) in tethers) {
            var direction = (target.Position - source.Position).Normalized();
            Arena.AddLine(target.Position, target.Position + direction * 9f, Colors.Danger);
        }
    }
}

// TODO add tank buster
// TODO 2nd mechanic - add wave cannon stuff
// TODO 3rd mechanic
// TODO 4th mechanic
