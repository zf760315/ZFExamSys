using ZFExamSys.ViewModels;
using ZFExamSys.Views;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ZFExamSys
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : PrismApplication
    {
        //设置启动起始页
        protected override Window CreateShell()
        {
            return Container.Resolve<MainView>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterForNavigation<ImageView, ImageViewModel>();
            //containerRegistry.RegisterForNavigation<ImageControl, ImageViewModel>();
        }

        //配置规则
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();
            ViewModelLocationProvider.Register<MainView, MainViewModel>();
            ViewModelLocationProvider.Register<ImageView, ImageViewModel>();
            //ViewModelLocationProvider.Register(typeof(MainView).ToString(), typeof(MainViewViewModel));
            //ViewModelLocationProvider.Register(typeof(MainView).ToString(), () => Container.Resolve<Foo>());
            //ViewModelLocationProvider.Register<MainView>(() => Container.Resolve<MainViewModel>());

            //ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            //{
            //    var viewName = viewType.FullName.Replace(".Views.", ".ViewModels.");
            //    var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
            //    var viewModelName = $"{viewName}Model, {viewAssemblyName}";
            //    return Type.GetType(viewModelName);
            //});
        }
    }
}
