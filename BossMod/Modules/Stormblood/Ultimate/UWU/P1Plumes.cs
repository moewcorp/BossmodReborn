namespace BossMod.Stormblood.Ultimate.UWU;

class P1Plumes(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _razor = module.Enemies(OID.RazorPlume);
    private readonly List<Actor> _spiny = module.Enemies(OID.SpinyPlume);
    private readonly List<Actor> _satin = module.Enemies(OID.SatinPlume);

    public bool Active => _razor.Any(p => p.IsTargetable) || _spiny.Any(p => p.IsTargetable) || _satin.Any(p => p.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_razor);
        Arena.Actors(_spiny);
        Arena.Actors(_satin);
    }
}
