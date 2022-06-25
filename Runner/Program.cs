using Mono.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Runner
{
    internal class Program
    {
        [Flags]
        enum AutomationAction
        {
            None,
            GetBuildOutput = 0x1,
            GetErrorList = 0x2
        }

        static int Main(string[] args)
        {
            var help = false;
            string findArg = null;
            IList<string> findManyArg = null;
            var action = AutomationAction.None;
            var options = new OptionSet
            {
                { "h|?|help", "Print this help screen.", _ => help = true },
                { "find=", "Find the VS instance with the given solution and run build.", v => findArg = v },
                { "findMany=", "Find the VS instances with the given solutions (comma delimited) and run build.", v => findManyArg = v.Split(',') },
                { "getBuildOutput", "Get the Build Output for the found instances", _ => action |= AutomationAction.GetBuildOutput },
                { "getErrorList", "Get the Error List for the found instances", _ => action |= AutomationAction.GetErrorList },
            };

            var extra = options.Parse(args);
            if (help || args.Length == 0)
            {
                options.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            if (extra.Count > 0)
            {
                Console.Error.WriteLine("Unknown aguments " + string.Join(" ", extra));
                options.WriteOptionDescriptions(Console.Out);
                return -1;
            }

            if (findArg == null == (findManyArg == null))
            {
                Console.Error.WriteLine("Either --find or --findMany must be given");
                options.WriteOptionDescriptions(Console.Out);
                return -2;
            }

            if (findArg != null)
            {
                var res = Ceridian.RunningVSInstanceFinder.Find(findArg);
                return DoAction(res, action);
            }
            {
                var res = Ceridian.RunningVSInstanceFinder.FindMany(findManyArg);
                if (res.Found.Count == 0)
                {
                    return -3;
                }
                int exitCode = 0;
                foreach (var o in res.Found.Values.SelectMany(o => o))
                {
                    exitCode += DoAction(o, action);
                }
                return exitCode;
            }
        }

        private static int DoAction(Ceridian.RunningVSInstanceFinder.FindResult o, AutomationAction action)
        {
            if (o.DTE == null)
            {
                Console.Error.WriteLine(o.ErrorMessage);
            }
            else
            {
                try
                {
                    if ((action & AutomationAction.GetErrorList) != 0)
                    {
                        Console.WriteLine(Ceridian.RunningVSInstanceFinder.GetErrorList(o.DTE).LastOrDefault());
                    }
                    if ((action & AutomationAction.GetBuildOutput) != 0)
                    {
                        Console.WriteLine(Ceridian.RunningVSInstanceFinder.GetOutputPaneText(o.DTE, "Build")
                            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                            .LastOrDefault());
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }

            return 1;
        }
    }
}