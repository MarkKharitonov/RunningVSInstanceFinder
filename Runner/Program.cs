using Mono.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Runner
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var help = false;
            string findArg = null;
            IList<string> findManyArg = null;
            var options = new OptionSet
            {
                { "h|?|help", "Print this help screen.", _ => help = true },
                { "find=", "Find the VS instance with the given solution and run build.", v => findArg = v },
                { "findMany=", "Find the VS instances with the given solutions (comma delimited) and run build.", v => findManyArg = v.Split(',') },
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
                return Build(res);
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
                    exitCode += Build(o);
                }
                return exitCode;
            }
        }

        private static int Build(Ceridian.RunningVSInstanceFinder.FindResult o)
        {
            if (o.DTE == null)
            {
                Console.Error.WriteLine(o.ErrorMessage);
            }
            else
            {
                try
                {
                    o.DTE.Solution.SolutionBuild.Build(true);
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }
        
            return 1;
        }
    }
}