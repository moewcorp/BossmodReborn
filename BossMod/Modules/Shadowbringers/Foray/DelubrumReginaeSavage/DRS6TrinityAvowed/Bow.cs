﻿namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

// aoe starts at cast and ends with envcontrol; it's not considered 'risky' when paired with quick march
class FlamesOfBozja(BossModule module, bool risky) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.FlamesOfBozjaAOE))
{
    public AOEInstance? AOE;
    private static readonly AOEShapeRect rect = new(45f, 25f);
    private readonly bool _risky = risky;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            AOE = new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), Risky: _risky);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is 0x12 or 0x13 && state == 0x00080004) // 12/13 for east/west
            AOE = null;
    }
}

// rows are 0 (Z=-102), 1 (Z=-92), 2 (Z=-82), 3 (Z=-72), 4 (Z=-62)
// what we've seen so far:
// ENVC 16.00020001; arrows spawn at X=-247 (E) aimed -90 (W); N to S we have Frost > None > Glacier > Flame > Spark, exploding N to S Spark > Glacier > Flame > Frost > None ==> 'inverted' pattern
// ENVC 16.00200010; arrows spawn at X=-247 (E) aimed -90 (W); N to S we have Spark > None > Frost > Glacier > Flame; exploding N to S Glacier > Flame > None > Frost > Spark ==> 'normal' pattern
// ENVC 17.00200010; arrows spawn at X=-297 (W) aimed +90 (E); N to S we have Glacier > Frost > Spark > Flame > None; exploding N to S None > Spark > Flame > Glacier > Frost ==> 'normal' pattern
// so, assumption: 16 is W->E, 17 is E->W; 00020001 is inverted, 00200010 is normal
class ShimmeringShot(BossModule module, float spawnToActivation) : TemperatureAOE(module)
{
    public enum Pattern { Unknown, EWNormal, EWInverted, WENormal, WEInverted }

    private readonly int[] _slotTempAdjustments = new int[5];
    private BitMask _arrowsInited;
    private Pattern _pattern;
    private readonly float _spawnToActivation = spawnToActivation;
    private DateTime _activation;

    private static readonly AOEShapeRect _shapeCell = new(5, 5, 5);
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
        return new AOEInstance[1] { new(_shapeCell, Arena.Center + new WDir(xOffset, zOffset), new(), _activation, Colors.SafeFromAOE, false) };
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
            (0x16, 0x00200010) => Pattern.EWNormal,
            (0x16, 0x00020001) => Pattern.EWInverted,
            (0x17, 0x00200010) => Pattern.WENormal,
            (0x17, 0x00020001) => Pattern.WEInverted,
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

    protected int RowIndex(WPos pos) => (pos.Z - Arena.Center.Z) switch
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

class FlamesOfBozja1(BossModule module) : FlamesOfBozja(module, false);

class QuickMarchBow1(BossModule module) : QuickMarch(module)
{
    private readonly FlamesOfBozja1? _flames = module.FindComponent<FlamesOfBozja1>();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_flames?.AOE?.Shape.Check(pos, _flames.AOE.Value.Origin, _flames.AOE.Value.Rotation) ?? false);
}

class ShimmeringShot1(BossModule module) : ShimmeringShot(module, 12.8f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActorUnsafeAt(actor, actor.Position))
            hints.Add("Go to safe zone!");
    }
}

class FlamesOfBozja2(BossModule module) : FlamesOfBozja(module, true);

class ShimmeringShot2(BossModule module) : ShimmeringShot(module, 14.0f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints) { } // no need for hints, quick march handles that
}

class QuickMarchBow2(BossModule module) : QuickMarch(module)
{
    private readonly ShimmeringShot2? _shimmering = module.FindComponent<ShimmeringShot2>();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_shimmering?.ActorUnsafeAt(actor, pos) ?? false);
}
