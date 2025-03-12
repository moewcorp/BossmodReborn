namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

class AllegiantArsenal(BossModule module) : Components.GenericAOEs(module)
{
    public enum Order { Unknown, SwordSecond, BowSecond, StaffSecond, StaffSwordBow, BowSwordStaff, SwordBowStaff, StaffBowSword, SwordStaffBow, BowStaffSword }

    public Order Mechanics { get; private set; }
    private AOEShape? _pendingAOE;
    private DateTime _activation;

    public bool Active => _pendingAOE != null;

    private static readonly AOEShapeCone _shapeSword = new(70f, 135f.Degrees(), 180f.Degrees());
    private static readonly AOEShapeCone _shapeBow = new(70f, 135f.Degrees());
    private static readonly AOEShapeCircle _shapeStaff = new(10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_pendingAOE != null)
            return new AOEInstance[1] { new(_pendingAOE, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, _activation) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AllegiantArsenalSword:
                Activate(_shapeSword, Module.CastFinishAt(spell), Mechanics switch
                {
                    Order.Unknown => Order.SwordSecond,
                    Order.BowSecond => Order.StaffBowSword,
                    Order.StaffSecond => Order.BowStaffSword,
                    _ => Order.Unknown
                });
                break;
            case (uint)AID.AllegiantArsenalBow:
                Activate(_shapeBow, Module.CastFinishAt(spell), Mechanics switch
                {
                    Order.Unknown => Order.BowSecond,
                    Order.SwordSecond => Order.StaffSwordBow,
                    Order.StaffSecond => Order.SwordStaffBow,
                    _ => Order.Unknown
                });
                break;
            case (uint)AID.AllegiantArsenalStaff:
                Activate(_shapeStaff, Module.CastFinishAt(spell), Mechanics switch
                {
                    Order.Unknown => Order.StaffSecond,
                    Order.SwordSecond => Order.BowSwordStaff,
                    Order.BowSecond => Order.SwordBowStaff,
                    _ => Order.Unknown
                });
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.InfernalSlash or (uint)AID.Flashvane or (uint)AID.FuryOfBozja)
            _pendingAOE = null;
    }

    private void Activate(AOEShape shape, DateTime finishAt, Order newOrder)
    {
        _pendingAOE = shape;
        _activation = finishAt.AddSeconds(5.2f);
        if (newOrder != Order.Unknown)
            Mechanics = newOrder;
    }
}
