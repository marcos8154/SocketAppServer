using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace propagate
{
    /// <summary>
    /// This program aims to copy the modified files
    /// from the SocketAppServer project (net4x) to 
    /// SocketAppServer.Standard (netstd2)
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Interator();
        }


        static void Interator()
        {
            Console.Write("> ");
            string command = Console.ReadLine();
            if (string.IsNullOrEmpty(command))
                Interator();

            try
            {
                string[] args = command.Split(' ');
                if (args[0] == "?" || args[0] == "help")
                    ShowCommandList();
                if (args[0] == "map-path")
                    MapPath(args);
                if (args[0] == "map-show")
                    ShowPathMap();
                if (args[0] == "exclude")
                    AddExclude(args);
                if (args[0] == "propagate")
                    PropagateNow();
                if (args[0] == "cls" || args[0] == "clear")
                {
                    Console.Clear();
                    //      Console.Write("> ");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }

            Interator();
        }

        private static void ShowPathMap()
        {
            string file = @".\paths.plist";
            string[] paths = (File.Exists(file)
                            ? File.ReadAllLines(file)
                            : new string[] { });

            if (paths.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("empty list.");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                paths.ToList().ForEach(p => Console.WriteLine($"    {p}"));
                Console.ForegroundColor = ConsoleColor.White;

            }
        }

        private static void ShowCommandList()
        {
            List<string> commands = new List<string>();
            commands.Add("map-show * displays the list of directories mapped to the source and target projects paths");
            commands.Add("map-path * adds the project's base directory mapping \n args: \n map-path [type] [path] \n[type]: source | target \n[path]: path");
            commands.Add("exclude * adds an exclusion file during the propagation process \n args: \nexclude [action] [filename] \n[action]: add | remove | list \n[file]: file name with extension (whitout path)");
            commands.Add("propagate * performs the propagation process; the previous steps must be configured");

            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (string cmd in commands)
            {
                string[] command = cmd.Split('*');
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(command[0] + " ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write(command[1]);
                Console.WriteLine("\n");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void AddExclude(string[] args)
        {
            string confFile = @".\exclude.plist";
            List<string> excludeList = (File.Exists(confFile)
                ? File.ReadAllLines(confFile).ToList()
                : new List<string>());

            string action = args[1];

            if (action != "add" &&
                action != "remove" &&
                action != "list")
                throw new Exception($"Invalid arg: '{action}'. Valid args: \nadd -- add file to exclude list \nremove -- remove file from exclude list \nlist -- list all files in exclude list");

            if (action == "list")
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                foreach (string exclude in excludeList)
                    Console.WriteLine($"    {exclude}");
                if (excludeList.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("empty list");
                }
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }

            string[] files = args[2].Split(',');
            if (action == "add")
                excludeList.AddRange(files);
            if (action == "remove")
                excludeList.RemoveAll(e => files.Contains(e));

            File.WriteAllLines(confFile, excludeList.ToArray());
            ShowDone();
        }

        private static void PropagateNow()
        {
            propagatedFiles = 0;
            string[] paths = File.ReadAllLines(@".\paths.plist");
            string sourcePath = paths[0].Replace("source:", "");
            string targetPath = paths[1].Replace("target:", "");

            string excludeFile = @".\exclude.plist";
            string[] excludeList = (File.Exists(excludeFile)
                ? File.ReadAllLines(excludeFile)
                : new string[] { });

            ScanSource(new DirectoryInfo(sourcePath), new DirectoryInfo(targetPath), excludeList);
            ShowDone();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Propagated files: {propagatedFiles}.");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void ScanSource(DirectoryInfo sourcePath, DirectoryInfo targetPath, string[] excludeList)
        {
            foreach (FileInfo file in sourcePath.GetFiles())
            {
                if (excludeList.Contains(file.Name))
                    continue;

                TryPropagateFile(file, targetPath);
            }

            foreach (DirectoryInfo subDir in sourcePath.GetDirectories())
                ScanSource(subDir, targetPath, excludeList);
        }

        static int propagatedFiles = 0;
        private static void TryPropagateFile(FileInfo sourceFile, DirectoryInfo targetPath)
        {
            foreach (FileInfo targetFile in targetPath.GetFiles())
            {

                if (targetFile.Name == sourceFile.Name)
                {
                    if (sourceFile.LastWriteTime > targetFile.LastWriteTime)
                    {
                        File.Copy(sourceFile.FullName, targetFile.FullName, true);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"    '{sourceFile.Name}' propagated.");
                        Console.ForegroundColor = ConsoleColor.White;

                        propagatedFiles += 1;
                        break;
                    }
                }
            }

            foreach (DirectoryInfo subDir in targetPath.GetDirectories())
                TryPropagateFile(sourceFile, subDir);
        }

        private static void MapPath(string[] args)
        {
            string pathType = args[1];
            string path = "";
            for (int i = 2; i < args.Length; i++)
                if (args[i].StartsWith("\\"))
                    path += args[i];
                else
                    path += $" {args[i]}";

            path = path.TrimStart().TrimEnd();
            if (pathType != "source" &&
                pathType != "target")
                throw new Exception(@"Invalid path type");

            if (!Directory.Exists(path))
                throw new Exception($"Directory not found: '{path}'");

            string confFile = $@".\paths.plist";
            string[] confStr = (File.Exists(confFile)
                ? File.ReadAllLines(confFile)
                : new string[] { "source:", "target:" });

            if (pathType == "source")
                confStr[0] = $"source:{path}";
            else
                confStr[1] = $"target:{path}";

            File.WriteAllLines(confFile, confStr);
            ShowDone();
        }

        private static void ShowDone()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
