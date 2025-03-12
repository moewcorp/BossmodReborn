namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

class MercyFourfold(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.MercyFourfoldAOE))
{
    public readonly List<AOEInstance> AOEs = [];
    private readonly List<AOEInstance?> _safezones = [];
    private static readonly AOEShapeCone _shapeAOE = new(50f, 90f.Degrees());
    private static readonly AOEShapeCone _shapeSafe = new(50f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (AOEs.Count > 0)
            return CollectionsMarshal.AsSpan(AOEs)[..1];
        if (_safezones.Count > 0 && _safezones[0] != null)
            return new AOEInstance[1] { _safezones[0]!.Value };
        return [];
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID != (uint)SID.Mercy)
            return;

        var dirOffset = status.Extra switch
        {
            0xF7 => -45f.Degrees(),
            0xF8 => -135f.Degrees(),
            0xF9 => 45f.Degrees(),
            0xFA => 135f.Degrees(),
            _ => default
        };
        if (dirOffset == default)
            return;

        var dir = actor.Rotation + dirOffset;
        if (AOEs.Count > 0)
        {
            // see whether there is a safezone for two contiguous aoes
            var mid = dir.ToDirection() + AOEs[^1].Rotation.ToDirection(); // length should be either ~sqrt(2) or ~0
            if (mid.LengthSq() > 1)
                _safezones.Add(new(_shapeSafe, actor.Position, Angle.FromDirection(-mid), default, Colors.SafeFromAOE, false));
            else
                _safezones.Add(null);
        }

        var activationDelay = 15 - 1.3f * AOEs.Count;
        AOEs.Add(new(_shapeAOE, actor.Position, dir, WorldState.FutureTime(activationDelay)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
        {
            if (AOEs.Count > 0)
                AOEs.RemoveAt(0);
            if (_safezones.Count > 0)
                _safezones.RemoveAt(0);
        }
    }
}
