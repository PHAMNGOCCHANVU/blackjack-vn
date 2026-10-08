using System;
using System.Windows.Forms;
using XiDachVN.Forms;
using XiDachVN.Tests;

namespace XiDachVN
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--test")
            {
                LogicVerifier.RunAllTests();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
