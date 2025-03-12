namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

class RuinfallAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RuinfallAOE), 6f);

class RuinfallKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.RuinfallKB), 21f, stopAfterWall: true, kind: Kind.DirForward)
{
    private readonly RuinfallTower _tower = module.FindComponent<RuinfallTower>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Casters.Count != 0 ? Casters[0] : null;
        if (source != null && actor.Role != Role.Tank)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.PrimaryActor.Position, new Angle(), 1f, default, 20f), Module.CastFinishAt(source.CastInfo));
        }
        var towers = _tower.Towers;
        var count = towers.Count;
        if (count == 0)
            return;
        var isDelayDeltaLow = (towers[0].Activation - WorldState.CurrentTime).TotalSeconds < 5d;
        var isActorInsideTower = false;
        for (var i = 0; i < count; ++i)
        {
            if (towers[i].IsInside(actor))
            {
                isActorInsideTower = true;
                break;
            }
        }
        if (isDelayDeltaLow && isActorInsideTower)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
    }
}

class RuinfallTower(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.RuinfallTower), 6f, 2, 2)
{
    public override void Update()
    {
        if (Towers.Count == 0)
            return;
        var forbidden = Raid.WithSlot(false, true, true).WhereActor(p => p.Role != Role.Tank).Mask();
        foreach (ref var t in Towers.AsSpan())
            t.ForbiddenSoakers = forbidden;
    }
}
