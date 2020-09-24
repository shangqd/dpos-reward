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

        /// <summary>
        /// BBC 常规检查
        /// </summary>
        static void BBC_Check()
        {
            Inspect obj = new Inspect();
            obj.BlockOut(3000);

            DateTime dt = new DateTime(2020, 9, 14);
            for (int i = 0; i < 10; i++)
            {
                string addr3 = "20g0epy7jerpbc542a15f99b00mzvex4g3rrkj04crdgzb30b7bp9ncfj";
                string addr4 = "20g096rdj9xmmw284fr5vze52p8fstc71bjhzkrjmssbz9hnjkechjnqv";

                Console.WriteLine(dt.AddDays(i).ToLongDateString());
                obj.TxCheck(addr3, dt.AddDays(i));
                obj.TxCheck(addr4, dt);
            }
        }

        /// <summary>
        ///  导出要发放的MKF地址和权重
        /// </summary>
        static void ExportJSON()
        {
            Inspect obj = new Inspect();
            obj.MKF();
        }

        static void Dpos()
        {
            Inspect obj = new Inspect();
            DateTime dt = new DateTime(2020, 9, 22);
            obj.Dpos(dt);
        }


        static void BBC_Sync()
        {
            DposTask ds = new DposTask();
            int id = ds.Init();
            if (id < 243800)
            {
                id = 243800;
            }
            ds.Run(id);
        }
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                BBC_Sync();
            }
            else
            {
                if (args[0] == "dpos")
                {
                    Dpos();
                }
                if (args[0] == "json")
                {
                    ExportJSON();
                }
                if (args[0] == "check")
                {
                    BBC_Check();
                }
            }
        }
    }
}
