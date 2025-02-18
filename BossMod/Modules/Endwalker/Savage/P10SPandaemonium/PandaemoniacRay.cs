namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class PandaemoniacRay(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PandaemoniacRayAOE), new AOEShapeRect(30, 25));

class JadePassage(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.JadePassage))
{
    private readonly List<Actor> _spheres = module.Enemies(OID.ArcaneSphere);
    private readonly DateTime _activation = module.WorldState.FutureTime(3.6f);

    private static readonly AOEShapeRect _shape = new(40, 1, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Actor[] spheres = [.. _spheres.Where(actor => !actor.IsDead)];
        var count = spheres.Length;
        if (count == 0)
            return [];
        var aoes = new List<AOEInstance>(count);
        for (var i = 0; i < count; ++i)
        {
            var s = spheres[i];
            AOEInstance aoeInstance = new(_shape, s.Position, s.Rotation, Module.CastFinishAt(s.CastInfo, 0, _activation));
            aoes.Add(aoeInstance);
        }
        return aoes;
    }
}
