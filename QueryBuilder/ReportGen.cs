﻿using System;
using System.Data;
using System.Configuration;
using FlexCel.Core;
using FlexCel.Report;
using FlexCel.XlsAdapter;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using QueryBuilder;
using FlexCel.Render;
using System.Collections.Generic;

namespace QueryBuilder
{
    public class ReportGen
    {
        string _FilterPara = "FilterPara";
        public static bool User2007
        {
            set { _format = value ? TFileFormats.Xlsx : TFileFormats.Xls; }
            get
            {
                return _format == TFileFormats.Xlsx;
            }
        }
        public static string Ext
        {
            get { return User2007 ? ".xlsx" : ".xls"; }
        }
        public string FilterPara
        {
            get { return _FilterPara; }
            set { _FilterPara = value; }
        }
        string _qdCode = "";
        public static string __connectString = string.Empty;
        public string QdCode
        {
            get { return _qdCode; }
            set { _qdCode = value; }
        }
        string _queryText = "";
        string _sErr = "";
        public string _fileName = "";
        string _database = "";
        string _pathTemplate = string.Empty;
        string _pathReport = string.Empty;
        ExcelFile _xlsFile = null;
        DataSet _dtSet = null;
        string _name = string.Empty;
        public static TFileFormats _format = TFileFormats.Xls;
        public string Name
        {

            get { return _name; }
            set { _name = value; }
        }
        static QueryBuilder.SQLBuilder _sqlBuilder = new QueryBuilder.SQLBuilder(QueryBuilder.processingMode.Details);

        public static QueryBuilder.SQLBuilder SqlBuilder
        {
            get { return ReportGen._sqlBuilder; }
            set { ReportGen._sqlBuilder = value; }
        }

        private DataTable CreateFilterTable(QueryBuilder.SQLBuilder sqlBuilder)
        {
            DataTable dt = new DataTable(_FilterPara);
            DataRow row = dt.NewRow();
            _sqlBuilder = sqlBuilder;
            if (_sqlBuilder.Filters.Count > 0)
            {
                dt.Rows.Add(row);
                int dem = 1;
                for (int i = 0; i < _sqlBuilder.Filters.Count; i++)
                {
                    if (!dt.Columns.Contains(_sqlBuilder.Filters[i].Code + "_From"))
                    {
                        string code = _sqlBuilder.Filters[i].Code + "_From";
                        string value = _sqlBuilder.Filters[i].FilterFrom;
                        string type = _sqlBuilder.Filters[i].Node.FType;

                        SetParameter(dt, code, value, type);

                        code = _sqlBuilder.Filters[i].Code + "_To";
                        value = _sqlBuilder.Filters[i].FilterTo;

                        SetParameter(dt, code, value, type);

                    }
                    else
                    {
                        //dt.Columns.Add(_sqlBuilder.Filters[i].Code + "_From" + i);
                        //dt.Columns.Add(_sqlBuilder.Filters[i].Code + "_To" + i);

                        string code = _sqlBuilder.Filters[i].Code + "_From" + i;
                        string value = _sqlBuilder.Filters[i].FilterFrom;
                        string type = _sqlBuilder.Filters[i].Node.FType;

                        SetParameter(dt, code, value, type);

                        code = _sqlBuilder.Filters[i].Code + "_To" + i;
                        value = _sqlBuilder.Filters[i].FilterTo;

                        SetParameter(dt, code, value, type);

                        //dt.Rows[0][_sqlBuilder.Filters[i].Code + "_From" + i] = _sqlBuilder.Filters[i].FilterFrom;
                        //dt.Rows[0][_sqlBuilder.Filters[i].Code + "_To" + i] = _sqlBuilder.Filters[i].FilterTo;

                    }
                }
            }
            return dt;
        }

        private static void SetParameter(DataTable dt, string code, string value, string type)
        {
            dt.Columns.Add(code);
            if (value == "C")
            {
                if (type == "D")
                {
                    dt.Rows[0][code] = DateTime.Today.ToString("yyyy-MM-dd");
                }
                else if (type == "SDN")
                {
                    dt.Rows[0][code] = DateTime.Today.ToString("yyyyMMdd");
                }
                else if (type == "SPN")
                {
                    dt.Rows[0][code] = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString("000");
                }
                else
                    dt.Rows[0][code] = value;
            }
            else
                dt.Rows[0][code] = value;
        }
        private void LoadUdfs(ExcelFile Xls)
        {
            Xls.AddUserDefinedFunction(TUserDefinedFunctionScope.Local, TUserDefinedFunctionLocation.Internal, new TT_XLB_EB());
        }
        private ExcelFile AddData(ExcelFile Xls)
        {
            LoadUdfs(Xls);
            try
            {
                TUnsupportedFormulaList a = Xls.RecalcAndVerify();
            }
            catch (Exception ex)
            {
            }
            if (clsListValueTT_XLB_EB.Values.Count > 0)
            {
                foreach (TPoint x in clsListValueTT_XLB_EB.Values.Keys)
                {
                    Xls.SetCellValue(x.X, x.Y, clsListValueTT_XLB_EB.Values[x]);
                }
                clsListValueTT_XLB_EB.Values.Clear();
            }
            Xls.AllowOverwritingFiles = true;
            //Xls.Save(filename);
            return Xls;
        }
        private ExcelFile Run_templatereport(FlexCelReport flexcelreport)
        {
            flexcelreport.SetUserFunction("DBEGIN", new DBEGIN());
            flexcelreport.SetUserFunction("DEND", new DEND());
            flexcelreport.SetUserFunction("STR2NUM", new STR2NUM());
            flexcelreport.SetUserFunction("NUM2ROMAN", new NUM2ROMAN());
            flexcelreport.SetUserFunction("SUNDATE2DATE", new SUNDATE2DATE());
            flexcelreport.SetUserFunction("PERIOD2STR", new PERIOD2STR());
            flexcelreport.SetUserFunction("NUM2STR", new NUM2STR());
            flexcelreport.SetUserFunction("Read_VN", new Read_VN());
            flexcelreport.SetUserFunction("Read_EN", new Read_EN());

            flexcelreport.SetUserFunction("PH", new PH());
            flexcelreport.SetUserFunction("PE", new PE());
            flexcelreport.SetUserFunction("PA", new PA());

            flexcelreport.SetUserFunction("YA", new YA());
            flexcelreport.SetUserFunction("YH", new YH());
            flexcelreport.SetUserFunction("YE", new YE());
            flexcelreport.SetUserFunction("YK", new YK());
            object misValue = System.Reflection.Missing.Value;
            string filename = "";

            filename = _pathTemplate + _qdCode + ".template" + ReportGen.Ext;
            if (!File.Exists(filename))
            {
                throw new Exception("Template Report is not exist!");
                return null;
            }

            ExcelFile result = new XlsFile(filename);
            flexcelreport.Run(result);
            return result;
        }

        #region Userfuntion
        private class TT_XLB_EB : TUserDefinedFunction
        {
            public TT_XLB_EB() : base("TT_XLB_EB") { }
            public override object Evaluate(TUdfEventArgs arguments, object[] parameters)
            {
                #region Get Parameters
                int XF = 0;
                TFlxFormulaErrorValue Err = TFlxFormulaErrorValue.ErrValue;
                TFormula tmp = (TFormula)arguments.Xls.GetCellValue(arguments.Sheet, arguments.Row, arguments.Col, ref XF);
                QueryBuilder.SQLBuilder sqlBuilder = new SQLBuilder(processingMode.Balance);

                string formular = tmp.Text;
                object[] para = new object[parameters.Length - 1];
                TXls3DRange DescCell = new TXls3DRange();
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (i == 1)
                    {
                        if (!TryGetCellRange(parameters[i], out DescCell, out Err))
                        {
                            break;
                        }
                        //formular = formular.Replace("{P}" + i, parameters[i].ToString());
                    }
                    else
                    {
                        TXls3DRange SourceCell;
                        if (!TryGetCellRange(parameters[i], out SourceCell, out Err))
                            break;
                        if (SourceCell.IsOneCell)
                        {
                            string value = "";
                            if (!TryGetString(arguments.Xls, parameters[i], out value, out Err))
                                return Err;
                            sqlBuilder.ParaValueList[i - 1] = value;
                            //TCellAddress a = new TCellAddress(SourceCell.Top, SourceCell.Left, false, false);

                            //formular = formular.Replace(a.CellRef, value);
                            formular = formular.Replace("{P}" + (i - 1), value);
                        }
                    }
                }

                #endregion Get Parameters
                //formular = formular.Replace("$", "");
                Parsing.Formular2SQLBuilder(formular, ref sqlBuilder);
                string query = sqlBuilder.BuildSQLEx("");
                CoreCommonControl control = new CoreCommonControl();
                object result = sqlBuilder.BuildObject(query, ReportGen.__connectString);
                //formular = sqlBuilder.BuildTTformula();
                arguments.Xls.SetComment(DescCell.Top, DescCell.Left, formular);
                TPoint x = new TPoint(DescCell.Top, DescCell.Left);
                if (result != DBNull.Value)
                {
                    clsListValueTT_XLB_EB.Values.Add(x, result);
                    //arguments.Xls.SetCellValue(DescCell.Top, DescCell.Left, result.ToString());
                }

                else
                {
                    result = 0;
                    clsListValueTT_XLB_EB.Values.Add(x, result);
                }
                return result;
            }

        }
        class STR2NUM : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                string chuoi = parameters[0].ToString();
                decimal kq = 0;
                try
                {
                    kq = Convert.ToDecimal(chuoi);
                }
                catch (System.Exception ex)
                {

                }
                return kq;
            }

        }
        class NUM2ROMAN : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                int chuoi = Convert.ToInt32(parameters[0]);
                String kq = "";
                try
                {

                    kq = CoreCommonControl.ToRoman(chuoi);
                }
                catch (System.Exception ex)
                {

                }
                return kq;
            }

        }
        class SUNDATE2DATE : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                String chuoi = parameters[0].ToString();
                string kq = "";
                try
                {
                    string year = chuoi.Substring(0, 4);
                    string month = chuoi.Substring(4, 2);
                    string day = chuoi.Substring(6, 2);
                    if (chuoi != "19000101")
                    {
                        kq = day + "/" + month + "/" + year;
                    }
                }
                catch (System.Exception ex)
                {

                }

                return kq;
            }

        }

        class PERIOD2STR : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                String chuoi = parameters[0].ToString();
                String kq = "";
                try
                {

                    kq = chuoi.Substring(5, 2) + "/" + chuoi.Substring(0, 4);

                }
                catch (System.Exception ex)
                {

                }

                return kq;
            }

        }
        class NUM2STR : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                String kq = "";
                if (parameters == null || parameters.Length != 2)
                    throw new ArgumentException("Invalid number of params for user defined function \"NUM2STR\"");
                try
                {
                    Decimal para = Convert.ToDecimal(parameters[0]);
                    String chuoi = parameters[1].ToString();
                    string fm = chuoi.Replace("#", "").Replace("0", "");
                    switch (fm)
                    {
                        case ".,":
                        case ",":
                            System.Globalization.CultureInfo a = new System.Globalization.CultureInfo("de-DE");
                            kq = para.ToString(chuoi.Replace(",", "_").Replace(".", ",").Replace("_", "."), a);
                            break;
                        case ",.":
                        case ".":
                            System.Globalization.CultureInfo b = new System.Globalization.CultureInfo("en-US");
                            kq = para.ToString(chuoi, b);
                            break;
                    }
                }
                catch (System.Exception ex)
                {
                    throw new ArgumentException("Invalid number of params for user defined function \"NUM2STR");
                }

                return kq;
            }

        }
        class Read_VN : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                if (parameters[0] == null)
                    return "";
                String chuoi = parameters[0].ToString();
                String kq = "";
                try
                {
                    ReadVN readtv = new ReadVN();
                    kq = readtv.Convert(chuoi.Trim(), '.', " lẻ ");
                    kq[0].ToString().ToUpper();
                }
                catch (System.Exception ex)
                {

                }

                return kq;
            }

        }
        class Read_EN : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                if (parameters[0] == null)
                    return "";
                Double chuoi = Convert.ToDouble(parameters[0]);
                String kq = "";
                try
                {
                    ReadEN readtv = new ReadEN();
                    kq = readtv.NumberToWords(chuoi);
                }
                catch (System.Exception ex)
                {

                }

                return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(kq);
            }

        }
        class DBEGIN : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                String chuoi = parameters[0].ToString();
                String kq = "";
                try
                {
                    string periodParterm = @"^[0-9]{7}$";
                    string sunDateParterm = @"^[0-9]{8}$";
                    if (parameters[0].GetType() == typeof(DateTime))
                    {
                        DateTime date = Convert.ToDateTime(parameters[0]);
                        kq = "1/" + date.Month + "/" + date.Year;
                    }
                    else if (Regex.IsMatch(chuoi, periodParterm))
                    {
                        int year = Convert.ToInt32(chuoi) / 1000;
                        int month = Convert.ToInt32(chuoi) - year * 1000;
                        kq = "1/" + month + "/" + year;
                    }
                    else if (Regex.IsMatch(chuoi, sunDateParterm))
                    {
                        int year = Convert.ToInt32(chuoi) / 10000;
                        int month = Convert.ToInt32(chuoi) - year * 100;
                        kq = "1/" + month + "/" + year;
                    }
                }
                catch (System.Exception ex)
                {

                }

                return kq;
            }

        }
        class DEND : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                String chuoi = parameters[0].ToString();
                String kq = "";
                try
                {
                    string periodParterm = @"^[0-9]{7}$";
                    string sunDateParterm = @"^[0-9]{8}$";
                    if (parameters[0].GetType() == typeof(DateTime))
                    {
                        DateTime date = Convert.ToDateTime(parameters[0]);
                        kq = DateTime.DaysInMonth(date.Year, date.Month) + "/" + date.Month + "/" + date.Year;
                    }
                    else if (Regex.IsMatch(chuoi, periodParterm))
                    {
                        int year = Convert.ToInt32(chuoi) / 1000;
                        int month = Convert.ToInt32(chuoi) - year * 1000;
                        DateTime date = new DateTime(year, month, 1);
                        kq = DateTime.DaysInMonth(date.Year, date.Month) + "/" + date.Month + "/" + date.Year;
                    }
                    else if (Regex.IsMatch(chuoi, sunDateParterm))
                    {
                        int year = Convert.ToInt32(chuoi) / 10000;
                        int month = Convert.ToInt32(chuoi) - year * 100;
                        DateTime date = new DateTime(year, month, 1);
                        kq = DateTime.DaysInMonth(date.Year, date.Month) + "/" + date.Month + "/" + date.Year;
                    }
                }
                catch (System.Exception ex)
                {

                }

                return kq;
            }

        }

        class PH : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                String chuoi = parameters[0].ToString();
                String kq = "";
                try
                {
                    string periodParterm = @"^[0-9]{7}$";
                    if (Regex.IsMatch(chuoi, periodParterm))
                    {
                        int year = Convert.ToInt32(chuoi) / 1000;
                        int month = Convert.ToInt32(chuoi) - year * 1000;
                        month--;
                        if (month == 0)
                        {
                            year--;
                            month = 12;
                        }
                        kq = year.ToString() + month.ToString("000");
                    }
                }
                catch (System.Exception ex)
                {

                }

                return kq;
            }

        }
        class PA : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                String chuoi = parameters[0].ToString();
                String kq = "";
                try
                {
                    string periodParterm = @"^[0-9]{7}$";
                    if (Regex.IsMatch(chuoi, periodParterm))
                    {
                        int year = Convert.ToInt32(chuoi) / 1000;
                        int month = Convert.ToInt32(chuoi) - year * 1000;
                        //month--;
                        //if (month == 0)
                        //{
                        //    year--;
                        //    month = 12;
                        //}
                        kq = year.ToString() + month.ToString("000");
                    }
                }
                catch (System.Exception ex)
                {

                }

                return kq;
            }

        }
        class PE : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                String chuoi = parameters[0].ToString();
                String kq = "";
                try
                {
                    string periodParterm = @"^[0-9]{7}$";
                    if (Regex.IsMatch(chuoi, periodParterm))
                    {
                        int year = Convert.ToInt32(chuoi) / 1000;
                        int month = Convert.ToInt32(chuoi) - year * 1000;
                        month = 12;
                        kq = year.ToString() + month.ToString("000");
                    }
                }
                catch (System.Exception ex)
                {

                }

                return kq;
            }

        }

        class YA : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                String chuoi = parameters[0].ToString();
                String kq = "";
                try
                {
                    string periodParterm = @"^[0-9]{7}$";
                    if (Regex.IsMatch(chuoi, periodParterm))
                    {
                        int year = Convert.ToInt32(chuoi) / 1000;
                        int month = Convert.ToInt32(chuoi) - year * 1000;

                        year--;
                        month = 1;

                        kq = year.ToString() + month.ToString("000");
                    }
                }
                catch (System.Exception ex)
                {

                }

                return kq;
            }

        }
        class YE : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                String chuoi = parameters[0].ToString();
                String kq = "";
                try
                {
                    string periodParterm = @"^[0-9]{7}$";
                    if (Regex.IsMatch(chuoi, periodParterm))
                    {
                        int year = Convert.ToInt32(chuoi) / 1000;
                        int month = Convert.ToInt32(chuoi) - year * 1000;
                        year--;
                        month = 1;
                        kq = year.ToString() + month.ToString("000");
                    }
                }
                catch (System.Exception ex)
                {

                }

                return kq;
            }

        }
        class YH : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                String chuoi = parameters[0].ToString();
                String kq = "";
                try
                {
                    string periodParterm = @"^[0-9]{7}$";
                    if (Regex.IsMatch(chuoi, periodParterm))
                    {
                        int year = Convert.ToInt32(chuoi) / 1000;
                        int month = Convert.ToInt32(chuoi) - year * 1000;
                        month = 12;
                        year--;
                        kq = year.ToString() + month.ToString("000");
                    }
                }
                catch (System.Exception ex)
                {

                }

                return kq;
            }

        }
        class YK : TFlexCelUserFunction
        {
            public override object Evaluate(object[] parameters)
            {
                if (parameters == null || parameters.Length > 1)
                    throw new ArgumentException("Invalid number of params for user defined function \"MyUserFunction");
                String chuoi = parameters[0].ToString();
                String kq = "";
                try
                {
                    string periodParterm = @"^[0-9]{7}$";
                    if (Regex.IsMatch(chuoi, periodParterm))
                    {
                        int year = Convert.ToInt32(chuoi) / 1000;
                        int month = Convert.ToInt32(chuoi) - year * 1000;
                        month = 12;
                        kq = year.ToString() + month.ToString("000");
                    }
                }
                catch (System.Exception ex)
                {

                }

                return kq;
            }

        }
        #endregion

        #region UserTable
        private void flexcelreport_UserTable(object sender, UserTableEventArgs e)
        {
            DataTable dt = new DataTable(e.TableName);

            //On this example we will just return the table with the name specified on parameters
            //but you could return whatever you wanted here.
            //As always, remember to *validate* what the user can enter on the parameters string.
            switch (e.Parameters.ToUpper(CultureInfo.InvariantCulture))
            {
                case "SUPPLIERS":
                    //genericAdapter.SelectCommand = new System.Data.OleDb.OleDbCommand("select * from suppliers", oleDbConnection1);
                    break;
                case "CATEGORIES":
                    //genericAdapter.SelectCommand = new System.Data.OleDb.OleDbCommand("select * from categories", oleDbConnection1);
                    break;
                case "PRODUCTS":
                    //genericAdapter.SelectCommand = new System.Data.OleDb.OleDbCommand("select * from products", oleDbConnection1);
                    break;
                default:
                    if (e.Parameters.ToUpper(CultureInfo.InvariantCulture).Contains("\""))
                    {
                        string formular = e.Parameters.ToString(CultureInfo.InvariantCulture);
                        //foreach (QueryBuilder.Filter x in _sqlBuilder.Filters)
                        //{
                        //    formular = formular.Replace("<#PARAMETER." + x.Code.ToUpper() + "_FROM>", x.ValueFrom);
                        //    formular = formular.Replace("<#PARAMETER." + x.Code.ToUpper() + "_TO>", x.ValueTo);
                        //}
                        SQLBuilder sqlBuilder = new SQLBuilder(processingMode.Details);
                        Parsing.Formular2SQLBuilder(formular, ref sqlBuilder);

                        CoreCommonControl commo = new CoreCommonControl();
                        string[] arrF = formular.Split(';');

                        if (arrF.Length >= 2 && arrF[1] == "A" && ReportGen.SqlBuilder.Table == sqlBuilder.Table)
                            foreach (Filter x in ReportGen.SqlBuilder.Filters)
                                sqlBuilder.Filters.Add(x);
                        else if (arrF.Length >= 2 && arrF[1] == "S")
                            foreach (Filter x in ReportGen.SqlBuilder.Filters)
                                foreach (Filter y in sqlBuilder.Filters)
                                {
                                    if (x.Node.MyCode == y.Node.MyCode)
                                    {
                                        y.Operate = x.Operate;
                                        y.IsNot = x.IsNot;
                                        y.ValueFrom = y.FilterFrom = x.ValueFrom;
                                        y.ValueTo = y.ValueTo = x.ValueTo;
                                    }
                                }

                        sqlBuilder.StrConnectDes = ReportGen.__connectString;
                        dt = sqlBuilder.BuildDataTable("");
                    }
                    break;

                //default: throw new Exception("Invalid parameter to user table: " + e.Parameters);
            }

            //genericAdapter.Fill(dt);
            ((FlexCelReport)sender).AddTable(e.TableName, dt, TDisposeMode.DisposeAfterRun);
        }
        #endregion UserTable

        #region Method
        public void GenTemplate()
        {
            if (!File.Exists(_pathTemplate + _qdCode + ".template" + ReportGen.Ext))
            {
                XlsFile xlsTemp = new XlsFile(_pathTemplate + "-.template" + ReportGen.Ext);
                xlsTemp.SetCellValue(xlsTemp.GetSheetIndex("<#Config>"), 10, 2, _qdCode, 0);
                xlsTemp.SetCellValue(xlsTemp.GetSheetIndex("<#Config>"), 17, 2, "FilterPara", 0);

                xlsTemp.Save(_pathTemplate + _qdCode + ".template" + ReportGen.Ext, _format);
            }
        }
        public ReportGen(SQLBuilder sqlBuilder, string qdCode, string sqlText, string connectString, string pathTemplate, string pathReport)
        {
            _format = User2007 ? TFileFormats.Xlsx : TFileFormats.Xls;
            _sqlBuilder = sqlBuilder;
            _qdCode = qdCode;
            _queryText = sqlText;
            _database = _sqlBuilder.Database;
            __connectString = connectString;
            _pathReport = pathReport;
            _pathTemplate = pathTemplate;
        }
        public ReportGen(DataSet dtSet, string qdCode, string database, string connectString, string pathTemplate, string pathReport)
        {
            //_sqlBuilder = sqlBuilder;
            _format = User2007 ? TFileFormats.Xlsx : TFileFormats.Xls;
            _qdCode = qdCode;
            _database = database;
            _dtSet = dtSet;
            __connectString = connectString;
            _pathReport = pathReport;
            _pathTemplate = pathTemplate;
        }
        public DataSet GetDataSet()
        {
            CoreCommonControl commo = new CoreCommonControl();
            _sqlBuilder.StrConnectDes = __connectString;
            DataTable dt = _sqlBuilder.BuildDataTable(_queryText);
            //dt = _sqlBuilder.BuildDataTable(_queryText);
            DataTable dt_filter = CreateFilterTable(_sqlBuilder);
            if (dt.Rows.Count > 0)
            {
                //DataSet temp = new DataSet();
                dt.TableName = _qdCode;
                //temp.Tables.Add(dt);
                if (dt_filter.Rows.Count > 0)
                {
                    dt.DataSet.Tables.Add(dt_filter);
                    dt.DataSet.Tables[1].TableName = "FilterPara";
                }
            }
            _dtSet = dt.DataSet;
            return _dtSet;
        }
        public ExcelFile ExportExcel(string path)
        {
            _xlsFile = CreateReport();
            return _xlsFile;
        }
        public string ExportExcelToPath(string path)
        {
            String filename = "";
            filename = path + "\\" + _qdCode + ReportGen.Ext;
            _xlsFile = CreateReport();
            _xlsFile.Save(filename, _format);
            return filename;
        }
        public string ExportExcelToFile(string path, string filename)
        {
            //String filename = path + "\\" + _qdCode + ".xls";
            string file = path + filename;
            _xlsFile = CreateReport();
            _xlsFile.Save(file, _format);
            return file;
        }
        public ExcelFile CreateReport()
        {
            try
            {
                if (_xlsFile != null)
                    return _xlsFile;
                FlexCelReport flexcelreport = new FlexCelReport();
                GenTemplate();
                if (_dtSet == null)
                {
                    CoreCommonControl commo = new CoreCommonControl();
                    _sqlBuilder.StrConnectDes = __connectString;
                    DataTable dt = _sqlBuilder.BuildDataTable(_queryText);
                    //if (dt.Rows.Count == 0)
                    //    throw new Exception("No data");
                    //dt = _sqlBuilder.BuildDataTable(_queryText);
                    DataTable dt_filter = CreateFilterTable(_sqlBuilder);
                    DataTable dt_param = new DataTable();
                    DataColumn[] cols = new DataColumn[] { new DataColumn("Code")
                    , new DataColumn("ValueFrom")
                    , new DataColumn("ValueTo")
                    , new DataColumn("Description")
                    , new DataColumn("IsNot")
                    , new DataColumn("Operate")};
                    dt_param.Columns.AddRange(cols);
                    dt_param.TableName = "params";
                    foreach (Filter x in _sqlBuilder.Filters)
                    {
                        DataRow row = dt_param.NewRow();
                        row["Code"] = x.Code;
                        row["Description"] = x.Description;
                        row["ValueFrom"] = x.ValueFrom;
                        row["ValueTo"] = x.ValueTo;
                        row["IsNot"] = x.IsNot;
                        row["Operate"] = x.Operate;
                        dt_param.Rows.Add(row);
                    }
                    //if (dt.Rows.Count > 0)
                    //{
                    //DataSet temp = new DataSet();
                    dt.TableName = _qdCode;
                    //temp.Tables.Add(dt);

                    dt.DataSet.Tables.Add(dt_filter);
                    dt.DataSet.Tables[1].TableName = _FilterPara;

                    //}
                    //if (dt_param.Rows.Count > 0)
                    //{
                    dt.DataSet.Tables.Add(dt_param);
                    //}
                    _dtSet = dt.DataSet;
                }
                flexcelreport.UserTable += new UserTableEventHandler(flexcelreport_UserTable);
                flexcelreport.AddTable(_dtSet);
                AddReportVariable(flexcelreport);
                ExcelFile rs = Run_templatereport(flexcelreport);
                rs = AddData(rs);
                _xlsFile = rs;
                return rs;
            }
            catch (Exception ex) { throw ex; }
        }

        private void AddReportVariable(FlexCelReport flexcelreport)
        {
            flexcelreport.SetValue("Date", DateTime.Now.ToShortDateString());
            flexcelreport.SetValue("QDName", _name);
            flexcelreport.SetValue("QDCode", _qdCode);
            flexcelreport.SetValue("DB", _database);
        }
        public MemoryStream ExportPDF(string path)
        {
            MemoryStream ms = new MemoryStream();
            //if (_xlsFile == null)
            _xlsFile = CreateReport();

            try
            {
                using (FlexCelPdfExport pdf = new FlexCelPdfExport())
                {
                    pdf.Workbook = _xlsFile;

                    pdf.BeginExport(ms);
                    pdf.ExportAllVisibleSheets(false, "test");
                    pdf.EndExport();
                }
                return ms;
            }
            catch
            {
                return null;
            }

        }
        public MemoryStream ExportPDF(ExcelFile xlsFile)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                using (FlexCelPdfExport pdf = new FlexCelPdfExport())
                {
                    pdf.Workbook = xlsFile;

                    pdf.BeginExport(ms);
                    pdf.ExportAllVisibleSheets(false, "test");
                    pdf.EndExport();
                }
                return ms;
            }
            catch
            {
                return null;
            }

        }
        public string ExportPDFToPath(string path)
        {
            String filename = path + "\\" + _qdCode + ".pdf";
            //FileStream file = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //if (_xlsFile == null)
            _xlsFile = CreateReport();

            try
            {
                using (FlexCelPdfExport pdf = new FlexCelPdfExport())
                {
                    pdf.Workbook = _xlsFile;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        pdf.BeginExport(ms);
                        pdf.ExportAllVisibleSheets(false, "test");
                        pdf.EndExport();

                        pdf.Export(filename);
                    }
                }
                return filename;
            }
            catch
            {
                return "";
            }

        }
        public string ExportHTMLToPath(string path)
        {
            String filename = path + _qdCode + ".htm";
            //TextWriter file = new StringWriter(); ;
            if (_xlsFile == null)
                _xlsFile = CreateReport();
            try
            {
                using (FlexCelHtmlExport html = new FlexCelHtmlExport())
                {
                    html.Workbook = _xlsFile;
                    //html.HtmlFileFormat = THtmlFileFormat.MHtml;
                    html.AllowOverwritingFiles = true;
                    html.SavedImagesFormat = THtmlImageFormat.Png;
                    //html.HtmlVersion = THtmlVersion.XHTML_10;
                    //if (File.Exists(filename))
                    //    File.Delete(filename);
                    //string pathx = Path.GetDirectoryName(filename);
                    //string name = Path.GetFileNameWithoutExtension(filename);
                    //string ext = ".png";
                    //string fileimage = pathx + "\\" + name + "_image1" + ext;
                    //if (File.Exists(fileimage))
                    //    File.Delete(fileimage);
                    //fileimage = pathx + "\\" + name + "_image2" + ext;
                    //if (File.Exists(fileimage))
                    //    File.Delete(fileimage);

                    html.Export(filename, "images", "css\\" + _qdCode + ".css");

                }
                return filename;
            }
            catch
            {
                return "";
            }
        }
        public string ExportHTMLToFile(string path, string filename)
        {
            string filehtml = path + filename;
            //if (_xlsFile == null)
            _xlsFile = CreateReport();
            try
            {
                using (FlexCelHtmlExport html = new FlexCelHtmlExport())
                {
                    html.Workbook = _xlsFile;
                    html.Workbook = _xlsFile;
                    //html.HtmlFileFormat = THtmlFileFormat.MHtml;
                    html.AllowOverwritingFiles = true;
                    html.SavedImagesFormat = THtmlImageFormat.Png;
                    //html.HtmlVersion = THtmlVersion.XHTML_10;
                    html.Export(filehtml, "images", "css\\" + _qdCode + ".css");
                }
                return filehtml;
            }
            catch
            {
                return "";
            }
        }
        public TextWriter ExportHTML(string path)
        {
            String filename = path + _qdCode + ".html";
            TextWriter file = new StringWriter(); ;
            //if (_xlsFile == null)
            _xlsFile = CreateReport();
            try
            {
                using (FlexCelHtmlExport html = new FlexCelHtmlExport())
                {
                    html.Workbook = _xlsFile;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        html.Workbook = _xlsFile;
                        html.Workbook = _xlsFile;
                        //html.HtmlFileFormat = THtmlFileFormat.MHtml;
                        html.AllowOverwritingFiles = true;
                        html.SavedImagesFormat = THtmlImageFormat.Png;
                        //html.HtmlVersion = THtmlVersion.XHTML_10;                      

                        html.Export(file, filename, null);

                    }
                }
                return file;
            }
            catch
            {
                return null;
            }

        }
        public TextWriter ExportHTML(string path, ExcelFile xlsFile)
        {
            //String filename = path + _pathTemplate + "\\" + _database + "\\" + _qdCode + ".html";
            TextWriter file = new StringWriter(); ;
            try
            {
                using (FlexCelHtmlExport html = new FlexCelHtmlExport())
                {
                    html.Workbook = xlsFile;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        //if (File.Exists(filename))
                        //    File.Delete(filename);
                        html.Export(file, "", null);
                        //file.ToString();
                    }
                }
                return file;
            }
            catch
            {
                return null;
            }

        }
        /*public string ExportSWF(string path, string qdID)
        {
            String filename = path + _pathTemplate + "\\" + _database + "\\" + _qdCode + ".swf";
            //String filePDF = path + _pathTemplate + "\\" + _sqlBuilder.Database + "\\" + _qdCode + ".pdf";
            try
            {
                string filePDF = ExportPDFToFile(path, qdID);
                System.Diagnostics.Process r = new System.Diagnostics.Process();
                r.StartInfo.UseShellExecute = false;
                r.StartInfo.RedirectStandardOutput = true;
                r.StartInfo.CreateNoWindow = true;
                r.StartInfo.RedirectStandardError = true;
                r.StartInfo.WorkingDirectory = HttpContext.Current.Server.MapPath("~/");
                r.StartInfo.FileName = HttpContext.Current.Server.MapPath("~/PDF2SWF/PDF2SWF.exe");
                r.StartInfo.Arguments = filePDF + " -o " + filename + " -T 9 -f ";
                r.Start();
                r.WaitForExit();
                r.Close();
                //file.Close();
                return filename;
            }
            catch
            {

                return "";
            }
        }*/
        #endregion Method

        #region ReadNumber

        class ReadVN
        {
            private string[] strSo = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            private string[] strDonViNho = { "linh", "lăm", "mười", "mươi", "mốt", "trăm" };
            private string[] strDonViLon = { "", "ngàn", "triệu", "tỷ" };
            private string[] strMainGroup;
            private string[] strSubGroup;
            private string Len1(string strA)
            {
                return strSo[int.Parse(strA)];
            }
            private string Len2(string strA)
            {
                if (strA.Substring(0, 1) == "0")
                {
                    return strDonViNho[0] + " " + Len1(strA.Substring(1, 1));
                }
                else if (strA.Substring(0, 1) == "1")
                {
                    if (strA.Substring(1, 1) == "5")
                    {
                        return strDonViNho[2] + " " + strDonViNho[1];
                    }
                    else if (strA.Substring(1, 1) == "0")
                    {
                        return strDonViNho[2];
                    }
                    else
                    {
                        return strDonViNho[2] + " " + Len1(strA.Substring(1, 1));
                    }
                }
                else
                {
                    if (strA.Substring(1, 1) == "5")
                    {
                        return Len1(strA.Substring(0, 1)) + " " + strDonViNho[3] + " " + strDonViNho[1];
                    }
                    else if (strA.Substring(1, 1) == "0")
                    {
                        return Len1(strA.Substring(0, 1)) + " " + strDonViNho[3];
                    }
                    else if (strA.Substring(1, 1) == "1")
                    {
                        return Len1(strA.Substring(0, 1)) + " " + strDonViNho[3] + " " + strDonViNho[4];
                    }
                    else
                    {
                        return Len1(strA.Substring(0, 1)) + " " + strDonViNho[3] + " " + Len1(strA.Substring(1, 1));
                    }
                }
            }
            private string Len3(string strA)
            {
                if ((strA.Substring(0, 3) == "000"))
                {
                    return null;
                }
                else if ((strA.Substring(1, 2) == "00"))
                {
                    return Len1(strA.Substring(0, 1)) + " " + strDonViNho[5];
                }
                else
                {
                    return Len1(strA.Substring(0, 1)) + " " + strDonViNho[5] + " " + Len2(strA.Substring(1, strA.Length - 1));
                }
            }
            /////////////////////
            private string FullLen(string strSend)
            {
                bool boKTNull = false;
                string strKQ = "";
                string strA = strSend.Trim();
                int iIndex = strA.Length - 9;
                int iPreIndex = 0;

                if (strSend.Trim() == "")
                {
                    return Len1("0");
                }
                //tra ve khong neu la khong
                for (int i = 0; i < strA.Length; i++)
                {
                    if (strA.Substring(i, 1) != "0")
                    {
                        break;
                    }
                    else if (i == strA.Length - 1)
                    {
                        return strSo[0];
                    }
                }
                int k = 0;
                while (strSend.Trim().Substring(k++, 1) == "0")
                {
                    strA = strA.Remove(0, 1);
                }
                //
                if (strA.Length < 9)
                {
                    iPreIndex = strA.Length;
                }
                //
                if ((strA.Length % 9) != 0)
                {
                    strMainGroup = new string[strA.Length / 9 + 1];
                }
                else
                {
                    strMainGroup = new string[strA.Length / 9];
                }
                //nguoc
                for (int i = strMainGroup.Length - 1; i >= 0; i--)
                {
                    if (iIndex >= 0)
                    {
                        iPreIndex = iIndex;
                        strMainGroup[i] = strA.Substring(iIndex, 9);
                        iIndex -= 9;
                    }
                    else
                    {
                        strMainGroup[i] = strA.Substring(0, iPreIndex);
                    }

                }
                /////////////////////////////////
                //tach moi maingroup thanh nhieu subgroup
                //xuoi
                for (int j = 0; j < strMainGroup.Length; j++)
                {
                    //gan lai gia tri
                    iIndex = strMainGroup[j].Length - 3;
                    if (strMainGroup[j].Length < 3)
                    {
                        iPreIndex = strMainGroup[j].Length;
                    }
                    ///
                    if ((strMainGroup[j].Length % 3) != 0)
                    {
                        strSubGroup = new string[strMainGroup[j].Length / 3 + 1];
                    }
                    else
                    {
                        strSubGroup = new string[strMainGroup[j].Length / 3];
                    }
                    for (int i = strSubGroup.Length - 1; i >= 0; i--)
                    {
                        if (iIndex >= 0)
                        {
                            iPreIndex = iIndex;
                            strSubGroup[i] = strMainGroup[j].Substring(iIndex, 3);
                            iIndex -= 3;
                        }
                        else
                        {
                            strSubGroup[i] = strMainGroup[j].Substring(0, iPreIndex);
                        }
                    }
                    //duyet subgroup de lay string
                    for (int i = 0; i < strSubGroup.Length; i++)
                    {
                        boKTNull = false;//phai de o day
                        if ((j == strMainGroup.Length - 1) && (i == strSubGroup.Length - 1))
                        {
                            if (strSubGroup[i].Length < 3)
                            {
                                if (strSubGroup[i].Length == 1)
                                {
                                    strKQ += Len1(strSubGroup[i]);
                                }
                                else
                                {
                                    strKQ += Len2(strSubGroup[i]);
                                }
                            }
                            else
                            {
                                strKQ += Len3(strSubGroup[i]);
                            }
                        }
                        else
                        {
                            if (strSubGroup[i].Length < 3)
                            {
                                if (strSubGroup[i].Length == 1)
                                {
                                    strKQ += Len1(strSubGroup[i]) + " ";
                                }
                                else
                                {
                                    strKQ += Len2(strSubGroup[i]) + " ";
                                }
                            }
                            else
                            {
                                if (Len3(strSubGroup[i]) == null)
                                {
                                    boKTNull = true;
                                }
                                else
                                {
                                    strKQ += Len3(strSubGroup[i]) + " ";
                                }
                            }
                        }
                        //dung
                        if (!boKTNull)
                        {
                            if (strSubGroup.Length - 1 - i != 0)
                            {
                                strKQ += strDonViLon[strSubGroup.Length - 1 - i] + " ";
                            }
                            else
                            {
                                strKQ += strDonViLon[strSubGroup.Length - 1 - i] + " ";
                            }

                        }
                    }
                    //dung
                    if (j != strMainGroup.Length - 1)
                    {
                        if (!boKTNull)
                        {
                            strKQ = strKQ.Substring(0, strKQ.Length - 1) + strDonViLon[3] + " ";
                        }
                        else
                        {
                            strKQ = strKQ.Substring(0, strKQ.Length - 1) + " " + strDonViLon[3] + " ";
                        }
                    }
                }
                //xoa ky tu trang
                strKQ = strKQ.Trim();
                //xoa dau , neu co
                if (strKQ.Substring(strKQ.Length - 1, 1) == ".")
                {
                    strKQ = strKQ.Remove(strKQ.Length - 1, 1);
                }
                return strKQ;

                ////////////////////////////////////


            }
            public string Convert(string strSend, char charInSeparator, string strOutSeparator)
            {
                if (strOutSeparator == "")
                {
                    return "Lỗi dấu phân cách đầu ra rỗng";
                }
                if (strSend == "")
                {
                    return Len1("0");
                }

                string[] strTmp = new string[2];
                try
                {

                    strTmp = strSend.Split(charInSeparator);
                    string strTmpRight = strTmp[1];
                    for (int i = strTmpRight.Length - 1; i >= 0; i--)
                    {
                        if (strTmpRight.Substring(i, 1) == "0")
                        {
                            strTmpRight = strTmpRight.Remove(i, 1);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (strTmpRight != "")
                    {
                        string strRight = "";
                        for (int i = 0; i < strTmpRight.Length; i++)
                        {
                            strRight += Len1(strTmpRight.Substring(i, 1)) + " ";
                        }


                        return FullLen(strTmp[0]) + " " + strOutSeparator + " " + strRight.TrimEnd();
                    }
                    else
                    {
                        return FullLen(strTmp[0]);
                    }
                }
                catch
                {
                    return FullLen(strTmp[0]);
                }

            }

        }
        class ReadEN
        {
            // Single-digit and small number names
            private string[] _smallNumbers = new string[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };

            // Tens number names from twenty upwards
            private string[] _tens = new string[] { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            // Scale number names for use during recombination
            private string[] _scaleNumbers = new string[] { "", "Thousand", "Million", "Billion" };



            // Converts an integer value into English words
            public string NumberToWords(Double number)
            {
                // Zero rule
                if (number == 0)
                    return _smallNumbers[0];

                // Array to hold four three-digit groups
                int[] digitGroups = new int[4];

                // Ensure a positive number to extract from
                int positive = Math.Abs(Convert.ToInt32(number));

                // Extract the three-digit groups
                for (int i = 0; i < 4; i++)
                {
                    digitGroups[i] = positive % 1000;
                    positive /= 1000;
                }

                // Convert each three-digit group to words
                string[] groupText = new string[4];

                for (int i = 0; i < 4; i++)
                    groupText[i] = ThreeDigitGroupToWords(digitGroups[i]);

                // Recombine the three-digit groups
                string combined = groupText[0];
                bool appendAnd;

                // Determine whether an 'and' is needed
                appendAnd = (digitGroups[0] > 0) && (digitGroups[0] < 100);

                // Process the remaining groups in turn, smallest to largest
                for (int i = 1; i < 4; i++)
                {
                    // Only add non-zero items
                    if (digitGroups[i] != 0)
                    {
                        // Build the string to add as a prefix
                        string prefix = groupText[i] + " " + _scaleNumbers[i];

                        if (combined.Length != 0)
                            prefix += appendAnd ? " and " : ", ";

                        // Opportunity to add 'and' is ended
                        appendAnd = false;

                        // Add the three-digit group to the combined string
                        combined = prefix + combined;
                    }
                }

                // Negative rule
                if (number < 0)
                    combined = "Negative " + combined;

                return combined;
            }



            // Converts a three-digit group into English words
            private string ThreeDigitGroupToWords(int threeDigits)
            {
                // Initialise the return text
                string groupText = "";

                // Determine the hundreds and the remainder
                int hundreds = threeDigits / 100;
                int tensUnits = threeDigits % 100;

                // Hundreds rules
                if (hundreds != 0)
                {
                    groupText += _smallNumbers[hundreds] + " Hundred";

                    if (tensUnits != 0)
                        groupText += " and ";
                }

                // Determine the tens and units
                int tens = tensUnits / 10;
                int units = tensUnits % 10;

                // Tens rules
                if (tens >= 2)
                {
                    groupText += _tens[tens];
                    if (units != 0)
                        groupText += " " + _smallNumbers[units];
                }
                else if (tensUnits != 0)
                    groupText += _smallNumbers[tensUnits];

                return groupText;
            }
        }
        #endregion

        public static class clsListValueTT_XLB_EB
        {
            static Dictionary<TPoint, object> _values = new Dictionary<TPoint, object>();

            public static Dictionary<TPoint, object> Values
            {
                get { return _values; }
                set { _values = value; }
            }
        }

        public void Close()
        {
            _xlsFile = null;
        }
        public bool IsClose()
        {
            if (_xlsFile == null)
                return true;
            return false;
        }
    }
}
