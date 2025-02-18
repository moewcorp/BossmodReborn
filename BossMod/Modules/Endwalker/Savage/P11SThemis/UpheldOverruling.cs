namespace BossMod.Endwalker.Savage.P11SThemis;

class UpheldOverruling(BossModule module) : Components.UniformStackSpread(module, 6, 13, 7, alwaysShowSpreads: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UpheldOverrulingLight:
            case AID.UpheldRulingLight:
                if (WorldState.Actors.Find(caster.Tether.Target) is var stackTarget && stackTarget != null)
                    AddStack(stackTarget, Module.CastFinishAt(spell, 0.3f));
                break;
            case AID.UpheldOverrulingDark:
            case AID.UpheldRulingDark:
                if (WorldState.Actors.Find(caster.Tether.Target) is var spreadTarget && spreadTarget != null)
                    AddSpread(spreadTarget, Module.CastFinishAt(spell, 0.3f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UpheldOverrulingAOELight:
            case AID.UpheldRulingAOELight:
                Stacks.Clear();
                break;
            case AID.UpheldOverrulingAOEDark:
            case AID.UpheldRulingAOEDark:
                Spreads.Clear();
                break;
        }
    }
}

abstract class Lightburst(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 13);
class LightburstBoss(BossModule module) : Lightburst(module, AID.LightburstBoss);
class LightburstClone(BossModule module) : Lightburst(module, AID.LightburstClone);

abstract class DarkPerimeter(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(8, 50));
class DarkPerimeterBoss(BossModule module) : DarkPerimeter(module, AID.DarkPerimeterBoss);
class DarkPerimeterClone(BossModule module) : DarkPerimeter(module, AID.DarkPerimeterClone);
