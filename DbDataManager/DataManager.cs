using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace DataAdmin.DbDataManager
{
    static class DataManager
    {
        #region VARIABLES

        private static MySqlConnection _connection;
        private static MySqlCommand _sqlCommand;
        private const string TblUsers = "tbl_users";

        private static readonly List<string> QueryQueue = new List<string>();
        private const int MaxQueueSize = 200;

        #endregion

        /// <summary>
        /// Initialize connection to DB
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="database">Database</param>
        /// <param name="user">User</param>
        /// <param name="password">Password</param>
        /// <returns>Return true if connection success</returns>
        public static bool Initialize(string host, string database, string user, string password)
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                CloseConnection();
            }

            var connectionString = "SERVER=" + host + ";UID=" + user + ";PASSWORD=" + password;
            _connection = new MySqlConnection(connectionString);

            if (OpenConnection())
            {
                CreateDataBase(database);
                if (_connection.State == ConnectionState.Open)
                    _connection.Clone();
            }
            else
                return false;

            var connectionDbString = "SERVER=" + host + ";DATABASE=" + database + ";UID=" + user + ";PASSWORD=" + password;
            _connection = new MySqlConnection(connectionDbString);

            if (OpenConnection())
            {
                CreateTables();
                return true;
            }
            return false;
        }
        /// <summary>
        /// Open connection to db
        /// </summary>
        /// <returns></returns>
        private static bool OpenConnection()
        {
            try
            {
                _connection.Open();

                if (_connection.State == ConnectionState.Open)
                {
                    _sqlCommand = _connection.CreateCommand();
                    _sqlCommand.CommandText = "SET AUTOCOMMIT=0;";
                    _sqlCommand.ExecuteNonQuery();

                    return true;
                }
            }
            catch (MySqlException)
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// close connection to db
        /// </summary>
        private static void CloseConnection()
        {
            if ((_connection.State != ConnectionState.Open) || (_connection.State == ConnectionState.Broken)) return;
            _sqlCommand.CommandText = "COMMIT;";
            _sqlCommand.ExecuteNonQuery();
            _connection.Close();
        }

        /// <summary>
        /// Is connected to database
        /// </summary>
        /// <returns></returns>
        public static bool IsConnected()
        {
            return _connection.State == ConnectionState.Open;
        }

        /// <summary>
        /// execute sql request
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static void DoSql(string sql)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    OpenConnection();
                }
                _sqlCommand.CommandText = sql;
                _sqlCommand.ExecuteNonQuery();
                
            }
            catch (MySqlException)
            {
                
            }
        }

        /// <summary>
        /// Return reader for input SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static MySqlDataReader GetReader(String sql)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    OpenConnection();
                }

                var command = _connection.CreateCommand();
                command.CommandText = sql;
                var reader = command.ExecuteReader();

                return reader;

            }
            catch (Exception)
            {
                return null;
            }

        }

               /// <summary>
        /// create database
        /// </summary>        
        /// <param name="dataBaseName"></param>
        private static void CreateDataBase(string dataBaseName)
        {
            var sql = "CREATE DATABASE IF NOT EXISTS `" + dataBaseName + "`;COMMIT;";
            DoSql(sql);
        }

        /// <summary>
        /// Create tables
        /// </summary>
        private static void CreateTables()
        {            
            const string createUsersSql = "CREATE TABLE  IF NOT EXISTS `" + TblUsers + "` ("
                                     + "`ID` INT(12) UNSIGNED  NOT NULL AUTO_INCREMENT,"
                                     + "`UserName` VARCHAR(50) NOT NULL,"
                                     + "`UserPassword` VARCHAR(50) NOT NULL,"
                                     + "`UserFullName` VARCHAR(100) NULL,"
                                     + "`UserEmail` VARCHAR(50) NULL,"
                                     + "`UserPhone` VARCHAR(50) NULL,"
                                     + "`UserIpAddress` VARCHAR(50) NULL,"
                                     + "`UserBlocked` TINYINT(1) NULL,"
                                     + "`UserAllowDataNet` TINYINT(1) NULL,"
                                     + "`UserAllowTickNet` TINYINT(1) NULL,"
                                     + "`UserAllowLocal` TINYINT(1) NULL,"
                                     + "`UserAllowRemote` TINYINT(1) NULL,"


                                     + "PRIMARY KEY (`ID`,`UserName`)"
                                     + ")"
                                     + "COLLATE='latin1_swedish_ci'"
                                     + "ENGINE=InnoDB;";
            DoSql(createUsersSql);            
        }

        private static void AddToQueue(string sql)
        {
            QueryQueue.Add(sql);
            if (QueryQueue.Count >= MaxQueueSize)
            {
                CommitQueue();
            }
        }

        internal static void CommitQueue()
        {
            if (QueryQueue.Count <= 0) return;

            var fullSql = QueryQueue.Aggregate("", (current, t) => current + t);
            fullSql += "COMMIT;";
            DoSql(fullSql);

            QueryQueue.Clear();
        }
    }
}
