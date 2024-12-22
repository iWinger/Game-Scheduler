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
        private ulong Id { get; set; }

        private string Name { get; set; }

        private Post Post { get; set; }

        private int Rating { get; set; }

        public User(SocketUser user)
        {
            this.Name = user.Username;
            this.Id = user.DiscriminatorValue;
        }

        public User(string name)
        {
            this.Name = name;
        }

        public void setId(ulong id)
        {
            this.Id = id;
        }

        public void setName(string name)
        {
            this.Name = name;
        }

        public void setPost(Post post)
        {
            Post = post;
        }

        public void setRating(int rating)
        {
            this.Rating = rating;
        }

        public ulong getId() { return this.Id; }
        
        public string getName() { return this.Name; }

        public Post getPost() { return this.Post; }

        public int getRating() { return this.Rating; }
    }
}
