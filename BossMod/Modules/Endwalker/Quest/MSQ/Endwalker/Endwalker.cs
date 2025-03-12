namespace BossMod.Endwalker.Quest.MSQ.Endwalker;

class EndwalkerStates : StateMachineBuilder
{
    public EndwalkerStates(Endwalker module) : base(module)
    {
        DeathPhase(0, id => { SimpleState(id, 10000, "Enrage"); })
            .ActivateOnEnter<Megaflare>()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<Puddles>()
            .ActivateOnEnter<JudgementBolt>()
            .ActivateOnEnter<Hellfire>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<StarBeyondStars>()
            .ActivateOnEnter<TheEdgeUnbound>()
            .ActivateOnEnter<WyrmsTongue>()
            .ActivateOnEnter<NineNightsAvatar>()
            .ActivateOnEnter<NineNightsHelpers>()
            .ActivateOnEnter<VeilAsunder>()
            .ActivateOnEnter<Exaflare>()
            .ActivateOnEnter<DiamondDust>()
            .ActivateOnEnter<DeadGaze>()
            .ActivateOnEnter<MortalCoil>()
            .ActivateOnEnter<TidalWave2>();

        SimplePhase(1, id => { SimpleState(id, 10000, "Enrage"); }, "P2")
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<SilveredEdge>()
            .ActivateOnEnter<VeilAsunder>()
            .ActivateOnEnter<SwiftAsShadow>()
            .ActivateOnEnter<Candlewick>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<Extinguishment>()
            .ActivateOnEnter<WyrmsTongue>()
            .ActivateOnEnter<UnmovingDvenadkatik>()
            .ActivateOnEnter<TheEdgeUnbound2>()
            .Raw.Update = () => module.ZenosP2() is var ZenosP2 && ZenosP2 != null && !ZenosP2.IsTargetable && ZenosP2.HPMP.CurHP <= 1;
    }
}

class Megaflare(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Megaflare), 6f);
class Puddles(BossModule module) : Components.PersistentInvertibleVoidzoneByCast(module, 5f, GetVoidzones, ActionID.MakeSpell(AID.Hellfire))
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Puddles);
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

class JudgementBolt(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.JudgementBoltVisual));
class Hellfire(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HellfireVisual));
class StarBeyondStars(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.StarBeyondStarsHelper), new AOEShapeCone(50f, 15f.Degrees()), 6);
class TheEdgeUnbound(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TheEdgeUnbound), 10f);
class WyrmsTongue(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WyrmsTongueHelper), new AOEShapeCone(40f, 30f.Degrees()));

class NineNightsAvatar : Components.SimpleAOEs
{
    public NineNightsAvatar(BossModule module) : base(module, ActionID.MakeSpell(AID.NineNightsAvatar), 10f) { Color = Colors.Danger; }
}

class NineNightsHelpers : Components.SimpleAOEs
{
    public NineNightsHelpers(BossModule module) : base(module, ActionID.MakeSpell(AID.NineNightsHelpers), 10f, 6) { MaxDangerColor = 2; }
}
class VeilAsunder(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.VeilAsunderHelper), 6f);
class MortalCoil(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MortalCoilVisual), new AOEShapeDonut(8f, 20f));
class DiamondDust(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DiamondDustVisual), "Raidwide. Turns floor to ice.");
class DeadGaze(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.DeadGazeVisual));
class TidalWave2(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.TidalWaveVisual2), 25f, kind: Kind.DirForward, stopAtWall: true);
class SwiftAsShadow(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.SwiftAsShadow), 1f);
class Extinguishment(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ExtinguishmentVisual), new AOEShapeDonut(10f, 30f));
class TheEdgeUnbound2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TheEdgeUnbound2), 10f);

class UnmovingDvenadkatik : Components.SimpleAOEs
{
    public UnmovingDvenadkatik(BossModule module) : base(module, ActionID.MakeSpell(AID.UnmovingDvenadkatikVisual), new AOEShapeCone(50f, 15f.Degrees()), 6) { MaxDangerColor = 2; }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat, Malediktus", PrimaryActorOID = (uint)OID.ZenosP1, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70000, NameID = 10393)]
public class Endwalker : BossModule
{
    private readonly Actor? _zenosP2;

    public Actor? ZenosP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? ZenosP2() => _zenosP2;

    public Endwalker(WorldState ws, Actor primary) : base(ws, primary, new(100f, 100f), new ArenaBoundsSquare(19.5f))
    {
        _zenosP2 = Enemies(OID.ZenosP2)[0];
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case 0:
                Arena.Actor(ZenosP1());
                break;
            case 1:
                Arena.Actor(ZenosP2());
                break;
        }
    }
}
