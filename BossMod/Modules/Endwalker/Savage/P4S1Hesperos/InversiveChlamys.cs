﻿namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to inversive chlamys mechanic (tethers)
// note that forbidden targets are selected either from bloodrake tethers (first instance of mechanic) or from tower types (second instance of mechanic)
class InversiveChlamys(BossModule module) : BossComponent(module)
{
    private bool _assigned;
    private BitMask _tetherForbidden;
    private BitMask _tetherTargets;
    private BitMask _tetherInAOE;

    private const float _aoeRange = 5;

    public bool TethersActive => _tetherTargets.Any();

    public override void Update()
    {
        if (!_assigned)
        {
            var coils = Module.FindComponent<BeloneCoils>();
            if (coils == null)
            {
                // assign from bloodrake tethers
                _tetherForbidden = Raid.WithSlot(false, true, true).Tethered(TetherID.Bloodrake).Mask();
                _assigned = true;
            }
            else if (coils.ActiveSoakers != BeloneCoils.Soaker.Unknown)
            {
                // assign from coils (note that it happens with some delay)
                _tetherForbidden = Raid.WithSlot(false, true, true).WhereActor(coils.IsValidSoaker).Mask();
                _assigned = true;
            }
        }

        _tetherTargets = _tetherInAOE = new();
        if (_tetherForbidden.None())
            return;

        foreach ((var i, var player) in Raid.WithSlot(false, true, true).Tethered(TetherID.Chlamys))
        {
            _tetherTargets.Set(i);
            _tetherInAOE |= Raid.WithSlot(false, true, true).InRadiusExcluding(player, _aoeRange).Mask();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_tetherForbidden.None())
            return;

        if (!_tetherForbidden[slot])
        {
            // we should be grabbing tethers
            if (_tetherTargets.None())
            {
                hints.Add("Tethers: prepare to intercept", false);
            }
            else if (!_tetherTargets[slot])
            {
                hints.Add("Tethers: intercept!");
            }
            else if (Raid.WithoutSlot(false, true, true).InRadiusExcluding(actor, _aoeRange).Any())
            {
                hints.Add("Tethers: GTFO from others!");
            }
            else
            {
                hints.Add("Tethers: OK", false);
            }
        }
        else
        {
            // we should be passing tethers
            if (_tetherTargets.None())
            {
                hints.Add("Tethers: prepare to pass", false);
            }
            else if (_tetherTargets[slot])
            {
                hints.Add("Tethers: pass!");
            }
            else if (_tetherInAOE[slot])
            {
                hints.Add("Tethers: GTFO from aoe!");
            }
            else
            {
                hints.Add("Tethers: avoid", false);
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var forbidden = Raid.WithSlot(true, true, true).IncludedInMask(_tetherForbidden).FirstOrDefault().Item2;
        if (forbidden != null)
        {
            hints.Add($"Intercept: {(forbidden.Role is Role.Tank or Role.Healer ? "DD" : "Tanks/Healers")}");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_tetherTargets.None())
            return;

        var failingPlayers = _tetherForbidden & _tetherTargets;
        foreach ((var i, var player) in Raid.WithSlot(false, true, true))
        {
            var failing = failingPlayers[i];
            var inAOE = _tetherInAOE[i];
            Arena.Actor(player, failing ? Colors.Danger : (inAOE ? Colors.PlayerInteresting : Colors.PlayerGeneric));

            if (player.Tether.ID == (uint)TetherID.Chlamys)
            {
                Arena.AddLine(player.Position, Module.PrimaryActor.Position, failing ? Colors.Danger : Colors.Safe);
                Arena.AddCircle(player.Position, _aoeRange, Colors.Danger);
            }
        }
    }
}
