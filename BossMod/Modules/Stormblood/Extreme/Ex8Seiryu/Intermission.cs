namespace BossMod.Stormblood.Extreme.Ex8Seiryu;

sealed class RedRush(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(82.6f, 2.5f), (uint)TetherID.RedRush, (uint)AID.RedRush, activationDelay: 6d)
{
    private readonly BlueBolt _stack = module.FindComponent<BlueBolt>()!;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID != (uint)OID.AkaNoShiki)
        {
            return;
        }
        base.OnTethered(source, tether);
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
        {
            _stack.ForbiddenPlayers.Set(Raid.FindSlot(player.InstanceID));
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID != (uint)OID.AkaNoShiki)
        {
            return;
        }
        base.OnUntethered(source, tether);
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
        {
            _stack.ForbiddenPlayers.Clear(Raid.FindSlot(player.InstanceID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaitsOn(actor).Count != 0)
        {
            hints.AddForbiddenZone(Arena.Bounds.Radius >= 20f ? ShapeDistance.InvertedCircle(Arena.Center, 5f) : ShapeDistance.Circle(Arena.Center, 18.5f), WorldState.FutureTime(ActivationDelay));
        }
    }
}

sealed class BlueBolt(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.BlueBoltMarker, (uint)AID.BlueBolt, 5.9d, 83f, 2.5f)
{
    public override void Update()
    {
        if (CurrentBaits.Count != 0)
        {
            CurrentBaits.Ref(0).Forbidden = ForbiddenPlayers;
        }
    }
}
sealed class BlueBoltStretch(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.BlueBolt, 25f, activationDelay: 5.9d);

sealed class Kanabo(BossModule module) : Components.TankbusterTether(module, (uint)AID.Kanabo, (uint)TetherID.Kanabo, new AOEShapeCone(45f, 30f.Degrees()), 6.2d)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_tetheredPlayers[slot])
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, Arena.Bounds.Radius >= 20f ? 19f : 18.5f), activation);
        }
    }
}

sealed class YamaKagura(BossModule module) : Components.SimpleAOEs(module, (uint)AID.YamaKagura, new AOEShapeRect(60f, 3f));
sealed class HundredTonzeSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HundredTonzeSwing, 16f);
sealed class Stoneskin(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Stoneskin, true, showNameInHint: true);
sealed class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.NumaNoShiki, (uint)OID.DoroNoShiki])
{
    public bool Started;

    public override void OnActorTargetable(Actor actor)
    {
        if (actor.OID == (uint)OID.NumaNoShiki)
        {
            Started = true;
        }
    }
}

sealed class StrengthOfSpirit(BossModule module) : Components.RaidwideCast(module, (uint)AID.StrengthOfSpirit); // not really a raidwide yet, but raidwide is after a cutscene, so we want to heal up before
