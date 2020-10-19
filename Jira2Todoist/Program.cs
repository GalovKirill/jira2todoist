using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Atlassian.Jira;
using CommandLine;
using Todoist.Net;
using Todoist.Net.Models;

namespace Jira2Todoist
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            await Parser.Default
                .ParseArguments<ProgramArgs>(args)
                .WithParsedAsync(programArgs => new Program().Run(programArgs));
        }

        private async Task Run(ProgramArgs args)
        {
            using var todoistClient = new TodoistClient(args.TodoistApiToken);
            var jira = Jira.CreateRestClient(args.JiraServer, args.JiraLogin, args.JiraPassword);
            
            foreach (var num in args.TaskKeyNumbers)
            {
                Issue? issue = null;
                string taskKey = $"{args.JiraProject}-{num}";
                try
                {
                    issue = await jira.Issues.GetIssueAsync(taskKey);
                }
                catch
                {
                    await Console.Error.WriteLineAsync($"Failed to fetch {taskKey}");
                }

                if(issue == null)
                    continue;

                try
                {
                    await todoistClient.Items.AddAsync(new Item($"{issue.Key} {issue.Summary}"));
                }
                catch
                {
                    await Console.Error.WriteLineAsync($"Failed to create item {issue.Key}");
                }
            }
        }
    }
    
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ProgramArgs
    {
        [Option("todoist-token", Required = true, HelpText = "Todoist API token for connecting with todoist")]
        public string TodoistApiToken { get; set; }
        
        [Option("jira-server", Required = true, HelpText = "Address jira server")]
        public string JiraServer { get; set; }
        
        [Option("jira-login", Required = true, HelpText = "Login in jira")]
        public string JiraLogin { get; set; }
        
        [Option("jira-password", Required = true, HelpText = "Password in jira")]
        public string JiraPassword { get; set; }
        
        [Option("jira-project", Required = true, HelpText = "Project in jira where task will be search")]
        public string JiraProject { get; set; }

        [Option("task-keys", Required = true, HelpText = "Task numbers")]
        public IEnumerable<string> TaskKeyNumbers { get; set; }
    }
}