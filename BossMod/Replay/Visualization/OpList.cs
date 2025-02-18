﻿using ImGuiNET;
using System.IO;

namespace BossMod.ReplayVisualization;

class OpList(Replay replay, Replay.Encounter? enc, BossModuleRegistry.Info? moduleInfo, IEnumerable<WorldState.Operation> ops, Action<DateTime> scrollTo)
{
    public readonly Replay.Encounter? Encounter = enc;
    public readonly BossModuleRegistry.Info? ModuleInfo = moduleInfo;
    private DateTime _relativeTS;
    private readonly List<(int Index, DateTime Timestamp, string Text, Action<UITree>? Children, Action? ContextMenu)> _nodes = [];
    private readonly HashSet<uint> _filteredOIDs = [];
    public static readonly HashSet<uint> BoringOIDs = [0x3E1A, 0x3E1B, 0x3E1C, 0x447E, 0x447D, 0x4480, 0x4583, 0x447F, 0x4584, 0x4570, 0x4256, 0x260E, 0x260B,
    0x2630, 0x2611, 0x2610, 0x2617, 0x2608, 0x2613, 0x2618, 0x2609, 0x261A, 0x262F, 0x2609, 0x2614, 0x2664, 0x2668, 0x2619, 0x2631, 0x2632, 0x260A, 0x2616, 0x2667,
    0x2E7F, 0x2F33, 0x2F32, 0x2F38, 0x2E80, 0x2E82, 0x2E81, 0x2F36, 0x2E7D, 0x2F35, 0x2EB0, 0x2F31, 0x2F37, 0x2E7C, 0x2E7B, 0x2EAE, 0x2F3A, 0x2F30, 0x2E7E, 0x2EAF,
    0x428B, 0x44B8, 0x43D2, 0x43D1, 0x41FD, 0x42A4, 0x41C5, 0x30B7, 0x4021, 0x4019, 0x401C, 0x401B, 0x401F, 0x40FB, 0x4105, 0x401D, 0x4102, 0x4629, 0x4628, 0x4631,
    0x4630, 0x46D6, 0xF5B, 0xF5C, 0x2E20, 0x2E21, 0x318A, 0x2E1E, 0x3346, 0x3353, 0x31D4, 0x3345, 0x3355, 0x3326, 0x3344, 0x31B1, 0x3343, 0x1EB165, 0x1EB166,
    0x1EB167, 0x1EB168, 0x4339, 0x4144, 0x4146, 0x4348, 0x4339, 0x4337, 0x35F5, 0x3226, 0x35FA, 0x35F6, 0x35F9, 0x35F7, 0x361A, 0x35F5, 0x34A4, 0x35F4, 0x3605, 0x35F2,
    0x375C, 0x375A, 0x3759, 0x375B, 0x35E0, 0x35E1, 0x35F1, 0x35F3, 0x3604, 0x39BF, 0x39BD, 0x39C0, 0x39C1, 0x39BE, 0x402D, 0x402E, 0x40B1, 0x3D7F, 0x3D80, 0x3D7E, 0x465C, 0x465D, 0x465E,
    0x466D, 0x466E, 0x466F, 0x466B, 0x466C];
    public static readonly HashSet<uint> BoringSIDs = [43, 44, 418, 364, 902, 414, 1050, 368, 362, 1086, 1461, 1463, 365, 1778, 1755, 360, 1411, 2625, 2626, 2627, 2415, 2449, 361, 367, 2355, 413];
    private readonly HashSet<ActionID> _filteredActions = [];
    private readonly HashSet<uint> _filteredStatuses = [];
    private readonly HashSet<uint> _filteredDirectorUpdateTypes = [];
#pragma warning disable IDE0032
    private bool _showActorSizeEvents;
#pragma warning restore IDE0032
    private bool _nodesUpToDate;

    public bool ShowActorSizeEvents
    {
        get => _showActorSizeEvents;
        set
        {
            _showActorSizeEvents = value;
            _nodesUpToDate = false;
        }
    }

    public void Draw(UITree tree, DateTime reference)
    {
        //foreach (var n in _tree.Node("Settings"))
        //{
        //    DrawSettings();
        //}

        if (!_nodesUpToDate)
        {
            _nodes.Clear();
            var i = 0;
            foreach (var op in ops)
            {
                if (FilterOp(op))
                {
                    _nodes.Add((i, op.Timestamp, OpName(op), OpChildren(op), OpContextMenu(op)));
                }
                ++i;
            }
            _nodesUpToDate = true;
        }

        var timeRef = ImGui.GetIO().KeyShift && _relativeTS != default ? _relativeTS : reference;
        foreach (var node in _nodes)
        {
            foreach (var n in tree.Node($"{(node.Timestamp - timeRef).TotalSeconds:f3}: {node.Text}###{node.Index}", node.Children == null, Colors.TextColor1, node.ContextMenu, () => scrollTo(node.Timestamp), () => _relativeTS = node.Timestamp))
            {
                node.Children?.Invoke(tree);
            }
        }
    }

    public void ClearFilters()
    {
        _filteredOIDs.Clear();
        _filteredActions.Clear();
        _filteredStatuses.Clear();
        _filteredDirectorUpdateTypes.Clear();
        _nodesUpToDate = false;
    }

    private bool FilterInterestingActor(ulong instanceID, DateTime timestamp, bool allowPlayers)
    {
        var p = replay.FindParticipant(instanceID, timestamp)!;
        if ((p.OwnerID & 0xFF000000) == 0x10000000 && p.Type != ActorType.Buddy)
            return false; // player's pet/area
        return (p.Type is not ActorType.Player and not ActorType.Buddy and not ActorType.Pet || allowPlayers) && !_filteredOIDs.Contains(p.OID) && !BoringOIDs.Contains(p.OID);
    }

    private bool FilterInterestingStatus(Replay.Status s)
    {
        if (s.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.Buddy)
            return false; // don't care about statuses applied by players
        if (s.Target.Type is ActorType.Pet)
            return false; // don't care about statuses applied to pets
        if (BoringSIDs.Contains(s.ID))
            return false; // don't care about resurrect-related and other trivial statuses
        if (_filteredOIDs.Contains(s.Target.OID))
            return false; // don't care about filtered out targets
        if (_filteredStatuses.Contains(s.ID))
            return false; // don't care about filtered out statuses
        return true;
    }

    private bool FilterInterestingStatuses(ulong instanceID, int index, DateTime timestamp) => FindStatuses(instanceID, index, timestamp).Any(FilterInterestingStatus);

    private bool FilterOp(WorldState.Operation o)
    {
        return o switch
        {
            WorldState.OpFrameStart => false,
            WorldState.OpDirectorUpdate op => !_filteredDirectorUpdateTypes.Contains(op.UpdateID),
            ActorState.OpCreate op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpDestroy op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpMove => false,
            ActorState.OpSizeChange op => _showActorSizeEvents && FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpHPMP => false,
            ActorState.OpTargetable op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpDead op => FilterInterestingActor(op.InstanceID, op.Timestamp, true),
            ActorState.OpCombat => false,
            ActorState.OpAggroPlayer => false,
            ActorState.OpEventState op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpTarget op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpCastInfo op => FilterInterestingActor(op.InstanceID, op.Timestamp, false) && !_filteredActions.Contains(FindCast(replay.FindParticipant(op.InstanceID, op.Timestamp), op.Timestamp, op.Value != null)?.ID ?? new()),
            ActorState.OpCastEvent op => FilterInterestingActor(op.InstanceID, op.Timestamp, false) && !_filteredActions.Contains(op.Value.Action),
            ActorState.OpEffectResult => false,
            ActorState.OpStatus op => FilterInterestingStatuses(op.InstanceID, op.Index, op.Timestamp) && FilterInterestingActor(op.InstanceID, op.Timestamp, true),
            ActorState.OpIncomingEffect => false,
            PartyState.OpLimitBreakChange => false,
            ActorState.OpEventNpcYell op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpEventObjectStateChange op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpEventObjectAnimation op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpRename op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
            ActorState.OpIcon op => FilterInterestingActor(op.InstanceID, op.Timestamp, true),
            ActorState.OpTether op => FilterInterestingActor(op.InstanceID, op.Timestamp, true),
            ClientState.OpActionRequest => false,
            //ClientState.OpActionReject => false,
            ClientState.OpAnimationLockChange => false,
            ClientState.OpComboChange => false,
            ClientState.OpCooldown => false,
            ClientState.OpForcedMovementDirectionChange => false,
            NetworkState.OpServerIPC => false,
            _ => true
        };
    }

    private string DumpOp(WorldState.Operation op)
    {
        using var stream = new MemoryStream(1024);
        var writer = new ReplayRecorder.TextOutput(stream, null);
        op.Write(writer);
        writer.Flush();
        stream.Position = 0;
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        var start = Array.IndexOf(bytes, (byte)'|') + 1;
        return Encoding.UTF8.GetString(bytes, start, bytes.Length - start);
    }

    private string OpName(WorldState.Operation o)
    {
        return o switch
        {
            ActorState.OpCreate op => $"Actor create: {ActorString(op.InstanceID, op.Timestamp)} #{op.SpawnIndex}",
            ActorState.OpDestroy op => $"Actor destroy: {ActorString(op.InstanceID, op.Timestamp)}",
            ActorState.OpRename op => $"Actor rename: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Name}",
            ActorState.OpClassChange op => $"Actor class change: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Class} L{op.Level}",
            ActorState.OpTargetable op => $"{(op.Value ? "Targetable" : "Untargetable")}: {ActorString(op.InstanceID, op.Timestamp)}",
            ActorState.OpDead op => $"{(op.Value ? "Die" : "Resurrect")}: {ActorString(op.InstanceID, op.Timestamp)}",
            ActorState.OpAggroPlayer op => $"Aggro player: {ActorString(op.InstanceID, op.Timestamp)} = {op.Has}",
            ActorState.OpEventState op => $"Event state: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Value}",
            ActorState.OpTarget op => $"Target: {ActorString(op.InstanceID, op.Timestamp)} -> {ActorString(op.Value, op.Timestamp)}",
            ActorState.OpMount op => $"Mount: {ActorString(op.InstanceID, op.Timestamp)} = {Service.LuminaRow<Lumina.Excel.Sheets.Mount>(op.Value)?.Singular ?? "<unknown>"}",
            ActorState.OpTether op => $"Tether: {ActorString(op.InstanceID, op.Timestamp)} {op.Value.ID} ({ModuleInfo?.TetherIDType?.GetEnumName(op.Value.ID)}) @ {ActorString(op.Value.Target, op.Timestamp)}",
            ActorState.OpCastInfo op => $"Cast {(op.Value != null ? "started" : "ended")}: {CastString(op.InstanceID, op.Timestamp, op.Value != null)}",
            ActorState.OpCastEvent op => $"Cast event: {ActorString(op.InstanceID, op.Timestamp)}: {op.Value.Action} ({ModuleInfo?.ActionIDType?.GetEnumName(op.Value.Action.ID)}) @ {CastEventTargetString(op.Value, op.Timestamp)} ({op.Value.Targets.Count} targets affected) #{op.Value.GlobalSequence}",
            ActorState.OpStatus op => $"Status change: {ActorString(op.InstanceID, op.Timestamp)} #{op.Index}: {StatusesString(op.InstanceID, op.Index, op.Timestamp)}",
            ActorState.OpIcon op => $"Icon: {ActorString(op.InstanceID, op.Timestamp)} -> {ActorString(op.TargetID, op.Timestamp)}: {op.IconID} ({ModuleInfo?.IconIDType?.GetEnumName(op.IconID)})",
            ActorState.OpEventObjectStateChange op => $"EObjState: {ActorString(op.InstanceID, op.Timestamp)} = {op.State:X4}",
            ActorState.OpEventObjectAnimation op => $"EObjAnim: {ActorString(op.InstanceID, op.Timestamp)} = {((uint)op.Param1 << 16) | op.Param2:X8}",
            ActorState.OpPlayActionTimelineEvent op => $"Play action timeline: {ActorString(op.InstanceID, op.Timestamp)} = {op.ActionTimelineID:X4}",
            ActorState.OpEventNpcYell op => $"Yell: {ActorString(op.InstanceID, op.Timestamp)} = {op.Message} '{Service.LuminaRow<Lumina.Excel.Sheets.NpcYell>(op.Message)?.Text}'",
            ClientState.OpDutyActionsChange op => $"Player duty actions change: {op.Slot0}, {op.Slot1}",
            ClientState.OpBozjaHolsterChange op => $"Player bozja holster change: {string.Join(", ", op.Contents.Select(e => $"{e.count}x {e.entry}"))}",
            _ => DumpOp(o)
        };
    }

    private Action<UITree>? OpChildren(WorldState.Operation o)
    {
        return o switch
        {
            ActorState.OpCastEvent op => op.Value.Targets.Count != 0 ? tree => DrawEventCast(tree, op) : null,
            _ => null
        };
    }

    private void DrawEventCast(UITree tree, ActorState.OpCastEvent op)
    {
        var action = replay.Actions.Find(a => a.GlobalSequence == op.Value.GlobalSequence);
        if (action != null && action.Timestamp == op.Timestamp && action.Source.InstanceID == op.InstanceID)
        {
            foreach (var t in tree.Nodes(action.Targets, t => new(ReplayUtils.ActionTargetString(t, op.Timestamp))))
            {
                tree.LeafNodes(t.Effects, ReplayUtils.ActionEffectString);
            }
        }
        else
        {
            foreach (var t in tree.Nodes(op.Value.Targets, t => new(ActorString(t.ID, op.Timestamp))))
            {
                tree.LeafNodes(t.Effects, ReplayUtils.ActionEffectString);
            }
        }
    }

    private Action? OpContextMenu(WorldState.Operation o)
    {
        return o switch
        {
            WorldState.OpDirectorUpdate op => () => ContextMenuDirectorUpdate(op),
            ActorState.OpStatus op => () => ContextMenuActorStatus(op),
            ActorState.OpCastInfo op => () => ContextMenuActorCast(op),
            ActorState.OpCastEvent op => () => ContextMenuEventCast(op),
            ActorState.Operation op => () => ContextMenuActor(op),
            _ => null,
        };
    }

    private void ContextMenuDirectorUpdate(WorldState.OpDirectorUpdate op)
    {
        if (ImGui.MenuItem($"Filter out type {op.UpdateID:X8}"))
        {
            _filteredDirectorUpdateTypes.Add(op.UpdateID);
            _nodesUpToDate = false;
        }
    }

    private void ContextMenuActor(ActorState.Operation op)
    {
        var oid = replay.FindParticipant(op.InstanceID, op.Timestamp)!.OID;
        if (ImGui.MenuItem($"Filter out OID {oid:X}"))
        {
            _filteredOIDs.Add(oid);
            _nodesUpToDate = false;
        }
    }

    private void ContextMenuActorStatus(ActorState.OpStatus op)
    {
        ContextMenuActor(op);
        foreach (var s in FindStatuses(op.InstanceID, op.Index, op.Timestamp))
        {
            if (ImGui.MenuItem($"Filter out {Utils.StatusString(s.ID)}"))
            {
                _filteredStatuses.Add(s.ID);
                _nodesUpToDate = false;
            }
        }
    }

    private void ContextMenuActorCast(ActorState.OpCastInfo op)
    {
        ContextMenuActor(op);
        var cast = FindCast(replay.FindParticipant(op.InstanceID, op.Timestamp), op.Timestamp, op.Value != null);
        if (cast != null && ImGui.MenuItem($"Filter out {cast.ID}"))
        {
            _filteredActions.Add(cast.ID);
            _nodesUpToDate = false;
        }
    }

    private void ContextMenuEventCast(ActorState.OpCastEvent op)
    {
        ContextMenuActor(op);
        if (ImGui.MenuItem($"Filter out {op.Value.Action}"))
        {
            _filteredActions.Add(op.Value.Action);
            _nodesUpToDate = false;
        }
    }

    private IEnumerable<Replay.Status> FindStatuses(ulong instanceID, int index, DateTime timestamp) => replay.Statuses.Where(s => s.Target.InstanceID == instanceID && s.Index == index && (s.Time.Start == timestamp || s.Time.End == timestamp));
    private Replay.Cast? FindCast(Replay.Participant? participant, DateTime timestamp, bool start) => participant?.Casts.Find(c => (start ? c.Time.Start : c.Time.End) == timestamp);

    private string ActorString(Replay.Participant? p, DateTime timestamp)
        => p != null ? $"{ReplayUtils.ParticipantString(p, timestamp)} ({ModuleInfo?.ObjectIDType?.GetEnumName(p.OID)}) {Utils.PosRotString(p.PosRotAt(timestamp))}" : "<none>";

    private string ActorString(ulong instanceID, DateTime timestamp)
    {
        var p = replay.FindParticipant(instanceID, timestamp);
        return p != null || instanceID == 0 ? ActorString(p, timestamp) : $"<unknown> {instanceID:X}";
    }

    private string CastEventTargetString(ActorCastEvent ev, DateTime timestamp) => $"{ActorString(ev.MainTargetID, timestamp)} / {Utils.Vec3String(ev.TargetPos)} / {ev.Rotation}";

    private string CastString(ulong instanceID, DateTime timestamp, bool start)
    {
        var p = replay.FindParticipant(instanceID, timestamp);
        var c = FindCast(p, timestamp, start);
        if (c == null)
            return $"{ActorString(p, timestamp)}: <unknown cast>";
        return $"{ActorString(p, timestamp)}: {c.ID} ({ModuleInfo?.ActionIDType?.GetEnumName(c.ID.ID)}), {c.ExpectedCastTime:f2}s ({c.Time} actual){(c.Interruptible ? " (interruptible)" : "")} @ {ReplayUtils.ParticipantPosRotString(c.Target, timestamp)} / {Utils.Vec3String(c.Location)} / {c.Rotation}";
    }

    private string StatusesString(ulong instanceID, int index, DateTime timestamp)
    {
        IEnumerable<string> Classify(Replay.Status s)
        {
            if (s.Time.Start == timestamp)
                yield return "gain";
            if (s.Time.End == timestamp)
                yield return "lose";
        }
        return string.Join("; ", FindStatuses(instanceID, index, timestamp).Select(s => $"{string.Join("/", Classify(s))} {Utils.StatusString(s.ID)} ({ModuleInfo?.StatusIDType?.GetEnumName(s.ID)}) ({s.StartingExtra:X}), {s.InitialDuration:f2}s / {s.Time}, from {ActorString(s.Source, timestamp)}"));
    }
}
