﻿using NUnit.Framework.Internal;

namespace ScottPlotCookbook;

/// <summary>
/// Individual recipes can inherit this so they double as <see cref="IRecipe"/> and
/// test cases that have a function decorated with the <see cref="Test"/> attribute.
/// </summary>
public abstract class RecipeTestBase : IRecipe
{
    public Plot MyPlot { get; private set; } = new();
    private int Width = 400;
    private int Height = 300;

    public abstract string Name { get; }
    public abstract string Description { get; }

    /// <summary>
    /// This function is called by code interacting with <see cref="IRecipe"/>
    /// </summary>
    public void Recipe(Plot plot)
    {
        MyPlot = plot;
        Recipe();
    }

    /// <summary>
    /// This function is called from within the test system
    /// </summary>
    [Test]
    public abstract void Recipe();
    public bool RecipeHasTestAttribute => GetType().IsDefined(typeof(Test), false);

    [TearDown]
    public void SaveRecipeImage()
    {
        System.Diagnostics.StackTrace stackTrace = new();
        string callingMethod = stackTrace.GetFrame(1)!.GetMethod()!.Name;
        string fileName = $"{callingMethod}.png";

        string outputFolder = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "cookbook"));
        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);
        string filePath = Path.Combine(outputFolder, fileName);

        MyPlot.SavePng(filePath, Width, Height);
        TestContext.WriteLine($"{filePath}");
    }
}