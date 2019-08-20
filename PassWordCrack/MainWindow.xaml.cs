using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using Ionic.Zip;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace PassWordCrack
{
    public partial class MainWindow : Window
    {

        string ZipPosition;
        string NewFilesPosition;
        string CurrentPassword = "";
        bool Finished = true;

        /// <summary>
        /// Version 1.0 Release Date : 30/03/2019
        /// Firts version, zip password brute cracker created by a 17 years old french student.
        /// </summary>

        public MainWindow()
        {
            InitializeComponent();
        }

        public void FindPassWord()
        {
            //ASCII table https://www.cs.cmu.edu/~pattis/15-1XX/common/handouts/ascii.html

            List<int> CharInt = new List<int>();
            int MaxCharRange = 0;
            int minCharRange = 0;
            int ScanMode = 0;

            //Change some UI.
            Dispatcher.Invoke(new Action(() =>
            {
                ScanLabel.Content = "Scan...";
                ScanButton.Content = "Cancel scan.";
                ScanProgressbar.Value = 0;
                ScanProgressbar.IsIndeterminate = true;
                ScanMode = ModeDropDown.SelectedIndex;
            }));

            //Set scan value range.
            if (ScanMode == 0)
            {
                MaxCharRange = 57;
                minCharRange = 47;
            }
            else if (ScanMode == 1)
            {
                MaxCharRange = 122;
                minCharRange = 96;
            }
            else if (ScanMode == 2)
            {
                MaxCharRange = 126;
                minCharRange = 64;
            }
            else if (ScanMode == 3)
            {
                MaxCharRange = 126;
                minCharRange = 47;
            }
            else if (ScanMode == 4)
            {
                MaxCharRange = 126;
                minCharRange = 31;
            }

            //Add the first char value.
            CharInt.Add(minCharRange);

            //Run scan while the password is not founded.
            while (!Finished)
            {
                //Check all char values.
                for (int i2 = 0; i2 < CharInt.Count; i2++)
                {
                    //Check if we need to add new value to list.
                    //If the last char is the last char value like "z" :
                    if (CharInt[CharInt.Count - 1] == MaxCharRange)
                    {
                        bool CanAddValue = true;
                        for (int i = 0; i < CharInt.Count; i++)
                        {
                            //If all chars aren't the last char value like "z", we can't add a new char in the password.
                            if (CharInt[i] != MaxCharRange && CharInt[i] != MaxCharRange + 1)
                            {
                                CanAddValue = false;
                                break;
                            }
                        }

                        //If all char are the last char value like "z", we can add a new char in the password.
                        if (CanAddValue)
                            CharInt.Add(minCharRange);
                    }

                    //If this value reach the last char like "z", we change this value and the next value.
                    if (CharInt[0] == MaxCharRange)
                    {
                        if (i2 + 1 < CharInt.Count)
                            CharInt[i2 + 1]++;

                        CharInt[0] = minCharRange;
                    }
                    if (CharInt[i2] == MaxCharRange + 1)
                    {
                        if (i2 + 1 < CharInt.Count)
                            CharInt[i2 + 1]++;

                        CharInt[i2] = minCharRange + 1;
                    }
                }

                //Increment value for the firts char value.
                CharInt[0]++;

                //Convert int to char.
                CurrentPassword = "";
                for (int i22 = 0; i22 < CharInt.Count; i22++)
                    CurrentPassword += Convert.ToChar(CharInt[i22]);

                /*Dispatcher.Invoke(new Action(() =>
                {
                    Debug.WriteLine(FinalPassword);
                }));*/

                //Start zip extraction.
                using (ZipFile zip = ZipFile.Read(ZipPosition))
                {
                    try
                    {
                        //Trying the password.
                        zip.Password = CurrentPassword;
                        zip.ExtractAll(NewFilesPosition, ExtractExistingFileAction.OverwriteSilently);
                        //If the extraction is succesfull, we stop the scan, update some UI and then clear all temp file.
                        Finished = true;
                        Dispatcher.Invoke(new Action(() =>
                        {
                            ScanLabel.Content = $"Password found : {CurrentPassword}";
                            ScanButton.Content = "Scan.";
                            ScanProgressbar.Value = 1;
                            ScanProgressbar.IsIndeterminate = false;
                        }));
                        CleanOutPut();
                        return;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            //Call dialog function or cancel function.
            if (Finished)
                AskDirectory();
            else
                CancelScan();
        }

        public void CancelScan()
        {
            //Cancel current string scan.
            Finished = true;
            ScanProgressbar.Value = 0;
            ScanProgressbar.IsIndeterminate = false;
            ScanLabel.Content = "Scan cancelled.";
            ScanButton.Content = "Scan.";
            CleanOutPut();
        }

        public void AskDirectory()
        {
            //Open a file dialog to find the file to scan.
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Zip Files(*.zip)|*.zip";
                dialog.Title = "Zip file to find Password.";
                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                    ZipPosition = dialog.FileName;
                else
                    return;
            }

            //Open a Folder browser dialog to set a output location.
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Please select an output folder.";
                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                    NewFilesPosition = dialog.SelectedPath;
                else
                    return;
            }

            //If file and folder is setted, we disable UI to prevent the user from using some UI.
            Finished = false;
            DisableUI();

            //Start scan thread..
            Thread StringThread = new Thread(new ThreadStart(FindPassWord))
            {
                IsBackground = true
            };
            StringThread.Start();
        }

        public void DisableUI()
        {
            //Disable some UI to prevent the user from changing the settings and other UI element.
            Dispatcher.Invoke(new Action(() =>
            {
                ModeDropDown.IsEnabled = false;
            }));
        }

        public void EnableUI()
        {
            //Enable some UI for the user for changing the settings.
            Dispatcher.Invoke(new Action(() =>
            {
                ModeDropDown.IsEnabled = true;
            }));
        }

        public void CleanOutPut()
        {
            //Remove all temp files created when the program is running.
            foreach (string item in Directory.GetFiles(NewFilesPosition))
                if (item.Contains("DotNetZip-"))
                    File.Delete(item);

            //Then enable UI elements.
            EnableUI();
        }
    }
}
