namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

sealed class RuinfallAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RuinfallAOE, 6f);

sealed class RuinfallKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RuinfallKB, 21f, stopAfterWall: true, kind: Kind.DirForward)
{
    private readonly RuinfallTower _tower = module.FindComponent<RuinfallTower>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
        {
            return;
        }
        if (actor.Role != Role.Tank)
        {
            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.PrimaryActor.Position, new WDir(default, 1f), 1f, default, 20f), c.Activation);
            return;
        }
        var towers = _tower.Towers;
        var count = towers.Count;
        if (count == 0)
        {
            return;
        }
        ref var t0 = ref towers.Ref(0);
        var isDelayDeltaLow = (t0.Activation - WorldState.CurrentTime).TotalSeconds < 5d;

        if (isDelayDeltaLow && t0.IsInside(actor))
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
        }
    }
}

sealed class RuinfallTower(BossModule module) : Components.CastTowers(module, (uint)AID.RuinfallTower, 6f, 2, 2)
{
    public override void Update()
    {
        var count = Towers.Count;
        if (count == 0)
        {
            return;
        }
        var party = Module.Raid.WithSlot(true, true, true);
        var len = party.Length;
        BitMask forbidden = default;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var p = ref party[i];
            if (p.Item2.Role != Role.Tank)
            {
                forbidden.Set(p.Item1);
            }
        }
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            ref var t = ref towers[i];
            t.ForbiddenSoakers = forbidden;
        }
    }
}
