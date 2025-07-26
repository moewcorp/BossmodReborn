﻿using BossMod.QuestBattle.Shadowbringers.RoleQuests;

namespace BossMod.Shadowbringers.Quest.Role.NyelbertsLament;

public enum OID : uint
{
    Boss = 0x2977,

    BovianBull = 0x2976,
    LooseBoulder = 0x2978, // R2.4
    Helper = 0x233C
}

public enum AID : uint
{
    FallingRock = 16595, // Helper->location, 3.0s cast, range 4 circle
    ZoomTargetSelect = 16599, // Helper->player, no cast, single-target
    ZoomIn = 16598, // Helper->self, no cast, range 42 width 8 rect
    TwoThousandMinaSlash = 16601, // Bovian->self/player, 5.0s cast, range 40 ?-degree cone
}

public enum SID : uint
{
    WingedShield = 1900
}

class TwoThousandMinaSlash : Components.GenericLineOfSightAOE
{
    private readonly List<Actor> _casters = [];

    public TwoThousandMinaSlash(BossModule module) : base(module, (uint)AID.TwoThousandMinaSlash, 40, false)
    {
        Refresh();
    }

    public Actor? ActiveCaster => _casters.MinBy(c => c.CastInfo!.RemainingTime);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _casters.Add(caster);
            Refresh();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _casters.Remove(caster);
            Refresh();
        }
    }

    private void Refresh()
    {
        var blockers = Module.Enemies((uint)OID.LooseBoulder);

        Modify(ActiveCaster?.CastInfo?.LocXZ, blockers.Select(b => (b.Position, b.HitboxRadius)), Module.CastFinishAt(ActiveCaster?.CastInfo));
        Safezones.Clear();
        AddSafezone(NextExplosion, default);
    }
}

class FallingRock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRock, 4f);
class ZoomIn(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.ZoomTargetSelect, (uint)AID.ZoomIn, 5.1d, 42)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaits.Count != 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 3f), ActiveBaits[0].Activation);
    }
}

class PassageOfArms(BossModule module) : BossComponent(module)
{
    private ActorCastInfo? EnrageCast => Module.PrimaryActor.CastInfo is { Action.ID: 16604 } castInfo ? castInfo : null;
    private Actor? Paladin => WorldState.Actors.FirstOrDefault(x => x.FindStatus((uint)SID.WingedShield) != null);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (EnrageCast != null && Paladin != null)
            hints.AddForbiddenZone(ShapeDistance.InvertedCone(Paladin.Position, 8, Paladin.Rotation + 180.Degrees(), 60.Degrees()), Module.CastFinishAt(EnrageCast));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (EnrageCast != null && Paladin != null)
            Arena.ZoneCone(Paladin.Position, 0f, 8f, Paladin.Rotation + 180f.Degrees(), 60f.Degrees(), Colors.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnrageCast != null && Paladin != null && !actor.Position.InCircleCone(Paladin.Position, 8f, Paladin.Rotation + 180.Degrees(), 60.Degrees()))
            hints.Add("Hide behind tank!");
    }
}

class NyelbertAI(BossModule module) : QuestBattle.RotationModule<AutoNyelbert>(module);

class BovianStates : StateMachineBuilder
{
    public BovianStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NyelbertAI>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<ZoomIn>()
            .ActivateOnEnter<TwoThousandMinaSlash>()
            .ActivateOnEnter<PassageOfArms>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69162, NameID = 8363)]
public class Bovian(WorldState ws, Actor primary) : BossModule(ws, primary, new(-440, -691), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly));
}
