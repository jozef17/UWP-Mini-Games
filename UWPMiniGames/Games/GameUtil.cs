using System.IO;
using Windows.Storage;

namespace UWPMiniGames
{
    public sealed class GameUtil
    {
        public static bool FileExists(string fileName) => File.Exists(ApplicationData.Current.LocalFolder.Path + "\\" + fileName);

        public static void DeleteFile(string fileName)
        {
            if (FileExists(fileName))
                File.Delete(ApplicationData.Current.LocalFolder.Path + "\\" + fileName);
        }

        public static string[] ReadFile(string fileName)
        {
            StreamReader file = new StreamReader(ApplicationData.Current.LocalFolder.Path + "\\" + fileName);
            string text = file.ReadLine();
            string[] state = text.Split(" ");
            file.Close();

            return state;
        }

        public static void SaveFile(string fileName, string content)
        {
            using (StreamWriter file = new StreamWriter(ApplicationData.Current.LocalFolder.Path + "\\" + fileName))
                file.WriteLine(content);
        }

    }
}
