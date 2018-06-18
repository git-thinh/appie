using System;
using System.Collections.Generic;
using System.Text;

namespace appie
{
    public static class test
    {
        public static void job_JobWebClient() {
            var jobs = new JobStore();

            jobs.f_addJob(new JobWebClient(jobs), "a");
            jobs.f_addJob(new JobWebClient(jobs), "a");
            jobs.f_addJob(new JobWebClient(jobs), "a");
            jobs.f_addJob(new JobWebClient(jobs), "a");
            jobs.f_addJob(new JobWebClient(jobs), "a");

            jobs.OnStopAll += (se, ev) => {
                Trace.WriteLine(">>>>> STOP ALL JOBS: DONE ...");
            };

            while (true)
            {
                Console.WriteLine("Input URL: ");
                string url = Console.ReadLine();
                jobs.f_url_AddRange(new string[] { "https://dictionary.cambridge.org/grammar/british-grammar/" });

                //Console.WriteLine("Enter to stop all...");
                //Console.ReadLine();
                //jobs.f_stopAll(); 
                //Console.WriteLine("Enter to restart all...");
                //Console.ReadLine();
                //jobs.f_restartAllJob(); 
            }
        }

        public static void job_Test()
        {
            var jobs = new JobStore();

            jobs.f_addJob(new JobTest(jobs), "a");
            jobs.f_addJob(new JobTest(jobs), "a");
            jobs.f_addJob(new JobTest(jobs), "a");
            jobs.f_addJob(new JobTest(jobs), "a");
            jobs.f_addJob(new JobTest(jobs), "a");

            jobs.OnStopAll += (se, ev) => {
                Trace.WriteLine(">>>>> STOP ALL JOBS: DONE ...");
            };

            while (true)
            {
                Console.WriteLine("Enter to stop all...");
                Console.ReadLine();
                jobs.f_stopAll();
                Console.WriteLine("Enter to restart all...");
                Console.ReadLine();
                jobs.f_restartAllJob();
            }
        }


    }
}
