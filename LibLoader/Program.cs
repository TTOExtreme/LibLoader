using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LibLoader
{
    class Program
    {
        private static string pathDll = System.IO.Directory.GetCurrentDirectory() + "\\libs\\";
        private static string path = System.IO.Directory.GetCurrentDirectory();
        private static List<DllsCaller> DllsFunc = new List<DllsCaller>();

        static void Main(string[] args)
        {
            Loop();
        }

        private static void Loop() { 
            while (true)
            {
                String command = Console.ReadLine();
                if (command == "exit") { break; }
                if (command == "cls") { System.Console.Clear(); }

                if (command == "load") { LoadLibs(); }
                if (command == "reload") { ReloadApp(); break; }
                if (command.IndexOf("exec") == 0) { Exec(command.Split(' ')); }
            }
        }

        private static void ReloadApp()
        {
            var fileName = System.Reflection.Assembly.GetEntryAssembly().Location;
            System.Diagnostics.Process.Start(fileName);
        }

        private static void Exec(string[] args) {
            foreach(DllsCaller dc in DllsFunc)
            {
                if (dc.DllName == args[1] && dc.DllMethod == args[2])
                {
                    object[] obj = new object[args.LongCount() - 3];
                    for (int i = 0; i < args.LongCount(); i++)
                    {
                        if (i > 2) { obj[i - 3] = args[i]; }
                    }
                    Console.WriteLine(dc.call(obj));
                    return;
                }
            }
            Console.WriteLine("wrong command");
        }
        
        private static void LoadLibs() {
            if (!Directory.Exists(pathDll)) { Directory.CreateDirectory(pathDll); Console.WriteLine("Directory dont exists \r\nCreating Directory"); LoadLibs(); }
            string[] dllFiles = Directory.GetFiles(pathDll);
            Console.WriteLine("Found: " + dllFiles.LongCount() + " Lib(s) instaled");
            foreach (string str in dllFiles)
            {
                if (str.IndexOf(".dll") > -1 && str.IndexOf("_sublib")==-1)
                {
                    Console.WriteLine("Loading: " + str.Substring(str.LastIndexOf("\\") + 1));
                    string str2 = str.Substring(str.LastIndexOf("\\") + 1, (str.LastIndexOf(".")) - (str.LastIndexOf("\\") + 1));
                    
                    var Dll = Assembly.LoadFile(str);
                    var theType = Dll.GetType(str2 + ".Main_Class");
                    var c = Activator.CreateInstance(theType);
                    var methods = theType.GetMethods();
                    //var method = theType.GetMethod("Commands");
                    //string str3 = method.Invoke(c, new object[] { }).ToString();
                    Console.WriteLine("\r\nMethods: ");
                    foreach (MethodInfo str1 in methods)
                    {
                        if (str1.Name.IndexOf("ToString") > -1) { break; }
                        Console.WriteLine(str.Substring(str.LastIndexOf("\\") + 1, str.LastIndexOf(".") - 2 - str.LastIndexOf("\\") + 1) + " " + str1.Name);

                        DllsCaller dc = new DllsCaller();
                        dc.init(str, str1.Name, false);
                        DllsFunc.Add(dc);

                        if (str1.Name == "SetPath") { Exec(new string[] { "", dc.DllName, dc.DllMethod, path }); }
                    }
                }
            }
        }

        private class DllsCaller{
            //public
            public string DllName;
            public string DllMethod;

            //private
            private string DllPath;
            private bool ReturnConsole;
            private object constructor;
            private string ReturnType;
            private MethodInfo DllMethodInfo;
            private Type TheType;

            public void init(string dllPath, string dllMethod, bool returnsInConsole)
            {
                DllPath = dllPath;
                DllMethod = dllMethod;
                ReturnConsole = returnsInConsole;
                DllName = DllPath.Substring(DllPath.LastIndexOf("\\") + 1, (DllPath.LastIndexOf(".")) - (DllPath.LastIndexOf("\\") + 1));
                
                var Dll = Assembly.LoadFile(DllPath);
                TheType = Dll.GetType(DllName + ".Main_Class");
                constructor = Activator.CreateInstance(TheType);
                DllMethodInfo = TheType.GetMethod(DllMethod);

                ReturnType = DllMethodInfo.ReturnType.Name;

                //string str2 = method.Invoke(c, new object[] { }).ToString();

            }
            
            public string call(object[] args)
            {
                try
                {
                    var Dll = Assembly.LoadFile(DllPath);
                    TheType = Dll.GetType(DllName + ".Main_Class");
                    constructor = Activator.CreateInstance(TheType);
                    DllMethodInfo = TheType.GetMethod(DllMethod);
                    string str = DllMethodInfo.Invoke(constructor, args).ToString();
                    return str;
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return "";
            }


        }
    }
}
