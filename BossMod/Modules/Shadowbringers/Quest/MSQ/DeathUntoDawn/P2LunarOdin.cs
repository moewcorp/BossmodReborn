using BossMod.QuestBattle;
using RID = BossMod.Roleplay.AID;

namespace BossMod.Shadowbringers.Quest.MSQ.DeathUntoDawn.P2;

public enum OID : uint
{
    Boss = 0x3200,
    Fetters = 0x3218
}

public enum AID : uint
{
    LunarGungnir = 24025, // LunarOdin->31EC, 12.0s cast, range 6 circle
    LunarGungnir1 = 24026, // LunarOdin->2E2E, 25.0s cast, range 6 circle
    GungnirAOE = 24698, // 233C->self, 10.0s cast, range 10 circle
    Gagnrath = 24030, // 321C->self, 3.0s cast, range 50 width 4 rect
    GungnirSpread = 24029, // 321C->self, no cast, range 10 circle
    LeftZantetsuken = 24034, // LunarOdin->self, 4.0s cast, range 70 width 39 rect
    RightZantetsuken = 24032, // LunarOdin->self, 4.0s cast, range 70 width 39 rect
}

class UriangerAI(WorldState ws) : UnmanagedRotation(ws, 25)
{
    public const ushort StatusParam = 158;

    private float HeliosLeft(Actor p) => p.IsTargetable ? StatusDetails(p, 836, Player.InstanceID).Left : float.MaxValue;

    protected override void Exec(Actor? primaryTarget)
    {
        var partyPositions = World.Party.WithoutSlot(false, false).Select(p => p.Position).ToList();

        Hints.GoalZones.Add(pos => partyPositions.Count(p => p.InCircle(pos, 16)));

        if (World.Party.WithoutSlot(false, false).All(p => HeliosLeft(p) < 1 && p.Position.InCircle(Player.Position, 15.5f + p.HitboxRadius)))
            UseAction(RID.AspectedHelios, Player);

        if (World.Party.WithoutSlot(false, false).FirstOrDefault(p => p.HPMP.CurHP < p.HPMP.MaxHP * 0.4f) is Actor low)
            UseAction(RID.Benefic, low);

        UseAction(RID.MaleficIII, primaryTarget);

        if (Player.FindStatus(Roleplay.SID.DestinyDrawn) != null)
        {
            if (ComboAction == RID.DestinyDrawn)
                UseAction(RID.LordOfCrowns, primaryTarget, -100);

            if (ComboAction == RID.DestinysSleeve)
                UseAction(RID.TheScroll, Player, -100);
        }
        else
        {
            UseAction(RID.DestinyDrawn, Player, -100);
            UseAction(RID.DestinysSleeve, Player, -100);
        }

        UseAction(RID.FixedSign, Player, -150);
    }
}

class AutoUrianger(BossModule module) : RotationModule<UriangerAI>(module);
class Fetters(BossModule module) : Components.Adds(module, (uint)OID.Fetters);

class GunmetalSoul(BossModule module) : Components.GenericAOEs(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Module.Enemies(0x1EB1D5).Where(e => e.EventState != 7).Select(e => new AOEInstance(new AOEShapeDonut(4, 100), e.Position));
}
class LunarGungnir1(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.LunarGungnir), 6);
class LunarGungnir2(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.LunarGungnir1), 6);
class Gungnir(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GungnirAOE), 10);
class Gagnrath(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Gagnrath), new AOEShapeRect(50, 2));
class GungnirSpread(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(10), 189, ActionID.MakeSpell(AID.GungnirSpread), 5.3f, centerAtTarget: true);

abstract class Zantetsuken(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(70, 19.5f));
class RightZantetsuken(BossModule module) : Zantetsuken(module, AID.RightZantetsuken);
class LeftZantetsuken(BossModule module) : Zantetsuken(module, AID.LeftZantetsuken);

public class LunarOdinStates : StateMachineBuilder
{
    public LunarOdinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoUrianger>()
            .ActivateOnEnter<Fetters>()
            .ActivateOnEnter<Gungnir>()
            .ActivateOnEnter<Gagnrath>()
            .ActivateOnEnter<GungnirSpread>()
            .ActivateOnEnter<GunmetalSoul>()
            .ActivateOnEnter<LunarGungnir1>()
            .ActivateOnEnter<LunarGungnir2>()
            .ActivateOnEnter<LeftZantetsuken>()
            .ActivateOnEnter<RightZantetsuken>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69602, NameID = 10034)]
public class LunarOdin(WorldState ws, Actor primary) : BossModule(ws, primary, new(146.5f, 84.5f), new ArenaBoundsCircle(20));
