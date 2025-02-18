﻿namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D063Rala;

public enum OID : uint
{
    Boss = 0x34C7, // R=4.75
    GoldenWings = 0x34C8, // R=1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    BenevolenceVisual = 25945, // Boss->self, 5.0s cast, single-target
    Benevolence = 25946, // Helper->players, 5.4s cast, range 6 circle, stack
    LamellarLight1 = 25939, // Helper->self, 6.0s cast, range 15 circle
    LamellarLight2 = 25942, // GoldenWings->self, 3.0s cast, single-target
    LamellarLight3 = 25951, // Helper->self, 3.0s cast, range 40 width 4 rect
    Lifesbreath = 25940, // Boss->self, 4.0s cast, range 50 width 10 rect
    LovingEmbraceLeft = 25943, // Boss->self, 7.0s cast, range 45 180-degree cone
    LovingEmbraceRight = 25944, // Boss->self, 7.0s cast, range 45 180-degree cone
    Pity = 25949, // Boss->player, 5.0s cast, single-target, tankbuster
    Prance1 = 25937, // Boss->location, 5.0s cast, single-target
    Prance2 = 25938, // Boss->location, no cast, single-target
    StillEmbraceVisual = 25947, // Boss->self, 5.0s cast, single-target
    StillEmbrace = 25948, // Helper->player, 5.4s cast, range 6 circle, spread mechanic
    Visual = 25941, // Boss->location, no cast, single-target, shortly before spawning Golden Wings
    WarmGlow = 25950 // Boss->self, 5.0s cast, range 40 circle, raidwide
}

public enum SID : uint
{
    Doom = 1769 // Helper->player, extra=0x0, heal to full doom
}

class Doom(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _doomed = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _doomed.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _doomed.Remove(actor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_doomed.Count != 0)
            if (_doomed.Contains(actor))
                if (!(actor.Role == Role.Healer))
                    hints.Add("You were doomed! Get healed to full fast.");
                else
                    hints.Add("Heal yourself to full! (Doom).");
            else if (actor.Role == Role.Healer)
                foreach (var c in _doomed)
                    hints.Add($"Heal to full {c.Name}! (Doom)");
    }
}

class LamellarLightCircle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LamellarLight1), 15, 3);
class Lifesbreath(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Lifesbreath), new AOEShapeRect(50, 5));
class LamellarLightRect(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LamellarLight3), new AOEShapeRect(40, 2));
class StillEmbrace(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.StillEmbrace), 6);
class Benevolence(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Benevolence), 6, 4, 4);

class LovingEmbrace(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(45, 90.Degrees()));
class LovingEmbraceLeft(BossModule module) : LovingEmbrace(module, AID.LovingEmbraceLeft);
class LovingEmbraceRight(BossModule module) : LovingEmbrace(module, AID.LovingEmbraceRight);

class Pity(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.Pity));
class WarmGlow(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WarmGlow));

class D063RalaStates : StateMachineBuilder
{
    public D063RalaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<LamellarLightCircle>()
            .ActivateOnEnter<LamellarLightRect>()
            .ActivateOnEnter<Lifesbreath>()
            .ActivateOnEnter<StillEmbrace>()
            .ActivateOnEnter<Benevolence>()
            .ActivateOnEnter<LovingEmbraceLeft>()
            .ActivateOnEnter<LovingEmbraceRight>()
            .ActivateOnEnter<Pity>()
            .ActivateOnEnter<WarmGlow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 792, NameID = 10316)]
public class D063Rala(WorldState ws, Actor primary) : BossModule(ws, primary, defaultBounds.Center, defaultBounds)
{
    private static readonly ArenaBoundsComplex defaultBounds = new([new Polygon(new(-380, -135), 19.5f * CosPI.Pi32th, 32)], [new Rectangle(new(-380, -114.25f), 20, 2)]);
}
