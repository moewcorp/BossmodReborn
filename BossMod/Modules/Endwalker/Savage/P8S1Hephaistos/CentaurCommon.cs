namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class Footprint(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.Footprint))
{
    public override ReadOnlySpan<Source> ActiveSources(int slot, Actor actor)
    {
        return new Source[1] { new(Module.PrimaryActor.Position, 20f) }; // TODO: activation
    }
}
