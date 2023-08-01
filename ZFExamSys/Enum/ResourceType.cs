using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZFExamSys.Enum
{
    public enum ResourceType
    {
        [Description("主题1")]
        FirstTheme=1,
        [Description("主题2")]
        SecondTheme=2,
        [Description("主题3")]
        ThirdTheme=3
    }
}
