namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class PartedPlumes : Components.SimpleAOEs
{
    public PartedPlumes(BossModule module) : base(module, ActionID.MakeSpell(AID.PartedPlumes), new AOEShapeCone(50, 10.Degrees()), 16) { MaxDangerColor = 2; }
}

class PartedPlumesVoidzone(BossModule module) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        yield return new(_shape, new WPos(100, 100));
    }
}
