namespace BossMod.Dawntrail.Raid.M01NBlackCat;

public class BlackCatCrossing(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeCone cone = new(60, 22.5f.Degrees());
    private enum Pattern { None, Cardinals, Intercardinals }
    private Pattern _currentPattern;
    private Actor? helper;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i < 4)
                aoes[i] = count > 4 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BlackCatCrossingFirst:
            case (uint)AID.BlackCatCrossingRest:
                _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                _aoes.SortBy(x => x.Activation);
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.Extra != 0x307 && _currentPattern == Pattern.None)
        {
            switch (status.ID)
            {
                case (uint)SID.BlackCatCrossing1:
                    _currentPattern = Pattern.Cardinals;
                    break;
                case (uint)SID.BlackCatCrossing2:
                    _currentPattern = Pattern.Intercardinals;
                    break;
            }
            InitIfReady();
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.LeapingAttacks)
        {
            helper = actor;
            InitIfReady();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (helper != null)
            switch (spell.Action.ID)
            {
                case (uint)AID.LeapingOneTwoPawVisual1:
                case (uint)AID.LeapingOneTwoPawVisual2:
                case (uint)AID.LeapingOneTwoPawVisual3:
                case (uint)AID.LeapingOneTwoPawVisual4:
                    helper = null;
                    break;
            }
        else if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.BlackCatCrossingFirst:
                case (uint)AID.BlackCatCrossingRest:
                case (uint)AID.LeapingBlackCatCrossingFirst:
                case (uint)AID.LeapingBlackCatCrossingRest:
                    _aoes.RemoveAt(0);
                    _currentPattern = Pattern.None;
                    break;
            }
    }

    private void InitIfReady()
    {
        if (helper != null && _currentPattern != Pattern.None)
        {
            AddAOEs(helper, _currentPattern == Pattern.Cardinals ? Angle.AnglesCardinals : Angle.AnglesIntercardinals, 9d);
            AddAOEs(helper, _currentPattern == Pattern.Cardinals ? Angle.AnglesIntercardinals : Angle.AnglesCardinals, 11d);
            _currentPattern = Pattern.None;
            helper = null;

            void AddAOEs(Actor actor, Angle[] angles, double futureTime)
            {
                for (var i = 0; i < 4; ++i)
                    _aoes.Add(new(cone, actor.Position, angles[i], WorldState.FutureTime(futureTime)));
            }
        }
    }
}
