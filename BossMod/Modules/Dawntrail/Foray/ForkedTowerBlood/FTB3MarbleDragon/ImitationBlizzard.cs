namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB3MarbleDragon;

sealed class ImitationBlizzard(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(20f);
    private static readonly AOEShapeCross cross = new(60f, 8f);
    private readonly List<AOEInstance> _aoes = new(8);
    private Actor? referenceIcewind;
    private bool IsPattern1;
    private bool aoesAdded;
    private bool show;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!show)
            return [];
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var act0 = aoes[0].Activation;
        var deadline1 = act0.AddSeconds(5d);
        var deadline2 = act0.AddSeconds(1d);

        var index = 0;
        while (index < count && aoes[index].Activation < deadline1)
        {
            ++index;
            if (aoes[index - 1].Activation < deadline2)
            {
                aoes[index - 1].Color = Colors.Danger;
            }
        }
        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.DraconiformMotion1)
        {
            show = true; // don't draw this stuff before baited cones appear so people don't panic and bait it to an unfortunate direction
        }
        else if (id == (uint)AID.FrigidDive)
        {
            var crossPuddle = Module.Enemies((uint)OID.WaterPuddleCross);
            if (crossPuddle.Count != 0)
            {
                var crossP = crossPuddle[0];
                _aoes.Add(new(cross, WPos.ClampToGrid(crossP.Position), crossP.Rotation, Module.CastFinishAt(spell, 4.1f)));
            }
        }
        else if (id == (uint)AID.ImitationIcicle)
        {
            var pos = caster.Position;
            if (pos.Z == 161f) // there is probably a way to calculate this stuff, but with only 4 possible patterns we might as well hardcode it for easy maintainability
            {
                (AOEShape, WPos)[] aoes = [];
                switch (pos.X)
                {
                    case -319f:
                        aoes =
                        [
                            (cross, new(-343f, 141f)),
                            (circle, new(-319f, 173f)),
                            (cross, new(-331f, 173f)),
                            (circle, new(-331f, 141f)),
                            (circle, new(-355f, 141f)),
                            (circle, new(-343f, 173f)),
                            (circle, new(-319f, 141f)),
                            (circle, new(-355f, 173f))
                        ];
                        break;
                    case -331f:
                        aoes =
                        [
                            (circle, new(-331f, 173f)),
                            (circle, new(-335f, 141f)),
                            (circle, new(-319f, 173f)),
                            (circle, new(-343f, 141f)),
                            (cross, new(-343f, 173f)),
                            (cross, new(-331f, 141f)),
                            (circle, new(-355f, 173f)),
                            (circle, new(-319f, 141f))
                        ];
                        break;
                    case -343f:
                        aoes =
                        [
                            (circle, new(-343f, 173f)),
                            (circle, new(-319f, 141f)),
                            (cross, new(-331f, 173f)),
                            (circle, new(-331f, 141f)),
                            (circle, new(-355f, 173f)),
                            (cross, new(-343f, 141f)),
                            (circle, new(-319f, 173f)),
                            (circle, new(-355f, 141f))
                        ];
                        break;
                    case -355f:
                        aoes =
                        [
                            (cross, new(-331f, 141f)),
                            (circle, new(-355f, 173f)),
                            (cross, new(-343f, 173f)),
                            (circle, new(-343f, 141f)),
                            (circle, new(-319f, 141f)),
                            (circle, new(-331f, 173f)),
                            (circle, new(-355f, 141f)),
                            (circle, new(-331f, 173f))
                        ];
                        break;

                }
                var act = Module.CastFinishAt(spell, 4.6f);
                for (var i = 0; i < 8; ++i)
                {
                    ref readonly var aoe = ref aoes[i];
                    var delay = i switch
                    {
                        < 2 => default,
                        < 5 => 1f,
                        < 7 => 2f,
                        _ => 3f
                    };
                    _aoes.Add(new(aoe.Item1, WPos.ClampToGrid(aoe.Item2), Angle.AnglesCardinals[1], act.AddSeconds(delay)));
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        var id = actor.OID;
        if (id == (uint)OID.Icewind && actor.Position.X == -362f)
        {
            referenceIcewind = actor;
        }
        else if (id == (uint)OID.WaterPuddleCross && actor.Position == new WPos(-321f, 141f))
        {
            IsPattern1 = true;
        }
    }

    public override void Update()
    {
        if (!aoesAdded && show && referenceIcewind != null && referenceIcewind.LastFrameMovement != default)
        {
            // with only 4 possible patterns we might as well hardcode it for easy maintainability
            (AOEShape, WPos)[] aoes = [];
            var activationPattern241 = false;
            switch (IsPattern1, (referenceIcewind.Position - Arena.Center).OrthoR().Dot(referenceIcewind.LastFrameMovement) < 0f) // bool in tuple checks if wind is moving counterclockwise
            {
                case (true, true):
                    aoes =
                    [
                        (cross, new(-353f, 173f)),
                        (cross, new(-321f, 141f)),
                        (circle, new(-337f, 133f)),
                        (circle, new(-337f, 181f)),
                        (circle, new(-321f, 173f)),
                        (circle, new(-353f, 141f)),
                        (cross, new(-337f, 157f))
                    ];
                    activationPattern241 = true;
                    break;
                case (true, false):
                    aoes =
                    [
                        (circle, new(-353f, 141f)),
                        (circle, new(-321f, 173f)),
                        (circle, new(-337f, 133f)),
                        (circle, new(-337f, 181f)),
                        (cross, new(-337f, 157f)),
                        (cross, new(-353f, 173f)),
                        (cross, new(-321f, 141f))
                    ];
                    break;
                case (false, true):
                    aoes =
                    [
                        (circle, new(-353f, 173f)),
                        (circle, new(-321f, 141f)),
                        (cross, new(-337f, 157f)),
                        (circle, new(-337f, 133f)),
                        (circle, new(-337f, 181f)),
                        (cross, new(-321f, 173f)),
                        (cross, new(-353f, 141f))
                    ];
                    break;
                case (false, false):
                    aoes =
                    [
                        (cross, new(-321f, 173f)),
                        (cross, new(-353f, 141f)),
                        (circle, new(-337f, 181f)),
                        (circle, new(-337f, 133f)),
                        (circle, new(-321f, 141f)),
                        (circle, new(-353f, 173f)),
                        (cross, new(-337f, 157f))
                    ];
                    activationPattern241 = true;
                    break;
            }
            var act = WorldState.FutureTime(8.5d);
            for (var i = 0; i < 7; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                var delay = i switch
                {
                    < 2 => default,
                    < 6 when activationPattern241 => 4f,
                    < 5 => 4f,
                    _ => 8f,
                };
                _aoes.Add(new(aoe.Item1, WPos.ClampToGrid(aoe.Item2), Angle.AnglesCardinals[1], act.AddSeconds(delay)));
            }
            aoesAdded = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ImitationBlizzardCircle or (uint)AID.ImitationBlizzardCross)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}

sealed class ImitationBlizzardTowers(BossModule module) : Components.GenericTowersOpenWorld(module)
{
    public override ReadOnlySpan<Tower> ActiveTowers(int slot, Actor actor)
    {
        var count = Towers.Count;
        if (count == 0)
            return [];
        var towers = CollectionsMarshal.AsSpan(Towers);
        var deadline = towers[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count && towers[index].Activation < deadline)
            ++index;

        return towers[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FrigidDive)
        {
            var towerPuddles = Module.Enemies((uint)OID.WaterPuddleTower);
            var count = towerPuddles.Count - 1;
            var origin = spell.LocXZ;
            var dir = spell.Rotation;
            var act0 = Module.CastFinishAt(spell, 4.1f);
            var act1 = act0.AddSeconds(4d);

            for (var i = count; i >= 0; --i)
            {
                var t = towerPuddles[i].Position;
                var posClamp = WPos.ClampToGrid(t);
                if (t.InRect(origin, dir, 60f, default, 10f))
                {
                    Towers.Insert(0, new(posClamp, 4f, 4, 8, null, act0));
                }
                else
                {
                    Towers.Add(new(posClamp, 4f, 4, 8, null, act1));
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ImitationBlizzardTower)
        {
            ++NumCasts;
            if (Towers.Count != 0)
            {
                Towers.RemoveAt(0);
            }
        }
    }
}
