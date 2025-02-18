﻿namespace BossMod.Autorotation.Utility;
//Contribution by Akechi
//Discord @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class RolePvPUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track
    {
        Elixir,
        Recuperate,
        Guard,
        Purify,
        Sprint,
    }
    public enum ElixirStrategy
    {
        Automatic,
        Close,
        Mid,
        Far,
        Force,
        Hold
    }

    public enum RecuperateStrategy
    {
        Automatic,
        Seventy,
        Fifty,
        Thirty,
        Force,
        Hold
    }

    public enum GuardStrategy
    {
        Automatic,
        Seventy,
        Fifty,
        Thirty,
        Force,
        Hold
    }

    public enum DefensiveStrategy
    {
        Automatic,
        Force,
        Delay
    }
    #endregion

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PvP", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic,
            BitMask.Build(
                Class.PLD, Class.WAR, Class.DRK, Class.GNB,
                Class.WHM, Class.SCH, Class.AST, Class.SGE,
                Class.MNK, Class.DRG, Class.NIN, Class.SAM, Class.RPR, Class.VPR,
                Class.BRD, Class.MCH, Class.DNC,
                Class.BLM, Class.SMN, Class.RDM, Class.PCT), 100, 30);

        res.Define(Track.Elixir).As<ElixirStrategy>("Elixir", uiPriority: 150)
            .AddOption(ElixirStrategy.Automatic, "Automatic")
            .AddOption(ElixirStrategy.Close, "Close")
            .AddOption(ElixirStrategy.Mid, "Mid")
            .AddOption(ElixirStrategy.Far, "Far")
            .AddOption(ElixirStrategy.Force, "Force")
            .AddOption(ElixirStrategy.Hold, "Hold")
            .AddAssociatedActions(ClassShared.AID.Elixir);
        res.Define(Track.Recuperate).As<RecuperateStrategy>("Recuperate", uiPriority: 150)
            .AddOption(RecuperateStrategy.Automatic, "Automatic")
            .AddOption(RecuperateStrategy.Seventy, "Seventy")
            .AddOption(RecuperateStrategy.Fifty, "Fifty")
            .AddOption(RecuperateStrategy.Thirty, "Thirty")
            .AddOption(RecuperateStrategy.Force, "Force")
            .AddOption(RecuperateStrategy.Hold, "Hold")
            .AddAssociatedActions(ClassShared.AID.Recuperate);
        res.Define(Track.Guard).As<GuardStrategy>("Guard", uiPriority: 150)
            .AddOption(GuardStrategy.Automatic, "Automatic")
            .AddOption(GuardStrategy.Seventy, "Seventy")
            .AddOption(GuardStrategy.Fifty, "Fifty")
            .AddOption(GuardStrategy.Thirty, "Thirty")
            .AddOption(GuardStrategy.Force, "Force")
            .AddOption(GuardStrategy.Hold, "Hold")
            .AddAssociatedActions(ClassShared.AID.Guard);
        res.Define(Track.Purify).As<DefensiveStrategy>("Purify", uiPriority: 150)
            .AddOption(DefensiveStrategy.Automatic, "Automatic")
            .AddOption(DefensiveStrategy.Force, "Force")
            .AddOption(DefensiveStrategy.Delay, "Delay")
            .AddAssociatedActions(ClassShared.AID.Purify);
        res.Define(Track.Sprint).As<DefensiveStrategy>("Sprint", uiPriority: 150)
            .AddOption(DefensiveStrategy.Automatic, "Automatic")
            .AddOption(DefensiveStrategy.Force, "Force")
            .AddOption(DefensiveStrategy.Delay, "Delay")
            .AddAssociatedActions(ClassShared.AID.Sprint);
        return res;
    }

    #region Priorities
    public enum GCDPriority
    {
        None = 0,
        Elixir = 500,
        ForcedGCD = 900,
    }
    public enum OGCDPriority
    {
        None = 0,
        Sprint = 300,
        Recuperate = 400,
        Guard = 600,
        Purify = 700,
        ForcedOGCD = 900,
    }
    #endregion

    #region Placeholders for Variables
    private bool hasSprint;
    private bool canElixir;
    private bool canRecuperate;
    private bool canGuard;
    private bool canPurify;
    private bool canSprint;
    public float GCDLength;
    #endregion

    #region Module Helpers
    private bool In10y(Actor? target) => Player.DistanceToHitbox(target) <= 9.9;
    private bool In20y(Actor? target) => Player.DistanceToHitbox(target) <= 19.9;
    private bool In30y(Actor? target) => Player.DistanceToHitbox(target) <= 29.9;
    private bool IsOffCooldown(ClassShared.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f;
    #endregion
    public float DebuffsLeft(Actor? target)
    {
        return target == null ? 0f
            : Utils.MaxAll(
            StatusDetails(target, ClassShared.SID.Silence, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.StunPvP, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.Bind, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.Heavy, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.Sleep, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.HalfAsleep, Player.InstanceID, 5).Left
        );
    }
    public bool HasAnyDebuff(Actor? target) => DebuffsLeft(target) > 0;

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        #region Variables
        hasSprint = Player.FindStatus(ClassShared.SID.SprintPvP) != null;

        #region Minimal Requirements
        canElixir = IsOffCooldown(ClassShared.AID.Elixir) && strategy.Option(Track.Elixir).As<ElixirStrategy>() != ElixirStrategy.Hold;
        canRecuperate = Player.HPMP.CurMP >= 2500 && strategy.Option(Track.Recuperate).As<RecuperateStrategy>() != RecuperateStrategy.Hold;
        canGuard = IsOffCooldown(ClassShared.AID.Guard) && strategy.Option(Track.Guard).As<GuardStrategy>() != GuardStrategy.Hold;
        canPurify = IsOffCooldown(ClassShared.AID.Purify) && strategy.Option(Track.Purify).As<DefensiveStrategy>() != DefensiveStrategy.Delay;
        canSprint = !hasSprint && strategy.Option(Track.Sprint).As<DefensiveStrategy>() != DefensiveStrategy.Delay;
        #endregion
        #endregion

        var elixirStrat = strategy.Option(Track.Elixir).As<ElixirStrategy>();
        if (ShouldUseElixir(elixirStrat, primaryTarget))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Elixir), Player, strategy.Option(Track.Elixir).Priority(), strategy.Option(Track.Elixir).Value.ExpireIn);

        var recuperateStrat = strategy.Option(Track.Recuperate).As<RecuperateStrategy>();
        if (ShouldUseRecuperate(recuperateStrat))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Recuperate), Player, strategy.Option(Track.Recuperate).Priority(), strategy.Option(Track.Recuperate).Value.ExpireIn);

        var guardStrat = strategy.Option(Track.Guard).As<GuardStrategy>();
        if (ShouldUseGuard(guardStrat))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Guard), Player, strategy.Option(Track.Guard).Priority(), strategy.Option(Track.Guard).Value.ExpireIn);

        var purifyStrat = strategy.Option(Track.Purify).As<DefensiveStrategy>();
        if (ShouldUsePurify(purifyStrat, primaryTarget))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Purify), Player, strategy.Option(Track.Purify).Priority(), strategy.Option(Track.Purify).Value.ExpireIn);

        var sprintStrat = strategy.Option(Track.Sprint).As<DefensiveStrategy>();
        if (ShouldUseSprint(sprintStrat))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), Player, strategy.Option(Track.Sprint).Priority(), strategy.Option(Track.Sprint).Value.ExpireIn);
    }

    //TODO: fix this later
    #region Core Execution Helpers
    /*
    public ClassShared.AID NextGCD; //Next global cooldown action to be used
    public GCDPriority NextGCDPrio; //Priority of the next GCD for cooldown decision making

    private void QueueGCD(ClassShared.AID aid, Actor? target, GCDPriority prio)
    {
        if (prio != GCDPriority.None)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + (int)prio);
            if (prio > NextGCDPrio)
            {
                NextGCD = aid;
                NextGCDPrio = prio;
            }
        }
    }
    private void QueueOGCD(ClassShared.AID aid, Actor? target, OGCDPriority prio, float basePrio = ActionQueue.Priority.Medium)
    {
        if (prio != OGCDPriority.None)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, basePrio + (int)prio);
        }
    }
    */
    #endregion

    public bool ShouldUseElixir(ElixirStrategy strategy, Actor? target) => strategy switch
    {
        ElixirStrategy.Automatic =>
            canElixir &&
            Player.HPMP.CurHP <= 2500 &&
            (In20y(target) || target != null),
        ElixirStrategy.Close => Player.HPMP.CurHP <= 4000 && In10y(target),
        ElixirStrategy.Mid => Player.HPMP.CurHP <= 4000 && In20y(target),
        ElixirStrategy.Far => Player.HPMP.CurHP <= 4000 && In30y(target),
        ElixirStrategy.Force => canElixir,
        ElixirStrategy.Hold => false,
        _ => false,
    };

    public bool ShouldUseRecuperate(RecuperateStrategy strategy) => strategy switch
    {
        RecuperateStrategy.Automatic =>
            canRecuperate &&
            Player.HPMP.CurHP <= 4000,
        RecuperateStrategy.Seventy => canRecuperate && Player.HPMP.CurHP <= 7000,
        RecuperateStrategy.Fifty => canRecuperate && Player.HPMP.CurHP <= 5000,
        RecuperateStrategy.Thirty => canRecuperate && Player.HPMP.CurHP <= 3000,
        RecuperateStrategy.Force => canRecuperate,
        RecuperateStrategy.Hold => false,
        _ => false,
    };

    public bool ShouldUseGuard(GuardStrategy strategy) => strategy switch
    {
        GuardStrategy.Automatic =>
            canGuard &&
            Player.HPMP.CurHP <= 3500,
        GuardStrategy.Seventy => canGuard && Player.HPMP.CurHP <= 7000,
        GuardStrategy.Fifty => canGuard && Player.HPMP.CurHP <= 5000,
        GuardStrategy.Thirty => canGuard && Player.HPMP.CurHP <= 3000,
        GuardStrategy.Force => canGuard,
        GuardStrategy.Hold => false,
        _ => false,
    };

    public bool ShouldUsePurify(DefensiveStrategy strategy, Actor? target) => strategy switch
    {
        DefensiveStrategy.Automatic =>
            canPurify &&
            HasAnyDebuff(target),
        DefensiveStrategy.Force => canPurify,
        DefensiveStrategy.Delay => false,
        _ => false,
    };

    public bool ShouldUseSprint(DefensiveStrategy strategy) => strategy switch
    {
        DefensiveStrategy.Automatic =>
            !Player.InCombat &&
            canSprint,
        DefensiveStrategy.Force => true,
        DefensiveStrategy.Delay => false,
        _ => false,
    };
}
