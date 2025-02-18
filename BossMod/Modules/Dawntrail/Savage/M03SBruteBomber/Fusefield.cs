namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

class Fusefield(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor spark, Actor target, int order)> _sparks = [];
    private readonly int[] _orders = new int[PartyState.MaxPartySize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_orders[slot] > 0)
            hints.Add($"Order: {_orders[slot]}", false);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var i = 0; i < _sparks.Count; ++i)
        {
            var s = _sparks[i];
            if (s.order == _orders[pcSlot])
            {
                Arena.AddLine(s.spark.Position, s.target.Position, Colors.Safe);
                Arena.Actor(s.spark, Colors.Object, true);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Bombarium && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            _orders[slot] = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 30 ? 1 : 2;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Bombarium && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            _orders[slot] = 0;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.SinisterSpark && tether.ID == (uint)TetherID.Fusefield && WorldState.Actors.Find(tether.Target) is var target && target != null)
            _sparks.Add((source, target, (source.Position - target.Position).LengthSq() < 55 ? 1 : 2));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ManaExplosion or AID.ManaExplosionKill)
        {
            _sparks.RemoveAll(s => s.spark == caster);
        }
    }
}

class FusefieldVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    public bool Active;
    private static readonly AOEShapeCircle circle = new(5);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x13)
            switch (state)
            {
                case 0x00020001:
                    _aoe = new(circle, Arena.Center, default, WorldState.FutureTime(9.1f));
                    break;
                case 0x00200010:
                    Arena.Bounds = M03SBruteBomber.FuseFieldBounds;
                    _aoe = null;
                    Active = true;
                    break;
                case 0x00080004:
                    Arena.Bounds = M03SBruteBomber.DefaultBounds;
                    Active = false;
                    break;
            }
    }
}
