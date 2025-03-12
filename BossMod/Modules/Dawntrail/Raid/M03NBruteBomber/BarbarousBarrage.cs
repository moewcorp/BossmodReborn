using BossMod.Shadowbringers.Foray.CriticalEngagement.CE14VigilForLost;

namespace BossMod.Dawntrail.Raid.M03NBruteBomber;

class BarbarousBarrageTower(BossModule module) : Components.GenericTowers(module)
{
    private static readonly WPos[] positions = [new(106, 94), new(94, 106), new(106, 106), new(94, 94)];

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x01 && state == 0x00020004)
            for (var i = 0; i < 4; ++i)
                Towers.Add(new(positions[i], 4f, 1, 1, default, WorldState.FutureTime(10d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion)
            Towers.Clear();
    }
}

class BarbarousBarrageKnockback(BossModule module) : Components.Knockback(module)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly BarbarousBarrageTower _tower = module.FindComponent<BarbarousBarrageTower>()!;

    public override ReadOnlySpan<Source> ActiveSources(int slot, Actor actor)
    {
        var towers = _tower.Towers;
        var count = towers.Count;
        if (count == 0)
            return [];
        var sources = new Source[count];
        for (var i = 0; i < count; ++i)
        {
            var t = towers[i];
            sources[i] = new(t.Position, 22f, t.Activation, circle);
        }
        return sources;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var towers = _tower.Towers;
        var count = towers.Count;
        if (count == 0)
            return;
        var isDelayDeltaLow = (towers[0].Activation - WorldState.CurrentTime).TotalSeconds < 5d;
        var isActorInsideTower = false;
        for (var i = 0; i < count; ++i)
        {
            if (towers[i].IsInside(actor))
            {
                isActorInsideTower = true;
                break;
            }
        }
        if (isDelayDeltaLow && isActorInsideTower)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Surecast), actor, ActionQueue.Priority.High);
        }
    }
}
