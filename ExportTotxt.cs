using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Data.OleDb;
using System.Data.Sql;
using System.Configuration;


public class ExportTotxt
{
    public static string connStr = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

    public static DataTable QueryData(string sql)
    {
        try
        {
            SqlConnection oleCon = new SqlConnection(connStr);
            using (SqlDataAdapter oleDa = new SqlDataAdapter(sql, oleCon))
            {
                using (DataSet ds = new DataSet())
                {
                    oleDa.Fill(ds);
                    return ds.Tables[0];
                }

            }
        }
        catch (Exception)
        {

        }
        return null;

    }

    public static string ExceSql(string sql)
    {
        string result = "";
        try
        {
            DataTable d1 = QueryData(sql);
            if (d1 == null || d1.Rows.Count < 1)
            {
                return "";
            }
            result = d1.Rows[0][0].ToString();

            return result;
        }
        catch (Exception)
        {
            return result;
        }
    }

    public static int ExceSql_int(string sql)
    {
        try
        {
            int num = 0;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand comm = new SqlCommand(sql, conn))
                {
                    num = int.Parse(comm.ExecuteNonQuery().ToString());
                }
                conn.Close();
            }
            return num;
        }
        catch (Exception)
        {
            return -1;
        }
    }

}


