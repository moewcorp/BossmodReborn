namespace BossMod.Stormblood.Alliance.A31Mustadio;

class EnergyBurst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EnergyBurst));
class ArmShot(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ArmShot));
class LegShot(BossModule module) : Components.PersistentVoidzone(module, 3f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.LegShotVoidzone);
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

abstract class Handgonne(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(30f, 105f.Degrees()));
class LeftHandgonne(BossModule module) : Handgonne(module, AID.LeftHandgonne);
class RightHandgonne(BossModule module) : Handgonne(module, AID.RightHandgonne);

class SatelliteBeam(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SatelliteBeam), new AOEShapeRect(30, 15)); // Satellite Beam and Compress can both be shown earleir through tether
class Compress(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Compress), new AOEShapeRect(100, 7.5f));

class BallisticSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BallisticImpact1), 6);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7915)] // 7919 
public class A31Mustadio(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, 290), new ArenaBoundsSquare(30, 45.Degrees()));
