[name]ACSC
[reference]system.dll
[reference]System.Drawing.dll
[reference]System.Windows.Forms.dll
[reference]System.Runtime.InteropServices.dll
[reference]system.core.dll
[reference]mscorlib.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Resources;

namespace csCompiler
{
    class CompilerSource
    {
        static bool isDir = false;

        List<string> customCode = new List<string>();

        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Cyan;
            //If the number of arguments is larger than 0
            if (args.Length > 0)
            {
                //Get the file attributes for file or directory
                FileAttributes attr = File.GetAttributes(@args[0]);

                //Detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    isDir = true;
                }
                else
                {
                    isDir = false;
                }

                string mainClass = args[0];
                string name = Path.GetFileNameWithoutExtension(args[0]);
                bool hideConsole = false;//Whether or not the compiled .exe has a hidden console(for win forms etc)
                List<string> references = new List<string>();
                List<string> classes = new List<string>();

                string icon = "";

                //If it is a directory, A.C.S.C will search the directory for a file named "base.cs", as that will be the main file to base the program off
                if (isDir)
                {
                    AprilSpeak("I don't know what you think you are doing... But I don't work like that!");
                }
                else //If it is just a file
                {
                        foreach(string line in File.ReadAllLines(args[0]))
                        {
                            if (line.StartsWith("//"))
                            {
                                if (line.ToLower().Trim().StartsWith("//[reference]") || line.ToLower().Trim().StartsWith("//[ref]")) //If the line reads "[reference]", then we want to add that reference path to the .dll file to the compiler
                                {
                                    references.Add(line.ToLower().Replace("//[reference]", "").Replace("//[ref]", "").Replace(";", "").Trim()); //This will add the reference to the compiler
                                }
                                if (line.ToLower().Trim().StartsWith("//[class]")) //If the line reads "[class]", then we want to add that class path to the .cs file to the temp file we are going to compile
                                {
                                    if (!line.ToLower().Replace("//[class]", "").Contains(":"))
                                    {
                                        classes.Add(Path.GetDirectoryName(args[0]) + "/" + line.ToLower().Replace("//[class]", "").Replace(";", "").Trim()); //This will add the reference to the compiler
                                    }
                                    else
                                    {
                                        classes.Add(line.ToLower().Replace("//[class]", "").Replace(";", "").Trim());
                                    }
                                }
                                if (line.ToLower().Trim().StartsWith("//[icon]")) //If the line reads "[class]", then we want to add that class path to the .cs file to the temp file we are going to compile
                                {
                                    if (!line.ToLower().Replace("//[icon]", "").Contains(":"))
                                    {
                                        icon = Path.GetDirectoryName(args[0]) + "\\" + line.ToLower().Replace("//[icon]", "").Replace(";", "").Trim();
                                        //classes.Add(Path.GetDirectoryName(args[0]) + "/" + line.ToLower().Replace("[icon]", "").Replace(";", "").Trim()); //This will add the reference to the compiler
                                    }
                                    else
                                    {
                                        icon = line.ToLower().Replace("//[icon]", "").Replace(";", "").Trim();
                                    }
                                }
                                if (line.ToLower().Trim().StartsWith("//[name]"))
                                {
                                    name = line.ToLower().Replace("//[name]", "").Replace(";", "").Trim();
                                }
                                /*if (line.ToLower().Trim().StartsWith("//[name]"))
                                {
                                    name = line.ToLower().Replace("//[name]", "").Trim();
                                }*/
                                if (line.ToLower().Trim().StartsWith("//[hidden]"))
                                {
                                    hideConsole = true;
                                }
                            }
                            else
                            {
                                if (line.ToLower().Trim().StartsWith("[reference]") || line.ToLower().Trim().StartsWith("[ref]")) //If the line reads "[reference]", then we want to add that reference path to the .dll file to the compiler
                                {
                                    references.Add(line.ToLower().Replace("[reference]", "").Replace("[ref]", "").Replace(";", "").Trim()); //This will add the reference to the compiler
                                }
                                if (line.ToLower().Trim().StartsWith("[class]")) //If the line reads "[class]", then we want to add that class path to the .cs file to the temp file we are going to compile
                                {
                                    if (!line.ToLower().Replace("[class]", "").Contains(":"))
                                    {
                                        classes.Add(Path.GetDirectoryName(args[0]) + "/" + line.ToLower().Replace("[class]", "").Replace(";", "").Trim()); //This will add the reference to the compiler
                                    }
                                    else
                                    {
                                        classes.Add(line.ToLower().Replace("[class]", "").Replace(";", "").Trim());
                                    }
                                }
                                if (line.ToLower().Trim().StartsWith("[icon]")) //If the line reads "[class]", then we want to add that class path to the .cs file to the temp file we are going to compile
                                {
                                    if (!line.ToLower().Replace("[icon]", "").Contains(":"))
                                    {
                                        icon = Path.GetDirectoryName(args[0]) + "\\" + line.ToLower().Replace("[icon]", "").Replace(";", "").Trim();
                                        //classes.Add(Path.GetDirectoryName(args[0]) + "/" + line.ToLower().Replace("[icon]", "").Replace(";", "").Trim()); //This will add the reference to the compiler
                                    }
                                    else
                                    {
                                        icon = line.ToLower().Replace("[icon]", "").Replace(";", "").Trim();
                                    }
                                }
                                if (line.ToLower().Trim().StartsWith("[name]"))
                                {
                                    name = line.ToLower().Replace("[name]", "").Replace(";", "").Trim();
                                }
                                /*if (line.ToLower().Trim().StartsWith("//[name]"))
                                {
                                    name = line.ToLower().Replace("//[name]", "").Trim();
                                }*/
                                if (line.ToLower().Trim().StartsWith("[hidden]"))
                                {
                                    hideConsole = true;
                                }
                            }
                        }

                        string[] temp;

                        //Now we want to create a temporary file, copy the main class file to it, and then copy the data from the included classes to it.
                        if (!File.Exists(Path.GetDirectoryName(args[0]) + "/temp.tmp"))
                        {
                            StreamWriter sw = File.CreateText(Path.GetDirectoryName(args[0]) + "/temp.tmp");
                            sw.Close();
                        }
                        else
                        {
                            File.Delete(Path.GetDirectoryName(args[0]) + "/temp.tmp");
                            StreamWriter sw = File.CreateText(Path.GetDirectoryName(args[0]) + "/temp.tmp");
                            sw.Close();
                        }
                        List<string> resUsing = new List<string>();//Reserved using commands

                        List<string> tempList = new List<string>();//List the temp file so we can add/remove any text needed for extras

                        temp = File.ReadAllLines(args[0]);

                        tempList.AddRange(temp);

                        for (int k = 0; k < temp.Length - 1; k++)
                        {
                            //AprilSpeak(temp[k]);
                            if (hideConsole)
                            {
                                //Add variables to hide window
                                try
                                {
                                    if ((temp[k - 1].Trim().ToLower().StartsWith("{") && temp[k - 2].Trim().ToLower().Contains("class ")) || (temp[k - 1].Trim().ToLower().EndsWith("{") && temp[k - 1].Trim().ToLower().Contains("class ")))
                                    {
                                        tempList.InsertRange(k, AddHiddenVariables());
                                        //AprilSpeak("Added var to class line " + k);
                                    }
                                }
                                catch (Exception ex) { }

                                try{
                                //Add code to Main() for hiding console window
                                    if ((temp[k - 1].Trim().ToLower().StartsWith("{") && temp[k - 2].Trim().Contains("void Main(")) || (temp[k - 1].Trim().ToLower().EndsWith("{") && temp[k - 1].Trim().Contains("void Main(")))
                                    {
                                        tempList.InsertRange(k + 7, HideConsole());
                                        //AprilSpeak("Added var to Main line " + k);
                                    }
                                }
                                catch (Exception ex) { }
                                //Console.ReadLine();
                            }
                            
                        }

                        tempList.Add("//[reference]System.Runtime.InteropServices.dll");

                        temp = tempList.ToArray();

                        for (int j = 0; j < temp.Length - 1; j++)
                        {
                            if (temp[j].StartsWith("using"))
                            {
                                if (!resUsing.Contains(temp[j]))
                                {
                                    resUsing.Add(temp[j]);
                                }
                                temp[j] = "";
                            }
                            if (temp[j].ToLower().Trim().StartsWith("[reference]") || temp[j].ToLower().Trim().StartsWith("[icon]") || temp[j].ToLower().Trim().StartsWith("[class]") || temp[j].ToLower().Trim().StartsWith("[ref]") || temp[j].ToLower().Trim().StartsWith("[name]") || temp[j].ToLower().Trim().StartsWith("[hidden]"))
                            {
                                temp[j] = "//" + temp[j];
                            }
                        }

                        if(hideConsole){
                            if(!resUsing.Contains("using System.Runtime.InteropServices;")){
                                resUsing.Add("using System.Runtime.InteropServices;");
                            }
                        }

                        File.AppendAllLines(Path.GetDirectoryName(args[0]) + "/temp.tmp", temp);
                        File.AppendAllText(Path.GetDirectoryName(args[0]) + "/temp.tmp", "\n\n");

                        if (classes.Count > 0)
                        {
                            for (int i = 0; i < classes.Count; i++)
                            {
                                temp = File.ReadAllLines(classes[i]);

                               
                                for (int j = 0; j < temp.Length - 1; j ++)
                                {
                                    if (temp[j].StartsWith("using"))
                                    {
                                        if (!resUsing.Contains(temp[j]))
                                        {
                                            resUsing.Add(temp[j]);
                                        }
                                        temp[j] = "";
                                    }
                                    if (temp[j].ToLower().Trim().StartsWith("[reference]") || temp[j].ToLower().Trim().StartsWith("[icon]") || temp[j].ToLower().Trim().StartsWith("[class]") || temp[j].ToLower().Trim().StartsWith("[ref]") || temp[j].ToLower().Trim().StartsWith("[name]") || temp[j].ToLower().Trim().StartsWith("[hidden]"))
                                    {
                                        temp[j] = "//" + temp[j];
                                    }
                                }

                                File.AppendAllLines(Path.GetDirectoryName(args[0]) + "/temp.tmp", temp);
                                File.AppendAllText(Path.GetDirectoryName(args[0]) + "/temp.tmp", "\n\n");
                            }
                        }

                        if (resUsing.Count > 0)
                        {
                            PrependAllLines(Path.GetDirectoryName(args[0]) + "/temp.tmp", resUsing.ToArray());
                        }

                        string sir = Path.GetDirectoryName(args[0]) + "\\temp.tmp";

                        //AprilSpeak(Path.GetDirectoryName(args[0]) + "/temp.cs");
                        //Console.ReadLine();

                        CompileFile(sir, Path.GetDirectoryName(args[0]) + "/" + name + ".exe", references.ToArray(), icon);
						AprilSpeak("I have successfully evaluated and compiled '" + name + ".exe' without any errors...");
						Console.ReadLine();

                        try
                        {
                            //File.Delete(Path.GetDirectoryName(args[0]) + "/temp.tmp");
                        }
                        catch (Exception ex) { AprilSpeak(ex.ToString());}
                }
            }
            else
            {
                //If the compiler has been clicked on instead of dragged over
                AprilSpeak("Hello. I am the April Compiler module. I can compile special types of C Sharp files sources. I make the compiler smarter and more user friendly to use without Visual Studio!");

                while(true){
                    Console.ForegroundColor = ConsoleColor.Green;
                    string input = Console.ReadLine().ToLower().Trim();
                    //Interpret input and reply with output
                    if (input == "help")
                    {
                        AprilSpeak("Well, if you have any source files, just drag them onto the Compiler(The executable you clicked on to bring me up!). My style is pretty much just the same Syntax as C Sharp, except you can call different commands from the beginning of the source file, making things easier! Here are a list of commands...");
                        AprilSpeak("");
                        AprilSpeak("'[reference]*DLL*' - This will add a dll/library file to the list of references. Example: [reference]System.dll");
                        AprilSpeak("'[class]*FILE*' - This will add a reference to a class file to add to the source. Example: [class]MyClass.cs");
                        AprilSpeak("'[name]*STRING*' - This will set the application executable name. Example: [name]My Application");
                        AprilSpeak("'[hidden]' - This will hide the console window for the executable. Good for making WinForm applications, or background tasks");
                    }
                    else if (input.StartsWith("generate"))
                    {
                        using (StreamWriter sw = new StreamWriter("Source.cs"))
                        {
                            sw.WriteLine("/*");
                            sw.WriteLine(" * This is a source file created using ACSC.");
                            sw.WriteLine(" * You can now build a project from here and compile it by dragging it");
                            sw.WriteLine(" * onto the ACSC application.");
                            sw.WriteLine(" *");
                            sw.WriteLine(" * Date Of Creation: " + DateTime.Now.ToString("dd/MM/yyyy h:mm tt"));
                            sw.WriteLine(" */");
                            sw.WriteLine("[Reference]System.dll");
                            sw.WriteLine("[Name]Source");
                            sw.WriteLine("");
                            sw.WriteLine("using System;");
                            sw.WriteLine("");
                            sw.WriteLine("namespace SourceFile {");
                            sw.WriteLine("    class Source {");
                            sw.WriteLine("");
                            sw.WriteLine("        static void Main() {");
                            sw.WriteLine("            Console.WriteLine(\"This is a source file created by April...\");");
                            sw.WriteLine("            Console.ReadLine();");
                            sw.WriteLine("        }");
                            sw.WriteLine("    }");
                            sw.WriteLine("}");
                        }
                        AprilSpeak("File has been generated. I named it 'source.cs'. This will make a good base for you to work from.");
                        Console.ReadLine();
                        return;
                    }
                    else if (input == "exit" || input == "quit")
                    {
                        AprilSpeak("Ok then. Goodbye!");
                        Console.ReadLine();
                        return;
                    }

                }
            }
        }

        public static void AprilSpeak(string str)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("APRIL: " + str);
        }
        
        private static void CompileFile(string code, string output_filename, string[] references, string icon)
        {
            CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");
            string Output = output_filename;

            System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
            //Make sure we generate an EXE, not a DLL
            parameters.GenerateExecutable = true;
            var assemblies = AppDomain.CurrentDomain
                            .GetAssemblies()
                            .Where(a => !a.IsDynamic)
                            .Select(a => a.Location);

            //parameters.ReferencedAssemblies.AddRange(assemblies.ToArray()); //This will add the references in this compiler to the compiled .exe
            if (references.Length > 0)
            {
                parameters.ReferencedAssemblies.AddRange(references);
            }

            parameters.GenerateInMemory = false;
            parameters.OutputAssembly = Output;
            //parameters.MainClass = main_class;
            if(icon != ""){
                parameters.CompilerOptions = "/platform:x86 /optimize+ /win32icon:\"" + icon + "\"";
            }else{
                parameters.CompilerOptions = "/platform:x86 /optimize+";
            }

            CompilerResults results = codeProvider.CompileAssemblyFromFile(parameters, code);

            if (results.Errors.Count > 0)
            {
                if (File.Exists(Environment.CurrentDirectory + "/temp.tmp"))
                {
                    try
                    {
                        File.Delete(Environment.CurrentDirectory + "/temp.tmp");
                    }
                    catch (Exception ex) { AprilSpeak(ex.Message); }
                }
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (CompilerError CompErr in results.Errors)
                {
                    AprilSpeak(
                                "Line number " + CompErr.Line +
                                ", Error Number: " + CompErr.ErrorNumber +
                                ", '" + CompErr.ErrorText + ";" +
                                Environment.NewLine + Environment.NewLine);

                }
                Console.ReadLine();
            }

            if (File.Exists(Environment.CurrentDirectory + "/temp.tmp"))
            {
                try
                {
                    File.Delete(Environment.CurrentDirectory + "/temp.tmp");
                }
                catch (Exception ex) { AprilSpeak(ex.Message); }
            }
        }

        private static string[] AddHiddenVariables()
        {
            string[] str = new string[6] { "[DllImport(\"kernel32.dll\")]", "static extern IntPtr GetConsoleWindow();", "[DllImport(\"user32.dll\")]", "static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);", "const int SW_HIDE = 0;", "const int SW_SHOW = 5;"};

            return str;
        }

        private static string[] HideConsole()
        {
            string[] str = new string[2] { "var handle = GetConsoleWindow();", "ShowWindow(handle, SW_HIDE);" };
            return str;
        }

        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false, true); // encoding used in File.ReadAll*()
        private static object _bufferSizeLock = new Object();
        private static int _bufferSize = 1024 * 1024; // 1mb
        public static int BufferSize
        {
            get
            {
                lock (_bufferSizeLock)
                {
                    return _bufferSize;
                }
            }
            set
            {
                lock (_bufferSizeLock)
                {
                    _bufferSize = value;
                }
            }
        }

        public static void PrependAllLines(string path, IEnumerable<string> contents)
        {
            PrependAllLines(path, contents, _defaultEncoding);
        }

        public static void PrependAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            var temp = Path.GetTempFileName();
            File.WriteAllLines(temp, contents, encoding);
            AppendToTemp(path, temp, encoding);
            File.Replace(temp, path, null);
        }

        public static void PrependAllText(string path, string contents)
        {
            PrependAllText(path, contents, _defaultEncoding);
        }

        public static void PrependAllText(string path, string contents, Encoding encoding)
        {
            var temp = Path.GetTempFileName();
            File.WriteAllText(temp, contents, encoding);
            AppendToTemp(path, temp, encoding);
            File.Replace(temp, path, null);
        }

        private static void AppendToTemp(string path, string temp, Encoding encoding)
        {
            var bufferSize = BufferSize;
            char[] buffer = new char[bufferSize];

            using (var writer = new StreamWriter(temp, true, encoding))
            {
                using (var reader = new StreamReader(path, encoding))
                {
                    int bytesRead;
                    while ((bytesRead = reader.ReadBlock(buffer, 0, bufferSize)) != 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
    }
}
