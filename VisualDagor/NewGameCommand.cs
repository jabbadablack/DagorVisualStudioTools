using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Gaijin.VisualDagor;

internal sealed class NewGameCommand
{
    private readonly AsyncPackage package;
    private readonly OleMenuCommand command;

    private NewGameCommand(AsyncPackage package, OleMenuCommandService commands)
    {
        this.package = package;
        command = new OleMenuCommand((_, _) => package.JoinableTaskFactory.RunAsync(CreateAsync).FileAndForget("VisualDagor/NewGame"),
            new CommandID(new Guid("4b96f2a4-4865-483f-9f69-acbd94fb772b"), 0x0100));
        commands.AddCommand(command);
    }

    public static async Task InitializeAsync(AsyncPackage package)
    {
        var commands = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
        await package.JoinableTaskFactory.SwitchToMainThreadAsync();
        if (commands != null) new NewGameCommand(package, commands);
    }

    private async Task CreateAsync()
    {
        await package.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await package.GetServiceAsync(typeof(SDTE)) as DTE;
        if (dte == null) throw new InvalidOperationException("Visual Studio automation is unavailable.");
        using var dialog = new NewGameDialog(FindEngine(dte?.Solution?.FullName));
        if (dialog.ShowDialog() != DialogResult.OK) return;
        command.Enabled = false;
        try
        {
            dte.StatusBar.Text = "Creating Dagor game " + dialog.GameName + " and generating IntelliSense...";
            var result = await GenerateAsync(dialog.EngineRoot, dialog.GameName, dialog.GameDirectory, dialog.TemplateName);
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();
            Project gameProject = null;
            dte.Solution.Open(result["solution"]);
            foreach (Project project in dte.Solution.Projects)
                if (string.Equals(project.FullName, result["project"], StringComparison.OrdinalIgnoreCase)) gameProject = project;
            if (gameProject != null) dte.Solution.SolutionBuild.StartupProjects = new[] { gameProject.UniqueName };
            dte.Solution.SolutionBuild.SolutionConfigurations.Item("dev").Activate();
            dte.StatusBar.Text = "Dagor game created. Run python prog/build.py in its folder for the first code/content build, then use F5.";
        }
        catch (Exception error)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();
            VsShellUtilities.ShowMessageBox(package, error.Message, "New Dagor Game", OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        finally { await package.JoinableTaskFactory.SwitchToMainThreadAsync(); command.Enabled = true; }
    }

    private static async Task<Dictionary<string, string>> GenerateAsync(string engine, string name, string directory, string template)
    {
        var devtools = Environment.GetEnvironmentVariable("GDEVTOOL") ?? Path.Combine(Directory.GetParent(engine).FullName, "devtools");
        var python = Path.Combine(devtools, "python3", "python.exe");
        if (!File.Exists(python)) python = "python";
        var script = Path.Combine(engine, "dng.py");
        using var process = new System.Diagnostics.Process { StartInfo = new ProcessStartInfo {
            FileName = python, Arguments = "\"" + script + "\" new --name " + name + " --directory \"" + directory + "\" --template " + template + " --no-engine-solution",
            WorkingDirectory = engine, UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true } };
        process.Start();
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await Task.Run(() => process.WaitForExit());
        var output = await stdout;
        var errors = await stderr;
        if (process.ExitCode != 0) throw new InvalidOperationException("Could not finish creating the game. Any generated source is retained at " + directory + ".\n\n" + errors);
        return new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(output);
    }

    private static string FindEngine(string solution)
    {
        for (var folder = string.IsNullOrEmpty(solution) ? null : new DirectoryInfo(Path.GetDirectoryName(solution)); folder != null; folder = folder.Parent)
        {
            if (File.Exists(Path.Combine(folder.FullName, "dng.py"))) return folder.FullName;
            var manifest = Path.Combine(folder.FullName, "dng-project.json");
            if (!File.Exists(manifest)) continue;
            try
            {
                var data = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(manifest));
                if (data.TryGetValue("engine", out var value) && value is string engine && File.Exists(Path.Combine(engine, "dng.py")))
                    return engine;
            }
            catch (ArgumentException) { }
            catch (IOException) { }
        }
        return Environment.GetEnvironmentVariable("DAGOR_ENGINE_ROOT") ?? "";
    }
}
