namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

class StampedingThunder(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40, 15);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var activation = WorldState.FutureTime(9.4f);
        if ((AID)spell.Action.ID == AID.StampedingThunderVisualWest)
            _aoe = new(rect, new(95, 80), caster.Rotation, activation);
        else if ((AID)spell.Action.ID == AID.StampedingThunderVisualEast)
            _aoe = new(rect, new(105, 80), caster.Rotation, activation);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x00 && state is 0x00200010 or 0x00020001)
            _aoe = null;
    }
}

class ArenaChanges(BossModule module) : BossComponent(module)
{
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
    public static readonly WPos DefaultCenter = new(100, 100);
    public static readonly WPos westRemovedCenter = new(115, 100);
    public static readonly WPos eastremovedCenter = new(85, 100);
    private static readonly ArenaBoundsRect damagedPlatform = new(5, 20);

    public override void OnEventEnvControl(byte index, uint state)
    {
        // index 0x00
        // 0x00200010 - west 3/4 disappears
        // 0x00020001 - east 3/4 disappears
        if (index == 0x00)
        {
            if (state == 0x00020001)
            {
                Arena.Bounds = damagedPlatform;
                Arena.Center = eastremovedCenter;
            }
            else if (state == 0x00200010)
            {
                Arena.Bounds = damagedPlatform;
                Arena.Center = westRemovedCenter;
            }
            else if (state is 0x00400004 or 0x00800004)
            {
                Arena.Bounds = DefaultBounds;
                Arena.Center = DefaultCenter;
            }
        }
    }
}
