namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class Phaser(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Phaser), new AOEShapeCone(23f, 30f.Degrees())) // TODO: verify angle
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];

        var deadline = Casters[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count && Casters[index].Activation < deadline)
            ++index;

        return CollectionsMarshal.AsSpan(Casters)[..index];
    }
}
