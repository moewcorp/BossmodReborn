﻿namespace BossMod.Dawntrail.Unreal.Un1Byakko;

class AratamaPuddleBait(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.AratamaPuddle, ActionID.MakeSpell(AID.AratamaPuddle), 4f, 5.1f)
{
    private DateTime _nextSpread;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == SpreadAction && WorldState.CurrentTime > _nextSpread)
        {
            if (++NumFinishedSpreads >= 3)
                Spreads.Clear();
            else
                _nextSpread = WorldState.FutureTime(0.5f); // protection in case one target dies
        }
    }
}

class AratamaPuddleVoidzone(BossModule module) : Components.PersistentVoidzone(module, 4f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.AratamaPuddle);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
