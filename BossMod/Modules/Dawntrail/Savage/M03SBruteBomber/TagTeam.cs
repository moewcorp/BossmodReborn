namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

sealed class TagTeamLariatCombo(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private readonly Actor?[] _tetherSource = new Actor?[PartyState.MaxPartySize];
    private readonly AOEInstance[][] _safespot = new AOEInstance[PartyState.MaxPartySize][];
    private const string Hint = "Go to correct spot!";
    private static readonly AOEShapeRect rect = new(70f, 17f);
    private ConeHA? cone;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
        {
            return [];
        }
        if (_tetherSource[slot] != null)
        {
            var aoe = _safespot[slot];
            if (aoe.Length == 0)
            {
                var (safeShapes, dangerShapes) = GetShapesForAOEs(slot);
                ConeHA[] coneShapes = cone != null ? [cone] : [];
                aoe = [new(new AOEShapeCustom(safeShapes, dangerShapes, coneShapes, true, cone != null ? OperandType.Intersection : OperandType.Union),
                    Arena.Center, default, AOEs.Ref(0).Activation, Colors.SafeFromAOE)];
            }
            return aoe;
        }
        return CollectionsMarshal.AsSpan(AOEs);
    }

    private (RectangleSE[] safeShapes, RectangleSE[] dangerShapes) GetShapesForAOEs(int slot)
    {
        var safeShapes = new RectangleSE[1];
        var dangerShapes = new RectangleSE[1];
        var count = AOEs.Count;
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            var isSafe = _tetherSource[slot]?.Position.AlmostEqual(aoe.Origin, 25f) ?? false;
            var shape = new RectangleSE(aoe.Origin, aoe.Origin + rect.LengthFront * aoe.Rotation.ToDirection(), rect.HalfWidth);

            if (isSafe)
            {
                safeShapes[0] = shape;
            }
            else
            {
                dangerShapes[0] = shape;
            }
        }
        return (safeShapes, dangerShapes);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var safe = _safespot[slot];
        if (safe == null || safe.Length == 0)
        {
            return;
        }

        ref var aoe = ref safe[0];
        if (_tetherSource[slot] == null)
        {
            base.AddHints(slot, actor, hints);
        }
        else if (_safespot[slot].Length != 0)
        {
            hints.Add(Hint, !aoe.Check(actor.Position));
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ChainDeathmatch && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
        {
            if (_safespot[slot] == null)
            {
                for (var i = 0; i < 8; ++i)
                {
                    _safespot[i] = [];
                }
            }
            _tetherSource[slot] = source;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TagTeamLariatComboFirstRAOE:
            case (uint)AID.TagTeamLariatComboFirstLAOE:
            case (uint)AID.FusesOfFuryLariatComboFirstRAOE:
            case (uint)AID.FusesOfFuryLariatComboFirstLAOE:
                AOEs.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.FusesOfFuryMurderousMist:
                cone = new(spell.LocXZ, 40f, spell.Rotation, 135f.Degrees());
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        switch (id)
        {
            case (uint)AID.TagTeamLariatComboFirstRAOE:
            case (uint)AID.TagTeamLariatComboFirstLAOE:
            case (uint)AID.TagTeamLariatComboSecondRAOE:
            case (uint)AID.TagTeamLariatComboSecondLAOE:
            case (uint)AID.FusesOfFuryLariatComboFirstRAOE:
            case (uint)AID.FusesOfFuryLariatComboFirstLAOE:
            case (uint)AID.FusesOfFuryLariatComboSecondRAOE:
            case (uint)AID.FusesOfFuryLariatComboSecondLAOE:
                ++NumCasts;
                Array.Fill(_tetherSource, null);
                var index = -1;
                var aoes = CollectionsMarshal.AsSpan(AOEs);
                var len = aoes.Length;
                var pos = spell.LocXZ;
                for (var i = 0; i < len; ++i)
                {
                    ref var aoe = ref aoes[i];
                    if (aoe.Origin == pos)
                    {
                        index = i;
                        break;
                    }
                }
                if (index < 0)
                {
                    ReportError($"Failed to find AOE for {spell.LocXZ}");
                }
                else if (id is (uint)AID.TagTeamLariatComboFirstRAOE or (uint)AID.TagTeamLariatComboFirstLAOE or (uint)AID.FusesOfFuryLariatComboFirstRAOE or (uint)AID.FusesOfFuryLariatComboFirstLAOE)
                {
                    ref var aoe = ref aoes[index];
                    aoe.Origin = (Arena.Center - (aoe.Origin - Arena.Center)).Quantized();
                    aoe.Rotation += 180f.Degrees();
                    aoe.Activation = WorldState.FutureTime(4.3d);
                }
                else
                {
                    AOEs.RemoveAt(index);
                }
                break;
            case (uint)AID.FusesOfFuryMurderousMist:
                cone = null;
                break;
        }
    }
}
