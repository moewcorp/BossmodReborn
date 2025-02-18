namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class Phaser(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Phaser), new AOEShapeCone(23f, 30f.Degrees())) // TODO: verify angle
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];
        var deadline = Casters[0].Activation.AddSeconds(1d);

        var aoes = new AOEInstance[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var caster = Casters[i];
            if (caster.Activation < deadline)
                aoes[index++] = caster;
        }
        return aoes[..index];
    }
}
