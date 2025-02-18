﻿namespace BossMod.Dawntrail.Hunt.RankS.ArchAethereater;

public enum OID : uint
{
    Boss = 0x4544, // R9.6
}

public enum AID : uint
{
    AutoAttack = 39517, // Boss->player, no cast, single-target

    Aethermodynamics1 = 39840, // Boss->self, 5.0s cast, range 40 circle
    Aethermodynamics2 = 39512, // Boss->self, 5.0s cast, range 40 circle
    Aethermodynamics3 = 39511, // Boss->self, 5.0s cast, range 40 circle
    Aethermodynamics4 = 39839, // Boss->self, 5.0s cast, range 40 circle
    FireIV1 = 39829, // Boss->self, 5.0s cast, range 15 circle
    FireIV2 = 39800, // Boss->self, 1.0s cast, range 15 circle
    FireIV3 = 39513, // Boss->self, 1.0s cast, range 15 circle
    FireIV4 = 39802, // Boss->self, 1.0s cast, range 15 circle
    FireIV5 = 39837, // Boss->self, 5.0s cast, range 15 circle
    BlizzardIV1 = 39838, // Boss->self, 5.0s cast, range 6-40 donut
    BlizzardIV2 = 39514, // Boss->self, 1.0s cast, range 6-40 donut
    BlizzardIV3 = 39801, // Boss->self, 1.0s cast, range 6-40 donut
    BlizzardIV4 = 39803, // Boss->self, 1.0s cast, range 6-40 donut
    BlizzardIV5 = 39830, // Boss->self, 5.0s cast, range 6-40 donut
    SoullessStream1 = 39509, // Boss->self, 5.0s cast, range 40 180-degree cone, FireIV2 combo or FireIV4 combo
    SoullessStream2 = 39510, // Boss->self, 5.0s cast, range 40 180-degree cone, BlizzardIV2 or BlizzardIV4 combo
    SoullessStream3 = 39507, // Boss->self, 5.0s cast, range 40 180-degree cone, BlizzardIV3 combo
    SoullessStream4 = 39508, // Boss->self, 5.0s cast, range 40 180-degree cone, FireIV3 combo
    Obliterate = 39515, // Boss->players, 5.0s cast, range 6 circle, stack
    Meltdown = 39516, // Boss->self, 3.0s cast, range 40 width 10 rect
    unknown = 19277 // Boss->self, no cast, single-target
}

public enum SID : uint
{
    Heatstroke = 4141, // Boss->player, extra=0x0
    ColdSweats = 4142, // Boss->player, extra=0x0
    Pyretic = 960, // none->player, extra=0x0
    FreezingUp = 2540, // none->player, extra=0x0
    DeepFreeze = 3519 // none->player, extra=0x0
}

class Heatstroke(BossModule module) : Components.StayMove(module, 3)
{
    private BitMask _heatstroke;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Heatstroke && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _heatstroke.Set(Raid.FindSlot(actor.InstanceID));
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            if (status.ID == (uint)SID.Heatstroke)
                _heatstroke.Clear(Raid.FindSlot(actor.InstanceID));
            else if (status.ID == (uint)SID.Pyretic)
                PlayerStates[slot] = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_heatstroke[slot])
            hints.Add($"Heatstroke on you in {(actor.FindStatus(SID.Heatstroke)!.Value.ExpireAt - WorldState.CurrentTime).TotalSeconds:f1}s. (Pyretic!)");
    }
}

class ColdSweats(BossModule module) : Components.StayMove(module, 3)
{
    private BitMask _coldsweats;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.ColdSweats && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = new(Requirement.Move, status.ExpireAt);
            _coldsweats.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            if (status.ID == (uint)SID.ColdSweats)
                _coldsweats.Clear(Raid.FindSlot(actor.InstanceID));
            else if (status.ID == (uint)SID.FreezingUp)
                PlayerStates[slot] = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_coldsweats[slot])
            hints.Add($"Cold Sweats on you in {(actor.FindStatus(SID.ColdSweats)!.Value.ExpireAt - WorldState.CurrentTime).TotalSeconds:f1}s. (Freezing!)");
    }
}

class Aethermodynamics1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Aethermodynamics1));
class Aethermodynamics2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Aethermodynamics2));
class Aethermodynamics3(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Aethermodynamics3));
class Aethermodynamics4(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Aethermodynamics4));

class Obliterate(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Obliterate), 6f, 8);
class Meltdown(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Meltdown), new AOEShapeRect(40f, 5f));

class Blizzard(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(6f, 40f));
class BlizzardIV1(BossModule module) : Blizzard(module, AID.BlizzardIV1);
class BlizzardIV5(BossModule module) : Blizzard(module, AID.BlizzardIV5);

class Fire(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 15f);
class FireIV1(BossModule module) : Fire(module, AID.FireIV1);
class FireIV5(BossModule module) : Fire(module, AID.FireIV5);

class SoullessStreamFireBlizzardCombo(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40f, 90f.Degrees());
    private static readonly AOEShapeDonut donut = new(6f, 40f);
    private static readonly AOEShapeCircle circle = new(15f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        {
            for (var i = 0; i < count; ++i)
            {
                var aoe = _aoes[i];
                if (i == 0)
                    aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
                else
                    aoes[i] = aoe;
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SoullessStream1:
            case (uint)AID.SoullessStream4:
                AddAOEs(cone, circle);
                break;
            case (uint)AID.SoullessStream2:
            case (uint)AID.SoullessStream3:
                AddAOEs(cone, donut);
                break;
        }

        void AddAOEs(AOEShape primaryShape, AOEShape secondaryShape)
        {
            var position = Module.PrimaryActor.Position;
            _aoes.Add(new(primaryShape, position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(secondaryShape, position, default, Module.CastFinishAt(spell, 2.5f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.SoullessStream1:
                case (uint)AID.SoullessStream2:
                case (uint)AID.SoullessStream3:
                case (uint)AID.SoullessStream4:
                case (uint)AID.BlizzardIV2:
                case (uint)AID.BlizzardIV3:
                case (uint)AID.BlizzardIV4:
                case (uint)AID.FireIV2:
                case (uint)AID.FireIV3:
                case (uint)AID.FireIV4:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

class ArchAethereaterStates : StateMachineBuilder
{
    public ArchAethereaterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Heatstroke>()
            .ActivateOnEnter<ColdSweats>()
            .ActivateOnEnter<Meltdown>()
            .ActivateOnEnter<Aethermodynamics1>()
            .ActivateOnEnter<Aethermodynamics2>()
            .ActivateOnEnter<Aethermodynamics3>()
            .ActivateOnEnter<Aethermodynamics4>()
            .ActivateOnEnter<Obliterate>()
            .ActivateOnEnter<BlizzardIV1>()
            .ActivateOnEnter<FireIV1>()
            .ActivateOnEnter<BlizzardIV5>()
            .ActivateOnEnter<FireIV5>()
            .ActivateOnEnter<SoullessStreamFireBlizzardCombo>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.SS, NameID = 13406)]
public class ArchAethereater(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
