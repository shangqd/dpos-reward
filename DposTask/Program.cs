using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASM.Data.ORM.Dapper;
using MySql.Data.MySqlClient;
using Rebex.Security.Cryptography;

namespace DposTask
{
    // sudo apt install mono-devel
    class Program
    {
        static void TestTokenDistribution()
        {
            TokenDistribution obj = new TokenDistribution();
            System.Diagnostics.Debug.Assert(obj.GetTotal(1) == 1153);
            System.Diagnostics.Debug.Assert(obj.GetTotal(2) == 1153 * 2);
        }

        static void TestEd25519()
        {
            Ed25519 obj = new Ed25519();
            // 前32个字节为0,后32个字节为真正的私钥
            var key = new byte[64];
            obj.FromPrivateKey(key);
            byte[] message = new byte[10];
            var sig = obj.SignMessage(message);
            bool b = obj.VerifyMessage(message,sig);
            System.Console.WriteLine(b);
        }

        static void Main(string[] args)
        {
            DposTask ds = new DposTask();
            int id = ds.Init();
            if (id < 243800)
            {
                id = 243800;
            }
            ds.Run(id);
        }
    }
}
