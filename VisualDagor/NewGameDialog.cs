using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Gaijin.VisualDagor;

internal sealed class NewGameDialog : Form
{
    private readonly TextBox engine = new() { Dock = DockStyle.Fill };
    private readonly TextBox name = new() { Dock = DockStyle.Fill, Text = "MyGame" };
    private readonly TextBox parent = new() { Dock = DockStyle.Fill };
    private readonly ComboBox template = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label location = new() { AutoSize = true, MaximumSize = new System.Drawing.Size(530, 0) };
    public string EngineRoot => engine.Text.Trim();
    public string GameName => name.Text.Trim();
    public string GameDirectory => Path.Combine(parent.Text.Trim(), GameName);
    public string TemplateName => template.SelectedIndex == 1 ? "multiplayer" : "singleplayer";

    public NewGameDialog(string initialEngine)
    {
        Text = "New Dagor Game";
        Width = 700;
        Height = 355;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 3, RowCount = 7 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85));
        layout.Controls.Add(new Label { Text = "Engine checkout", AutoSize = true }, 0, 0);
        layout.Controls.Add(engine, 1, 0);
        var browseEngine = new Button { Text = "Browse...", Dock = DockStyle.Fill };
        browseEngine.Click += (_, _) => Browse(engine, "Select your Dagor Engine checkout");
        layout.Controls.Add(browseEngine, 2, 0);
        layout.Controls.Add(new Label { Text = "Game name", AutoSize = true }, 0, 1);
        layout.Controls.Add(name, 1, 1);
        layout.SetColumnSpan(name, 2);
        layout.Controls.Add(new Label { Text = "Template", AutoSize = true }, 0, 2);
        template.Items.AddRange(new object[] { "Single-player", "Multiplayer (ENet host/client)" });
        template.SelectedIndex = 0;
        layout.Controls.Add(template, 1, 2);
        layout.SetColumnSpan(template, 2);
        layout.Controls.Add(new Label { Text = "Project location", AutoSize = true }, 0, 3);
        layout.Controls.Add(parent, 1, 3);
        var browseParent = new Button { Text = "Browse...", Dock = DockStyle.Fill };
        browseParent.Click += (_, _) => Browse(parent, "Select the parent folder for your game project");
        layout.Controls.Add(browseParent, 2, 3);
        layout.Controls.Add(location, 1, 4);
        layout.SetColumnSpan(location, 2);
        layout.Controls.Add(new Label { Text = "Creates a standalone project using the engine in place, with Jam,\nCMake for Rider/CLion and a Visual Studio solution.", AutoSize = true }, 0, 5);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 5), 3);
        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
        var create = new Button { Text = "Create" };
        create.Click += (_, _) => {
            try {
                if (!File.Exists(Path.Combine(EngineRoot, "dng.py"))) throw new InvalidOperationException("Select an engine checkout with the Visual Dagor template integration.");
                if (!Regex.IsMatch(GameName, @"^[A-Za-z][A-Za-z0-9_]{0,63}$")) throw new InvalidOperationException("Use 1-64 letters, digits or underscores, starting with a letter.");
                if (string.IsNullOrWhiteSpace(parent.Text)) throw new InvalidOperationException("Choose a project location.");
                engine.Text = Path.GetFullPath(EngineRoot).TrimEnd(Path.DirectorySeparatorChar);
                parent.Text = Path.GetFullPath(parent.Text.Trim());
                if (Directory.Exists(GameDirectory)) throw new InvalidOperationException("The project directory already exists. Choose another name or location.");
                DialogResult = DialogResult.OK;
                Close();
            } catch (Exception error) { MessageBox.Show(this, error.Message, Text); }
        };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(create);
        layout.Controls.Add(buttons, 1, 6);
        layout.SetColumnSpan(buttons, 2);
        Controls.Add(layout);
        AcceptButton = create;
        CancelButton = cancel;
        engine.TextChanged += (_, _) => UpdateLocation();
        name.TextChanged += (_, _) => UpdateLocation();
        parent.TextChanged += (_, _) => UpdateLocation();
        engine.Text = initialEngine;
        parent.Text = string.IsNullOrEmpty(initialEngine) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Dagor Projects") : Directory.GetParent(initialEngine).FullName;
        UpdateLocation();
    }

    private void Browse(TextBox field, string description)
    {
        using var picker = new FolderBrowserDialog { Description = description, SelectedPath = field.Text };
        if (picker.ShowDialog(this) == DialogResult.OK) field.Text = picker.SelectedPath;
    }
    private void UpdateLocation() => location.Text = "Create in: " + GameDirectory;
}
