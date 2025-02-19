﻿namespace BossMod.Endwalker.Alliance.A34Eulogia;

class SolarFans(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.SolarFansAOE), 5f);

class RadiantRhythm(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeDonutSector _shape = new(20f, 30f, 45f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 4 ? 4 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i < 2)
                aoes[i] = count > 2 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SolarFansAOE)
        {
            // assumption: flames always move CCW
            var pattern1 = false;
            if ((int)spell.LocXZ.Z == -945f)
                pattern1 = true;
            NumCasts = 0;
            var activation = Module.CastFinishAt(spell, 2.8f);
            for (var i = 1; i < 5; ++i)
            {
                var act = activation.AddSeconds(2.1d * (i - 1));
                var angle = ((pattern1 ? 225f : 135f) + i * 90f).Degrees();
                AddAOE(angle, act);
                AddAOE(angle + 180f.Degrees(), act);
            }
            void AddAOE(Angle rotation, DateTime activation) => _aoes.Add(new(_shape, WPos.ClampToGrid(Arena.Center), rotation, activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RadiantFlight)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }
}

class RadiantFlourish(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCircle _shape = new(25);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // assumption: we always have 4 moves, so flames finish where they start
        switch ((AID)spell.Action.ID)
        {
            case AID.SolarFansAOE:
                _aoes.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell, 13.8f)));
                break;
            case AID.RadiantFlourish:
                // verify the assumption
                if (!_aoes.Any(aoe => aoe.Origin.AlmostEqual(caster.Position, 1)))
                    ReportError($"Unexpected AOE position: {caster.Position}");
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RadiantFlourish)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
