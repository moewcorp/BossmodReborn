namespace BossMod.Heavensward.Dungeon.D04TheVault.D041SerAdelphel;

public enum OID : uint
{
    Boss = 0x1051, // R2.2
    BrightSphere = 0x1052, // R1.0
    SerAdelphelBrightblade = 0x104E, // R0.5
    VaultDeacon = 0x1050, // R0.5
    VaultOstiary = 0x104F, // R0.5
    Helper = 0xD25
}

public enum AID : uint
{
    AutoAttack = 870, // VaultOstiary/Boss->player, no cast, single-target
    FastBlade = 9, // VaultOstiary->player, no cast, single-target
    FastBlade2 = 717, // SerAdelphelBrightblade->player, no cast, single-target
    Bloodstain = 1099, // SerAdelphelBrightblade->self, 2.5s cast, range 5 circle
    Fire = 966, // VaultDeacon->player, 1.0s cast, single-target

    AdventVisual1 = 4979,  // SerAdelphelBrightblade->self, no cast, single-target
    AdventVisual2 = 4122, // SerAdelphelBrightblade->self, no cast, single-target
    AdventVisual3 = 4123, // Boss->self, no cast, single-target
    Advent = 4980, // Helper->self, no cast, range 80 circle, knockback 18, away from source
    Retreat = 4257, // SerAdelphelBrightblade->self, no cast, single-target

    HoliestOfHoly = 4126, // Boss->self, 3.0s cast, range 80+R circle
    HeavenlySlash = 4125, // Boss->self, no cast, range 8+R 90-degree cone
    HolyShieldBash = 4127, // Boss->player, 4.0s cast, single-target
    SolidAscension1 = 4128, // Boss->player, no cast, single-target, on HolyShieldBash target
    SolidAscension2 = 4129, // Helper->player, no cast, single-target, on HolyShieldBash target
    ShiningBlade = 4130, // Boss->location, no cast, width 6 rect charge
    BrightFlare = 4132, // Brightsphere->self, no cast, range 5+R circle

    Execution = 4131, // Boss->location, no cast, range 5 circle

    Visual = 4121, // Boss->self, no cast, single-target
    BossPhase1Vanish = 4256, // SerAdelphelBrightblade->self, no cast, single-target
    BossPhase2Vanish = 4124 // Boss->self, no cast, single-target
}

public enum IconID : uint
{
    Stunmarker = 16, // player
    Spreadmarker = 32 // player
}

class Bloodstain(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Bloodstain), 5);
class HeavenlySlash(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.HeavenlySlash), new AOEShapeCone(10.2f, 45.Degrees()));
class HoliestOfHoly(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HoliestOfHoly));
class HolyShieldBash(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HolyShieldBash), "Stun + single target damage x2");

class BrightSphere(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        List<AOEInstance> aoes = new(count);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes.Add(count > 1 ? aoe with { Color = Colors.Danger } : aoe);
            else
                aoes.Add(aoe);
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BrightSphere)
            _aoes.Add(new(circle, actor.Position, default, WorldState.FutureTime(4.6f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID == AID.BrightFlare)
            _aoes.RemoveAt(0);
    }
}

class Execution(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5), (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.Execution), 4.8f, true);

class ShiningBlade(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly WPos west = new(-18.509f, -100.023f), south = new(-0.015f, -81.834f);
    private static readonly WPos north = new(-0.015f, -117.205f), east = new(18.387f, -100.053f);

    private const int HalfWidth = 3;
    private const float SubsequentActivationDelay = 2.2f;
    private static readonly Angle a90 = 90.Degrees(), a180 = 180.Degrees(), am90 = -90.Degrees(), a60 = 60.Degrees(), a0 = 0.Degrees();

    private readonly List<AOEInstance> _aoes = new(4);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        List<AOEInstance> aoes = new(count);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes.Add(count > 1 ? aoe with { Color = Colors.Danger } : aoe);
            else
                aoes.Add(aoe);
        }
        return aoes;
    }

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        if (id != 2523)
            return;

        var primary = Module.PrimaryActor.Position;
        var activationTimes = GetActivationTimes(WorldState.FutureTime(0.08f));

        if (primary.InCone(Arena.Center, a90, a60))
            AddAOEs(primary, west, south, north, east, activationTimes);
        else if (primary.InCone(Arena.Center, am90, a60))
            AddAOEs(primary, east, north, south, west, activationTimes);
        else if (primary.InCone(Arena.Center, a180, a60))
            AddAOEs(primary, south, east, west, north, activationTimes);
        else if (primary.InCone(Arena.Center, a0, a60))
            AddAOEs(primary, north, west, east, south, activationTimes);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID == AID.ShiningBlade)
            _aoes.RemoveAt(0);
    }

    private static List<DateTime> GetActivationTimes(DateTime activation)
    {
        return
        [
            activation,
            activation.AddSeconds(SubsequentActivationDelay),
            activation.AddSeconds(2 * SubsequentActivationDelay),
            activation.AddSeconds(3 * SubsequentActivationDelay)
        ];
    }

    private void AddAOEs(WPos primary, WPos first, WPos second, WPos third, WPos fourth, List<DateTime> activationTimes)
    {
        _aoes.Add(new(new AOEShapeRect((first - primary).Length(), HalfWidth), primary, Angle.FromDirection(first - primary), activationTimes[0]));
        _aoes.Add(new(new AOEShapeRect((second - first).Length(), HalfWidth), first, Angle.FromDirection(second - first), activationTimes[1]));
        _aoes.Add(new(new AOEShapeRect((third - second).Length(), HalfWidth), second, Angle.FromDirection(third - second), activationTimes[2]));
        _aoes.Add(new(new AOEShapeRect((fourth - third).Length(), HalfWidth), third, Angle.FromDirection(fourth - third), activationTimes[3]));
    }
}

class D041SerAdelphelStates : StateMachineBuilder
{
    public D041SerAdelphelStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeavenlySlash>()
            .ActivateOnEnter<Execution>()
            .ActivateOnEnter<Bloodstain>()
            .ActivateOnEnter<BrightSphere>()
            .ActivateOnEnter<HolyShieldBash>()
            .ActivateOnEnter<ShiningBlade>()
            .ActivateOnEnter<HoliestOfHoly>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS), Xyzzy", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3634)]
public class D041SerAdelphel(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -100), arena)
{
    protected override bool CheckPull() => PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.SerAdelphelBrightblade).Any(e => e.InCombat);

    public static readonly ArenaBounds arena = new ArenaBoundsComplex([new Circle(new(0, -100), 19.5f)], [new Rectangle(new(0, -120), 20, 1.75f), new Rectangle(new(-21, -100), 20, 1.75f, 90.Degrees())]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.SerAdelphelBrightblade));
        Arena.Actors(Enemies(OID.VaultDeacon));
        Arena.Actors(Enemies(OID.VaultOstiary));
    }
}
