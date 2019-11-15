using EnvDTE80;

namespace Ceridian
{
    public static partial class RunningVSInstanceFinder
    {
        public class FindResult
        {
            public readonly DTE2 DTE;
            public readonly string SolutionFullName;
            public readonly string ErrorMessage;

            public FindResult(DTE2 dTE, string solutionFullName)
            {
                DTE = dTE;
                SolutionFullName = solutionFullName;
            }

            public FindResult(string errorMessage)
            {
                ErrorMessage = errorMessage;
            }
        }
    }
}