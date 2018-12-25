﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;
using System.Data;

namespace DapperExtensions
{
    public static partial class DapperExtension
    {
        #region common method for ado.net

        public static async Task<DataTable> GetDataTableAsync(this IDbConnection conn, string sql, object param = null, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await Task.Run(() =>
            {
                return GetDataTable(conn, sql, param, tran, commandTimeout, commandType);
            });
        }

        public static async Task<DataSet> GetDataSetAsync(this IDbConnection conn, string sql, object param = null, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await Task.Run(() =>
            {
                return GetDataSet(conn, sql, param, tran, commandTimeout, commandType);
            });
        }

        public static async Task<DataTable> GetSchemaTableAsync<T>(this IDbConnection conn, string returnFields = null, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await Task.Run(() =>
            {
                return GetSchemaTable<T>(conn, returnFields, tran, commandTimeout, commandType);
            });
        }

        public static async Task BulkCopyAsync(this IDbConnection conn, IDbTransaction tran, DataTable dt, string tableName, string copyFields = null, bool insert_identity = false, int batchSize = 20000, int timeOut = 100)
        {
            await Task.Run(() =>
           {
               BulkCopy(conn, tran, dt, tableName, copyFields, insert_identity, batchSize, timeOut);
           });

        }

        public static async Task BulkCopyAsync<T>(this IDbConnection conn, IDbTransaction tran, DataTable dt, string copyFields = null, bool insert_identity = false, int batchSize = 20000, int timeOut = 100)
        {
            await Task.Run(() =>
            {
                BulkCopy<T>(conn, tran, dt, copyFields, insert_identity, batchSize, timeOut);
            });
        }

        public static async Task BulkUpdateAsync(this IDbConnection conn, IDbTransaction tran, DataTable dt, string tableName, string column = "*", int batchSize = 20000, int timeOut = 100)
        {
            await Task.Run(() =>
            {
                BulkUpdate(conn, tran, dt, tableName, column, batchSize, timeOut);
            });
        }

        public static async Task BulkUpdateAsync<T>(this IDbConnection conn, IDbTransaction tran, DataTable dt, string column = "*", int batchSize = 20000, int timeOut = 100)
        {
            await Task.Run(() =>
            {
                BulkUpdate<T>(conn, tran, dt, column, batchSize, timeOut);
            });
        }

        #endregion

        #region method (Insert Update Delete)

        public static async Task<int> InsertAsync<T>(this IDbConnection conn, T model, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return await conn.ExecuteAsync(builder.InsertSql<T>(), model, tran, commandTimeout, commandType);
        }

        public static async Task<int> InsertIdentityAsync<T>(this IDbConnection conn, T model, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return await conn.ExecuteAsync(builder.InsertIdentitySql<T>(), model, tran, commandTimeout, commandType);
        }

        public static async Task<int> UpdateAsync<T>(this IDbConnection conn, T model, string updateFields = null, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return await conn.ExecuteAsync(builder.UpdateSql<T>(updateFields), model, tran, commandTimeout, commandType);
        }

        public static async Task<int> UpdateByWhereAsync<T>(this IDbConnection conn, string where, string updateFields, T model, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return await conn.ExecuteAsync(builder.UpdateByWhere<T>(where, updateFields), model, tran, commandTimeout, commandType);
        }

        public static async Task<int> InsertOrUpdateAsync<T>(this IDbConnection conn, T model, string updateFields = null, bool update = true, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await Task.Run(() =>
            {
                var builder = BuilderFactory.GetBuilder(conn);
                int effectRow = 0;
                dynamic total = conn.ExecuteScalar<dynamic>(builder.ExistsKeySql<T>(), model, tran, commandTimeout, commandType);
                if (total > 0)
                {
                    if (update)
                    {
                        effectRow += Update(conn, model, updateFields, tran, commandTimeout, commandType);
                    }
                }
                else
                {
                    effectRow += Insert(conn, model, tran, commandTimeout, commandType);
                }

                return effectRow;
            });
        }

        public static async Task<int> InsertIdentityOrUpdateAsync<T>(this IDbConnection conn, T model, string updateFields = null, bool update = true, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await Task.Run(() =>
            {
                var builder = BuilderFactory.GetBuilder(conn);
                int effectRow = 0;
                dynamic total = conn.ExecuteScalar<dynamic>(builder.ExistsKeySql<T>(), model, tran, commandTimeout, commandType);
                if (total > 0)
                {
                    if (update)
                    {
                        effectRow += Update(conn, model, updateFields, tran, commandTimeout, commandType);
                    }
                }
                else
                {
                    effectRow += InsertIdentity(conn, model, tran, commandTimeout, commandType);
                }

                return effectRow;
            });
        }

        public static async Task<int> DeleteAsync<T>(this IDbConnection conn, object id, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return await conn.ExecuteAsync(builder.DeleteById<T>(), new { id = id }, tran, commandTimeout, commandType);
        }

        public static async Task<int> DeleteByIdsAsync<T>(this IDbConnection conn, object ids, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            DynamicParameters dpar = new DynamicParameters();
            dpar.Add("@ids", ids);
            return await conn.ExecuteAsync(builder.DeleteByIds<T>(), dpar, tran, commandTimeout, commandType);
        }

        public static async Task<int> DeleteByWhereAsync<T>(this IDbConnection conn, string where, object param, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return await conn.ExecuteAsync(builder.DeleteAllSql<T>() + where, param, tran, commandTimeout, commandType);
        }

        public static async Task<int> DeleteAllAsync<T>(this IDbConnection conn, IDbTransaction tran = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var builder = BuilderFactory.GetBuilder(conn);
            return await conn.ExecuteAsync(builder.DeleteAllSql<T>(), null, tran, commandTimeout, commandType);

        }


        #endregion

    }
}
