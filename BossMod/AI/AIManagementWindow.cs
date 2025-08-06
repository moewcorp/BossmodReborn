using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using System.Globalization;

namespace BossMod.AI;

sealed class AIManagementWindow : UIWindow
{
    private static readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private readonly AIManager _manager;
    private readonly EventSubscriptions _subscriptions;
    private const string _title = $"AI: off{_windowID}";
    private const string _windowID = "###AI debug window";
    private static readonly string[] positionals = Enum.GetNames<Positional>();
    private readonly byte[] _maxDistanceToTargetBuffer = new byte[64];
    private readonly byte[] _maxDistanceToSlotBuffer = new byte[64];
    private readonly byte[] _minDistanceBuffer = new byte[64];
    private readonly byte[] _preferredDistanceBuffer = new byte[64];
    private readonly byte[] _moveDelayBuffer = new byte[64];
    private bool _floatBuffersInitialized;

    public AIManagementWindow(AIManager manager) : base(_windowID, false, new(100f, 100f))
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
        if (!_floatBuffersInitialized)
        {
            SyncFloatBuffer(_config.MaxDistanceToTarget, _maxDistanceToTargetBuffer);
            SyncFloatBuffer(_config.MaxDistanceToSlot, _maxDistanceToSlotBuffer);
            SyncFloatBuffer(_config.MinDistance, _minDistanceBuffer);
            SyncFloatBuffer(_config.PreferredDistance, _preferredDistanceBuffer);
            SyncDoubleBuffer(_config.MoveDelay, _moveDelayBuffer);
            _floatBuffersInitialized = true;
        }

        ImGui.TextUnformatted($"Navi={_manager.Controller.NaviTargetPos}");

        configModified |= ImGui.Checkbox("Forbid actions", ref _config.ForbidActions);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Forbid movement", ref _config.ForbidMovement);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Idle while mounted", ref _config.ForbidAIMovementMounted);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Follow during combat", ref _config.FollowDuringCombat);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Must be enabled for follow target.");
            ImGui.EndTooltip();
        }
        ImGui.Spacing();
        configModified |= ImGui.Checkbox("Follow during active boss module", ref _config.FollowDuringActiveBossModule);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Must be enabled for following targets during active boss modules.");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Follow out of combat", ref _config.FollowOutOfCombat);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Follow target", ref _config.FollowTarget);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Follow the target with your distance settings.\nFollow during combat and follow during active boss module need to be activated.");
            ImGui.EndTooltip();
        }
        ImGui.Spacing();
        configModified |= ImGui.Checkbox("Manual targeting", ref _config.ManualTarget);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Allows manual targeting with an active AI autorotation.");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("Disable loading obstacle maps", ref _config.DisableObstacleMaps);

        ImGui.Text("Follow party slot");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(250);
        ImGui.SetNextWindowSizeConstraints(default, new Vector2(float.MaxValue, ImGui.GetTextLineHeightWithSpacing() * 50f));
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
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("Select player to follow to enable AI. Usually you select yourself for this.");
            ImGui.EndTooltip();
        }
        ImGui.Separator();
        ImGui.Text("Desired positional");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var positionalIndex = (int)_config.DesiredPositional;
        if (ImGui.Combo("##DesiredPositional", ref positionalIndex, positionals, 4))
        {
            _config.DesiredPositional = (Positional)positionalIndex;
            configModified = true;
        }

        bool DrawFloatInput(string label, ref float value, byte[] buffer, string tooltip)
        {
            var changed = false;
            ImGui.SameLine();
            ImGui.Text(label);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);

            if (ImGui.InputText($"##{label}", buffer))
            {
                var strVal = Encoding.UTF8.GetString(buffer).Trim('\0').Replace(',', '.');
                if (float.TryParse(strVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    value = parsed;
                    SyncFloatBuffer(value, buffer);
                    changed = true;
                }
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(tooltip);
                ImGui.EndTooltip();
            }
            return changed;
        }
        bool DrawDoubleInput(string label, ref double value, byte[] buffer, string tooltip)
        {
            var changed = false;
            ImGui.SameLine();
            ImGui.Text(label);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);

            if (ImGui.InputText($"##{label}", buffer))
            {
                var strVal = Encoding.UTF8.GetString(buffer).Trim('\0').Replace(',', '.');
                if (double.TryParse(strVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    value = parsed;
                    SyncDoubleBuffer(value, buffer);
                    changed = true;
                }
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(tooltip);
                ImGui.EndTooltip();
            }
            return changed;
        }
        configModified |= DrawFloatInput("Max distance - to targets", ref _config.MaxDistanceToTarget, _maxDistanceToTargetBuffer, "Maximum distance in yalms to keep away from targets.");
        configModified |= DrawFloatInput("- to slots", ref _config.MaxDistanceToSlot, _maxDistanceToSlotBuffer, "Maximum distance in yalms to keep away from followed allies.");
        configModified |= DrawFloatInput("Minimum distance", ref _config.MinDistance, _minDistanceBuffer, "Distance in yalms to keep away from target hitbox.");
        configModified |= DrawFloatInput("Pref distance to forbidden zones", ref _config.PreferredDistance, _preferredDistanceBuffer, "Distance in yalms to keep away from forbidden zones.");
        configModified |= DrawDoubleInput("Movement decision delay", ref _config.MoveDelay, _moveDelayBuffer, "Minimum time to start moving after movement decision has been made.\nAvoid setting this too high depending on the content.");
        ImGui.SameLine();
        ImGui.Text("Autorotation AI preset");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(250f);
        ImGui.SetNextWindowSizeConstraints(default, new Vector2(float.MaxValue, ImGui.GetTextLineHeightWithSpacing() * 50f));

        var aipreset = _config.AIAutorotPresetName;
        var presets = _manager.Autorot.Database.Presets.VisiblePresets;
        var count = presets.Count;
        List<string> presetNames = new(count + 1);
        for (var i = 0; i < count; ++i)
            presetNames.Add(presets[i].Name);
        if (aipreset != null)
            presetNames.Add("Deactivate");

        var countnames = presetNames.Count;
        var selectedIndex = presetNames.IndexOf(aipreset ?? "");

        if (ImGui.Combo("##AI preset", ref selectedIndex, [.. presetNames], countnames))
        {
            if (selectedIndex == countnames - 1 && aipreset != null)
            {
                _manager.SetAIPreset(null);
                configModified = true;
                selectedIndex = -1;
            }
            else if (selectedIndex >= 0 && selectedIndex < count)
            {
                var selectedPreset = presets[selectedIndex];
                _manager.SetAIPreset(selectedPreset);
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

    private void SyncFloatBuffer(float value, byte[] buffer)
    {
        var str = value.ToString(CultureInfo.InvariantCulture);
        Encoding.UTF8.GetBytes(str.AsSpan(), buffer);
        var nullTerminator = Array.IndexOf(buffer, default);
        if (nullTerminator >= 0)
            Array.Clear(buffer, nullTerminator, buffer.Length - nullTerminator);
    }

    private void SyncDoubleBuffer(double value, byte[] buffer)
    {
        var str = value.ToString(CultureInfo.InvariantCulture);
        Encoding.UTF8.GetBytes(str.AsSpan(), buffer);
        var nullTerminator = Array.IndexOf(buffer, default);
        if (nullTerminator >= 0)
            Array.Clear(buffer, nullTerminator, buffer.Length - nullTerminator);
    }
}
