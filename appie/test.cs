using appie.FORM;
using Salar.Bois;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace appie
{
    public static class test
    {
        public static void f_bookmark_import() {
            string s = File.ReadAllText("bookmarks.html");
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(s);

            List<oLink> list = new List<oLink>();
            string tag = string.Empty;
            var nodes = doc.DocumentNode.QuerySelectorAll("h3"); 
            foreach (var node in nodes) {
                if (node.ParentNode.QuerySelectorAll("h3").Count == 1) {
                    tag = node.ParentNode.QuerySelector("h3").InnerText; 
                    var a = node.ParentNode.QuerySelectorAll("a").Select(x => new oLink () { Tags = tag, Title = x.InnerText, Link = x.getAttribute("href") }).ToArray();
                    list.AddRange(a);
                }
            }

            var remains = doc.DocumentNode.QuerySelectorAll("a").Select(x => new oLink() { Tags = tag, Title = x.InnerText, Link = x.getAttribute("href") }).ToArray();
            list.AddRange(remains);
            list = list.GroupBy(x => x.Link).Select(x => x.First()).ToList();

            oLinkTag[] tags = list.GroupBy(x => x.Tags).Select(x => new oLinkTag() { Count = x.Count(), Tag = x.Key }).ToArray();

            using (var file = new FileStream("lin.dat", FileMode.OpenOrCreate))
            { 
                new BoisSerializer().Serialize<oLink[]>(list.ToArray(), file);
                file.Close();
            }

        }

        public static void f_MediaMP3Stream_Demo() { 
            Application.EnableVisualStyles();
            Application.Run(new fMediaMP3Stream_Demo());
        }

        public static void f_jobTest()
        {
            var jobs = new JobStore();

            jobs.f_addJob(new JobTest(jobs));
            jobs.f_addJob(new JobTest(jobs));
            jobs.f_addJob(new JobTest(jobs));
            jobs.f_addJob(new JobTest(jobs));
            jobs.f_addJob(new JobTest(jobs));

            jobs.OnStopAll += (se, ev) => {
                Tracer.WriteLine(">>>>> STOP ALL JOBS: DONE ...");
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

            jobs.f_addJob(new JobWebClient(jobs));
            jobs.f_addJob(new JobWebClient(jobs));
            jobs.f_addJob(new JobWebClient(jobs));
            jobs.f_addJob(new JobWebClient(jobs));
            jobs.f_addJob(new JobWebClient(jobs));

            jobs.OnStopAll += (se, ev) => {
                Tracer.WriteLine(">>>>> STOP ALL JOBS: DONE ...");
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

            int id = jobs.f_addJob(new JobSpeechEN(jobs)); 

            jobs.OnStopAll += (se, ev) => {
                Tracer.WriteLine(">>>>> STOP ALL JOBS: DONE ...");
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
                //jobs.f_job_sendMessage(id, input);
                
            }
        }

        public static void f_JobGooTranslate()
        {
            var jobs = new JobStore();

            int id = jobs.f_addJob(new JobGooTranslate(jobs)); 

            jobs.OnStopAll += (se, ev) => {
                Tracer.WriteLine(">>>>> STOP ALL JOBS: DONE ...");
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
                //jobs.f_job_sendMessage(id, input);
                
            }
        }

        public static void f_JobWord()
        {
            var jobs = new JobStore();

            int id = jobs.f_addJob(new JobWord(jobs)); 

            jobs.OnStopAll += (se, ev) => {
                Tracer.WriteLine(">>>>> STOP ALL JOBS: DONE ...");
            };

            while (true)
            {
                //Console.WriteLine("Enter to stop all...");
                //Console.ReadLine();
                //jobs.f_stopAll();
                //Console.WriteLine("Enter to restart all...");
                //Console.ReadLine();
                //jobs.f_restartAllJob();
                Console.Write("Enter word to dictionary: ");
                string input = Console.ReadLine();
                //jobs.f_job_sendMessage(id, input);
                
            }
        }


    }
}
