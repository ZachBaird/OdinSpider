# OdinSpider

### Purpose
This application is meant to be run in the terminal. It hits the courses in The Odin Project to collect the lessons in each course, and then parses the lessons for any anchor tags that meet the criteria listed in `LinkExtensions`. A web request is made to the links and - if the response is bad - the broken link is output to the terminal for further investigation.

### How to Run
You will need the .NET 5 SDK installed on your machine. You can find instructions to download it [here](https://dotnet.microsoft.com/download/dotnet/5.0).

1. Clone `OdinSpider` to your machine.
2. `cd` into `OdinSpider/OdinSpider`
3. If you want to write code, use Visual Studio or VSCode with `code .` in this dir.
4. If you want to run it, enter `dotnet run` in your terminal.
5. This app can be published to a self-contained executable if necessary.

### Notes
This is very beta version for this tool, and was written just to see if there's any additional broken links in the Odin curriculum. The app's performance can and will be improved on. Additionally, there isn't a lot of feedback from the app while it runs so - when I refactor it - that will be a todo.
