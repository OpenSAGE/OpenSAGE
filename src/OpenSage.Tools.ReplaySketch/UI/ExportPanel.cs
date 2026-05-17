using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using OpenSage.Tools.ReplaySketch.Model;
using OpenSage.Tools.ReplaySketch.Services;

namespace OpenSage.Tools.ReplaySketch.UI;

internal sealed class ExportPanel
{
    private List<string> _validationErrors = [];
    private bool _validationRan = false;
    private bool _exportAttempted = false;
    private string? _lastExportPath;
    private string? _lastExportError;

    private const string PreviewReplayName = "sketch_preview.rep";

    public void Draw(ReplayScenario scenario, MapMetadataService? map, LauncherLocatorService launcherLocator)
    {
        ImGui.BeginChild("ExportPanel", new Vector2(0, 0), ImGuiChildFlags.Borders, ImGuiWindowFlags.None);

        ImGui.SeparatorText("Validate & Export");

        // ── Validate ──────────────────────────────────────────────────────
        var validateDisabled = map == null;
        if (validateDisabled) ImGui.BeginDisabled();

        if (ImGui.Button("Validate", new Vector2(120, 0)) && map != null)
        {
            var result = TerrainValidator.Validate(scenario, map);
            _validationErrors = result.Errors.Select(e => e.ToString()).ToList();
            _validationRan = true;
            _exportAttempted = false;
        }

        if (validateDisabled)
        {
            ImGui.EndDisabled();
            ImGui.SameLine();
            ImGui.TextDisabled("(load a map first)");
        }

        if (_validationRan)
        {
            if (_validationErrors.Count == 0)
            {
                ImGui.TextColored(new Vector4(0.2f, 1f, 0.2f, 1f), "✓ All positions valid.");
            }
            else
            {
                ImGui.TextColored(new Vector4(1f, 0.3f, 0.3f, 1f), $"✗ {_validationErrors.Count} error(s):");
                foreach (var err in _validationErrors)
                {
                    ImGui.BulletText(err);
                }
            }
        }

        ImGui.Spacing();

        // ── Export ────────────────────────────────────────────────────────
        var exportDisabled = map == null;
        if (exportDisabled) ImGui.BeginDisabled();

        if (ImGui.Button("Export to Replays Dir", new Vector2(180, 0)) && map != null)
        {
            var replaysDir = ReplaysPathResolver.Resolve();
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var outputPath = Path.Combine(replaysDir, $"sketch_{timestamp}.rep");

            var error = ReplayExporter.Export(scenario, map, outputPath);

            _lastExportPath = error == null ? outputPath : null;
            _lastExportError = error;
            _exportAttempted = true;
        }

        if (exportDisabled)
        {
            ImGui.EndDisabled();
        }

        if (_exportAttempted)
        {
            if (_lastExportError != null)
            {
                ImGui.TextColored(new Vector4(1f, 0.3f, 0.3f, 1f), "Export failed:");
                ImGui.TextWrapped(_lastExportError);
            }
            else if (_lastExportPath != null)
            {
                ImGui.TextColored(new Vector4(0.2f, 1f, 0.2f, 1f), "✓ Exported to:");
                ImGui.TextWrapped(_lastExportPath);
                if (ImGui.Button("Copy path##cp"))
                {
                    ImGui.SetClipboardText(_lastExportPath);
                }
            }
        }

        ImGui.Spacing();

        // ── Play in Game ──────────────────────────────────────────────────
        var launcherPath = launcherLocator.Locate();
        var playDisabled = map == null || launcherPath == null;
        if (playDisabled) ImGui.BeginDisabled();

        if (ImGui.Button("Play in Game", new Vector2(160, 0)) && map != null && launcherPath != null)
        {
            var replaysDir = ReplaysPathResolver.Resolve();
            var outputPath = Path.Combine(replaysDir, PreviewReplayName);

            var error = ReplayExporter.Export(scenario, map, outputPath);
            if (error != null)
            {
                _lastExportPath = null;
                _lastExportError = error;
                _exportAttempted = true;
            }
            else
            {
                _lastExportPath = outputPath;
                _lastExportError = null;
                _exportAttempted = true;

                ProcessStartInfo psi;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Wrap in cmd /k so the console window stays open after exit/crash
                    psi = new ProcessStartInfo("cmd.exe")
                    {
                        Arguments = $"/k \"{launcherPath}\" --replay {PreviewReplayName} --noaudio --noshellmap",
                        UseShellExecute = false,
                        CreateNoWindow = false,
                    };
                }
                else
                {
                    psi = new ProcessStartInfo(launcherPath)
                    {
                        Arguments = $"--replay {PreviewReplayName} --noaudio --noshellmap",
                        UseShellExecute = false,
                        CreateNoWindow = false,
                    };
                }
                Process.Start(psi);
            }
        }

        if (playDisabled)
        {
            ImGui.EndDisabled();
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                var tooltip = launcherPath == null
                    ? "OpenSage.Launcher not found in sibling build dirs"
                    : "(load a map first)";
                ImGui.SetTooltip(tooltip);
            }
        }

        ImGui.EndChild();
    }
}
