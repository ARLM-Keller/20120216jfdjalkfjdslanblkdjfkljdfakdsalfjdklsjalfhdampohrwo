using System;
using System.Collections.Generic;
using System.Text;
using DTO;
using System.Data;
using System.Data.SqlClient;

namespace DAO
{
    /// <summary> 
    ///Author: nnamthach@gmail.com 
    /// <summary>
    public class LIST_QD_SCHEMADataAccess : Connection
    {
        #region Local Variable
        private string _strSPInsertName = "dbo.[procLIST_QD_SCHEMA_add]";
        private string _strSPUpdateName = "dbo.[procLIST_QD_SCHEMA_update]";
        private string _strSPDeleteName = "dbo.[procLIST_QD_SCHEMA_delete]";
        private string _strSPGetName = "dbo.[procLIST_QD_SCHEMA_get]";
        private string _strSPGetFieldName = "dbo.[procLIST_QD_SCHEMA_getfield]";
        private string _strSPGetDefaultDBName = "procLIST_QD_SCHEMA_getdb";
        private string _strSPGetJoinsName = "procLIST_QD_SCHEMA_getjoins";
        private string _strSPGetAllName = "dbo.[procLIST_QD_SCHEMA_getall]";
        private string _strSPGetPages = "dbo.[procLIST_QD_SCHEMA_getpaged]";
        private string _strSPIsExist = "dbo.[procLIST_QD_SCHEMA_isexist]";
        private string _strTableName = "[LIST_QD_SCHEMA]";
        private string _strSPGetTransferOutName = "dbo.[procLIST_QD_SCHEMA_gettransferout]";
        #endregion Local Variable

        #region Method
        public LIST_QD_SCHEMAInfo Get(
        String CONN_ID,
        String SCHEMA_ID,
        ref string sErr)
        {
            LIST_QD_SCHEMAInfo objEntr = new LIST_QD_SCHEMAInfo(); DataTable list = new DataTable();
            try
            {
                connect();
                InitSPCommand(_strSPGetName);
                AddParameter(LIST_QD_SCHEMAInfo.Field.CONN_ID.ToString(), CONN_ID);
                AddParameter(LIST_QD_SCHEMAInfo.Field.SCHEMA_ID.ToString(), SCHEMA_ID);


                list = executeSelectSP(command);
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }
            finally
            {
                disconnect();
            }

            if (list.Rows.Count > 0)
                objEntr = (LIST_QD_SCHEMAInfo)GetDataFromDataRow(list, 0);
            //if (dr != null) list = CBO.FillCollection(dr, ref list);
            if (sErr != "") ErrorLog.SetLog(sErr);
            return objEntr;
        }

        protected override object GetDataFromDataRow(DataTable dt, int i)
        {
            LIST_QD_SCHEMAInfo result = new LIST_QD_SCHEMAInfo();
            result.CONN_ID = (dt.Rows[i][LIST_QD_SCHEMAInfo.Field.CONN_ID.ToString()] == DBNull.Value ? "" : Convert.ToString(dt.Rows[i][LIST_QD_SCHEMAInfo.Field.CONN_ID.ToString()]));
            result.SCHEMA_ID = (dt.Rows[i][LIST_QD_SCHEMAInfo.Field.SCHEMA_ID.ToString()] == DBNull.Value ? "" : Convert.ToString(dt.Rows[i][LIST_QD_SCHEMAInfo.Field.SCHEMA_ID.ToString()]));
            result.LOOK_UP = (dt.Rows[i][LIST_QD_SCHEMAInfo.Field.LOOK_UP.ToString()] == DBNull.Value ? "" : Convert.ToString(dt.Rows[i][LIST_QD_SCHEMAInfo.Field.LOOK_UP.ToString()]));
            result.DESCRIPTN = (dt.Rows[i][LIST_QD_SCHEMAInfo.Field.DESCRIPTN.ToString()] == DBNull.Value ? "" : Convert.ToString(dt.Rows[i][LIST_QD_SCHEMAInfo.Field.DESCRIPTN.ToString()]));
            result.FIELD_TEXT = (dt.Rows[i][LIST_QD_SCHEMAInfo.Field.FIELD_TEXT.ToString()] == DBNull.Value ? "" : Convert.ToString(dt.Rows[i][LIST_QD_SCHEMAInfo.Field.FIELD_TEXT.ToString()]));
            result.FROM_TEXT = (dt.Rows[i][LIST_QD_SCHEMAInfo.Field.FROM_TEXT.ToString()] == DBNull.Value ? "" : Convert.ToString(dt.Rows[i][LIST_QD_SCHEMAInfo.Field.FROM_TEXT.ToString()]));
            result.DAG = (dt.Rows[i][LIST_QD_SCHEMAInfo.Field.DAG.ToString()] == DBNull.Value ? "" : Convert.ToString(dt.Rows[i][LIST_QD_SCHEMAInfo.Field.DAG.ToString()]));
            result.SCHEMA_STATUS = (dt.Rows[i][LIST_QD_SCHEMAInfo.Field.SCHEMA_STATUS.ToString()] == DBNull.Value ? "" : Convert.ToString(dt.Rows[i][LIST_QD_SCHEMAInfo.Field.SCHEMA_STATUS.ToString()]));
            result.UPDATED = (dt.Rows[i][LIST_QD_SCHEMAInfo.Field.UPDATED.ToString()] == DBNull.Value ? 0 : Convert.ToInt32(dt.Rows[i][LIST_QD_SCHEMAInfo.Field.UPDATED.ToString()]));
            result.ENTER_BY = (dt.Rows[i][LIST_QD_SCHEMAInfo.Field.ENTER_BY.ToString()] == DBNull.Value ? "" : Convert.ToString(dt.Rows[i][LIST_QD_SCHEMAInfo.Field.ENTER_BY.ToString()]));
            result.DEFAULT_CONN = (dt.Rows[i][LIST_QD_SCHEMAInfo.Field.DEFAULT_CONN.ToString()] == DBNull.Value ? "" : Convert.ToString(dt.Rows[i][LIST_QD_SCHEMAInfo.Field.DEFAULT_CONN.ToString()]));

            return result;
        }

        public DataTable GetAll(string conn,
        ref string sErr)
        {
            DataTable list = new DataTable();
            try
            {
                connect();
                InitSPCommand(_strSPGetAllName);
                AddParameter("CONN_ID", conn);

                list = executeSelectSP();
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }
            disconnect();


            if (sErr != "") ErrorLog.SetLog(sErr);
            return list;
        }
        /// <summary>
        /// Return 1: Table is exist Identity Field
        /// Return 0: Table is not exist Identity Field
        /// Return -1: Erro
        /// </summary>
        /// <param name="tableName"></param>
        public Int32 Add(LIST_QD_SCHEMAInfo objEntr, ref string sErr)
        {
            int ret = -1; try
            {
                connect();
                InitSPCommand(_strSPInsertName);
                AddParameter(LIST_QD_SCHEMAInfo.Field.CONN_ID.ToString(), objEntr.CONN_ID);
                AddParameter(LIST_QD_SCHEMAInfo.Field.SCHEMA_ID.ToString(), objEntr.SCHEMA_ID);
                AddParameter(LIST_QD_SCHEMAInfo.Field.LOOK_UP.ToString(), objEntr.LOOK_UP);
                AddParameter(LIST_QD_SCHEMAInfo.Field.DESCRIPTN.ToString(), objEntr.DESCRIPTN);
                AddParameter(LIST_QD_SCHEMAInfo.Field.FIELD_TEXT.ToString(), objEntr.FIELD_TEXT);
                AddParameter(LIST_QD_SCHEMAInfo.Field.FROM_TEXT.ToString(), objEntr.FROM_TEXT);
                AddParameter(LIST_QD_SCHEMAInfo.Field.DAG.ToString(), objEntr.DAG);
                AddParameter(LIST_QD_SCHEMAInfo.Field.SCHEMA_STATUS.ToString(), objEntr.SCHEMA_STATUS);
                AddParameter(LIST_QD_SCHEMAInfo.Field.UPDATED.ToString(), objEntr.UPDATED);
                AddParameter(LIST_QD_SCHEMAInfo.Field.ENTER_BY.ToString(), objEntr.ENTER_BY);
                AddParameter(LIST_QD_SCHEMAInfo.Field.DEFAULT_CONN.ToString(), objEntr.DEFAULT_CONN);


                //command.ExecuteNonQuery();
                object tmp = executeSPScalar();
                if (tmp != null && tmp != DBNull.Value)
                    ret = Convert.ToInt32(tmp);
                else
                    ret = 0;
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }
            disconnect();
            if (sErr != "") ErrorLog.SetLog(sErr);

            return ret;
        }

        public string Update(LIST_QD_SCHEMAInfo objEntr)
        {
            string sErr = "";
            try
            {
                connect();
                InitSPCommand(_strSPUpdateName);
                AddParameter(LIST_QD_SCHEMAInfo.Field.CONN_ID.ToString(), objEntr.CONN_ID);
                AddParameter(LIST_QD_SCHEMAInfo.Field.SCHEMA_ID.ToString(), objEntr.SCHEMA_ID);
                AddParameter(LIST_QD_SCHEMAInfo.Field.LOOK_UP.ToString(), objEntr.LOOK_UP);
                AddParameter(LIST_QD_SCHEMAInfo.Field.DESCRIPTN.ToString(), objEntr.DESCRIPTN);
                AddParameter(LIST_QD_SCHEMAInfo.Field.FIELD_TEXT.ToString(), objEntr.FIELD_TEXT);
                AddParameter(LIST_QD_SCHEMAInfo.Field.FROM_TEXT.ToString(), objEntr.FROM_TEXT);
                AddParameter(LIST_QD_SCHEMAInfo.Field.DAG.ToString(), objEntr.DAG);
                AddParameter(LIST_QD_SCHEMAInfo.Field.SCHEMA_STATUS.ToString(), objEntr.SCHEMA_STATUS);
                AddParameter(LIST_QD_SCHEMAInfo.Field.UPDATED.ToString(), objEntr.UPDATED);
                AddParameter(LIST_QD_SCHEMAInfo.Field.ENTER_BY.ToString(), objEntr.ENTER_BY);
                AddParameter(LIST_QD_SCHEMAInfo.Field.DEFAULT_CONN.ToString(), objEntr.DEFAULT_CONN);


                excuteSPNonQuery();
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }
            disconnect();
            if (sErr != "") ErrorLog.SetLog(sErr);
            return sErr;
        }

        public string Delete(
        String CONN_ID,
        String SCHEMA_ID
        )
        {
            string sErr = "";
            try
            {
                connect();
                InitSPCommand(_strSPDeleteName);
                AddParameter(LIST_QD_SCHEMAInfo.Field.CONN_ID.ToString(), CONN_ID);
                AddParameter(LIST_QD_SCHEMAInfo.Field.SCHEMA_ID.ToString(), SCHEMA_ID);


                excuteSPNonQuery();
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }
            disconnect();
            if (sErr != "") ErrorLog.SetLog(sErr);
            return sErr;
        }

        public DataTableCollection Get_Page(LIST_QD_SCHEMAInfo obj, string orderBy, int pageIndex, int pageSize, ref String sErr)
        {
            string whereClause = CreateWhereClause(obj);
            DataTableCollection dtList = null; try
            {
                connect();
                InitSPCommand(_strSPGetPages);

                AddParameter("WhereClause", whereClause);
                AddParameter("OrderBy", orderBy);
                AddParameter("PageIndex", pageIndex);
                AddParameter("PageSize", pageSize);


                dtList = executeCollectSelectSP();
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }
            disconnect();
            if (sErr != "") ErrorLog.SetLog(sErr);
            return dtList;
        }

        public Boolean IsExist(
        String CONN_ID,
        String SCHEMA_ID
        )
        {
            string sErr = "";
            DataTable list = new DataTable();
            try
            {
                connect();
                InitSPCommand(_strSPIsExist);
                AddParameter(LIST_QD_SCHEMAInfo.Field.CONN_ID.ToString(), CONN_ID);
                AddParameter(LIST_QD_SCHEMAInfo.Field.SCHEMA_ID.ToString(), SCHEMA_ID);


                list = executeSelectSP();
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }
            disconnect();
            if (sErr != "") ErrorLog.SetLog(sErr);
            if (list.Rows.Count == 1)
                return true;
            return false;
        }

        private string CreateWhereClause(LIST_QD_SCHEMAInfo obj)
        {
            String result = "";

            return result;
        }

        public DataTable Search(string columnName, string columnValue, string condition, string tableName, ref string sErr)
        {
            string query = "select * from " + _strTableName + " where " + columnName + " " + condition + " " + columnValue;
            DataTable list = new DataTable(); try
            {
                connect();

                list = executeSelectQuery(query);
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }
            disconnect();
            //if (dr != null) list = CBO.FillCollection(dr, ref list);
            //    if (sErr != "") ErrorLog.SetLog(sErr);
            return list;
        }
        public DataTable GetTransferOut(string dtb, object from, object to, ref string sErr)
        {
            DataTable list = new DataTable();
            try
            {
                connect();
                InitSPCommand(_strSPGetTransferOutName);
                AddParameter("DB", dtb);
                AddParameter("FROM", from);
                AddParameter("TO", to);

                list = executeSelectSP();
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }
            disconnect();


            if (sErr != "") ErrorLog.SetLog(sErr);
            return list;
        }
        #endregion Method
        public DataTable GetJoins(string dtb, ref string sErr)
        {
            DataTable list = new DataTable();
            try
            {
                InitConnect();
                InitSPCommand(_strSPGetJoinsName);
                AddParameter(LIST_QD_SCHEMAInfo.Field.CONN_ID.ToString(), dtb);


                list = executeSelectSP();
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }


            if (sErr != "") ErrorLog.SetLog(sErr);

            return list;
        }
        public string GetField(String CONN_ID, String SCHEMA_ID, ref string sErr)
        {
            object objEntr = null;
            try
            {
                connect();
                InitSPCommand(_strSPGetFieldName);
                AddParameter(LIST_QD_SCHEMAInfo.Field.CONN_ID.ToString(), CONN_ID);
                AddParameter(LIST_QD_SCHEMAInfo.Field.SCHEMA_ID.ToString(), SCHEMA_ID);

                objEntr = executeSPScalar();
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }
            finally
            {
                disconnect();
            }

            if (sErr != "") ErrorLog.SetLog(sErr);
            if (objEntr != null)
                return objEntr.ToString();
            else return "";
        }

        public string GetDefaultDB(string db, string table, ref string sErr)
        {
            object objEntr = null;
            try
            {
                connect();
                InitSPCommand(_strSPGetDefaultDBName);
                AddParameter(LIST_QD_SCHEMAInfo.Field.CONN_ID.ToString(), db);
                AddParameter(LIST_QD_SCHEMAInfo.Field.SCHEMA_ID.ToString(), table);

                objEntr = executeSPScalar();
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }
            finally
            {
                disconnect();
            }

            if (sErr != "") ErrorLog.SetLog(sErr);
            if (objEntr != null)
                return objEntr.ToString();
            else return "";
        }
    }
}
