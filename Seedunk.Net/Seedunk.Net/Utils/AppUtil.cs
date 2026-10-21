
namespace Seedunk.Net.Utils
{
    public class AppUtil
    {

        public static bool IsRunning
        {

            get
            {
                string MName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;
                string PName = System.IO.Path.GetFileNameWithoutExtension(MName);
                System.Diagnostics.Process[] myProcess = System.Diagnostics.Process.GetProcessesByName(PName);
                return myProcess.Length > 1;

            }

        }




    }
}
