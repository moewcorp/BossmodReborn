namespace BossMod.Stormblood.Ultimate.UWU;

class VulcanBurst(BossModule module, AID aid, Actor? source) : Components.Knockback(module, ActionID.MakeSpell(aid))
{
    protected Actor? SourceActor = source;

    public override ReadOnlySpan<Source> ActiveSources(int slot, Actor actor)
    {
        if (SourceActor != null)
            return new Source[1] { new(SourceActor.Position, 15f) }; // TODO: activation
        return [];
    }
}

class P2VulcanBurst(BossModule module) : VulcanBurst(module, AID.VulcanBurst, ((UWU)module).Ifrit());
class P4VulcanBurst(BossModule module) : VulcanBurst(module, AID.VulcanBurstUltima, ((UWU)module).Ultima());
