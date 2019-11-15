namespace Ceridian
{
    public static partial class RunningVSInstanceFinder
    {
        public class ErrorItem
        {
            public ErrorItem(BuildErrorLevel errorLevel, string description, string fileName, int line, int column, string project)
            {
                ErrorLevel = errorLevel;
                Description = description;
                FileName = fileName;
                Line = line;
                Column = column;
                Project = project;
            }

            public BuildErrorLevel ErrorLevel { get; set; }
            public string Description { get; set; }
            public string FileName { get; set; }
            public int Line { get; set; }
            public int Column { get; set; }
            public string Project { get; set; }
        }
    }
}