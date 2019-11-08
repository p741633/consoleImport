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
            string FL_Filecode = "";
            string FL_TotalRecord = "";
            string HD_PoNo = "";
            int counter = 1;

            // Display title
            //Console.Write(FiggleFonts.Ogre.Render("------------"));
            Console.WriteWithGradient(FiggleFonts.Banner.Render("txt to db"), Color.LightGreen, Color.ForestGreen, 16);

            Colorful.Console.ReplaceAllColorsWithDefaults();

            Console.WriteLine(" --------------- Created by PiriyaV ----------------\n", Color.LawnGreen);
            /*
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

            // Ask the user to type path
            Console.Write(@"Enter source path (eg: D:\folder) : ", Color.LightYellow);
            sourceDirectory = Convert.ToString(Console.ReadLine());
            Console.Write("\n");

            var folderBackup = "imported_" + DateTime.Now.ToString("ddMMyyyy_HHmmss");
            var targetPath = Path.Combine(sourceDirectory, folderBackup);

            try
            {
                var txtFiles = Directory.EnumerateFiles(sourceDirectory, "*.txt");

                DirectoryInfo di = new DirectoryInfo(sourceDirectory);
                var countFiles = di.GetFiles("*.txt").Length;
                var pb = new ProgressBar(PbStyle.DoubleLine, countFiles);

                if (countFiles == 0)
                {
                    throw new ArgumentException("Text file not found in folder.");
                }

                foreach (string currentFile in txtFiles)
                {
                    returnModel Model = new returnModel();

                    fileName = Path.GetFileName(currentFile);
                    headerLineNo = 0;
                    detailLineNo = 0;

                    pb.Refresh(counter, "Import in process, Don't close the window. :-)");
                    Thread.Sleep(75);

                    using (StreamReader file = new StreamReader(currentFile))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            var parser = CreateParser(line, ",");

                            string[] cells = line.Split(",");
                            var firstColumn = cells[0];

                            switch (firstColumn)
                            {
                                case "FL":
                                    //Dump(parser);
                                    FL_Filecode = cells[1];
                                    FL_TotalRecord = cells[2];
                                    break;
                                case "HD":
                                    if (string.IsNullOrEmpty(FL_Filecode) || string.IsNullOrEmpty(FL_TotalRecord))
                                    {
                                        throw new ArgumentException("FL key not found!");
                                    }

                                    HD_PoNo = DumpHD(parser, conn, FL_Filecode, FL_TotalRecord);
                                    headerLineNo++;
                                    break;
                                case "LN":
                                    if (string.IsNullOrEmpty(HD_PoNo))
                                    {
                                        throw new ArgumentException("HD key not found!");
                                    }

                                    DumpLN(parser, conn, HD_PoNo);
                                    detailLineNo++;
                                    break;
                                default:
                                    throw new ArgumentException("Incorrect format, File must contain 'FL, HD or LN' in the first column on each row!");
                                    //break;
                                    //continue;
                            }
                        }
                    }

                    // Create folder for file import successful
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }

                    // Move file to folder backup
                    var destFile = Path.Combine(targetPath, fileName);
                    File.Move(currentFile, destFile);

                    Model.HeaderNo = headerLineNo;
                    Model.DetailNo = detailLineNo;
                    Model.FileName = fileName;
                    returnCollection.Add(Model);

                    counter++;
                }
            } 
            catch (Exception ex)
            {
                Console.Write("\nError occured : " , Color.OrangeRed);
                Console.WriteLine(ex.Message);
                //Console.WriteLine("Error trace : " + ex.StackTrace);

                if (!String.IsNullOrEmpty(fileName))
                {
                    Console.Write("\nError on : ", Color.OrangeRed);
                    Console.WriteLine("'" + fileName + "'");
                }
                
                Console.WriteLine("\nPlease check your path or file and try again.\n", Color.Yellow);
            }
            finally
            {
                
                if (returnCollection.Count > 0)
                {
                    Console.WriteLine("\n--------------- Imported list ---------------", Color.LightGreen);
                    ConsoleTable.From(returnCollection).Write();
                }
                
                //Console.WriteLine(JsonSerializer.Serialize(returnCollection));

                if (Directory.Exists(targetPath))
                {
                    Console.Write("\nImported folder : ", Color.LightGreen);
                    Console.WriteLine($"'{ targetPath }'");
                }
            }

            Console.Write("\nPress any key to close this window ");
            Console.ReadKey();
        }

    }

}
