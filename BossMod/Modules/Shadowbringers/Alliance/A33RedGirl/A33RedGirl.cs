namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class CrueltyP1(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.CrueltyVisualP1, (uint)AID.Cruelty, 0.1d);
sealed class CrueltyP2(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.CrueltyVisualP2, (uint)AID.Cruelty, 0.1d);
sealed class SublimeTranscendence(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.SublimeTranscendenceVisual, (uint)AID.SublimeTranscendence, 0.1d);
sealed class ShockWhiteBlack(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ShockWhiteAOE, (uint)AID.ShockBlackAOE], 5f);

sealed class GenerateBarrier1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrierVisual1, new AOEShapeRect(18f, 1.5f));
sealed class GenerateBarrier2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrierVisual2, new AOEShapeRect(24f, 1.5f));
sealed class GenerateBarrier3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrierVisual3, new AOEShapeRect(12f, 1.5f));
sealed class GenerateBarrier4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrierVisual4, new AOEShapeRect(6f, 1.5f));

sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 9f, riskyWithSecondsLeft: 5d);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9920)]
public sealed class A33RedGirl(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingArena)
{
    public static readonly WPos ArenaCenter = new(845f, -851f);
    public static readonly ArenaBoundsSquare StartingArena = new(24.5f);
    public static readonly PolygonCustom InnerSquare = new([new(842.49945f, -848.50018f), new(842.5f, -853.5f), new(847.5f, -853.5f), new(847.5f, -848.5f)]); // one vertice of the inner square is slightly misplaced. since it kills instantly we prefer perfection.
    public static readonly AOEShapeCustom ArenaTransition = new([new Square(ArenaCenter, 25f)], [new Square(ArenaCenter, 20f)], [InnerSquare]);
    public static readonly ArenaBoundsComplex DefaultArena = new([new Square(ArenaCenter, 20f)], [InnerSquare]);
    public static readonly PolygonCustomO[] VirusArena1 = [new([new(6f, 856f), new(-6f, 856f), new(-6f, 868f), new(-1.5f, 868f), new(-1.5f, 880f),
    new(-8f, 880f), new(-8f, 882f), new(-12f, 882f), new(-12f, 884f), new(-14f, 884f),
    new(-14f, 886f), new(-16f, 886f), new(-16f, 888f), new(-18f, 888f), new(-18f, 892f),
    new(-20f, 892f), new(-20f, 908f), new(-18f, 908f), new(-18f, 912f), new(-16f, 912f),
    new(-16f, 914f), new(-14f, 914f), new(-14f, 916f), new(-12f, 916f), new(-12f, 918f),
    new(-8f, 918f), new(-8f, 920f), new(-1.5f, 920f), new(-1.5f, 932f), new(-6f, 932f),
    new(-6f, 944f), new(6f, 944f), new(6f, 932f), new(1.5f, 932f), new(1.5f, 920f),
    new(8f, 920f), new(8f, 918f), new(12f, 918f), new(12f, 916f), new(14f, 916f),
    new(14f, 914f), new(16f, 914f), new(16f, 912f), new(18f, 912f), new(18f, 908f),
    new(20f, 908f), new(20f, 892f), new(18f, 892f), new(18f, 888f), new(16f, 888f),
    new(16f, 886f), new(14f, 886f), new(14f, 884f), new(12f, 884f), new(12f, 882f),
    new(8f, 882f), new(8f, 880f), new(1.5f, 880f), new(1.5f, 868f), new(6f, 868f)], -0.5f)];
    public static readonly PolygonCustomO[] VirusArena2 = GenerateVirusArena(new(default, -412f));
    public static readonly PolygonCustomO[] VirusArena3 = GenerateVirusArena(new(default, -912f));
    private static PolygonCustomO[] GenerateVirusArena(WDir offset)
    {
        var vertices = new WPos[60];
        var o = offset;
        var vertices1 = VirusArena1[0].Vertices;
        for (var i = 0; i < 60; ++i)
        {
            vertices[i] = vertices1[i] + o;
        }
        return [new(vertices, -0.5f)];
    }
    // wall sizes 1.625, 1

    public Actor? BossP2;
    public Actor? RedSphere;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (RedSphere == null)
        {
            var b = Enemies((uint)OID.RedSphere);
            RedSphere = b.Count != 0 ? b[0] : null;
        }
        if (StateMachine.ActivePhaseIndex >= 1 && BossP2 == null)
        {
            var b = Enemies((uint)OID.BossP2);
            BossP2 = b.Count != 0 ? b[0] : null;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case -1:
            case 0:
                Arena.Actor(PrimaryActor);
                break;
            case 1:
                Arena.Actor(RedSphere);
                Arena.Actors(Enemies((uint)OID.BlackPylon));
                Arena.Actors(Enemies((uint)OID.WhitePylon));
                Arena.Actors(Enemies((uint)OID.BlackWall), Colors.Object);
                Arena.Actors(Enemies((uint)OID.WhiteWall), Colors.Object);
                break;
            case 2:
                Arena.Actor(BossP2);
                break;
        }
    }
}
