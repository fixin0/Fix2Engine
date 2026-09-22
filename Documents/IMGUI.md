# UI — Native ImGui

> **Note:** `Fix2Engine.IMGUI` was removed. All UI now uses native `ImGuiNET` + `rlImGui-cs` directly. This document shows the current native patterns.

All UI must be drawn between `rlImGui.Begin()` and `rlImGui.End()` (wrap `SceneManager.RenderUI()` in this block inside your application's `Render()` override).

## Setup

Call once in `Game.Start()`:

```csharp
rlImGui.Setup(true);
var style = ImGui.GetStyle();
style.WindowRounding = 12.0f;
style.FrameRounding = 8.0f;
```

Configure rounding, spacing, and `ImGuiCol` colors through `ImGui.GetStyle()`.

## Native Patterns

### Centered Panel

Old `IMGUI.BeginCenteredPanel` → native:

```csharp
var viewport = ImGui.GetMainViewport();
ImGui.SetNextWindowPos(viewport.GetCenter(), ImGuiCond.Always, new Vector2(0.5f, 0.5f));
ImGui.SetNextWindowSize(new Vector2(360, 360), ImGuiCond.Always);
if (ImGui.Begin("MainMenu", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings))
{
    // header
    string header = "MAIN MENU";
    float tw = ImGui.CalcTextSize(header).X;
    ImGui.SetCursorPosX((ImGui.GetWindowSize().X - tw) * 0.5f);
    ImGui.TextColored(new Vector4(0.90f, 0.92f, 0.98f, 1), header);
    ImGui.Separator();

    if (ImGui.Button("Play", new Vector2(280, 48)))
        SceneManager.LoadScene<MyScene>(); // your application's scene

    // danger button
    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.45f, 0.15f, 0.18f, 0.80f));
    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.65f, 0.20f, 0.22f, 1.00f));
    bool exit = ImGui.Button("Exit", new Vector2(280, 48));
    ImGui.PopStyleColor(2);
    if (exit) ImGui.OpenPopup("ExitConfirm");

    ImGui.End();
}

if (ImGui.BeginPopupModal("ExitConfirm", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings))
{
    ImGui.Text("Are you sure?");
    if (ImGui.Button("Exit", new Vector2(120, 0))) { ImGui.CloseCurrentPopup(); Environment.Exit(0); }
    ImGui.SameLine();
    if (ImGui.Button("Cancel", new Vector2(120, 0))) ImGui.CloseCurrentPopup();
    ImGui.EndPopup();
}
```

### Widgets

Old `Widgets.*` → native `ImGui.*`:

| Old | Native |
|-----|--------|
| `Widgets.Spacer(h)` | `ImGui.Dummy(new Vector2(0, h))` |
| `Widgets.SeparatorWithLabel(l)` | `ImGui.Separator(); ImGui.Text(l); ImGui.Separator();` |
| `Widgets.Toggle(l, ref v)` | `ImGui.Checkbox(l, ref v)` |
| `Widgets.SliderFloat(l, ref v, min, max)` | `ImGui.SliderFloat(l, ref v, min, max)` |
| `Widgets.SliderInt` / `InputText` / `Combo` | `ImGui.SliderInt` / `ImGui.InputText` / `ImGui.Combo` |

### Modals

Old `Modal.Open` / `Modal.Begin` / `Modal.Confirm` → native:

```csharp
ImGui.OpenPopup("SettingsModal");
if (ImGui.BeginPopupModal("SettingsModal", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings))
{
    ImGui.Text("SETTINGS");
    ImGui.Checkbox("Music", ref musicEnabled);
    ImGui.SliderFloat("Volume", ref volume, 0, 1);
    if (ImGui.Button("Close", new Vector2(120, 36))) ImGui.CloseCurrentPopup();
    ImGui.EndPopup();
}
```

## Raw ImGui

You can always use raw `ImGuiNET` inside `RenderUI`:

```csharp
ImGui.Text($"Score: {score}");
ImGui.Separator();
ImGui.SliderFloat("Speed", ref speed, 0, 10);
```

Call `RenderUI` inside `rlImGui.Begin()`/`End()` as shown above.
