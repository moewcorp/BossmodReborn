namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D110AlbusGriffin;

public enum OID : uint
{
    Boss = 0x3D56, //R=3.96
    Caladrius = 0x3CE2, //R=1.8
    AlbusGriffin = 0x3E9F, //R=4.6
}

public enum AID : uint
{
    AutoAttack1 = 872, // Caladrius/Boss->player, no cast, single-target
    AutoAttack2 = 870, // AlbusGriffin->player, no cast, single-target

    TransonicBlast = 32535, // Caladrius->self, 4.0s cast, range 9 90-degree cone
    WindsOfWinter = 32785, // AlbusGriffin->self, 5.0s cast, range 40 circle
    Freefall = 32786, // AlbusGriffin->location, 3.5s cast, range 8 circle
    GoldenTalons = 32787, // AlbusGriffin->self, 4.5s cast, range 8 90-degree cone
}

class TransonicBlast(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TransonicBlast), new AOEShapeCone(9, 45.Degrees()));
class WindsOfWinter(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WindsOfWinter));
class WindsOfWinterStunHint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.WindsOfWinter), false, true);
class GoldenTalons(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GoldenTalons), new AOEShapeCone(8, 45.Degrees()));
class Freefall(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Freefall), 8);

class D110AlbusGriffinStates : StateMachineBuilder
{
    public D110AlbusGriffinStates(D110AlbusGriffin module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TransonicBlast>()
            .Raw.Update = () => module.Enemies(D110AlbusGriffin.TrashP1).All(e => e.IsDeadOrDestroyed);
        TrivialPhase(1)
            .ActivateOnEnter<Freefall>()
            .ActivateOnEnter<WindsOfWinter>()
            .ActivateOnEnter<WindsOfWinterStunHint>()
            .ActivateOnEnter<GoldenTalons>()
            .Raw.Update = () => module.Enemies(D110AlbusGriffin.TrashP1).All(e => e.IsDestroyed) && module.Enemies(OID.AlbusGriffin).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 896, NameID = 12245)]
public class D110AlbusGriffin(WorldState ws, Actor primary) : BossModule(ws, primary, new(47, -570.5f), new ArenaBoundsRect(8.5f, 11.5f))
{
    public static readonly uint[] TrashP1 = [(uint)OID.Boss, (uint)OID.Caladrius];
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.Caladrius));
        Arena.Actors(Enemies(OID.AlbusGriffin));
    }
}
