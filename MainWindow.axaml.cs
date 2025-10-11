using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FocSet
{
    public partial class MainWindow : Window
    {
        private readonly List<string> bodyParts = new List<string> { "ArmLo", "ArmUp", "Chest", "Head", "Legs" };
        private readonly List<string> partNames = new List<string>
        {
            "AirRaid", "Blastoff", "Brawl", "Breakdown", "Bruticus Head", "BumbleBee", "Bee Crown", "Cliffjumper", "Deadend", "Demolishor", "Dragstrip",
            "Grimlock", "HardBack Insect", "Hardshell", "Hardshell Insect", "Hound", "Ironhide", "Jazz", "Jetfire", "Kickback", "Kickback Insect", "Megatron", "Megatron Crown", "Metroplex Head", "Onslaught",
            "Optimus G1", "Optimus Prime", "Optimus Crown", "Perceptor", "Quake", "Ratchet", "Scattershot", "Sharpshot", "SharpshotHead", "Shockwave",
            "Sideswipe", "SilverBolt", "Slug", "Snarl", "Soundwave", "StarScream", "StarScream Crown", "Swindle", "Swoop", "Titan Head", "Trypticon", "UltraMagnus", "Vortex",
            "Warpath", "Wheeljack", "ZetaPrime"
        };
        public MainWindow()
        {
            InitializeComponent();
            InitializeComponents();
        }
        private void InitializeComponents()
        {
           
            BodyPartComboBox.ItemsSource = bodyParts;
            BodyPartComboBox.SelectedIndex = 0;

          
            Part1ComboBox.ItemsSource = partNames;
            Part1ComboBox.SelectedIndex = 0;

            var partNamesWithNull = new List<string> { "Null" };
            partNamesWithNull.AddRange(partNames);

            var partComboBoxes = new[] { Part2ComboBox, Part3ComboBox, Part4ComboBox, Part5ComboBox,
                                       Part6ComboBox, Part7ComboBox, Part8ComboBox, Part9ComboBox, Part10ComboBox };

            foreach (var comboBox in partComboBoxes)
            {
                comboBox.ItemsSource = partNamesWithNull;
                comboBox.SelectedIndex = 0;
            }
        }

        private async void BrowseExe_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select Executable File";
            dialog.Filters.Add(new FileDialogFilter { Name = "Executable files", Extensions = { "exe" } });

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var result = await dialog.ShowAsync(this);
                if (result != null && result.Length > 0)
                {
                    ExePathTextBox.Text = result[0];
                }
            }
        }

        private void AddPart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddText();
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
        }


        private void AddText()
        {
            var exePath = ExePathTextBox.Text;
            if (string.IsNullOrEmpty(exePath))
            {
                ShowError("Please select the game executable file.");
                return;
            }

            var dlcMapsPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(exePath)), "DLC", "DLCMaps");

            if (!Directory.Exists(dlcMapsPath))
            {
                Directory.CreateDirectory(dlcMapsPath);
            }

            var transcustomizationIniPath = Path.Combine(dlcMapsPath, "Transcustomization.ini");
            var transgameIntPath = Path.Combine(dlcMapsPath, "Transgame.int");

            var variableA = InGameNameTextBox.Text;
            var variableB = GenerateRandomString(6);

            if (CheckExistingEntry(variableB, transcustomizationIniPath))
            {
                ShowError("This unique code name already exists");
                return;
            }

            var selectedClasses = new List<string>();
            if (ScoutCheckBox.IsChecked == true) selectedClasses.Add("Scout");
            if (LeaderCheckBox.IsChecked == true) selectedClasses.Add("Leader");
            if (SoldierCheckBox.IsChecked == true) selectedClasses.Add("Soldier");
            if (ScientistCheckBox.IsChecked == true) selectedClasses.Add("Scientist");

            var bodyPart = BodyPartComboBox.SelectedItem as string;

            var selectedParts = new List<string>
            {
                Part1ComboBox.SelectedItem as string,
                Part2ComboBox.SelectedItem as string,
                Part3ComboBox.SelectedItem as string,
                Part4ComboBox.SelectedItem as string,
                Part5ComboBox.SelectedItem as string,
                Part6ComboBox.SelectedItem as string,
                Part7ComboBox.SelectedItem as string,
                Part8ComboBox.SelectedItem as string,
                Part9ComboBox.SelectedItem as string,
                Part10ComboBox.SelectedItem as string
            };

            var classesStr = string.Join(",", selectedClasses);

            if (string.IsNullOrEmpty(classesStr))
            {
                ShowError("Please select at least one class.");
                return;
            }

            if (string.IsNullOrEmpty(variableA))
            {
                ShowError("Please set in game name.");
                return;
            }

            var textToAppend = BuildTextToAppend(variableA, variableB, bodyPart, classesStr, selectedParts);

         
            File.AppendAllText(transcustomizationIniPath, "\n" + textToAppend);
            File.AppendAllText(transgameIntPath, "\n" + textToAppend);

            StatusLabel.Text = "Part added!";
        }

        private string GenerateRandomString(int length)
        {
            var random = new Random();
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private bool CheckExistingEntry(string variableB, string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("UniqueId="))
                {
                    var existingVariableB = line.Trim().Split('=')[1];
                    if (existingVariableB == variableB)
                        return true;
                }
            }
            return false;
        }

        private string BuildTextToAppend(string variableA, string variableB, string bodyPart, string classesStr, List<string> selectedParts)
        {
            var textToAppend = $";PartStart={variableA} {variableB}\n";
            textToAppend += $"[{variableB} TnDataProvider_Part]\n";
            textToAppend += $"FriendlyName={variableA}\n";
            textToAppend += $"UniqueId={variableB}\n";
            textToAppend += $"PartType={bodyPart}\n";
            textToAppend += $"SpecialtyRestriction={classesStr}\n";

            foreach (var name in selectedParts)
            {
                if (name == "Null") continue;

                var processedName = name;
                var shouldProceed = true;

             
                switch (name)
                {
                    case "Bee Crown":
                        processedName = "CarCrown";
                        shouldProceed = ShowWarning("Crown Heads are only for heads. Do you want to proceed?");
                        break;
                    case "HardBack Insect":
                        processedName = "HardBackInsect";
                        shouldProceed = ShowWarning("Boss Heads are only for heads. Do you want to proceed?");
                        break;
                    case "Hardshell Inesct":
                        processedName = "HardshellInsect";
                        shouldProceed = ShowWarning("Boss Heads are only for heads. Do you want to proceed?");
                        break;
                    case "Kickback Insect":
                        processedName = "KickbackInsect";
                        shouldProceed = ShowWarning("Boss Heads are only for heads. Do you want to proceed?");
                        break;
                    case "Sharpshot Insect":
                        processedName = "SharpshotInsect";
                        shouldProceed = ShowWarning("Boss Heads are only for heads. Do you want to proceed?");
                        break;
                    case "Metroplex Head":
                        processedName = "MetroplexHead";
                        shouldProceed = ShowWarning("Boss Heads are only for heads. Do you want to proceed?");
                        break;
                    case "Megatron Crown":
                        processedName = "TankCrown";
                        shouldProceed = ShowWarning("Crown Heads are only for heads. Do you want to proceed?");
                        break;
                    case "Titan Head":
                        processedName = "TitanHead";
                        shouldProceed = ShowWarning("Boss Heads are only for heads. Do you want to proceed?");
                        break;
                    case "Bruticus Head":
                        processedName = "BruticusHead";
                        shouldProceed = ShowWarning("Boss Heads are only for heads. Do you want to proceed?");
                        break;
                    case "Optimus Crown":
                        processedName = "TruckCrown";
                        shouldProceed = ShowWarning("Crown Heads are only for heads. Do you want to proceed?");
                        break;
                    case "StarScream Crown":
                        processedName = "JetCrown";
                        shouldProceed = ShowWarning("Crown Heads are only for heads. Do you want to proceed?");
                        break;
                }

                if (!shouldProceed)
                    throw new Exception("User cancelled the operation");

            
                textToAppend += BuildPartPath(processedName, bodyPart);
            }

            textToAppend += $";PartEnd={variableA} {variableB}\n";
            return textToAppend;
        }

        private string BuildPartPath(string name, string bodyPart)
        {
           
            switch (bodyPart)
            {
                case "ArmLo":
                    return $"PartPaths=TR_{name}_ROBO_p.CharSkel.RB_{name}_PART_ArmLoL\n" +
                           $"PartPaths=TR_{name}_ROBO_p.CharSkel.RB_{name}_PART_ArmLoR\n";
                case "ArmUp":
                    return $"PartPaths=TR_{name}_ROBO_p.CharSkel.RB_{name}_PART_ArmUpL\n" +
                           $"PartPaths=TR_{name}_ROBO_p.CharSkel.RB_{name}_PART_ArmUpR\n" +
                           $"PartPaths=TR_{name}_ROBO_p.CharSkel.RB_{name}_PART_CapL\n" +
                           $"PartPaths=TR_{name}_ROBO_p.CharSkel.RB_{name}_PART_CapR\n";
                default:
                    return $"PartPaths=TR_{name}_ROBO_p.CharSkel.RB_{name}_PART_{bodyPart}\n";
            }
        }

        private void ShowError(string message)
        {
          
            StatusLabel.Text = $"Error: {message}";
        }

        private bool ShowWarning(string message)
        {
            
           
            return true;
        }

        private void AsymmetricArms_Click(object sender, RoutedEventArgs e)
        {
            PartsPanel.IsVisible = AsymmetricArms.IsChecked == false;
            PartsPanelAA.IsVisible = AsymmetricArms.IsChecked == true;
        }
    }
}
