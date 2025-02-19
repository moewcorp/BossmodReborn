namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon;

class Enaero(BossModule module) : BossComponent(module)
{
    private bool EnaeroBuff;
    private bool casting;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (casting)
            hints.Add("Applies Enaero to Lyon. Use Dispell to remove it");
        if (EnaeroBuff)
            hints.Add("Enaero on Lyon. Use Dispell to remove it! You only need to do this once per duel, so you can switch to a different action after removing his buff.");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor == Module.PrimaryActor && status.ID == (uint)SID.Enaero)
            EnaeroBuff = true;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RagingWinds1)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RagingWinds1)
            casting = false;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor == Module.PrimaryActor && status.ID == (uint)SID.Enaero)
            EnaeroBuff = false;
    }
}

class HeartOfNatureConcentric(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

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

class TasteOfBlood(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TasteOfBlood), new AOEShapeCone(40f, 90f.Degrees()));
class TasteOfBloodHint(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.TasteOfBlood), "Go behind Lyon!");

class RavenousGale(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(1.5f);
    private readonly List<AOEInstance> _aoes = [];
    private bool casting;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (!casting && count == 0)
            return [];
        List<AOEInstance> aoes = new(count + 1);
        if (casting)
            aoes.Add(new(circle, actor.Position, default));
        aoes.AddRange(_aoes);
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.RavenousGaleVoidzone)
            _aoes.Add(new(circle, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(4.6d)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.RavenousGaleVoidzone)
            _aoes.RemoveAt(0);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RavenousGale)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RavenousGale)
            casting = false;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        base.AddGlobalHints(hints);
        if (casting)
            hints.Add("Move a little to avoid voidzone spawning under you");
    }
}

class TwinAgonies(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.TwinAgonies), "Heavy Tankbuster, use Manawall or tank mitigations");
class WindsPeak(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WindsPeak1), 5f);

class WindsPeakKB(BossModule module) : Components.Knockback(module)
{
    private DateTime Time;
    private bool watched;
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (watched && WorldState.CurrentTime < Time.AddSeconds(4.4d))
            return [new(Module.PrimaryActor.Position, 15f, _activation)];
        else
            return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindsPeak1)
        {
            watched = true;
            Time = WorldState.CurrentTime;
            _activation = Module.CastFinishAt(spell);
        }
    }
}

class TheKingsNotice(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.TheKingsNotice));
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

class SpitefulFlameCircleVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.VermillionFlame)
            _aoes.Add(new(circle, WPos.ClampToGrid(actor.Position)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SpitefulFlame1)
        {
            if (++NumCasts == 12)
            {
                NumCasts = 0;
                _aoes.Clear();
            }
        }
    }
}

class SpitefulFlameRect(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SpitefulFlame2), new AOEShapeRect(80f, 2f));

class DynasticFlame(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(10f), (uint)TetherID.fireorbs, centerAtTarget: true)
{
    private int orbcount;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count != 0 && CurrentBaits[0].Target == actor)
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 18f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count != 0 && CurrentBaits[0].Target == actor)
            hints.Add("Go to the edge and run until 4 orbs are spawned");
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.VermillionFlame)
        {
            if (++orbcount == 4)
            {
                CurrentBaits.Clear();
                orbcount = 0;
            }
        }
    }
}

class SkyrendingStrike(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SkyrendingStrike), "Enrage!", true);
