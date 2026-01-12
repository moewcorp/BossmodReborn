using System.ComponentModel;

namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

// ---------
// Constants
// ---------
static class InsaneAirData
{

    public static readonly AOEShapeCone SnapCone = new(60f, 15f.Degrees()); // total 30° default
}

// -----------------------
// Baited conal telegraphs
// -----------------------
sealed class InsaneAirSnaps(BossModule module) : Components.GenericBaitAway(module)

{
    private readonly List<(ulong origin, ulong target)> _pairs = [];

    private const uint IconFire = (uint)IconID._Gen_Icon_m0982trg_c1c; // 665 (fire target)
    private const uint IconWater = (uint)IconID._Gen_Icon_m0982trg_c0c; // 651 (water target)

    private static readonly uint[] SurfboardOIDs =
    [
        
        
        
        (uint)OID.Actor1ebf35,
    ];

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var (o, t) in _pairs)
        {
            var origin = WorldState.Actors.Find(o);
            var target = WorldState.Actors.Find(t);
            if (origin != null && target != null)
                CurrentBaits.Add(new(origin, target, InsaneAirData.SnapCone));
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID != IconFire && iconID != IconWater)
            return;

        var wantSID = iconID == IconFire ? (uint)SID.Firesnaking : (uint)SID.Watersnaking;
        if (actor.FindStatus(wantSID) == null)
            return;

        var origin = FindBestSurfboardOrigin(actor);
        if (origin != null)
            _pairs.Add((origin.InstanceID, actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BlastingSnap1)
            _pairs.RemoveAll(p => WorldState.Actors.Find(p.target)?.FindStatus((uint)SID.Firesnaking) != null);

        if (spell.Action.ID == (uint)AID.PlungingSnap1)
            _pairs.RemoveAll(p => WorldState.Actors.Find(p.target)?.FindStatus((uint)SID.Watersnaking) != null);
    }

    private Actor? FindBestSurfboardOrigin(Actor target)
    {
        Actor? best = null;
        float bestDist2 = float.MaxValue;

        foreach (var oid in SurfboardOIDs)
        {
            foreach (var a in Module.Enemies(oid))
            {
                var d2 = (a.Position - target.Position).LengthSq();
                if (d2 < bestDist2)
                {
                    bestDist2 = d2;
                    best = a;
                }
            }
        }

        return best;
    }
}

// -----------------------------
// Persistent fire cone "puddles"
// -----------------------------
sealed class BlastingSnapPersistent(BossModule module) : Components.GenericAOEs(module, (uint)AID.BlastingSnap1)
{
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            // Fire snap resolves -> leave a persistent cone at the helper's pos/rot
            case (uint)AID.BlastingSnap1:
                _aoes.Add(new(InsaneAirData.SnapCone, caster.Position, caster.Rotation, WorldState.CurrentTime, Colors.AOE, true, caster.InstanceID));
                break;

            // Cleanup at end of mechanic
            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _aoes.Clear();
                break;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        var id = actor.InstanceID;
        _aoes.RemoveAll(a => a.ActorID == id);
    }
}
