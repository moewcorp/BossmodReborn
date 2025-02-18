namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class MousasScorn(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.MousasScorn), 4);

class HerossSundering(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.HerossSundering), new AOEShapeCone(40, 45.Degrees()))
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class HerossRadiance(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HerossRadiance));
class MagossRadiance(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MagossRadiance));
class RadiantHalo(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.RadiantHalo));
class CrystallineStoneIII(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.CrystallineStoneIII2), 6, 8, 8);
class CrystallineBlizzardIII(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.CrystallineBlizzardIII2), 5);
class Beacon(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Beacon), 3);
class Beacon2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Beacon2), new AOEShapeRect(45, 3), 10);
class HydaelynsRay(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HydaelynsRay), new AOEShapeRect(45, 15));

class T02HydaelynStates : StateMachineBuilder
{
    public T02HydaelynStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ParhelicCircle>()
            .ActivateOnEnter<MousasScorn>()
            .ActivateOnEnter<Echoes>()
            .ActivateOnEnter<Beacon>()
            .ActivateOnEnter<Beacon2>()
            .ActivateOnEnter<CrystallineStoneIII>()
            .ActivateOnEnter<CrystallineBlizzardIII>()
            .ActivateOnEnter<HerossSundering>()
            .ActivateOnEnter<HerossRadiance>()
            .ActivateOnEnter<MagossRadiance>()
            .ActivateOnEnter<HydaelynsRay>()
            .ActivateOnEnter<RadiantHalo>()
            .ActivateOnEnter<Lightwave>()
            .ActivateOnEnter<WeaponTracker>()
            .ActivateOnEnter<Exodus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 790, NameID = 10453)]
public class T02Hydaelyn(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, ArenaBounds)
{
    public static readonly WPos ArenaCenter = new(100, 100);
    public static readonly ArenaBoundsComplex ArenaBounds = new([new Polygon(ArenaCenter, 20, 48)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.CrystalOfLight), Colors.Object);
    }
}
