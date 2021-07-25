using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OdinSpider.Extensions;
using OdinSpider.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OdinSpider
{
    class Program
    {
        private const string _rootUrl = "https://theodinproject.com/paths";

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
                "https://www.theodinproject.com/home"
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
                foreach (var course in courses)
                {
                    // Prepare to parse the course. Open the browsing context.
                    Console.WriteLine($"Parsing for {course}");
                    var document = await context.OpenAsync(_rootUrl + course);

                    // Prepare to collect the lesson links within the course.
                    var links = document.QuerySelectorAll<IElement>("a");
                    List<Link> courseLinks = new();

                    foreach (IHtmlAnchorElement link in links)
                    {
                        if (LinkExtensions.CanAdd(link, course, ignoreList, completedParses))
                        {
                            var linkModel = new Link()
                            {
                                LinkText = link.TextContent,
                                Href = link.Href
                            };

                            courseLinks.Add(linkModel);
                        }
                    }                    

                    // Prepare to grab the links within the lessons.
                    foreach (Link lesson in courseLinks)
                    {
                        if (lesson.Href == document.BaseUri || document.BaseUri.Contains("#"))
                            continue;

                        // Open the course to parse for lessons.
                        var lessonCtx = await context.OpenAsync(lesson.Href);
                        Console.WriteLine(lessonCtx.Title);

                        // Collect the links in the lesson.
                        var lessonLinks = document.QuerySelectorAll<IElement>("a");
                        List<Link> linksInLesson = new();

                        foreach (IHtmlAnchorElement lessonLink in lessonLinks)
                        {
                            if (LinkExtensions.CanAdd(lessonLink, course, ignoreList, completedParses))
                            {
                                var linkModel = new Link()
                                {
                                    LinkText = lessonLink.TextContent,
                                    Href = lessonLink.Href
                                };

                                linksInLesson.Add(linkModel);
                            }
                        }

                        foreach (var aLink in linksInLesson)
                        {
                            try
                            {
                                var request = WebRequest.Create(aLink.Href);

                                if (!aLink.WasParsed)
                                    request.GetResponse();

                            }
                            catch (WebException webex)
                            {
                                Console.WriteLine("Broken link found: " + aLink.Href);
                                brokenLinks.Add(aLink);
                            }
                            finally
                            {
                                aLink.WasParsed = true;
                                completedParses.Add(aLink);
                            }
                        }
                    }
                }


                // Print results.
                Console.WriteLine("These are all the detected broken links to test:");
                foreach (var brokenLink in brokenLinks)
                {
                    Console.WriteLine($"{brokenLink.LinkText} - {brokenLink.Href}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something has gone wrong!");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
