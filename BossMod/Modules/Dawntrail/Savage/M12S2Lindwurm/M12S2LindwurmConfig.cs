namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

[ConfigDisplay(Order = 0x160, Parent = typeof(DawntrailConfig))]
public sealed class M12S2LindwurmConfig : ConfigNode
{
    // ============================================================
    // Replication 1 — Strategy Selection
    // ============================================================

    [PropertyDisplay("Replication 1 strategy")]
    public Replication1Strategy Rep1Strategy = Replication1Strategy.CloneRelative;

    public Replication1Effective GetReplication1()
        => new(Rep1Strategy);

    public enum Replication1Strategy
    {
        [PropertyDisplay("Clone Relative")]
        CloneRelative,

        [PropertyDisplay("DN")]
        DN
    }

    public readonly struct Replication1Effective(Replication1Strategy strat)
    {
        public Replication1Strategy Strategy { get; } = strat;

        public bool IsCloneRelative => Strategy == Replication1Strategy.CloneRelative;
        public bool IsDN => Strategy == Replication1Strategy.DN;
    }

    // ============================================================
    // Replication 2 — Strategy Selection
    // ============================================================

    [PropertyDisplay("Replication 2 strategy")]
    public Replication2Strategy Rep2Strategy = Replication2Strategy.DN;

    // ============================================================
    // Replication 3 — Idyllic Dream
    // ============================================================

    [PropertyDisplay("Replication 3: clockspot → mechanic roles")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    public Replication3Role[] Rep3Roles =
    [
        Replication3Role.Stack1,
        Replication3Role.Stack2,
        Replication3Role.Stack3,
        Replication3Role.Stack4,
        Replication3Role.Defam1,
        Replication3Role.Defam2,
        Replication3Role.Defam3,
        Replication3Role.Defam4
    ];

    // ============================================================
    // Effective Runtime Views (USED BY MODULE CODE)
    // ============================================================

    public Replication2Effective GetReplication2()
    {
        return Rep2Strategy switch
        {
            Replication2Strategy.DN => Replication2Effective.FromPreset(Replication2Presets.DN),
            Replication2Strategy.BananaCodex => Replication2Effective.FromPreset(Replication2Presets.BC),
            _ => Replication2Effective.FromPreset(Replication2Presets.DN)
        };
    }

    public Replication3Effective GetReplication3()
        => new(Rep3Roles);
}

//
// ============================================================
// Strategy Selection
// ============================================================
//

public enum Replication2Strategy
{
    [PropertyDisplay("DN (true north)")]
    DN,

    [PropertyDisplay("Banana Codex (west = north)")]
    BananaCodex
}

//
// ============================================================
// Clock Positions
// ============================================================
//

public enum Clockspot
{
    [PropertyDisplay("North")] N,
    [PropertyDisplay("Northeast")] NE,
    [PropertyDisplay("East")] E,
    [PropertyDisplay("Southeast")] SE,
    [PropertyDisplay("South")] S,
    [PropertyDisplay("Southwest")] SW,
    [PropertyDisplay("West")] W,
    [PropertyDisplay("Northwest")] NW
}

//
// ============================================================
// Replication 2 Roles
// ============================================================
//

public enum Replication2Role
{
    [PropertyDisplay("Boss")] Boss,
    [PropertyDisplay("None")] None,

    [PropertyDisplay("Cone (CW)")] Cone1,
    [PropertyDisplay("Cone (CCW)")] Cone2,

    [PropertyDisplay("Stack (CW)")] Stack1,
    [PropertyDisplay("Stack (CCW)")] Stack2,

    [PropertyDisplay("Defamation (CW)")] Defam1,
    [PropertyDisplay("Defamation (CCW)")] Defam2,
}

//
// ============================================================
// Replication 3 Roles
// ============================================================
//

public enum Replication3Role
{
    [PropertyDisplay("Stack N1")] Stack1,
    [PropertyDisplay("Stack N2")] Stack2,
    [PropertyDisplay("Stack S1")] Stack3,
    [PropertyDisplay("Stack S2")] Stack4,

    [PropertyDisplay("Defamation N1")] Defam1,
    [PropertyDisplay("Defamation N2")] Defam2,
    [PropertyDisplay("Defamation S1")] Defam3,
    [PropertyDisplay("Defamation S2")] Defam4,
}

//
// ============================================================
// Effective Runtime Containers
// ============================================================
//

public readonly struct Replication2Effective(Clockspot relativeNorth, Replication2Role[] roles)
{
    public Clockspot RelativeNorth { get; } = relativeNorth;
    public Replication2Role[] Roles { get; } = roles;

    public static Replication2Effective FromPreset(Replication2Preset p)
        => new(p.RelativeNorth, p.Roles);

    public Replication2Role GetRole(Clockspot spot)
        => Roles[(int)spot];
}

public readonly struct Replication3Effective(Replication3Role[] roles)
{
    public Replication3Role[] Roles { get; } = roles;

    public Replication3Role GetRole(Clockspot spot)
        => Roles[(int)spot];
}

//
// ============================================================
// Presets (Original BossMod Behavior Preserved)
// ============================================================
//

public readonly struct Replication2Preset(Clockspot relativeNorth, Replication2Role[] roles)
{
    public Clockspot RelativeNorth { get; } = relativeNorth;
    public Replication2Role[] Roles { get; } = roles;
}

public static class Replication2Presets
{
    // DN — true north
    public static readonly Replication2Preset DN = new(
        Clockspot.N,
        [
            Replication2Role.Boss,
            Replication2Role.Cone1,
            Replication2Role.Stack1,
            Replication2Role.Defam1,
            Replication2Role.None,
            Replication2Role.Defam2,
            Replication2Role.Stack2,
            Replication2Role.Cone2
        ]
    );

    // Banana Codex — west treated as north, stack/cone pairing differs
    public static readonly Replication2Preset BC = new(
        Clockspot.W,
        [
            Replication2Role.Boss,
            Replication2Role.Stack1,
            Replication2Role.Cone1,
            Replication2Role.Defam1,
            Replication2Role.None,
            Replication2Role.Defam2,
            Replication2Role.Cone2,
            Replication2Role.Stack2
        ]
    );
}

//
// ============================================================
// Other Shared Types (unchanged)
// ============================================================
//

public enum Cardinal { N, S }

public record struct ReplicatedCloneOrder(Cardinal Group, int Order)
{
    public override readonly string ToString() => $"{Group}{Order + 1}";
}

public enum Element
{
    Wind,
    Dark,
    Earth,
    Fire
}

//
// ============================================================
// Utility Extensions
// ============================================================
//

// false positive
#pragma warning disable CA1708

public static class WurmExtensions
{
    extension(Clockspot c)
    {
        public Angle Angle => AngleExtensions.Degrees(180 - (int)c * 45);

        public static Clockspot GetClosest(Angle input)
            => typeof(Clockspot)
                .GetEnumValues()
                .Cast<Clockspot>()
                .MinBy(c => MathF.Abs(c.Angle.Rad - input.Rad));
    }

    extension(Replication2Role r)
    {
        public bool IsDefam => r is Replication2Role.Defam1 or Replication2Role.Defam2;
        public bool IsStack => r is Replication2Role.Stack1 or Replication2Role.Stack2;
        public bool IsCone => r is Replication2Role.Cone1 or Replication2Role.Cone2;
    }
}

//using Dalamud.Bindings.ImGui;
//using Dalamud.Interface.Utility.Raii;
//[ConfigDisplay(Parent = typeof(DawntrailConfig), Order = -1)]

/*
public class M12S2LindwurmConfig : ConfigNode
{
    public Replication2Tethers Rep2Assignments = new()
    {
        RelativeNorth = Replication2Tethers.DN.RelativeNorth,
        RolesOrdered = [.. Replication2Tethers.DN.RolesOrdered]
    };
    public Replication3Tethers Rep3Assignments = new() { RolesOrdered = [.. Replication3Tethers.DN.RolesOrdered] };

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        Rep2Assignments.DrawCustom(tree, Modified);
        Rep3Assignments.DrawCustom(tree, Modified);
    }
}

public enum Clockspot
{
    [PropertyDisplay("North")]
    N,
    [PropertyDisplay("Northeast")]
    NE,
    [PropertyDisplay("East")]
    E,
    [PropertyDisplay("Southeast")]
    SE,
    [PropertyDisplay("South")]
    S,
    [PropertyDisplay("Southwest")]
    SW,
    [PropertyDisplay("West")]
    W,
    [PropertyDisplay("Northwest")]
    NW
}

public enum Replication2Role
{
    [PropertyDisplay("Boss")]
    Boss,
    [PropertyDisplay("None")]
    None,
    [PropertyDisplay("Cone (CW)")]
    Cone1,
    [PropertyDisplay("Cone (CCW)")]
    Cone2,
    [PropertyDisplay("Stack (CW)")]
    Stack1,
    [PropertyDisplay("Stack (CCW)")]
    Stack2,
    [PropertyDisplay("Defam (CW)")]
    Defam1,
    [PropertyDisplay("Defam (CCW)")]
    Defam2,
}

public enum Cardinal
{
    N,
    S
}

public record struct ReplicatedCloneOrder(Cardinal Group, int Order)
{
    public override readonly string ToString() => $"{Group}{Order + 1}";
}

public class Replication2Tethers
{
    public Replication2Role[] RolesOrdered = new Replication2Role[8];
    public Clockspot RelativeNorth = Clockspot.N;

    public Replication2Role this[int index] => IsValid() ? RolesOrdered[index] : Replication2Role.None;
    public Replication2Role this[Clockspot index] => this[(int)index];

    public bool IsValid() => RolesOrdered.Distinct().Count() == 8;

    public void DrawCustom(UITree tree, Event modified)
    {
        foreach (var _ in tree.Node("Replication 2: clone assignments", contextMenu: () => DrawContextMenu(modified)))
        {
            if (UICombo.Enum("Relative north (for tether priority)", ref RelativeNorth))
                modified.Fire();

            using (var table = ImRaii.Table("tab2", 10, ImGuiTableFlags.SizingFixedFit))
            {
                if (table)
                {
                    foreach (var r in typeof(Clockspot).GetEnumValues())
                        ImGui.TableSetupColumn(r.ToString(), ImGuiTableColumnFlags.None, 25);
                    ImGui.TableSetupColumn("Role");
                    ImGui.TableHeadersRow();

                    foreach (var r in typeof(Replication2Role).GetEnumValues().Cast<Replication2Role>())
                    {
                        ImGui.TableNextRow();
                        for (var i = 0; i < RolesOrdered.Length; i++)
                        {
                            ImGui.TableNextColumn();
                            if (ImGui.RadioButton($"###r{r}_i{i}", RolesOrdered[i] == r))
                            {
                                RolesOrdered[i] = r;
                                modified.Fire();
                            }
                        }
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(UICombo.EnumString(r));
                    }
                }
            }

            if (IsValid())
            {
                using (ImRaii.PushColor(ImGuiCol.Text, 0xFF00FF00))
                    ImGui.TextUnformatted("All good!");
            }
            else
            {
                using (ImRaii.PushColor(ImGuiCol.Text, 0xFF00FFFF))
                    ImGui.TextUnformatted("Invalid assignments: roles must be unique");
            }
        }
    }

    void DrawContextMenu(Event modified)
    {
        if (ImGui.MenuItem("DN: true North; stack paired with stack"))
        {
            ReplaceWith(DN);
            modified.Fire();
        }
        if (ImGui.MenuItem("Banana Codex: West-relative; stack paired with defam"))
        {
            ReplaceWith(BC);
            modified.Fire();
        }
    }

    void ReplaceWith(Replication2Tethers preset)
    {
        Array.Copy(preset.RolesOrdered, RolesOrdered, 8);
        RelativeNorth = preset.RelativeNorth;
    }

    public static readonly Replication2Tethers DN = new()
    {
        RolesOrdered = [Replication2Role.Boss, Replication2Role.Cone1, Replication2Role.Stack1, Replication2Role.Defam1, Replication2Role.None, Replication2Role.Defam2, Replication2Role.Stack2, Replication2Role.Cone2],
        RelativeNorth = Clockspot.N
    };

    public static readonly Replication2Tethers BC = new()
    {
        RolesOrdered = [Replication2Role.Boss, Replication2Role.Stack1, Replication2Role.Cone1, Replication2Role.Defam1, Replication2Role.None, Replication2Role.Defam2, Replication2Role.Cone2, Replication2Role.Stack2],
        RelativeNorth = Clockspot.W
    };
}

public enum Replication3Role
{
    [PropertyDisplay("Stack N1")]
    Stack1,
    [PropertyDisplay("Stack N2")]
    Stack2,
    [PropertyDisplay("Stack S1")]
    Stack3,
    [PropertyDisplay("Stack S2")]
    Stack4,
    [PropertyDisplay("Defam N1")]
    Defam1,
    [PropertyDisplay("Defam N2")]
    Defam2,
    [PropertyDisplay("Defam S1")]
    Defam3,
    [PropertyDisplay("Defam S2")]
    Defam4,
}

public class Replication3Tethers
{
    public Replication3Role[] RolesOrdered = new Replication3Role[8];

    public Replication3Role this[int index] => IsValid() ? RolesOrdered[index] : Replication3Role.Stack1;
    public Replication3Role this[Clockspot index] => this[(int)index];

    public bool IsValid() => RolesOrdered.Distinct().Count() == 8;

    public void DrawCustom(UITree tree, Event modified)
    {
        foreach (var _ in tree.Node("Idyllic Dream: clone assignments", contextMenu: () => DrawContextMenu(modified)))
        {
            using (var table = ImRaii.Table("tab2", 10, ImGuiTableFlags.SizingFixedFit))
            {
                if (table)
                {
                    foreach (var r in typeof(Clockspot).GetEnumValues())
                        ImGui.TableSetupColumn(r.ToString(), ImGuiTableColumnFlags.None, 25);
                    ImGui.TableSetupColumn("Role");
                    ImGui.TableHeadersRow();

                    foreach (var r in typeof(Replication3Role).GetEnumValues().Cast<Replication3Role>())
                    {
                        ImGui.TableNextRow();
                        for (var i = 0; i < RolesOrdered.Length; i++)
                        {
                            ImGui.TableNextColumn();
                            if (ImGui.RadioButton($"###r{r}_i{i}", RolesOrdered[i] == r))
                            {
                                RolesOrdered[i] = r;
                                modified.Fire();
                            }
                        }
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(UICombo.EnumString(r));
                    }
                }
            }

            if (IsValid())
            {
                using (ImRaii.PushColor(ImGuiCol.Text, 0xFF00FF00))
                    ImGui.TextUnformatted("All good!");
            }
            else
            {
                using (ImRaii.PushColor(ImGuiCol.Text, 0xFF00FFFF))
                    ImGui.TextUnformatted("Invalid assignments: roles must be unique");
            }
        }
    }

    public static readonly Replication3Tethers DN = new()
    {
        RolesOrdered = [Replication3Role.Stack1, Replication3Role.Stack2, Replication3Role.Stack3, Replication3Role.Stack4, Replication3Role.Defam1, Replication3Role.Defam2, Replication3Role.Defam3, Replication3Role.Defam4]
    };

    public static readonly Replication3Tethers Caro = new()
    {
        RolesOrdered = [Replication3Role.Defam1, Replication3Role.Stack1, Replication3Role.Stack2, Replication3Role.Defam2, Replication3Role.Defam3, Replication3Role.Stack3, Replication3Role.Stack4, Replication3Role.Defam4]
    };

    void DrawContextMenu(Event modified)
    {
        if (ImGui.MenuItem("DN: N group stacks, S group defams"))
        {
            Array.Copy(DN.RolesOrdered, RolesOrdered, 8);
            modified.Fire();
        }
        if (ImGui.MenuItem("Caro: stack/defam alternate"))
        {
            Array.Copy(Caro.RolesOrdered, RolesOrdered, 8);
            modified.Fire();
        }
    }
}

public enum Element
{
    Wind,
    Dark,
    Earth,
    Fire
}

// false positive
#pragma warning disable CA1708 // Identifiers should differ by more than case
public static class WurmExtensions
{
    extension(Clockspot c)
    {
        public Angle Angle => AngleExtensions.Degrees(180 - (int)c * 45);

        public static Clockspot GetClosest(Angle input) => typeof(Clockspot).GetEnumValues().Cast<Clockspot>().MinBy(c => MathF.Abs(c.Angle.Rad - input.Rad));
    }

    extension(Replication2Role r)
    {
        public bool IsDefam => r is Replication2Role.Defam1 or Replication2Role.Defam2;
        public bool IsStack => r is Replication2Role.Stack1 or Replication2Role.Stack2;
        public bool IsCone => r is Replication2Role.Cone1 or Replication2Role.Cone2;
    }
}
*/