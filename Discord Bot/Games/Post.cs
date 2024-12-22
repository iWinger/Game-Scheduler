using Discord;
using Discord.WebSocket;
using Discord_Bot.Game;
using Discord_Bot.StartUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Discord_Bot.Games
{
    internal class Post
    {
        private ulong Id { get; set; }

        private User Creator { get; set; }

        private SocketMessage Message { get; set; }

        private ActionRowComponent Reminder { get; set; }

        private List<User> Users { get; set; }

        private DateTime Time { get; set; }

        private Repository Mode { get; set; }
        
        private int Minutes { get; set; }



        public Post(User creator, SocketMessage message, ActionRowComponent reminder, DateTime time, int minutes)
        {
            this.Creator = creator;
            this.Message = message;
            this.Id = Message.Id;
            this.Reminder = reminder;
            this.Users = new List<User>();
            this.Time = time;
            this.Minutes = minutes;
        }

        public SocketMessage getMessage() { return this.Message; }
        public List<User> getUsers() { return this.Users; }

        public ulong getId() { return this.Id; }

        public DateTime getTime() { return this.Time; }
        public void addUser(User User)
        {
            Users.Add(User);
        }

        public void removeUsers(User User)
        {
            Users.Remove(User);
        }

    }
}
