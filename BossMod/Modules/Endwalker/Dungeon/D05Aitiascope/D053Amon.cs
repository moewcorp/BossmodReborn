namespace BossMod.Endwalker.Dungeon.D05Aitiascope.D053Amon;

public enum OID : uint
{
    Boss = 0x346E, // R=16.98
    YsaylesSpirit = 0x346F, // R2.0
    Antistrophe = 0x1EB26D, // R0.5
    Ice = 0x3470, // R6.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 24712, // Boss->player, no cast, single-target

    Antistrophe = 25694, // Boss->self, 3.0s cast, single-target
    DarkForte = 25700, // Boss->player, 5.0s cast, single-targe, tankbuster
    Entracte = 25701, // Boss->self, 5.0s cast, range 40 circle, raidwide

    DreamsOfIce = 27756, // Helper->self, 14.7s cast, range 6 circle, knockback 13, dir forward, summons ice to hide behind
    CurtainCall = 25702, // Boss->self, 32.0s cast, range 40 circle, line of sight AOE

    Epode = 25695, // Helper->self, 8.0s cast, range 70 width 12 rect

    EruptionForteVisual = 24709, // Boss->self, 3.0s cast, single-target
    EruptionForte = 25704, // Helper->location, 4.0s cast, range 8 circle, baited AOE

    LeftFiragaForte = 25697, // Boss->self, 7.0s cast, range 40 width 20 rect
    RightFiragaForte = 25696, // Boss->self, 7.0s cast, range 40 width 20 rect

    Strophe = 25693, // Boss->self, 3.0s cast, single-target

    ThundagaForteProximity = 25690, // Boss->location, 5.0s cast, range 40 circle, damage fall off AoE
    ThundagaForteCone1 = 25691, // Helper->self, 5.0s cast, range 20 45-degree cone
    ThundagaForteCone2 = 25692, // Helper->self, 11.0s cast, range 20 45-degree cone

    Visual = 25703 // YsaylesSpirit->self, no cast, single-target
}

class CurtainCallArenaChange(BossModule module) : BossComponent(module)
{
    private static readonly Polygon[] circle = [new Polygon(new(11f, -490f), 6.5f, 20, 8.5f.Degrees())];
    public static readonly ArenaBoundsComplex CurtaincallArena = new(D053Amon.union, [.. D053Amon.difference, .. circle]);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x05)
        {
            if (state == 0x00020001)
                Arena.Bounds = CurtaincallArena;
            else if (state == 0x00080004)
                Arena.Bounds = D053Amon.arena;
        }
    }
}

class Epode(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Epode), new AOEShapeRect(70f, 6f));
class EruptionForte(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.EruptionForte), 8f);

abstract class FiragaForte(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(40f, 10f));
class LeftFiragaForte(BossModule module) : FiragaForte(module, AID.LeftFiragaForte);
class RightFiragaForte(BossModule module) : FiragaForte(module, AID.RightFiragaForte);

class ThundagaForteProximity(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ThundagaForteProximity), 15f);
class DarkForte(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.DarkForte));
class Entracte(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Entracte));
class DreamsOfIce(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DreamsOfIce), 6f);

class CurtainCall(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.CurtainCall), 60f)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies((uint)OID.Ice);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return Arena.Bounds == CurtainCallArenaChange.CurtaincallArena ? Safezones : [];
    }
}

class ThundagaForteCone(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(20f, 22.5f.Degrees());
    private readonly List<AOEInstance> _aoes = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 4 ? 4 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
            aoes[i] = _aoes[i];
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ThundagaForteCone1 or (uint)AID.ThundagaForteCone2)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 8)
                _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.ThundagaForteCone1 or (uint)AID.ThundagaForteCone2)
            _aoes.RemoveAt(0);
    }
}

class D053AmonStates : StateMachineBuilder
{
    public D053AmonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CurtainCall>()
            .ActivateOnEnter<CurtainCallArenaChange>()
            .ActivateOnEnter<DreamsOfIce>()
            .ActivateOnEnter<Epode>()
            .ActivateOnEnter<EruptionForte>()
            .ActivateOnEnter<LeftFiragaForte>()
            .ActivateOnEnter<RightFiragaForte>()
            .ActivateOnEnter<ThundagaForteProximity>()
            .ActivateOnEnter<ThundagaForteCone>()
            .ActivateOnEnter<DarkForte>()
            .ActivateOnEnter<Entracte>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 786, NameID = 10293)]
public class D053Amon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly Polygon[] union = [new Polygon(new(11f, -490f), 19.5f, 48)];
    public static readonly Rectangle[] difference = [new(new(11f, -469.521f), 20, 1.25f)];
    public static readonly ArenaBoundsComplex arena = new(union, difference);
}
