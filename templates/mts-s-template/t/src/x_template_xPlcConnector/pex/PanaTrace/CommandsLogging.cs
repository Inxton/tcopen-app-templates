using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanaTrace
{
    public class CommandsLogging
    {

        private CommandsLogging()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.RollingFile( $"Logs\\panatrace.log",
                                                     retainedFileCountLimit: 7,
                                                     fileSizeLimitBytes: null,
                                                     flushToDiskInterval: new TimeSpan(0, 1, 0))
                                                     .MinimumLevel.Information()
                                                     .CreateLogger();
        }

        private static volatile object mutex = new object();
        private static CommandsLogging _instance;

        public static void InitliazeLogger()
        {
            
        }

        public static CommandsLogging Get
        {
            get
            {
                lock(mutex)
                {
                    if(_instance == null)
                    {
                        _instance = new CommandsLogging();
                    }
                }

                return _instance;
            }
        }

        public ILogger Logger { get { return Log.Logger; } }
    }
}
