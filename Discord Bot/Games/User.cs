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

        public void setName(string name)
        {
            this.Name = name;
        }

        public void setPost(Post post)
        {
            Post = post;
        }

        public void setId(ulong id)
        {
            this.Id = id;
        }

        public Post getPost() { return this.Post; }
        public string getName() { return this.Name; }
        public ulong getId() { return this.Id; }
    }
}
