namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD50ServomechanicalChimera14X;

public enum OID : uint
{
    Boss = 0x3D9C, // R6.0
    Cacophony = 0x3D9D, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    SongsOfIceAndThunder = 31851, // Boss->self, 5.0s cast, range 9 circle
    SongsOfThunderAndIce = 31852, // Boss->self, 5.0s cast, range 8-40 donut
    TheRamsVoice1 = 31854, // Boss->self, no cast, range 9 circle
    TheRamsVoice2 = 32807, // Boss->self, no cast, range 9 circle
    TheDragonsVoice1 = 31853, // Boss->self, no cast, range 8-40 donut
    TheDragonsVoice2 = 32806, // Boss->self, no cast, range 8-40 donut
    RightbreathedCold = 31863, // Boss->self, 5.0s cast, range 40 180-degree cone
    LeftbreathedThunder = 31861, // Boss->self, 5.0s cast, range 40 180-degree cone
    ColdThunder = 31855, // Boss->player, 5.0s cast, width 8 rect charge
    ThunderousCold = 31856, // Boss->player, 5.0s cast, width 8 rect charge
    Cacophony = 31864, // Boss->self, 3.0s cast, single-target
    ChaoticChorus = 31865 // Cacophony->self, no cast, range 6 circle
}

public enum TetherID : uint
{
    OrbTether = 17 // Cacophony->player
}

class RamsDragonVoice(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(8f, 40f);
    private static readonly AOEShapeCircle circle = new(9f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];
        {
            for (var i = 0; i < max; ++i)
            {
                var aoe = _aoes[i];
                if (i == 0)
                    aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
                else
                    aoes[i] = aoe with { Risky = false };
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SongsOfIceAndThunder:
                AddAOEs(circle, donut, spell);
                break;
            case (uint)AID.SongsOfThunderAndIce:
                AddAOEs(donut, circle, spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ColdThunder:
                AddAOEs(circle, donut, spell);
                break;
            case (uint)AID.ThunderousCold:
                AddAOEs(donut, circle, spell);
                break;
        }
    }

    private void AddAOEs(AOEShape first, AOEShape second, ActorCastInfo spell)
    {
        var position = spell.LocXZ;
        _aoes.Add(new(first, position, default, Module.CastFinishAt(spell)));
        _aoes.Add(new(second, position, default, Module.CastFinishAt(spell, 3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.TheDragonsVoice1:
                case (uint)AID.TheRamsVoice1:
                case (uint)AID.TheDragonsVoice2:
                case (uint)AID.TheRamsVoice2:
                case (uint)AID.SongsOfIceAndThunder:
                case (uint)AID.SongsOfThunderAndIce:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

class TC(BossModule module, AID aid) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(aid), 4f);
class ThunderousCold(BossModule module) : TC(module, AID.ThunderousCold);
class ColdThunder(BossModule module) : TC(module, AID.ColdThunder);

class Breath(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40f, 90f.Degrees()));
class RightbreathedCold(BossModule module) : Breath(module, AID.RightbreathedCold);
class LeftbreathedThunder(BossModule module) : Breath(module, AID.LeftbreathedThunder);

class ChargeTether(BossModule module) : Components.StretchTetherDuo(module, 15f, 5.1f);

class Cacophony(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6);
    private readonly List<Actor> _orbs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _orbs.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
            aoes[i] = new(circle, _orbs[i].Position);
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Cacophony)
            _orbs.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ChaoticChorus)
            _orbs.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _orbs.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
        {
            var w = _orbs[i];
            hints.AddForbiddenZone(ShapeDistance.Capsule(w.Position, w.Rotation, 10f, 6f));
        }
    }
}

class CacophonyTether(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.OrbTether, 15f, needToKite: true);

class DD50ServomechanicalChimera14XStates : StateMachineBuilder
{
    public DD50ServomechanicalChimera14XStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Cacophony>()
            .ActivateOnEnter<CacophonyTether>()
            .ActivateOnEnter<RamsDragonVoice>()
            .ActivateOnEnter<ThunderousCold>()
            .ActivateOnEnter<ColdThunder>()
            .ActivateOnEnter<RightbreathedCold>()
            .ActivateOnEnter<LeftbreathedThunder>()
            .ActivateOnEnter<ChargeTether>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 901, NameID = 12265)]
public class DD50ServomechanicalChimera14X(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -300f), new ArenaBoundsCircle(19.5f));
