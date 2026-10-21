 
using Seedunk.Net.Models;
using System.Collections.Generic;
 
namespace  Seedunk.Net
{
    /// <summary>
    /// 查询接口类
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="connectString">连接字符串</param>
        /// <returns></returns>
        ResultResponse Connect(Dictionary<string, object> ConnectArgs );
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        ResultResponse Disonnect();
        /// <summary>
        /// 执行SQL并返回影响行数
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        AffectedResponse  ExecuteNonQuery(string sql, Dictionary<string, object> Parameters = null);
        /// <summary>
        /// 执行SQL并返回执行结果
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        ObjectResponse ExecuteScalar(string sql, Dictionary<string, object> Parameters = null);
        /// <summary>
        /// 执行SQL并返回数据集
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        QueryResponse GetDataTable(string sql, Dictionary<string, object> Parameters = null);

    }
}
