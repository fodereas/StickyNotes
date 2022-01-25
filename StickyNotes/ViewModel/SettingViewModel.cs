﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using StickyNotes.Utils;
using StickyNotes.Utils.HotKeyUtil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComboBoxItem = System.Windows.Controls.ComboBoxItem;

namespace StickyNotes
{
    public class SettingViewModel : ViewModelBase
    {
        public ProgramData Datas { get; set; }
        
        public List<string> Themes { get; set; }  = new List<string>()
        {
            "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna"
        };
        public List<string> Languages { get; set; } = new List<string>()
        {
            "Chinese",
            "English",
        };
        public HotKeyModel ShowAllHotKey { get; set; }

        public RelayCommand<bool> IsTopMostChangedCommand { get; set; }

        public RelayCommand<bool> IsStartUpWithSystemChangedCommand { get; set; }

        public RelayCommand<SelectionChangedEventArgs> SelectionChangedCommand { get; set; }
        public RelayCommand<SelectionChangedEventArgs> LanguageChangeCommand { get; set; }
        public RelayCommand<KeyEventArgs> ShowAllHotKeyChangedCommand { get; set; }

        #region 窗体数据

        public bool IsTopMost { get; set; }
        public string HotKeyStr { get; set; }
        #endregion
        /// <summary>
        /// 当输入Ctrl+A，X，C，Z，V等按键，调用此command
        /// </summary>
        public RelayCommand<string> ShowAllTextUsedCommand { get; set; }

        public SettingViewModel()
        {
            Datas = ProgramData.Instance;
            IsTopMost = Datas.IsWindowTopMost;
            ShowAllHotKey = ProgramData.Instance.ShowAllHotKey;
            HotKeyStr = ShowAllHotKey.ToString();
            IsTopMostChangedCommand = new RelayCommand<bool>(IsTopMostChangedMethod);
            IsStartUpWithSystemChangedCommand = new RelayCommand<bool>(IsStartUpWithSystemChangedMethod);
            SelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(SelectionChangedMethod);
            ShowAllHotKeyChangedCommand = new RelayCommand<KeyEventArgs>(ShowAllShortCutChangedMethod);
            ShowAllTextUsedCommand = new RelayCommand<string>(ShowAllTextUsedMethod);
            LanguageChangeCommand = new RelayCommand<SelectionChangedEventArgs>(LanguageChangeMethod);
        }

        private void LanguageChangeMethod(SelectionChangedEventArgs arg)
        {
            int index=Languages.IndexOf(arg.AddedItems[0].ToString());
            LanguageManager.ChangeLanguage((Language)index);
        }

        private void ShowAllTextUsedMethod(string key)
        {
            //使用了ctrl
            bool useCtrl = true;
            bool useAlt = false;
            bool useShift = false;
            EKey? useKey = null;
            if (key == "X")
            {
                useKey = EKey.X;
            }
            else if (key == "A")
            {
                useKey = EKey.A;
            }
            else if (key == "C")
            {
                useKey = EKey.C;
            }
            else if (key == "V")
            {
                useKey = EKey.V;
            }
            else if (key == "Z")
            {
                useKey = EKey.Z;
            }
            var oldHotKey = ShowAllHotKey;
            var newModel = ShowAllHotKey;
            newModel.IsSelectAlt = useAlt;
            newModel.IsSelectCtrl = useCtrl;
            newModel.IsSelectShift = useShift;
            if (useKey != null) newModel.SelectKey = (EKey)useKey;

            if (!useCtrl && !useAlt && !useShift)
                return;
            var hotKeyList = new ObservableCollection<HotKeyModel>
            {
                ShowAllHotKey
            };
            HotKeySettingsManager.Instance.IsShowAllWindowHotKeyNeedChanged = true;
            if (!HotKeySettingsManager.Instance.RegisterGlobalHotKey(hotKeyList))
            {
                //todo 
                MessageBox.Show("快捷键注册失败，可能系统或其它软件存在冲突");
                ShowAllHotKey = oldHotKey;
            }
            else
            {
                ShowAllHotKey = newModel;
                ProgramData.Instance.ShowAllHotKey = ShowAllHotKey;
                this.HotKeyStr = ShowAllHotKey.ToString();
                //this.SettingWin.ShowAllTB.Text = ShowAllHotKey.ToString();
            }

            return;

        }
        private void ShowAllShortCutChangedMethod(KeyEventArgs e)
        {
            bool useCtrl = false;//(e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            bool useAlt = false;//(e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
            bool useShift = false;//(e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                                  //            EKey useKey = EKey.Q;
            foreach (int v in Enum.GetValues(typeof(EKey)))
            {
                string keyName = Enum.GetName(typeof(EKey), v);
                if (e.Key.ToString() == keyName)
                {
                    var useKey = (EKey)v;
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        useCtrl = true;
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        useShift = true;
                    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                        useAlt = true;
                    var oldHotKey = new HotKeyModel();
                    oldHotKey.IsSelectAlt = ShowAllHotKey.IsSelectAlt;
                    oldHotKey.IsSelectCtrl = ShowAllHotKey.IsSelectCtrl;
                    oldHotKey.IsSelectShift = ShowAllHotKey.IsSelectShift;
                    oldHotKey.Name = ShowAllHotKey.Name;
                    oldHotKey.IsUsable = ShowAllHotKey.IsUsable;
                    oldHotKey.SelectKey = ShowAllHotKey.SelectKey;

                    ShowAllHotKey.IsSelectAlt = useAlt;
                    ShowAllHotKey.IsSelectCtrl = useCtrl;
                    ShowAllHotKey.IsSelectShift = useShift;
                    ShowAllHotKey.SelectKey = useKey;
                    if (!useCtrl && !useAlt && !useShift)
                        return;
                    var hotKeyList = new ObservableCollection<HotKeyModel>
                    {
                        ShowAllHotKey
                    };
                    HotKeySettingsManager.Instance.IsShowAllWindowHotKeyNeedChanged = true;
                    if (!HotKeySettingsManager.Instance.RegisterGlobalHotKey(hotKeyList))
                    {
                        //todo 
                        MessageBox.Show("快捷键注册失败，可能系统或其它软件存在冲突");
                        ShowAllHotKey = oldHotKey;
                    }
                    else
                    {
                        ProgramData.Instance.ShowAllHotKey = ShowAllHotKey;
                        HotKeyStr = ShowAllHotKey.ToString();
                        //this.SettingWin.ShowAllTB.Text = ShowAllHotKey.ToString();
                    }

                    return;
                }
            }

        }



        /// <summary>
        /// 选择主题发生改变
        /// </summary>
        /// <param name="obj"></param>
        private void SelectionChangedMethod(SelectionChangedEventArgs e)
        {
            var content = e.AddedItems[0] as string;

            ThemeAssist.ChangeTheme(content);
        }

        /// <summary>
        /// 是否开机启动
        /// </summary>
        /// <param name="param">当前选项是否勾选</param>
        private void IsStartUpWithSystemChangedMethod(bool param)
        {
            if (param == false)
            {
                Microsoft.Win32.RegistryKey key =
                    Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                        true);

                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }
            else
            {
                Microsoft.Win32.RegistryKey key =
                    Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                        true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                if (key.GetValue(curAssembly.GetName().Name) != null)
                {
                    key.DeleteValue(curAssembly.GetName().Name);
                }
            }

            Datas.IsStartUpWithSystem = !param;
        }

        /// <summary>
        /// 窗体是否置顶
        /// </summary>
        /// <param name="param"></param>
        private void IsTopMostChangedMethod(bool param)
        {
            foreach (var win in WindowsManager.Instance.Windows)
            {
                win.Topmost = param;
                win.Activate();
            }
            //this.IsTopMost = !param;


            Datas.IsWindowTopMost = IsTopMost;

        }

    }
}
