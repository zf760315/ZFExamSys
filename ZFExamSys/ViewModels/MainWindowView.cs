using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZFExamSys.ViewModels
{
    public class MainWindowView: BindableBase
    {
        List<string> _mainMenus = new List<string>();

        /// <summary>
        /// 主菜单
        /// </summary>
        public List<string> MainMenus
        {
            get { return _mainMenus; }
            set { SetProperty(ref _mainMenus, value); }
        }

        public void InitData()
        {
            _mainMenus.Add("组卷");
            _mainMenus.Add("出题");
            _mainMenus.Add("单机练习");
            _mainMenus.Add("考试练习");
            _mainMenus.Add("专项练习");
        }
    }
}
