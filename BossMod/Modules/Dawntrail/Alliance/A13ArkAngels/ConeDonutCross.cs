namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class ConcertedDissolution(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ConcertedDissolution), new AOEShapeCone(40, 20.Degrees()))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var chain = Module.FindComponent<LightsChain>();
        var check = chain != null && chain.Casters.Count != 0;
        return ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), check ? Colors.Danger : Colors.AOE));
    }
}

class LightsChain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LightsChain), new AOEShapeDonut(4, 40))
{
    private readonly ConcertedDissolution? _aoe = module.FindComponent<ConcertedDissolution>();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var reaver = Module.FindComponent<CrossReaver>();
        var check = _aoe != null && _aoe.Casters.Count != 0;
        var check2 = reaver != null && reaver.Casters.Count != 0;
        return ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), check2 ? Colors.Danger : Colors.AOE, !check));
    }
}

class CrossReaver(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CrossReaverAOE), new AOEShapeCross(50, 6))
{
    private readonly LightsChain? _aoe = module.FindComponent<LightsChain>();
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var check = _aoe != null && _aoe.Casters.Count != 0;
        return ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), Risky: !check));
    }
}
