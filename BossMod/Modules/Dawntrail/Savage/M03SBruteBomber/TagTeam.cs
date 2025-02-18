namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

class TagTeamLariatCombo(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private readonly Actor?[] _tetherSource = new Actor?[PartyState.MaxPartySize];
    private const string Hint = "Go to correct spot!";
    private static readonly AOEShapeRect rect = new(70f, 17f);
    private ConeHA? cone;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        if (_tetherSource[slot] != null)
        {
            var (safeShapes, dangerShapes) = GetShapesForAOEs(slot);
            Shape[] coneShapes = cone != null ? [cone] : [];

            return [new(new AOEShapeCustom(safeShapes, dangerShapes, coneShapes, true, cone != null ? OperandType.Intersection : OperandType.Union),
                Arena.Center, default, AOEs[0].Activation, Colors.SafeFromAOE)];
        }
        else
            return AOEs;
    }

    private (Shape[] safeShapes, Shape[] dangerShapes) GetShapesForAOEs(int slot)
    {
        List<Shape> safeShapes = [];
        List<Shape> dangerShapes = [];
        var count = AOEs.Count;
        for (var i = 0; i < count; ++i)
        {
            var aoe = AOEs[i];
            var isSafe = _tetherSource[slot]?.Position.AlmostEqual(aoe.Origin, 25f) ?? false;
            var shape = new RectangleSE(aoe.Origin, aoe.Origin + rect.LengthFront * aoe.Rotation.ToDirection(), rect.HalfWidth);

            if (isSafe)
                safeShapes.Add(shape);
            else
                dangerShapes.Add(shape);
        }
        return ([.. safeShapes], [.. dangerShapes]);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = AOEs.Count;
        if (count == 0)
            return;
        if (ActiveAOEs(slot, actor).Any(c => c.Shape == rect))
            base.AddHints(slot, actor, hints);
        else if (ActiveAOEs(slot, actor).Any(c => !c.Check(actor.Position)))
            hints.Add(Hint);
        else
            hints.Add(Hint, false);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ChainDeathmatch && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
            _tetherSource[slot] = source;
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
        switch (spell.Action.ID)
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
                var index = AOEs.FindIndex(aoe => aoe.Origin.AlmostEqual(spell.LocXZ, 1));
                if (index < 0)
                {
                    ReportError($"Failed to find AOE for {spell.LocXZ}");
                }
                else if (spell.Action.ID is (uint)AID.TagTeamLariatComboFirstRAOE or (uint)AID.TagTeamLariatComboFirstLAOE or (uint)AID.FusesOfFuryLariatComboFirstRAOE or (uint)AID.FusesOfFuryLariatComboFirstLAOE)
                {
                    ref var aoe = ref AOEs.Ref(index);
                    aoe.Origin = Arena.Center - (aoe.Origin - Arena.Center);
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
