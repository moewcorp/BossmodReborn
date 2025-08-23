namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

sealed class PrincelyBlow(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60f, 5f), (uint)IconID.PrincelyBlow, (uint)AID.PrincelyBlow, 8.3d, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class PrincelyBlowKB(BossModule module) : Components.GenericKnockback(module)
{
    private readonly ShieldBash shieldBash = module.FindComponent<ShieldBash>()!;
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => targets[slot] ? _kb : [];
    private Knockback[] _kb = [];
    private BitMask targets;
    private static readonly ActionID armslength = ActionID.MakeSpell(ClassShared.AID.ArmsLength);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.PrincelyBlow)
        {
            if (targets == default)
            {
                _kb = [new(A23Kamlanaut.ArenaCenter.Quantized(), 30f, WorldState.FutureTime(8.3d))];
            }
            targets.Set(Raid.FindSlot(targetID));
            if (Arena.Bounds.Radius == 29.5f)
            {
                StopAtWall = true;
            }
            else
            {
                StopAtWall = false;
                StopAfterWall = true;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PrincelyBlow)
        {
            targets = default;
            _kb = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (targets[slot] && Arena.Bounds.Radius > 30f)
        {
            ref readonly var kb = ref _kb[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                // if possible press Arms Length so there are more places to go with the tankbuster
                hints.ActionsToExecute.Push(armslength, actor, ActionQueue.Priority.High);
                if (!shieldBash.PolygonInit)
                {
                    shieldBash.Polygon = A23Kamlanaut.P2ArenaWithBridges.poly.Offset(-1f); // pretend polygon is 1y smaller than real for less suspect knockbacks
                    shieldBash.PolygonInit = true;
                }
                var origin = kb.Origin;
                var center = Arena.Center;
                var poly = shieldBash.Polygon;
                hints.AddForbiddenZone(p =>
                {
                    if (poly.Contains(p - center + 30f * (p - origin).Normalized()))
                    {
                        return 1f;
                    }
                    return default;
                }, act);
            }
        }
    }
}
