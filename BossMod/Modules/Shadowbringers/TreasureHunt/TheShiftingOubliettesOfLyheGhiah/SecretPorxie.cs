namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretPorxie;

public enum OID : uint
{
    Boss = 0x3014, //R=1.2
    MagickedBroomHelper = 0x310A, // R=0.5
    MagickedBroom1 = 0x30F3, // R=3.125
    MagickedBroom2 = 0x30F2, // R=3.125
    MagickedBroom3 = 0x30F4, // R=3.125
    MagickedBroom4 = 0x30F1, // R=3.125
    MagickedBroom5 = 0x3015, // R=3.125
    MagickedBroom6 = 0x30F0, // R=3.125
    KeeperOfKeys = 0x3034, // R3.23
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/KeeperOfKeys/Mandragoras->player, no cast, single-target

    BrewingStorm = 21670, // Boss->self, 2.5s cast, range 5 60-degree cone, knockback 10 away from source
    HarrowingDream = 21671, // Boss->self, 3.0s cast, range 6 circle, applies sleep
    BecloudingDust = 22935, // Boss->self, 3.0s cast, single-target
    BecloudingDust2 = 22936, // BossHelper->location, 3.0s cast, range 6 circle
    SweepStart = 22937, // Brooms>self, 4.0s cast, range 6 circle
    SweepRest = 21672, // Brooms->self, no cast, range 6 circle
    SweepVisual = 22508, // Brooms->self, no cast, single-target, visual
    SweepVisual2 = 22509, // Brooms->self, no cast, single-target

    Telega = 9630, // KeeperOfKeys/Mandragoras->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768, // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
}

class BrewingStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BrewingStorm), new AOEShapeCone(5, 30.Degrees()));
class HarrowingDream(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HarrowingDream), new AOEShapeCircle(6));
class BecloudingDust(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BecloudingDust2), 6);

class Sweep(BossModule module) : Components.Exaflare(module, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SweepStart)
            Lines.Add(new() { Next = caster.Position, Advance = 12 * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell, 0.9f), TimeToMove = 4.5f, ExplosionsLeft = 4, MaxShownExplosions = 3 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Lines.Count > 0 && (AID)spell.Action.ID == AID.SweepRest)
        {
            var index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class Spin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(11));
class Mash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Mash), new AOEShapeRect(13, 2));
class Scoop(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Scoop), new AOEShapeCone(15, 60.Degrees()));

abstract class Mandragoras(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(6.84f));
class PluckAndPrune(BossModule module) : Mandragoras(module, AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, AID.Pollen);

class SecretPorxieStates : StateMachineBuilder
{
    public SecretPorxieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BrewingStorm>()
            .ActivateOnEnter<HarrowingDream>()
            .ActivateOnEnter<BecloudingDust>()
            .ActivateOnEnter<Sweep>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => Module.WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9795)]
public class SecretPorxie(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.SecretEgg).Concat(Enemies(OID.SecretTomato)).Concat(Enemies(OID.SecretQueen)).Concat(Enemies(OID.SecretGarlic)).Concat(Enemies(OID.SecretOnion))
        .Concat(Enemies(OID.KeeperOfKeys)), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.SecretOnion => 5,
                OID.SecretEgg => 4,
                OID.SecretGarlic => 3,
                OID.SecretTomato => 2,
                OID.SecretQueen or OID.KeeperOfKeys => 1,
                _ => 0
            };
        }
    }
}
