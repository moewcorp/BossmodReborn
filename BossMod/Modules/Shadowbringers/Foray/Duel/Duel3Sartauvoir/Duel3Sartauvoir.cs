namespace BossMod.Shadowbringers.Foray.Duel.Duel3Sartauvoir;

class Pyrolatry(BossModule module) : Components.RaidwideCast(module, (uint)AID.Pyrolatry);
class Flashover(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flashover, 19f);
class FlamingRain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlamingRain, 6f);
class PillarOfFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarOfFlame, 8f);
class TimeEruption(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TimeEruption1, (uint)AID.TimeEruption2], new AOEShapeRect(20f, 10f), 2, 4);
class ThermalGust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermalGust, new AOEShapeRect(44f, 5f));
class ThermalWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermalWaveVisual3, new AOEShapeCone(60f, 45f.Degrees()));
class SearingWind(BossModule module) : Components.Voidzone(module, 3f, GetSearingWind)
{
    private static List<Actor> GetSearingWind(BossModule module) => module.Enemies((uint)OID.Peri);
}

class Backdraft(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Backdraft, 16f)
{
    private static readonly Angle a45 = 45f.Degrees(), a90 = 90f.Degrees(), a225 = 22.5f.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            var act = Module.CastFinishAt(source.CastInfo);
            if (!IsImmune(slot, act))
            {
                var forbidden = new Func<WPos, float>[4];
                for (var i = 0; i < 4; ++i)
                    forbidden[i] = ShapeDistance.InvertedCone(source.Position, 5f, a45 + a90 * i, a225);
                hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), act);
            }
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 735, NameID = 12)]
public class Duel3Sartauvoir(WorldState ws, Actor primary) : BossModule(ws, primary, new(-15, 145), new ArenaBoundsSquare(19f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InSquare(Arena.Center, 20f);
}