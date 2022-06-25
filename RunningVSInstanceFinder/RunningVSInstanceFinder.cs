using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace Ceridian
{
    public static partial class RunningVSInstanceFinder
    {
        public static FindResult Find(string solutionFilePath)
        {
            var messages = new HashSet<string>();
            foreach (var item in EnumerateEnvDTE())
            {
                if (item.ErrorMessage != null)
                {
                    messages.Add(item.ErrorMessage);
                }
                else if (string.Equals(solutionFilePath, item.SolutionFullName, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
            return new FindResult(string.Join(Environment.NewLine, messages));
        }

        public static FindManyResult FindMany(IList<string> solutionFilePaths)
        {
            if (solutionFilePaths == null || solutionFilePaths.Count == 0)
            {
                return null;
            }

            var res = new Dictionary<string, List<FindResult>>();
            var messages = new HashSet<string>();
            foreach (var item in EnumerateEnvDTE())
            {
                if (item.ErrorMessage != null)
                {
                    messages.Add(item.ErrorMessage);
                }
                else if (item.SolutionFullName != null && solutionFilePaths.Any(s => string.Equals(s, item.SolutionFullName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!res.TryGetValue(item.SolutionFullName, out var instances))
                    {
                        res[item.SolutionFullName] = instances = new List<FindResult>();
                    }
                    instances.Add(item);
                }
            }

            return new FindManyResult(res, messages.ToList());
        }

        public static ErrorItem[] GetErrorList(object dteObject)
        {
            var dte = (DTE2)dteObject;
            if (dte != null)
            {
                RunActionWithRetries(() => dte.Application.ExecuteCommand("View.ErrorList", " "));
                var errors = RunActionWithRetries(() => dte.ToolWindows.ErrorList.ErrorItems);
                if (errors.Count > 0)
                {
                    var res = new ErrorItem[errors.Count];
                    for (var i = 1; i <= errors.Count; ++i)
                    {
                        var o = errors.Item(i);
                        res[i - 1] = new ErrorItem((BuildErrorLevel)o.ErrorLevel, o.Description, o.FileName, o.Line, o.Column, o.Project);
                    }
                    return res;
                }
            }
            return Array.Empty<ErrorItem>();
        }

        public static string GetOutputPaneText(object dteObject, string toolName)
        {
            var dte = (DTE2)dteObject;
            if (dte != null)
            {
                RunActionWithRetries(() => dte.Application.ExecuteCommand("View.Output", " "));
                var outputWindow = RunActionWithRetries(() => dte.ToolWindows.OutputWindow);
                var outputWindowPanes = RunActionWithRetries(() => outputWindow.OutputWindowPanes);
                for (var i = 1; i <= outputWindowPanes.Count; ++i)
                {
                    var curOutputWindowPane = outputWindowPanes.Item(i);
                    if (curOutputWindowPane.Name == toolName)
                    {
                        RunActionWithRetries(() => curOutputWindowPane.Activate());
                        var sel = RunActionWithRetries(() => curOutputWindowPane.TextDocument.Selection);
                        RunActionWithRetries(() => sel.StartOfDocument(false));
                        RunActionWithRetries(() => sel.EndOfDocument(true));
                        return RunActionWithRetries(() => sel.Text);
                    }
                }
            }
            return null;
        }

        private static void RunActionWithRetries(Action action, int retryCount = 20, int delayMS = 100)
        {
            RunActionWithRetries<object>(() =>
            {
                action();
                return null;
            }, retryCount, delayMS);
        }

        private static T RunActionWithRetries<T>(Func<T> action, int retryCount = 5, int delayMS = 100)
        {
            for (; ; )
            {
                try
                {
                    return action();
                }
                catch
                {
                    if (retryCount == 0)
                    {
                        throw;
                    }

                    --retryCount;
                    Thread.Sleep(delayMS);
                }
            }
        }

        private static IEnumerable<FindResult> EnumerateEnvDTE()
        {
            IntPtr pceltFetched = new IntPtr();
            IMoniker[] rgelt = new IMoniker[1];
            GetRunningObjectTable(0, out IRunningObjectTable table);
            table.EnumRunning(out IEnumMoniker moniker);
            moniker.Reset();
            var visited = new HashSet<object>();
            while (moniker.Next(1, rgelt, pceltFetched) == 0)
            {
                CreateBindCtx(0, out _);
                table.GetObject(rgelt[0], out object obj2);
                if (!visited.Add(obj2))
                {
                    continue;
                }
                var dte = obj2 as DTE2;
                FindResult item;
                try
                {
                    if (dte?.Name != "Microsoft Visual Studio")
                    {
                        continue;
                    }

                    item = new FindResult(dte, dte.Solution?.FullName);
                }
                catch (Exception exception)
                {
                    item = new FindResult(exception.Message);
                }
                yield return item;
            }
        }

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);
    }
}