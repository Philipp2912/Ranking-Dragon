using System;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

public class FileHandler
{
    string mainPath = $"C:\\Users\\{Environment.UserName}\\Desktop\\Ranking Dragon";
    private string currentFileOrDict = $"C:\\Users\\{Environment.UserName}\\Desktop\\Ranking Dragon";

    public void Setup() //needed if started for the first time, creates all needed files and folders
    {
        currentFileOrDict = mainPath;
        Console.ForegroundColor = ConsoleColor.Green;
        Directory.CreateDirectory(currentFileOrDict);
        currentFileOrDict = mainPath + "\\currentPrefix.txt";
        File.Create(currentFileOrDict).Dispose();
        StreamWriter writer = new StreamWriter(currentFileOrDict);
        writer.Write("rd!");
        writer.Close();
        Console.WriteLine($"    created {currentFileOrDict} \n");
        currentFileOrDict = mainPath + "\\religions.txt";
        createBasicFile();
        currentFileOrDict = mainPath + "\\staff.txt";
        createBasicFile();
        currentFileOrDict = mainPath + "\\uniqueRoles.txt";
        createBasicFile();
        currentFileOrDict = mainPath + "\\roles.txt";
        createBasicFile();
        currentFileOrDict = mainPath + "\\commandList.txt";
        createBasicFile();
        currentFileOrDict = mainPath + "\\staffCommandList.txt";
        createBasicFile();
        currentFileOrDict = mainPath + "\\staffCommandListOutput.txt";
        createBasicFile();
        currentFileOrDict = mainPath + "\\commandListOutput.txt";
        createBasicFile();
        currentFileOrDict = mainPath + "\\useableChannels.txt";
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"Created all needed Files");
        Console.ForegroundColor = ConsoleColor.White;
    }
    public void checkFiles() //ensures all files and folders exist and replaces missing ones
    {
        Console.ForegroundColor = ConsoleColor.Red;
        currentFileOrDict = mainPath + "\\currentPrefix.txt";
        if (!File.Exists(currentFileOrDict))
        {
            Console.WriteLine($"{currentFileOrDict} not found. Please don't remove any files from the folder!");
            Console.WriteLine($"recreating {currentFileOrDict}/n");
            File.Create(currentFileOrDict).Dispose();
            StreamWriter writer = new StreamWriter(currentFileOrDict);
            writer.Write("rd!");
            writer.Close();
            Console.WriteLine($"recreated {currentFileOrDict}/n");
        }
        currentFileOrDict = mainPath + "\\religions.txt";
        if (!File.Exists(currentFileOrDict))
            wasDeleated();
        currentFileOrDict = mainPath + "\\staff.txt";
        if (!File.Exists(currentFileOrDict))
            wasDeleated();
        currentFileOrDict = mainPath + "\\uniqueRoles.txt";
        if (!File.Exists(currentFileOrDict))
            wasDeleated();
        currentFileOrDict = mainPath + "\\roles.txt";
        if (!File.Exists(currentFileOrDict))
            wasDeleated();
        currentFileOrDict = mainPath + "\\commandList.txt";
        if (!File.Exists(currentFileOrDict))
            wasDeleated();
        currentFileOrDict = mainPath + "\\staffCommandList.txt";
        if (!File.Exists(currentFileOrDict))
            wasDeleated();
        currentFileOrDict = mainPath + "\\commandListOutput.txt";
        if (!File.Exists(currentFileOrDict))
            wasDeleated();
        currentFileOrDict = mainPath + "\\staffCommandListOutput.txt";
        if (!File.Exists(currentFileOrDict))
            wasDeleated();
        currentFileOrDict = mainPath + "\\useableChannels.txt";
        if (!File.Exists(currentFileOrDict))
            wasDeleated();
        Console.ForegroundColor = ConsoleColor.White;
    }

    private void createBasicFile() //for setup
    {
        File.Create(currentFileOrDict).Dispose();
        Console.WriteLine($"    created {currentFileOrDict} \n");
    }

    private void wasDeleated() //for check
    {
        Console.WriteLine($"{currentFileOrDict} not found. Please don't remove any files from the folder!");
        Console.WriteLine($"recreating {currentFileOrDict} \n");
        File.Create(currentFileOrDict).Dispose();
        Console.WriteLine($"recreated {currentFileOrDict} \n");
    }

    public bool userRoleInList(string filename /*the list the user should be in*/, SocketMessage message) //checks if user is allowed to use the command
    {
        //takes riles form the list
        string[] rolesInList = getList(filename);
        /*sees if the user has at one role from the list of roles with the permission to use the command
        by comparing all of his roles with roles from the list */
        foreach (SocketRole role in ((SocketGuildUser)message.Author).Roles)
        {
            foreach (string roleFromList in rolesInList) 
            {
                if (role.ToString() == roleFromList)
                //if one role was matching true gets returned
                    return true; 
            }
        }
        //elswise false gets returned
        return false;
    }

    public string[] getList(string filename) //gets all roles in a file
    {
        int size = 0;
        Console.WriteLine("preparing");
        StreamReader reader = new StreamReader($"{mainPath}\\{filename}");
        Console.WriteLine("trying");
            while (reader.ReadLine() != null)
            {
                size++;
            }
            reader.Close();
            reader = new StreamReader($"{mainPath}\\{filename}");
            string[] list = new string[size];
            for (int i = 0; i < size; i++)
            {
                list[i] = reader.ReadLine();
            }
            reader.Close();
            return list;
    }

    /*a roles classified as a religion is a role that clashes with other roles classified as a religion,
    the name was chosen as it was a DnD roleplay server and that's one of the rules: only one religion at a time*/
    public string getUserReligion(SocketMessage message) //religion of user
    {
        string[] rolesInList = getList("religions.txt");
        //looks at each role of the user and looks for a role listed a religion
        foreach (SocketRole role in ((SocketGuildUser)message.Author).Roles)
        {
            foreach (string roleFromList in rolesInList)
            {
                if (role.ToString() == roleFromList)
                    //returns the religion of the user
                    return role.ToString();
            }
        }
        //that basicly means the user has no religion
        return "";
    }

    public int staffOrUniqueRole(string requestedRole) //0=no restriction 1=unique 2=staff, the command is needed to prevent users from missusing the join command
    {
        //staff roles are roles like Moderator, Admin or simmular roles
        string[] rolesInStaffList = getList("staff.txt");
        foreach (string roleFromStList in rolesInStaffList)
        {
            Console.WriteLine($"checking if {requestedRole} equals {roleFromStList}");
            if (roleFromStList == requestedRole)
                return 2;
        }
        //unique roles are not ment to be owned by everybody so not everybody should be able to get them, they need to be given by an Admin
        string[] rolesInUniqueList = getList("uniqueRoles.txt");
        foreach (string roleFromUnList in rolesInUniqueList)
        {
            Console.WriteLine($"checking if {requestedRole} equals {roleFromUnList}");
            if (roleFromUnList == requestedRole)
                return 1;
        }
        //checks if the role is existing
        string[] defaultRolesInList = getList("roles.txt");
        foreach (string roleFromList in defaultRolesInList)
        {
            Console.WriteLine($"checking if {requestedRole} equals {roleFromList}");
            if (roleFromList == requestedRole)
                return 0;
        }
        //in case the role doesn't exist on any list
        return 3;
    }
    public bool isReligin(string requestedRole) //checks if the roles is classified as a religion
    {
        string[] rolesInStaffList = getList("religions.txt");
        foreach (string roleFromList in rolesInStaffList)
        {
            if (roleFromList == requestedRole)
                return true;
        }
        return false;
    }

    public bool isExistingCommand(string input) //checks if the command exists
    {
        string[] CommandList = getList("commandList.txt");
        foreach (string roleFromList in CommandList)
        {
            if (roleFromList == input)
                return true;
        }
        string[] StaffCommandList = getList("staffCommandList.txt");
        foreach (string roleFromList in StaffCommandList)
        {
            if (roleFromList == input)
                return true;
        }
        return false;
    }

    public void prefixUpdate(string prefix) //used to change the keyword for the bot
    {
        string newFile = mainPath + "\\currentPrefix.txt";
        File.Create(newFile).Dispose();
        StreamWriter writer = new StreamWriter(newFile);
        writer.Write(prefix);
        writer.Close();
        Console.WriteLine($"Changed prefix to {prefix}");
    }

    public void editFileAdd(string fileName, string role) //adds a role to its respective file
    {
        File.AppendAllText(mainPath+"\\"+fileName, Environment.NewLine + role);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(role + " added");
        Console.ForegroundColor = ConsoleColor.White;
    }

    public void editFileRemove(string fileName, string role) //removes a role to its respective file
    {
        string[] roles = getList(fileName);
        File.Create(mainPath + "\\" + fileName).Dispose();
        StreamWriter writer = new StreamWriter(mainPath + "\\" + fileName);
        //writes all roles appart from the role that needs to be removed to the file
        for(int i=0; i<roles.Length;i++)
        {
            if (roles[i] != role)
                writer.WriteLine(roles[i]);
        }
        writer.Close();
        //a little feedback
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(role + " removed");
        Console.ForegroundColor = ConsoleColor.White;
    }
    public bool chanelIsUsable(SocketMessage message) //checks if the bot is allowed to opperate in the channel
    {
        string [] list=getList("useableChannels.txt");
        for(int i=0; i<list.Length;i++)
        {
            if (list[i] == message.Channel.ToString())
            {
                return true;
            }
        }
        return false;
    }
}