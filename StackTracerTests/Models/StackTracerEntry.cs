namespace StackTracerTests.Models
{
    public class StackTracerEntry
    {
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public Stack[] Stacks { get; set; }
        public string SiteName { get; set; }
        public bool IsDotNetApp { get; set; }
    }
}