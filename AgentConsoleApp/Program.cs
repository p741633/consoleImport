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
            string fileName = "";
            List<returnModel> returnCollection = new List<returnModel>();
            string FL_Filecode;
            string FL_TotalRecord;
            string HD_PoNo;
            int counterFile = 1;
            int counterLine;

            // Display title
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
            var folderBackup = "imported_" + DateTime.Now.ToString("ddMMyyyy_HHmmss");
            var folderBackupPath = Path.Combine(sourceDirectory, folderBackup);

            try
            {
                // Full path for txt
                var FilePath = Directory.EnumerateFiles(sourceDirectory, "*.txt");

                // Count txt file
                DirectoryInfo di = new DirectoryInfo(sourceDirectory);
                var FileNum = di.GetFiles("*.txt").Length;

                // Create progress bar (Overall)
                var pbOverall = new ProgressBar(PbStyle.DoubleLine, FileNum);

                // Throw no txt file
                if (FileNum == 0)
                {
                    throw new ArgumentException("Text file not found in folder.");
                }

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
                    var LineNum = CountLinesReader(currentFile);
                    var pbDetail = new ProgressBar(PbStyle.SingleLine, LineNum);

                    // Update progress bar (Overall)
                    pbOverall.Refresh(counterFile, "");
                    Thread.Sleep(50);

                    using (StreamReader file = new StreamReader(currentFile))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            var parser = CreateParser(line, ",");

                            // Store first column
                            string[] cells = line.Split(",");
                            var firstColumn = cells[0];

                            // Update progress bar (Each file)
                            pbDetail.Refresh(counterLine, fileName);
                            Thread.Sleep(20);

                            // Determine what firstColumn is
                            switch (firstColumn)
                            {
                                case "FL":
                                    // Store FL key for
                                    FL_Filecode = cells[1];
                                    FL_TotalRecord = cells[2];
                                    //Dump(parser);
                                    break;
                                case "HD":
                                    // Throw if FL key not found
                                    if (string.IsNullOrEmpty(FL_Filecode) || string.IsNullOrEmpty(FL_TotalRecord))
                                    {
                                        throw new ArgumentException("FL key not found!");
                                    }

                                    // Insert to DB
                                    HD_PoNo = DumpHD(parser, conn, FL_Filecode, FL_TotalRecord);
                                    headerLineNo++;
                                    break;
                                case "LN":
                                    // Throw if HD key not found
                                    if (string.IsNullOrEmpty(HD_PoNo))
                                    {
                                        throw new ArgumentException("HD key not found!");
                                    }

                                    // Insert to DB
                                    DumpLN(parser, conn, HD_PoNo);
                                    detailLineNo++;
                                    break;
                                default:
                                    throw new ArgumentException("Incorrect format, File must contain 'FL, HD or LN' in the first column on each row!");
                                    //break;
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
                    var destFile = Path.Combine(folderBackupPath, fileName);
                    File.Move(currentFile, destFile);

                    // Add detail to model for showing in table
                    Model.HeaderNo = headerLineNo;
                    Model.DetailNo = detailLineNo;
                    Model.FileName = fileName;
                    returnCollection.Add(Model);

                    // Change wording in progress bar
                    if (counterFile == FileNum)
                    {
                        pbOverall.Refresh(counterFile, "Finished.");
                    }

                    counterFile++;
                }
            } 
            catch (Exception ex)
            {
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
