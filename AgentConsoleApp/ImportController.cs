using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Text;
using Dapper;
using System.Data.SqlClient;

namespace AgentConsoleApp
{
    class ImportController
    {
        public static TextFieldParser CreateParser(string value, params string[] delims)
        {
            var parser = new TextFieldParser(ToStream(value));
            parser.Delimiters = delims;
            return parser;
        }

        static Stream ToStream(string value)
        {
            return new MemoryStream(Encoding.Default.GetBytes(value));
        }

        public static void Dump(TextFieldParser parser)
        {
            while (!parser.EndOfData)
            {
                foreach (var field in parser.ReadFields())
                {
                    Console.WriteLine(field);
                }
            }
        }

        public static string DumpHD(TextFieldParser parser, string connectionString, string FL_Filecode, string FL_TotalRecord)
        {
            //int affectedRows = 0;
            string HD_PoNo = "";
            string[] fields;

            while (!parser.EndOfData)
            {
                fields = parser.ReadFields();
                HD_PoNo = fields[1];

                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    string sql = "INSERT INTO [EDI_POHEADER LEVEL] (" +
                        "[FILE_CODE]" +
                        ",[TOTAL_RECORDS]" +
                        ",[PO_NUMBER]" +
                        ",[PO_TYPE]" +
                        ",[CONTRACT_NUMBER]" +
                        ",[ORDERED_DATE]" +
                        ",[DELIVERY_DATE]" +
                        ",[HOSP_CODE]" +
                        ",[HOSP_NAME]" +
                        ",[BUYER_NAME]" +
                        ",[BUYER_DEPARTMENT]" +
                        ",[EMAIL]" +
                        ",[SUPPLIER_CODE]" +
                        ",[SHIP_TO_CODE]" +
                        ",[BILL_TO_CODE]" +
                        ",[Approval Code]" +
                        ",[Budget Code]" +
                        ",[CURRENCY_CODE]" +
                        ",[PAYMENT_TERM]" +
                        ",[DISCOUNT_PCT]" +
                        ",[TOTAL_AMOUNT]" +
                        ",[NOTE_TO_SUPPLIER]" +
                        ",[RESEND_FLAG]" +
                        ",[CREATION_DATE]" +
                        ",[QUATATION_ID]" +
                        ",[CUSTOMER_ID]" +
                        ",[LAST_INTERFACED_ DATE]" +
                        ",[INTERFACE_ID]" +
                        ",[imp_Date]" +
                        ") " +
                        "VALUES (" +
                        "@column01, " +
                        "@column02," +
                        "@column03," +
                        "@column04," +
                        "@column05," +
                        "@column06," +
                        "@column07," +
                        "@column08," +
                        "@column09," +
                        "@column10," +
                        "@column11," +
                        "@column12," +
                        "@column13," +
                        "@column14," +
                        "@column15," +
                        "@column16," +
                        "@column17," +
                        "@column18," +
                        "@column19," +
                        "@column20," +
                        "@column21," +
                        "@column22," +
                        "@column23," +
                        "@column24," +
                        "@column25," +
                        "@column26," +
                        "@column27," +
                        "@column28," +
                        "GETDATE()" +
                        ")";
                    //affectedRows = sqlConnection.Execute(sql, new { 
                    sqlConnection.Execute(sql, new { 
                        column01 = FL_Filecode,
                        column02 = FL_TotalRecord,
                        column03 = fields[1],
                        column04 = fields[2],
                        column05 = fields[3],
                        column06 = fields[4],
                        column07 = fields[5],
                        column08 = fields[6],
                        column09 = fields[7],
                        column10 = fields[8],
                        column11 = fields[9],
                        column12 = fields[10],
                        column13 = fields[11],
                        column14 = fields[12],
                        column15 = fields[13],
                        column16 = fields[14],
                        column17 = fields[15],
                        column18 = fields[16],
                        column19 = fields[17],
                        column20 = fields[18],
                        column21 = fields[19],
                        column22 = fields[20],
                        column23 = fields[21],
                        column24 = fields[22],
                        column25 = fields[23],
                        column26 = fields[24],
                        column27 = fields[25],
                        column28 = fields[26]
                    });
                }
            }
            return HD_PoNo;
        }

        public static int DumpLN(TextFieldParser parser, string connectionString, string HD_PoNo)
        {
            int affectedRows = 0;
            string[] fields;

            while (!parser.EndOfData)
            {
                fields = parser.ReadFields();

                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    string sql = "INSERT INTO [EDI_POLINE LEVEL] (" +
                        "[PO_NUMBER]" +
                        ",[LINE_NUMBER]" +
                        ",[HOSPITEM_CODE]" +
                        ",[HOSPITEM_ NAME]" +
                        ",[DISTITEM_CODE]" +
                        ",[PACK_SIZE_DESC]" +
                        ",[ORDERED_QTY]" +
                        ",[UOM]" +
                        ",[PRICE_PER_UNIT]" +
                        ",[LINE_AMOUNT]" +
                        ",[DISCOUNT_LINE_ITEM]" +
                        ",[URGENT_FLAG]" +
                        ",[COMMENT]" +
                        ") " +
                        "VALUES (" +
                        "@column01, " +
                        "@column02," +
                        "@column03," +
                        "@column04," +
                        "@column05," +
                        "@column06," +
                        "@column07," +
                        "@column08," +
                        "@column09," +
                        "@column10," +
                        "@column11," +
                        "@column12," +
                        "@column13" +
                        ")";
                    affectedRows = sqlConnection.Execute(sql, new
                    {
                        column01 = HD_PoNo,
                        column02 = fields[1],
                        column03 = fields[2],
                        column04 = fields[3],
                        column05 = fields[4],
                        column06 = fields[5],
                        column07 = fields[6],
                        column08 = fields[7],
                        column09 = fields[8],
                        column10 = fields[9],
                        column11 = fields[10],
                        column12 = fields[11],
                        column13 = fields[12]
                    });
                }
            }
            return affectedRows;
        }

        public static string ExtractFirstColumn(TextFieldParser parser)
        {
            string[] fields;
            string firstColumn = "";
            while (!parser.EndOfData)
            {
                fields = parser.ReadFields();
                firstColumn = fields[0];
            }

            return firstColumn;
        }

        public static int CountLinesReader(string FilePath)
        {
            int lineCounter = 0;
            using (var reader = new StreamReader(FilePath))
            {
                while (reader.ReadLine() != null)
                {
                    lineCounter++;
                }
            }
            return lineCounter;
        }
    }
}
