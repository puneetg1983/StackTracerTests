namespace StackTracerTests.Models
{
    public class StackTracerInvocationOutput
    {
        public bool IsSuccess { get; set; }
        public StackTracerEntry StackTrace { get; set; }
        public string[] Output { get; internal set; }
        public long ElapsedMilliseconds { get; internal set; }
    }
}