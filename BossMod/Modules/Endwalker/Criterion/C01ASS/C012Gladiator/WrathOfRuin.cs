namespace BossMod.Endwalker.VariantCriterion.C01ASS.C012Gladiator;

class GoldenSilverFlame(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _goldenFlames = [];
    private readonly List<Actor> _silverFlames = [];
    private readonly int[] _debuffs = new int[PartyState.MaxPartySize]; // silver << 16 | gold

    public bool Active => _goldenFlames.Count + _silverFlames.Count > 0;

    private static readonly AOEShapeRect _shape = new(60f, 5f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (DebuffsAtPosition(actor.Position) != _debuffs[slot])
            hints.Add("Go to correct cell!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // TODO: implement
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
        {
            var color = Colors.SafeFromAOE;
            foreach (var c in SafeCenters(_debuffs[pcSlot]))
                Arena.ZoneRect(c, new WDir(1f, 0f), _shape.HalfWidth, _shape.HalfWidth, _shape.HalfWidth, color);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var debuff = status.ID switch
        {
            (uint)SID.GildedFate => status.Extra,
            (uint)SID.SilveredFate => status.Extra << 16,
            _ => 0
        };

        if (debuff == 0)
            return;
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _debuffs[slot] |= debuff;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CasterList(spell)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CasterList(spell)?.Remove(caster);
    }

    private List<Actor>? CasterList(ActorCastInfo spell) => spell.Action.ID switch
    {
        (uint)AID.NGoldenFlame or (uint)AID.SGoldenFlame => _goldenFlames,
        (uint)AID.NSilverFlame or (uint)AID.SSilverFlame => _silverFlames,
        _ => null
    };

    private int CastersHittingPosition(List<Actor> casters, WPos pos) => casters.Count(a => _shape.Check(pos, a.Position, a.CastInfo!.Rotation));
    private int DebuffsAtPosition(WPos pos) => CastersHittingPosition(_silverFlames, pos) | (CastersHittingPosition(_goldenFlames, pos) << 16);

    private IEnumerable<WPos> SafeCenters(int debuff)
    {
        var limit = Arena.Center + new WDir(Arena.Bounds.Radius, Arena.Bounds.Radius);
        var first = Arena.Center - new WDir(Arena.Bounds.Radius - _shape.HalfWidth, Module.Bounds.Radius - _shape.HalfWidth);
        var advance = 2 * _shape.HalfWidth;
        for (var x = first.X; x < limit.X; x += advance)
            for (var z = first.Z; z < limit.Z; z += advance)
                if (DebuffsAtPosition(new WPos(x, z)) == debuff)
                    yield return new(x, z);
    }
}

// note: actual spell targets location, but it seems to be incorrect...
// note: we can predict cast start during Regret actor spawn...
abstract class RackAndRuin(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(40f, 2.5f), 8);
class NRackAndRuin(BossModule module) : RackAndRuin(module, AID.NRackAndRuin);
class SRackAndRuin(BossModule module) : RackAndRuin(module, AID.SRackAndRuin);

abstract class NothingBesideRemains(BossModule module, AID aid) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(aid), 8f);
class NNothingBesideRemains(BossModule module) : NothingBesideRemains(module, AID.NNothingBesideRemainsAOE);
class SNothingBesideRemains(BossModule module) : NothingBesideRemains(module, AID.SNothingBesideRemainsAOE);
