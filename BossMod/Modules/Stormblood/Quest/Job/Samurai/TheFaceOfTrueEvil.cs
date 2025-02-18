namespace BossMod.Stormblood.Quest.Job.Samurai.TheFaceOfTrueEvil;

public enum OID : uint
{
    Boss = 0x1BEE,
    Musosai = 0x1BF0, // R1.0
    ViolentWind = 0x1BF1, // R1.0
    Helper2 = 0x1BEF,
    Helper = 0x233C
}

public enum AID : uint
{
    HissatsuTo = 8415, // 1BEF->self, 3.0s cast, range 44+R width 4 rect
    HissatsuKyuten = 8412, // Boss->self, 3.0s cast, range 5+R circle
    ArashiVisual = 8418, // Boss->self, 4.0s cast, single-target
    Arashi = 8419, // 1BF0->self, no cast, range 4 circle
    HissatsuKiku1 = 8417, // Musosai->self, 4.0s cast, range 44+R width 4 rect
    Maiogi = 8421, // Musosai->self, 4.0s cast, range 80+R ?-degree cone
    Musojin = 8422, // Boss->self, 25.0s cast, single-target
    ArashiNoKiku = 8643, // Boss->self, 3.0s cast, single-target
    ArashiNoMaiogi = 8642, // Boss->self, 3.0s cast, single-target
}

class Musojin(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Musojin));

abstract class Hissatsu(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(44.5f, 2));
class HissatsuKiku(BossModule module) : Hissatsu(module, AID.HissatsuKiku1);
class HissatsuTo(BossModule module) : Hissatsu(module, AID.HissatsuTo);

class Maiogi(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Maiogi), new AOEShapeCone(80, 25.Degrees()));
class HissatsuKyuten(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HissatsuKyuten), 5.5f);
class Arashi(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime? Activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Activation == null)
            yield break;

        foreach (var e in Module.Enemies(OID.Musosai))
            yield return new(new AOEShapeCircle(4), e.Position, default, Activation.Value);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Arashi or AID.ArashiNoKiku or AID.ArashiNoMaiogi)
            Activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Arashi)
            Activation = null;
    }
}
class ViolentWind(BossModule module) : Components.Adds(module, (uint)OID.ViolentWind);

class MusosaiStates : StateMachineBuilder
{
    public MusosaiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HissatsuTo>()
            .ActivateOnEnter<HissatsuKyuten>()
            .ActivateOnEnter<Arashi>()
            .ActivateOnEnter<HissatsuKiku>()
            .ActivateOnEnter<Maiogi>()
            .ActivateOnEnter<Musojin>()
            .ActivateOnEnter<ViolentWind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68101, NameID = 6111)]
public class Musosai(WorldState ws, Actor primary) : BossModule(ws, primary, new(-217.27f, -158.31f), new ArenaBoundsSquare(15));
