using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.IO;
using Newtonsoft.Json;
using MyNameSpace;
using FileManagementCS;
using System.Drawing;
using System.Net.Mail;
using System.Xml;

Bot bot = new();
FileManagementCS.FILE_MANAGEMENT fm = new();


// Output Bot App Directory
string output = fm.applicationDirectory;
Console.WriteLine(output);
Console.WriteLine('\n');

// Run Bot
await bot.Run();


// Bot Main Code
namespace MyNameSpace
{
    public class Bot
    {
        static readonly string TokenPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "token.txt");

        DiscordSocketClient _client;

        private string Token
        {
            get
            {
                using var sr = File.OpenText(TokenPath);
                var returnVar = sr.ReadToEnd().ReplaceLineEndings("");
                sr.Close();
                return returnVar;
            }
        }

        public Bot()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;
        }

        public async Task Run()
        {
            await _client.LoginAsync(TokenType.Bot, Token);
            await _client.StartAsync();
            await Task.Delay(-1);

        }
        public Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }


        // Determine which command processing to run based on which command was used
        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "example":
                    await ExampleCommand(command);
                    break;
                default:
                    break;
            }
        }        

        // Command processing function for the example command
        private async Task ExampleCommand(SocketSlashCommand command)
        {
            // We need to extract the first data parameter from the command. since we only have one option and it's required, we can just use the first option.
            var data1 = (SocketGuildUser)command.Data.Options.First().Value;            
            Console.WriteLine(data1.ToString());
        }
               
        // Instantiate Commands
        private async Task Client_Ready()
        {
            FileManagementCS.FILE_MANAGEMENT fm = new();

            //Test command, was used to learn some stuff about commands but is not meant to be a primary function of the bot
            var example = new SlashCommandBuilder()
                .WithName("example") // Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
                .WithDescription("Example Command") // Descriptions can have a max length of 100.
                .AddOption("Text", ApplicationCommandOptionType.String, "Say Stuff", isRequired: true);


            //Init Commands within Servers
            ulong guildID;
            int guildNum = 0;
            int guildNumTot = fm.fileLinesCheck(fm.applicationDirectory, "server_id.txt");
            do
            {
                // Set Guild ID
                guildID = Convert.ToUInt64(fm.fileReadSpecific(fm.applicationDirectory, "server_id.txt", guildNum, guildNum));


                try
                {
                    // Generate Commands
                    await _client.Rest.CreateGuildCommand(example.Build(), guildID);
                }
                catch (HttpException exception)
                {
                    var json = JsonConvert.SerializeObject(exception.Errors, Newtonsoft.Json.Formatting.Indented);
                    Console.WriteLine(json);
                }
                guildNum += 2;
            } while (guildNum <= guildNumTot);
        }
    }
}
