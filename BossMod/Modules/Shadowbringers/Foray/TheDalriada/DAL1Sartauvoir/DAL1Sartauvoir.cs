namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Sartauvoir;

class PyrokinesisAOE(BossModule module) : Components.RaidwideCast(module, (uint)AID.PyrokinesisAOE);

class Flamedive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flamedive, new AOEShapeRect(55f, 2.5f));
class BurningBlade(BossModule module) : Components.SingleTargetCast(module, (uint)AID.BurningBlade);

class MannatheihwonFlameRW(BossModule module) : Components.RaidwideCast(module, (uint)AID.MannatheihwonFlameRaidwide);
class MannatheihwonFlameRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MannatheihwonFlameRect, new AOEShapeRect(50f, 4f));
class MannatheihwonFlameCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MannatheihwonFlameCircle, 10f);

class Brand(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftBrand, (uint)AID.RightBrand], new AOEShapeCone(40f, 90f.Degrees()));

class Pyrocrisis(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Pyrocrisis, 6f);
class Pyrodoxy(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Pyrodoxy, 6f, 8);

class ThermalGustAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermalGustAOE, new AOEShapeRect(44f, 5f));
class GrandCrossflameAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandCrossflameAOE, new AOEShapeCross(40f, 9f));
class TimeEruption(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ReverseTimeEruption1, (uint)AID.ReverseTimeEruption2,
(uint)AID.TimeEruption1, (uint)AID.TimeEruption2], new AOEShapeRect(20f, 10f), 2, 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.TheDaldriada, GroupID = 778, NameID = 9384, SortOrder = 2)]
public class DAL1Sartauvoir(WorldState ws, Actor primary) : BossModule(ws, primary, new(631f, 157f), new ArenaBoundsSquare(19f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.BossP2));
        Arena.Actor(PrimaryActor);
    }
}
