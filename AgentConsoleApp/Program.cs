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

            // Display title
            Console.Title = "TxtToDB 1.02";

            // Display header
            Console.WriteWithGradient(FiggleFonts.Banner.Render("txt to db"), Color.LightGreen, Color.ForestGreen, 16);
            Console.ReplaceAllColorsWithDefaults();

            // Display copyright
            Console.WriteLine(" --------------- Created by PiriyaV ----------------\n", Color.LawnGreen);

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
            Console.Write(@"Enter source path (eg: D:\folder) : ", Color.LightYellow);
            sourceDirectory = Convert.ToString(Console.ReadLine());
            Console.Write("\n");

            // Variable for backup
            string folderBackup = "imported_" + DateTime.Now.ToString("ddMMyyyy_HHmmss");
            string folderBackupPath = Path.Combine(sourceDirectory, folderBackup);

            try
            {
                // Full path for txt
                var FilePath = Directory.EnumerateFiles(sourceDirectory, "*.txt");

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
                            }

                            if (Lineno == 1 && firstColumn != "FL")
                            {
                                IsHeaderValid = false;
                                ErrorMsg = "First line must contain FL column";
                            }
                            else if (Lineno ==2 && firstColumn != "HD")
                            {
                                IsHeaderValid = false;
                                ErrorMsg = "Second line must contain HD column";
                            }
                            else if (Lineno >= 3 && firstColumn != "LN")
                            {
                                IsHeaderValid = false;
                                ErrorMsg = $"Data must contain LN column (At line { Lineno })";
                            }

                            if (!IsHeaderValid)
                            {
                                pbValidate.Refresh(counterFileValidate, "Validate failed.");
                                throw new ArgumentException(ErrorMsg);
                            }

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
                                        ErrorMsg = $"LN must have 13 columns (At line { Lineno }, { ColumnNo } columns found)";
                                    }
                                    break;
                                default:
                                    IsColumnValid = false;
                                    ErrorMsg = $"Incorrect format, File must contain 'FL, HD or LN' in the first column on each row! (At Line { Lineno})";
                                    break;
                                    //continue;
                            }

                            if (!IsColumnValid)
                            {
                                pbValidate.Refresh(counterFileValidate, "Validate failed.");
                                throw new ArgumentException(ErrorMsg);
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
                                    HD_PoNo = DumpHD(parser, conn, FL_Filecode, FL_TotalRecord);
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

    }

}
