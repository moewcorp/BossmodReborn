﻿using RoomFlags = FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon.RoomFlags;

namespace BossMod;

public sealed class DeepDungeonState
{
    public enum DungeonType : byte
    {
        None = 0,
        POTD = 1,
        HOH = 2,
        EO = 3
    }

    public readonly record struct DungeonProgress(byte Floor, byte Tileset, byte WeaponLevel, byte ArmorLevel, byte SyncedGearLevel, byte HoardCount, byte ReturnProgress, byte PassageProgress);
    public readonly record struct PartyMember(ulong EntityId, byte Room);
    public readonly record struct PomanderState(byte Count, byte Flags)
    {
        public readonly bool Usable => (Flags & (1 << 0)) != 0;
        public readonly bool Active => (Flags & (1 << 1)) != 0;
    }
    public readonly record struct Chest(byte Type, byte Room);

    public const int NumRooms = 25;
    public const int NumPartyMembers = 4;
    public const int NumPomanderSlots = 16;
    public const int NumChests = 16;
    public const int NumMagicites = 3;

    public DungeonType DungeonId;
    public DungeonProgress Progress;
    public readonly RoomFlags[] Rooms = new RoomFlags[NumRooms];
    public readonly PartyMember[] Party = new PartyMember[NumPartyMembers];
    public readonly PomanderState[] Pomanders = new PomanderState[NumPomanderSlots];
    public readonly Chest[] Chests = new Chest[NumChests];
    public readonly byte[] Magicite = new byte[NumMagicites];

    public bool ReturnActive => Progress.ReturnProgress >= 11;
    public bool PassageActive => Progress.PassageProgress >= 11;
    public byte Floor => Progress.Floor;
    public bool IsBossFloor => Progress.Floor % 10 == 0;

    public Lumina.Excel.Sheets.DeepDungeon GetDungeonDefinition() => Service.LuminaRow<Lumina.Excel.Sheets.DeepDungeon>((uint)DungeonId)!.Value;
    public int GetPomanderSlot(PomanderID pid) => GetDungeonDefinition().PomanderSlot.FindIndex(p => p.RowId == (uint)pid);
    public PomanderState GetPomanderState(PomanderID pid) => GetPomanderSlot(pid) is var s && s >= 0 ? Pomanders[s] : default;
    public PomanderID GetPomanderID(int slot) => GetDungeonDefinition().PomanderSlot is var slots && slot >= 0 && slot < slots.Count ? (PomanderID)slots[slot].RowId : PomanderID.None;

    public List<WorldState.Operation> CompareToInitial()
    {
        var ops = new List<WorldState.Operation>(6);
        if (DungeonId != DungeonType.None)
        {
            ops.Add(new OpProgressChange(DungeonId, Progress));
            ops.Add(new OpMapDataChange(Rooms));
            ops.Add(new OpPartyStateChange(Party));
            ops.Add(new OpPomandersChange(Pomanders));
            ops.Add(new OpChestsChange(Chests));
            ops.Add(new OpMagiciteChange(Magicite));
        }
        return ops;
    }

    public Event<OpProgressChange> ProgressChanged = new();
    public sealed record class OpProgressChange(DungeonType DungeonId, DungeonProgress Value) : WorldState.Operation
    {
        protected override void Exec(ref WorldState ws)
        {
            ws.DeepDungeon.DungeonId = DungeonId;
            ws.DeepDungeon.Progress = Value;
            ws.DeepDungeon.ProgressChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDPG"u8)
                .Emit((byte)DungeonId)
                .Emit(Value.Floor)
                .Emit(Value.Tileset)
                .Emit(Value.WeaponLevel)
                .Emit(Value.ArmorLevel)
                .Emit(Value.SyncedGearLevel)
                .Emit(Value.HoardCount)
                .Emit(Value.ReturnProgress)
                .Emit(Value.PassageProgress);
        }
    }

    public Event<OpMapDataChange> MapDataChanged = new();
    public sealed record class OpMapDataChange(RoomFlags[] Rooms) : WorldState.Operation
    {
        public readonly RoomFlags[] Rooms = Rooms;

        protected override void Exec(ref WorldState ws)
        {
            Array.Copy(Rooms, ws.DeepDungeon.Rooms, NumRooms);
            ws.DeepDungeon.MapDataChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDMP"u8);
            foreach (var r in Rooms)
                output.Emit((byte)r, "X2");
        }
    }

    public Event<OpPartyStateChange> PartyStateChanged = new();
    public sealed record class OpPartyStateChange(PartyMember[] Value) : WorldState.Operation
    {
        public readonly PartyMember[] Value = Value;

        protected override void Exec(ref WorldState ws)
        {
            Array.Copy(Value, ws.DeepDungeon.Party, NumPartyMembers);
            ws.DeepDungeon.PartyStateChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDPT"u8);
            foreach (var member in Value)
                output.EmitActor(member.EntityId).Emit(member.Room);
        }
    }

    public Event<OpPomandersChange> PomandersChanged = new();
    public sealed record class OpPomandersChange(PomanderState[] Value) : WorldState.Operation
    {
        public readonly PomanderState[] Value = Value;

        protected override void Exec(ref WorldState ws)
        {
            Array.Copy(Value, ws.DeepDungeon.Pomanders, NumPomanderSlots);
            ws.DeepDungeon.PomandersChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDIT"u8);
            foreach (var item in Value)
                output.Emit(item.Count).Emit(item.Flags, "X");
        }
    }

    public Event<OpChestsChange> ChestsChanged = new();
    public sealed record class OpChestsChange(Chest[] Value) : WorldState.Operation
    {
        public readonly Chest[] Value = Value;

        protected override void Exec(ref WorldState ws)
        {
            Array.Copy(Value, ws.DeepDungeon.Chests, NumChests);
            ws.DeepDungeon.ChestsChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDCT"u8);
            foreach (var chest in Value)
                output.Emit(chest.Type).Emit(chest.Room);
        }
    }

    public Event<OpMagiciteChange> MagiciteChanged = new();
    public sealed record class OpMagiciteChange(byte[] Value) : WorldState.Operation
    {
        public readonly byte[] Value = Value;

        protected override void Exec(ref WorldState ws)
        {
            Array.Copy(Value, ws.DeepDungeon.Magicite, NumMagicites);
            ws.DeepDungeon.MagiciteChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output)
        {
            output.EmitFourCC("DDMG"u8).Emit(Value);
        }
    }
}
