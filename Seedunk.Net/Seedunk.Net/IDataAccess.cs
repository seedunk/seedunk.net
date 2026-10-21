 
using System;
using System.Collections.Generic; 
using System.Data;
 

namespace Seedunk.Net
{
    
    /// <summary>
    /// 数据操作接口
    /// </summary>
    public interface IDataAccess : IDisposable
    {
       
        // ---------- 执行命令 ----------
        /// <summary>
        /// 执行非查询SQL（INSERT/UPDATE/DELETE/DDL），返回受影响行数
        /// </summary>
        int ExecuteNonQuery(string sql, Dictionary<string,object> parameters = null, CommandType commandType = CommandType.Text);

        /// <summary>
        /// 执行查询，返回结果集的第一行第一列
        /// </summary>
        object ExecuteScalar(string sql, Dictionary<string,object> parameters = null, CommandType commandType = CommandType.Text);

        /// <summary>
        /// 执行查询，返回 DataReader（需调用方手动处理）
        /// </summary>
        DataTable Execute(string sql, Dictionary<string,object> parameters = null, CommandType commandType = CommandType.Text);



        /// <summary>
        /// 在一个事务中执行多个操作（委托方式）
        /// </summary>
        bool ExecuteInTransaction(Action<IDataAccess> action);

      
         
    }

 }