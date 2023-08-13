using System;
using System.IO;
using System.Linq;
using System.Windows;
using Npgsql;

namespace HealthCenter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string? HostArg;
        public string? PortArg;
        public string? DatabaseArg;
        public string? UsernameArg;
        public string? PasswordArg;
        public string? SchemaArg;

        protected override void OnStartup(StartupEventArgs e)
        {
            string[] args = e.Args;
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith("--"))
                {
                    continue;
                }

                string key = args[i].Substring(2);
                string value = args[i + 1].Trim('"');
                switch (key)
                {
                    case "ArgFile":
                        args = File.ReadLines(value)
                            .SelectMany(line =>
                            {
                                return line
                                    .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                    .Select(part => part.Trim('"'));
                            })
                            .ToArray();
                        i = -1;
                        break;

                    case "Host":
                        HostArg = value;
                        break;

                    case "Port":
                        PortArg = value;
                        break;

                    case "Database":
                        DatabaseArg = value;
                        break;

                    case "Username":
                        UsernameArg = value;
                        break;

                    case "Password":
                        PasswordArg = value;
                        break;

                    case "Schema":
                        SchemaArg = value;
                        break;
                }
            }

            base.OnStartup(e);
        }
    }
}
