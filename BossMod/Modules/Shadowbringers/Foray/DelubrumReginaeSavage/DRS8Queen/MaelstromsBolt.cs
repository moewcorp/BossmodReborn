namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

// TODO: show reflect hints, show stay under dome hints
class MaelstromsBolt(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.MaelstromsBoltAOE))
{
    private readonly List<Actor> _ballLightnings = module.Enemies(OID.BallLightning);
    private readonly List<Actor> _domes = module.Enemies(OID.ProtectiveDome);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in _ballLightnings.Where(b => !b.IsDead))
        {
            Arena.Actor(b, Colors.Object, true);
            Arena.AddCircle(b.Position, 8, Colors.Object);
        }
        for (var i = 0; i < _domes.Count; ++i)
            Arena.AddCircle(_domes[i].Position, 8, Colors.Safe);
    }
}
