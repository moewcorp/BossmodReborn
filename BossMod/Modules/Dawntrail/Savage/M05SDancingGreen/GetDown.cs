namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

sealed class GetDownCone : Components.SimpleAOEs
{
    public GetDownCone(BossModule module) : base(module, (uint)AID.GetDownCone, GetDownBait.Cone)
    {
        Color = Colors.Danger;
    }
}

sealed class GetDownOutIn(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeCircle circle = new(7f);
    private static readonly AOEShapeDonut donut = new(5f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GetDownCircle1)
        {
            for (var i = 0; i < 4; ++i)
            {
                AddAOE(circle, i * 5f);
                AddAOE(donut, i * 5f + 2.5f);
            }
        }

        void AddAOE(AOEShape shape, float delay)
            => _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell, delay)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.GetDownCircle1 or (uint)AID.GetDownCircle2 or (uint)AID.GetDownDonut)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }
}

sealed class GetDownBait(BossModule module) : Components.GenericBaitAway(module, damageType: AIHints.PredictedDamageType.Raidwide)
{
    public static readonly AOEShapeCone Cone = new(40f, 22.5f.Degrees());
    public bool First = true;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GetDownCircle1)
        {
            var party = Raid.WithoutSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                CurrentBaits.Add(new(Module.PrimaryActor, p, Cone, Module.CastFinishAt(spell, 0.3d)));
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (First && status.ID is (uint)SID.WavelengthAlpha or (uint)SID.WavelengthBeta)
        {
            var count = CurrentBaits.Count;
            for (var i = 0; i < count; ++i)
            {
                if (CurrentBaits[i].Target == actor)
                {
                    CurrentBaits.RemoveAt(i);
                    break;
                }
            }
            var remaining = CollectionsMarshal.AsSpan(CurrentBaits);
            var len = remaining.Length;
            var act = WorldState.FutureTime(2.5d);
            for (var i = 0; i < len; ++i)
            {
                ref var b = ref remaining[i];
                b.Activation = act;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GetDownBait)
        {
            if (++NumCasts == 8)
            {
                CurrentBaits.Clear();
                return;
            }
            if (!First)
            {
                var targets = spell.Targets;
                var countT = targets.Count;
                var countB = CurrentBaits.Count;
                if (countT == 1)
                {
                    for (var i = 0; i < countB; ++i)
                    {
                        if (CurrentBaits[i].Target.InstanceID == targets[0].ID)
                        {
                            CurrentBaits.RemoveAt(i);
                            break;
                        }
                    }
                }
                else
                {
                    var closestDiff = new Angle(MathF.PI);
                    Actor? closestActor = null;

                    for (var i = 0; i < countT; ++i)
                    {
                        var actor = WorldState.Actors.Find(targets[i].ID);
                        if (actor == null)
                            continue;
                        var angleToActor = Angle.FromDirection(actor.Position - Arena.Center);
                        var diff = (angleToActor - spell.Rotation).Normalized();

                        if (Math.Abs(diff.Deg) < Math.Abs(closestDiff.Deg))
                        {
                            closestDiff = diff;
                            closestActor = actor;
                        }
                    }

                    for (var i = 0; i < countB; ++i)
                    {
                        if (CurrentBaits[i].Target == closestActor)
                        {
                            CurrentBaits.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }
    }
}
