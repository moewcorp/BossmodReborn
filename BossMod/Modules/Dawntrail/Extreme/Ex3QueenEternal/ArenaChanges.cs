namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override bool KeepOnPhaseChange => true;
    private bool firstEarthArena = true;

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID != 0x8000000D || param1 > 0x08)
            return;
        switch (param1)
        {
            case 0x01: // default arena
                SetArena(Ex3QueenEternal.NormalBounds, Ex3QueenEternal.ArenaCenter);
                break;
            case 0x02: // x arena (wind)
                SetArena(Ex3QueenEternal.WindBounds, Ex3QueenEternal.WindBounds.Center);
                break;
            case 0x04: // disjointed rect (Earth) arena
                if (firstEarthArena)
                    firstEarthArena = false; // don't want to switch arena here because of gravity stuff
                else
                    SetArena(Ex3QueenEternal.EarthBounds, Ex3QueenEternal.EarthBounds.Center);
                break;
            case 0x08: // ice arena
                SetArena(Ex3QueenEternal.IceBounds, Ex3QueenEternal.IceBounds.Center);
                break;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x08)
        {
            if (state == 0x01000080)
                SetArena(Ex3QueenEternal.HalfBounds, Ex3QueenEternal.HalfBoundsCenter);
            else if (state == 0x02000001)
                SetArena(Ex3QueenEternal.NormalBounds, Ex3QueenEternal.ArenaCenter);
        }
    }

    private void SetArena(ArenaBounds bounds, WPos center)
    {
        Arena.Bounds = bounds;
        Arena.Center = center;
    }
}
