namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA4ProtoOzma;

class Holy(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Holy), 3f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 9f), Module.CastFinishAt(Casters[0].CastInfo));
    }
}

class ShootingStar(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ShootingStar), 8f, shape: new AOEShapeCircle(26f))
{
    private readonly TransitionAttacks _aoe = module.FindComponent<TransitionAttacks>()!;
    private static readonly Angle a60 = 60f.Degrees(), am60 = -60.Degrees(), a180 = 180.Degrees(), a120 = 120f.Degrees(), am120 = -120f.Degrees(), a30 = 30f.Degrees();
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        if (count == 0)
            return;
        var transitionAOE = _aoe.AOEs.Count != 0 ? _aoe.AOEs[0].Shape : null;
        var forbidden = new Func<WPos, float>[transitionAOE != null ? count : 2 * count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var caster = Casters[i];
            var pos = caster.Position;
            void AddForbiddenCone(Angle direction) => forbidden[index++] = ShapeDistance.InvertedCone(pos, 3.5f, direction, a30);

            switch ((int)pos.X)
            {
                case -38:
                    if (transitionAOE != TransitionAttacks.Donut)
                        AddForbiddenCone(a60);
                    if (transitionAOE != TransitionAttacks.Circle)
                        AddForbiddenCone(am120);
                    break;
                case -17:
                    if (transitionAOE != TransitionAttacks.Donut)
                        AddForbiddenCone(a180);
                    if (transitionAOE != TransitionAttacks.Circle)
                        AddForbiddenCone(default);
                    break;
                case 4:
                    if (transitionAOE != TransitionAttacks.Donut)
                        AddForbiddenCone(am60);
                    if (transitionAOE != TransitionAttacks.Circle)
                        AddForbiddenCone(a120);
                    break;
            }
        }
        hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), Module.CastFinishAt(Casters[0].CastInfo));
    }
}
