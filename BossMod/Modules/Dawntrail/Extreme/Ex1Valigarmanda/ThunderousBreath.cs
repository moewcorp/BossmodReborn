namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class ThunderousBreath : Components.CastCounter
{
    public ThunderousBreath(BossModule module) : base(module, ActionID.MakeSpell(AID.ThunderousBreathAOE))
    {
        var platform = module.FindComponent<ThunderPlatform>();
        if (platform != null)
        {
            var party = module.Raid.WithSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
                platform.RequireHint[i] = platform.RequireLevitating[i] = true;
        }
    }
}

class ArcaneLighning(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ArcaneLightning))
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(50f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ArcaneSphere)
        {
            AOEs.Add(new(_shape, WPos.ClampToGrid(actor.Position), actor.Rotation, WorldState.FutureTime(8.6d)));
        }
    }
}
