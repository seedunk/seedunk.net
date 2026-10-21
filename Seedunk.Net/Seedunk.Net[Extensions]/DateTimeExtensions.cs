using System;
namespace Seedunk.Net 
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Format(this DateTime dateTime, string format= "yyyy-MM-dd HH:mm:ss")
        {
            return dateTime.ToString(format);
        }

    }
}
