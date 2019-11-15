using System.Collections.Generic;

namespace Ceridian
{
    public static partial class RunningVSInstanceFinder
    {
        public class FindManyResult
        {
            public readonly Dictionary<string, List<FindResult>> Found;
            public readonly List<string> ErrorMessages;

            public FindManyResult(Dictionary<string, List<FindResult>> found, List<string> errorMessages)
            {
                Found = found;
                ErrorMessages = errorMessages;
            }
        }
    }
}