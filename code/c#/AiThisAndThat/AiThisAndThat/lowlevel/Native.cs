using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AiThisAndThat.lowlevel {
    public class Native {
        const uint PAGE_EXECUTE_READWRITE = 0x40;
        const uint MEM_COMMIT = 0x1000;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        private delegate int IntReturner();

        public static void test() {
            List<byte> bodyBuilder = new List<byte>();
            bodyBuilder.Add(0xb8);
            bodyBuilder.AddRange(BitConverter.GetBytes(42));
            bodyBuilder.Add(0xc3);
            byte[] body = bodyBuilder.ToArray();
            IntPtr buf = VirtualAlloc(IntPtr.Zero, (uint)body.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            Marshal.Copy(body, 0, buf, body.Length);

            IntReturner ptr = (IntReturner)Marshal.GetDelegateForFunctionPointer(buf, typeof(IntReturner));
            Console.WriteLine(ptr());

            int here = 5;

            int n = addInt(5, 7);
        }

        static int addInt(int a, int b) {
            return a + b;
        }
    }
}
