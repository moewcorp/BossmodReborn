namespace BossMod.Endwalker.Ultimate.DSW2;

// used by two trio mechanics, in p2 and in p5
class DragonsGaze(BossModule module, OID bossOID) : Components.GenericGaze(module, ActionID.MakeSpell(AID.DragonsGazeAOE))
{
    public bool EnableHints;
    private readonly OID _bossOID = bossOID;
    private Actor? _boss;
    private WPos _eyePosition;

    public bool Active => _boss != null;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        // TODO: activation time
        if (_boss != null && NumCasts == 0)
        {
            var eyes = new Eye[2];
            eyes[0] = new(_eyePosition, Range: EnableHints ? 10000f : default);
            eyes[1] = new(_boss.Position, Range: EnableHints ? 10000f : default);
            return eyes;
        }
        return [];
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        // seen indices: 2 = E, 5 = SW, 6 = W => inferring 0=N, 1=NE, ... cw order
        if (state == 0x00020001 && index <= 7)
        {
            _boss = Module.Enemies(_bossOID)[0];
            _eyePosition = Arena.Center + 40f * (180f - index * 45f).Degrees().ToDirection();
        }
    }
}
