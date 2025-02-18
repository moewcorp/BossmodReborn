namespace BossMod.Stormblood.Ultimate.UCOB;

class LiquidHell(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.LiquidHell), m => m.Enemies(OID.VoidzoneLiquidHell).Where(z => z.EventState != 7), 1.3f)
{
    public void Reset() => NumCasts = 0;
}

class P1LiquidHell(BossModule module) : LiquidHell(module)
{
    public override bool KeepOnPhaseChange => true;
}
