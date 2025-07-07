namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class PainStormFrigidPulsePainfulGust(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(20f);
    private static readonly AOEShapeDonut donut = new(8f, 25f);
    private static readonly AOEShapeCone cone = new(35f, 65f.Degrees());
    private readonly NorthSouthwind _kb = module.FindComponent<NorthSouthwind>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.FrigidPulse or (uint)AID.FrigidPulseShadow => donut,
            (uint)AID.PainStorm or (uint)AID.PainStormShadow => cone,
            (uint)AID.PainfulGust or (uint)AID.PainfulGustShadow => circle,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var count = _aoes.Count;
        var id = caster.InstanceID;
        for (var i = 0; i < count; ++i)
        {
            if (_aoes[i].ActorID == id)
            {
                _aoes.RemoveAt(i);
                return;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var kbs = _kb.ActiveKnockbacks(slot, actor);
        if (kbs.Length != 0 && !_kb.IsImmune(slot, kbs[0].Activation))
        { }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
