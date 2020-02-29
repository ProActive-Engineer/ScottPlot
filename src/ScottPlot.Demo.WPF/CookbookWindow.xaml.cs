﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScottPlot.Demo.WPF
{
    /// <summary>
    /// Interaction logic for DemoNavigator.xaml
    /// </summary>
    public partial class DemoNavigator : Window
    {
        public DemoNavigator()
        {
            InitializeComponent();
            LoadTreeWithDemos();
        }

        private void DemoSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedDemoItem = (TreeViewItem)DemoTreeview.SelectedItem;
            if (selectedDemoItem.Tag != null)
            {
                DemoPlotControl1.Visibility = Visibility.Visible;
                AboutControl.Visibility = Visibility.Hidden;
                DemoPlotControl1.LoadDemo(selectedDemoItem.Tag.ToString());
            }
            else
            {
                DemoPlotControl1.Visibility = Visibility.Hidden;
                AboutControl.Visibility = Visibility.Visible;
            }
        }

        private void LoadTreeWithDemos()
        {
            // TODO: make this tree in our own class and use binding to display it

            // GENERAL
            var generalTreeItem = new TreeViewItem() { Header = "General Plots", IsExpanded = true };
            DemoTreeview.Items.Add(generalTreeItem);
            foreach (string demoName in Demo.Reflection.GetDemoPlots("ScottPlot.Demo.General."))
            {
                IPlotDemo plotDemo = Reflection.GetPlot(demoName);
                generalTreeItem.Items.Add(new TreeViewItem() { Header = plotDemo.name, ToolTip = plotDemo.description, Tag = demoName.ToString() });
            }

            // PLOT TYPES
            var plotTypesTreeItem = new TreeViewItem() { Header = "Plot Types", IsExpanded = false };
            DemoTreeview.Items.Add(plotTypesTreeItem);
            foreach (string demoName in Demo.Reflection.GetDemoPlots("ScottPlot.Demo.PlotTypes."))
            {
                IPlotDemo plotDemo = Reflection.GetPlot(demoName);
                plotTypesTreeItem.Items.Add(new TreeViewItem() { Header = plotDemo.name, ToolTip = plotDemo.description, Tag = demoName.ToString() });
            }

            // STYLE
            var styleTreeItem = new TreeViewItem() { Header = "Custom Plot Styles", IsExpanded = false };
            DemoTreeview.Items.Add(styleTreeItem);
            foreach (string demoName in Demo.Reflection.GetDemoPlots("ScottPlot.Demo.Style."))
            {
                IPlotDemo plotDemo = Reflection.GetPlot(demoName);
                styleTreeItem.Items.Add(new TreeViewItem() { Header = plotDemo.name, ToolTip = plotDemo.description, Tag = demoName.ToString() });
            }

            // EXPERIMENTAL
            var experimentalTreeItem = new TreeViewItem() { Header = "Experimental Plots", IsExpanded = Debugger.IsAttached };
            DemoTreeview.Items.Add(experimentalTreeItem);
            foreach (string demoName in Demo.Reflection.GetDemoPlots("ScottPlot.Demo.Experimental."))
            {
                IPlotDemo plotDemo = Reflection.GetPlot(demoName);
                experimentalTreeItem.Items.Add(new TreeViewItem() { Header = plotDemo.name, ToolTip = plotDemo.description, Tag = demoName.ToString() });
            }
        }
    }
}