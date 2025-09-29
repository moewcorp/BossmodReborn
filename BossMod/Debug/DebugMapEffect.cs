<<<<<<< HEAD:BossMod/Debug/DebugEnvControl.cs
﻿using Dalamud.Interface.Utility.Raii;
=======
﻿using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
>>>>>>> merge:BossMod/Debug/DebugMapEffect.cs
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using System.Globalization;

namespace BossMod;

<<<<<<< HEAD:BossMod/Debug/DebugEnvControl.cs
sealed unsafe class DebugEnvControl
=======
sealed unsafe class DebugMapEffect : IDisposable
>>>>>>> merge:BossMod/Debug/DebugMapEffect.cs
{
    private delegate void ProcessMapEffectDelegate(void* self, uint index, ushort s1, ushort s2);
    private readonly ProcessMapEffectDelegate ProcessMapEffect = Marshal.GetDelegateForFunctionPointer<ProcessMapEffectDelegate>(Service.SigScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 41 0F B7 E8"));

    private readonly List<string> _history = [];
    private string _current = "";
<<<<<<< HEAD:BossMod/Debug/DebugEnvControl.cs
=======
    private bool deduplicate;

    private readonly EventSubscription _onMapEffect;

    public DebugMapEffect(WorldState ws)
    {
        _onMapEffect = ws.MapEffect.Subscribe(OnMapEffect);
    }

    public void Dispose() => _onMapEffect.Dispose();
>>>>>>> merge:BossMod/Debug/DebugMapEffect.cs

    public void Draw()
    {
        ImGui.SetNextItemWidth(100);
        ImGui.InputText("ii.ssssssss", ref _current, 12);
        ImGui.SameLine();
        if (ImGui.Button("Execute"))
<<<<<<< HEAD:BossMod/Debug/DebugEnvControl.cs
            ExecuteEnvControl();
=======
            ApplyMapEffect();
        ImGui.SameLine();
        if (ImGui.Button("Clear history"))
            _history.Clear();
>>>>>>> merge:BossMod/Debug/DebugMapEffect.cs

        using var hist = ImRaii.ListBox("History");
        if (hist)
            foreach (var h in _history)
                if (ImGui.Selectable(h, h == _current))
                    _current = h;
    }

<<<<<<< HEAD:BossMod/Debug/DebugEnvControl.cs
    private void ExecuteEnvControl()
=======
    private void OnMapEffect(WorldState.OpMapEffect ec)
    {
        if (deduplicate)
        {
            deduplicate = false;
            return;
        }
        _history.Insert(0, $"{ec.Index:X2}.{ec.State:X8}");
    }

    private void ApplyMapEffect()
>>>>>>> merge:BossMod/Debug/DebugMapEffect.cs
    {
        var parts = _current.Split('.');
        if (parts.Length != 2 || !byte.TryParse(parts[0], NumberStyles.HexNumber, null, out var index) || !uint.TryParse(parts[1], NumberStyles.HexNumber, null, out var state))
            return;

        _history.Remove(_current);
        _history.Insert(0, _current);
        var director = EventFramework.Instance()->DirectorModule.ActiveContentDirector;
        if (director == null)
        {
            Service.Log("No active content director, doing nothing");
            return;
        }
<<<<<<< HEAD:BossMod/Debug/DebugEnvControl.cs
        ProcessEnvControl(director, index, (ushort)(state & 0xFFFF), (ushort)(state >> 16));
=======
        deduplicate = true;
        ProcessMapEffect(director, index, (ushort)(state & 0xFFFF), (ushort)(state >> 16));
>>>>>>> merge:BossMod/Debug/DebugMapEffect.cs
    }
}
