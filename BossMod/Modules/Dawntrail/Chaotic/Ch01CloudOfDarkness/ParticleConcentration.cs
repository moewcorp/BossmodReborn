﻿namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

// envcontrols:
// 1F-2E = 1-man towers
// - 00020001 - appear
// - 00200010 - occupied
// - 00080004 - disappear
// - 08000001 - ? (spot animation)
// - arrangement:
//      25             26
//   21 xx 1F xx xx 20 xx 22
//      23             24
//      xx             xx
//      xx             xx
//      2B             2C
//   29 xx 27 xx xx 28 xx 2A
//      2D             2E
// 2F-3E = 2-man towers
// - 00020001 - appear
// - 00200010 - occupied by 1
// - 00800040 - occupied by 2
// - 00080004 - disappear
// - 08000001 - ? (spot animations)
// - arrangement (also covers intersecting square):
//      35             36
//   31 xx 2F xx xx 30 xx 32
//      33             34
//      xx             xx
//      xx             xx
//      3B             3C
//   39 xx 37 xx xx 38 xx 3A
//      3D             3E
// 3F-46 = 3-man towers
// - 00020001 - appear
// - 00200010 - occupied by 1
// - 00800040 - occupied by 2
// - 02000100 - occupied by 3
// - 00080004 - disappear
// - 08000001 - ? (spot animations)
// - arrangement:
//     3F         43
//   42  40     44  46
//     41         45
// 47-56 = 1-man tower falling orb
// 57-66 = 2-man tower falling orb
// 67-6E = 3-man tower falling orb
sealed class ParticleConcentration(BossModule module) : Components.GenericTowers(module)
{
    private BitMask _innerPlayers;
    private BitMask _outerPlayers;
    private readonly List<WPos> _outerTowers = []; // note: initially we don't show outer towers, as players resolve different mechanics first

    public void ShowOuterTowers()
    {
        var activation = Towers.Count != 0 ? Towers[0].Activation : default;
        var count = _outerTowers.Count;
        for (var i = 0; i < count; ++i)
        {
            Towers.Add(new(_outerTowers[i], 3f, 3, 3, _innerPlayers, activation));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.InnerDarkness:
                _innerPlayers.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.OuterDarkness:
                _outerPlayers.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.InnerDarkness:
                _innerPlayers.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.OuterDarkness:
                _outerPlayers.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state != 0x00020001u) // appear
            return;

        var (offset, count) = index switch
        {
            0x1F => (new(-9f, -15f), 1),
            0x20 => (new(9f, -15f), 1),
            0x21 => (new(-21f, -15f), 1),
            0x22 => (new(21f, -15f), 1),
            0x23 => (new(-15f, -9f), 1),
            0x24 => (new(15f, -9f), 1),
            0x25 => (new(-15f, -21f), 1),
            0x26 => (new(15f, -21f), 1),
            0x27 => (new(-9f, 15f), 1),
            0x28 => (new(9f, 15f), 1),
            0x29 => (new(-21f, 15f), 1),
            0x2A => (new(21f, 15f), 1),
            0x2B => (new(-15f, 9f), 1),
            0x2C => (new(15f, 9f), 1),
            0x2D => (new(-15f, 21f), 1),
            0x2E => (new(15f, 21f), 1),
            0x2F => (new(-12f, -15f), 2),
            0x30 => (new(12f, -15f), 2),
            0x31 => (new(-18f, -15f), 2),
            0x32 => (new(18f, -15f), 2),
            0x33 => (new(-15f, -12f), 2),
            0x34 => (new(15f, -12f), 2),
            0x35 => (new(-15f, -18f), 2),
            0x36 => (new(15f, -18f), 2),
            0x37 => (new(-12f, 15f), 2),
            0x38 => (new(12f, 15f), 2),
            0x39 => (new(-18f, 15f), 2),
            0x3A => (new(18f, 15f), 2),
            0x3B => (new(-15f, 12f), 2),
            0x3C => (new(15f, 12f), 2),
            0x3D => (new(-15f, 18f), 2),
            0x3E => (new(15f, 18f), 2),
            0x3F => (new(-26.5f, -4.5f), 3),
            0x40 => (new(-22f, 0f), 3),
            0x41 => (new(-26.5f, +4.5f), 3),
            0x42 => (new(-31f, 0f), 3),
            0x43 => (new(+26.5f, -4.5f), 3),
            0x44 => (new(+22f, 0f), 3),
            0x45 => (new(+26.5f, +4.5f), 3),
            0x46 => (new(+31f, 0f), 3),
            _ => ((WDir)default, 0)
        };

        if (count == 3)
        {
            _outerTowers.Add(Arena.Center + offset);
        }
        else if (count > 0)
        {
            Towers.Add(new(Arena.Center + offset, 3f, count, count, _outerPlayers, WorldState.FutureTime(10.1d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ParticleBeam1 or (uint)AID.ParticleBeam2 or (uint)AID.ParticleBeam3)
        {
            ++NumCasts;
            var count = Towers.Count;
            var pos = caster.Position;
            var towers = CollectionsMarshal.AsSpan(Towers);
            for (var i = 0; i < count; ++i)
            {
                ref var t = ref towers[i];
                if (t.Position == pos)
                {
                    Towers.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
