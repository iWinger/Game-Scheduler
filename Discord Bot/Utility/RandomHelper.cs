using Discord_Bot.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Utility
{
    internal class RandomHelper
    {
        private User[] Players;

        private Random Random;

        public RandomHelper(User[] players)
        {
            this.Players = players;
            Random = new Random();
        }

        public List<(List<User>, List<User>)> calculateTeams()
        {
            // Generate combinations of n/2 sets of people

            List<(List<User>,List<User>)> teams = new List<(List<User>,List<User>)>();
            for(int i = 0; i < Players.Length; i++)
            {
                for(int j = i+1; j < Players.Length; j++)
                {
                    for(int k = j+1; k < Players.Length; k++)
                    {
                        List<User> A_Team = new List<User>();
                        A_Team.Add(Players[i]);
                        A_Team.Add(Players[j]);
                        A_Team.Add(Players[k]);
                        List<User> B_Team = AddOtherTeam(i, j, k);
                        teams.Add((A_Team, B_Team));

                    }
                    
                }
            }

            return teams;
            
             
        }

        public string PrintTeam(List<User> team)
        {
            string players = "";
            for(int i = 0; i < team.Count; i++)
            {
                if (i != team.Count - 1)
                    players += $"{team[i].getName()}, ";
                else
                {
                    players += $"{team[i].getName()}";
                }
            }

            return players;
        }

        private List<User> AddOtherTeam(int i, int j, int k)
        {
            // Calculate the other team

            int idx = 0;
            List<User> B_Team = new List<User>();
            while(idx < Players.Length)
            {
                if (idx != i && idx != j && idx != k)
                {
                    B_Team.Add(Players[idx]);
                    
                }
                idx++;
            }

            return B_Team;
        }

        public (List<User>, List<User>) GetRandomTeam(List<(List<User>, List<User>)> teams)
        {
            //Console.WriteLine(teams.Count);
            int idx = Random.Next(teams.Count); // Generates a random index from 0 to players.Length-1

            return teams[idx];

        }


    }
}
