namespace BossMod.Endwalker.Alliance.A34Eulogia;

class Quintessence(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCone cone = new(50f, 90f.Degrees());
    private static readonly AOEShapeDonut donut = new(8f, 60f);
    private byte _index;
    private WPos position;

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
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state != 0x00020001 || index is < 0x4C or > 0x57)
            return;

        // there are 12 possible arrows: from center to each corner (4) + pairs between neighbouring corners (8):
        //     <-- 57 ---
        //   * --- 54 --> *
        //  |^ \        / |^
        //  ||  4E    4F  ||
        // 55|    \  /   56|
        //  |52   /  \    |51
        //  ||  4D    4C  ||
        //  v| /        \ v|
        //   * <-- 50 --- *
        //     --- 53 -->

        WDir offset = index switch
        {
            0x4C or 0x53 or 0x56 => new(9f, 9f),
            0x4D or 0x50 or 0x55 => new(-9f, 9f),
            0x4E or 0x52 or 0x57 => new(-9f, -9f),
            0x4F or 0x51 or 0x54 => new(9f, -9f),
            _ => default
        };
        _index = index;
        position = WPos.ClampToGrid(Arena.Center + offset);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_index == default)
            return;
        void AddAOE(Angle rotation, AOEShape? shape = null)
        {
            _aoes.Add(new(shape ?? cone, position, rotation, Module.CastFinishAt(spell, 19.5f - _aoes.Count * 3.7f)));
            _index = default;
        }

        switch (spell.Action.ID)
        {
            case (uint)AID.FirstFormRight:
                switch (_index)
                {
                    case 0x4C:
                        AddAOE(Angle.AnglesIntercardinals[0]);
                        break;
                    case 0x4D:
                        AddAOE(Angle.AnglesIntercardinals[3]);
                        break;
                    case 0x4E:
                        AddAOE(Angle.AnglesIntercardinals[2]);
                        break;
                    case 0x4F:
                        AddAOE(Angle.AnglesIntercardinals[1]);
                        break;
                }
                break;
            case (uint)AID.FirstFormLeft:
                switch (_index)
                {
                    case 0x4C:
                        AddAOE(Angle.AnglesIntercardinals[2]);
                        break;
                    case 0x4D:
                        AddAOE(Angle.AnglesIntercardinals[1]);
                        break;
                    case 0x4E:
                        AddAOE(Angle.AnglesIntercardinals[0]);
                        break;
                    case 0x4F:
                        AddAOE(Angle.AnglesIntercardinals[3]);
                        break;
                }
                break;
            case (uint)AID.FirstFormDonut:
            case (uint)AID.SecondFormDonut:
            case (uint)AID.ThirdFormDonut:
                AddAOE(default, donut);
                break;
            case (uint)AID.SecondFormRight or (uint)AID.ThirdFormRight:
                switch (_index)
                {
                    case 0x50 or 0x57:
                        AddAOE(Angle.AnglesCardinals[2]);
                        break;
                    case 0x51 or 0x52:
                        AddAOE(Angle.AnglesCardinals[3]);
                        break;
                    case 0x53 or 0x54:
                        AddAOE(Angle.AnglesCardinals[1]);
                        break;
                    case 0x55 or 0x56:
                        AddAOE(Angle.AnglesCardinals[0]);
                        break;
                }
                break;
            case (uint)AID.SecondFormLeft or (uint)AID.ThirdFormLeft:
                switch (_index)
                {
                    case 0x50 or 0x57:
                        AddAOE(Angle.AnglesCardinals[1]);
                        break;
                    case 0x55 or 0x56:
                        AddAOE(Angle.AnglesCardinals[3]);
                        break;
                    case 0x51 or 0x52:
                        AddAOE(Angle.AnglesCardinals[0]);
                        break;
                    case 0x53 or 0x54:
                        AddAOE(Angle.AnglesCardinals[2]);
                        break;
                }
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.QuintessenceAOE1Right:
            case (uint)AID.QuintessenceAOE1Left:
            case (uint)AID.QuintessenceAOE1Donut:
            case (uint)AID.QuintessenceAOE2Right:
            case (uint)AID.QuintessenceAOE2Left:
            case (uint)AID.QuintessenceAOE2Donut:
            case (uint)AID.QuintessenceAOE3Right:
            case (uint)AID.QuintessenceAOE3Left:
            case (uint)AID.QuintessenceAOE3Donut:
                ++NumCasts;
                if (_aoes.Count != 0)
                    _aoes.RemoveAt(0);
                break;
        }
    }
}
