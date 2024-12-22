﻿using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord_Bot.Game;
using Discord_Bot.Games;
using Discord_Bot.Utility;
using Discord.Commands;

using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Discord.Rest;


namespace Discord_Bot.StartUp
{
    internal class Program
    {

        private readonly DiscordSocketClient client;
        private Repository repository;
        private Dictionary<ulong, Post> dict;
        private User poster { get; set; }
        private string token { get; set; }
        private ulong posterId { get; set; }
        private int minutes = 0;

        private IConfigurationRoot configuration;


        public Program()
        {
            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
            repository = new Repository();
            dict = repository.GetDict();
            configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
            client = new DiscordSocketClient(_config);
            client.MessageReceived += MessageHandler;
            client.Ready += Client_Ready;
            client.SlashCommandExecuted += SlashCommandHandler;
            client.ButtonExecuted += MyButtonHandler; // Subscribes to events
            client.ReactionAdded += OnReactionAddedEvent;
            //client.UserIsTyping += UserIsTyping;
        }


        public async Task StartBotAsync()
        {
            //Secret value

            var token = configuration["token"];
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();


            await Task.Delay(-1); // Makes it run forever
        }

        public async Task Client_Ready()
        {
            
            foreach (var guild in client.Guilds) { 
            

                var tvtCommand = new SlashCommandBuilder().WithName("tvt").WithDescription("Starting a Team Vs Team").AddOption("minutes", ApplicationCommandOptionType.String, "the value to set the field", isRequired: true);

                var deleteCommand = new SlashCommandBuilder().WithName("delete").WithDescription("Deleting your active post");

                var statusCommand = new SlashCommandBuilder().WithName("status").WithDescription("Shows list of participants for most active post from creator");

                var repostCommand = new SlashCommandBuilder().WithName("repost").WithDescription("Allows you to repost your current post if needed");

                var balancedCommand = new SlashCommandBuilder().WithName("balance").WithDescription("Generates a balanced team based on assigned player value ratings");

                var randomCommand = new SlashCommandBuilder().WithName("random").WithDescription("Creates a random set of teams based on players").AddOption("players", ApplicationCommandOptionType.String, "put in list of players separated by space", isRequired: true);

                var rulesCommand = new SlashCommandBuilder().WithName("rules").WithDescription("How to use this bot");


                try
                {
                    //await guild.CreateApplicationCommandAsync(tvtCommand.Build());
                    await guild.CreateApplicationCommandAsync(tvtCommand.Build());
                    await guild.CreateApplicationCommandAsync(deleteCommand.Build());
                    await guild.CreateApplicationCommandAsync(statusCommand.Build());
                    await guild.CreateApplicationCommandAsync(repostCommand.Build());
                    await guild.CreateApplicationCommandAsync(balancedCommand.Build());
                    await guild.CreateApplicationCommandAsync(randomCommand.Build());
                    await guild.CreateApplicationCommandAsync(rulesCommand.Build());
                    //await client.CreateGlobalApplicationCommandAsync(globalCommand.Build());

                }
                catch (ApplicationCommandException exception)
                {
                    var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                    Console.WriteLine(json);
                }
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
                if(post.getId() == msg.Id)
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
                if (!post.getUsers().Contains(user) && key != userId) // If it's not the owner
                {
                    dict[key].addUser(user); // Add to dictionary
                }
            }
            
        
        }
      

        private void createPost(SocketMessage msg, ActionRowComponent reminder, int minutes)
        {
            Post post = new Post(poster,msg, reminder, DateTime.Now,minutes); // Save the socket message ID into the post 
            poster.setId(posterId);
            post.addUser(poster);
            var key = posterId; 

            if (!dict.ContainsKey(key))
            {
                // Create a post with the associated poster
                dict.Add(key, post);
            }
           
        }

        private async void deletePost(SocketSlashCommand command, ulong posterId)
        {   
            
            var key = posterId;
            
            if (dict.ContainsKey(posterId)){
                dict.Remove(posterId);
                await command.RespondAsync($"You have successfully deleted your post 😀", null, false, true);
            }
            else
            {
                await command.RespondAsync($"You have no post to delete", null, false, true);
            }
        }
        private async void repost(SocketSlashCommand command, ulong posterId)
        {
            var key = posterId;

            if (dict.ContainsKey(posterId))
            {
                SocketMessage msg = dict[posterId].getMessage();
                Button button = new Button("Reminder");
                string customId = command.User.Id.ToString();
                var builder = button.Spawn(customId);
                await command.RespondAsync(msg.Content, components: builder.Build());
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
           
                case "tvt":
                    // Create a post
                    if (!dict.ContainsKey(posterId))
                        await HandleGameCommand(command, username, "TVT");
                    else
                        await command.RespondAsync($"You already have an active post", null, false, true);
                    break;     
                case "delete":
                    // Deletes the post
                    deletePost(command, posterId);
                    
                    break;
                case "status":
                    // Gets how many active users are in the post
                    await HandleStatusCommand(command, posterId);

                    break;
                case "repost":
                    // Reposts active post
                    repost(command, posterId);
                    break;
                case "balanced":
                    break;
                case "random":
                    //Creates 2 random sets of teams
                    await HandleRandomCommand(command);
                    break;
                case "rules":
                    //Rules on the bot
                    await command.RespondAsync($"Hello fellow player 😀! These are the commands you can use for this bot: \n\n */tvt set [minutes]* \n Description: Sets a TVT game post in x amount of minutes. People who react to the post display interest in joining, and are also allowed to be notified by the bot through a DM. \n  */status* \n Description: Shows the users who reacted to the post and are interested in the game. \n */delete* \n Description: Deletes your current active post. \n */repost* \n Description: Reposts your current active post \n */random [player_1 ... player_n] * \n Description: Creates two sets of random teams. The number of players must be even. ");
                    break;
                default:
                    await command.RespondAsync($"Not a valid command");
                    break;


            }
            
            
        }

        private async Task HandleStatusCommand(SocketSlashCommand command, ulong posterId)
        {
            string res = "";
            var key = posterId;
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                List<User> users = dict[key].getUsers();

                foreach (User user in users)
                {
                    res += $"{user.getName()}\n";
                }
                await command.RespondAsync($"Active participants in this TVT: \n\n {res} \n Current number of players interested: {users.Count}");
            }
            else await command.RespondAsync($"You have no ongoing post");

        }

        private async Task HandleRandomCommand(SocketSlashCommand command)
        {
            var value = command.Data.Options.FirstOrDefault().Value;
            string str = value.ToString().Trim();
            string[] arr = str.Split(' ',20); // 20 is the Predetermined max size
            User[] users = new User[arr.Length];
            int idx = 0;

            if (arr.Length % 2 != 0)
            {
                await command.RespondAsync($"Please input an even number of players.");
            }
            else {
                foreach (string name in arr)
                {
                    User user = new User(name);
                    users[idx++] = user;

                }
                RandomHelper helper = new RandomHelper(users);
                List<(List<User>, List<User>)> teams = helper.calculateTeams();
                (List<User>, List<User>) randomTeam = helper.GetRandomTeam(teams);

                await command.RespondAsync($"Random Team A: {helper.PrintTeam(randomTeam.Item1)} \n\nRandom Team B: {helper.PrintTeam(randomTeam.Item2)}");
            }

        }

        private async Task HandleGameCommand(SocketSlashCommand command, string username, string type)
        {
            var fieldName = command.Data.Options.First().Name;
            var value = command.Data.Options.FirstOrDefault().Value;

            DateTime currTime = DateTime.Now;
            DateTime beforeTime = currTime.AddMinutes((double)minutes);
            string timeMsg = $"in {value} minutes. ({beforeTime})";
           
            timeMsg = value.ToString() == "0" ? "now!" : timeMsg;

            Button button = new Button("Reminder");
            minutes = (value == null) ? Int32.Parse("0") : Int32.Parse(value.ToString());
            
            string customId = command.User.Id.ToString();
            var builder = button.Spawn(customId);
            


            await command.RespondAsync($"{type} game has been issued by {username} {timeMsg} React to this to join!", components: builder.Build());
        }


        private async void SendDM(IDMChannel channel, int time)
        {

           await Task.Delay(time);
           await channel.SendMessageAsync("Your game has started now!");
           
        }
        public async Task MyButtonHandler(SocketMessageComponent component)
        {
            // Get the post's time and subtract it
            ulong id = component.User.Id;
            IUser user = client.GetUserAsync(id).Result;
            var channel = await user.CreateDMChannelAsync();

            // I am a user and i should be involved in the post
            ulong key = Convert.ToUInt64(component.Data.CustomId);
            bool foundUser = false;
            if (dict[key] != null)
            {
                foreach(User u in dict[key].getUsers())
                {
                    
                    if (id == u.getId())
                    {
                        foundUser = true;
                    }
                    break;
                }
            }
            if (foundUser)
            {
                Post post = dict[key];
                DateTime beforeTime = post.getTime().AddMinutes((double)minutes);
                DateTime currTime = DateTime.Now;
                long difference = (beforeTime - currTime).Ticks;
                difference = difference / 10000;
                if (difference >= 0)
                {
                    await component.RespondAsync($"You will be reminded 😀", null, false, true);
                    SendDM(channel, (int)difference);
                }
                else
                {
                    await component.RespondAsync("This game has already happened or is happening!", null , false, true);
                }
            }
            else
            {
                await component.RespondAsync($"Please join first by reacting to the post 😀", null, false, true);
            }


        }
        private async Task MessageHandler(SocketMessage message)
        {

            if (message.Author.IsBot)
            {
                if (message.Components.Count == 1)
                {
                          
                    if (!dict.ContainsKey(posterId))
                    {
                        ActionRowComponent reminder = message.Components.SingleOrDefault();
                        createPost(message, reminder, minutes);
                    }
                    
                    
                }
            }

        }

        
        /*
        private async Task UserIsTyping(Cacheable<IUser, ulong> user, Cacheable<IMessageChannel, ulong> channel)
        {
            this.channel = await channel.GetOrDownloadAsync();
            //this.channel.SendMessageAsync("I know you are typing...");
        }
        */

        static void Main(string[] args) => new Program().StartBotAsync().GetAwaiter().GetResult();
    }
}
