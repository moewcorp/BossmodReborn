﻿using BossMod.Network.ServerIPC;
using Dalamud.Memory;

namespace BossMod.Network;

public abstract unsafe class PacketDecoder
{
    protected DateTime Now;
    private const float Hundredth = 1e-2f;
    private const float Thousandth = 1e-3f;
    private const float TwoKD65k = (float)(2000d / 65535);
    private const float Inv65kDoublePI = (float)(1d / 65535 * 2 * Math.PI);

    public class TextNode(string text)
    {
        public string Text = text;
        public List<TextNode>? Children;

        public TextNode AddChild(TextNode child)
        {
            Children ??= [];
            Children.Add(child);
            return child;
        }

        public TextNode AddChild(string text) => AddChild(new TextNode(text));
    }

    public TextNode Decode(NetworkState.ServerIPC ipc, DateTime now)
    {
        Now = now;
        var sb = new StringBuilder($"Server IPC {ipc.ID} [0x{ipc.Opcode:X4}]: {DecodeActor(ipc.SourceServerActor)}, sent {(now - ipc.SendTimestamp).TotalMilliseconds:f3}ms ago, epoch={ipc.Epoch}, data=");
        for (var i = 0; i < ipc.Payload.Length; ++i)
            sb.Append($"{ipc.Payload[i]:X2}");
        var node = new TextNode(sb.ToString());
        byte[] payloadArray = [.. ipc.Payload];
        fixed (byte* payloadPtr = payloadArray)
        {
            var child = DecodePacket(ipc.ID, payloadPtr);
            if (child != null)
                node.AddChild(child);
        }
        return node;
    }

    public void LogNode(TextNode n, string prefix)
    {
        Service.Log($"[Network] {prefix}{n.Text}");
        if (n.Children == null)
            return;
        var subPrefix = prefix.Length > 0 ? "-" + prefix : "- ";
        foreach (var c in n.Children)
            LogNode(c, subPrefix);
    }

    protected abstract string DecodeActor(ulong instanceID);

    private TextNode? DecodePacket(PacketID id, byte* payload) => id switch
    {
        PacketID.RSVData when (RSVData*)payload is var p => new($"{MemoryHelper.ReadStringNullTerminated((nint)p->Key)} = {MemoryHelper.ReadString((nint)p->Value, p->ValueLength)} [{p->ValueLength}]"),
        PacketID.Countdown when (Countdown*)payload is var p => new($"{p->Time}s from {DecodeActor(p->SenderID)}{(p->FailedInCombat != 0 ? " fail-in-combat" : "")} '{MemoryHelper.ReadStringNullTerminated((nint)p->Text)}' u={p->u4:X4} {p->u9:X2} {p->u10:X2}"),
        PacketID.CountdownCancel when (CountdownCancel*)payload is var p => new($"from {DecodeActor(p->SenderID)} '{MemoryHelper.ReadStringNullTerminated((nint)p->Text)}' u={p->u4:X4} {p->u6:X4}"),
        PacketID.MarketBoardItemListingCount when (MarketBoardItemListingCount*)payload is var p => new($"{p->NumItems} items, {p->Error:X} error"),
        PacketID.MarketBoardItemListing when (MarketBoardItemListing*)payload is var p => DecodeMarketBoardItemListing(p),
        PacketID.MarketBoardPurchase when (MarketBoardPurchase*)payload is var p => new($"{p->Quantity}x {p->ItemId} (stackable={p->Stackable}), error={Utils.LogMessageString(p->ErrorLogId)}"),
        PacketID.MarketBoardSale when (MarketBoardSale*)payload is var p => new($"{p->Quantity}x {p->ItemId} at {p->UnitPrice} minus {p->TotalTax} tax in {p->TownId} town; type={p->SaleType}"),
        PacketID.MarketBoardItemListingHistory when (MarketBoardItemListingHistory*)payload is var p => DecodeMarketBoardItemHistory(p),
        PacketID.RetainerState when (RetainerState*)payload is var p => new($"{p->RetainerId:X} '{MemoryHelper.ReadStringNullTerminated((nint)p->Name)}', change={p->StateChange}, flags={p->Flags:X16} (town={p->Town}, selling={p->IsSelling}), custom message={Utils.LogMessageString(p->CustomMessageId)}"),
        PacketID.StatusEffectList when (StatusEffectList*)payload is var p => DecodeStatusEffectList(p),
        PacketID.StatusEffectListEureka when (StatusEffectListEureka*)payload is var p => DecodeStatusEffectList(&p->Data, $", rank={p->Rank}/{p->Element}/{p->u2}, pad={p->pad3:X2}"),
        PacketID.StatusEffectListBozja when (StatusEffectListBozja*)payload is var p => DecodeStatusEffectList(&p->Data, $", rank={p->Rank}, pad={p->pad1:X2}{p->pad2:X4}"),
        PacketID.StatusEffectListDouble when (StatusEffectListDouble*)payload is var p => DecodeStatusEffectListDouble(p),
        PacketID.EffectResult1 when (EffectResultN*)payload is var p => DecodeEffectResult((EffectResultEntry*)p->Entries, Math.Min((int)p->NumEntries, 1)),
        PacketID.EffectResult4 when (EffectResultN*)payload is var p => DecodeEffectResult((EffectResultEntry*)p->Entries, Math.Min((int)p->NumEntries, 4)),
        PacketID.EffectResult8 when (EffectResultN*)payload is var p => DecodeEffectResult((EffectResultEntry*)p->Entries, Math.Min((int)p->NumEntries, 8)),
        PacketID.EffectResult16 when (EffectResultN*)payload is var p => DecodeEffectResult((EffectResultEntry*)p->Entries, Math.Min((int)p->NumEntries, 16)),
        PacketID.EffectResultBasic1 when (EffectResultBasicN*)payload is var p => DecodeEffectResultBasic((EffectResultBasicEntry*)p->Entries, Math.Min((int)p->NumEntries, 1)),
        PacketID.EffectResultBasic4 when (EffectResultBasicN*)payload is var p => DecodeEffectResultBasic((EffectResultBasicEntry*)p->Entries, Math.Min((int)p->NumEntries, 4)),
        PacketID.EffectResultBasic8 when (EffectResultBasicN*)payload is var p => DecodeEffectResultBasic((EffectResultBasicEntry*)p->Entries, Math.Min((int)p->NumEntries, 8)),
        PacketID.EffectResultBasic16 when (EffectResultBasicN*)payload is var p => DecodeEffectResultBasic((EffectResultBasicEntry*)p->Entries, Math.Min((int)p->NumEntries, 16)),
        PacketID.EffectResultBasic32 when (EffectResultBasicN*)payload is var p => DecodeEffectResultBasic((EffectResultBasicEntry*)p->Entries, Math.Min((int)p->NumEntries, 32)),
        PacketID.EffectResultBasic64 when (EffectResultBasicN*)payload is var p => DecodeEffectResultBasic((EffectResultBasicEntry*)p->Entries, Math.Min((int)p->NumEntries, 64)),
        PacketID.ActorControl when (ActorControl*)payload is var p => DecodeActorControl(p->category, p->param1, p->param2, p->param3, p->param4, 0, 0, 0xE0000000),
        PacketID.ActorControlSelf when (ActorControlSelf*)payload is var p => DecodeActorControl(p->category, p->param1, p->param2, p->param3, p->param4, p->param5, p->param6, 0xE0000000),
        PacketID.ActorControlTarget when (ActorControlTarget*)payload is var p => DecodeActorControl(p->category, p->param1, p->param2, p->param3, p->param4, 0, 0, p->TargetID),
        PacketID.UpdateHpMpTp when (UpdateHpMpTp*)payload is var p => new($"hp={p->HP}, mp={p->MP}, gp={p->GP}"),
        PacketID.ActionEffect1 when (ActionEffect1*)payload is var p => DecodeActionEffect(&p->Header, (ActionEffect*)p->Effects, p->TargetID, 1, new()),
        PacketID.ActionEffect8 when (ActionEffect8*)payload is var p => DecodeActionEffect(&p->Header, (ActionEffect*)p->Effects, p->TargetID, 8, IntToFloatCoords(p->TargetX, p->TargetY, p->TargetZ)),
        PacketID.ActionEffect16 when (ActionEffect16*)payload is var p => DecodeActionEffect(&p->Header, (ActionEffect*)p->Effects, p->TargetID, 16, IntToFloatCoords(p->TargetX, p->TargetY, p->TargetZ)),
        PacketID.ActionEffect24 when (ActionEffect24*)payload is var p => DecodeActionEffect(&p->Header, (ActionEffect*)p->Effects, p->TargetID, 24, IntToFloatCoords(p->TargetX, p->TargetY, p->TargetZ)),
        PacketID.ActionEffect32 when (ActionEffect32*)payload is var p => DecodeActionEffect(&p->Header, (ActionEffect*)p->Effects, p->TargetID, 32, IntToFloatCoords(p->TargetX, p->TargetY, p->TargetZ)),
        PacketID.StatusEffectListPlayer when (StatusEffectListPlayer*)payload is var p => DecodeStatusEffectListPlayer(p),
        PacketID.UpdateRecastTimes when (UpdateRecastTimes*)payload is var p => DecodeUpdateRecastTimes(new(p->Elapsed, 80), new(p->Total, 80)),
        PacketID.UpdateDutyRecastTimes when (UpdateDutyRecastTimes*)payload is var p => DecodeUpdateRecastTimes(new(p->Elapsed, 2), new(p->Total, 2)),
        PacketID.ActorMove when (ActorMove*)payload is var p => new($"{Utils.Vec3String(IntToFloatCoords(p->X, p->Y, p->Z))} {IntToFloatAngle(p->Rotation)}, anim={p->AnimationFlags:X4}/{p->AnimationSpeed}, u={p->UnknownRotation:X2} {p->Unknown:X8}"),
        PacketID.ActorSetPos when (ActorSetPos*)payload is var p => new($"{Utils.Vec3String(new(p->X, p->Y, p->Z))} {IntToFloatAngle(p->Rotation)}, u={p->u2:X2} {p->u3:X2} {p->u4:X8} {p->u14:X8}"),
        PacketID.ActorCast when (ActorCast*)payload is var p => DecodeActorCast(p),
        PacketID.UpdateHate when (UpdateHate*)payload is var p => DecodeUpdateHate(p),
        PacketID.UpdateHater when (UpdateHater*)payload is var p => DecodeUpdateHater(p),
        PacketID.SpawnObject when (SpawnObject*)payload is var p => DecodeSpawnObject(p),
        PacketID.UpdateClassInfo when (UpdateClassInfo*)payload is var p => DecodeUpdateClassInfo(p),
        PacketID.UpdateClassInfoEureka when (UpdateClassInfoEureka*)payload is var p => DecodeUpdateClassInfo(&p->Data, $", rank={p->Rank}/{p->Element}/{p->u2}, pad={p->pad3:X2}"),
        PacketID.UpdateClassInfoBozja when (UpdateClassInfoBozja*)payload is var p => DecodeUpdateClassInfo(&p->Data, $", rank={p->Rank}, pad={p->pad1:X2}{p->pad2:X4}"),
        PacketID.RetainerSummary when (RetainerSummary*)payload is var p => new($"#{p->SequenceId}, {p->NumInformationPackets} packets, {p->MaxRetainerEntitlement} max retainers, response={p->IsResponseToServerCallbackRequest != 0} (listener index={p->ServerCallbackListenerIndex})"),
        PacketID.RetainerInformation when (RetainerInformation*)payload is var p => new($"#{p->SequenceId}.{p->Index}: {p->RetainerId:X} '{MemoryHelper.ReadSeStringNullTerminated((nint)p->Name)}', {p->NumItemsInInventory}/{p->NumItemsOnMarket} items, avail={p->Available}, u={p->Unk2A} {p->Unk2C}"),
        PacketID.ItemMarketBoardSummary when (ItemMarketBoardSummary*)payload is var p => new($"#{p->SequenceId}, {p->NumItemPackets} items"),
        PacketID.ItemMarketBoardInfo when (ItemMarketBoardInfo*)payload is var p => new($"#{p->SequenceId}, at {p->InventoryType}.{p->Slot}, unk={p->Unk10} {p->Unk18}"),
        PacketID.EventPlay when (EventPlayN*)payload is var p => DecodeEventPlay(p, Math.Min((int)p->PayloadLength, 1)),
        PacketID.EventPlay4 when (EventPlayN*)payload is var p => DecodeEventPlay(p, Math.Min((int)p->PayloadLength, 4)),
        PacketID.EventPlay8 when (EventPlayN*)payload is var p => DecodeEventPlay(p, Math.Min((int)p->PayloadLength, 8)),
        PacketID.EventPlay16 when (EventPlayN*)payload is var p => DecodeEventPlay(p, Math.Min((int)p->PayloadLength, 16)),
        PacketID.EventPlay32 when (EventPlayN*)payload is var p => DecodeEventPlay(p, Math.Min((int)p->PayloadLength, 32)),
        PacketID.EventPlay64 when (EventPlayN*)payload is var p => DecodeEventPlay(p, Math.Min((int)p->PayloadLength, 64)),
        PacketID.EventPlay128 when (EventPlayN*)payload is var p => DecodeEventPlay(p, Math.Min((int)p->PayloadLength, 128)),
        PacketID.EventPlay255 when (EventPlayN*)payload is var p => DecodeEventPlay(p, Math.Min((int)p->PayloadLength, 255)),
        PacketID.ServerRequestCallbackResponse1 when (DecodeServerRequestCallbackResponse*)payload is var p => DecodeServerRequestCallbackResponse(p),
        PacketID.EnvControl when (EnvControl*)payload is var p => new($"{p->DirectorID:X8}.{p->Index} = {p->State1:X4} {p->State2:X4}, pad={p->pad9:X2} {p->padA:X4} {p->padC:X8}"),
        PacketID.NpcYell when (NpcYell*)payload is var p => new($"{DecodeActor(p->SourceID)}: {p->Message} '{Service.LuminaRow<Lumina.Excel.Sheets.NpcYell>(p->Message)?.Text}' (u8={p->u8}, uE={p->uE}, u10={p->u10}, u18={p->u18})"),
        PacketID.WaymarkPreset when (WaymarkPreset*)payload is var p => DecodeWaymarkPreset(p),
        PacketID.Waymark when (ServerIPC.Waymark*)payload is var p => DecodeWaymark(p),
        PacketID.ActorGauge when (ActorGauge*)payload is var p => new($"{p->ClassJobID} = {p->Payload:X16}"),
        _ => null
    };

    private TextNode DecodeMarketBoardItemListing(MarketBoardItemListing* p)
    {
        var res = new TextNode($"request {p->RequestId}, page {p->FirstPageIndex}-{p->NextPageIndex}");
        for (int i = 0; i < 10; ++i)
        {
            var item = (MarketBoardItemListingEntry*)p->EntriesRaw + i;
            var s1 = MemoryHelper.ReadString((nint)item + 0x46, 32);
            var s2 = MemoryHelper.ReadString((nint)item + 0x66, 32);
            res.AddChild($"{item->ListingId:X}: {item->Quantity}x {item->ItemId} @ {item->UnitPrice}+{item->TotalTax}, cont={item->ContainerId}, nmat={item->MateriaCount}, s1={s1}, s2={s2}, unks={item->Unk40:X8} {item->Unk44:X4} {item->Unk88:X2} {item->Unk8C:X8}");
        }
        return res;
    }

    private TextNode DecodeMarketBoardItemHistory(MarketBoardItemListingHistory* p)
    {
        var res = new TextNode($"item {p->ItemId}");
        for (int i = 0; i < 20; ++i)
        {
            var entry = (MarketBoardItemListingHistoryEntry*)p->RawEntries + i;
            res.AddChild($"{entry->Quantity}x at {entry->UnitPrice} @ {DateTimeOffset.FromUnixTimeSeconds(entry->SaleUnixTimestamp)} from {MemoryHelper.ReadStringNullTerminated((nint)entry->RetainerName)}");
        }
        return res;
    }

    private TextNode DecodeStatusEffectList(StatusEffectList* p, string extra = "")
    {
        var res = new TextNode($"L{p->Level} {p->ClassID}, hp={p->CurHP}/{p->MaxHP}, mp={p->CurMP}/{p->MaxMP}, shield={p->ShieldValue}%{extra}, u={p->u2:X2} {p->u3:X2} {p->u12:X4} {p->u17C:X8}");
        AddStatuses(res, (Status*)p->Statuses, 30);
        return res;
    }

    private TextNode DecodeStatusEffectListDouble(StatusEffectListDouble* p)
    {
        var res = DecodeStatusEffectList(&p->Data);
        AddStatuses(res, (Status*)p->SecondSet, 30, 30);
        return res;
    }

    private TextNode DecodeStatusEffectListPlayer(StatusEffectListPlayer* p)
    {
        var res = new TextNode("");
        AddStatuses(res, (Status*)p->Statuses, 30);
        return res;
    }

    private void AddStatuses(TextNode res, Status* list, int count, int offset = 0)
    {
        for (var i = 0; i < count; ++i)
        {
            var s = list + i;
            if (s->ID != 0)
                res.AddChild($"[{i + offset}] {Utils.StatusString(s->ID)} {s->Extra:X4} {s->RemainingTime:f3}s left, from {DecodeActor(s->SourceID)}");
        }
    }

    private TextNode DecodeUpdateRecastTimes(ReadOnlySpan<float> elapsed, ReadOnlySpan<float> total)
    {
        var res = new TextNode("");
        for (int i = 0; i < elapsed.Length; ++i)
            res.AddChild($"group {i}: {elapsed[i]:f3}/{total[i]:f3}s");
        return res;
    }

    private TextNode DecodeEffectResult(EffectResultEntry* entries, int count)
    {
        var res = new TextNode($"{count} entries, u={*(uint*)(entries + count):X8}");
        for (var i = 0; i < count; ++i)
        {
            var e = entries + i;
            var resEntry = res.AddChild($"[{i}] seq={e->RelatedActionSequence}/{e->RelatedTargetIndex}, actor={DecodeActor(e->ActorID)}, class={e->ClassID}, hp={e->CurHP}/{e->MaxHP}, mp={e->CurMP}, shield={e->ShieldValue}, u={e->u16:X4}");
            var cnt = Math.Min(4, (int)e->EffectCount);
            var eff = (EffectResultEffect*)e->Effects;
            for (var j = 0; j < cnt; ++j)
            {
                resEntry.AddChild($"#{eff->EffectIndex}: id={Utils.StatusString(eff->StatusID)}, extra={eff->Extra:X2}, dur={eff->Duration:f3}s, src={DecodeActor(eff->SourceID)}, pad={eff->pad1:X2} {eff->pad2:X4}");
                ++eff;
            }
        }
        return res;
    }

    private TextNode DecodeEffectResultBasic(EffectResultBasicEntry* entries, int count)
    {
        var res = new TextNode($"{count} entries, u={*(uint*)(entries + count):X8}");
        for (var i = 0; i < count; ++i)
        {
            var e = entries + i;
            res.AddChild($"[{i}] seq={e->RelatedActionSequence}/{e->RelatedTargetIndex}, actor={DecodeActor(e->ActorID)}, hp={e->CurHP}, u={e->uD:X2} {e->uE:X4}");
        }
        return res;
    }

    private TextNode DecodeActorControl(ActorControlCategory category, uint p1, uint p2, uint p3, uint p4, uint p5, uint p6, ulong targetID)
    {
        var details = category switch
        {
            ActorControlCategory.CancelCast => $"{Utils.LogMessageString(p1)}; action={new ActionID((ActionType)p2, p3)}, interrupted={p4 == 1}", // note: some successful boss casts have this message on completion
            ActorControlCategory.RecastDetails => $"group {p1}: {p2 * Hundredth:f2}/{p3 * Hundredth:f2}s",
            ActorControlCategory.Cooldown => $"group {p1}: action={new ActionID(ActionType.Spell, p2)}, time={p3 * Hundredth:f2}s",
            ActorControlCategory.GainEffect => $"{Utils.StatusString(p1)}: extra={p2:X4}",
            ActorControlCategory.LoseEffect => $"{Utils.StatusString(p1)}: extra={p2:X4}, source={DecodeActor(p3)}, unk-update={p4 != 0}",
            ActorControlCategory.UpdateEffect => $"#{p1} {Utils.StatusString(p2)}: extra={p3:X4}",
            ActorControlCategory.TargetIcon => $"{p1 - IDScramble.Delta} ({p1}-{IDScramble.Delta})",
            ActorControlCategory.Tether => $"#{p1}: {p2} -> {DecodeActor(p3)} progress={p4}%",
            ActorControlCategory.TetherCancel => $"#{p1}: {p2}",
            ActorControlCategory.SetTarget => $"{DecodeActor(targetID)}",
            ActorControlCategory.SetAnimationState => $"#{p1} = {p2}",
            ActorControlCategory.SetModelState => $"{p1}",
            ActorControlCategory.SetName => $"'{Service.LuminaRow<Lumina.Excel.Sheets.BNpcName>(p1)?.Singular}' ({p1})",
            ActorControlCategory.SetCompanionOwnerId => $"{DecodeActor(p1)}",
            ActorControlCategory.ForcedMovement => $"dest={Utils.Vec3String(IntToFloatCoords((ushort)p1, (ushort)p2, (ushort)p3))}, rot={IntToFloatAngle((ushort)p4)}deg over {p5 * 0.0001:f4}s, type={p6}",
            ActorControlCategory.PlayActionTimeline => $"{p1:X4}",
            ActorControlCategory.EObjSetState => $"{p1:X4}, housing={(p3 != 0 ? p4 : null)}",
            ActorControlCategory.EObjAnimation => $"{p1:X4} {p2:X4}",
            ActorControlCategory.LimitBreakGauge => $"{p1} bars, {p2}/{p3}, uE={p4}, uF={p5}",
            ActorControlCategory.AchievementProgress => $"{p1} '{Service.LuminaRow<Lumina.Excel.Sheets.Achievement>(p1)?.Name}': {p2}/{p3}",
            ActorControlCategory.ActionRejected => $"{Utils.LogMessageString(p1)}; action={new ActionID((ActionType)p2, p3)}, recast={p4 * Hundredth:f2}/{p5 * Hundredth:f2}, src-seq={p6}",
            ActorControlCategory.ServerRequestCallbackResponse => $"listener index={p1}, listener request type={p2}, data={p3} {p4} {p5} {p6}",
            ActorControlCategory.SetDutyActionSet => $"row={p1}",
            ActorControlCategory.SetDutyActionDetails => $"slot0: {new ActionID(ActionType.Spell, p1)} ({p5}/{p2} charges), slot1: {new ActionID(ActionType.Spell, p3)} ({p6}/{p4} charges)",
            ActorControlCategory.SetDutyActionPresent => $"value={p1}",
            ActorControlCategory.SetDutyActionActive => $"slot0={p1}, slot1={p2}",
            ActorControlCategory.SetDutyActionCharges => $"slot0={p1}, slot1={p2}",
            ActorControlCategory.IncrementRecast => $"group {p1}: dt=dt={p2 * Hundredth:f2}s",
            _ => ""
        };
        return new TextNode($"{category} {details} ({p1:X8} {p2:X8} {p3:X8} {p4:X8} {p5:X8} {p6:X8} {DecodeActor(targetID)})");
    }

    private TextNode DecodeActionEffect(ActionEffectHeader* data, ActionEffect* effects, ulong* targetIDs, uint maxTargets, Vector3 targetPos)
    {
        var rot = IntToFloatAngle(data->rotation);
        var aid = data->actionId - IDScramble.Delta;
        var res = new TextNode($"#{data->globalEffectCounter} ({data->SourceSequence}) {new ActionID(data->actionType, aid)} ({data->actionId}/{data->actionAnimationId}), animTarget={DecodeActor(data->animationTargetId)}, animLock={data->animationLockTime:f3}, rot={rot}, pos={Utils.Vec3String(targetPos)}, var={data->variation}, ballista={DecodeActor(data->BallistaEntityId)}, flags={data->Flags:X2} pad={data->padding21:X4}");
        var targets = Math.Min(data->NumTargets, maxTargets);
        for (var i = 0; i < targets; ++i)
        {
            var targetId = targetIDs[i];
            if (targetId == 0)
                continue;
            var resTarget = res.AddChild($"target {i} == {DecodeActor(targetId)}");
            for (var j = 0; j < 8; ++j)
            {
                var eff = effects + i * 8 + j;
                if (eff->Type == ActionEffectType.Nothing)
                    continue;
                resTarget.AddChild($"effect {j} == {eff->Type}, params={eff->Param0:X2} {eff->Param1:X2} {eff->Param2:X2} {eff->Param3:X2} {eff->Param4:X2} {eff->Value:X4}");
            }
        }
        return res;
    }

    private TextNode DecodeActorCast(ActorCast* p)
    {
        var aid = p->ActionID - IDScramble.Delta;
        return new($"{new ActionID(p->ActionType, aid)} ({new ActionID(ActionType.Spell, p->SpellID)}) @ {DecodeActor(p->TargetID)}, time={p->CastTime:f3} ({p->BaseCastTime100ms * 0.1f:f1}), rot={IntToFloatAngle(p->Rotation)}, targetpos={Utils.Vec3String(IntToFloatCoords(p->PosX, p->PosY, p->PosZ))}, interruptible={p->Interruptible}, ballista={DecodeActor(p->BallistaEntityId)}, u1={p->u1:X2}, u3={p->u3:X4}");
    }

    private TextNode DecodeUpdateHate(UpdateHate* p)
    {
        var res = new TextNode($"{p->NumEntries} entries, pad={p->pad1:X2} {p->pad2:X4} {p->pad3:X8}");
        var e = (UpdateHateEntry*)p->Entries;
        for (int i = 0, cnt = Math.Min((int)p->NumEntries, 8); i < cnt; ++i, ++e)
            res.AddChild($"{DecodeActor(e->ObjectID)} = {e->Enmity}%");
        return res;
    }

    private TextNode DecodeUpdateHater(UpdateHater* p)
    {
        var res = new TextNode($"{p->NumEntries} entries, pad={p->pad1:X2} {p->pad2:X4} {p->pad3:X8}");
        var e = (UpdateHateEntry*)p->Entries;
        for (int i = 0, cnt = Math.Min((int)p->NumEntries, 32); i < cnt; ++i, ++e)
            res.AddChild($"{DecodeActor(e->ObjectID)} = {e->Enmity}%");
        return res;
    }

    private TextNode DecodeSpawnObject(SpawnObject* p)
    {
        var res = new TextNode($"#{p->Index} {p->DataID:X} <{p->InstanceID:X}>");
        res.AddChild($"Kind={p->Kind}, u2={p->u2_state}, u3={p->u3}, u20={p->u20}, tail={p->u3C:X2} {p->u3E:X2}");
        res.AddChild($"LevelID={p->u_levelID}, GimmickID={p->u_gimmickID}, DutyID={p->DutyID}, FateID={p->FateID}");
        res.AddChild($"Pos={Utils.Vec3String(p->Position)}, Rot={IntToFloatAngle(p->Rotation)}deg, Scale={p->Scale:f3}");
        res.AddChild($"Model={p->u_modelID}, EState={p->EventState}, EObjState={p->EventObjectState}");
        return res;
    }

    private TextNode DecodeUpdateClassInfo(UpdateClassInfo* p, string extra = "") => new($"L{p->CurLevel}/{p->ClassLevel}/{p->SyncedLevel} {p->ClassID}, exp={p->CurExp}+{p->RestedExp}{extra}");

    private TextNode DecodeEventPlay(EventPlayN* p, int payloadLength)
    {
        var sb = new StringBuilder($"target={DecodeActor(p->TargetID)}, handler={p->EventHandler:X8}, fC={p->uC:X4}, f10={p->u10:X16}, pad={p->pad1:X4} {p->pad2:X2} {p->pad3:X4}, payload=[{payloadLength}]");
        for (var i = 0; i < payloadLength; ++i)
            sb.Append($" {p->Payload[i]:X8}");
        var res = new TextNode(sb.ToString());
        switch (p->EventHandler)
        {
            case 0x000A0001:
                // crafting
                var cp = (EventPlayN.PayloadCrafting*)p->Payload;
                var craftingNode = res.AddChild($"Crafting op {cp->OpId}");
                switch (cp->OpId)
                {
                    case EventPlayN.PayloadCrafting.OperationId.StartInfo:
                        craftingNode.AddChild($"Recipe #{cp->OpStartInfo.RecipeId}, quality={cp->OpStartInfo.StartingQuality}, u8={cp->OpStartInfo.u8}");
                        break;
                    case EventPlayN.PayloadCrafting.OperationId.ReturnedReagents:
                        if (cp->OpReturnedReagents.u0 != 0 || cp->OpReturnedReagents.u4 != 0 || cp->OpReturnedReagents.u8 != 0)
                            craftingNode.AddChild($"Unks: {cp->OpReturnedReagents.u0} {cp->OpReturnedReagents.u4} {cp->OpReturnedReagents.u8}");
                        for (var i = 0; i < 8; ++i)
                            if (cp->OpReturnedReagents.ItemIds[i] != 0)
                                craftingNode.AddChild($"{i}: {cp->OpReturnedReagents.ItemIds[i]} '{Service.LuminaRow<Lumina.Excel.Sheets.Item>(cp->OpReturnedReagents.ItemIds[i])?.Name}' {cp->OpReturnedReagents.NumNQ[i]}nq/{cp->OpReturnedReagents.NumHQ[i]}hq");
                        break;
                    case EventPlayN.PayloadCrafting.OperationId.AdvanceCraftAction:
                    case EventPlayN.PayloadCrafting.OperationId.AdvanceNormalAction:
                        var actionName = cp->OpId == EventPlayN.PayloadCrafting.OperationId.AdvanceCraftAction
                            ? Service.LuminaRow<Lumina.Excel.Sheets.CraftAction>((uint)cp->OpAdvanceStep.LastActionId)?.Name
                            : Service.LuminaRow<Lumina.Excel.Sheets.Action>((uint)cp->OpAdvanceStep.LastActionId)?.Name;
                        craftingNode.AddChild($"Step #{cp->OpAdvanceStep.StepIndex}, condition={cp->OpAdvanceStep.Condition} ({cp->OpAdvanceStep.ConditionParam}), delta-cp={cp->OpAdvanceStep.DeltaCP}");
                        craftingNode.AddChild($"Action: {cp->OpAdvanceStep.LastActionId} '{actionName}' ({(cp->OpAdvanceStep.Flags.HasFlag(EventPlayN.PayloadCrafting.StepFlags.LastActionSucceeded) ? "succeeded" : "failed")})");
                        craftingNode.AddChild($"Progress: {cp->OpAdvanceStep.CurProgress} (delta={cp->OpAdvanceStep.DeltaProgress})");
                        craftingNode.AddChild($"Quality: {cp->OpAdvanceStep.CurQuality} (delta={cp->OpAdvanceStep.DeltaQuality}, hq={cp->OpAdvanceStep.HQChance}, u38={cp->OpAdvanceStep.u38})");
                        craftingNode.AddChild($"Durability: {cp->OpAdvanceStep.CurDurability} (delta={cp->OpAdvanceStep.DeltaDurability})");
                        craftingNode.AddChild($"Flags: {cp->OpAdvanceStep.Flags}");
                        craftingNode.AddChild($"u44: {cp->OpAdvanceStep.u44}");
                        for (var i = 0; i < 7; ++i)
                            if (cp->OpAdvanceStep.RemoveStatusIds[i] != 0)
                                craftingNode.AddChild($"Removed status {i}: {Utils.StatusString((uint)cp->OpAdvanceStep.RemoveStatusIds[i])} param={cp->OpAdvanceStep.RemoveStatusParams[i]}");
                        break;
                    case EventPlayN.PayloadCrafting.OperationId.QuickSynthStart:
                        craftingNode.AddChild($"Recipe #{cp->OpQuickSynthStart.RecipeId}, max={cp->OpQuickSynthStart.MaxCount}");
                        break;
                }
                break;
        }
        return res;
    }

    private TextNode DecodeServerRequestCallbackResponse(DecodeServerRequestCallbackResponse* p)
    {
        var sb = new StringBuilder($"listener index={p->ListenerIndex}, listener request type={p->ListenerRequestType}, data=[{p->DataCount}]");
        for (int i = 0; i < p->DataCount; ++i)
            sb.Append($" {p->Data[i]}");
        return new(sb.ToString());
    }

    private TextNode DecodeWaymarkPreset(WaymarkPreset* p)
    {
        var res = new TextNode($"pad={p->pad1:X2} {p->pad2:X4}");
        for (var i = 0; i < 8; ++i)
            res.AddChild($"{(Waymark)i}: {(p->Mask & (1 << i)) != 0} at {Utils.Vec3String(new(p->PosX[i] * Thousandth, p->PosY[i] * Thousandth, p->PosZ[i] * Thousandth))}");
        return res;
    }
    private TextNode DecodeWaymark(ServerIPC.Waymark* p) => new($"{p->ID}: {p->Active != 0} at {Utils.Vec3String(new(p->PosX * Thousandth, p->PosY * Thousandth, p->PosZ * Thousandth))}, pad={p->pad2:X4}");

    public static Vector3 IntToFloatCoords(ushort x, ushort y, ushort z)
    {
        var fx = x * TwoKD65k - 1000;
        var fy = y * TwoKD65k - 1000;
        var fz = z * TwoKD65k - 1000;
        return new(fx, fy, fz);
    }

    public static Angle IntToFloatAngle(ushort rot)
    {
        return (rot * Inv65kDoublePI - MathF.PI).Radians();
    }
}

public sealed class PacketDecoderGame : PacketDecoder
{
    protected override string DecodeActor(ulong instanceID) => Utils.ObjectString(instanceID);
}
