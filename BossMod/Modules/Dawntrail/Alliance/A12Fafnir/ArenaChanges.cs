namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(30, 35);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DarkMatterBlast && Arena.Bounds != A12Fafnir.DefaultBounds)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 1.1f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x22)
        {
            Arena.Bounds = A12Fafnir.DefaultBounds;
            _aoe = null;
        }
    }
}

class DragonBreathArenaChange(BossModule module) : BossComponent(module)
{
    private Angle initialRot;
    private DateTime started;
    private static readonly Circle circle = new(A12Fafnir.ArenaCenter, 16);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.FireVoidzone)
        {
            if (state == 0x00010002) // outer arena starts to turn unsafe
                Arena.Bounds = A12Fafnir.FireArena;
            else if (state == 0x00040008) // outer arena starts to turn safe again
            {
                initialRot = actor.Rotation;
                started = WorldState.CurrentTime;
            }
        }
    }

    public override void Update()
    {
        if (started != default)
        {
            var time = (WorldState.CurrentTime - started).TotalSeconds;
            var angle = initialRot - ((float)time * 30).Degrees(); // 30° of the outer arena turn safe again per second
            if (time >= 12)
            {
                started = default;
                Arena.Bounds = A12Fafnir.DefaultBounds;
                Arena.Center = A12Fafnir.ArenaCenter;
                return;
            }
            ArenaBoundsComplex refresh = new([circle, new Cone(A12Fafnir.ArenaCenter, 30, angle, initialRot)]);
            Arena.Bounds = refresh;
            Arena.Center = refresh.Center;
        }
    }
}
