﻿using ConsoleTables;
using Figgle;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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

            // Display title
            //Console.Write(FiggleFonts.Ogre.Render("------------"));
            Console.Write(FiggleFonts.Slant.Render("txt to db by FO"));
            Console.WriteLine("------------------------------------------------------------------------\n");

            // Ask the user to type path
            Console.Write(@"Enter source directory path (eg: D:\folder) : ");
            sourceDirectory = Convert.ToString(Console.ReadLine());
            Console.Write("\n");

            var folderBackup = "Imported_" + DateTime.Now.ToString("ddMMyyyy_HHmmss");
            var targetPath = Path.Combine(sourceDirectory, folderBackup);

            try
            {
                var txtFiles = Directory.EnumerateFiles(sourceDirectory, "*.txt");

                // Create folder for file import successful
                Directory.CreateDirectory(targetPath);

                foreach (string currentFile in txtFiles)
                {
                    returnModel Model = new returnModel();

                    fileName = Path.GetFileName(currentFile);
                    headerLineNo = 0;
                    detailLineNo = 0;

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
                                    break;
                                case "HD":
                                    DumpHD(parser, conn);
                                    headerLineNo++;
                                    break;
                                case "LN":
                                    DumpLN(parser, conn);
                                    detailLineNo++;
                                    break;
                                default:
                                    break;
                                    //continue;
                            }
                        }
                    }

                    // Move file to folder backup
                    var destFile = Path.Combine(targetPath, fileName);
                    File.Move(currentFile, destFile);

                    Model.HeaderNo = headerLineNo;
                    Model.DetailNo = detailLineNo;
                    Model.FileName = fileName;
                    returnCollection.Add(Model);
                }
            } 
            catch (Exception ex)
            {
                Console.WriteLine("Error occured : " + ex.Message);

                if (!String.IsNullOrEmpty(fileName))
                {
                    Console.WriteLine("Error on : '" + fileName + "'");
                }
                
                Console.WriteLine("Please check your path or file and try again.\n");
            }
            finally
            {
                Console.WriteLine("----- Import success list. -----");
                ConsoleTable.From(returnCollection).Write();
                //Console.WriteLine(JsonSerializer.Serialize(returnCollection));
            }

            Console.Write("\nPress any key to close this window ");
            Console.ReadKey();
        }

    }

}
