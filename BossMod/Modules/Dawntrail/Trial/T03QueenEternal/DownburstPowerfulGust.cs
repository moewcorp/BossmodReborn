namespace BossMod.Dawntrail.Trial.T03QueenEternal;

sealed class PowerfulGustKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.PowerfulGust, 20f, kind: Kind.DirForward, stopAfterWall: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            var act = Module.CastFinishAt(source.CastInfo);
            if (!IsImmune(slot, act))
                hints.AddForbiddenZone(ShapeDistance.InvertedRect(source.Position, source.Rotation, 9.5f, default, 20f), act);
        }
    }
}

sealed class DownburstKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Downburst, 10f, stopAfterWall: true)
{
    private RelSimplifiedComplexPolygon polygon = new();
    private bool polygonInit;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var castinfo = Casters[0].CastInfo!;
            var act = Module.CastFinishAt(castinfo);
            if (!IsImmune(slot, act))
            {
                if (!polygonInit)
                {
                    polygon = T03QueenEternal.XArena.poly.Offset(-1f); // pretend polygon is 1y smaller than real for less suspect knockbacks
                    polygonInit = true;
                }
                var origin = castinfo.LocXZ;
                var center = Arena.Center;
                var poly = polygon;
                hints.AddForbiddenZone(p =>
                {
                    // while doing a point in polygon test and intersection test seems like double the work, the intersection test is actually a lot slower than the PiP test, so this is a net positive to filter out some cells beforehand
                    var offsetSource = (p - origin).Normalized();
                    var offsetCenter = p - center;
                    if (polygon.Contains(offsetCenter + 10f * offsetSource) && Intersect.RayPolygon(offsetCenter, offsetSource, poly) > 10f)
                        return 1f;
                    return default;
                }, act);
            }
        }
    }
}

sealed class PowerfulGustDownburstRW(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.PowerfulGust, (uint)AID.Downburst]);
