﻿using BossMod.Autorotation.xan;

namespace BossMod.Autorotation.MiscAI;

// TODO this module is now useless and has been merged with AutoFarm, but some plugins still use it, like Questionable (rip)
public sealed class AutoPull(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { QuestBattle, DeepDungeon, EpicEcho, Hunt }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Auto-pull", "Automatically attack passive mobs in certain circumstances", "AI", "xan", RotationModuleQuality.Basic, new(~0ul), 1000, Order: RotationModuleOrder.HighLevel, CanUseWhileRoleplaying: true);

        def.AbilityTrack(Track.QuestBattle, "Automatically attack solo duty bosses");
        def.AbilityTrack(Track.DeepDungeon, "Automatically attack deep dungeon bosses when solo");
        def.AbilityTrack(Track.EpicEcho, "Automatically attack all targets if the Epic Echo status is present (i.e. when unsynced)");
        def.AbilityTrack(Track.Hunt, "Automatically attack hunt marks once they are below 95% HP");

        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (World.Client.CountdownRemaining != null)
            return;

        // TODO set HP threshold lower, or remove entirely? want to avoid getting one guy'd by an early puller
        if (strategy.Enabled(Track.Hunt) && Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.Hunt && Bossmods.ActiveModule?.PrimaryActor is Actor p && p.InCombat && p.HPRatio < 0.95f)
        {
            Hints.SetPriority(p, 0);
            primaryTarget = Hints.ForcedTarget = p;
            return;
        }

        var enabled = false;

        if (strategy.Enabled(Track.QuestBattle))
            enabled |= Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.Quest;

        if (strategy.Enabled(Track.DeepDungeon))
            enabled |= Bossmods.ActiveModule?.Info?.Category == BossModuleInfo.Category.DeepDungeon && World.Party.WithoutSlot().Length == 1;

        if (strategy.Enabled(Track.EpicEcho))
            enabled |= Player.Statuses.Any(s => s.ID == 2734);

        if (enabled)
        {
            Hints.PrioritizeAll();
            Hints.PotentialTargets.Sort((b, a) => a.Priority.CompareTo(b.Priority));
            Hints.HighestPotentialTargetPriority = Math.Max(0, Hints.PotentialTargets[0].Priority);

            if (primaryTarget == null && Hints.PotentialTargets.MinBy(t => t.Actor.DistanceToHitbox(Player)) is AIHints.Enemy tar)
                primaryTarget = Hints.ForcedTarget = tar.Actor;
        }
    }
}

