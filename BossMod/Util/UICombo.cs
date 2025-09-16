﻿using Dalamud.Bindings.ImGui;
using System.Reflection;

namespace BossMod;

public static class UICombo
{
    public static string EnumString(Enum v)
    {
        var name = v.ToString();
        return v.GetType().GetField(name)?.GetCustomAttribute<PropertyDisplayAttribute>()?.Label ?? name;
    }

    public static bool Enum<T>(string label, ref T v, Func<T, string>? print = null) where T : Enum
    {
        print ??= p => EnumString(p);
        var res = false;
        ImGui.SetNextItemWidth(200);
        if (ImGui.BeginCombo(label, print(v)))
        {
            foreach (var opt in System.Enum.GetValues(v.GetType()))
            {
                if (ImGui.Selectable(print((T)opt), opt.Equals(v)))
                {
                    v = (T)opt;
                    res = true;
                }
            }
            ImGui.EndCombo();
        }
        return res;
    }

    public static bool Int(string label, string[] values, ref int v)
    {
        var res = false;
        ImGui.SetNextItemWidth(200);
        if (ImGui.BeginCombo(label, v < values.Length ? values[v] : v.ToString()))
        {
            for (var i = 0; i < values.Length; ++i)
            {
                if (ImGui.Selectable(values[i], v == i))
                {
                    v = i;
                    res = true;
                }
            }
            ImGui.EndCombo();
        }
        return res;
    }

    public static bool Bool(string label, string[] values, ref bool v)
    {
        var val = v ? 1 : 0;
        if (!Int(label, values, ref val))
            return false;
        v = val != 0;
        return true;
    }
}
