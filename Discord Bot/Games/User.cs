using Discord.WebSocket;
using Discord_Bot.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Game
{
    internal class User
    {
        private string Name { get; set; }

        private ulong Id { get; set; }

        private Post Post { get; set; }

        public User(SocketUser user)
        {
            this.Name = user.Username;
            this.Id = user.DiscriminatorValue;
        }

        public void setPost(Post post)
        {
            Post = post;
        }

        public void setId(ulong Id)
        {
            this.Id = Id;
        }
        public string GetName() { return this.Name; }
        public ulong getId() { return this.Id; }
    }
}
