namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class MightyGripArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x46)
        {
            switch (state)
            {
                case 0x00200010u:
                    _aoe = [new(A21FaithboundKirin.RectArenaAOE, Arena.Center, default, WorldState.FutureTime(11.1d))];
                    break;
                case 0x00020001u:
                    Arena.Bounds = new ArenaBoundsRect(12.5f, 15f);
                    Arena.Center = A21FaithboundKirin.RectCenter;
                    _aoe = [];
                    break;
                case 0x00080004u:
                    Arena.Bounds = A21FaithboundKirin.DefaultArena;
                    Arena.Center = A21FaithboundKirin.DefaultCenter;
                    break;
            }
        }
    }
}

sealed class DeadlyHold(BossModule module) : Components.GenericTowers(module, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x52 && state == 0x00020001u)
        {
            WPos[] positions = [new(-858f, 781f), new(-858f, 789f), new(-842f, 781f), new(-842f, 789f)];

            BitMask forbidden = default;
            var party = Raid.WithSlot(true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var p = ref party[i];
                var actor = p.Item2;
                if (actor.Role != Role.Tank && actor.OID == default) // Alxaal takes one of the 4 towers, but doesn't have a tank role
                {
                    forbidden.Set(p.Item1);
                }
            }

            var act = WorldState.FutureTime(11d);
            for (var i = 0; i < 4; ++i)
            {
                Towers.Add(new(positions[i], 3f, 1, 1, forbidden, act));
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.StandingFirm)
        {
            Towers.Clear();
        }
    }
}

sealed class Bury(BossModule module) : Components.SingleTargetInstant(module, (uint)AID.Bury, 8.5d)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.StandingFirm)
        {
            var id = actor.InstanceID;
            Targets.Add((Raid.FindSlot(id), WorldState.FutureTime(Delay), id, Module.PrimaryActor, actor));
        }
    }
}

sealed class Shockwave(BossModule module) : Components.RaidwideInstant(module, (uint)AID.Shockwave, 6.6d)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x46)
        {
            if (state == 0x00020001u)
            {
                Activation = WorldState.FutureTime(Delay);
            }
            else if (state == 0x00080004u)
            {
                Activation = default;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        var id = spell.Action.ID;
        if (id == WatchedAction && NumCasts is 2 or 6)
        {
            Activation = WorldState.FutureTime(9d);
        }
        else if (id == (uint)AID.Bury)
        {
            Activation = WorldState.FutureTime(5.4d);
        }
    }
}

sealed class KirinCaptivator(BossModule module) : Components.CastHints(module, [(uint)AID.KirinCaptivatorEnrage1, (uint)AID.KirinCaptivatorEnrage2], "Enrage!", true);
