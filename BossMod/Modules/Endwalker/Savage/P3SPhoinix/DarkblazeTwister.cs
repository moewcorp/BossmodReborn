namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to darkblaze twister mechanics
class TwisterVoidzone(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(OID.TwisterVoidzone).Where(z => z.EventState != 7));
class BurningTwister(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BurningTwister), new AOEShapeDonut(7, 20));

class DarkTwister(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.DarkTwister), _knockbackRange, true)
{
    private const float _knockbackRange = 17;
    private const float _aoeInnerRadius = 5;
    private const float _aoeMiddleRadius = 7;
    private const float safeOffset = _knockbackRange + (_aoeInnerRadius + _aoeMiddleRadius) / 2;
    private const float safeRadius = (_aoeMiddleRadius - _aoeInnerRadius) / 2;

    public IEnumerable<Actor> BurningTwisters() => Module.Enemies(OID.DarkblazeTwister).Where(t => t.CastInfo?.IsSpell(AID.BurningTwister) ?? false);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (Casters.Count == 0)
            return;
        var darkTwister = Casters[0];
        foreach (var burningTwister in BurningTwisters())
        {
            var dir = burningTwister.Position - darkTwister.Position;
            var len = dir.Length();
            dir /= len;
            Arena.AddCircle(darkTwister.Position + dir * (len - safeOffset), safeRadius, Colors.Safe);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<TwisterVoidzone>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) ||
    (Module.FindComponent<BurningTwister>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);

}
