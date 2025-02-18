namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

public class WickedCannon(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private static readonly AOEShapeRect rect = new(40f, 5f);
    private static readonly float[] delays3fold = [9.5f, 8.3f, 7.7f];
    private static readonly float[] delays4fold = [12, 10.9f, 10.2f, 9.5f];
    private static readonly float[] delays5fold = [14.5f, 13.4f, 12.8f, 12.2f, 11.6f];
    private float[] currentDelays = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        currentDelays = spell.Action.ID switch
        {
            (uint)AID.ThreefoldBlast1 or (uint)AID.ThreefoldBlast2 => delays3fold,
            (uint)AID.FourfoldBlast1 or (uint)AID.FourfoldBlast2 => delays4fold,
            (uint)AID.FivefoldBlast1 or (uint)AID.FivefoldBlast2 => delays5fold,
            _ => []
        };
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        void AddAOE(ushort extra)
        {
            var rotation = extra switch
            {
                0x2D3 => 180f.Degrees(),
                0x2D4 => default,
                _ => default
            };
            _aoes.Add(new(rect, Module.PrimaryActor.Position, rotation, WorldState.FutureTime(currentDelays[_aoes.Count])));
        }
        if (status.ID == (uint)SID.WickedCannon && currentDelays.Length > _aoes.Count)
            AddAOE(status.Extra);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.WickedCannon1:
                case (uint)AID.WickedCannon2:
                case (uint)AID.WickedCannon3:
                case (uint)AID.WickedCannon4:
                case (uint)AID.WickedCannon5:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}
