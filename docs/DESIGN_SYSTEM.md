# AP2 Focus Design System

Product: **AP2 Master – FIAE Prüfungstrainer**  
Platform: .NET 8 WPF  
Default theme: Dark navy. Also Light, System, HighContrast.

## Principle

One desktop product. Mockups in `DesignReferences/` set the language. Data is always real. Navigation stays the existing 17 sections. Status is never colour-only.

## Colour

| Role | Dark | Light | High contrast |
| --- | --- | --- | --- |
| App background | `#0B0F19` | `#F4F6FB` | `#000000` |
| Surface primary | `#161B2C` | `#FFFFFF` | `#000000` |
| Surface secondary | `#1D2437` | `#F0F3F9` | `#000000` |
| Sidebar | `#0E1220` | `#FFFFFF` | `#000000` |
| Text primary | `#E8ECF5` | `#15192B` | `#FFFF00` |
| Text secondary | `#94A0B8` | `#5C6784` | `#FFFFFF` |
| Border | `#242C42` | `#DDE3EE` | `#FFFFFF` |
| Primary / cobalt | `#3B82F6` | `#2563EB` | `#00FFFF` |
| Cyan highlight | `#22D3EE` | `#0891B2` | `#00FFFF` |
| Success / teal | `#22C55E` | `#0F7B3B` | `#00FF00` |
| Warning / amber | `#F59E0B` | `#9A5B00` | `#FFFF00` |
| Error / danger | `#EF4444` / `#DC2626` | `#C42B1C` | `#FF6060` |
| Focus | `#FFB900` | `#FFB900` | `#FFFF00` |
| Teil 1 | `#3B82F6` | same family | cyan |
| Teil 2 | `#8B5CF6` | same family | magenta-safe with label |
| WiSo | `#F59E0B` | same family | yellow |

Semantic resource keys live in `src/Ap2Master.App/Themes/`. Views bind `DynamicResource`, never local hex.

Legacy aliases (`BgBrush`, `SurfaceBrush`, `AccentBrush`, …) remain so existing screens pick up the system immediately.

## Spacing

`4, 8, 12, 16, 24, 32, 48`

Page padding 24. Card padding 16. Nav item padding 12. Compact chips 8/12.

## Radius

| Name | px | Use |
| --- | --- | --- |
| Small | 6 | inputs, checkboxes |
| Medium | 10 | buttons, nav items |
| Large | 14 | cards |
| Extra large | 20 | search field, pills |

Shadows: one optional 12 px / 18 % black on elevated popups only. No blur on the canvas.

## Typography

Font: `Segoe UI Variable, Segoe UI`.  
Mono: `Cascadia Code, Cascadia Mono, Consolas`.

| Style | Size | Weight |
| --- | --- | --- |
| App / page title | 22–26 | Semibold / Bold |
| Section | 16–18 | Semibold |
| Card heading | 14–15 | Semibold |
| Body | 14 | Regular |
| Secondary / caption | 12 | Regular |
| Metric | 28–32 | Bold |
| Code / SQL | 13 | Regular, mono |

Do not go below 12 for UI chrome. `FontScale` in settings multiplies the window base size.

## Motion

120–220 ms. Opacity / translate only. `ReducedMotion` (`Bewegung reduzieren`) disables storyboards.

## Shell

Custom caption: logo, search (Strg+K), exam countdown, inbox, learner chip (real `DisplayName` + initials), min/max/close.

Sidebar: grouped 17 sections, Fluent glyphs + text, collapse to icons with tooltips, persist `SidebarCollapsed`, theme toggle, DB/autosave status.

## Components

Styles in `Themes/Controls.xaml` / `Fluent.xaml`:

`Card`, `Metric` text, `Primary` / `Ghost` / `Danger` / `Icon` buttons, `NavButton`, `Pill`, `OptionCard` (exam), `FilterChip`, progress bar, virtualized `ListBox`/`DataGrid`.

## Charts

LiveCharts only when the series is real (`StudySessions`, `ReadinessSnapshots`, `UserAnswers`). Empty state is a caption, not a fake 78 %.

## Accessibility

Visible focus (`FocusBrush`), keyboard order, icon+text, colour+shape for exam navigator, high-contrast theme, reduced motion, 32 px click targets on primary actions.

## Themes

`ThemeKind`: Dark, Light, HighContrast, **System**.  
System follows Windows AppsUseLightTheme and HighContrast. Change applies without restart via `IThemeService`.
