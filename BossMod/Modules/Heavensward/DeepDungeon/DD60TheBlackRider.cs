namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD60TheBlackRider;

public enum OID : uint
{
    Boss = 0x1814, // R3.92
    Voidzone = 0x1E858E, // R0.5
    VoidsentDiscarnate = 0x18E6 // R1.0
}

public enum AID : uint
{
    AutoAttack = 7179, // Boss->player, no cast, range 8+R 90-degree cone

    Geirrothr = 7087, // Boss->self, no cast, range 6+R 90-degree cone, 5.1s after pull, 7.1s after Valfodr + 8.1s after every 2nd HallofSorrow
    HallOfSorrow = 7088, // Boss->location, no cast, range 9 circle
    Infatuation = 7157, // VoidsentDiscarnate->self, 6.5s cast, range 6+R circle
    Valfodr = 7089 // Boss->player, 4.0s cast, width 6 rect charge, knockback 25, dir forward
}

class CleaveAuto(BossModule module) : Components.Cleave(module, default, new AOEShapeCone(11.92f, 45f.Degrees()), activeWhileCasting: false);
class Infatuation(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Infatuation), 7f);
class HallOfSorrow(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 9f, ActionID.MakeSpell(AID.HallOfSorrow), GetVoidzones, 1.3f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Voidzone);
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
class Valfodr(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.Valfodr), 3);
class ValfodrKB(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.Valfodr), stopAtWall: true) // note actual knockback is delayed by upto 1.2s in replay
{
    private readonly Infatuation _aoe = module.FindComponent<Infatuation>()!;
    private int _target;
    private Source? _source;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_target == slot && _source != null)
            return [_source.Value];
        else
            return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _source = new(spell.LocXZ, 25f, Module.CastFinishAt(spell));
            _target = Raid.FindSlot(spell.TargetID);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _target = -1;
            _source = null;
        }
    }

    private Func<WPos, float>? GetFireballZone()
    {
        var count = _aoe.Casters.Count;
        if (count == 0)
            return null;
        var forbidden = new Func<WPos, float>[count];
        for (var i = 0; i < count; ++i)
            forbidden[i] = ShapeDistance.Circle(_aoe.Casters[i].Origin, 7f);
        return ShapeDistance.Union(forbidden);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => GetFireballZone() is var z && z != null && z(pos) < 0f;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_target != slot || _source == null)
            return;

        var dangerZone = GetFireballZone();
        if (dangerZone == null)
            return;

        var kbSource = _source.Value.Origin;

        hints.AddForbiddenZone(p =>
        {
            var dir = (p - kbSource).Normalized();
            var proj = Arena.ClampToBounds(p + dir * 25f);
            return dangerZone(proj);
        }, _source.Value.Activation);
    }
}

class DD60TheBlackRiderStates : StateMachineBuilder
{
    public DD60TheBlackRiderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HallOfSorrow>()
            .ActivateOnEnter<Valfodr>()
            .ActivateOnEnter<ValfodrKB>()
            .ActivateOnEnter<CleaveAuto>()
            .ActivateOnEnter<Infatuation>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 204, NameID = 5309)]
public class DD60TheBlackRider(WorldState ws, Actor primary) : BossModule(ws, primary, SharedBounds.ArenaBounds607080.Center, SharedBounds.ArenaBounds607080);
