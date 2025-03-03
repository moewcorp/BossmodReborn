namespace BossMod.Dawntrail.Raid.M01NBlackCat;

class ElevateAndEviscerate(BossModule module) : Components.Knockback(module, ignoreImmunes: true, stopAfterWall: true)
{
    public DateTime Activation;
    public (Actor source, Actor target) Tether;
    public WPos Cache;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Tether != default && actor == Tether.target)
            return [new(Tether.source.Position, 10f, Activation)];
        return [];
    }

    public override void Update()
    {
        foreach (var _ in Sources(0, Tether.target))
        {
            var movements = CalculateMovements(0, Tether.target);
            if (movements.Count != 0)
                Cache = movements[0].to;
            return;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElevateAndEviscerate)
            Activation = Module.CastFinishAt(spell);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.ElevateAndEviscerateGood or (uint)TetherID.ElevateAndEviscerateBad)
            Tether = (source, WorldState.Actors.Find(tether.Target)!);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElevateAndEviscerate)
        {
            Tether = default;
            ++NumCasts;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<ElevateAndEviscerateHint>()?.ActiveAOEs(slot, actor).Any(z => z.Check(pos)) ?? false) || !Arena.InBounds(pos);
}

class ElevateAndEviscerateHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ElevateAndEviscerate _kb = module.FindComponent<ElevateAndEviscerate>()!;
    public static readonly AOEShapeRect Rect = new(5f, 5f, 5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var tether = _kb.Tether;
        if (tether != default && actor == tether.target)
        {
            var damagedCells = Module.FindComponent<ArenaChanges>()!.DamagedCells;
            var tiles = ArenaChanges.Tiles;
            var aoes = new List<AOEInstance>();

            foreach (var index in damagedCells.SetBits())
            {
                var tile = tiles[index];
                aoes.Add(new(Rect, tile.Center, Color: Colors.FutureVulnerable, Risky: false));
            }
            return aoes;
        }
        return [];
    }
}

class ElevateAndEviscerateImpact(BossModule module) : Components.GenericAOEs(module, default, "GTFO from impact!")
{
    private readonly ElevateAndEviscerate _kb = module.FindComponent<ElevateAndEviscerate>()!;
    private AOEInstance? aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = new List<AOEInstance>();
        if (_kb.Tether != default && _kb.Tether.target != actor && Module.InBounds(_kb.Cache))
            aoes.Add(new(ElevateAndEviscerateHint.Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.Cache)), default, _kb.Activation.AddSeconds(3.6d)));
        if (aoe != null)
            aoes.Add(aoe.Value);
        return aoes;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElevateAndEviscerate && Module.InBounds(_kb.Cache))
            aoe = new(ElevateAndEviscerateHint.Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.Cache)), default, Module.CastFinishAt(spell, 3.6f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Impact)
            aoe = null;
    }

    public override void Update()
    {
        if (aoe != null && _kb.Tether != default && _kb.Tether.target.IsDead) // impact doesn't happen if player dies between ElevateAndEviscerate and Impact
            aoe = null;
    }
}
