﻿using HandyControl.Data;
using Octokit;
using SkEditorPlus.Managers;
using SkEditorPlus.Windows;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Application = System.Windows.Application;

namespace SkEditorPlus.Functionalities
{
    public class MenuBarFunc : IFunctionality
    {
        static readonly RoutedCommand[] fileApplicationCommands = new RoutedCommand[]
        {
            ApplicationCommands.New,
            ApplicationCommands.Open,
            ApplicationCommands.Save,
            ApplicationCommands.SaveAs,
            ApplicationCommands.Close,
            new RoutedCommand("Publish", typeof(MenuBarFunc),
                new InputGestureCollection(new InputGesture[]
                {
                    new KeyGesture(Key.P, ModifierKeys.Control | ModifierKeys.Shift)
                }.ToList())),
            new RoutedCommand("Export", typeof(MenuBarFunc),
                new InputGestureCollection(new InputGesture[]
                {
                    new KeyGesture(Key.E, ModifierKeys.Control | ModifierKeys.Shift)
                }.ToList()))
        };
        static readonly RoutedCommand[] editApplicationCommands = new RoutedCommand[]
        {
            new RoutedCommand("Generate", typeof(MenuBarFunc),
                new InputGestureCollection(new InputGesture[]
                {
                    new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift)
                }.ToList())),
            new RoutedCommand("Format", typeof(MenuBarFunc),
                new InputGestureCollection(new InputGesture[]
                {
                    new KeyGesture(Key.F, ModifierKeys.Control | ModifierKeys.Shift)
                }.ToList())),
            new RoutedCommand("Backpack", typeof(MenuBarFunc),
                new InputGestureCollection(new InputGesture[]
                {
                    new KeyGesture(Key.B, ModifierKeys.Control | ModifierKeys.Shift)
                }.ToList()))
        };
        static readonly RoutedCommand[] otherApplicationCommands = new RoutedCommand[]
        {
            new RoutedCommand("Settings", typeof(MenuBarFunc),
                new InputGestureCollection(new InputGesture[]
                {
                    new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Shift)
                }.ToList())),
        };

        FileManager fileManager;
        SkEditorAPI skEditor;

        public void OnEnable(SkEditorAPI skEditor)
        {
            this.skEditor = skEditor;
            fileManager = skEditor.GetMainWindow().GetFileManager();
            foreach (MenuItem menuItem in skEditor.GetMainWindow().File_MenuItem.Items)
            {
                menuItem.Click += File_MenuItem_Click;
            }
            foreach (MenuItem menuItem in skEditor.GetMainWindow().Edit_MenuItem.Items)
            {
                menuItem.Click += Edit_MenuItem_Click;
            }
            foreach (MenuItem menuItem in skEditor.GetMainWindow().Other_MenuItem.Items)
            {
                menuItem.Click += Other_MenuItem_Click;
            }
            foreach (RoutedCommand command in fileApplicationCommands)
            {
                skEditor.GetMainWindow().CommandBindings.Add(new CommandBinding(command, File_MenuItem_Click));
            }
            foreach (RoutedCommand command in editApplicationCommands)
            {
                skEditor.GetMainWindow().CommandBindings.Add(new CommandBinding(command, Edit_MenuItem_Click));
            }
            foreach (RoutedCommand command in otherApplicationCommands)
            {
                skEditor.GetMainWindow().CommandBindings.Add(new CommandBinding(command, Other_MenuItem_Click));
            }

        }

        private void File_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (GetName(sender, e))
            {
                case "Menu_NewFile":
                case "New":
                    fileManager.NewFile();
                    break;

                case "Menu_Open":
                case "Open":
                    fileManager.OpenFile();
                    break;

                case "Menu_OpenFolder":
                    fileManager.OpenFolder();
                    break;

                case "Menu_Save":
                case "Save":
                    fileManager.Save();
                    break;

                case "Menu_SaveAs":
                case "SaveAs":
                    fileManager.SaveDialog();
                    break;

                case "Menu_Publish":
                case "Publish":
                    if (skEditor.GetMainWindow().GetFileManager().GetTextEditor() == null) return;
                    PublishWindow publishWindow = new(skEditor);
                    publishWindow.ShowDialog();
                    break;

                case "Menu_Export":
                case "Export":
                    fileManager.Export();
                    break;

                case "Menu_ExportOptions":
                    ExportOptionsWindow exportOptionsWindow = new(skEditor);
                    exportOptionsWindow.ShowDialog();
                    break;

                case "Menu_CloseFile":
                case "Close":
                    fileManager.CloseFile();
                    break;
            }
        }

        private void Edit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (GetName(sender, e))
            {
                case "Menu_Generate":
                case "Generate":
                    if (skEditor.GetMainWindow().GetFileManager().GetTextEditor() == null) return;
                    GenerateWindow generatorWindow = new(skEditor);
                    generatorWindow.ShowDialog();
                    break;
                case "Menu_Format":
                case "Format":
                    fileManager.FormatCode();
                    break;
                case "Menu_Backpack":
                case "Backpack":
                    if (skEditor.GetMainWindow().GetFileManager().GetTextEditor() == null) return;
                    BackpackWindow backpackWindow = new(skEditor);
                    backpackWindow.Show();
                    break;
            }
        }

        private void Other_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (GetName(sender, e))
            {
                case "Menu_Settings":
                case "Settings":
                    //OptionsWindow optionsWindow = new(skEditor);
                    NewOptionsWindow optionsWindow = new(skEditor);
                    optionsWindow.ShowDialog();
                    break;

                case "Menu_ChangeSyntax":
                    fileManager.ChangeSyntax("Skript");
                    break;

                case "Menu_Parser":
                    fileManager.OpenParser();
                    break;
                case "Menu_Docs":
                    fileManager.OpenDocs();
                    break;
                case "Menu_CheckUpdate":
                    CheckUpdate();
                    break;
            }
        }

        private static async void CheckUpdate()
        {
            var github = new GitHubClient(new ProductHeaderValue("SkEditorPlus"));
            var releases = await github.Repository.Release.GetAll("NotroDev", "SkEditorPlus");
            string latest = "";
            foreach (var release in releases)
            {
                if (!release.Prerelease)
                {
                    latest = release.TagName.Replace("v", "");
                    break;
                }
            }

            var current = MainWindow.Version;

            if (latest != current)
            {
                string newVersionTitle = (string)Application.Current.FindResource("NewVersion");
                string updateAvailable = (string)Application.Current.FindResource("UpdateAvailable");
                string download = (string)Application.Current.FindResource("Download");
                string ignore = (string)Application.Current.FindResource("Ignore");

                MessageBoxResult result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
                {
                    Message = updateAvailable.Replace("{0}", latest).Replace("{1}", current).Replace("{n}", Environment.NewLine),
                    Caption = newVersionTitle,
                    Button = MessageBoxButton.YesNo,
                    YesContent = download,
                    NoContent = ignore,
                    IconBrushKey = ResourceToken.DarkInfoBrush,
                    IconKey = ResourceToken.InfoGeometry
                });

                if (result == MessageBoxResult.Yes)
                {
                    string url = "https://github.com/NotroDev/SkEditorPlus/releases/latest";

                    try
                    {
                        Process.Start(url);
                    }
                    catch
                    {
                        url = url.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    }
                }
            }
            else
            {
                string noNewVersionTitle = (string)Application.Current.FindResource("NoNewVersion");
                string noNewVersion = (string)Application.Current.FindResource("UpdateNotAvailable");
                HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
                {
                    Message = noNewVersion.Replace("{0}", current).Replace("{n}", Environment.NewLine),
                    Caption = noNewVersionTitle,
                    Button = MessageBoxButton.OK,
                    ConfirmContent = "OK",
                    IconBrushKey = ResourceToken.DarkInfoBrush,
                    IconKey = ResourceToken.InfoGeometry
                });
            }
        }

        static string GetName(object sender, RoutedEventArgs e)
        {
            if (e.GetType() == typeof(ExecutedRoutedEventArgs))
            {
                ExecutedRoutedEventArgs ex = (ExecutedRoutedEventArgs)e;
                RoutedCommand command = (RoutedCommand)ex.Command;
                return command.Name;
            }
            FrameworkElement element = (FrameworkElement)sender;
            return element.Name;
        }
    }
}
