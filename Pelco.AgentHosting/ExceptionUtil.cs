// Copyright (c) 2013 Ivan Krivyakov

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pelco.AgentHosting
{
    public class ExceptionUtil
    {
        public static string GetUserMessage(Exception ex)
        {
            return string.Join("\r\n",
                GetInnerExceptions(ex).Select(
                    e => string.Format("{0}: {1}", e.GetType().Name, e.Message)).ToArray());
        }

        private static IEnumerable<Exception> GetInnerExceptions(Exception ex)
        {
            for (var exception = ex; exception != null; exception = exception.InnerException)
                yield return exception;
        }
    }
}
