namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

class WitchHunt(BossModule module) : Components.GenericBaitAway(module, (uint)AID.FellFirework) {
    private enum Proximity { Close, Far}
    private List<Proximity> Order = [];
    private DateTime[] Vulns = new DateTime[PartyState.MaxPartySize];
    private DateTime NextActivation;
    public bool Active => Order.Count > 0;

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if ((OID)actor.OID == OID.PariOfPlenty && status.ID == (uint)SID.WitchHunt) {
            Order.Add(status.Extra == 0x3F4 ? Proximity.Close : Proximity.Far);
        }
        
        if (status.ID == (uint)SID.DarkResistanceDown) {
            var slot = Raid.FindSlot(actor.InstanceID);
            Vulns[slot] = status.ExpireAt;
        }
    }
    
    public override void AddGlobalHints(GlobalHints hints) {
        if (Order.Count > 0) {
            var hintString = new StringBuilder();

            for (int i = 0; i < Order.Count; i++) {
                if (i > 0) {
                    hintString.Append(" -> ");
                }
                hintString.Append(Order[i].ToString());
            }
            
            hints.Add(hintString.ToString());
        }
    }
    
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.FireflightFourLongNightsRight || (AID)spell.Action.ID == AID.FireflightFourLongNightsLeft) {
            NextActivation = WorldState.FutureTime(17.0f + 2.2f);
        }
    }
    
    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == WatchedAction) {
            if (++NumCasts > 0 && Order.Count > 0) {
                Order.RemoveAt(0);
                if (Order.Count > 0) {
                    NextActivation = WorldState.FutureTime(3.1f);
                }
            }
        }
    }
    
    public override void Update() {
        CurrentBaits.Clear();
        
        if (!Active) {
            return;
        }
        
        var targets = Raid.WithoutSlot().SortedByRange(Module.PrimaryActor.Position);
        targets = Order[0] == Proximity.Close ? targets.Take(1) : targets.TakeLast(1);
        foreach (var t in targets) {
            CurrentBaits.Add(new(t, t, new AOEShapeCircle(3), NextActivation));
        }
        
        ForbiddenPlayers = Raid.WithSlot().Where(p => Vulns[p.Item1] > NextActivation).Mask();
    }
}

class WitchHuntStack(BossModule module) : Components.GenericBaitStack(module, (uint)AID.FellFirework, true) {
    private DateTime NextActivation;
    private int RemainingStacks;
    public bool Active => RemainingStacks > 0;
    
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.FireflightFourLongNightsRight || (AID)spell.Action.ID == AID.FireflightFourLongNightsLeft) {
            NextActivation = WorldState.FutureTime(17.0f + 2.2f);
        }
    }
    
    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if ((OID)actor.OID == OID.PariOfPlenty && status.ID == (uint)SID.WitchHunt) {
            RemainingStacks++;
        }
    }
    
    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if ((AID)spell.Action.ID == AID.FireWell) {
            if (RemainingStacks > 0) {
                RemainingStacks--;
            }
        }
    }
    
    public override void Update() {
        CurrentBaits.Clear();
        
        if (!Active) {
            return;
        }
        
        var targets = Raid.WithoutSlot().SortedByRange(Module.PrimaryActor.Position);
        var target = targets.ElementAt(1);
        CurrentBaits.Add(new(target, target, new AOEShapeCircle(3), NextActivation));
    }
}