﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

// aoe starts at cast and ends with envcontrol; it's not considered 'risky' when paired with quick march
class FlamesOfBozja(BossModule module, bool risky) : Components.GenericAOEs(module, (uint)AID.FlamesOfBozjaAOE)
{
    public AOEInstance? AOE;
    private readonly bool _risky = risky;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            AOE = new(TrinityAvowed.ArenaChange2, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), Risky: _risky);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            AOE = null;
    }
}

class ShimmeringShot(BossModule module, float spawnToActivation) : TemperatureAOE(module)
{
    public enum Pattern { Unknown, EWNormal, EWInverted, WENormal, WEInverted }

    private readonly int[] _slotTempAdjustments = new int[5];
    private BitMask _arrowsInited;
    private Pattern _pattern;
    private readonly float _spawnToActivation = spawnToActivation;
    private DateTime _activation;

    private static readonly AOEShapeRect _shapeCell = new(5f, 5f, 5f);
    private static readonly int[,] _remap = { { 0, 1, 2, 3, 4 }, { 4, 2, 3, 0, 1 }, { 3, 4, 1, 2, 0 }, { 3, 4, 1, 2, 0 }, { 4, 2, 3, 0, 1 } };

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_arrowsInited.Raw != 0x1B)
            return [];
        var temp = Temperature(actor);
        var cell = Array.IndexOf(_slotTempAdjustments, -temp);
        if (cell < 0)
            return [];

        var xOffset = _pattern is Pattern.EWNormal or Pattern.EWInverted ? -20f : +20f;
        var zOffset = 10f * (cell - 2);
        return new AOEInstance[1] { new(_shapeCell, TrinityAvowed.ArenaCenter + new WDir(xOffset, zOffset), default, _activation, Colors.SafeFromAOE, false) };
    }

    public override void Update()
    {
        InitArrow((uint)OID.SparkArrow, +1);
        InitArrow((uint)OID.FlameArrow, +2);
        InitArrow((uint)OID.FrostArrow, -1);
        InitArrow((uint)OID.GlacierArrow, -2);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ChillArrow1 or (uint)AID.FreezingArrow1 or (uint)AID.HeatedArrow1 or (uint)AID.SearingArrow1
        or (uint)AID.ChillArrow2 or (uint)AID.FreezingArrow2 or (uint)AID.HeatedArrow2 or (uint)AID.SearingArrow2)
            ++NumCasts;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        var pattern = (index, state) switch
        {
            (0x16, 0x00200010u) => Pattern.EWNormal,
            (0x16, 0x00020001u) => Pattern.EWInverted,
            (0x17, 0x00200010u) => Pattern.WENormal,
            (0x17, 0x00020001u) => Pattern.WEInverted,
            _ => Pattern.Unknown
        };
        if (pattern != Pattern.Unknown)
            _pattern = pattern;
    }

    public bool ActorUnsafeAt(Actor actor, WPos pos)
    {
        var offset = pos - Arena.Center;
        var posInFlames = _pattern switch
        {
            Pattern.EWNormal or Pattern.EWInverted => offset.X > -15f,
            Pattern.WENormal or Pattern.WEInverted => offset.X < +15f,
            _ => false
        };
        if (posInFlames)
            return true;

        if (_arrowsInited.Raw != 0x1B)
            return false; // no arrows yet, any position is safe
        var row = RowIndex(pos);
        return _slotTempAdjustments[row] != -Temperature(actor);
    }

    protected int RowIndex(WPos pos) => (pos.Z - TrinityAvowed.ArenaCenter.Z) switch
    {
        < -15 => 0,
        < -5 => 1,
        < 5 => 2,
        < 15 => 3,
        _ => 4
    };

    private void InitArrow(uint oid, int temp)
    {
        if (_arrowsInited[temp + 2])
            return;
        var arrows = Module.Enemies(oid);
        var arrow = arrows.Count != 0 ? arrows[0] : null;
        if (arrow == null)
            return;

        if ((arrow.Position.X < Arena.Center.X) != _pattern is Pattern.WENormal or Pattern.WEInverted)
            ReportError("Unexpected arrow X coord");
        var srcRow = RowIndex(arrow.Position);
        var destRow = _remap[(int)_pattern, srcRow];
        _slotTempAdjustments[destRow] = temp;
        _arrowsInited.Set(temp + 2);
        _activation = WorldState.FutureTime(_spawnToActivation);
    }
}

sealed class FlamesOfBozja1(BossModule module) : FlamesOfBozja(module, false);

sealed class QuickMarchBow1(BossModule module) : QuickMarch(module)
{
    private readonly FlamesOfBozja1? _flames = module.FindComponent<FlamesOfBozja1>();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_flames?.AOE?.Shape.Check(pos, _flames.AOE.Value.Origin, _flames.AOE.Value.Rotation) ?? false);
}

sealed class ShimmeringShot1(BossModule module) : ShimmeringShot(module, 12.8f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActorUnsafeAt(actor, actor.Position))
            hints.Add("Go to safe zone!");
    }
}

sealed class FlamesOfBozja2(BossModule module) : FlamesOfBozja(module, true);

sealed class ShimmeringShot2(BossModule module) : ShimmeringShot(module, 14.0f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints) { } // no need for hints, quick march handles that
}

sealed class QuickMarchBow2(BossModule module) : QuickMarch(module)
{
    private readonly ShimmeringShot2? _shimmering = module.FindComponent<ShimmeringShot2>();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_shimmering?.ActorUnsafeAt(actor, pos) ?? false);
}
