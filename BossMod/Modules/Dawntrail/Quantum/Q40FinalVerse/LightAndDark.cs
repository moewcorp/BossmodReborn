namespace BossMod.Dawntrail.Quantum.FinalVerse.Q40EminentGrief;

[SkipLocalsInit]
sealed class LightAndDark(BossModule module) : Endwalker.DeepDungeon.PilgrimsTraverse.LightAndDarkBase(module)
{
    private readonly Q40EminentGrief bossmod = (Q40EminentGrief)module;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DrainAether1:
                UpdateWantedStatus(0, true);
                break;
            case (uint)AID.DrainAether2:
                UpdateWantedStatus(1, false);
                break;
            case (uint)AID.DrainAether3:
                UpdateWantedStatus(0, false);
                break;
            case (uint)AID.DrainAether4:
                UpdateWantedStatus(1, true);
                break;
        }

        void UpdateWantedStatus(int slot, bool wantL)
        {
            wantLight[slot] = wantL;
            aetherdrainActive = true;
            activations[slot] = Module.CastFinishAt(spell, -1d);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DrainAether1 or (uint)AID.DrainAether2 or (uint)AID.DrainAether3 or (uint)AID.DrainAether4)
        {
            if ((++NumCasts & 1) == 0)
            {
                aetherdrainActive = false;
            }
            wantLight[0] = wantLight[1];
            activations[0] = activations[1];
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.LightVengeance:
                UpdateStatus(ref lightBuff, actor, status.ExpireAt);
                break;
            case (uint)SID.DarkVengeance:
                UpdateStatus(ref darkBuff, actor, status.ExpireAt);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.LightVengeance:
                UpdateStatus(ref lightBuff, actor, default);
                break;
            case (uint)SID.DarkVengeance:
                UpdateStatus(ref darkBuff, actor, default);
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.StateMachine.ActivePhaseIndex == -1)
        {
            var countL = lightBuff.NumSetBits();
            var countD = darkBuff.NumSetBits();
            if (!combined[slot])
            {
                if (combined.NumSetBits() == 0 || countL == 1 && countD == 1)
                {
                    hints.Add(either);
                }
                else if (countL > countD)
                {
                    hints.Add(dark);
                }
                else
                {
                    hints.Add(light);
                }
            }
            else if (darkBuff[slot] && countD > 2 || lightBuff[slot] && countL > 2)
            {
                hints.Add(switchColor);
            }
        }
        else if (aetherdrainActive)
        {
            if (wantLight[0])
            {
                hints.Add(light, !lightBuff[slot]);
            }
            else
            {
                hints.Add(dark, !darkBuff[slot]);
            }
        }
        else if (!aetherdrainActive)
        {
            if (eaterHP <= 1u && !lightBuff[slot])
            {
                hints.Add(light);
            }
            else if (griefHP <= 1u && !darkBuff[slot])
            {
                hints.Add(dark);
            }
            else if (Math.Abs(hpDifference) > 25f || !combined[slot])
            {
                if (hpDifference > 0f)
                {
                    hints.Add(dark);
                }
                else
                {
                    hints.Add(light);
                }
            }
            else
            {
                hints.Add($"Target {(darkBuff[slot] ? bossmod.BossEater?.Name : Module.PrimaryActor.Name)}!", false);
            }
        }
    }

    public override void Update()
    {
        if (bossmod.BossEater is Actor eater)
        {
            ref var eaterHPref = ref eater.HPMP;
            ref var primaryHPref = ref Module.PrimaryActor.HPMP;
            var eaterHPs = eaterHP = eaterHPref.CurHP;
            var griefHPs = griefHP = primaryHPref.CurHP;
            hpDifference = (int)(eaterHPs - griefHPs) * 100f / primaryHPref.MaxHP;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            var oid = e.Actor.OID;
            ref var hp = ref e.Actor.HPMP;
            e.Priority = hp.CurHP <= 1u ? AIHints.Enemy.PriorityInvincible : darkBuff[slot] && oid == (uint)OID.DevouredEater ? 0
                : lightBuff[slot] && oid == (uint)OID.EminentGrief ? 0 : oid == (uint)OID.VodorigaMinion ? 1 : AIHints.Enemy.PriorityInvincible;
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }
}
