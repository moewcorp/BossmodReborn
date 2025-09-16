﻿using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace BossMod;

[ConfigDisplay(Name = "队伍角色分配", Order = 2)]
public class PartyRolesConfig : ConfigNode
{
    public enum Assignment { MT, OT, H1, H2, M1, M2, R1, R2, Unassigned }

    public Dictionary<ulong, Assignment> Assignments = [];

    public Assignment this[ulong contentID] => Assignments.GetValueOrDefault(contentID, Assignment.Unassigned);

    // return either array of assigned roles per party slot (if each role is assigned exactly once) or empty array (if assignments are invalid)
    public Assignment[] AssignmentsPerSlot(PartyState party)
    {
        var counts = new int[(int)Assignment.Unassigned];
        var res = Utils.MakeArray(PartyState.MaxPartySize, Assignment.Unassigned);
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            var r = this[party.Members[i].ContentId];
            if (r == Assignment.Unassigned)
                return [];
            if (counts[(int)r]++ > 0)
                return [];
            res[i] = r;
        }
        return res;
    }

    // return either array of party slots per assigned role (if each role is assigned exactly once) or empty array (if assignments are invalid)
    public int[] SlotsPerAssignment(PartyState party)
    {
        var res = Utils.MakeArray((int)Assignment.Unassigned, PartyState.MaxPartySize);
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            var r = this[party.Members[i].ContentId];
            if (r == Assignment.Unassigned)
                return [];
            if (res[(int)r] != PartyState.MaxPartySize)
                return [];
            res[(int)r] = i;
        }
        return res;
    }

    // return array of effective roles per party slot
    public Role[] EffectiveRolePerSlot(PartyState party)
    {
        var res = new Role[PartyState.MaxPartySize];
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            res[i] = this[party.Members[i].ContentId] switch
            {
                Assignment.MT or Assignment.OT => Role.Tank,
                Assignment.H1 or Assignment.H2 => Role.Healer,
                Assignment.M1 or Assignment.M2 => Role.Melee,
                Assignment.R1 or Assignment.R2 => Role.Ranged,
                _ => party[i]?.Role ?? Role.None
            };
        }
        return res;
    }

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        using (var table = ImRaii.Table("tab2", 10, ImGuiTableFlags.SizingFixedFit))
        {
            if (table)
            {
                foreach (var r in typeof(Assignment).GetEnumValues())
                    ImGui.TableSetupColumn(r.ToString(), ImGuiTableColumnFlags.None, 25);
                ImGui.TableSetupColumn("Name");
                ImGui.TableHeadersRow();

                List<(ulong cid, string name, char role, Assignment assignment)> party = [];
                for (var i = 0; i < PartyState.MaxPartySize; ++i)
                {
                    ref var m = ref ws.Party.Members[i];
                    if (m.IsValid())
                        party.Add((m.ContentId, m.Name, ws.Party[i]?.Role.ToString()[0] ?? '?', this[m.ContentId]));
                }

                party.Sort((a, b) => a.role.CompareTo(b.role));
                foreach (var (contentID, name, classRole, assignment) in party)
                {
                    ImGui.TableNextRow();
                    foreach (var r in typeof(Assignment).GetEnumValues().Cast<Assignment>())
                    {
                        ImGui.TableNextColumn();
                        if (ImGui.RadioButton($"###{contentID:X}:{r}", assignment == r))
                        {
                            if (r != Assignment.Unassigned)
                                Assignments[contentID] = r;
                            else
                                Assignments.Remove(contentID);
                            Modified.Fire();
                        }
                    }
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"({classRole}) {name}");
                }
            }
        }

        if (AssignmentsPerSlot(ws.Party).Length == 0)
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, Colors.TextColor2);
            ImGui.TextUnformatted("无效分配：每个角色应该只有一名团队成员");
        }
        else
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, Colors.TextColor4);
            ImGui.TextUnformatted("一切都好！");
        }
    }
}
