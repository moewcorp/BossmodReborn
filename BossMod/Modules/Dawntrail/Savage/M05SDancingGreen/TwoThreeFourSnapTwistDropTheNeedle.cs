namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

sealed class TwoThreeFourSnapTwistDropTheNeedle(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private readonly AOEShapeRect rect = new(20f, 20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(AOEs)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst1:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst2:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst3:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst4:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst5:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst6:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst7:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst8:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst1:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst2:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst3:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst4:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst5:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst6:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst7:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst8:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst1:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst2:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst3:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst4:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst5:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst6:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst7:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst8:
                AddAOE();
                AddAOE(180f.Degrees(), 3.5d);
                break;
        }
        void AddAOE(Angle offset = default, double delay = default)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            var pos = delay != default ? loc - 5f * rot.ToDirection() : loc;
            var rot2 = rot + offset;
            AOEs.Add(new(rect, pos, rot2, Module.CastFinishAt(spell, delay), shapeDistance: rect.Distance(pos, rot2)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst1:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst2:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst3:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst4:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst5:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst6:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst7:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst8:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst1:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst2:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst3:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst4:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst5:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst6:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst7:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst8:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst1:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst2:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst3:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst4:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst5:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst6:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst7:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst8:
                ++NumCasts;
                break;
            case (uint)AID.TwoSnapTwistDropTheNeedle2:
            case (uint)AID.TwoSnapTwistDropTheNeedle3:
            case (uint)AID.ThreeSnapTwistDropTheNeedle3:
            case (uint)AID.ThreeSnapTwistDropTheNeedle4:
            case (uint)AID.FourSnapTwistDropTheNeedle4:
            case (uint)AID.FourSnapTwistDropTheNeedle5:
                var count = AOEs.Count;
                if (count != 0)
                {
                    AOEs.RemoveAt(0);
                    if (count == 2)
                    {
                        ref var aoe2 = ref AOEs.Ref(0);
                        var rot = aoe2.Rotation;
                        aoe2.Origin -= 5f * rot.ToDirection();
                        aoe2.ShapeDistance = rect.Distance(aoe2.Origin, rot);
                    }
                }
                ++NumCasts;
                break;
        }
    }
}

sealed class FlipToABSide(BossModule module) : Components.GenericBaitAway(module, default)
{
    public Actor? Source;
    private bool _lightparty;
    private readonly TwoThreeFourSnapTwistDropTheNeedle _aoe = module.FindComponent<TwoThreeFourSnapTwistDropTheNeedle>()!;

    private AOEShape ActiveShape => _lightparty ? rect : cone;

    private static readonly AOEShapeCone cone = new(60f, 22.5f.Degrees());
    private static readonly AOEShapeRect rect = new(50f, 4f);

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Source != null && _aoe.AOEs.Count == 0 && _aoe.NumCasts == 0)
            hints.Add($"Stored: {(_lightparty ? "Light party" : "Role")} stack");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Source != null && (_aoe.AOEs.Count != 0 || _aoe.NumCasts > 0))
        {
            var pcDir = Angle.FromDirection(actor.Position - Source.Position);
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            var clipped = new List<Actor>(8);
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (ActiveShape.Check(p.Position, Module.PrimaryActor.Position, pcDir))
                {
                    clipped.Add(p);
                }
            }
            var count = clipped.Count;
            var tanks = 0;
            var healers = 0;
            var dps = 0;
            var clip = CollectionsMarshal.AsSpan(clipped);
            for (var i = 0; i < count; ++i)
            {
                var c = clip[i];
                if (c.Role == Role.Tank)
                {
                    ++tanks;
                }
                else if (c.Role == Role.Healer)
                {
                    ++healers;
                }
                else
                {
                    ++dps;
                }
            }
            if (_lightparty)
            {
                hints.Add("Light party stack!", healers != 1 || tanks != 1 || dps != 2);
            }
            else
            {
                var condTank = tanks == 2 && healers == 0 && dps == 0;
                var condHealer = healers == 2 && tanks == 0 && dps == 0;
                var condDps = dps == 4 && healers == 0 && tanks == 0;
                hints.Add("Role stack!", actor.Role == Role.Healer ? !condHealer : actor.Role == Role.Tank ? !condTank : !condDps);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (_lightparty || Source == null)
            return PlayerPriority.Irrelevant;
        return ClassRole.IsSameRole(pc, player) ? PlayerPriority.Interesting : PlayerPriority.Danger;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Source != null && (_aoe.AOEs.Count != 0 || _aoe.NumCasts > 0))
        {
            var pcDir = Angle.FromDirection(pc.Position - Source.Position);
            ActiveShape.Outline(Arena, Source.Position, pcDir, Colors.Safe);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FlipToASide:
                Source = caster;
                ++NumCasts;
                _lightparty = false;
                break;
            case (uint)AID.FlipToBSide:
                Source = caster;
                _lightparty = true;
                ++NumCasts;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PlayASide or (uint)AID.PlayBSide)
        {
            Source = null;
        }
    }
}
