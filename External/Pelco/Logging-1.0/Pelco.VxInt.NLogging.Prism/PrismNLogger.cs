using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Logging;

namespace Pelco.VxInt.Logging
{
    public class PrismNLogger : ILoggerFacade
    {
        private INLogger Logger { get; set; }

        public PrismNLogger(INLogger logger)
        {
            Logger = logger;
        }
        
        public void Log(string message, Category category, Priority priority)
        {
            switch (category)
            {
                case Category.Debug:
                    Logger.Debug(message, "--", "Unknown", 999);
                    break;
                case Category.Info:
                    Logger.Info(message, "--", "Unknown", 999);
                    break;
                case Category.Warn:
                    Logger.Warn(message, "--", "Unknown", 999);
                    break;
                case Category.Exception:
                    Logger.Error(message, null, "--", "Unknown", 999);
                    break;
            }
        }
    }
}
