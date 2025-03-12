namespace BossMod.Stormblood.Quest.Job.Summoner.AnArtForTheLiving;

public enum OID : uint
{
    Boss = 0x1CBA,
    ExplosiveIndicator = 0x1CD7, // R0.5
    AetherochemicalExplosive = 0x1CD5, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    PiercingLaser = 8683, // Boss->self, 3.0s cast, range 30+R width 6 rect
    NerveGas = 8707, // 1CD8->self, 3.0s cast, range 30+R 120-degree cone
    NerveGasLeft = 8708, // FX1979->self, 3.0s cast, range 30+R 180-degree cone
    NerveGasRight = 8709, // 1CD8->self, 3.0s cast, range 30+R 180-degree cone
    W111TonzeSwing = 8697, // 1CD1->self, 4.0s cast, range 8+R circle
    W11TonzeSwipe = 8699 // 1CD1->self, 3.0s cast, range 5+R 120-degree cone
}

public enum SID : uint
{
    Invincibility = 325
}

class OneOneOneTonzeSwing(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.W111TonzeSwing), 12f);
class OneOneTonzeSwipe(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.W11TonzeSwipe), new AOEShapeCone(9f, 60f.Degrees()));

class NerveGas1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.NerveGas), new AOEShapeCone(35f, 60f.Degrees()));

abstract class NerveGasLR(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(35f, 90f.Degrees()));
class NerveGasRight(BossModule module) : NerveGasLR(module, AID.NerveGasRight);
class NerveGasLeft(BossModule module) : NerveGasLR(module, AID.NerveGasLeft);

class PiercingLaser(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PiercingLaser), new AOEShapeRect(33.68f, 3f));

class AetherochemicalExplosive(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Actor, bool Primed, DateTime Activation)> Explosives = [];
    private static readonly AOEShapeCircle circle = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Explosives.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var e = Explosives[i];
            if (!e.Actor.IsDead || !e.Primed)
            {
                aoes[i] = new(circle, e.Actor.Position, Activation: e.Activation);
            }
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ExplosiveIndicator)
        {
            Explosives.Add((actor, false, WorldState.CurrentTime.AddSeconds(3d)));
        }
        else if (actor.OID == (uint)OID.AetherochemicalExplosive)
        {
            var count = Explosives.Count;
            for (var i = 0; i < count; ++i)
            {
                var e = Explosives[i];
                if (e.Actor.Position.AlmostEqual(actor.Position, 1f))
                {
                    Explosives[i] = (actor, true, e.Activation);
                    return;
                }
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.AetherochemicalExplosive)
        {
            var count = Explosives.Count;
            for (var i = 0; i < count; ++i)
            {
                var e = Explosives[i];
                if (e.Actor.Position.AlmostEqual(actor.Position, 1f))
                {
                    Explosives.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class Adds(BossModule module) : Components.AddsMulti(module, [0x1CB6, 0x1CD1, 0x1CD6, 0x1CD8])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.FindStatus((uint)SID.Invincibility) == null ? 1 : AIHints.Enemy.PriorityInvincible;
        }
    }
}

class SummoningNodeStates : StateMachineBuilder
{
    public SummoningNodeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PiercingLaser>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<NerveGas1>()
            .ActivateOnEnter<NerveGasLeft>()
            .ActivateOnEnter<NerveGasRight>()
            .ActivateOnEnter<AetherochemicalExplosive>()
            .ActivateOnEnter<OneOneOneTonzeSwing>()
            .ActivateOnEnter<OneOneTonzeSwipe>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68165, NameID = 6695)]
public class SummoningNode(WorldState ws, Actor primary) : BossModule(ws, primary, new(-111, -295), ArenaBounds)
{
    private static readonly List<WDir> vertices = [
        new(-4.5f, 22.66f),
        new(4.5f, 22.66f),
        new(18f, 14.75f),
        new(22.2f, 7.4f),
        new(22.7f, 7.4f),
        new(22.7f, -7.4f),
        new(22.2f, -7.4f),
        new(18.15f, -15.77f),
        new(4.5f, -23.68f),
        new(-4.5f, -23.68f),
        new(-18.15f, -15.77f),
        new(-22.2f, -7.4f),
        new(-22.7f, -7.4f),
        new(-22.7f, 6.4f),
        new(-22.2f, 6.4f),
        new(-18f, 14.75f)
    ];

    public static readonly ArenaBoundsCustom ArenaBounds = new(30, new(vertices));
}
