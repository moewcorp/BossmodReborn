namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

class BombarianSpecial(BossModule module) : Components.UniformStackSpread(module, 5, 5, 2, 2, alwaysShowSpreads: true)
{
    public enum Mechanic { None, Spread, Pairs }

    public Mechanic CurMechanic;

    public void Show(float delay)
    {
        switch (CurMechanic)
        {
            case Mechanic.Spread:
                AddSpreads(Raid.WithoutSlot(true, true, true), WorldState.FutureTime(delay));
                break;
            case Mechanic.Pairs:
                // TODO: can target any role
                AddStacks(Raid.WithoutSlot(true, true, true).Where(p => p.Class.IsSupport()), WorldState.FutureTime(delay));
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurMechanic != Mechanic.None)
            hints.Add(CurMechanic.ToString());
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var mechanic = (AID)spell.Action.ID switch
        {
            AID.OctoboomBombarianSpecial => Mechanic.Spread,
            AID.QuadroboomBombarianSpecial => Mechanic.Pairs,
            _ => Mechanic.None
        };
        if (mechanic != Mechanic.None)
            CurMechanic = mechanic;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BombariboomSpread or AID.BombariboomPair)
        {
            Spreads.Clear();
            Stacks.Clear();
            CurMechanic = Mechanic.None;
        }
    }
}

class BombarianSpecialRaidwide(BossModule module) : Components.CastCounterMulti(module, [ActionID.MakeSpell(AID.BombarianSpecialRaidwide1),
ActionID.MakeSpell(AID.BombarianSpecialRaidwide2), ActionID.MakeSpell(AID.BombarianSpecialRaidwide3), ActionID.MakeSpell(AID.BombarianSpecialRaidwide4),
ActionID.MakeSpell(AID.BombarianSpecialRaidwide5), ActionID.MakeSpell(AID.BombarianSpecialRaidwide6), ActionID.MakeSpell(AID.SpecialBombarianSpecialRaidwide1),
ActionID.MakeSpell(AID.SpecialBombarianSpecialRaidwide2), ActionID.MakeSpell(AID.SpecialBombarianSpecialRaidwide3),
ActionID.MakeSpell(AID.SpecialBombarianSpecialRaidwide4), ActionID.MakeSpell(AID.SpecialBombarianSpecialRaidwide5), ActionID.MakeSpell(AID.SpecialBombarianSpecialRaidwide6)]);

abstract class SpecialOut(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 10);
class BombarianSpecialOut(BossModule module) : SpecialOut(module, AID.BombarianSpecialOut);
class SpecialBombarianSpecialOut(BossModule module) : SpecialOut(module, AID.SpecialBombarianSpecialOut);

abstract class SpecialIn(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(6, 40));
class BombarianSpecialIn(BossModule module) : SpecialIn(module, AID.BombarianSpecialIn);
class SpecialBombarianSpecialIn(BossModule module) : SpecialIn(module, AID.SpecialBombarianSpecialIn);

class BombarianSpecialAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BombarianSpecialAOE), 8);
class BombarianSpecialKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.BombarianSpecialKnockback), 10);

