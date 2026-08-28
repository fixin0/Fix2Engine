# IMGUI / UI

Sources: `Fix2Engine.IMGUI/*` — namespace `Fix2Engine.IMGUI`

Wraps [Dear ImGui](https://github.com/ocornut/imgui) via `ImGuiNET` + `rlImGui-cs`. All UI must be drawn between `rlImGui.Begin()` and `rlImGui.End()` (done in `Game.Render()` which calls `SceneManager.RenderUI()` inside that block).

## Setup

Call once in `Game.Start()`:

```csharp
rlImGui.Setup(true); // dark theme = true
Theme.ApplyDark();
```

`Theme.ApplyDark()` configures `ImGui.GetStyle()` — rounding, spacing, and all `ImGuiCol` colors from `Palette`.

---

## IMGUI (Core Panels & Buttons)

`IMGUI.cs`

```csharp
public static class IMGUI
{
    public static bool BeginCenteredPanel(string id, Vector2 size);
    public static bool BeginPanelAt(string id, Vector2 pos, Vector2 size, Vector2? pivot = null);
    public static void EndPanel();
    public static void Header(string text);
    public static bool Button(string label, Vector2 size);
    public static bool DangerButton(string label, Vector2 size);
}
```

Usage:

```csharp
public void RenderUI()
{
    if (IMGUI.BeginCenteredPanel("MainMenu", new Vector2(360, 360)))
    {
        IMGUI.Header("MAIN MENU");

        if (IMGUI.Button("Play", new Vector2(280, 48)))
            SceneManager.LoadScene<FpsDemoScene>();

        if (IMGUI.DangerButton("Exit", new Vector2(280, 48)))
            Modal.Open("ExitConfirm");
    }
    IMGUI.EndPanel(); // pops style vars/colors automatically

    if (Modal.Confirm("ExitConfirm", "Are you sure?", "Exit", "Cancel") == true)
        Environment.Exit(0);
}
```

- `BeginCenteredPanel` centers the window, pushes 4 style vars + 5 colors.
- `BeginPanelAt` places a panel at an explicit position (for HUD elements).
- `EndPanel` calls `ImGui.End()` and pops the pushed styles via an internal stack.
- `Header` draws centered colored text + separator.
- `Button` / `DangerButton` are centered; `DangerButton` pushes `Palette.Danger` colors.

---

## Widgets

`Widgets.cs`

```csharp
public static class Widgets
{
    public static void Spacer(float h = 8f);
    public static void SeparatorWithLabel(string label);
    public static bool Checkbox(string label, ref bool v);
    public static bool Toggle(string label, ref bool v);          // iOS-style switch
    public static bool SliderFloat(string label, ref float v, float min, float max, string fmt = "%.1f");
    public static bool SliderInt(string label, ref int v, int min, int max);
    public static bool InputText(string label, ref string v, uint maxLen = 256);
    public static bool Combo(string label, ref int currentIndex, string[] options);
    public static void ProgressBar(float fraction, string overlay = "");
    public static void Tooltip(string text);
    public static bool IconButton(string glyph, Vector2 size);
    public static bool SuccessButton(string label, Vector2 size);
}
```

Example — settings modal:

```csharp
bool musicEnabled = true;
float volume = 0.8f;

if (Modal.Begin("SettingsModal", new Vector2(320, 180)))
{
    IMGUI.Header("SETTINGS");
    Widgets.Toggle("Music", ref musicEnabled);
    Widgets.SliderFloat("Volume", ref volume, 0.0f, 1.0f);
    Widgets.Spacer(12);
    if (IMGUI.Button("Close", new Vector2(120, 36)))
        ImGui.CloseCurrentPopup();
    Modal.End();
}
```

`Toggle` renders a custom switch via `ImDrawList` (`AddRectFilled` + `AddCircleFilled`). Tab bar helpers (`TabBar`, `Tab`, `EndTab`) are also available.

---

## Modals

`Modal.cs`

```csharp
public static class Modal
{
    public static void Open(string id);
    public static bool Begin(string id, Vector2 size);
    public static void End();
    public static bool? Confirm(string id, string message, string confirmLabel = "Yes", string cancelLabel = "Cancel");
}
```

- `Open(id)` — calls `ImGui.OpenPopup(id)`.
- `Begin` / `End` — wraps `BeginPopupModal` / `EndPopup`.
- `Confirm` — one-liner yes/no dialog; returns `true` (confirmed), `false` (cancelled), or `null` (still open).

```csharp
if (Modal.Confirm("ExitConfirm", "Quit the game?", "Quit", "Cancel") == true)
    Environment.Exit(0);
```

---

## Theme / Palette

`Theme.cs`

```csharp
public static class Palette
{
    public static Vector4 Background, PanelBorder, Text, TextMuted;
    public static Vector4 Accent, AccentHover, AccentActive;
    public static Vector4 Danger, DangerHover;
    public static Vector4 Success, SuccessHover;
    public static Vector4 FrameBg, FrameBgHover;
}

public static class Theme
{
    public static void ApplyDark(); // idempotent
}
```

Customize `Palette` values **before** calling `Theme.ApplyDark()` to change the look globally.

---

## Raw ImGui

You can always drop down to raw `ImGuiNET` inside `RenderUI`:

```csharp
ImGui.Text($"Score: {score}");
ImGui.Separator();
ImGui.SliderFloat("Speed", ref speed, 0, 10);
```

This works because `RenderUI` is already inside `rlImGui.Begin()`/`End()`.
