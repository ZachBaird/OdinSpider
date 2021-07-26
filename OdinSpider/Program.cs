using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OdinSpider.Extensions;
using OdinSpider.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OdinSpider
{
    class Program
    {
        private const string _rootUrl = "https://www.theodinproject.com/paths";

        /// <summary>
        /// This method collects all the lesson hrefs within a course.
        /// </summary>
        /// <param name="context">The browsing context.</param>
        /// <param name="course">The current course to collect lessons from.</param>
        /// <param name="ignoreList">The list of URIs we are ignoring.</param>
        /// <param name="completedParses">The list of URIs we've already parsed.</param>
        /// <returns>A list of <see cref="Link"/> models.</returns>
        static async Task<List<Link>> CollectLessons(
            IBrowsingContext context, 
            string course, 
            List<string> ignoreList, 
            List<Link> completedParses)
        {
            List<Link> result = new();
            Console.WriteLine($"Parsing {course}");

            var document = await context.OpenAsync(_rootUrl + course);

            var links = document.QuerySelectorAll<IHtmlAnchorElement>("a");

            foreach (var link in links)
            {
                // Don't need these.
                if (link.Href.Contains("#") || link.Href == $"{_rootUrl}{course}")
                    continue;

                if (LinkExtensions.CanAdd(link, course, ignoreList, completedParses))
                {
                    Link model = new()
                    {
                        LinkText = link.TextContent,
                        Href = link.Href
                    };

                    result.Add(model);
                }
            }

            return result;
        }

        /// <summary>
        /// This method, rather identical to <see cref="CollectLessons(IBrowsingContext, string, List{string}, List{Link})"/>, collects all links from a lesson.
        /// <para>
        /// Collecting the links is kept separate incase we need extra logic specifically for either case down the road.
        /// </para>
        /// </summary>
        /// <param name="context">The browsing context.</param>
        /// <param name="lesson">The lesson <see cref="Link"/> model.</param>
        /// <param name="ignoreList">The list of URIs to ignore.</param>
        /// <param name="completedParses">The list of URIs we've already parsed.</param>
        /// <returns>A collection of <see cref="Link"/> for each link in a lesson.</returns>
        static async Task<List<Link>> CollectLinksInLesson(IBrowsingContext context, string course, Link lesson, List<string> ignoreList, List<Link> completedParses)
        {
            List<Link> result = new();
            Console.WriteLine($"Collecting links from {lesson.Href}");

            var document = await context.OpenAsync(lesson.Href);

            var links = document.QuerySelectorAll<IHtmlAnchorElement>("a");

            foreach (var link in links)
            {
                // Don't need these.
                if (link.Href.Contains("#") 
                    || link.Href == $"{_rootUrl}{course}" 
                    || link.Href.Contains("thinkful") 
                    || link.Href == document.BaseUri 
                    || link.TextContent == "Next Lesson" 
                    || link.TextContent.Contains("Improve this lesson"))
                    continue;

                var textContent = link.TextContent;
                var href = link.Href;

                if (LinkExtensions.CanAdd(link, lesson.Href, ignoreList, completedParses))
                {
                    Link model = new()
                    {
                        LinkText = link.TextContent,
                        Href = link.Href
                    };

                    result.Add(model);
                }
            }

            return result;
        }

        /// <summary>
        /// Tests a link by making a web request.
        /// </summary>
        /// <param name="link">The current link to test.</param>
        /// <param name="completedParses">Our list of completed parses.</param>
        /// <returns>True if the test is good or already has been done, false if it fails our test.</returns>
        static bool TestLink(Link link, List<Link> completedParses)
        {
            // If the link is in our completed list, just return true;
            if (completedParses.Select(cp => cp.Href).Contains(link.Href))
                return true;

            // Prepare the test request.
            var request = WebRequest.Create(link.Href);
            Console.WriteLine($"Hitting {link.Href}");

            try
            {
                // If the response we get back is not a 200 or 302 (this list may expand later), then this may be a broken link.
                var response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.OK
                    && response.StatusCode != HttpStatusCode.Redirect)
                {
                    Console.WriteLine($"Found potential broken link: {link.Href}");
                    return false;
                }

                return true;
            }
            catch (WebException webex)
            {
                Console.WriteLine("Something went wrong hitting the link");
                Console.WriteLine(webex.Message);
                return false;
            }
        }

        static async Task Main(string[] args)
        {
            List<string> ignoreList = new()
            {
                "https://discord.gg/fbFCkYabZB",
                "https://www.theodinproject.com/terms_of_use",
                "https://www.theodinproject.com/contributing",
                "https://www.theodinproject.com/success_stories",
                "https://medium.com/the-odin-project",
                "https://www.theodinproject.com/faq",
                "https://www.theodinproject.com/about",
                "https://twitter.com/TheOdinProject",
                "https://www.facebook.com/theodinproject/",
                "https://discord.gg/V75WSQG",
                "https://github.com/TheOdinProject",
                "https://www.theodinproject.com/home",
                "https://www.theodinproject.com/paths",
                "https://www.theodinproject.com/sign_up",
                "https://www.theodinproject.com/login"
            };

            List<string> courses = new()
            { 
               "/foundations/courses/foundations", 
               "/full-stack-ruby-on-rails/courses/ruby-programming", 
               "/full-stack-ruby-on-rails/courses/javascript",
               "/full-stack-ruby-on-rails/courses/databases",
               "/full-stack-ruby-on-rails/courses/ruby-on-rails",
               "/full-stack-ruby-on-rails/courses/html-and-css",
               "/full-stack-ruby-on-rails/courses/getting-hired",
               "/full-stack-javascript/courses/nodejs"
            };

            List<Link> completedParses = new();
            List<Link> brokenLinks = new();

            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());

            try
            {
                // Iterate through the courses.
                foreach (var course in courses)
                {
                    // Collect the lessons in the course.
                    var lessons = await CollectLessons(context, course, ignoreList, completedParses);

                    foreach (var lesson in lessons)
                    {
                        // Collect all the links in a lesson.
                        var links = await CollectLinksInLesson(context, course, lesson, ignoreList, completedParses);

                        foreach (var link in links)
                        {
                            var testResult = TestLink(link, completedParses);

                            if (!testResult)
                                brokenLinks.Add(link);

                            completedParses.Add(link);
                        }
                    }
                }

                Console.WriteLine("Tests Complete");

                if (brokenLinks.Count == 0)
                    Console.WriteLine("No broken links!");
                else
                {
                    Console.WriteLine("Investigate the following potential broken links:");
                    foreach (var link in brokenLinks)
                        Console.WriteLine($"{link.LinkText} - {link.Href}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong during execution");
                Console.WriteLine(ex.Message);
            }

            
        }
    }
}
