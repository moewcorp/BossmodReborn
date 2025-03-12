namespace BossMod.Endwalker.Alliance.A13Azeyma;

class SolarWings(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(30f, 75f.Degrees()));
class SolarWingsL(BossModule module) : SolarWings(module, AID.SolarWingsL);
class SolarWingsR(BossModule module) : SolarWings(module, AID.SolarWingsR);

class SolarFlair(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<WPos> _sunstorms = [];
    private BitMask _adjusted;

    private const float _kickDistance = 18f;
    private static readonly AOEShapeCircle _shape = new(15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _sunstorms.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
            aoes[i] = new(_shape, _sunstorms[i]);
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Sunstorm)
            _sunstorms.Add(actor.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TeleportHeat:
                if (_sunstorms.Count != 0)
                {
                    WPos? closestSunstorm = null;
                    var minDistance = float.MaxValue;
                    var closestIndex = -1;

                    for (var i = 0; i < _sunstorms.Count; ++i)
                    {
                        if (_adjusted[i])
                            continue;

                        var distance = (_sunstorms[i] - spell.TargetXZ).LengthSq();
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestSunstorm = _sunstorms[i];
                            closestIndex = i;
                        }
                    }

                    // should teleport within range ~6
                    var pos = closestSunstorm!.Value;
                    if ((pos - spell.TargetXZ).LengthSq() < 50f)
                    {
                        _sunstorms[closestIndex] = pos + _kickDistance * (pos - spell.TargetXZ).Normalized();
                        _adjusted[closestIndex] = true;
                    }
                    else
                    {
                        ReportError($"Unexpected teleport location: {spell.TargetXZ}, closest sunstorm at {pos}");
                    }
                }
                else
                {
                    ReportError("Unexpected teleport, no sunstorms active");
                }
                break;
            case (uint)AID.SolarFlair:
                ++NumCasts;
                if (_sunstorms.RemoveAll(p => p.AlmostEqual(caster.Position, 1f)) != 1)
                    ReportError($"Unexpected solar flair position {caster.Position}");
                break;
        }
    }
}
