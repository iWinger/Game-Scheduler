using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord_Bot.Game;
using Discord_Bot.Games;
using Newtonsoft.Json;
using System.Windows.Markup;

namespace Discord_Bot.StartUp
{
    internal class Program
    {

        private readonly DiscordSocketClient client;
        private const string token = "MTI4NDI4MjI0MjQ5OTIxOTU1MQ.Gyk4fL.zqaJiMFUtXIaCncAVMEesM1gNAbMUDk7gir0Og";
        private IMessageChannel channel;
        private User poster { get; set; }
        private ulong posterId { get; set; }
        private Repository repository;
        private Dictionary<ulong, Post> dict;
        private int time = 0;


        public Program()
        {
            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
            repository = new Repository();
            dict = repository.GetDict();
            client = new DiscordSocketClient(_config);
            client.MessageReceived += MessageHandler;
            client.Ready += Client_Ready;
            client.SlashCommandExecuted += SlashCommandHandler;
            client.ButtonExecuted += MyButtonHandler; // Subscribes to events
            client.ReactionAdded += OnReactionAddedEvent;
        }

        public async Task StartBotAsync()
        {
            /* Make sure to put the token into the environmental variable so it doesn't get exposed */
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

           

            client.UserIsTyping += UserIsTyping;

            await Task.Delay(-1); // Makes it run forever
        }

        public async Task Client_Ready()
        {
            ulong testGuild = 1202800312499572776; // A test guild for now (server)


            var guild = client.GetGuild(testGuild);

            var guildCommand = new SlashCommandBuilder().WithName("tvt").WithDescription("Starting a TVT").AddOption("minutes", ApplicationCommandOptionType.String, "the value to set the field", isRequired: true);
            guildCommand.WithDescription("Starting a TVT");


            /*
            var statsCommand = new SlashCommandBuilder();
            statsCommand.WithName("stats");
            statsCommand.WithDescription("Generates statistics for active users");
            */

            var bossCommand = new SlashCommandBuilder().WithName("boss").WithDescription("Starting a Boss war").AddOption("minutes", ApplicationCommandOptionType.String, "the value to set the field", isRequired: true);

            var mbCommand = new SlashCommandBuilder().WithName("mb").WithDescription("Starting a MB").AddOption("minutes", ApplicationCommandOptionType.String, "the value to set the field", isRequired: true);

            var statusCommand = new SlashCommandBuilder();
            statusCommand.WithName("status");
            statusCommand.WithDescription("Generates activate participants for most active post from creator");

           

            /*
            var globalCommand = new SlashCommandBuilder();
            globalCommand.WithName("first-global-command");
            globalCommand.WithDescription("This is my first global slash command");
            */

            try
            {
                await guild.CreateApplicationCommandAsync(guildCommand.Build());
                //await guild.CreateApplicationCommandAsync(statsCommand.Build());
                await guild.CreateApplicationCommandAsync(bossCommand.Build());
                await guild.CreateApplicationCommandAsync(mbCommand.Build());
                await guild.CreateApplicationCommandAsync(statusCommand.Build());

                //await client.CreateGlobalApplicationCommandAsync(globalCommand.Build());

            }
            catch(ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                Console.WriteLine(json);
            }
        }

        private async Task OnReactionAddedEvent(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel,ulong> originChannel, SocketReaction reaction)
        {
            SocketMessage msg = (SocketMessage)reaction.Message;
            SocketGuildUser displayName = (SocketGuildUser)reaction.User;
            User user = new User(displayName);
            user.setId(displayName.Id);
            user.setName(displayName.GlobalName);
            //string username = user.GetName();
            ulong userId = user.getId();
            ulong key = 0;
            // We want to compare socket message IDs and find the right one
            foreach(KeyValuePair<ulong,Post> item in dict)
            {
                Post post = item.Value;
                if(post.GetId() == msg.Id)
                {
                    // Found the right one
                    key = item.Key;
                    break;
                }
            }
            
     
            if (dict.ContainsKey(key))
            {
                // If the dictionary contains a valid post
                Post post = dict[key];
                if (!post.GetUsers().Contains(user) && key != userId) // If it's not the owner
                {
                    dict[key].AddUser(user); // Add to dictionary
                }
            }
            
        
        }
      

        private void createPost(SocketMessage msg, ActionRowComponent reminder)
        {
            Post post = new Post(poster,msg, reminder); // Save the socket message ID into the post 
            post.AddUser(poster);
            var key = posterId; 

            if (!dict.ContainsKey(key))
            {
                // Create a post with the associated poster
                dict.Add(key, post);
            }
           
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            // Create a button for the post
            Button button = new Button("Reminder");
            var builder = button.Spawn("custom-id");
            string username = command.User.GlobalName;
            poster = new User(command.User); // Save the original author of this command
            poster.setName(username);
            posterId = command.User.Id;
            

         
            

            switch (command.Data.Name)
            {
                // Create a post
                case "tvt":
                    if (!dict.ContainsKey(posterId))
                    {
                        await HandleGameCommand(command, username, "TVT");
                  
                    }
                    else
                        await command.RespondAsync($"You already have an active post");
                    break;
                case "boss":
                    if (!dict.ContainsKey(posterId))
                    {
                        await HandleGameCommand(command, username, "Boss");
                    }
                    else
                        await command.RespondAsync($"You already have an active post");
                    break;
                case "mb":
                    if (!dict.ContainsKey(posterId))
                    {
                        await HandleGameCommand(command, username, "MB");
                    }
                    else
                        await command.RespondAsync($"You already have an active post");
                    break;
                case "status":
                    // Creator of the post can get the information
                    string res = "";
                    var key = posterId;
                    if (dict.ContainsKey(key) && dict[key] != null)
                    {
                        List<User> users = dict[key].GetUsers();
                      
                        foreach (User user in users)
                        {
                            res += $"{user.getName()}\n";
                        }
                        await command.RespondAsync($"Active participants in this TVT: \n\n {res} \n Current number of players interested: {users.Count}");
                    }
                    else await command.RespondAsync($"You have no ongoing post");

                    break;
                case "stats":
                    await command.RespondAsync($"{username} executed stats");
                    break;
                default:
                    await command.RespondAsync($"Not a valid command");
                    break;


            }
            
            
        }


        private async Task HandleGameCommand(SocketSlashCommand command, string username, string type)
        {
            var fieldName = command.Data.Options.First().Name;
            var value = command.Data.Options.FirstOrDefault().Value;
            
            
            string timeMsg = $"in {value} minutes.";
            timeMsg = value.ToString() == "0" ? "now!" : timeMsg;

            Button button = new Button("Reminder");
            string? customId = (value == null) ? "0" : value.ToString();
            var builder = button.Spawn(customId);


            await command.RespondAsync($"{type} game has been issued by {username} {timeMsg} React to this to join!", components: builder.Build());
        }


            private async void SendDM(IDMChannel channel, int time)
        {

           await Task.Delay(time);
           await channel.SendMessageAsync("Your TVT has started now");
           
        }
        public async Task MyButtonHandler(SocketMessageComponent component)
        {
            ulong id = component.User.Id;
            IUser user = client.GetUserAsync(id).Result;
            var channel = await user.CreateDMChannelAsync();


            //component.Data.CustomId

            int time = Int32.Parse(component.Data.CustomId) * 60000;
            await component.RespondAsync($"You will be reminded 😀", null, false, true);
            SendDM(channel, time); // In milliseconds
            
                    
                   
        }
        private async Task MessageHandler(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                if (message.Components.Count == 1)
                {
                   
                        ActionRowComponent reminder = message.Components.SingleOrDefault();
                        createPost(message, reminder);
                    
                    
                }
            }

            /* Process commands */
            /*

            await ReplyAsync(message, "Hello! I am an expense tracker bot. Let's get you started. Would you like to make an account? ");

            string answer = message.Content.ToLower();
            if (answer.Equals("yes"))
            {
                // Create a detailed information account
                string name = message.Author.GlobalName;
                Account account = new Account(name);
            }
            else
            {
                await ReplyAsync(message, "It looks like you don't want to save money. ");
            }
            */

        }

        private async Task UserIsTyping(Cacheable<IUser, ulong> user, Cacheable<IMessageChannel, ulong> channel)
        {
            this.channel = await channel.GetOrDownloadAsync();
            //this.channel.SendMessageAsync("I know you are typing...");
        }


        private async Task ReplyAsync(SocketMessage message, string response) => await message.Channel.SendMessageAsync(response);

        


        static void Main(string[] args) => new Program().StartBotAsync().GetAwaiter().GetResult();
    }
}
