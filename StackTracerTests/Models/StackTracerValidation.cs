namespace StackTracerTests.Controllers
{
    public class StackTracerValidation
    {
        public  bool IsSuccess { get; set; }
        public bool IsProcessIdMatching { get; set; }
        public bool IsProcessNameMatching{ get; set; }

        public bool IsStackTraceMatching { get; set; }
    }
}