using System.Threading;
using System.Web.Http;

namespace StackTracerTests.Controllers
{
    public class SleepController : ApiController
    {

        [HttpGet]
        [ActionName("invoke")]
        public string Invoke()
        {
            for (int i = 0; i < 5; i++)
            {
                DoSleepOnThread();
            }

            return "Threads Started";
        }

        private void DoSleepOnThread()
        {
            Thread t = new Thread(new ThreadStart(DoSleep));
            t.Start();
        }

        private void DoSleep()
        {
            Thread.Sleep(10000);
        }
    }
}
