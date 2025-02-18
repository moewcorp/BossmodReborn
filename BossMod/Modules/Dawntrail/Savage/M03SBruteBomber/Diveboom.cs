namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

abstract class Proximity(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 20);  // TODO: verify falloff
class OctoboomDiveProximity(BossModule module) : Proximity(module, AID.OctoboomDiveProximityAOE);
class QuadroboomDiveProximity(BossModule module) : Proximity(module, AID.QuadroboomDiveProximityAOE);

abstract class DiveKB(BossModule module, AID aid) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(aid), 25);
class OctoboomDiveKnockback(BossModule module) : DiveKB(module, AID.OctoboomDiveKnockbackAOE);
class QuadroboomDiveKnockback(BossModule module) : DiveKB(module, AID.QuadroboomDiveKnockbackAOE);

class Diveboom(BossModule module) : Components.UniformStackSpread(module, 5, 5, 2, 2, alwaysShowSpreads: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.OctoboomDiveProximityAOE:
            case AID.OctoboomDiveKnockbackAOE:
                AddSpreads(Raid.WithoutSlot(true, true, true), Module.CastFinishAt(spell));
                break;
            case AID.QuadroboomDiveProximityAOE:
            case AID.QuadroboomDiveKnockbackAOE:
                // TODO: can target any role
                AddStacks(Raid.WithoutSlot(true, true, true).Where(p => p.Class.IsSupport()), Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DiveboomSpread or AID.DiveboomPair)
        {
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}
