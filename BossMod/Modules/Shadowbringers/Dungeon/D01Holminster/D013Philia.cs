namespace BossMod.Shadowbringers.Dungeon.D01Holmintser.D013Philia;

public enum OID : uint
{
    Boss = 0x278C, // R9.8
    IronChain = 0x2895, // R1.0
    SludgeVoidzone = 0x1EABFA,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    ScavengersDaughter = 15832, // Boss->self, 4.0s cast, range 40 circle
    HeadCrusher = 15831, // Boss->player, 4.0s cast, single-target
    Pendulum = 16777, // Boss->self, 5.0s cast, single-target, cast to jump
    PendulumAOE1 = 16790, // Boss->location, no cast, range 40 circle, jump to target
    PendulumAOE2 = 15833, // Boss->location, no cast, range 40 circle, jump back to center
    PendulumAOE3 = 16778, // Helper->location, 4.5s cast, range 40 circle, damage fall off AOE visual
    ChainDown = 17052, // Boss->self, 5.0s cast, single-target 
    AethersupFirst = 15848, // Boss->self, 15.0s cast, range 21 120-degree cone
    AethersupRest = 15849, // Helper->self, no cast, range 24+R 120-degree cone
    RightKnout = 15846, // Boss->self, 5.0s cast, range 24 210-degree cone
    LeftKnout = 15847, // Boss->self, 5.0s cast, range 24 210-degree cone
    Taphephobia = 15842, // Boss->self, 4.5s cast, single-target
    Taphephobia2 = 16769, // Helper->player, 5.0s cast, range 6 circle
    IntoTheLightMarker = 15844, // Helper->player, no cast, single-target, line stack
    IntoTheLightVisual = 17232, // Boss->self, 5.0s cast, single-target
    IntoTheLight = 15845, // Boss->self, no cast, range 50 width 8 rect
    FierceBeatingRotationVisual = 15834, // Boss->self, 5.0s cast, single-target
    FierceBeatingVisual1 = 15836, // Boss->self, no cast, single-target
    FierceBeatingVisual2 = 15835, // Boss->self, no cast, single-target
    FierceBeatingExaFirst = 15837, // Helper->self, 5.0s cast, range 4 circle
    FierceBeatingExaRestFirst = 15838, // Helper->self, no cast, range 4 circle
    FierceBeatingExaRestRest = 15839, // Helper->location, no cast, range 4 circle
    CatONineTailsVisual = 15840, // Boss->self, no cast, single-target
    CatONineTails = 15841 // Helper->self, 2.0s cast, range 25 120-degree cone
}

public enum IconID : uint
{
    SpreadFlare = 87, // player
    ChainTarget = 92 // player
}

public enum SID : uint
{
    Fetters = 1849 // none->player, extra=0xEC4
}

class SludgeVoidzone(BossModule module) : Components.PersistentVoidzone(module, 9.8f, m => m.Enemies(OID.SludgeVoidzone).Where(z => z.EventState != 7));
class ScavengersDaughter(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ScavengersDaughter));
class HeadCrusher(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HeadCrusher));

class Fetters(BossModule module) : BossComponent(module)
{
    private bool chained;
    private bool chainsactive;
    private Actor? chaintarget;
    private bool casting;

    public override void Update()
    {
        var fetters = chaintarget?.FindStatus((uint)SID.Fetters) != null;
        if (fetters)
            chainsactive = true;
        if (fetters && !chained)
            chained = true;
        if (chaintarget != null && !fetters && !casting)
        {
            chained = false;
            chaintarget = null;
            chainsactive = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (chaintarget != null && !chainsactive)
            hints.Add($"{chaintarget.Name} is about to be fettered!");
        else if (chaintarget != null && chainsactive)
            hints.Add($"Destroy fetters on {chaintarget.Name}!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (chained && actor != chaintarget)
            for (var i = 0; i < hints.PotentialTargets.Count; ++i)
            {
                var e = hints.PotentialTargets[i];
                e.Priority = e.Actor.OID switch
                {
                    (uint)OID.IronChain => 1,
                    (uint)OID.Boss => AIHints.Enemy.PriorityInvincible,
                    _ => 0
                };
            }
        var ironchain = Module.Enemies((uint)OID.IronChain).FirstOrDefault();
        if (ironchain != null && !ironchain.IsDead)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(ironchain.Position, 3.6f));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ChainTarget)
        {
            chaintarget = actor;
            casting = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChainDown)
            casting = false;
    }
}

class Aethersup(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(24f, 60f.Degrees());
    private AOEInstance _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe != default)
            return [_aoe with { Risky = Module.Enemies((uint)OID.IronChain).Any(x => x.IsDead) }];
        else
            return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AethersupFirst)
            _aoe = new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AethersupFirst:
            case (uint)AID.AethersupRest:
                if (++NumCasts == 4)
                {
                    _aoe = default;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class PendulumFlare(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(20), (uint)IconID.SpreadFlare, ActionID.MakeSpell(AID.PendulumAOE1), 5.1f, true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaits.Any(x => x.Target == actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(D013Philia.ArenaCenter, 18.5f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaits.Any(x => x.Target == actor))
            hints.Add("Bait away!");
    }
}

class PendulumAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PendulumAOE3), 15f);

class Knout(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(24f, 105f.Degrees()));
class LeftKnout(BossModule module) : Knout(module, AID.LeftKnout);
class RightKnout(BossModule module) : Knout(module, AID.RightKnout);

class Taphephobia(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Taphephobia2), 6f);

class IntoTheLight(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.IntoTheLightMarker), ActionID.MakeSpell(AID.IntoTheLight), 5.3f);

class CatONineTails(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shape = new(25, 60.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FierceBeatingRotationVisual)
            Sequences.Add(new(_shape, spell.LocXZ, spell.Rotation + 180f.Degrees(), -45f.Degrees(), Module.CastFinishAt(spell), 2, 8));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CatONineTails)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

class FierceBeating(BossModule module) : Components.Exaflare(module, 4f)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var linesCount = Lines.Count;
        if (linesCount == 0)
            return [];
        var futureAOEs = FutureAOEs(linesCount);
        var imminentAOEs = ImminentAOEs(linesCount);
        var futureCount = futureAOEs.Count;
        var imminentCount = imminentAOEs.Length;
        var aoesCount = _aoes.Count;
        var total = futureCount + imminentCount + aoesCount;
        var index = 0;
        var aoes = new AOEInstance[total];
        for (var i = 0; i < futureCount; ++i)
        {
            var aoe = futureAOEs[i];
            aoes[index++] = new(Shape, aoe.Item1, aoe.Item3, aoe.Item2, FutureColor);
        }
        for (var i = 0; i < imminentCount; ++i)
        {
            var aoe = imminentAOEs[i];
            aoes[index++] = new(Shape, aoe.Item1, aoe.Item3, aoe.Item2, ImminentColor);
        }
        for (var i = 0; i < _aoes.Count; ++i)
        {
            var aoe = _aoes[i];
            aoes[index++] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FierceBeatingExaFirst)
        {
            AddLine(ref caster, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FierceBeatingExaRestFirst)
        {
            AddLine(ref caster, WorldState.FutureTime(1d));
        }
        if (Lines.Count != 0)
        {
            if (spell.Action.ID is (uint)AID.FierceBeatingExaFirst or (uint)AID.FierceBeatingExaRestFirst)
                Advance(caster.Position);
            else if (spell.Action.ID == (uint)AID.FierceBeatingExaRestRest)
                Advance(spell.TargetXZ);
        }

        void Advance(WPos pos)
        {
            var index = Lines.FindIndex(item => item.Next.AlmostEqual(pos, 1f));
            if (index < 0)
                return;
            AdvanceLine(Lines[index], pos);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }

    public void AddLine(ref Actor caster, DateTime activation)
    {
        var adv = 2.5f * caster.Rotation.ToDirection();
        Lines.Add(new() { Next = caster.Position, Advance = adv, NextExplosion = activation, TimeToMove = 1f, ExplosionsLeft = 7, MaxShownExplosions = 3 });
        ++NumCasts;
        if (_aoes.Count != 0 && NumCasts > 2)
            _aoes.RemoveAt(0);
        if (NumCasts <= 14)
            _aoes.Add(new(circle, WPos.ClampToGrid(WPos.RotateAroundOrigin(45, D013Philia.ArenaCenter, caster.Position + adv)), default, WorldState.FutureTime(3.7d)));
        if (NumCasts == 16)
            NumCasts = 0;
    }
}

class D013PhiliaStates : StateMachineBuilder
{
    public D013PhiliaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScavengersDaughter>()
            .ActivateOnEnter<HeadCrusher>()
            .ActivateOnEnter<PendulumFlare>()
            .ActivateOnEnter<PendulumAOE>()
            .ActivateOnEnter<Aethersup>()
            .ActivateOnEnter<Fetters>()
            .ActivateOnEnter<SludgeVoidzone>()
            .ActivateOnEnter<LeftKnout>()
            .ActivateOnEnter<RightKnout>()
            .ActivateOnEnter<Taphephobia>()
            .ActivateOnEnter<IntoTheLight>()
            .ActivateOnEnter<CatONineTails>()
            .ActivateOnEnter<FierceBeating>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 676, NameID = 8301)]
public class D013Philia(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(134, -465); // slightly different from calculated center due to difference operation
    private static readonly ArenaBoundsComplex arena = new([new Circle(ArenaCenter, 19.5f)], [new Rectangle(new(134, -445), 20, 1.5f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.IronChain), Colors.Vulnerable);
    }
}