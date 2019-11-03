using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.Json;
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
            string fileName;
            List<returnModel> returnCollection = new List<returnModel>();
            
            // Display title as the C# console app
            Console.WriteLine("Import txt to DB (PiriyaV)\r");
            Console.WriteLine("------------------------\n");

            // Ask the user to type path.
            Console.Write(@"Enter source directory path (eg: D:\folder): ");
            sourceDirectory = Convert.ToString(Console.ReadLine());

            try
            {
                var txtFiles = Directory.EnumerateFiles(sourceDirectory, "*.txt");
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
                                    continue;
                            }
                        }
                    }

                    Model.HeaderNo = headerLineNo;
                    Model.DetailNo = detailLineNo;
                    Model.FileName = fileName;
                    returnCollection.Add(Model);
                }
            } 
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            finally
            {
                ConsoleTable.From(returnCollection).Write();
                //Console.WriteLine(JsonSerializer.Serialize(returnCollection));
            }

            Console.WriteLine();
            Console.Write(@"Press any key to close this window");
            Console.ReadKey();
        }

    }

}
