using System;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace gunmetal_fix
{
    static class Program
    {
        const string TITLE = "Gun Metal Fix";
        const string FILE_CFG = "gunmetal_fix.ini";
        const string GAME_EXE = "gundx.exe";
        const string MODULE_NAME = "KERNELBASE.dll";

        static MemoryEdit.Memory mem = new MemoryEdit.Memory();
        static byte[] asm_limit = { 0x6A, 0x10, 0x90 }; //push 10; nop
        //static byte[] asm_orig = { 0xFF, 0x75, 0x08 }; //push [ebp+08]

        static void Main(string[] args)
        {
            byte sleep_val = 16;
            try
            {
                using (StreamReader sr = new StreamReader(FILE_CFG, Encoding.Default))
                {
                    sleep_val = byte.Parse(sr.ReadLine());
                }
            }
            catch { }
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = GAME_EXE;
            psi.Arguments = string.Join(" ", args);
            Process proc = Process.Start(psi);
            mem.Attach(proc, MemoryEdit.Memory.ProcessAccessFlags.All);
            asm_limit[1] = sleep_val;
            IntPtr kernel_module = mem.GetModule(MODULE_NAME);
            uint address = (uint)MemoryEdit.Memory.GetProcAddress((IntPtr)kernel_module, "Sleep") + 0x07;
            uint oldproct;
            mem.SetProtection(address, 0x100, MemoryEdit.Memory.Protection.PAGE_EXECUTE_READWRITE, out oldproct);
            mem.WriteBytes(address, asm_limit, 3);
            mem.SetProtection(address, 0x100, (MemoryEdit.Memory.Protection)oldproct, out oldproct);
            mem.Detach();
        }
    }
}
