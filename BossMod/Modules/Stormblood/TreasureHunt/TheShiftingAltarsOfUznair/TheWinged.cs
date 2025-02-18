namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.TheWinged;

public enum OID : uint
{
    Boss = 0x253D, //R=3.36
    FeatherOfTheWinged = 0x253E, //R=0.5
    AltarQueen = 0x254A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    AltarMatanga = 0x2545, // R3.42
    GoldWhisker = 0x2544, // R0.54
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // BonusAdds->player, no cast, single-target

    Filoplumes = 13376, // Boss->self, 3.0s cast, range 8+R width 4 rect
    Wingbeat = 13377, // Boss->self, 3.0s cast, range 40+R 60-degree cone, knockback 20 away from source
    FeatherSquallVisual = 13378, // Boss->self, 3.0s cast, single-target
    FeatherSquall = 13379, // BossHelper->location, 3.0s cast, range 6 circle
    Sideslip = 13380, // Boss->self, 3.5s cast, range 50+R circle
    Pinion = 13381, // Featherofthewinged->self, 3.0s cast, range 40+R width 3 rect

    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // AltarMatanga/Mandragoras/GoldWhisker->self, no cast, single-target, bonus adds disappear
}

class Filoplumes(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Filoplumes), new AOEShapeRect(11.36f, 2));
class Wingbeat(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Wingbeat), new AOEShapeCone(43.36f, 30.Degrees()));
class WingbeatKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Wingbeat), 20, false, 1, new AOEShapeCone(43.36f, 30.Degrees()), stopAtWall: true);
class FeatherSquall(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FeatherSquall), 6);
class Pinion(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Pinion), new AOEShapeRect(40.5f, 1.5f));
class Sideslip(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Sideslip));

abstract class Mandragoras(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 6.84f);
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 60.Degrees()));
class Hurl(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), [(uint)OID.AltarMatanga]);

class TheWingedStates : StateMachineBuilder
{
    public TheWingedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Filoplumes>()
            .ActivateOnEnter<Wingbeat>()
            .ActivateOnEnter<WingbeatKB>()
            .ActivateOnEnter<FeatherSquall>()
            .ActivateOnEnter<Sideslip>()
            .ActivateOnEnter<Pinion>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(TheWinged.All);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7595)]
public class TheWinged(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.AltarEgg, (uint)OID.AltarGarlic, (uint)OID.AltarOnion, (uint)OID.AltarTomato,
    (uint)OID.AltarQueen, (uint)OID.GoldWhisker, (uint)OID.AltarMatanga];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.AltarOnion => 6,
                (uint)OID.AltarEgg => 5,
                (uint)OID.AltarGarlic => 4,
                (uint)OID.AltarTomato => 3,
                (uint)OID.AltarQueen or (uint)OID.GoldWhisker => 2,
                (uint)OID.AltarMatanga => 1,
                _ => 0
            };
        }
    }
}
