using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using System.Globalization;

namespace BossMod.AI;

sealed class AIManagementWindow : UIWindow
{
    private static readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private readonly AIManager _manager;
    private readonly EventSubscriptions _subscriptions;
    private const string _title = $"AI: off{_windowID}";
    private const string _windowID = "###AI debug window";

    public AIManagementWindow(AIManager manager) : base(_windowID, false, new(100, 100))
    {
        WindowName = _title;
        _manager = manager;
        _subscriptions = new
        (
            _config.Modified.ExecuteAndSubscribe(() => IsOpen = _config.DrawUI)
        );
        RespectCloseHotkey = false;
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    public void SetVisible(bool vis)
    {
        if (_config.DrawUI != vis)
        {
            _config.DrawUI = vis;
            _config.Modified.Fire();
        }
    }

    public override void Draw()
    {
        var configModified = false;

        ImGui.TextUnformatted($"Navi={_manager.Controller.NaviTargetPos}");

        configModified |= ImGui.Checkbox("Forbid actions", ref _config.ForbidActions);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Forbid movement", ref _config.ForbidMovement);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Idle while mounted", ref _config.ForbidAIMovementMounted);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Follow during combat", ref _config.FollowDuringCombat);
        ImGui.Spacing();
        configModified |= ImGui.Checkbox("Follow during active boss module", ref _config.FollowDuringActiveBossModule);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Follow out of combat", ref _config.FollowOutOfCombat);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Follow target", ref _config.FollowTarget);
        ImGui.Spacing();
        configModified |= ImGui.Checkbox("Manual targeting", ref _config.ManualTarget);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Disable loading obstacle maps", ref _config.DisableObstacleMaps);

        ImGui.Text("Follow party slot");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(250);
        ImGui.SetNextWindowSizeConstraints(new Vector2(0, 0), new Vector2(float.MaxValue, ImGui.GetTextLineHeightWithSpacing() * 50));
        if (ImRaii.Combo("##Leader", _manager.Beh == null ? "<idle>" : _manager.WorldState.Party[_manager.MasterSlot]?.Name ?? "<unknown>"))
        {
            if (ImGui.Selectable("<idle>", _manager.Beh == null))
                _manager.SwitchToIdle();
            foreach (var (i, p) in _manager.WorldState.Party.WithSlot(true))
            {
                if (ImGui.Selectable(p.Name, _manager.MasterSlot == i))
                {
                    _manager.SwitchToFollow(i);
                    _config.FollowSlot = i;
                    configModified = true;
                }
            }
            ImGui.EndCombo();
        }
        ImGui.Separator();
        ImGui.Text("Desired positional");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        var positionalOptions = Enum.GetNames(typeof(Positional));
        var positionalIndex = (int)_config.DesiredPositional;
        if (ImGui.Combo("##DesiredPositional", ref positionalIndex, positionalOptions, 4))
        {
            _config.DesiredPositional = (Positional)positionalIndex;
            configModified = true;
        }
        ImGui.SameLine();
        ImGui.Text("Max distance - to targets");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        var maxDistanceTargetStr = _config.MaxDistanceToTarget.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MaxDistanceToTarget", ref maxDistanceTargetStr, 64))
        {
            maxDistanceTargetStr = maxDistanceTargetStr.Replace(',', '.');
            if (float.TryParse(maxDistanceTargetStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var maxDistance))
            {
                _config.MaxDistanceToTarget = maxDistance;
                configModified = true;
            }
        }
        ImGui.SameLine();
        ImGui.Text("- to slots");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        var maxDistanceSlotStr = _config.MaxDistanceToSlot.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MaxDistanceToSlot", ref maxDistanceSlotStr, 64))
        {
            maxDistanceSlotStr = maxDistanceSlotStr.Replace(',', '.');
            if (float.TryParse(maxDistanceSlotStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var maxDistance))
            {
                _config.MaxDistanceToSlot = maxDistance;
                configModified = true;
            }
        }

        ImGui.Text("Movement decision delay");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        var movementDelayStr = _config.MoveDelay.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MovementDelay", ref movementDelayStr, 64))
        {
            movementDelayStr = movementDelayStr.Replace(',', '.');
            if (float.TryParse(movementDelayStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var delay))
            {
                _config.MoveDelay = delay;
                configModified = true;
            }
        }
        ImGui.SameLine();
        ImGui.Text("Autorotation AI preset");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(250);
        ImGui.SetNextWindowSizeConstraints(default, new Vector2(float.MaxValue, ImGui.GetTextLineHeightWithSpacing() * 50));
        var aipreset = _config.AIAutorotPresetName;
        var presets = _manager.Autorot.Database.Presets.VisiblePresets;

        var count = presets.Count;
        List<string> presetNames = new(count + 1);
        for (var i = 0; i < count; ++i)
        {
            presetNames.Add(presets[i].Name);
        }

        if (aipreset != null)
            presetNames.Add("Deactivate");
        var countnames = presetNames.Count;
        var selectedIndex = presetNames.IndexOf(aipreset ?? "");

        if (ImGui.Combo("##AI preset", ref selectedIndex, [.. presetNames], countnames))
        {
            if (selectedIndex == countnames - 1 && aipreset != null)
            {
                _manager.SetAIPreset(null);
                _config.AIAutorotPresetName = null;
                configModified = true;
                selectedIndex = -1;
            }
            else if (selectedIndex >= 0 && selectedIndex < count)
            {
                var selectedPreset = presets[selectedIndex];
                _manager.SetAIPreset(selectedPreset);
                _config.AIAutorotPresetName = selectedPreset.Name;
                configModified = true;
            }
        }
        if (configModified)
            _config.Modified.Fire();
    }

    public override void OnClose() => SetVisible(false);

    public void UpdateTitle()
    {
        var masterSlot = _manager?.MasterSlot ?? -1;
        var masterName = _manager?.Autorot?.WorldState?.Party[masterSlot]?.Name ?? "unknown";
        var masterSlotNumber = masterSlot != -1 ? (masterSlot + 1).ToString() : "N/A";

        WindowName = $"AI: {(_manager?.Beh != null ? "on" : "off")}, master={masterName}[{masterSlotNumber}]{_windowID}";
    }
}
