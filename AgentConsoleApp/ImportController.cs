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

        public static int DumpHD(TextFieldParser parser, string connectionString)
        {
            int affectedRows = 0;
            string[] fields;


            while (!parser.EndOfData)
            {
                fields = parser.ReadFields();

                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    var sql = "INSERT INTO header (" +
                        "column01, " +
                        "column02, " +
                        "column03, " +
                        "column04, " +
                        "column05, " +
                        "column06, " +
                        "column07, " +
                        "column08, " +
                        "column09, " +
                        "column10, " +
                        "column11, " +
                        "column12, " +
                        "column13, " +
                        "column14, " +
                        "column15, " +
                        "column16, " +
                        "column17," +
                        "column18, " +
                        "column19, " +
                        "column20, " +
                        "column21, " +
                        "column22, " +
                        "column23, " +
                        "column24, " +
                        "column25, " +
                        "column26, " +
                        "column27" +
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
                        "@column27" +
                        ")";
                    affectedRows = sqlConnection.Execute(sql, new { 
                        column01 = fields[0],
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
                        column13 = fields[12],
                        column14 = fields[13],
                        column15 = fields[14],
                        column16 = fields[15],
                        column17 = fields[16],
                        column18 = fields[17],
                        column19 = fields[18],
                        column20 = fields[19],
                        column21 = fields[20],
                        column22 = fields[21],
                        column23 = fields[22],
                        column24 = fields[23],
                        column25 = fields[24],
                        column26 = fields[25],
                        column27 = fields[26]
                    });
                }
            }
            return affectedRows;
        }

        public static int DumpLN(TextFieldParser parser, string connectionString)
        {
            int affectedRows = 0;
            string[] fields;


            while (!parser.EndOfData)
            {
                fields = parser.ReadFields();

                using (var sqlConnection = new SqlConnection(connectionString))
                {
                    var sql = "INSERT INTO detail (" +
                        "column01, " +
                        "column02, " +
                        "column03, " +
                        "column04, " +
                        "column05, " +
                        "column06, " +
                        "column07, " +
                        "column08, " +
                        "column09, " +
                        "column10, " +
                        "column11, " +
                        "column12, " +
                        "column13" +
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
                        column01 = fields[0],
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
    }
}
