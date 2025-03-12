namespace BossMod.Endwalker.VariantCriterion.V01SS.V013Gladiator;

class SunderedRemains(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SunderedRemains), 10f);
class Landing(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Landing), 20f);

class GoldenFlame(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GoldenFlame), new AOEShapeRect(60f, 5f));
class SculptorsPassion(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SculptorsPassion), new AOEShapeRect(60f, 4f));
class RackAndRuin(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RackAndRuin), new AOEShapeRect(40f, 2.5f), 8);

class MightySmite(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.MightySmite));

class BitingWindBad(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BitingWindBad), 4f);

class ShatteringSteel(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShatteringSteel), "Get in bigger Whirlwind to dodge");
class ViperPoisonPatterns(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6f, ActionID.MakeSpell(AID.BitingWindBad), GetVoidzones, 0f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.WhirlwindBad);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class RingOfMight1(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(8f), new AOEShapeDonut(8f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RingOfMight1Out)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.RingOfMight1Out => 0,
                (uint)AID.RingOfMight1In => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class RingOfMight2(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(13f), new AOEShapeDonut(13f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RingOfMight2Out)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.RingOfMight2Out => 0,
                (uint)AID.RingOfMight2In => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class RingOfMight3(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(18f), new AOEShapeDonut(18f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RingOfMight3Out)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.RingOfMight3Out => 0,
                (uint)AID.RingOfMight3In => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class RushOfMight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCone _shape = new(60f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.RushOfMightFront or (uint)AID.RushOfMightBack)
        {
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 2)
                _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.RushOfMightFront or (uint)AID.RushOfMightBack)
            _aoes.RemoveAt(0);
    }
}

class FlashOfSteel1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FlashOfSteel1));
class FlashOfSteel2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FlashOfSteel2));
class ShatteringSteelMeteor(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.ShatteringSteel), 60f)
{
    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var boulders = Module.Enemies((uint)OID.AntiqueBoulder);
        var count = boulders.Count;
        if (count == 0)
            return [];
        var actors = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var b = boulders[i];
            if (!b.IsDead)
                actors.Add(b);
        }
        return CollectionsMarshal.AsSpan(actors);
    }
}

class SilverFlame(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeRect _shape = new(60f, 4f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var increment = spell.Action.ID switch
        {
            (uint)AID.SilverFlameFirstCW => -10f.Degrees(),
            (uint)AID.SilverFlameFirstCCW => 10f.Degrees(),
            _ => default
        };
        if (increment != default)
            Sequences.Add(new(_shape, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell), 2f, 5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SilverFlameFirstCCW or (uint)AID.SilverFlameFirstCW or (uint)AID.SilverFlameRest)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11387)]
public class V013Gladiator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35, -271), new ArenaBoundsSquare(20));
