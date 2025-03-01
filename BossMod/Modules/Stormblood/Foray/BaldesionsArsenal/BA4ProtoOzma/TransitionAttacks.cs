namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA4ProtoOzma;

class TransitionAttacks(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShapeCircle Circle = new(27f);
    public static readonly AOEShapeDonut Donut = new(17.5f, 38.5f);
    private static readonly AOEShapeRect rect = new(40.5f, 5.5f);
    public readonly List<AOEInstance> AOEs = new(3);
    private static readonly Angle[] angles = [-120.003f.Degrees(), -0.003f.Degrees(), 119.997f.Degrees()];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var firstactivation = AOEs[0].Activation;
        var aoes = new AOEInstance[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var aoe = AOEs[i];
            if ((aoe.Activation - firstactivation).TotalSeconds < 1d)
                aoes[index++] = aoe;
        }
        return aoes[..index];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        void AddAOEs()
        {
            for (var i = 0; i < 3; ++i)
                AddAOE(rect, angles[i], new(-17f, 29.012f));
        }
        void AddAOE(AOEShape shape, Angle rotation = default, WPos position = default)
        => AOEs.Add(new(shape, WPos.ClampToGrid(position == default ? caster.Position : position), rotation, WorldState.FutureTime(7.8)));
        void TransfigurationCounter()
        {
            if (caster == Module.PrimaryActor)
                ++NumCasts;
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.TransfigurationStar:
                TransfigurationCounter();
                if (NumCasts < 7) // no transition AOE at last transition, but enrage start
                    AddAOE(Circle);
                break;
            case (uint)AID.TransfigurationCube:
                AddAOE(Donut);
                TransfigurationCounter();
                break;
            case (uint)AID.TransfigurationPyramid:
                if (caster == Module.PrimaryActor)
                {
                    AddAOEs();
                    TransfigurationCounter();
                }
                else
                    switch ((int)caster.Position.X)
                    {
                        case -58:
                            AddAOE(rect, 59.995f.Degrees());
                            break;
                        case -17:
                            AddAOE(rect, 180.Degrees());
                            break;
                        case 24:
                            AddAOE(rect, -60.Degrees());
                            break;
                    }
                break;
            case (uint)AID.Execration:
            case (uint)AID.FlareStar:
            case (uint)AID.MourningStar:
                if (AOEs.Count != 0)
                    AOEs.RemoveAt(0);
                break;
        }
    }
}
