namespace BossMod.Shadowbringers.Foray.Duel.Duel6Lyon;

class OnFire(BossModule module) : BossComponent(module)
{
    private bool _hasBuff;
    private bool _isCasting;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_isCasting)
            hints.Add("Applies On Fire to Lyon. Use Dispell to remove it");
        else if (_hasBuff)
            hints.Add("Lyon has 'On Fire'. Use Dispell to remove it!");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor == Module.PrimaryActor && status.ID == (uint)SID.OnFire)
            _hasBuff = true;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HarnessFire)
            _isCasting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HarnessFire)
            _isCasting = false;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor == Module.PrimaryActor && status.ID == (uint)SID.OnFire)
            _hasBuff = false;
    }
}

class WildfiresFury(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WildfiresFury));

class HeavenAndEarth(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;

    private static readonly AOEShapeCone _shape = new(20f, 15f.Degrees());

    private int _index;

    private void UpdateIncrement(Angle increment)
    {
        _increment = increment;
        for (var i = 0; i < Sequences.Count; ++i)
        {
            Sequences[i] = Sequences[i] with { Increment = _increment };
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavenAndEarthCW)
            UpdateIncrement(-30.Degrees());
        else if (spell.Action.ID == (uint)AID.HeavenAndEarthCCW)
            UpdateIncrement(30.Degrees());
        else if (spell.Action.ID == (uint)AID.HeavenAndEarthStart)
            Sequences.Add(new(_shape, spell.LocXZ, spell.Rotation, _increment, Module.CastFinishAt(spell), 1.2f, 4));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HeavenAndEarthMove)
            AdvanceSequence(++_index % Sequences.Count, WorldState.CurrentTime);
    }
}

class HeartOfNatureConcentric(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NaturesPulse1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.NaturesPulse1 => 0,
                (uint)AID.NaturesPulse2 => 1,
                (uint)AID.NaturesPulse3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class CagedHeartOfNature(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CagedHeartOfNature), 6f);

class WindsPeak(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WindsPeak1), 5f);

class WindsPeakKB(BossModule module) : Components.Knockback(module)
{
    private DateTime _time;
    private bool _watched;
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_watched && WorldState.CurrentTime < _time.AddSeconds(4.4d))
            return [new(Module.PrimaryActor.Position, 15f, _activation)];
        else
            return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindsPeak1)
        {
            _watched = true;
            _time = WorldState.CurrentTime;
            _activation = Module.CastFinishAt(spell);
        }
    }
}

class SplittingRage(BossModule module) : Components.TemporaryMisdirection(module, ActionID.MakeSpell(AID.SplittingRage));

class NaturesBlood(BossModule module) : Components.Exaflare(module, 4f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NaturesBlood1)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 6f * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.1f, ExplosionsLeft = 7, MaxShownExplosions = 3 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NaturesBlood1 or (uint)AID.NaturesBlood2)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class MoveMountains(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MoveMountains3), new AOEShapeRect(40f, 3f))
{
    // TODO predict rotation
}

class WildfireCrucible(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.WildfireCrucible), "Enrage!", true);
