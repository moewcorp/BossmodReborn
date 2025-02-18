namespace BossMod.Dawntrail.Savage.M02SHoneyBLovely;

class DropSplashOfVenom(BossModule module) : Components.UniformStackSpread(module, 6, 6, 2, 2, alwaysShowSpreads: true)
{
    public enum Mechanic { None, Pairs, Spread }

    public Mechanic NextMechanic;
    public DateTime Activation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (NextMechanic != Mechanic.None)
            hints.Add(NextMechanic.ToString());
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SplashOfVenom:
            case AID.SpreadLove:
                NextMechanic = Mechanic.Spread;
                break;
            case AID.DropOfVenom:
            case AID.DropOfLove:
                NextMechanic = Mechanic.Pairs;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TemptingTwistAOE:
            case AID.HoneyBeelineAOE:
            case AID.TemptingTwistBeatAOE:
            case AID.HoneyBeelineBeatAOE:
                switch (NextMechanic)
                {
                    case Mechanic.Pairs:
                        // note: it's random whether dd or supports are hit, select supports arbitrarily
                        Activation = WorldState.FutureTime(4.5f);
                        AddStacks(Raid.WithoutSlot(true, true, true).Where(p => p.Class.IsSupport()), Activation);
                        break;
                    case Mechanic.Spread:
                        Activation = WorldState.FutureTime(4.5f);
                        AddSpreads(Raid.WithoutSlot(true, true, true), Activation);
                        break;
                }
                break;
            case AID.SplashOfVenomAOE:
            case AID.DropOfVenomAOE:
            case AID.SpreadLoveAOE:
            case AID.DropOfLoveAOE:
                Spreads.Clear();
                Stacks.Clear();
                NextMechanic = Mechanic.None;
                break;
        }
    }
}

class Twist(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(7, 30));
class TemptingTwist(BossModule module) : Twist(module, AID.TemptingTwistAOE);
class TemptingTwistBeat(BossModule module) : Twist(module, AID.TemptingTwistBeatAOE);

class Beeline(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(60, 7));
class HoneyBeeline(BossModule module) : Beeline(module, AID.HoneyBeelineAOE);
class HoneyBeelineBeat(BossModule module) : Beeline(module, AID.HoneyBeelineBeatAOE);

class Splinter(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 8);
class PoisonCloudSplinter(BossModule module) : Splinter(module, AID.PoisonCloudSplinter);
class SweetheartSplinter(BossModule module) : Splinter(module, AID.SweetheartSplinter);
