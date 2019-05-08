using System;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

/*
RankingDragon ist ein Discordbot, der auf discord-net basiert. discord-net ist ein inoffiziellen .Net wrapper für die Discord API, da die offizielle API 
für normale Nutzer nicht zugänglich ist. ( https://github.com/discord-net/Discord.Net )

Der Bot wurde für einen DnD-Rollenspiel-Server eines Freundes programmiert und ist dafür verantwortlich, dass sich die Nutzer selbständig Rollen zuweisen 
können um Admins und Moderatoren zu entlasten.
Er ist einsatzbereit und Funktioniert problemlos, auch wenn noch ein paar Zeilen fehlen. Bislang erstellt er zwar die benötigten Ordner und Datein selbstständig, 
diese sind allerdings leer, die Kommandoliste und die Liste an Rollen mit Moderationsrechten ect, diese sachen müssen noch von Hand
ausgefüllt werden, was allerdings leicht zu beheben ist.

Vor dem hochladen habe ich den Token entfernt, den der Bot benötigt um online zu gehen, an entsprechender stelle gibt es allerdings eine kleine beschreibung 
woher man einen neuen Bot bei Diskord registriert und wo der zum Bot gehörige token zu finden ist.
*/

namespace RankingDragons
{
    class RankingDragon
    {
        // prefix is needed to adress the bot
        string prefix; //gets drawn from file
        //commands for normal users
        string join = "join"; //makes a user join a role
        string remove = "leave"; //makes a user leave a role
        string help = "help"; //displays all commands normal users need
        string roleList = "roles"; //displays all publicly avavible roles
        string staffCommandList = "staffCommandList"; //displays commands for staff members
        //beginning of staff commands
        string changePrefix = "changePrefix"; //changes the prefix
        string removeFromReligion = "removeReligion"; //removes religion from list
        string addToReligion = "addReligion"; //adds see above
        string removeFromStaff = "removeStaff"; //removes staff from List
        string addToStaff = "addStaff"; //adds see above
        string removeFromRoles = "removeRole"; //removes roles from list
        string addToRoles = "addRole"; //adds see above
        string addToUniqueRoles = "addUniqueRole"; //adds unique role
        string removeFromUniqueRoles = "removeUniqueRole"; //removes see above
        string removeChannel = "removeChannel"; //removes a channel from the lists of channels the bot is allowed to opperate in
        string addChannel = "addChannel"; //adds a channel to the lists of channels the bot is allowed to opperate in

        FileHandler handler = new FileHandler();
        int pos = 0;
        internal static DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig
        {
            WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance
        });

        static void Main(string[] args)
            => new RankingDragon().MainAsync().GetAwaiter().GetResult(); //gets the bot started

        public async Task MainAsync()
        {
            Console.ForegroundColor = ConsoleColor.White; 
            //checks if the bot is running on a pc for the first time
            if (!Directory.Exists($"C:\\Users\\{Environment.UserName}\\Desktop\\Ranking Dragon"))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("prepearing workspace");
                handler.Setup(); //creates all needed folders and files
            }
            handler.checkFiles();
            client.Log += LogAsync;
            client.Ready += ReadyAsync;
            client.MessageReceived += MessageReceivedAsync;
            await client.LoginAsync(TokenType.Bot, "" 
            /*
                the bot token got removed, you can create a new application at https://discordapp.com/developers/applications/
                and get a new token from there under Bots, make sure to allow it to edit roles*/);
            await client.StartAsync();
            prefix = handler.getList("currentPrefix.txt")[0] + " ";
            await client.SetGameAsync(prefix + "help");
            await Task.Delay(-1);

        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync() //showing that the bot started
        {
            Console.WriteLine($"Bot started!");

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (Prefixcheck(message.Content)) //controlles if the message is adressed at the bot
            {
                string output = "";
                 // ensures that the bot doesn't trigger itself by naming a role [prefix] roles
                if (message.Author.Id == client.CurrentUser.Id)
                    return;
                string input = commandCut(message.Content);
                if (handler.userRoleInList("staff.txt", message))
                {
                    if (input == addChannel)
                    {
                        //adds the channel named in the command to the list of usable channels
                        handler.editFileAdd("useableChannels.txt", message.Channel.ToString());
                        await message.Channel.SendMessageAsync($"Added {message.Channel.ToString()} to watchlist.");
                    }
                }
                //checkes if it is allowed to opperate in the current channel
                if (handler.chanelIsUsable(message))
                {
                    //put commands in here
                    if (input == join)
                    {

                        await joinRank(cutAddition(message.Content), message);
                    }

                    else if (input == remove)
                    {
                        await leaveRank(cutAddition(message.Content), message);
                    }
                    else if (input == help)
                    {
                        await message.Channel.SendMessageAsync("Commands are:");
                        string[] commands = handler.getList("commandListOutput.txt");
                        for (int i = 0; i < commands.Length; i++)
                        {
                            output += commands[i] + Environment.NewLine;
                        }
                        await message.Channel.SendMessageAsync(output);
                    }
                    else if (input == roleList)
                    {
                        string[] roles = handler.getList("roles.txt");
                        List<string> list = new List<string>();
                        for (int i = 0; i < roles.Length; i++)
                            list.Add(new string(stringToChar(roles[i])));
                        list.Sort();
                        output = "roles:" + Environment.NewLine;
                        for (int i = 0; i < roles.Length; i++)
                        {
                            output += list[i] + Environment.NewLine;
                        }
                        await message.Channel.SendMessageAsync(output);
                        output = Environment.NewLine + "Religions are:" + Environment.NewLine;
                        string[] relRoles = handler.getList("religions.txt");
                        List<string> listRel = new List<string>();
                        listRel.Sort();
                        for (int i = 0; i < relRoles.Length; i++)
                            listRel.Add(new string(stringToChar(relRoles[i])));
                        listRel.Sort();
                        for (int i = 0; i < relRoles.Length; i++)
                        {
                            output += listRel[i] + Environment.NewLine;
                        }
                        await message.Channel.SendMessageAsync(output);
                    }
                    else if (input == staffCommandList)
                    {
                        await message.Channel.SendMessageAsync("Commands for staff are:");
                        string[] commands = handler.getList("staffCommandListOutput.txt");
                        for (int i = 0; i < commands.Length; i++)
                        {
                            output += commands[i] + Environment.NewLine;
                        }
                        await message.Channel.SendMessageAsync(output);
                    }
                    else if (handler.isExistingCommand(input))
                    {
                        //staff commands
                        if (handler.userRoleInList("staff.txt", message))  //editors are users that are allowed to add and remove roles, mainly Admins
                        {
                            if (input == changePrefix)
                            {
                                prefix = cutAddition(message.Content) + " ";
                                await client.SetGameAsync(prefix + "help");
                                await message.Channel.SendMessageAsync("Successfully set prefix to " + prefix + ".");
                                handler.prefixUpdate(prefix);
                            }
                            else if (input == addToReligion)
                            {
                                handler.editFileAdd("religions.txt", cutAddition(message.Content));
                                await message.Channel.SendMessageAsync($"added rank");
                            }
                            else if (input == removeFromReligion)
                            {
                                handler.editFileRemove("religions.txt", cutAddition(message.Content));
                                await message.Channel.SendMessageAsync($"removed rank");
                            }
                            else if (input == addToStaff)
                            {
                                handler.editFileAdd("staff.txt", cutAddition(message.Content));
                                await message.Channel.SendMessageAsync($"added rank");
                            }
                            else if (input == removeFromStaff)
                            {
                                handler.editFileRemove("staff.txt", cutAddition(message.Content));
                                await message.Channel.SendMessageAsync($"removed rank");
                            }
                            else if (input == addToRoles)
                            {
                                handler.editFileAdd("roles.txt", cutAddition(message.Content));
                                await message.Channel.SendMessageAsync($"added rank");
                            }
                            else if (input == removeFromRoles)
                            {
                                handler.editFileRemove("roles.txt", cutAddition(message.Content));
                                await message.Channel.SendMessageAsync($"removed rank");
                            }
                            else if (input == addToUniqueRoles)
                            {
                                handler.editFileAdd("uniqueRoles.txt", cutAddition(message.Content));
                                await message.Channel.SendMessageAsync($"added rank");
                            }
                            else if (input == removeFromUniqueRoles)
                            {
                                handler.editFileRemove("uniqueRoles.txt", cutAddition(message.Content));
                                await message.Channel.SendMessageAsync($"added rank");
                            }
                            else if (input == removeChannel)
                            {
                                handler.editFileRemove("useableChannels.txt", message.Channel.ToString());
                                await message.Channel.SendMessageAsync($"Removed {message.Channel.ToString()} from watchlist.");
                            }
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync("You are not authorized to use this command.");
                        }
                    }
                    else
                        await message.Channel.SendMessageAsync($"I'm sorry, but {message.Content} doesn't seem to exist. To see a list of all commands use {prefix} {help} or {prefix} {staffCommandList}.");

                }
            }
        }

        // command methodes
        private async Task joinRank(string roleName, SocketMessage message) //may need to be edited
        {
            var user = message.Author;
            try
            {
                if (handler.isReligin(roleName)) //kicks in if the requested role is classified as a religion (only one religion at a time)
                {
                    var role = (message.Author as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name.ToString() == roleName);
                    //checks if the user is in a religion
                    if (handler.userRoleInList("religions.txt", message))
                    {
                        //removes the old religion
                        await leaveRank(handler.getUserReligion(message), message);
                        //adds new religion
                        await (message.Author as IGuildUser).AddRoleAsync(role);
                        await message.Channel.SendMessageAsync($"added {role} to {user}");
                    }
                    else //simply add the roles
                    {
                        await (message.Author as IGuildUser).AddRoleAsync(role);
                        await message.Channel.SendMessageAsync($"added {role} to {user}");
                    }
                }
                else
                {
                    int check = handler.staffOrUniqueRole(roleName); //looks if the user is allowed to get the role
                    if (check == 2)
                    {
                        await message.Channel.SendMessageAsync($"Requested role {roleName} is a staff role and needs to be added by a staff member!");
                    }
                    else if (check == 1)
                    {
                        await message.Channel.SendMessageAsync($"Requested role {roleName} is a unique or otherwise restricted role and needs to be added by a staff member!");
                    }
                    else if (check == 0)
                    {
                        var role = (message.Author as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name.ToString() == roleName);
                        await (message.Author as IGuildUser).AddRoleAsync(role); /*not working! Nilschwein <-- it is working, i just left it in as a refference to a friend, who tought me to
                        mark things that aren't working propperly so i don't have to search them after taking a break*/
                        await message.Channel.SendMessageAsync($"added {role} to {user}");
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync($"Requested role {roleName} isn't listed. Did you misspell it or isn't it a part of the role list? If that's the case contact a staff member. {Environment.NewLine} Use {prefix}{roleList} to see all available ranks.");
                    }
                }
            }
            catch (Exception e) //just in case something went wrong
            {
                Console.WriteLine(e.StackTrace);
                //again, DnD roleplay, I thought it was a nice way to let the user know that something went wrong.
                await message.Channel.SendMessageAsync($"I'm sorry, but it seems as if a wild horde of goblins stole your message. Could you please send it again? Or tell somebody about it in case it happens again.");
            }
        }
        //makes a user leave a role
        private async Task leaveRank(string roleName, SocketMessage message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"the input is: {roleName}");
            var user = message.Author;
            Console.WriteLine($"trying to grab role");
            var role = (message.Author as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name.ToString() == roleName);
            Console.WriteLine($"grabed role");
            await (message.Author as IGuildUser).RemoveRoleAsync(role);
            await message.Channel.SendMessageAsync($"removed {role} from {user}");
            Console.ForegroundColor = ConsoleColor.White;
        }


        //return methodes

        private string commandCut(string input) //returns command
        {
            string output = "";
            //jumps past the keyword
            pos = prefix.Length;
            //the command is either as long as the message or one word so a space ends it
            while (pos < input.Length && input[pos] != ' ')
            {
                output += input[pos];
                pos++;
                Console.WriteLine($"output is {output}");
            }
            //pos doesn't get reset as it is needed in cutAddition
            pos++;
            return output;
        }

        private string cutAddition(string input) //gets rank
        {
            Console.WriteLine($"cuttin' rank");
            string output = "";
            //pos comes from commandCut, it just starts at whatever comes after the command
            while (pos < input.Length)
            {
                Console.WriteLine($"at step {pos} of {input.Length}");
                output += input[pos];
                pos++;
                Console.WriteLine($"output is {output}");
            }
            Console.WriteLine($"output is \"{output}\"");
            return output;
        }

        private bool Prefixcheck(string input) //checks if it applys to the bot
        {
            Console.WriteLine($"Prefixcheck {prefix} {input}");
            string messageBeginning = "";
            int prfPos = 0;
            while (prfPos < prefix.Length && prefix.Length <= input.Length)
            {
                messageBeginning += input[prfPos];
                prfPos++;
            }
            if (messageBeginning == prefix)
            {
                //if the message starts with the prefix it returns true
                return true;
            }
            //otherwise it returns false
            return false;
        }

        char[] stringToChar(string toConvert) //convertes a String into a char array
        {
            char[] returnChar = new char[toConvert.Length];
            for (int i = 0; i < toConvert.Length; i++)
            {
                returnChar[i] = toConvert[i];
            }
            return returnChar;
        }
    }
}