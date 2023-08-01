using Command;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ZFExamSys.Enum;

namespace ZFExamSys.ViewModels
{
    public class MainWindowViewModel: BindableBase
    {
        private ObservableCollection<ResourceType> _resourceTypes = new ObservableCollection<ResourceType>();

        public ObservableCollection<ResourceType> ResourceTypes
        {
            get { return _resourceTypes; }
            set { SetProperty(ref _resourceTypes, value); }
        }

        private ObservableCollection<string> _mainMenus = new ObservableCollection<string>();

        /// <summary>
        /// 主菜单
        /// </summary>
        public ObservableCollection<string> MainMenus
        {
            get { return _mainMenus; }
            set { SetProperty(ref _mainMenus, value); }
        }

        private DelegateCommand _loadCommand;

        public DelegateCommand LoadCommand =>
            _loadCommand ?? (_loadCommand = new DelegateCommand(InitData));

        private DelegateCommand<object> _changeResourceCommand;

        public DelegateCommand<object> ChangeResourceCommand =>
            _changeResourceCommand ?? (_changeResourceCommand = new DelegateCommand<object>(ChangeResouse));

        public void InitData()
        {
            GetResourceTypeDescriptions();
            
            MainMenus.Add("组卷");
            MainMenus.Add("出题");
            MainMenus.Add("单机练习");
            MainMenus.Add("考试练习");
            MainMenus.Add("专项练习");
        }

        public void ChangeResouse(object resourceType)
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            if (resourceType is ExCommandParameter exp)
            {
                //(ResourceType)((FrameworkElement)exp.Sender).DataContext
                if (exp.Sender is FrameworkElement fe)
                {
                    if (fe.DataContext is ResourceType rt)
                    {
                        switch (rt)
                        {
                            case ResourceType.FirstTheme:
                                ResourceDictionary re = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Util.Controls;component/Style/Colors1.xaml") };
                                Application.Current.Resources.MergedDictionaries.Add(re);
                                break;
                            case ResourceType.SecondTheme:
                                re = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Util.Controls;component/Style/Colors.xaml") };
                                Application.Current.Resources.MergedDictionaries.Add(re);
                                break;
                            case ResourceType.ThirdTheme:
                                re = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Util.Controls;component/Style/Colors1.xaml") };
                                Application.Current.Resources.MergedDictionaries.Add(re);
                                break;
                            default:
                                break;
                        }
                        ResourceDictionary re2 = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Util.Controls;component/Style/Global.xaml") };
                        Application.Current.Resources.MergedDictionaries.Add(re2);
                        ResourceDictionary re3 = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Util.Controls;component/Style/Themes/FIcon.xaml") };
                        Application.Current.Resources.MergedDictionaries.Add(re3);
                        ResourceDictionary re4 = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Util.Controls;component/Control/FButton.xaml") };
                        Application.Current.Resources.MergedDictionaries.Add(re4);
                        ResourceDictionary re5 = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Util.Controls;component/Style/Style.xaml") };
                        Application.Current.Resources.MergedDictionaries.Add(re5);
                        ResourceDictionary re6 = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Util.Controls;component/Control/WindowBase.xaml") };
                        Application.Current.Resources.MergedDictionaries.Add(re6);
                    }
                }
            }
        }

        public void GetResourceTypeDescriptions()
        {
            List<ResourceType> l= FactoryHelperManager.EnumHelper.GetDescriptionByEnum<ResourceType>();
            foreach(ResourceType rt in l)
            {
                ResourceTypes.Add(rt);
            }
        }
    }
}
