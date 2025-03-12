namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

class MercifulMoon(BossModule module) : Components.GenericGaze(module, ActionID.MakeSpell(AID.MercifulMoon))
{
    private Eye? _eye;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor) => Utils.ZeroOrOne(ref _eye);

    public override void Update()
    {
        var orbs = Module.Enemies((uint)OID.AetherialOrb);
        if (orbs.Count == 0)
            return;
        if (_eye == null && Module.Enemies((uint)OID.AetherialOrb)[0] is var orb)
            _eye = new(orb.Position, WorldState.FutureTime(5.8d)); // time from spawn to cast
    }
}
