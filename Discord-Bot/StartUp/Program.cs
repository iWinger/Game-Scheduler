using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord_Bot.Game;
using Discord_Bot.Games;
using Discord_Bot.Utility;
using Discord.Commands;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;



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
        private int minutes;
        private ulong guildId;

        private IConfigurationRoot configuration;


        public Program()
        {
            var _config = new DiscordSocketConfig { MessageCacheSize = 200 };
            repository = new Repository();
            dict = repository.GetDict();
            minutes = 0;
            guildId = 0;
            client = new DiscordSocketClient(_config);
            client.MessageReceived += MessageHandler;
            client.JoinedGuild += JoinHandler;
            client.SlashCommandExecuted += SlashCommandHandler;
            client.ButtonExecuted += MyButtonHandler; // Subscribes to events
            client.ReactionAdded += OnReactionAddedEvent;
            client.Log += LogAsync;
            //client.UserIsTyping += UserIsTyping;
        }


        public async Task StartBotAsync()
        {
            //Secret value
            configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
            // var token = configuration["token"];
            await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("token"));
            //await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();


           await Task.Delay(-1); // Makes it run forever
        }

        public async Task JoinHandler(SocketGuild socketGuild)
        {

                Console.WriteLine("joined");
                var guild = client.GetGuild(socketGuild.Id);
            Console.WriteLine(socketGuild.Id);

            //var guild = client.GetGuild(testGuild);
            var tvtCommand = new SlashCommandBuilder().WithName("tvt").WithDescription("Starting a Team Vs Team game").AddOption("minutes", ApplicationCommandOptionType.String, "the value to set the field", isRequired: true);

                var deleteCommand = new SlashCommandBuilder().WithName("delete").WithDescription("Deleting your active post");

                var statusCommand = new SlashCommandBuilder().WithName("status").WithDescription("Shows list of participants for most active post from creator");

                var repostCommand = new SlashCommandBuilder().WithName("repost").WithDescription("Allows you to repost your current post if needed");

                var balancedCommand = new SlashCommandBuilder().WithName("balance").WithDescription("Generates a set of balanced teams based on assigned player value ratings").AddOption("playersratings", ApplicationCommandOptionType.String, "put in player and then number separated by space", isRequired: true);

                var randomCommand = new SlashCommandBuilder().WithName("random").WithDescription("Creates a random set of teams based on players").AddOption("players", ApplicationCommandOptionType.String, "put in list of players separated by space", isRequired: true);

                var quoteCommand = new SlashCommandBuilder().WithName("quote").WithDescription("Gets a random quote from legendary figure Sun Tzu on warfare");

                var rulesCommand = new SlashCommandBuilder().WithName("rules").WithDescription("How to use this bots and the commands");

                var helpCommand = new SlashCommandBuilder().WithName("help").WithDescription("How to use this bots and the commands");


                try
                {
                //await guild.CreateApplicationCommandAsync(tvtCommand.Build());
                if (guild != null)
                {
                    await guild.CreateApplicationCommandAsync(tvtCommand.Build());
                    await guild.CreateApplicationCommandAsync(deleteCommand.Build());
                    await guild.CreateApplicationCommandAsync(statusCommand.Build());
                    await guild.CreateApplicationCommandAsync(repostCommand.Build());
                    await guild.CreateApplicationCommandAsync(balancedCommand.Build());
                    await guild.CreateApplicationCommandAsync(randomCommand.Build());
                    await guild.CreateApplicationCommandAsync(quoteCommand.Build());
                    await guild.CreateApplicationCommandAsync(rulesCommand.Build());
                    await guild.CreateApplicationCommandAsync(helpCommand.Build());
                }
                    //await client.CreateGlobalApplicationCommandAsync(globalCommand.Build());

                }
                catch (HttpException exception)
                {
                    var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                    Console.WriteLine(json);
                }
           
        }

        

        private async Task OnReactionAddedEvent(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel,ulong> originChannel, SocketReaction reaction)
        {
            
            SocketMessage msg = (SocketMessage)reaction.Message;
            SocketGuildUser displayName = (SocketGuildUser)reaction.User;

            ulong key = (msg.Interaction != null) ? msg.Interaction.User.Id : 0;
            User user = new User(displayName.GlobalName, displayName.Id);
           
            //string username = user.GetName();


     
            if (dict.ContainsKey(key))
            {
                // If the dictionary contains a valid post
                Post post = dict[key];
                
                bool canAdd = true;
                foreach(User u in post.getUsers())
                {
                    if(u.getId() == user.getId())
                    {
                        canAdd = false;
                    }
                }
                
                if (canAdd && key != user.getId()) // If it's not the owner
                {
                    dict[key].addUser(user); // Add to dictionary
                }
            }

            await Task.CompletedTask;
        
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
                await command.RespondAsync($"You have successfully deleted your post 😀");
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
                (int x,DateTime y) item = calculateTime(posterId);
                int mins = item.Item1;
                DateTime finishTime = item.Item2;
                SocketMessage msg = dict[posterId].getMessage();
                Button button = new Button("Reminder");
                //string customId = command.User.Id.ToString();
                var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
                var builder = button.Spawn("custom-id");
                var timeMsg = $" Game starts in {mins.ToString()} minutes. (__{finishTime} UTC{offset}__).";
                if(mins <= 0)
                {
                    timeMsg = "Game starts in 0 minutes. Game is already happening! ";
                }

                var embed = new EmbedBuilder()
               .WithFooter(footer => footer.Text = "React to this post to join!")
               .WithColor(Color.Blue)
               .WithTitle($"TVT game has been issued by {command.User.GlobalName}.")
               .WithDescription($"{timeMsg}");
                //await command.RespondAsync(msg.Content, components: builder.Build());
                await command.RespondAsync(embed: embed.Build(), components: builder.Build());
            }
            else
            {
                await command.RespondAsync($"There is no active post");
            }
        }

        private (int,DateTime) calculateTime(ulong posterId)
        {
            Post post = dict[posterId];
            int max = post.getMinutes();
            DateTime time = post.getTime();
            DateTime finishTime = time.AddMinutes((double)max);
            DateTime currTime = DateTime.Now;
            long difference = (currTime - time).Ticks;
            difference = (int)(difference / 10000);
            int mins = (int)(difference / 60000);
            mins = max - mins;
            return (mins, finishTime);
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
                case "balance":
                    await HandleBalancedCommand(command);
                    break;
                case "random":
                    //Creates 2 random sets of teams
                    await HandleRandomCommand(command);
                    break;
                case "quote":
                    await HandleQuoteCommand(command);
                    break;
                case "rules":
                    //Rules on the bot
                    await command.RespondAsync($"Hello fellow player 😀! These are the commands you can use for this bot: \n\n ***/tvt [minutes]*** \n __Example:__ /tvt 30 \n Sets a Team Vs Team game post in x amount of minutes. People who react to the post display interest in joining, and are also allowed to be notified by the bot through a DM. \n ***/status*** \n Shows the users who reacted to the post and are interested in the game. \n ***/delete*** \n Deletes your current active post. \n ***/repost*** \n Reposts your current active post with the time left to projected game time. \n ***/balance [player_1 rating_1 ... player_n rating_n]*** \n __Example:__ /balance jack 95 mary 85 joe 65 sarah 55 \n Creates a list of balanced teams sorted by ascending values in difference between the two teams. Best rating values would range from [0,100]. \n ***/random [player_1 ... player_n]*** \n __Example:__ /random jack mary joe sarah \n Creates two sets of random teams. The number of players must be even. \n ***/quote*** \n Gets a random quote from the legendary figure Sun Tzu on the art of warfare.");
                    break;
                case "help":
                    await command.RespondAsync($"Hello fellow player 😀! These are the commands you can use for this bot: \n\n ***/tvt [minutes]*** \n Sets a Team Vs Team game post in x amount of minutes. People who react to the post display interest in joining, and are also allowed to be notified by the bot through a DM. \n ***/status*** \n Shows the users who reacted to the post and are interested in the game. \n ***/delete*** \n Deletes your current active post. \n ***/repost*** \n Reposts your current active post with the time left to projected game time. \n ***/balance [player_1 rating_1 ... player_n rating_n]*** \n Creates a list of balanced teams sorted by ascending values in difference between the two teams. Best rating values would range from [0,100]. \n ***/random [player_1 ... player_n]*** \n Creates two sets of random teams. The number of players must be even. \n ***/quote*** \n Gets a random quote from the legendary figure Sun Tzu on the art of warfare. \n ***/rules*** \n All the commands and examples you need to know for this bot.");
                    break;
                default:
                    await command.RespondAsync($"Not a valid command", null, false, true);
                    break;

            }
 
        }

        private async Task HandleQuoteCommand(SocketSlashCommand command)
        {
            
            int maxLimit = 50;
            List<string> quotes = new List<string>();
            for(int i = 0; i <= maxLimit; i++)
            {
                string num = i.ToString();
                //var quote = configuration["quote" + num];
                var quote = Environment.GetEnvironmentVariable("quote" + num);
                if (quote != null)
                {
                    quotes.Add(quote);
                    //quotes.Add(configuration["quote" + num]);
                }

            }
            
            Random random = new Random();
            int idx = random.Next(quotes.Count+1);
            string chosenQuote = quotes[idx];

            await command.RespondAsync($"```bash\n\"{chosenQuote}\"```");
        }

        private async Task HandleStatusCommand(SocketSlashCommand command, ulong posterId)
        {
 
            var key = posterId;
            
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                string res = "";
                (int x, DateTime y) item = calculateTime(posterId);
                int mins = item.Item1;
                DateTime finishTime = item.Item2;
                var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
                mins = (mins < 0) ? 0 : mins;
                List<User> users = dict[key].getUsers();

                foreach (User user in users)
                {
                    res += $"{user.getName()} ✅\n";
                }
                await command.RespondAsync($"Active participants in this TVT game: \n\n{res} \nCurrent number of players interested: **{users.Count}** \nTime until the game starts : **{mins}** minutes\n{finishTime} UTC{offset}");
            }
            else await command.RespondAsync($"You have no ongoing post", null, false, true);

        }

        private async Task HandleBalancedCommand(SocketSlashCommand command)
        {
            var value = command.Data.Options.FirstOrDefault().Value;
            string strip = value.ToString().Trim();
            string str = "";
            for (int i = 0; i < strip.Length; i++)
            {
                if (Char.IsLetterOrDigit(strip[i]) || strip[i] == ' ')
                    str += strip[i];
            }


            string[] arr = str.Split(' ', 40); // 20 is the Predetermined max size
            int rating = 0;
            int[] ratings = new int[arr.Length / 2];
            User[] players = new User[arr.Length / 2];
            int idx = 0;
            int count = 0;
            bool hasErrors = false;
            string msg = "";

            
            if (arr.Length % 2 != 0)
                {
                    msg = "Please put in an even number of players and ratings. ";
                    hasErrors = true;
                    //return Task.CompletedTask;
                }

            if (!hasErrors)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    bool isNumber = Int32.TryParse(arr[i], out rating);

                    if (i % 2 == 0)
                    {
                        User user = new User(arr[i]);
                        players[count++] = user;
                    }

                    if (i % 2 == 1 && !isNumber)
                    {
                        msg = "Please put a rating after each player's name.";
                        hasErrors = true;
                        break;
                    }

                    else if (i % 2 == 1 && isNumber)
                    {
                        ratings[idx++] = rating;
                    }
                }
            }
            int j = 0;
            foreach(User user in players)
            {
                if (user != null)
                    user.setRating(ratings[j++]);
            }
            
            if (!hasErrors)
            {

                RandomHelper helper = new RandomHelper(players);
                List<(List<User>, List<User>)> teams = helper.calculateTeams();
                Dictionary<int, int> dict = new Dictionary<int, int>(); // Maps each unique index to each sum difference
                for (int i = 0; i < teams.Count; i++)
                {
                    int sumA = 0;
                    int sumB = 0;
                    foreach (User user in teams[i].Item1)
                    {
                        if (user != null)
                            sumA += user.getRating();
                    }
                    foreach (User user in teams[i].Item2)
                    {
                        if (user != null)
                            sumB += user.getRating();
                    }
                    int sumDiff = (sumA - sumB) >= 0 ? sumA - sumB : sumB - sumA;
                    dict.Add(i, sumDiff);
                }
                // Now we have each set of team assigned a difference in sums
                msg = "The ideal balance number is 0. The closer to 0, the more balanced the teams are 😀\n\n";
                var sortedDict = dict.OrderBy(i => i.Value);
                int limit = 10; // Instead of number of entries, why not values < 10? 
                int x = 0;
                HashSet<int> dups = new HashSet<int>();
                foreach (KeyValuePair<int, int> pair in sortedDict)
                {
                    if (dups.Contains(pair.Value)) continue;
                    dups.Add(pair.Value);
                    if (x > limit) break;
                    (List<User>, List<User>) set = teams[pair.Key];
                    msg += $"Random Team A: {helper.PrintTeam(set.Item1)} \nRandom Team B: {helper.PrintTeam(set.Item2)} \n **The difference between the two teams in ratings is: {pair.Value}** \n\n";
                    x++;
                }
                await command.RespondAsync(msg);
            }
            else
            {
                await command.RespondAsync(msg, null, false, true);
            }

            
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
                await command.RespondAsync($"Please input an even number of players.",null,false,true);
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
            string val = value.ToString();
            double v = Convert.ToDouble(val);
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            DateTime currTime = DateTime.Now;
            DateTime beforeTime = currTime.AddMinutes(v);
            string timeMsg = $"Game starts in {value} minutes. (__{beforeTime} UTC{offset}__)."; // Timezone is wherever this program is hosted in
           
            timeMsg = value.ToString() == "0" ? "now!" : timeMsg;

            Button button = new Button("Reminder");
            minutes = (value == null) ? Int32.Parse("0") : Int32.Parse(value.ToString());
            
            string customId = command.User.Id.ToString();
            var builder = button.Spawn(customId);
            string art = "art";
            Random r = new Random();
            int num = r.Next(5);
            List<string> arts = new List<string> { "   |\\                     /)\r\n /\\_\\\\__               (_//\r\n|   `>\\-`     _._       //`)\r\n \\ /` \\\\  _.-`:::`-._  //\r\n  `    \\|`    :::    `|/\r\n        |     :::     |\r\n        |.....:::.....|\r\n        |:::::::::::::|\r\n        |     :::     |\r\n        \\     :::     /\r\n         \\    :::    /\r\n          `-. ::: .-'\r\n           //`:::`\\\\\r\n          //   '   \\\\\r\n         |/         \\\\", "       .---.\r\n  ___ /_____\\\r\n /\\.-`( '.' )\r\n/ /    \\_-_/_\r\n\\ `-.-\"`'V'//-.\r\n `.__,   |// , \\\r\n     |Ll //Ll|\\ \\\r\n     |__//   | \\_\\\r\n    /---|[]==| / /\r\n    \\__/ |   \\/\\/\r\n    /_   | Ll_\\|\r\n     |`^\"\"\"^`|\r\n     |   |   |\r\n     L___l___J\r\n      |_ | _|\r\n     (___|___)\r", "               /\\_[]_/\\\r\n              |] _||_ [|\r\n       ___     \\/ || \\/\r\n      /___\\       ||\r\n     (|0 0|)      ||\r\n   __/{\\U/}\\_ ___/vvv\r\n  / \\  {~}   / _|_P|\r\n  | /\\  ~   /_/   []\r\n  |_| (____)        \r\n  \\_]/______\\        \r\n     _\\_||_/_           \r\n    (_,_||_,_)", "         /^\\     .\r\n    /\\   \"V\"\r\n   /__\\   I      O  o\r\n  //..\\\\  I     .\r\n  \\].`[/  I\r\n  /l\\/j\\  (]    .  O\r\n /. ~~ ,\\/I          .\r\n \\\\L__j^\\/I       o\r\n  \\/--v}  I     o   .\r\n  |    |  I   _________\r\n  |    |  I c(`       ')o\r\n  |    l  I   \\.     ,/\r\n_/j  L l\\_!  _//^---^\\\\_ ", "      _,.\r\n    ,` -.)\r\n   ( _/-\\\\-._\r\n  /,|`--._,-^|            ,\r\n  \\_| |`-._/||          ,'|\r\n    |  `-, / |         /  /\r\n    |     || |        /  /\r\n     `r-._||/   __   /  /\r\n __,-<_     )`-/  `./  /\r\n'  \\   `---'   \\   /  /\r\n    |           |./  /\r\n    /           //  /\r\n\\_/' \\         |/  /\r\n |    |   _,^-'/  /\r\n |    , ``  (\\/  /_\r\n  \\,.->._    \\X-=/^\r\n  (  /   `-._//^`\r\n" };
            

            var embed = new EmbedBuilder()
               .WithFooter(footer => footer.Text = "React to this post to join!")
               .WithColor(Color.Blue)
               .WithTitle($"{type} game has been issued by {username}.")
               .WithDescription($"{timeMsg}\n" + // Randomly generates a ascii art
               $"```{arts[num]}```");
               //$"```{configuration[art+num]}```"); 


            await command.RespondAsync(embed:embed.Build(), components: builder.Build());

            //await command.RespondAsync($"{type} game has been issued by {username}. {timeMsg} React to this to join!", embed:embed.Build(), components: builder.Build());
        }



        private async void SendDM(IDMChannel channel, int time)
        {
            if (time < 0 || time >= Int32.MaxValue) { await channel.SendMessageAsync("Not a valid time"); }
            else
            {
                await Task.Delay(time);
                await channel.SendMessageAsync("Your game has started now!");
            }
           
        }
        public async Task MyButtonHandler(SocketMessageComponent component)
        {
            // Get the post's time and subtract it
            ulong id = component.User.Id;
            //id = component.Message.Interaction.Id;
            IUser user = client.GetUserAsync(id).Result;
            var channel = await user.CreateDMChannelAsync();

            // I am a user and i should be involved in the post
            //ulong key = Convert.ToUInt64(component.Data.CustomId);
            ulong key = component.Message.Interaction.User.Id;
            bool foundUser = false;
            if (dict[key] != null)
            {
                foreach(User u in dict[key].getUsers())
                {
                    
                    if (id == u.getId())
                    {
                        foundUser = true;
                        break;
                    }        
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

            await Task.CompletedTask;

        }

        private Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                    + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else
                Console.WriteLine($"[General/{message.Severity}] {message}");

            return Task.CompletedTask;
        }


        /*
        private async Task UserIsTyping(Cacheable<IUser, ulong> user, Cacheable<IMessageChannel, ulong> channel)
        {
            IMessageChannel chan = await channel.GetOrDownloadAsync();
            chan.SendMessageAsync("Hello");
        }
        */


        static void Main(string[] args) => new Program().StartBotAsync().GetAwaiter().GetResult();
    }
}
