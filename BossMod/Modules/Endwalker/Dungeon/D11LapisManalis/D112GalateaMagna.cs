namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D112GalateaMagna;

public enum OID : uint
{
    Boss = 0x3971, //R=5.0
    Helper2 = 0x3D06, // R3.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 32625, // Boss->location, no cast, single-target, boss teleports to spot marked by icons 1,2,3,4 or to mid

    WaningCycleVisual = 32623, // Boss->self, no cast, single-target (between in->out)
    WaxingCycleVisual = 31378, // Boss->self, no cast, single-target (between out->in)
    WaningCycle1 = 32622, // Boss->self, 4.0s cast, range 10-40 donut
    WaningCycle2 = 32624, // Helper->self, 6.0s cast, range 10 circle
    WaxingCycle1 = 31377, // Boss->self, 4.0s cast, range 10 circle
    WaxingCycle2 = 31379, // Helper->self, 6.7s cast, range 10-40 donut

    SoulScythe = 31386, // Boss->location, 6.0s cast, range 18 circle
    SoulNebula = 31390, // Boss->self, 5.0s cast, range 40 circle, raidwide
    ScarecrowChaseVisual1 = 31387, // Boss->self, 8.0s cast, single-target
    ScarecrowChaseVisual2 = 31389, // Boss->self, no cast, single-target
    ScarecrowChase = 32703, // Helper->self, 1.8s cast, range 40 width 10 cross

    Tenebrism = 31382, // Boss->self, 4.0s cast, range 40 circle, small raidwide, spawns 4 towers, applies glass-eyed on tower resolve
    Burst = 31383, // Helper->self, no cast, range 5 circle, tower success
    BigBurst = 31384, // Helper->self, no cast, range 60 circle, tower fail
    StonyGaze = 31385 // Helper->self, no cast, gaze
}

public enum IconID : uint
{
    Icon1 = 336, // Helper2
    Icon2 = 337, // Helper2
    Icon3 = 338, // Helper2
    Icon4 = 339 // Helper2
}

public enum SID : uint
{
    Doom = 3364, // Helper->player, extra=0x0
    GlassyEyed = 3511 // Boss->player, extra=0x0, takes possession of the player after status ends and does a petrifying attack in all direction
}

class ScarecrowChase(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCross cross = new(40, 5);
    private readonly List<AOEInstance> _aoes = new(4);
    private bool first = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= (uint)IconID.Icon1 and <= (uint)IconID.Icon4)
        {
            _aoes.Add(new(cross, WPos.ClampToGrid(actor.Position), Angle.AnglesIntercardinals[1], WorldState.FutureTime(9.9d).AddSeconds((-(uint)IconID.Icon1 + iconID) * 3d)));
            if (_aoes.Count is var count && (count == 4 || count == 2 && first))
            {
                first = false;
                _aoes.SortBy(x => x.Activation);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.ScarecrowChase)
            _aoes.RemoveAt(0);
    }
}

class WaxingCycle(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 40f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WaxingCycle1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.WaxingCycle1 => 0,
                (uint)AID.WaxingCycle2 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2.7d));
        }
    }
}

class WaningCycle(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeDonut(10f, 40f), new AOEShapeCircle(10f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WaningCycle1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.WaningCycle1 => 0,
                (uint)AID.WaningCycle2 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class GlassyEyed(BossModule module) : Components.GenericGaze(module)
{
    private DateTime _activation;
    private readonly List<Actor> _affected = new(4);

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = _affected.Count;
        if (count == 0 || WorldState.CurrentTime < _activation.AddSeconds(-10d))
            return [];
        var eyes = new Eye[count];
        for (var i = 0; i < count; ++i)
            eyes[i] = new(_affected[i].Position, _activation);
        return eyes;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.GlassyEyed)
        {
            _activation = status.ExpireAt;
            _affected.Add(actor);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.GlassyEyed)
            _affected.Remove(actor);
    }
}

public class TenebrismTowers(BossModule module) : Components.GenericTowers(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00010008)
        {
            WPos position = index switch
            {
                0x07 => new(350f, -404f),
                0x08 => new(360f, -394f),
                0x09 => new(350f, -384f),
                0x0A => new(340f, -394f),
                _ => default
            };
            if (position != default)
                Towers.Add(new(position, 5f, activation: WorldState.FutureTime(6d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
            for (var i = 0; i < Towers.Count; ++i)
            {
                var tower = Towers[i];
                if (tower.Position == caster.Position)
                {
                    Towers.RemoveAt(i);
                    break;
                }
            }
    }
}

class Doom(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _doomed = new(4);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
            _doomed.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
            _doomed.Remove(actor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = _doomed.Count;
        if (count == 0)
            return;
        if (_doomed.Contains(actor) && !(actor.Role == Role.Healer || actor.Class == Class.BRD))
            hints.Add("You were doomed! Get cleansed fast.");
        if (!(actor.Role == Role.Healer || actor.Class == Class.BRD))
            return;
        if (_doomed.Contains(actor))
            hints.Add("Cleanse yourself! (Doom).");
        for (var i = 0; i < count; ++i)
        {
            var doom = _doomed[i];
            if (!_doomed.Contains(actor))
                hints.Add($"Cleanse {doom.Name} (Doom)");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _doomed.Count;
        if (count != 0)
            for (var i = 0; i < count; ++i)
            {
                var doom = _doomed[i];
                if (actor.Role == Role.Healer)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Esuna), doom, ActionQueue.Priority.High, castTime: 1);
                else if (actor.Class == Class.BRD)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(BRD.AID.WardensPaean), doom, ActionQueue.Priority.High);
            }
    }
}

class SoulScythe(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SoulScythe), 18f);

class D112GalateaMagnaStates : StateMachineBuilder
{
    public D112GalateaMagnaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<TenebrismTowers>()
            .ActivateOnEnter<WaningCycle>()
            .ActivateOnEnter<WaxingCycle>()
            .ActivateOnEnter<GlassyEyed>()
            .ActivateOnEnter<SoulScythe>()
            .ActivateOnEnter<ScarecrowChase>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 896, NameID = 10308)]
public class D112GalateaMagna(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(350f, -394f), 19.5f)], [new Rectangle(new(350f, -373f), 20f, 2.25f), new Rectangle(new(350f, -414f), 20f, 1.25f)]);
}
