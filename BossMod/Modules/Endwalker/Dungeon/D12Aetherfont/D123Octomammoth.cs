namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D123Octomammoth;

public enum OID : uint
{
    Boss = 0x3EAA, // R=26.0
    MammothTentacle = 0x3EAB, // R=6.0
    Crystals = 0x3EAC, // R=0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 33357, // Boss->player, no cast, single-target

    Breathstroke = 34551, // Boss->self, 16.5s cast, range 35 180-degree cone
    Clearout = 33348, // MammothTentacle->self, 9.0s cast, range 16 120-degree cone
    Octostroke = 33347, // Boss->self, 16.0s cast, single-target
    SalineSpit1 = 33352, // Boss->self, 3.0s cast, single-target
    SalineSpit2 = 33353, // Helper->self, 6.0s cast, range 8 circle
    Telekinesis1 = 33349, // Boss->self, 5.0s cast, single-target
    Telekinesis2 = 33351, // Helper->self, 10.0s cast, range 12 circle
    TidalBreath = 33354, // Boss->self, 10.0s cast, range 35 180-degree cone
    TidalRoar = 33356, // Boss->self, 5.0s cast, range 60 circle
    VividEyes = 33355, // Boss->self, 4.0s cast, range 20-26 donut
    WaterDrop = 34436, // Helper->player, 5.0s cast, range 6 circle
    WallopVisual = 33350, // Boss->self, no cast, single-target, visual, starts tentacle wallops
    Wallop = 33346 // MammothTentacle->self, 3.0s cast, range 22 width 8 rect
}

class Wallop(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Wallop), new AOEShapeRect(22, 4));
class VividEyes(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.VividEyes), new AOEShapeDonut(20, 26));
class Clearout(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Clearout), new AOEShapeCone(16, 60.Degrees()));

abstract class Breath(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(35, 90.Degrees()));
class TidalBreath(BossModule module) : Breath(module, AID.TidalBreath);
class Breathstroke(BossModule module) : Breath(module, AID.Breathstroke);

class TidalRoar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TidalRoar));
class WaterDrop(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.WaterDrop), 6);
class SalineSpit(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SalineSpit2), 8);
class Telekinesis(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Telekinesis2), 12);

class D123OctomammothStates : StateMachineBuilder
{
    public D123OctomammothStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<Clearout>()
            .ActivateOnEnter<VividEyes>()
            .ActivateOnEnter<WaterDrop>()
            .ActivateOnEnter<TidalRoar>()
            .ActivateOnEnter<TidalBreath>()
            .ActivateOnEnter<Telekinesis>()
            .ActivateOnEnter<Breathstroke>()
            .ActivateOnEnter<SalineSpit>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12334, SortOrder = 8)]
public class D123Octomammoth(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-344.93f, -375.58f), new(-342.87f, -375.25f), new(-342.4f, -375.08f), new(-340.55f, -374.13f), new(-339.05f, -372.64f),
    new(-338.74f, -372.23f), new(-337.78f, -370.35f), new(-337.46f, -368.29f), new(-337.45f, -367.76f), new(-337.77f, -365.71f),
    new(-338.71f, -363.82f), new(-339, -363.4f), new(-340.46f, -361.94f), new(-340.89f, -361.67f), new(-342.34f, -360.94f),
    new(-342.86f, -360.75f), new(-345.01f, -360.39f), new(-346.55f, -358.32f), new(-346.94f, -355.76f), new(-346.69f, -355.3f),
    new(-345.77f, -354.04f), new(-345.5f, -353.58f), new(-344.86f, -351.61f), new(-344.83f, -349.13f), new(-345.48f, -347.15f),
    new(-345.71f, -346.67f), new(-346.93f, -344.99f), new(-348.61f, -343.75f), new(-349.07f, -343.49f), new(-351.11f, -342.83f),
    new(-353.52f, -342.83f), new(-355.52f, -343.48f), new(-356, -343.74f), new(-357.73f, -344.96f), new(-360.25f, -344.59f),
    new(-362.35f, -343.03f), new(-362.5f, -342.52f), new(-362.74f, -341.01f), new(-362.88f, -340.49f), new(-363.81f, -338.67f),
    new(-365.37f, -337.04f), new(-365.8f, -336.73f), new(-367.71f, -335.78f), new(-369.78f, -335.45f), new(-370.33f, -335.47f),
    new(-372.42f, -335.82f), new(-374.3f, -336.78f), new(-374.7f, -337.1f), new(-376.16f, -338.58f), new(-377.12f, -340.48f),
    new(-377.27f, -340.97f), new(-377.61f, -343.02f), new(-379.7f, -344.58f), new(-382.29f, -344.93f), new(-382.74f, -344.66f),
    new(-383.97f, -343.76f), new(-384.42f, -343.5f), new(-386.39f, -342.86f), new(-388.86f, -342.83f), new(-390.8f, -343.45f),
    new(-391.27f, -343.67f), new(-392.94f, -344.88f), new(-393.29f, -345.3f), new(-394.47f, -346.99f), new(-395.11f, -348.96f),
    new(-395.17f, -351.51f), new(-394.54f, -353.44f), new(-394.33f, -353.92f), new(-393.09f, -355.64f), new(-393.11f, -356.15f),
    new(-393.4f, -358.18f), new(-393.66f, -358.61f), new(-394.93f, -360.32f), new(-395.41f, -360.48f), new(-397.42f, -360.83f),
    new(-399.3f, -361.79f), new(-399.7f, -362.1f), new(-401.15f, -363.56f), new(-402.1f, -365.42f), new(-402.26f, -365.93f),
    new(-402.59f, -367.98f), new(-402.26f, -370.03f), new(-402.12f, -370.56f), new(-401.18f, -372.39f), new(-399.65f, -373.94f),
    new(-399.23f, -374.26f), new(-397.4f, -375.19f), new(-395.36f, -375.53f), new(-394.84f, -375.56f), new(-392.8f, -375.24f),
    new(-392.34f, -375.05f), new(-390.46f, -374.06f), new(-388.92f, -372.52f), new(-387.96f, -370.68f), new(-387.78f, -370.21f),
    new(-387.45f, -368.15f), new(-387.48f, -367.61f), new(-387.85f, -365.54f), new(-388.8f, -363.68f), new(-389.16f, -363.24f),
    new(-390.67f, -361.74f), new(-390.71f, -361.22f), new(-390.49f, -359.67f), new(-390.23f, -359.23f), new(-389.29f, -357.96f),
    new(-388.75f, -357.8f), new(-386.55f, -357.81f), new(-384.58f, -357.2f), new(-384.1f, -356.99f), new(-382.43f, -355.78f),
    new(-382.09f, -355.39f), new(-380.9f, -353.72f), new(-380.24f, -351.7f), new(-380.19f, -351.17f), new(-380.2f, -349.09f),
    new(-379.94f, -348.64f), new(-378.69f, -347.71f), new(-378.23f, -347.5f), new(-376.61f, -347.27f), new(-376.14f, -347.44f),
    new(-374.66f, -348.94f), new(-374.25f, -349.25f), new(-372.33f, -350.22f), new(-370.24f, -350.55f), new(-369.74f, -350.55f),
    new(-367.66f, -350.21f), new(-365.85f, -349.29f), new(-365.39f, -348.99f), new(-363.91f, -347.48f), new(-363.44f, -347.26f),
    new(-361.92f, -347.48f), new(-361.39f, -347.65f), new(-360.1f, -348.61f), new(-359.8f, -349.03f), new(-359.81f, -351.22f),
    new(-359.74f, -351.73f), new(-359.11f, -353.7f), new(-357.91f, -355.38f), new(-357.57f, -355.78f), new(-355.89f, -357),
    new(-355.4f, -357.2f), new(-353.9f, -357.69f), new(-351.08f, -357.82f), new(-350.66f, -358.09f), new(-349.69f, -359.33f),
    new(-349.5f, -359.82f), new(-349.3f, -361.44f), new(-349.5f, -361.9f), new(-350.95f, -363.35f), new(-351.25f, -363.76f),
    new(-352.19f, -365.61f), new(-352.53f, -367.63f), new(-352.57f, -368.13f), new(-352.23f, -370.27f), new(-351.31f, -372.13f),
    new(-351.04f, -372.56f), new(-349.49f, -374.11f), new(-347.62f, -375.08f), new(-347.12f, -375.25f), new(-345.11f, -375.57f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);

    protected override bool CheckPull() => InBounds(Raid.Player()!.Position);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if ((OID)e.Actor.OID == OID.Boss)
            {
                e.Priority = 1;
                break;
            }
        }
    }
}
