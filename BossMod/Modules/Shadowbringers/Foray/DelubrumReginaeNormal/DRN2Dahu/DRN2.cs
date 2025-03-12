namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN2Dahu;

class FallingRock(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FallingRock), 4f);
class HotCharge(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.HotCharge), 4f);
class Firebreathe(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Firebreathe), new AOEShapeCone(60f, 45f.Degrees()));
class HeadDown(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.HeadDown), 2f);
class HuntersClaw(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HuntersClaw), 8f);

class FeralHowl(BossModule module) : Components.Knockback(module)
{
    private Actor? _source;

    public override ReadOnlySpan<Source> ActiveSources(int slot, Actor actor)
    {
        if (_source != null)
            return new Source[1] { new(_source.Position, 30f, Module.CastFinishAt(_source.CastInfo)) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FeralHowl)
            _source = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FeralHowl)
        {
            _source = null;
            ++NumCasts;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9751)]
public class DRN2Dahu(WorldState ws, Actor primary) : BossModule(ws, primary, new(82f, 138f), new ArenaBoundsCircle(30f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.Marchosias));
    }
}
