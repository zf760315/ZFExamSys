using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FactoryHelperManager
{
    public static class ChineseNumberAndArabicNumberconvert
    {
        private static char ToNum(char x)
        {
            string strChnNames = "零一二三四五六七八九";
            string strNumNames = "0123456789";
            return strChnNames[strNumNames.IndexOf(x)];
        }

        /// <summary>
        /// 转换万以下整数
        /// </summary>
        /// <param name="x">数字</param>
        /// <returns></returns>
        private static string NumberToChinese(string x)
        {
            string[] strArrayLevelNames = new string[4] { "", "十", "百", "千" };
            string ret = "";
            int i;
            for (i = x.Length - 1; i >= 0; i--)
                if (x[i] == '0')
                    ret = ToNum(x[i]) + ret;
                else
                    ret = ToNum(x[i]) + strArrayLevelNames[x.Length - 1 - i] + ret;
            while ((i = ret.IndexOf("零零")) != -1)
                ret = ret.Remove(i, 1);
            if (ret[ret.Length - 1] == '零' && ret.Length > 1)
                ret = ret.Remove(ret.Length - 1, 1);
            if (ret.Length >= 2 && ret.Substring(0, 2) == "一十")
                ret = ret.Remove(0, 1);
            return ret;
        }

        /// <summary>
        /// 转换整数
        /// </summary>
        /// <param name="x">数字</param>
        /// <returns></returns>
        private static string NoToChinese(string x)
        {
            int len = x.Length;
            string ret, temp;
            if (len <= 4)
                ret = NumberToChinese(x);
            else if (len <= 8)
            {
                ret = NumberToChinese(x.Substring(0, len - 4)) + "万";
                temp = NumberToChinese(x.Substring(len - 4, 4));
                if (temp.IndexOf("千") == -1 && temp != "")
                    ret += "零" + temp;
                else
                    ret += temp;
            }
            else
            {
                ret = NumberToChinese(x.Substring(0, len - 8)) + "亿";
                temp = NumberToChinese(x.Substring(len - 8, 4));
                if (temp.IndexOf("千") == -1 && temp != "")
                    ret += "零" + temp;
                else
                    ret += temp;
                ret += "万";
                temp = NumberToChinese(x.Substring(len - 4, 4));
                if (temp.IndexOf("千") == -1 && temp != "")
                    ret += "零" + temp;
                else
                    ret += temp;
            }
            int i;
            if ((i = ret.IndexOf("零万")) != -1)
                ret = ret.Remove(i + 1, 1);
            while ((i = ret.IndexOf("零零")) != -1)
                ret = ret.Remove(i, 1);
            if (ret[ret.Length - 1] == '零' && ret.Length > 1)
                ret = ret.Remove(ret.Length - 1, 1);
            return ret;
        }

        ///// <summary>
        ///// 一位数转换成
        ///// </summary>
        ///// <param name="x"></param>
        ///// <returns></returns>
        //private static string DecimalToChinese(string x)
        //{
        //    string ret = "";
        //    for (int i = 0; i < x.Length; i++)
        //        ret += ToNum(x[i]);
        //    return ret;
        //}

        /// <summary>
        /// 数字（int或decimal）转成中文
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string DecimalNumberToChinese(string x)
        {
            if (x.Length == 0)
                return "";
            string ret = "";
            try
            {
                if (x[0] == '-')
                {
                    ret = "负";
                    x = x.Remove(0, 1);
                }
                if (x[0].ToString() == ".")
                    x = "0" + x;
                if (x[x.Length - 1].ToString() == ".")
                    x = x.Remove(x.Length - 1, 1);
                if (x.IndexOf(".") > -1)
                    ret += NoToChinese(x.Substring(0, x.IndexOf("."))) + "点" + DecimalNumberToChinese(x.Substring(x.IndexOf(".") + 1));
                else
                    ret += NoToChinese(x);
            }
            catch (Exception ex)
            {
                ret = "";
                LogHelper.WriteMessage("整型数字或小数数字转换成中文数字出错：\r\rn" + ex.Message);
            }
            return ret;
        }

        /// <summary>
        /// 数字（只能为整型）转成中文
        /// </summary>
        /// <param name="x"></param>
        /// <returns>报错或错误返回空</returns>
        public static string IntNumToChinese(string x)
        {
            //数字转换为中文后的数组 //转载请注明来自   
            string[] P_array_num = new string[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            //为数字位数建立一个位数组  
            string[] P_array_digit = new string[] { "", "十", "百", "千" };
            //为数字单位建立一个单位数组  
            string[] P_array_units = new string[] { "", "万", "亿", "万亿" };
            string P_str_returnValue = ""; //返回值  
            int finger = 0; //字符位置指针  

            try
            {
                int P_int_m = x.Length % 4; //取模  
                int P_int_k = 0;
                if (P_int_m > 0)
                    P_int_k = x.Length / 4 + 1;
                else
                    P_int_k = x.Length / 4;
                //外层循环,四位一组,每组最后加上单位: ",万亿,",",亿,",",万,"  
                for (int i = P_int_k; i > 0; i--)
                {
                    int P_int_L = 4;
                    if (i == P_int_k && P_int_m != 0)
                        P_int_L = P_int_m;
                    //得到一组四位数  
                    string four = x.Substring(finger, P_int_L);
                    int P_int_l = four.Length;
                    //内层循环在该组中的每一位数上循环  
                    for (int j = 0; j < P_int_l; j++)
                    {
                        //处理组中的每一位数加上所在的位  
                        int n = Convert.ToInt32(four.Substring(j, 1));
                        if (n == 0)
                        {
                            if (j < P_int_l - 1 && Convert.ToInt32(four.Substring(j + 1, 1)) > 0 && !P_str_returnValue.EndsWith(P_array_num[n]))
                                P_str_returnValue += P_array_num[n];
                        }
                        else
                        {
                            if (!(n == 1 && (P_str_returnValue.EndsWith(P_array_num[0]) | P_str_returnValue.Length == 0) && j == P_int_l - 2))
                                P_str_returnValue += P_array_num[n];
                            P_str_returnValue += P_array_digit[P_int_l - j - 1];
                        }
                    }
                    finger += P_int_L;
                    //每组最后加上一个单位:",万,",",亿," 等  
                    if (i < P_int_k) //如果不是最高位的一组  
                    {
                        if (Convert.ToInt32(four) != 0)
                            //如果所有4位不全是0则加上单位",万,",",亿,"等  
                            P_str_returnValue += P_array_units[i - 1];
                    }
                    else
                    {
                        //处理最高位的一组,最后必须加上单位  
                        P_str_returnValue += P_array_units[i - 1];
                    }
                }
            }
            catch (Exception ex)
            {
                P_str_returnValue = "";
                LogHelper.WriteMessage("阿拉伯数字转换成中文数字错误：\r\rn" + ex.Message);
            }
            return P_str_returnValue;
        }

        /// <summary>
        /// 数字钱转成大写钱
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        public static string GetChinaMoney(decimal money)
        {
            string[] strArray;
            string str = "";
            string str2 = "";
            try
            {
                string str3 = money.ToString("0.00");
                switch (str3.Trim().Length)
                {
                    case 4:
                        strArray = new string[] { str3[0].ToString(), "y", str3[2].ToString(), "j", str3[3].ToString(), "f" };
                        str = string.Concat(strArray);
                        break;

                    case 5:
                        strArray = new string[] { str3[0].ToString(), "s", str3[1].ToString(), "y", str3[3].ToString(), "j", str3[4].ToString(), "f" };
                        str = string.Concat(strArray);
                        break;

                    case 6:
                        strArray = new string[] { str3[0].ToString(), "b", str3[1].ToString(), "s", str3[2].ToString(), "y", str3[4].ToString(), "j", str3[5].ToString(), "f" };
                        str = string.Concat(strArray);
                        break;

                    case 7:
                        strArray = new string[] { str3[0].ToString(), "q", str3[1].ToString(), "b", str3[2].ToString(), "s", str3[3].ToString(), "y", str3[5].ToString(), "j", str3[6].ToString(), "f" };
                        str = string.Concat(strArray);
                        break;

                    case 8:
                        strArray = new string[] { str3[0].ToString(), "w", str3[1].ToString(), "q", str3[2].ToString(), "b", str3[3].ToString(), "s", str3[4].ToString(), "y", str3[6].ToString(), "j", str3[7].ToString(), "f" };
                        str = string.Concat(strArray);
                        break;

                    case 9:
                        strArray = new string[] { str3[0].ToString(), "s", str3[1].ToString(), "w", str3[2].ToString(), "q", str3[3].ToString(), "b", str3[4].ToString(), "s", str3[5].ToString(), "y", str3[7].ToString(), "j", str3[8].ToString(), "f" };
                        str = string.Concat(strArray);
                        break;

                    case 10:
                        strArray = new string[] {
                        str3[0].ToString(), "b", str3[1].ToString(), "s", str3[2].ToString(), "w", str3[3].ToString(), "q", str3[4].ToString(), "b", str3[5].ToString(), "s", str3[6].ToString(), "y", str3[8].ToString(), "j",
                        str3[9].ToString(), "f"
                     };
                        str = string.Concat(strArray);
                        break;

                    case 11:
                        strArray = new string[] {
                        str3[0].ToString(), "q", str3[1].ToString(), "b", str3[2].ToString(), "s", str3[3].ToString(), "w", str3[4].ToString(), "q", str3[5].ToString(), "b", str3[6].ToString(), "s", str3[7].ToString(), "y",
                        str3[9].ToString(), "j", str3[10].ToString(), "f"
                     };
                        str = string.Concat(strArray);
                        break;

                    case 12:
                        strArray = new string[] {
                        str3[0].ToString(), "m", str3[1].ToString(), "q", str3[2].ToString(), "b", str3[3].ToString(), "s", str3[4].ToString(), "w", str3[5].ToString(), "q", str3[6].ToString(), "b", str3[7].ToString(), "s",
                        str3[8].ToString(), "y", str3[10].ToString(), "j", str3[11].ToString(), "f"
                     };
                        str = string.Concat(strArray);
                        break;
                }
                for (int i = 0; i < str.Trim().Length; i++)
                {
                    switch (str[i])
                    {
                        case '0':
                            str2 = str2 + "零";
                            break;

                        case '1':
                            str2 = str2 + "壹";
                            break;

                        case '2':
                            str2 = str2 + "贰";
                            break;

                        case '3':
                            str2 = str2 + "叁";
                            break;

                        case '4':
                            str2 = str2 + "肆";
                            break;

                        case '5':
                            str2 = str2 + "伍";
                            break;

                        case '6':
                            str2 = str2 + "陆";
                            break;

                        case '7':
                            str2 = str2 + "柒";
                            break;

                        case '8':
                            str2 = str2 + "捌";
                            break;

                        case '9':
                            str2 = str2 + "玖";
                            break;

                        case 'b':
                            str2 = str2 + "佰";
                            break;

                        case 'f':
                            str2 = str2 + "分";
                            break;

                        case 'j':
                            str2 = str2 + "角";
                            break;

                        case 'm':
                            str2 = str2 + "亿";
                            break;

                        case 'q':
                            str2 = str2 + "仟";
                            break;

                        case 's':
                            str2 = str2 + "拾";
                            break;

                        case 'w':
                            str2 = str2 + "万";
                            break;

                        case 'y':
                            str2 = str2 + "圆";
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                str2 = "";
                LogHelper.WriteMessage("转换成大写Money错误：\r\rn"+ex.Message);
            }
            return str2;
        }

        /// <summary>  
        /// 将中文数字转换成阿拉伯数字  
        /// </summary>  
        /// <param name="cnNumber"></param>  
        /// <returns></returns>  
        private static int ConverToDigit(string cnNumber)
        {
            int result = 0;
            int temp = 0;
            foreach (char c in cnNumber)
            {
                int temp1 = ToDigit(c);
                if (temp1 == 10000)
                {
                    result += temp;
                    result *= 10000;
                    temp = 0;
                }
                else if (temp1 > 9)
                {
                    if (temp1 == 10 && temp == 0) temp = 1;
                    result += temp * temp1;
                    temp = 0;
                }
                else temp = temp1;
            }
            result += temp;
            return result;

        }

        /// <summary>  
        /// 将中文数字转换成阿拉伯数字  
        /// </summary> 
        /// <param name="cn"></param>  
        /// <returns></returns>  
        private static int ToDigit(char cn)
        {
            int number = 0;
            switch (cn)
            {
                case '壹':
                case '一':
                    number = 1;
                    break;
                case '两':
                case '贰':
                case '二':
                    number = 2;
                    break;
                case '叁':
                case '三':
                    number = 3;
                    break;
                case '肆':
                case '四':
                    number = 4;
                    break;
                case '伍':
                case '五':
                    number = 5;
                    break;
                case '陆':
                case '六':
                    number = 6;
                    break;
                case '柒':
                case '七':
                    number = 7;
                    break;
                case '捌':
                case '八':
                    number = 8;
                    break;
                case '玖':
                case '九':
                    number = 9;
                    break;
                case '拾':
                case '十':
                    number = 10;
                    break;
                case '佰':
                case '百':
                    number = 100;
                    break;
                case '仟':
                case '千':
                    number = 1000;
                    break;
                case '萬':
                case '万':
                    number = 10000;
                    break;
                case '零':
                default:
                    number = 0;
                    break;
            }

            return number;

        }

        /// <summary>  
        /// 将中文数字转换成阿拉伯数字  
        /// </summary>  
        /// <param name="cnDigit"></param>  
        /// <returns></returns>  
        public static long ChineseToIntNumber(string cnDigit)
        {
            long result = 0;
            try
            {
                string[] str = cnDigit.Split('亿');
                result = ConverToDigit(str[0]);
                if (str.Length > 1)
                {
                    result *= 100000000;
                    result += ConverToDigit(str[1]);
                }
            }
            catch (Exception ex)
            {
                result = 0;
                LogHelper.WriteMessage("中文数字转换成阿拉伯数字错误：\r\rn" + ex.Message);
            }
            return result;
        }
    }
}
