using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ASM.Data.ORM.Dapper;
using System.IO;
using System.Web.Script.Serialization;

namespace DposTask
{
    public class Addr
    {
        /// <summary>
        /// id 自动增加
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 钱包地址
        /// </summary>
        public string addr { get; set; }
        /// <summary>
        /// 是否使用
        /// </summary>
        public bool is_use { get; set; }
        /// <summary>
        /// 主人
        /// </summary>
        public string master { get; set; }
    }

    public class Pool
    {
        /// <summary>
        /// 矿池地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 矿池名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 出块次数
        /// </summary>
        public int c { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Format("name:#####;addr:{0};次数:{1}", address, c);
            }
            else
            {
                return string.Format("name{0};addr:{1};次数:{2}", name, address, c);
            }
        }
    }

    public class Inspect
    {
        static readonly string connStr = ConfigurationManager.AppSettings["connStr"];

        private void AddAddr(string addr_)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                string sql = "Insert Addr(addr) values(@addr)";
                conn.Execute(sql, new { addr = addr_ });
            }
        }

        /// <summary>
        /// 近n个块，矿池出块分布
        /// </summary>
        public void BlockOut(int C)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                string sql = "SELECT * from Block where is_useful = 1 ORDER BY id desc LIMIT @c";
                List<Block> blocks = conn.Query<Block>(sql, new { c = C }).ToList();
                Dictionary<string, Pool> dic = new Dictionary<string, Pool>();
                foreach (Block b in blocks)
                {
                    if (dic.ContainsKey(b.reward_address))
                    {
                        dic[b.reward_address].c++;
                    }
                    else
                    {
                        Pool p = new Pool();
                        p.address = b.reward_address;
                        p.c = 1;
                        dic.Add(b.reward_address, p);
                    }
                }
                sql = "SELECT address,`name` from Pool";
                List<Pool> pools = conn.Query<Pool>(sql, new { c = 2000 }).ToList();
                foreach (Pool p in pools)
                {
                    if (dic.ContainsKey(p.address))
                    {
                        dic[p.address].name = p.name;
                    }
                }
                foreach (string key in dic.Keys)
                {
                    if (!string.IsNullOrEmpty(dic[key].name))
                    {
                        if (dic[key].name.Substring(0, 4) == "dpos")
                        {
                            Console.WriteLine(dic[key].ToString());
                        }
                    }
                }
                Console.WriteLine("===============================");
                foreach (string key in dic.Keys)
                {
                    if (!string.IsNullOrEmpty(dic[key].name))
                    {
                        if (dic[key].name.Substring(0, 4) != "dpos")
                        {
                            Console.WriteLine(dic[key].ToString());
                        }
                    }
                }
                Console.WriteLine("===============================");
                foreach (string key in dic.Keys)
                {
                    if (string.IsNullOrEmpty(dic[key].name))
                    {
                        Console.WriteLine(dic[key].ToString());
                    }
                }
                Console.WriteLine("===============================");
            }
        }

        class BlockHash
        {
            public string h1 { get; set; }
            public string h2 { get; set; }
            public decimal reward_money { get; set; }
        }

        /// <summary>
        /// 关于矿池收益分发的情况
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="dt"></param>
        public void TxCheck(string addr, DateTime dt)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                string sql = "select * from Addr";
                List<Addr> addrs = conn.Query<Addr>(sql, new { }).ToList();
                Dictionary<string, Addr> dic = new Dictionary<string, Addr>();
                foreach (Addr obj in addrs)
                {
                    dic.Add(obj.addr, obj);
                }
                sql = "SELECT SUM(reward_money) as reward_money from Block where time >= UNIX_TIMESTAMP(@begin) and time < UNIX_TIMESTAMP(@end) and reward_address = @addr and is_useful = 1";
                decimal m = conn.Query<BlockHash>(sql, new { addr = addr, begin = dt.AddDays(-1).ToShortDateString(), end = dt.ToShortDateString() }).Single().reward_money;
                sql = "SELECT MAX(`hash`) as h1, MIN(`hash`) as h2 from Block where time >= UNIX_TIMESTAMP(@begin) and time < UNIX_TIMESTAMP(@end) and is_useful = 1";
                BlockHash bs = conn.Query<BlockHash>(sql, new { begin = dt.ToShortDateString(), end = dt.AddDays(1).ToShortDateString() }).Single();
                sql = "SELECT `to`,SUM(amount) as amount from Tx where form = @addr and block_hash <= @hash_max and block_hash >= @hash_min GROUP BY `to`";

                List<Tx> txs = conn.Query<Tx>(sql, new { addr = addr, hash_max = bs.h1, hash_min = bs.h2 }).ToList();
                Dictionary<string, decimal> dic2 = new Dictionary<string, decimal>();

                foreach (Tx tx in txs)
                {
                    if (tx.to == addr)
                    {
                        continue;
                    }
                    if (dic.ContainsKey(tx.to))
                    {
                        if (dic2.ContainsKey(dic[tx.to].master))
                        {
                            dic2[dic[tx.to].master] += tx.amount;
                        }
                        else
                        {
                            dic2.Add(dic[tx.to].master, tx.amount);
                        }
                    }
                    else
                    {
                        string key = "";
                        if (tx.to == addr)
                        {
                            key = "pool";
                        }
                        else
                        {
                            key = "err";
                            Console.WriteLine(tx.to);
                        }
                        if (dic2.ContainsKey(key))
                        {
                            dic2[key] += tx.amount;
                        }
                        else
                        {
                            dic2.Add(key, tx.amount);
                        }
                    }
                }
                Console.WriteLine("===============================");
                foreach (string key in dic2.Keys)
                {
                    Console.WriteLine(key + " 收到:" + dic2[key]);
                }
                Console.WriteLine("总金额:" + m);
            }
        }

        public class AddrWeight
        {
            /// <summary>
            /// 地址
            /// </summary>
            public string addr { get; set; }
            /// <summary>
            /// 权重
            /// </summary>
            public int weight { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public void Dpos(DateTime dt)
        {
            //PockMine节点：1g7b465meqgafj2d3scd9jksr8d9qqmh4ww2a60btv9hb56tyj9d6cevc
            //BBCNET节点：1g03a0775sbarkrazjrs7qymdepbkn3brn7375p7ysf0tnrcx408pj03n
            //BBC Russia节点：1w704220m8jc4mqx6czrqsc6a3tdztbbz3s4dkbz05mkwrbj7a41me6df
            //BBC基金会节点：18f2dv1vc6nv2xj7ak0e0yye4tx77205f5j73ep2a7a5w6szhjexkd5mj
            //PockChains节点：17k4bv0dw8q4p0hx69hx9pgg5bwxqdandwegh8rry4zrcezjpyv1zrvff
            //BBC Stake节点：1t877w7b61wsx1rabkd69dbn2kgybpj4ayw2eycezg8qkyfekn97hrmgy
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("20m0bte7qgzgbhdep552kw4at43cjcmvsdgmqkcw0rpphr46f9bezj6am", "1g7b465meqgafj2d3scd9jksr8d9qqmh4ww2a60btv9hb56tyj9d6cevc");
            dic.Add("20m02ah2kd2aa7948cc160893xaaahf6rtz3cawj3mp4m6y9ra9en9y58", "1g03a0775sbarkrazjrs7qymdepbkn3brn7375p7ysf0tnrcx408pj03n");
            dic.Add("20m01gbqzmw3nvndx684nsxp1z3tcy4rg0715tgc9sraxd7m56ydttama", "1w704220m8jc4mqx6czrqsc6a3tdztbbz3s4dkbz05mkwrbj7a41me6df");
            dic.Add("20m04mdaq57pvvtg2f0y7q2gb8f973h231gv3mhn3nrfzgtk5pkd1m2c7", "18f2dv1vc6nv2xj7ak0e0yye4tx77205f5j73ep2a7a5w6szhjexkd5mj");
            dic.Add("20m0emvkr82b7qn8hq2fc8tt2135tph52ghnkqs8ggm06qfn42d6zxheq", "17k4bv0dw8q4p0hx69hx9pgg5bwxqdandwegh8rry4zrcezjpyv1zrvff");
            dic.Add("20m03w2c5xhphzfq7fqzh8qfgpdgsn86dzzdrhxb613ar2frg5y71t2yx", "1t877w7b61wsx1rabkd69dbn2kgybpj4ayw2eycezg8qkyfekn97hrmgy");
            StreamWriter sw = new StreamWriter("./Dpos.log");
            using (var conn = new MySqlConnection(connStr))
            {
                string sql = "SELECT MAX(`hash`) as h1, MIN(`hash`) as h2 from Block where time >= UNIX_TIMESTAMP(@begin) and time<UNIX_TIMESTAMP(@end) and is_useful = 1";
                BlockHash bs = conn.Query<BlockHash>(sql, new { begin = dt.AddDays(1).ToShortDateString(), end = dt.AddDays(2).ToShortDateString() }).Single();
                sql = "SELECT * from DposPayment where payment_date = @date";
                List<DposPayment> dps = conn.Query<DposPayment>(sql, new { date = dt }).ToList();
                foreach (DposPayment obj in dps)
                {
                    sql = "SELECT SUM(amount) as amount,max(txid) as txid from Tx where form = @f and `to` = @t and block_hash <= @hash_max and block_hash >= @hash_min ";
                    Tx tx = conn.Query<Tx>(sql, new { f = dic[obj.dpos_addr], t = obj.client_addr, hash_max = bs.h1, hash_min = bs.h2 }).Single();

                    string info = string.Format("txid:{0};{1}:{2}", tx.txid, tx.amount, obj.payment_money);
                    if (tx.amount > obj.payment_money * 0.95m)
                    {
                        if ((tx.amount - obj.payment_money) > 1.0m)
                        {
                            info = "err-->>" + info;
                            sw.WriteLine(info);
                            Console.WriteLine(info);
                        }
                        else
                        {
                            info = "warning-->>" + info;
                            sw.WriteLine(info);
                            Console.WriteLine(info);
                        }
                    }
                    else
                    {
                        info = "OK-->>" + info;
                        sw.WriteLine(info);
                        Console.WriteLine(info);
                    }
                }
            }
            sw.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        public void MKF()
        {
            // 
            using (var conn = new MySqlConnection(connStr))
            {
                string sql = "SELECT* from addr LIMIT @c";
                Random rand = new Random();
                List<Addr> addrs = conn.Query<Addr>(sql, new { c = 600 }).ToList();
                List<AddrWeight> list = new List<AddrWeight>();
                foreach (Addr  addr in addrs)
                {
                    if (rand.Next() % 7 != 1)
                    {
                        AddrWeight aw = new AddrWeight();
                        aw.addr = addr.addr;
                        aw.weight = 20 + rand.Next() % 20;
                        list.Add(aw);
                    }
                }
                JavaScriptSerializer js = new JavaScriptSerializer();
                string json = js.Serialize(list);
                File.Delete("./json.txt");
                using (FileStream fs = new FileStream("./json.txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                {
                    byte[] bs = Encoding.UTF8.GetBytes(json);
                    fs.Write(bs, 0, bs.Length);
                    fs.Close();
                }
                Console.WriteLine("OK");
            }
        }
    }
}