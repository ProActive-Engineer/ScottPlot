﻿using ScottPlot5_WinForms_Demo;
using System.Data;
using System.Runtime.Serialization;

namespace WinForms_Demo;

public partial class MainMenuForm : Form
{
    // TODO: how the types are held is a bit wonky. Maybe wrap this crazy insize a DemoWindowManager class or something.
    // TODO: improve demo item ordering.

    private readonly Dictionary<string, Type> Demos = DemoWindows.GetDemoTypesByTitle();

    public MainMenuForm()
    {
        InitializeComponent();
        label2.Text = ScottPlot.Version.VersionString;
    }

    private void MainMenuForm_Load(object sender, EventArgs e)
    {
        int initialWidth = Width;

        int nextItemPositionY = 100;
        int paddingBetweenItems = 10;
        int itemHeight = 83;

        IDemoForm[] demoForms = Demos.Values.Select(x => (IDemoForm)FormatterServices.GetUninitializedObject(x)).ToArray();

        foreach (IDemoForm demoForm in demoForms)
        {
            MenuItem item = new(demoForm, Demos[demoForm.Title])
            {
                Location = new Point(12, nextItemPositionY),
                Size = new Size(initialWidth - 55, itemHeight),
            };

            nextItemPositionY += itemHeight + paddingBetweenItems;

            Controls.Add(item);
        }
    }
}
