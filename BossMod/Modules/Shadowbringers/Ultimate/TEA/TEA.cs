namespace BossMod.Shadowbringers.Ultimate.TEA;

class P1FluidSwing(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.FluidSwing), new AOEShapeCone(11.5f, 45f.Degrees()));
class P1FluidStrike(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.FluidStrike), new AOEShapeCone(11.6f, 45f.Degrees()), [(uint)OID.LiquidHand]);
class P1Sluice(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Sluice), 5);
class P1Splash(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Splash));
class P1Drainage(BossModule module) : Components.TankbusterTether(module, ActionID.MakeSpell(AID.DrainageP1), (uint)TetherID.Drainage, 6f);
class P2JKick(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.JKick));
class P2EyeOfTheChakram(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.EyeOfTheChakram), new AOEShapeRect(76f, 3f));
class P2HawkBlasterOpticalSight(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HawkBlasterP2), 10f);
class P2Photon(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.PhotonAOE));
class P2SpinCrusher(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SpinCrusher), new AOEShapeCone(10f, 45f.Degrees()));
class P2Drainage(BossModule module) : Components.PersistentVoidzone(module, 8f, GetVoidzones) // TODO: verify distance
{
    private static List<Actor> GetVoidzones(BossModule module) => module.Enemies((uint)OID.LiquidRage);
}

class P2PropellerWind(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.PropellerWind), 50)
{
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(Module.Enemies((uint)OID.GelidGaol));
}

class P2DoubleRocketPunch(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.DoubleRocketPunch), 3f);
class P3ChasteningHeat(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.ChasteningHeat), new AOEShapeCircle(5f), true);
class P3DivineSpear(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.DivineSpear), new AOEShapeCone(24.2f, 45f.Degrees()), [(uint)OID.AlexanderPrime]); // TODO: verify angle
class P3DivineJudgmentRaidwide(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.DivineJudgmentRaidwide));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 694, PlanLevel = 80)]
public class TEA : BossModule
{
    private readonly List<Actor> _liquidHand;
    public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? LiquidHand() => _liquidHand.Count != 0 ? _liquidHand[0] : null;

    private Actor? _bruteJustice;
    private Actor? _cruiseChaser;
    public Actor? BruteJustice() => _bruteJustice;
    public Actor? CruiseChaser() => _cruiseChaser;

    private Actor? _alexPrime;
    private readonly List<Actor> _trueHeart;
    public Actor? AlexPrime() => _alexPrime;
    public Actor? TrueHeart() => _trueHeart.Count != 0 ? _trueHeart[0] : null;

    private Actor? _perfectAlex;
    public Actor? PerfectAlex() => _perfectAlex;

    public TEA(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
    {
        _liquidHand = Enemies((uint)OID.LiquidHand);
        _trueHeart = Enemies((uint)OID.TrueHeart);
    }

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bruteJustice ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies((uint)OID.BruteJustice)[0] : null;
        _cruiseChaser ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies((uint)OID.CruiseChaser)[0] : null;
        _alexPrime ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies((uint)OID.AlexanderPrime)[0] : null;
        _perfectAlex ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies((uint)OID.PerfectAlexander)[0] : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case -1:
            case 0:
                Arena.Actor(BossP1());
                Arena.Actor(LiquidHand());
                break;
            case 1:
                Arena.Actor(_bruteJustice, allowDeadAndUntargetable: true);
                Arena.Actor(_cruiseChaser, allowDeadAndUntargetable: true);
                break;
            case 2:
                Arena.Actor(_alexPrime);
                Arena.Actor(TrueHeart());
                Arena.Actor(_bruteJustice);
                Arena.Actor(_cruiseChaser);
                break;
            case 3:
                Arena.Actor(_perfectAlex);
                break;
        }
    }
}
