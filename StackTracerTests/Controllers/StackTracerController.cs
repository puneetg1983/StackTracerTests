using Newtonsoft.Json;
using StackTracerTests.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace StackTracerTests.Controllers
{
    public class StackTracerOutput
    {
        public bool IsSuccess { get; set; }
        public string StackTrace { get; set; }
    }

    public class StackTracerController : ApiController
    {
        [HttpGet]
        [ActionName("invoke")]
        public StackTracerInvocationOutput Get()
        {
            string fileName = Environment.ExpandEnvironmentVariables($"%TEMP%\\stacktracer_{DateTime.UtcNow.Ticks}.txt");
            return LaunchProcess(Process.GetCurrentProcess().Id, fileName);
        }

        [HttpGet]
        [ActionName("validate")]
        public StackTracerValidation Validate()
        {
            var validationOutput = new StackTracerValidation();
            StackTracerInvocationOutput output = this.GetStackTracerOutput();
            if (!output.IsSuccess)
            {
                return validationOutput;
            }

            validationOutput.IsSuccess = true;

            StackTracerEntry stackTrace = output.StackTrace.FirstOrDefault();
            if (stackTrace == null)
            {
                return validationOutput;
            }

            if (stackTrace.ProcessId != Process.GetCurrentProcess().Id)
            {
                return validationOutput;
            }

            validationOutput.IsProcessIdMatching = true;
            if (stackTrace.ProcessName != Process.GetCurrentProcess().ProcessName)
            {
                return validationOutput;
            }

            validationOutput.IsProcessNameMatching = true;


            foreach (var stack in stackTrace.Stacks)
            {
                if (stack.CallStack.Any(stackFrame => stackFrame.StartsWith("System.Threading.Thread.Sleep")) 
                    && stack.CallStack.Any(stackFrame => stackFrame.StartsWith("StackTracerTests.Controllers.SleepController.DoSleep")))
                {
                    validationOutput.IsStackTraceMatching = true;
                    break;
                }
            }

            return validationOutput;
        }

        private StackTracerInvocationOutput GetStackTracerOutput()
        {
            DateTime utcNow = DateTime.UtcNow;
            string fileName = Environment.ExpandEnvironmentVariables(string.Format("%TEMP%\\stacktracer_{0}.txt", utcNow.Ticks));
            return this.LaunchProcess(Process.GetCurrentProcess().Id, fileName);
        }

        private StackTracerInvocationOutput LaunchProcess(int processId, string outputFile)
        {
            string stackTracerPath = Environment.ExpandEnvironmentVariables("%ProgramFiles%\\IIS\\Microsoft Web Hosting Framework\\DWASMod\\stacktracer\\stacktracer.exe");
            string args = string.Format("-p:{0} -o:{1}", processId, outputFile);
            StackTracerInvocationOutput output = new StackTracerInvocationOutput();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Concat("Launching: ", stackTracerPath, " ", args));
            Process stackTracerProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = stackTracerPath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };
            stackTracerProcess.OutputDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs e) => stringBuilder.Append(e.Data));
            Stopwatch watch = new Stopwatch();
            watch.Start();
            stackTracerProcess.Start();
            stackTracerProcess.BeginOutputReadLine();
            stackTracerProcess.WaitForExit();
            stackTracerProcess.CancelOutputRead();
            watch.Stop();
            StackTracerEntry[] stackTraces = JsonConvert.DeserializeObject<StackTracerEntry[]>(File.ReadAllText(outputFile));
            if (!File.Exists(outputFile))
            {
                output.IsSuccess = false;
                output.Output = stringBuilder.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                output.ElapsedMilliseconds = watch.ElapsedMilliseconds;
            }
            else
            {
                output.IsSuccess = true;
                output.StackTrace = stackTraces;
                output.Output = stringBuilder.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                output.ElapsedMilliseconds = watch.ElapsedMilliseconds;
            }
            return output;
        }

    }
}