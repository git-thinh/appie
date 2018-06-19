using appie.FORM;
using System;
using System.Windows.Forms;

namespace appie
{
    public static class test
    {
        public static void f_MediaMP3Stream_Demo() { 
            Application.EnableVisualStyles();
            Application.Run(new fMediaMP3Stream_Demo());
        }

        public static void f_jobTest()
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

        public static void f_jobWebClient() {
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

        public static void f_jobSpeechEN()
        {
            var jobs = new JobStore();

            int id = jobs.f_addJob(new JobSpeechEN(jobs), "a"); 

            jobs.OnStopAll += (se, ev) => {
                Trace.WriteLine(">>>>> STOP ALL JOBS: DONE ...");
            };

            while (true)
            {
                //Console.WriteLine("Enter to stop all...");
                //Console.ReadLine();
                //jobs.f_stopAll();
                //Console.WriteLine("Enter to restart all...");
                //Console.ReadLine();
                //jobs.f_restartAllJob();
                Console.Write("Enter to speech: ");
                string input = Console.ReadLine();
                jobs.f_job_postData(id, input);
                
            }
        }

        public static void f_JobGooTranslate()
        {
            var jobs = new JobStore();

            int id = jobs.f_addJob(new JobGooTranslate(jobs), "a"); 

            jobs.OnStopAll += (se, ev) => {
                Trace.WriteLine(">>>>> STOP ALL JOBS: DONE ...");
            };

            while (true)
            {
                //Console.WriteLine("Enter to stop all...");
                //Console.ReadLine();
                //jobs.f_stopAll();
                //Console.WriteLine("Enter to restart all...");
                //Console.ReadLine();
                //jobs.f_restartAllJob();
                Console.Write("Enter to translate: ");
                string input = Console.ReadLine();
                jobs.f_job_postData(id, input);
                
            }
        }


    }
}
