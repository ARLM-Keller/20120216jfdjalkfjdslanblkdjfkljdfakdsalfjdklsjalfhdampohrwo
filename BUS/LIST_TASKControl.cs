using System;
using System.Collections.Generic;
using System.Text;
using DTO;
using DAO;
using System.Data;

namespace BUS
{
	/// <summary> 
	///Author: nnamthach@gmail.com 
	/// <summary>
    public class LIST_TASKControl
    {
		#region Local Variable
        private LIST_TASKDataAccess _objDAO;
		#endregion Local Variable
		
		#region Method
        public LIST_TASKControl()
        {
            _objDAO = new LIST_TASKDataAccess();
        }
		
        public LIST_TASKInfo Get(
        String DTB,
        String Code,
		ref string sErr)
        {
            return _objDAO.Get(
            DTB,
            Code,
			ref sErr);
        }
		
        public DataTable GetAll(
        String DTB,
        ref string sErr)
        {
            return _objDAO.GetAll(
            DTB,
            ref sErr);
        }
		public DataTable GetByPos(
        String DTB,
        int pos, ref string sErr)
        {
            return _objDAO.GetByPos(
            DTB,
            pos, ref sErr);
        }
		public int GetCountRecord(
        String DTB,
        ref string sErr)
        {
            return _objDAO.GetCountRecord(
            DTB,
            ref sErr);
        }
		
        public Int32 Add(LIST_TASKInfo obj, ref string sErr)
        {
            return _objDAO.Add(obj, ref sErr);
        }
		
        public string Update(LIST_TASKInfo obj)
        {
            return _objDAO.Update(obj);
        }
		
        public string Delete(
        String DTB,
        String Code
		)
        {
            return _objDAO.Delete(
            DTB,
            Code
			);
        }  
        public Boolean IsExist(
        String DTB,
        String Code
		)
        {
            return _objDAO.IsExist(
            DTB,
            Code
			);
        } 
		      		
		public DataTableCollection Get_Page(LIST_TASKInfo obj, string orderBy, int pageIndex, int pageSize,ref String sErr)
        {
            return _objDAO.Get_Page(obj, orderBy, pageIndex, pageSize, ref sErr);
        }
        
        public DataTable Search(String columnName, String columnValue, String condition, String tableName, ref String sErr)
        {           
            return _objDAO.Search(columnName, columnValue, condition, tableName, ref  sErr);
        }
        public string InsertUpdate(LIST_TASKInfo obj)
        {
            string sErr = "";
            if (IsExist(
            obj.DTB,
            obj.Code
			))
            {
                sErr = Update(obj);
            }
            else
                Add(obj, ref sErr);
            return sErr;
        }
		
        public DataTable GetTransferOut(string dtb, object from, object to, ref string sErr)
        {
            return _objDAO.GetTransferOut(dtb, from, to, ref sErr);
        }

        public DataTable ToTransferInStruct()
        {
			return LIST_TASKInfo.ToDataTable();             
        }
		
		public string TransferIn(DataRow row)
        {
            LIST_TASKInfo inf = new LIST_TASKInfo(row);
            return InsertUpdate(inf);
        }
		#endregion Method

    }
}
