﻿namespace BossMod.Stormblood.Ultimate.UCOB;

class P3SeventhUmbralEra(BossModule module) : Components.GenericKnockback(module, ActionID.MakeSpell(AID.SeventhUmbralEra), true)
{
    private readonly DateTime _activation = module.WorldState.FutureTime(5.3d);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        return new Knockback[1] { new(Arena.Center, 11, _activation) };
    }
}

class P3CalamitousFlame(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.CalamitousFlame));
class P3CalamitousBlaze(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.CalamitousBlaze));
