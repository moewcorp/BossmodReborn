namespace BossMod.Stormblood.Ultimate.UWU;

class P1Slipstream(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Slipstream), new AOEShapeCone(11.7f, 45.Degrees()));
class P1Downburst(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Downburst), new AOEShapeCone(11.7f, 45.Degrees()));
class P1EyeOfTheStorm(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.EyeOfTheStorm), new AOEShapeDonut(12, 25));
class P1Gigastorm(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Gigastorm), 6.5f);
class P2RadiantPlume(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RadiantPlumeAOE), 8);
class P2Incinerate(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Incinerate), new AOEShapeCone(15, 60.Degrees()), [(uint)OID.Ifrit]);
class P3RockBuster(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.RockBuster), new AOEShapeCone(10.55f, 60.Degrees()), [(uint)OID.Titan]); // TODO: verify angle
class P3MountainBuster(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.MountainBuster), new AOEShapeCone(15.55f, 45.Degrees()), [(uint)OID.Titan]); // TODO: verify angle
class P3WeightOfTheLand(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WeightOfTheLandAOE), 6);
class P3Upheaval(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Upheaval), 24, true);
class P3Tumult(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Tumult));
class P4Blight(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Blight));
class P4HomingLasers(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HomingLasers), 4);
class P4DiffractiveLaser(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.DiffractiveLaser), new AOEShapeCone(18, 45.Degrees()), [(uint)OID.UltimaWeapon]); // TODO: verify angle
class P5MistralSongCone(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MistralSongCone), new AOEShapeCone(21.7f, 75.Degrees()));

abstract class P5AetherochemicalLaser(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(46, 4));
class P5AetherochemicalLaserCenter(BossModule module) : P5AetherochemicalLaser(module, AID.AetherochemicalLaserCenter);
class P5AetherochemicalLaserRight(BossModule module) : P5AetherochemicalLaser(module, AID.AetherochemicalLaserRight);
class P5AetherochemicalLaserLeft(BossModule module) : P5AetherochemicalLaser(module, AID.AetherochemicalLaserLeft);

class P5LightPillar(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LightPillarAOE), 3); // TODO: consider showing circle around baiter
class P5AethericBoom(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.AethericBoom), 10);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Garuda, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 539, PlanLevel = 70)]
public class UWU : BossModule
{
    private readonly List<Actor> _titan;
    private readonly List<Actor> _lahabrea;
    private readonly List<Actor> _ultima;
    private Actor? _mainIfrit;

    public List<Actor> Ifrits { get; }

    public Actor? Garuda() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Ifrit() => _mainIfrit;
    public Actor? Titan() => _titan.FirstOrDefault();
    public Actor? Lahabrea() => _lahabrea.FirstOrDefault();
    public Actor? Ultima() => _ultima.FirstOrDefault();

    public UWU(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
    {
        Ifrits = Enemies(OID.Ifrit);
        _titan = Enemies(OID.Titan);
        _lahabrea = Enemies(OID.Lahabrea);
        _ultima = Enemies(OID.UltimaWeapon);
    }

    protected override void UpdateModule()
    {
        if (StateMachine.ActivePhaseIndex == 1)
            _mainIfrit ??= Ifrits.FirstOrDefault(a => a.IsTargetable);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(Garuda());
        Arena.Actor(Ifrit());
        Arena.Actor(Titan());
        Arena.Actor(Lahabrea());
        Arena.Actor(Ultima());
    }
}
