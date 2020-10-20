using ConsoleTables;
using Figgle;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using Konsole;
using Console = Colorful.Console;
using static AgentConsoleApp.ImportController;
using System.Linq;

namespace AgentConsoleApp
{
    class Program
    {
        public class returnModel
        {
            public string FileName { get; set; }
            public int HeaderNo { get; set; }
            public int DetailNo { get; set; }
        }

        static void Main(string[] args)
        {
            var conn = ConfigurationManager.AppSettings["DBConnectionString"].ToString();
            string sourceDirectory;
            string line;
            int detailLineNo, headerLineNo;
            List<returnModel> returnCollection = new List<returnModel>();
            string FL_Filecode;
            string FL_TotalRecord;
            string HD_PoNo;
            string fileName = "";
            int counterFile = 1;
            int counterFileValidate = 1;
            int counterLine;

            #region Fancy header
            /*
            Console.Write(FiggleFonts.Ogre.Render("------------"));
            List<char> chars = new List<char>()
            {
                ' ', 'C', 'r', 'e', 'a', 't', 'e', 'd', ' ',
                'b', 'y', ' ',
                'P', 'i', 'r', 'i', 'y', 'a', 'V', ' '
            };
            Console.Write("---------------", Color.LawnGreen);
            Console.WriteWithGradient(chars, Color.Blue, Color.Purple, 16);
            Console.Write("---------------", Color.LawnGreen);
            Console.WriteLine("\n");
            */
            #endregion

            // Ask the user to type path
            if (args.Length == 0)
            {
                // Display title
                Console.Title = "TxtToDB 1.0.5";

                // Display header
                Console.WriteWithGradient(FiggleFonts.Banner.Render("txt to db"), Color.LightGreen, Color.ForestGreen, 16);
                Console.ReplaceAllColorsWithDefaults();

                // Display copyright
                Console.WriteLine(" --------------- Created by PiriyaV ----------------\n", Color.LawnGreen);

                Console.Write(@"Enter source path (eg: D:\folder) : ", Color.LightYellow);
                sourceDirectory = Convert.ToString(Console.ReadLine());
                Console.Write("\n");
            } else
            {
                sourceDirectory = Convert.ToString(args[0]);
            }

            // Variable for backup
            string folderBackup = "imported_" + DateTime.Now.ToString("ddMMyyyy_HHmmss");
            string folderBackupPath = Path.Combine(sourceDirectory, folderBackup);

            try
            {
                // Full path for txt
                var FilePath = Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.ToLower().EndsWith(".txt"));

                // Count txt file
                DirectoryInfo di = new DirectoryInfo(sourceDirectory);
                int FileNum = di.GetFiles("*.txt").Length;

                // Throw no txt file
                if (FileNum == 0)
                {
                    throw new ArgumentException("Text file not found in folder.");
                }

                #region Validate Section
                var pbValidate = new ProgressBar(PbStyle.DoubleLine, FileNum);

                foreach (string currentFile in FilePath)
                {
                    // Update progress bar (Overall)
                    fileName = Path.GetFileName(currentFile);
                    pbValidate.Refresh(counterFileValidate, "Validating, Please wait...");
                    Thread.Sleep(50);

                    int Lineno = 1;
                    string ErrorMsg = "";
                    Boolean IsHeaderValid = true;
                    Boolean IsColumnValid = true;

                    using (StreamReader file = new StreamReader(currentFile))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            var parser = CreateParser(line, ",");

                            string firstColumn = "";
                            int ColumnNo = 0;
                            string[] cells;
                            while (!parser.EndOfData)
                            {
                                cells = parser.ReadFields();
                                firstColumn = cells[0];
                                ColumnNo = cells.Length;

                                #region header
                                if (Lineno == 1 && firstColumn != "FL")
                                {
                                    IsHeaderValid = false;
                                    ErrorMsg = "First line must contain FL column";
                                }
                                else if (Lineno == 2 && firstColumn != "HD")
                                {
                                    IsHeaderValid = false;
                                    ErrorMsg = "Second line must contain HD column";
                                }
                                else if (Lineno >= 3 && firstColumn != "LN")
                                {
                                    IsHeaderValid = false;
                                    ErrorMsg = $"Data must contain LN column (At line: { Lineno })";
                                }

                                if (!IsHeaderValid)
                                {
                                    pbValidate.Refresh(counterFileValidate, "Validate failed.");
                                    throw new ArgumentException(ErrorMsg);
                                }
                                #endregion

                                #region column length
                                switch (firstColumn)
                                {
                                    case "FL":
                                        if (ColumnNo != 3)
                                        {
                                            IsColumnValid = false;
                                            ErrorMsg = $"FL must have 3 columns ({ ColumnNo } columns found)";
                                        }
                                        break;
                                    case "HD":
                                        if (ColumnNo != 27)
                                        {
                                            IsColumnValid = false;
                                            ErrorMsg = $"HD must have 27 columns ({ ColumnNo } columns found)";
                                        }
                                        break;
                                    case "LN":
                                        if (ColumnNo != 13)
                                        {
                                            IsColumnValid = false;
                                            ErrorMsg = $"LN must have 13 columns (At line: { Lineno }, { ColumnNo } columns found)";
                                        }
                                        break;
                                    default:
                                        IsColumnValid = false;
                                        ErrorMsg = $"Incorrect format, File must contain 'FL, HD or LN' in the first column on each row! (At Line: { Lineno})";
                                        break;
                                        //continue;
                                }

                                if (!IsColumnValid)
                                {
                                    pbValidate.Refresh(counterFileValidate, "Validate failed.");
                                    throw new ArgumentException(ErrorMsg);
                                }
                                #endregion

                                #region data type
                                switch (firstColumn)
                                {
                                    #region data type FL
                                    case "FL":
                                        ValidateLengthRange(cells[1], 1, 40, pbValidate, counterFileValidate, Lineno, "FILE_CODE", "2");
                                        ValidateInteger(cells[2], pbValidate, counterFileValidate, Lineno, "TOTAL_RECORDS", "3");
                                        break;
                                    #endregion
                                    #region data type HD
                                    case "HD":
                                        ValidateLengthRange(cells[1], 1, 40, pbValidate, counterFileValidate, Lineno, "PO_NUMBER", "2");
                                        ValidateBit(cells[2], pbValidate, counterFile, Lineno, "PO_TYPE", "3");
                                        ValidateLengthRange(cells[3], 0, 40, pbValidate, counterFileValidate, Lineno, "CONTRACT_NUMBER", "4");
                                        ValidateDateTime(cells[4], pbValidate, counterFile, Lineno, "ORDERED_DATE", "5");
                                        ValidateDateTime(cells[5], pbValidate, counterFile, Lineno, "DELIVERY_DATE", "6");
                                        ValidateLengthRange(cells[6], 1, 40, pbValidate, counterFileValidate, Lineno, "HOSP_CODE", "7");
                                        ValidateLengthRange(cells[7], 1, 80, pbValidate, counterFileValidate, Lineno, "HOSP_NAME", "8");
                                        ValidateLengthRange(cells[8], 0, 100, pbValidate, counterFileValidate, Lineno, "BUYER_NAME", "9");
                                        ValidateLengthRange(cells[9], 0, 100, pbValidate, counterFileValidate, Lineno, "BUYER_DEPARTMENT", "10");
                                        ValidateLengthRange(cells[10], 1, 40, pbValidate, counterFileValidate, Lineno, "EMAIL", "11");
                                        ValidateLengthRange(cells[11], 1, 40, pbValidate, counterFileValidate, Lineno, "SUPPLIER_CODE", "12");
                                        ValidateLengthRange(cells[12], 1, 40, pbValidate, counterFileValidate, Lineno, "SHIP_TO_CODE", "13");
                                        ValidateLengthRange(cells[13], 1, 40, pbValidate, counterFileValidate, Lineno, "BILL_TO_CODE", "14");
                                        ValidateLengthRange(cells[14], 1, 20, pbValidate, counterFileValidate, Lineno, "Approval Code", "15");
                                        ValidateLengthRange(cells[15], 1, 20, pbValidate, counterFileValidate, Lineno, "Budget Code", "16");
                                        ValidateLengthRange(cells[16], 1, 420, pbValidate, counterFileValidate, Lineno, "CURRENCY_CODE", "17");
                                        ValidateLengthRange(cells[17], 1, 80, pbValidate, counterFileValidate, Lineno, "PAYMENT_TERM", "18");
                                        ValidateFloat(cells[18], pbValidate, counterFile, Lineno, "DISCOUNT_PCT", "19");
                                        ValidateFloat(cells[19], pbValidate, counterFile, Lineno, "TOTAL_AMOUNT", "20");
                                        ValidateLengthRange(cells[20], 0, 500, pbValidate, counterFileValidate, Lineno, "NOTE_TO_SUPPLIER", "21");
                                        ValidateLengthRange(cells[21], 0, 40, pbValidate, counterFileValidate, Lineno, "RESEND_FLAG", "22");
                                        ValidateDateTime(cells[22], pbValidate, counterFile, Lineno, "CREATION_DATE", "23");
                                        ValidateDateTime(cells[23], pbValidate, counterFile, Lineno, "LAST_INTERFACED_DATE", "24");
                                        ValidateLengthRange(cells[24], 1, 20, pbValidate, counterFileValidate, Lineno, "INTERFACE_ID", "25");
                                        ValidateLengthRange(cells[25], 0, 20, pbValidate, counterFileValidate, Lineno, "QUATATION_ID", "26");
                                        ValidateLengthRange(cells[26], 0, 20, pbValidate, counterFileValidate, Lineno, "CUSTOMER_ID", "27");
                                        break;
                                    #endregion
                                    #region data type LN
                                    case "LN":
                                        ValidateInteger(cells[1], pbValidate, counterFile, Lineno, "LINE_NUMBER", "2");
                                        ValidateLengthRange(cells[2], 1, 40, pbValidate, counterFileValidate, Lineno, "HOSPITEM_CODE", "3");
                                        ValidateLengthRange(cells[3], 0, 4000, pbValidate, counterFileValidate, Lineno, "HOSPITEM_", "4");
                                        ValidateLengthRange(cells[4], 1, 40, pbValidate, counterFileValidate, Lineno, "DISTITEM_CODE", "5");
                                        ValidateLengthRange(cells[5], 0, 40, pbValidate, counterFileValidate, Lineno, "PACK_SIZE_DESC", "6");
                                        ValidateFloat(cells[6], pbValidate, counterFile, Lineno, "ORDERED_QTY", "7");
                                        ValidateLengthRange(cells[7], 1, 20, pbValidate, counterFileValidate, Lineno, "UOM", "8");
                                        ValidateFloat(cells[8], pbValidate, counterFile, Lineno, "PRICE_PER_UNIT", "9");
                                        ValidateFloat(cells[9], pbValidate, counterFile, Lineno, "LINE_AMOUNT", "10");
                                        ValidateFloat(cells[10], pbValidate, counterFile, Lineno, "DISCOUNT_LINE_ITEM", "11");
                                        ValidateLengthRange(cells[11], 1, 2, pbValidate, counterFileValidate, Lineno, "URGENT_FLAG", "12");
                                        ValidateLengthRange(cells[12], 0, 255, pbValidate, counterFileValidate, Lineno, "COMMENT", "13");
                                        break;
                                        #endregion
                                }
                                #endregion
                            }

                            Lineno++;
                        }
                    }

                    // Change wording in progress bar
                    if (counterFileValidate == FileNum)
                    {
                        pbValidate.Refresh(counterFileValidate, "Validate finished.");
                    }

                    counterFileValidate++;
                }
                #endregion

                #region Import Section
                // Create progress bar (Overall)
                var pbOverall = new ProgressBar(PbStyle.DoubleLine, FileNum);

                foreach (string currentFile in FilePath)
                {
                    // Initial variable
                    headerLineNo = 0;
                    detailLineNo = 0;
                    counterLine = 1;
                    FL_Filecode = "";
                    FL_TotalRecord = "";
                    HD_PoNo = "";

                    returnModel Model = new returnModel();

                    fileName = Path.GetFileName(currentFile);

                    // Create progress bar (Each file)
                    int LineNum = CountLinesReader(currentFile);
                    var pbDetail = new ProgressBar(PbStyle.SingleLine, LineNum);

                    // Update progress bar (Overall)
                    pbOverall.Refresh(counterFile, "Importing, Please wait...");
                    Thread.Sleep(50);

                    using (StreamReader file = new StreamReader(currentFile))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            var parser = CreateParser(line, ",");
                            var parserFirstcolumn = CreateParser(line, ",");
                            var parserFL = CreateParser(line, ",");

                            // Store first column
                            string[] cells;
                            string firstColumn = "";
                            while (!parserFirstcolumn.EndOfData)
                            {
                                cells = parserFirstcolumn.ReadFields();
                                firstColumn = cells[0];
                            }
                            //string[] cells = line.Split(",");

                            // Update progress bar (Each file)
                            pbDetail.Refresh(counterLine, fileName);
                            Thread.Sleep(20);

                            // Determine what firstColumn is
                            switch (firstColumn)
                            {
                                case "FL":
                                    // Store FL key for insert HD
                                    while (!parserFL.EndOfData)
                                    {
                                        cells = parserFL.ReadFields();
                                        FL_Filecode = cells[1];
                                        FL_TotalRecord = cells[2];
                                    }
                                    //Dump(parser);
                                    break;
                                case "HD":
                                    /*
                                    // Throw if FL key not found
                                    if (string.IsNullOrEmpty(FL_Filecode) || string.IsNullOrEmpty(FL_TotalRecord))
                                    {
                                        throw new ArgumentException("FL key not found!");
                                    }
                                    */

                                    // Insert to DB
                                    HD_PoNo = DumpHD(parser, conn, FL_Filecode, FL_TotalRecord, fileName);
                                    headerLineNo++;
                                    break;
                                case "LN":
                                    /*
                                    // Throw if HD key not found
                                    if (string.IsNullOrEmpty(HD_PoNo))
                                    {
                                        throw new ArgumentException("HD key not found!");
                                    }
                                    */

                                    // Insert to DB
                                    DumpLN(parser, conn, HD_PoNo);
                                    detailLineNo++;
                                    break;
                                default:
                                    //throw new ArgumentException("Incorrect format, File must contain 'FL, HD or LN' in the first column on each row!");
                                    break;
                                    //continue;
                            }

                            counterLine++;
                        }
                    }

                    // Create folder for file import successful
                    if (!Directory.Exists(folderBackupPath))
                    {
                        Directory.CreateDirectory(folderBackupPath);
                    }

                    // Move file to folder backup
                    string destFile = Path.Combine(folderBackupPath, fileName);
                    File.Move(currentFile, destFile);

                    // Add detail to model for showing in table
                    Model.HeaderNo = headerLineNo;
                    Model.DetailNo = detailLineNo;
                    Model.FileName = fileName;
                    returnCollection.Add(Model);

                    // Change wording in progress bar
                    if (counterFile == FileNum)
                    {
                        pbOverall.Refresh(counterFile, "Import finished.");
                    }

                    counterFile++;
                }
                #endregion
            }
            catch (Exception ex)
            {
                //pbOverall.Refresh(counterFile, "Import failed");

                // Show error message
                Console.Write("\nError occured : " , Color.OrangeRed);
                Console.WriteLine(ex.Message);
                //Console.WriteLine("Error trace : " + ex.StackTrace);

                // Show error on
                if (!String.IsNullOrEmpty(fileName))
                {
                    Console.Write("\nError on : ", Color.OrangeRed);
                    Console.WriteLine("'" + fileName + "'");
                }
                
                // Show description
                Console.WriteLine("\nPlease check your path or file and try again.\n", Color.Yellow);
            }
            finally
            {
                // Show table
                if (returnCollection.Count > 0)
                {
                    Console.WriteLine("\n--------------- Imported detail ---------------", Color.LightGreen);
                    ConsoleTable.From(returnCollection).Write();
                }
                //Console.WriteLine(JsonSerializer.Serialize(returnCollection));

                // Show backup folder path
                if (Directory.Exists(folderBackupPath))
                {
                    Console.Write("\nImported folder : ", Color.LightGreen);
                    Console.WriteLine($"'{ folderBackupPath }'");
                }
            }

            // Wait key to terminate
            Console.Write("\nPress any key to close this window ");
            Console.ReadKey();
        }

        public static void ValidateLengthRange(string cell, int minLength, int maxLength, ProgressBar pb, int fileNum, int lineNum, string columnName, string columnNum)
        {
            if (cell.Length < minLength || cell.Length > maxLength)
            {
                pb.Refresh(fileNum, "Validate failed.");
                if (minLength == maxLength)
                {
                    throw new ArgumentException($"{columnName} must have {minLength} character ( At Line: { lineNum }, column: {columnNum} )");
                } else
                {
                    throw new ArgumentException($"{columnName} must have {minLength} to {maxLength} character ( At Line: { lineNum }, column: {columnNum} )");
                }
            }
        }

        public static void ValidateDateTime(string cell, ProgressBar pb, int fileNum, int lineNum, string columnName, string columnNum)
        {
            try
            {
                var test = Convert.ToDateTime(cell);
            }
            catch
            {
                pb.Refresh(fileNum, "Validate failed.");
                throw new ArgumentException($"{columnName} must be date ( At Line: { lineNum }, column: {columnNum} )");
            }
        }

        public static void ValidateFloat(string cell, ProgressBar pb, int fileNum, int lineNum, string columnName, string columnNum)
        {
            try
            {
                var test = Convert.ToSingle(cell);
            }
            catch
            {
                pb.Refresh(fileNum, "Validate failed.");
                throw new ArgumentException($"{columnName} must be float ( At Line: { lineNum }, column: {columnNum} )");
            }
        }

        public static void ValidateInteger(string cell, ProgressBar pb, int fileNum, int lineNum, string columnName, string columnNum)
        {
            try
            {
                var test = Convert.ToInt32(cell);
            }
            catch
            {
                pb.Refresh(fileNum, "Validate failed.");
                throw new ArgumentException($"{columnName} must be integer ( At Line: { lineNum }, column: {columnNum} )");
            }
        }

        public static void ValidateBit(string cell, ProgressBar pb, int fileNum, int lineNum, string columnName, string columnNum)
        {
            if (cell != "0" && cell != "1")
            {
                pb.Refresh(fileNum, "Validate failed.");
                throw new ArgumentException($"{columnName} must be 0 or 1 ( At Line: { lineNum }, column: {columnNum} )");
            }
        }
    }
}
