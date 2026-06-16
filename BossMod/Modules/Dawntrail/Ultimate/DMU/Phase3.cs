namespace BossMod.Dawntrail.Ultimate.DMU;

// TODO check knockback point is from the boss
class AeroIIIAssault(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AeroIIIAssault, 15f);

// TODO I guess
class TheDecisiveBattle(BossModule module) : BossComponent(module) {
    private Actor? chaosBoss;
    private Actor? exDeathBoss;
    private readonly PartyRolesConfig partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (chaosBoss == null || exDeathBoss == null) {
            return;
        }

        var players = Raid.WithoutSlot().SortedByRange(chaosBoss.Position).Take(4).ToList();
        foreach (var player in players) {
            Arena.AddLine(chaosBoss.Position, player.Position, Colors.Danger);
        }

        players = Raid.WithoutSlot().SortedByRange(exDeathBoss.Position).Take(4).ToList();
        foreach (var player in players) {
            Arena.AddLine(exDeathBoss.Position, player.Position, Colors.Danger);
        }

        var slots = partyConfig.SlotsPerAssignment(Raid);
        if (slots.Length == 0) {
            return;
        }

        var assignment = partyConfig[Raid.Members[pcSlot].ContentId];

        if (assignment == PartyRolesConfig.Assignment.MT) {
            Arena.AddCircle(new(92 - MathF.Sqrt(2), 100 - MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.H1) {
            Arena.AddCircle(new(92 + MathF.Sqrt(2), 100 - MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.M1) {
            Arena.AddCircle(new(92 - MathF.Sqrt(2), 100 + MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.M2) {
            Arena.AddCircle(new(92 + MathF.Sqrt(2), 100 + MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.OT) {
            Arena.AddCircle(new(108 - MathF.Sqrt(2), 100 - MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.H2) {
            Arena.AddCircle(new(108 + MathF.Sqrt(2), 100 - MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.R1) {
            Arena.AddCircle(new(108 - MathF.Sqrt(2), 100 + MathF.Sqrt(2)), 1, Colors.Safe);
        }

        if (assignment == PartyRolesConfig.Assignment.R2) {
            Arena.AddCircle(new(108 + MathF.Sqrt(2), 100 + MathF.Sqrt(2)), 1, Colors.Safe);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TheDecisiveBattle) {
            chaosBoss = caster;
        }

        if (spell.Action.ID == (uint)AID.TheDecisiveBattle1) {
            exDeathBoss = caster;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TheDecisiveBattle || spell.Action.ID == (uint)AID.TheDecisiveBattle1) {
            chaosBoss = null;
            exDeathBoss = null;
        }
    }
}
