namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class WeaponTracker(BossModule module) : Components.GenericAOEs(module)
{
    public bool AOEImminent;
    private AOEInstance? _aoe;
    public enum Stance { None, Sword, Staff, Chakram }
    public Stance CurStance;
    private static readonly AOEShapeDonut donut = new(5f, 40f);
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeCross cross = new(40f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.HydaelynsWeapon)
        {
            var activation = WorldState.FutureTime(6);
            if (status.Extra == 0x1B4)
            {
                _aoe = new(circle, Module.PrimaryActor.Position, default, activation);
                CurStance = Stance.Staff;
                AOEImminent = true;
            }
            else if (status.Extra == 0x1B5)
            {
                _aoe = new(donut, Module.PrimaryActor.Position, default, activation);
                AOEImminent = true;
                CurStance = Stance.Chakram;
            }
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.HydaelynsWeapon)
        {
            _aoe = new(cross, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, WorldState.FutureTime(6.9d));
            AOEImminent = true;
            CurStance = Stance.Sword;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WeaponChangeAOEChakram or (uint)AID.WeaponChangeAOEStaff or (uint)AID.WeaponChangeAOESword)
        {
            AOEImminent = false;
            _aoe = null;
        }
    }
}
