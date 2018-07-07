using System;
using System.Collections.Generic;
using System.Linq;

namespace VSSolutionTemplateTool
{
    internal class Program
    {
        protected Program()
        {
        }

        static void Main()
        {
            List<string> extensionsToProcess = new List<string>
            {
                ".config"
                ,".xml"
                ,".js"
                ,".css"
                ,".txt"
                ,".cs"
                ,".csproj"
                ,".asax"
                ,".user"
                ,".cshtml"
                ,".sln"
            };

            List<string> foldersToSkip = new List<string>
            {
                "bin"
                ,"obj"
            };

            Console.Title = "Solution Template Tool";

            Console.Write("Enter template folder path >");

            string templateFolderPath = Settings1.Default.Properties["SolutionFolder"]?.DefaultValue.ToString();

            bool pathExists = System.IO.Directory.Exists(templateFolderPath);

            if (!pathExists)
            {

                Console.WriteLine($"the path {templateFolderPath} does not exist");

                Console.WriteLine("Press enter to exit >");

                Console.ReadLine();

                return;
            }


            string outputFolderInput = Settings1.Default.Properties["DestinationFolder"]?.DefaultValue.ToString();
            pathExists = System.IO.Directory.Exists(outputFolderInput);

            if (!pathExists)
            {
                Console.WriteLine("the path does not exist, create folder");
                //Console.WriteLine("Would you like to create it? (y/n)>");
                //if (Console.ReadLine()?.ToLower() == "y")
                //{
                    try
                    {
                        System.IO.Directory.CreateDirectory(outputFolderInput ?? throw new InvalidOperationException());
                    }
                    catch (Exception)
                    {

                        Console.WriteLine($"Could not create {outputFolderInput}");
                        Console.WriteLine("Press enter to exit >");

                        Console.ReadLine();
                        return;
                    }

                //}
                //else
                //{
                //    Console.WriteLine("Press enter to exit >");

                //    Console.ReadLine();

                //    return;
                //}
            }

            string outputFolderPath = string.IsNullOrWhiteSpace(outputFolderInput) ? System.IO.Path.Combine(templateFolderPath, "OutputFolder") : outputFolderInput;
            string templateToken = Settings1.Default.Properties["KeywordForReplacement"]?.DefaultValue.ToString();

            if (string.IsNullOrWhiteSpace(templateToken))
            {
                Console.WriteLine("missing KeywordForReplacement");
                Console.WriteLine("Press enter to exit >");

                Console.ReadLine();
                return;
            }

            Console.WriteLine("Looking for solution file . . .");

            IEnumerable<string> files = System.IO.Directory.EnumerateFiles(templateFolderPath, "*.sln");

            var enumerable = files.ToList();
            if (enumerable.Count == 1)
            {

                string fileName = System.IO.Path.GetFileName(enumerable.Single());

                Console.WriteLine("Solution file {0} was found in the folder {1}.", fileName, templateFolderPath);

                string newSolutionName = Settings1.Default.Properties["NewSolutionKeyword"]?.DefaultValue.ToString();

                Console.WriteLine("Processing the {0} template . . . ", fileName);
                Console.WriteLine("Creating the {0} solution from the {1} template . . .", newSolutionName, fileName);

                string[] directoryNames = System.IO.Directory.GetDirectories(templateFolderPath, "*", System.IO.SearchOption.AllDirectories);

                List<string> directories = directoryNames.OrderBy(s => s).ToList();

                ProcessDirectory(templateFolderPath, templateFolderPath, outputFolderPath, newSolutionName, templateToken, extensionsToProcess, foldersToSkip);


                foreach (var directory in directories)
                {

                    ProcessDirectory(directory, templateFolderPath, outputFolderPath, newSolutionName, templateToken, extensionsToProcess, foldersToSkip);

                }

            }
            else
            {

                if (!enumerable.Any())
                {

                    Console.WriteLine("The folder {0} does not contain a solution file.", templateFolderPath);

                }
                else
                {

                    Console.WriteLine("The folder {0} contains more than 1 solution file.\n {1} files found.\nThis is not permitted.", templateFolderPath, enumerable.Count);

                }

                Console.WriteLine("Press enter to exit >");

                Console.ReadLine();

                return;

            }

            Console.WriteLine("Press enter to exit >");

            Console.ReadLine();

        }

        private static void ProcessDirectory(string directory, string templatePath, string outputFolderPath, string newName, string templateToken, List<string> extensionsToProcess, List<string> foldersToSkip)
        {
            string newDirectoryName = directory.Replace(templatePath, outputFolderPath);

            if (foldersToSkip.Any(directory.Contains))
            {

                Console.WriteLine("Skipping {0} \n\n\n", directory);

                return;

            }

            Console.WriteLine("Processing {0} . . . ", directory);

            if (directory.Contains(templateToken))
            {

                newDirectoryName = newDirectoryName.Replace(templateToken, newName);

                Console.WriteLine("Renaming. ");

            }

            Console.WriteLine("Creating the folder {0}", newDirectoryName);

            System.IO.Directory.CreateDirectory(newDirectoryName);

            Console.WriteLine("Processing files . . . ");

            List<string> fileNames = System.IO.Directory.GetFiles(directory).OrderBy(d => d).ToList();

            foreach (var processingFileName in fileNames)
            {

                string extension = System.IO.Path.GetExtension(processingFileName);

                string fileNameToCreate = System.IO.Path.GetFileName(processingFileName)?.Replace(templateToken, newName);

                string fileToCreate = System.IO.Path.Combine(newDirectoryName, fileNameToCreate ?? throw new InvalidOperationException());

                if (extensionsToProcess.Contains(extension))
                {

                    int replacements = 0;

                    Console.WriteLine("\tProcessing {0} . . . ", fileNameToCreate);

                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(System.IO.File.Create(fileToCreate)))
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(processingFileName))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {

                                if (line.Contains(templateToken))
                                {
                                    Console.WriteLine(line);

                                    line = line.Replace(templateToken, newName);

                                    replacements++;

                                    Console.WriteLine(line);
                                }

                                writer.WriteLine(line);

                            }
                        }

                    }

                    Console.WriteLine("\t{0} lines with token altered.", replacements);

                }
                else
                {

                    Console.WriteLine("Copying {0}  ", fileNameToCreate);

                    System.IO.File.Copy(processingFileName, fileToCreate, true);

                }



            }

            Console.WriteLine("\n\n");
        }
    }
}
