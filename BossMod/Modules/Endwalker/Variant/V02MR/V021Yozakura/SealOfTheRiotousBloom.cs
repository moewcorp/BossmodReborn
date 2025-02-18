namespace BossMod.Endwalker.VariantCriterion.V02MR.V021Yozakura;

class SealOfRiotousBloom(BossModule module) : Components.GenericAOEs(module)
{
    private enum Element { Fire, Water, Thunder, Wind }
    private readonly List<AOEInstance> _aoes = new(10);
    private readonly List<Element> elements = new(4);
    private static readonly AOEShapeCircle circle = new(9f);
    private static readonly AOEShapeDonut donut = new(5f, 60f);
    private static readonly AOEShapeCone cone = new(70f, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 5 ? 5 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
            aoes[i] = _aoes[i];
        return aoes;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        Element? element = actor.OID switch
        {
            (uint)OID.Fire => Element.Fire,
            (uint)OID.Water => Element.Water,
            (uint)OID.Wind => Element.Wind,
            (uint)OID.Thunder => Element.Thunder,
            _ => null
        };
        if (element is Element e)
        {
            switch (state)
            {
                case 0x00100020: // seals spawn
                    elements.Add(e);
                    break;

                case 0x00400080: // seal activates
                    if (elements.Contains(e))
                        ActivateAOE(e, WorldState.FutureTime(8.1d));
                    break;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 4 && spell.Action.ID is (uint)AID.SealOfTheFireblossom or (uint)AID.SealOfTheWindblossom)
            _aoes.RemoveRange(0, 5);
    }

    private void ActivateAOE(Element element, DateTime activation)
    {
        switch (element)
        {
            case Element.Fire:
                AddAOE(circle, activation);
                break;
            case Element.Thunder:
                AddConeAOEs(Angle.AnglesCardinals, activation);
                break;
            case Element.Water:
                AddConeAOEs(Angle.AnglesIntercardinals, activation);
                break;
            case Element.Wind:
                AddAOE(donut, activation);
                break;
        }
        elements.Remove(element);
        var eCount = elements.Count;
        if (_aoes.Count == 5 && eCount != 0)
        {
            Element[] ele = [.. elements];
            for (var i = 0; i < eCount; ++i)
                ActivateAOE(ele[i], WorldState.FutureTime(16.3d));
        }
    }

    private void AddAOE(AOEShape shape, DateTime activation, Angle rotation = default) => _aoes.Add(new(shape, WPos.ClampToGrid(Arena.Center), rotation, activation));

    private void AddConeAOEs(ReadOnlySpan<Angle> angles, DateTime activation)
    {
        for (var i = 0; i < 4; ++i)
            AddAOE(cone, activation, angles[i]);
    }
}
