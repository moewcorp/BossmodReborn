namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class DecisiveBattle(BossModule module) : BossComponent(module)
{
    public readonly Actor?[] AssignedBoss = new Actor?[PartyState.MaxAllianceSize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (slot < PartyState.MaxAllianceSize && AssignedBoss[slot] != null)
        {
            var target = WorldState.Actors.Find(actor.TargetID);
            if (target != null && target != AssignedBoss[slot] && (OID)target.OID is OID.BossMR or OID.BossTT or OID.BossGK)
                hints.Add($"Target {AssignedBoss[slot]?.Name}!");
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.DecisiveBattle && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0)
        {
            AssignedBoss[slot] = WorldState.Actors.Find(tether.Target);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (slot < AssignedBoss.Length && AssignedBoss[slot] != null)
            for (var i = 0; i < hints.PotentialTargets.Count; ++i)
            {
                var enemy = hints.PotentialTargets[i];
                if (enemy.Actor != AssignedBoss[slot])
                    enemy.Priority = AIHints.Enemy.PriorityForbidFully;
            }
    }
}
