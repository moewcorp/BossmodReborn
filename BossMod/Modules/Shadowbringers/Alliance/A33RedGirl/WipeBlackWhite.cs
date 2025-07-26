namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

// contrary to intuition this mechanic does not seem to be a true line of sight AOE, so instead of a fan we just get just rectangles projected behind the walls
sealed class WipeBlackWhite(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    private AOEInstance? _aoe;
    private readonly WPos[] positions = new WPos[2];
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _arena.NumWalls < 10 ? Utils.ZeroOrOne(ref _aoe) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.WipeWhite)
        {
            AddPosition();
        }
        else if (id == (uint)AID.WipeBlack)
        {
            AddPosition(1);
        }

        void AddPosition(int index = default)
        {
            positions[index] = caster.Position;
            activation = Module.CastFinishAt(spell);
        }
    }

    public void UpdateAOE()
    {
        if (activation == default)
        {
            return;
        }

        var walls = _arena.Walls;
        var shapes1 = new List<RectangleSE>();
        var shapes2 = new List<RectangleSE>();
        var center = Arena.Center;
        for (var i = 0; i < 2; ++i)
        {
            var p = positions[i];
            if (p != default)
            {
                var iswhite = i == 0;
                for (var j = 0; j < 28; ++j)
                {
                    var w = walls[j];
                    if (w.isWhite != iswhite)
                    {
                        continue;
                    }

                    var delta = w.position - p;
                    Angle? angle;

                    if (w.rotation.AlmostEqual(-90f.Degrees(), Angle.DegToRad))
                    {
                        angle = (delta.X > 0 ? 1f : -1f) * 90f.Degrees();
                    }
                    else
                    {
                        angle = (delta.Z > 0 ? 0f : 180f).Degrees();
                    }
                    if (angle == null)
                    {
                        continue;
                    }
                    var dir = angle.Value.ToDirection();
                    var intersect = Intersect.RayAABB(w.position - center, dir, 25f, 25f);

                    var shape = new RectangleSE(w.position, w.position + dir * intersect, 3f);
                    if (iswhite)
                    {
                        shapes1.Add(shape);
                    }
                    else
                    {
                        shapes2.Add(shape);
                    }
                }
            }
        }

        var aoe = new AOEShapeCustom(shapes1, shapes2, null, true);
        if (positions[1] != default && positions[0] != default) // if there are 2 meteors, we need to intersect the union of each meteor's rectangles
        {
            var clipper = new PolygonClipper();
            var union1 = Union(shapes1);
            var union2 = Union(shapes2);
            aoe.Polygon = clipper.Intersect(new PolygonClipper.Operand(union1), new PolygonClipper.Operand(union2));

            RelSimplifiedComplexPolygon Union(List<RectangleSE> shapes)
            {
                var operand = new PolygonClipper.Operand();
                var count = shapes.Count;
                for (var i = 0; i < count; ++i)
                {
                    operand.AddPolygon(shapes[i].ToPolygon(center));
                }
                return clipper.Simplify(operand);
            }
        }
        _aoe = new(aoe, center, default, activation, Colors.SafeFromAOE);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WipeBlack or (uint)AID.WipeWhite)
        {
            _aoe = null;
            activation = default;
            Array.Clear(positions);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe is AOEInstance a)
        {
            ref var aoe = ref a;
            hints.Add("Wait in safe area!", !aoe.Check(actor.Position));
        }
    }
}
