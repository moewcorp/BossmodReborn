﻿namespace BossMod;

public record struct ClientActionRequest
(
    ActionID Action,
    ulong TargetID,
    Vector3 TargetPos,
    uint SourceSequence,
    float InitialAnimationLock,
    float InitialCastTimeElapsed,
    float InitialCastTimeTotal,
    float InitialRecastElapsed,
    float InitialRecastTotal
)
{
    public override readonly string ToString() => $"#{SourceSequence} {Action} @ {TargetID:X8} {TargetPos:f3}";
}

public record struct ClientActionReject(ActionID Action, uint SourceSequence, float RecastElapsed, float RecastTotal, uint LogMessageID)
{
    public override readonly string ToString() => $"#{SourceSequence} {Action} ({LogMessageID})";
}

public record struct Cooldown(float Elapsed, float Total)
{
    public readonly float Remaining => Total - Elapsed;

    public override readonly string ToString() => $"{Elapsed:f3}/{Total:f3}";
}

// client-specific state and events (action requests, gauge, etc)
// this is generally not available for non-player party members, but we can try to guess
public sealed class ClientState
{
    public readonly record struct Fate(uint ID, Vector3 Center, float Radius);
    public record struct Combo(uint Action, float Remaining);
    public record struct Gauge(ulong Low, ulong High);
    public record struct Stats(int SkillSpeed, int SpellSpeed, int Haste);
    public record struct Pet(ulong InstanceID, byte Order, byte Stance);
    public record struct DutyAction(ActionID Action, byte CurCharges, byte MaxCharges);

    public const int NumCooldownGroups = 82;
    public const int NumClassLevels = 32; // see ClassJob.ExpArrayIndex
    public const int NumBlueMageSpells = 24;

    public float? CountdownRemaining;
    public Angle CameraAzimuth; // updated every frame by the frame-start event
    public Gauge GaugePayload; // updated every frame by the frame-start event
    public float AnimationLock;
    public Combo ComboState;
    public Stats PlayerStats;
    public readonly Cooldown[] Cooldowns = new Cooldown[NumCooldownGroups];
    public readonly DutyAction[] DutyActions = new DutyAction[2];
    public readonly byte[] BozjaHolster = new byte[(int)BozjaHolsterID.Count]; // number of copies in holster per item
    public readonly uint[] BlueMageSpells = new uint[NumBlueMageSpells];
    public readonly short[] ClassJobLevels = new short[NumClassLevels];
    public Fate ActiveFate;
    public Pet ActivePet;
    public ulong FocusTargetId;
    public Angle ForcedMovementDirection; // used for temporary misdirection and spinning states
    public uint[] ContentKeyValueData = new uint[6]; // used for content-specific persistent player attributes, like bozja resistance rank

    public uint GetContentValue(uint key) => ContentKeyValueData[0] == key
        ? ContentKeyValueData[1]
        : ContentKeyValueData[2] == key
            ? ContentKeyValueData[3]
            : ContentKeyValueData[4] == key
                ? ContentKeyValueData[5]
                : 0;

    public uint ElementalLevel => GetContentValue(4);
    public uint ElementalLevelSynced => GetContentValue(2);
    public uint ResistanceRank => GetContentValue(5);

    public int ClassJobLevel(Class c)
    {
        var index = Service.LuminaRow<Lumina.Excel.Sheets.ClassJob>((uint)c)?.ExpArrayIndex ?? -1;
        return index >= 0 && index < ClassJobLevels.Length ? ClassJobLevels[index] : -1;
    }

    // TODO: think about how to improve it...
    public static unsafe T GetGauge<T>(in Gauge gauge) where T : unmanaged
    {
        T res = default;
        ((ulong*)&res)[1] = gauge.Low;
        if (sizeof(T) > 16)
            ((ulong*)&res)[2] = gauge.High;
        return res;
    }
    public unsafe T GetGauge<T>() where T : unmanaged => GetGauge<T>(GaugePayload);

    public List<WorldState.Operation> CompareToInitial()
    {
        List<WorldState.Operation> ops = new(13);
        if (CountdownRemaining != null)
            ops.Add(new OpCountdownChange(CountdownRemaining));

        if (AnimationLock != 0)
            ops.Add(new OpAnimationLockChange(AnimationLock));

        if (ComboState.Remaining != 0)
            ops.Add(new OpComboChange(ComboState));

        if (PlayerStats != default)
            ops.Add(new OpPlayerStatsChange(PlayerStats));

        var cdlen = Cooldowns.Length;
        if (cdlen != 0)
        {
            var cooldowns = new List<(int, Cooldown)>(cdlen);

            for (var i = 0; i < cdlen; ++i)
            {
                var cds = Cooldowns[i];
                if (cds.Total != 0)
                    cooldowns.Add((i, cds));
            }
            if (cooldowns.Count != 0)
                ops.Add(new OpCooldown(false, cooldowns));
        }
        if (DutyActions.Any(a => a != default))
            ops.Add(new OpDutyActionsChange(DutyActions[0], DutyActions[1]));

        var holsterlen = BozjaHolster.Length;
        if (holsterlen != 0)
        {
            var bozjaHolster = new List<(BozjaHolsterID, byte)>(holsterlen);
            for (var i = 0; i < holsterlen; ++i)
            {
                var holster = BozjaHolster[i];
                if (holster != 0)
                    bozjaHolster.Add(((BozjaHolsterID)i, holster));
            }
            var hasNonZeroHolster = false;
            for (var i = 0; i < holsterlen; ++i)
            {
                if (BozjaHolster[i] != 0)
                {
                    hasNonZeroHolster = true;
                    break;
                }
            }
            if (hasNonZeroHolster)
                ops.Add(new OpBozjaHolsterChange(bozjaHolster));
        }

        var hasNonZeroBMSpell = false;
        for (var i = 0; i < BlueMageSpells.Length; ++i)
        {
            if (BlueMageSpells[i] != 0)
            {
                hasNonZeroBMSpell = true;
                break;
            }
        }
        if (hasNonZeroBMSpell)
            ops.Add(new OpBlueMageSpellsChange(BlueMageSpells));

        var hasNonZeroLevels = false;
        for (var i = 0; i < ClassJobLevels.Length; ++i)
        {
            if (ClassJobLevels[i] != 0)
            {
                hasNonZeroLevels = true;
                break;
            }
        }
        if (hasNonZeroLevels)
            ops.Add(new OpClassJobLevelsChange(ClassJobLevels));

        if (ActiveFate.ID != 0)
            ops.Add(new OpActiveFateChange(ActiveFate));

        if (ActivePet.InstanceID != 0)
            ops.Add(new OpActivePetChange(ActivePet));

        if (FocusTargetId != 0)
            ops.Add(new OpFocusTargetChange(FocusTargetId));

        if (ForcedMovementDirection != default)
            ops.Add(new OpForcedMovementDirectionChange(ForcedMovementDirection));
        return ops;
    }

    public void Tick(float dt)
    {
        if (CountdownRemaining != null)
            CountdownRemaining = CountdownRemaining.Value - dt;

        if (AnimationLock > 0)
            AnimationLock = Math.Max(0, AnimationLock - dt);

        if (ComboState.Remaining > 0)
        {
            ComboState.Remaining -= dt;
            if (ComboState.Remaining <= 0)
                ComboState = default;
        }

        // TODO: update cooldowns only if 'timestop' status is not active...
        foreach (ref var cd in Cooldowns.AsSpan())
        {
            cd.Elapsed += dt;
            if (cd.Elapsed >= cd.Total)
                cd.Elapsed = cd.Total = 0;
        }
    }

    // implementation of operations
    public Event<OpActionRequest> ActionRequested = new();
    public sealed record class OpActionRequest(ClientActionRequest Request) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws) => ws.Client.ActionRequested.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLAR"u8)
            .Emit(Request.Action)
            .EmitActor(Request.TargetID)
            .Emit(Request.TargetPos)
            .Emit(Request.SourceSequence)
            .Emit(Request.InitialAnimationLock, "f3")
            .EmitFloatPair(Request.InitialCastTimeElapsed, Request.InitialCastTimeTotal)
            .EmitFloatPair(Request.InitialRecastElapsed, Request.InitialRecastTotal);
    }

    public Event<OpActionReject> ActionRejected = new();
    public sealed record class OpActionReject(ClientActionReject Value) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws) => ws.Client.ActionRejected.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLRJ"u8)
            .Emit(Value.Action)
            .Emit(Value.SourceSequence)
            .EmitFloatPair(Value.RecastElapsed, Value.RecastTotal)
            .Emit(Value.LogMessageID);
    }

    public Event<OpCountdownChange> CountdownChanged = new();
    public sealed record class OpCountdownChange(float? Value) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws)
        {
            ws.Client.CountdownRemaining = Value;
            ws.Client.CountdownChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            if (Value != null)
                output.EmitFourCC("CDN+"u8).Emit(Value.Value);
            else
                output.EmitFourCC("CDN-"u8);
        }
    }

    public Event<OpAnimationLockChange> AnimationLockChanged = new();
    public sealed record class OpAnimationLockChange(float Value) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws)
        {
            ws.Client.AnimationLock = Value;
            ws.Client.AnimationLockChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLAL"u8).Emit(Value);
    }

    public Event<OpComboChange> ComboChanged = new();
    public sealed record class OpComboChange(Combo Value) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws)
        {
            ws.Client.ComboState = Value;
            ws.Client.ComboChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLCB"u8).Emit(Value.Action).Emit(Value.Remaining);
    }

    public Event<OpPlayerStatsChange> PlayerStatsChanged = new();
    public sealed record class OpPlayerStatsChange(Stats Value) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws)
        {
            ws.Client.PlayerStats = Value;
            ws.Client.PlayerStatsChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLST"u8).Emit(Value.SkillSpeed).Emit(Value.SpellSpeed).Emit(Value.Haste);
    }

    public Event<OpCooldown> CooldownsChanged = new();
    public sealed record class OpCooldown(bool Reset, List<(int group, Cooldown value)> Cooldowns) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws)
        {
            if (Reset)
                Array.Fill(ws.Client.Cooldowns, default);
            var count = Cooldowns.Count;
            for (var i = 0; i < count; ++i)
            {
                var cd = Cooldowns[i];
                ws.Client.Cooldowns[cd.group] = cd.value;
            }
            ws.Client.CooldownsChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            var count = Cooldowns.Count;
            output.EmitFourCC("CLCD"u8);
            output.Emit(Reset);
            output.Emit((byte)count);
            for (var i = 0; i < count; ++i)
            {
                var cd = Cooldowns[i];
                output.Emit((byte)cd.group).Emit(cd.value.Elapsed).Emit(cd.value.Total);
            }
        }
    }

    public Event<OpDutyActionsChange> DutyActionsChanged = new();
    public sealed record class OpDutyActionsChange(DutyAction Slot0, DutyAction Slot1) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws)
        {
            ws.Client.DutyActions[0] = Slot0;
            ws.Client.DutyActions[1] = Slot1;
            ws.Client.DutyActionsChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLDA"u8).Emit(Slot0.Action).Emit(Slot0.CurCharges).Emit(Slot0.MaxCharges).Emit(Slot1.Action).Emit(Slot1.CurCharges).Emit(Slot1.MaxCharges);
    }

    public Event<OpBozjaHolsterChange> BozjaHolsterChanged = new();
    public sealed record class OpBozjaHolsterChange(List<(BozjaHolsterID entry, byte count)> Contents) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws)
        {
            Array.Fill(ws.Client.BozjaHolster, (byte)0);
            for (var i = 0; i < Contents.Count; ++i)
            {
                var e = Contents[i];
                ws.Client.BozjaHolster[(int)e.entry] = e.count;
            }
            ws.Client.BozjaHolsterChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            var count = Contents.Count;
            output.EmitFourCC("CLBH"u8);
            output.Emit((byte)count);
            for (var i = 0; i < count; ++i)
            {
                var e = Contents[i];
                output.Emit((byte)e.entry).Emit(e.count);
            }
        }
    }

    public Event<OpBlueMageSpellsChange> BlueMageSpellsChanged = new();
    public sealed record class OpBlueMageSpellsChange(uint[] Values) : WorldState.Operation
    {
        public readonly uint[] Values = Values;

        protected override void Exec(ref WorldState ws)
        {
            Array.Copy(Values, ws.Client.BlueMageSpells, NumBlueMageSpells);
            ws.Client.BlueMageSpellsChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            var len = Values.Length;
            output.EmitFourCC("CBLU"u8);
            output.Emit((byte)Values.Length);
            for (var i = 0; i < len; ++i)
            {
                output.Emit(Values[i]);
            }
        }
    }

    public Event<OpClassJobLevelsChange> ClassJobLevelsChanged = new();
    public sealed record class OpClassJobLevelsChange(short[] Values) : WorldState.Operation
    {
        public readonly short[] Values = Values;

        protected override void Exec(ref WorldState ws)
        {
            Array.Fill(ws.Client.ClassJobLevels, (short)0);
            for (var i = 0; i < Values.Length; ++i)
                ws.Client.ClassJobLevels[i] = Values[i];
            ws.Client.ClassJobLevelsChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            var len = Values.Length;
            output.EmitFourCC("CLVL"u8);
            output.Emit((byte)len);
            for (var i = 0; i < len; ++i)
            {
                output.Emit(Values[i]);
            }
        }
    }

    public Event<OpActiveFateChange> ActiveFateChanged = new();
    public sealed record class OpActiveFateChange(Fate Value) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws)
        {
            ws.Client.ActiveFate = Value;
            ws.Client.ActiveFateChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLAF"u8).Emit(Value.ID).Emit(Value.Center).Emit(Value.Radius, "f3");
    }

    public Event<OpActivePetChange> ActivePetChanged = new();
    public sealed record class OpActivePetChange(Pet Value) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws)
        {
            ws.Client.ActivePet = Value;
            ws.Client.ActivePetChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CPET"u8).Emit(Value.InstanceID, "X8").Emit(Value.Order).Emit(Value.Stance);
    }

    public Event<OpFocusTargetChange> FocusTargetChanged = new();
    public sealed record class OpFocusTargetChange(ulong Value) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws)
        {
            ws.Client.FocusTargetId = Value;
            ws.Client.FocusTargetChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLFT"u8).Emit(Value, "X8");
    }

    public Event<OpForcedMovementDirectionChange> ForcedMovementDirectionChanged = new();
    public sealed record class OpForcedMovementDirectionChange(Angle Value) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws)
        {
            ws.Client.ForcedMovementDirection = Value;
            ws.Client.ForcedMovementDirectionChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("CLFD"u8).Emit(Value);
    }

    public Event<OpContentKVDataChange> ContentKVDataChanged = new();
    public sealed record class OpContentKVDataChange(uint[] Value) : WorldState.Operation
    {
        public readonly uint[] Value = Value;
        protected override void Exec(ref WorldState ws)
        {
            ws.Client.ContentKeyValueData = Value;
            ws.Client.ContentKVDataChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("CLKV"u8);
            var len = Value.Length;
            for (var i = 0; i < len; ++i)
                output.Emit(Value[i]);
        }
    }

    public Event<OpFateInfo> FateInfo = new();
    public sealed record class OpFateInfo(uint FateId, DateTime StartTime) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws) => ws.Client.FateInfo.Fire(this);
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("FATE"u8).Emit(FateId).Emit(StartTime.Ticks);
    }
}
