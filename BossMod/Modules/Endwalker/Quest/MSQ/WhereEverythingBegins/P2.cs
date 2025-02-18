﻿using BossMod.Endwalker.Quest.MSQ.WhereEverythingBegins.P1;

namespace BossMod.Endwalker.Quest.MSQ.WhereEverythingBegins.P2;

public enum OID : uint
{
    Boss = 0x39B7, // R7.7
    PlunderedMaid = 0x39B8, // R1.5
    PlunderedGuard = 0x39BA, // R2.0
    FilthyShackle = 0x39BB, // R2.0
    VarshahnShield = 0x1EB762, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 30053, // Boss->tank, no cast, single-target
    Teleport = 30335, // Boss->location, no cast, single-target

    CursedNoiseVisual = 30026, // Boss->self, 5.0s cast, single-target
    CursedNoise = 30027, // Helper->self, no cast, range 60 circle

    BlightedBuffet = 30032, // Boss->self, 18.0s cast, range 9 circle
    VacuumWave = 30033, // Helper->self, 18.5s cast, range 60 circle
    DarkMist1 = 30034, // PlunderedMaid->self, 17.0s cast, range 8 circle
    DarkMist2 = 31259, // PlunderedMaid->self, 17.0s cast, range 16 circle
    VoidThunderIII = 30054, // PlunderedMaid->tank, no cast, single-target

    VoidSlashVisual = 30048, // PlunderedGuard->self, 14.7s cast, single-target
    VoidSlash = 30049, // Helper->self, 15.0s cast, range 30 90-degree cone

    VoidQuakeIIIVisual = 30045, // Boss->self, 20.0s cast, single-target
    VoidQuakeIII = 30046, // Helper->self, 20.0s cast, range 40 width 10 cross

    BlightedSweep = 30052, // Boss->self, 7.0s cast, range 40 180-degree cone
    BlightedSwathe = 30044, // Boss->self, 30.0s cast, range 40 180-degree cone

    VoidVortexVisual = 30024, // Boss->self, 4.0+1,0s cast, single-target
    VoidVortex = 30025, // Helper->players, 5.0s cast, range 6 circle
    CursebindVisual = 30042, // Boss->self, 5.0s cast, single-target
    Cursebind = 30043, // Helper->tank, 6.0s cast, single-target

    CursedEcho = 30035, // Boss->self, 5.0s cast, range 60 circle
    RottenRampageVisual1 = 30028, // Boss->self, 8.0+2,0s cast, single-target
    RottenRampageVisual2 = 30030, // Boss->self, no cast, single-target
    RottenRampage = 30031, // Helper->location, 10.0s cast, range 6 circle
    RottenRampageSpread = 30056, // Helper->player, 10.0s cast, range 6 circle

    DeathStreak = 31242, // PlunderedGuard->self, 20.0s cast, range 60 circle
}

class CursedNoise(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.CursedNoiseVisual), ActionID.MakeSpell(AID.CursedNoise), 0.1f);
class CursedEcho(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CursedEcho));
class DeathStreak(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DeathStreak));
class VoidVortex(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.VoidVortex), 6, 4, 4);
class RottenRampageSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.RottenRampageSpread), 6);
class RottenRampage(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RottenRampage), 6);

abstract class BlightedCleave(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40, 90.Degrees()));
class BlightedSwathe(BossModule module) : BlightedCleave(module, AID.BlightedSwathe)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;
        var enemies = Module.Enemies(OID.FilthyShackle);
        var isFettered = false;
        for (var i = 0; i < enemies.Count; ++i)
        {
            if (enemies[i].IsDead)
                isFettered = true;
        }

        if (isFettered)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
class BlightedSweep(BossModule module) : BlightedCleave(module, AID.BlightedSweep);

class BlightedBuffet(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BlightedBuffet), 9);
class DarkMist1 : Components.SimpleAOEs
{
    public DarkMist1(BossModule module) : base(module, ActionID.MakeSpell(AID.DarkMist1), 8)
    {
        MaxRisky = 3;
    }
}

class DarkMist2 : Components.SimpleAOEs
{
    public DarkMist2(BossModule module) : base(module, ActionID.MakeSpell(AID.DarkMist2), 16)
    {
        MaxRisky = 7;
    }
}

class VoidSlash : Components.SimpleAOEs
{
    public VoidSlash(BossModule module) : base(module, ActionID.MakeSpell(AID.VoidSlash), new AOEShapeCone(30, 45.Degrees()))
    {
        MaxRisky = 3;
        Color = Colors.Danger;
    }
}

class VacuumWave(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.VacuumWave), 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;
        var c = Casters[0];
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(c.Position, 13), activation: Module.CastFinishAt(c.CastInfo));
    }
}
class VoidQuakeIII(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.VoidQuakeIII), new AOEShapeCross(40, 5));

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeDonut donut = new(18, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (NumCasts == 0 && (AID)spell.Action.ID == AID.CursedNoiseVisual)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (NumCasts == 0 && index == 0x01 && state == 0x00020001)
        {
            _aoe = null;
            ++NumCasts;
            Arena.Bounds = ScarmiglioneP2.DefaultBounds;
            Arena.Center = ScarmiglioneP2.DefaultBounds.Center;
        }
    }
}

class Shield(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(5, true);
    private AOEInstance? _aoe;
    private const string Hint = "Go under shield!";

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.VarshahnShield)
            _aoe = new(circle, actor.Position, Color: Colors.SafeFromAOE);
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if ((OID)actor.OID == OID.VarshahnShield && state == 0x0004)
            _aoe = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe == null)
            return;
        if (!_aoe.Value.Check(actor.Position))
            hints.Add(Hint);
        else
            hints.Add(Hint, false);
    }
}

public class ScarmiglioneP2States : StateMachineBuilder
{
    public ScarmiglioneP2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<CursedNoise>()
            .ActivateOnEnter<CursedEcho>()
            .ActivateOnEnter<DeathStreak>()
            .ActivateOnEnter<DarkMist1>()
            .ActivateOnEnter<DarkMist2>()
            .ActivateOnEnter<VoidSlash>()
            .ActivateOnEnter<BlightedSweep>()
            .ActivateOnEnter<BlightedBuffet>()
            .ActivateOnEnter<BlightedSwathe>()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<VoidQuakeIII>()
            .ActivateOnEnter<RottenRampage>()
            .ActivateOnEnter<RottenRampageSpread>()
            .ActivateOnEnter<Shield>()
            .ActivateOnEnter<VoidVortex>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70130, NameID = 11407)]
public class ScarmiglioneP2(WorldState ws, Actor primary) : BossModule(ws, primary, ScarmiglioneP1.ArenaBounds.Center, ScarmiglioneP1.ArenaBounds)
{
    public static readonly ArenaBoundsComplex DefaultBounds = new([new Polygon(ScarmiglioneP1.ArenaCenter, 18, 36)]);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID == (uint)OID.FilthyShackle ? 1 : 0;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.FilthyShackle), Colors.Object);
    }
}
