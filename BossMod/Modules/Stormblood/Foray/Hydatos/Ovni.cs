﻿namespace BossMod.Stormblood.Foray.Hydatos.Ovni;

public enum OID : uint
{
    Boss = 0x2685, // R16.000, x1
}

public enum AID : uint
{
    RockHard = 14786, // Boss->location, 3.0s cast, range 8 circle
    TorrentialTorment = 14785, // Boss->self, 5.0s cast, range 40+R 45-degree cone
    Fluorescence = 14789, // Boss->self, 3.0s cast, single-target
    PullOfTheVoid = 14782, // Boss->self, 3.0s cast, range 30 circle
    Megastorm = 14784, // Boss->self, 5.0s cast, range ?-40 donut
    IonShower = 14787, // Boss->player, 5.0s cast, single-target
    IonStorm = 14788, // Boss->players, no cast, range 20 circle
    VitriolicBarrage = 14790, // Boss->self, 4.0s cast, range 25+R circle
    ConcussiveOscillation = 14783, // Boss->self, 5.0s cast, range 24 circle
}

public enum IconID : uint
{
    IonShower = 111, // player->self
}

class PullOfTheVoid(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.PullOfTheVoid), 30f, kind: Kind.TowardsOrigin, minDistanceBetweenHitboxes: true);
class Megastorm(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Megastorm), new AOEShapeDonut(5f, 40f));
class ConcussiveOscillation(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ConcussiveOscillation), 24f);
class VitriolicBarrage(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.VitriolicBarrage));
class RockHard(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RockHard), 8);
class TorrentialTorment(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TorrentialTorment), new AOEShapeCone(56f, 22.5f.Degrees()));
class IonShower(BossModule module) : Components.GenericStackSpread(module, alwaysShowSpreads: true, raidwideOnResolve: false)
{
    private int _numCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.IonShower && WorldState.Actors.Find(targetID) is { } target)
        {
            _numCasts = 0;
            Spreads.Add(new(target, 20, WorldState.FutureTime(5d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.IonStorm && ++_numCasts >= 3)
            Spreads.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Spreads.Any(s => s.Target == actor))
            // just gtfo from boss as far as possible
            hints.GoalZones.Add(p => (p - Module.PrimaryActor.Position).LengthSq() > 1600f ? 100f : 0f);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class OvniStates : StateMachineBuilder
{
    public OvniStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PullOfTheVoid>()
            .ActivateOnEnter<Megastorm>()
            .ActivateOnEnter<ConcussiveOscillation>()
            .ActivateOnEnter<VitriolicBarrage>()
            .ActivateOnEnter<RockHard>()
            .ActivateOnEnter<TorrentialTorment>()
            .ActivateOnEnter<IonShower>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 639, NameID = 8060, Contributors = "xan")]
public class Ovni(WorldState ws, Actor primary) : BossModule(ws, primary, new(266.1068f, -97.09414f), new ArenaBoundsCircle(80f, MapResolution: 1));

