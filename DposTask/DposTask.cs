using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASM.Data.ORM.Dapper;
using MySql.Data.MySqlClient;

namespace DposTask
{
    /// <summary>
    /// dpos 投票的状态，每天做一个备份，为了防止回滚最后的状态与最高的块相差30
    /// </summary>
    public class DposState
    {
        public int id { get; set; }
        public string dpos_addr { get; set; }
        public string client_addr { get; set; }
        public DateTime audit_date { get; set; }
        public decimal audit_money { get; set; }

    }

    /// <summary>
    /// 要支付给投票者的金额，每天做一个结算
    /// </summary>
    public class DposPayment
    {
        public int id { get; set; }
        public string dpos_addr { get; set; }
        public string client_addr { get; set; }
        public DateTime payment_date { get; set; }
        public decimal payment_money { get; set; }

    }

    public class Block
    {
        public int id { get; set; }
        public string hash { get; set; }
        public string reward_address { get; set; }
        public decimal reward_money { get; set; }
        public uint height { get; set; }
        public uint time { get; set; }
        public string type { get; set; }
    }

    public class Tx
    {
        public string txid { get; set; }
        public string dpos_in { get; set; }
        public string client_in { get; set; }
        public string dpos_out { get; set; }
        public string client_out { get; set; }
        public decimal amount { get; set; }
        public decimal free { get; set; }
        public string form { get; set; }
        public string to { get; set; }
        public string type { get; set; }
    }

    public class VoteReward
    {
        public VoteReward(decimal vote_, decimal reward_)
        {
            vote = vote_;
            reward = reward_;
        }

        /// <summary>
        ///  投票的金额
        /// </summary>
        public decimal vote { get; set; }

        /// <summary>
        /// 奖励金额
        /// </summary>
        public decimal reward { get; set; }
    }

    public class DposTask
    {
        /// <summary>
        /// 区块链开始时间
        /// </summary>
        static readonly DateTime BeginDate = new DateTime(2020, 3, 10, 0, 0, 0, 0);
        /// <summary>
        /// 
        /// </summary>
        static readonly string connStr = ConfigurationManager.AppSettings["connStr"];
        /// <summary>
        /// dpos 地址前缀
        /// </summary>
        static readonly string DposPrefix = "20m0";
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, Dictionary<string, VoteReward>> m_vote_reward;
        /// <summary>
        /// 最后的utc时间
        /// </summary>
        private uint m_last_utc;
        public DposTask()
        {
            m_vote_reward = new Dictionary<string, Dictionary<string, VoteReward>>();
            m_last_utc = 0;
        }
        bool IsSave(long a, long b)
        {
            DateTime d_a = ToDateTime(a);
            DateTime d_b = ToDateTime(b);
            if (d_a.Year == d_b.Year &&
                d_a.Month == d_b.Month &&
                d_a.Day == d_b.Day)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        DateTime ToDateTime(long TimeStamps)
        {
            var now = DateTime.Now;
            var timezone = now - now.ToUniversalTime();
            var date = new DateTime(1970, 1, 1, timezone.Hours, 0, 0).AddSeconds(TimeStamps);
            return new DateTime(date.Year,date.Month,date.Day);
        }

        uint GetTimestamp(DateTime dateTime)
        {
            var now = DateTime.Now;
            var timezone = now - now.ToUniversalTime();
            DateTime dt = new DateTime(1970, 1, 1, timezone.Hours, 0, 0, 0);
            return (uint)(dateTime - dt).TotalSeconds;
        }

        List<DposState> GetDposState(DateTime audit_date)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                return conn.Query<DposState>("select * from DposState where audit_date=@audit_date", new { audit_date = audit_date }).ToList();
            }
        }

        List<DposState> GetDposStateLast()
        {
            using (var conn = new MySqlConnection(connStr))
            {
                return conn.Query<DposState>("SELECT * from DposState ORDER BY id desc LIMIT 1", null).ToList();
            }
        }

        int GetBlockIdByTime(long time)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                return conn.Query<Block>("SELECT * FROM `Block` where time < @time order by id desc LIMIT 1",new { time = time }).Single().id;
            }
        }

        List<Block> GetBlock(int id)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                return conn.Query<Block>("SELECT * FROM Block  where id > @id", new { id = id }).ToList();
            }
        }

        public uint GetLastHeight()
        {
            using (var conn = new MySqlConnection(connStr))
            {
                string sql = "SELECT ifnull(MAX(height),0) as height FROM Block";
                return conn.Query<Block>(sql, null).Single().height;
            }
        }

        List<Tx> GetTx(string hash_id)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                return conn.Query<Tx>("select * from Tx where type in ('token','stake') and block_hash = @block_hash", new { block_hash = hash_id }).ToList();
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        public int Init()
        {
            /*
            List<DposState> state = GetDposStateLast();
            if (state.Count == 1)
            {
                // 清理上次运行不彻底的数据
                string sql = "delete from DposState where audit_date = @audit_date";
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Execute(sql, new { audit_date = state[0].audit_date });
                }
            }*/
            return Load();
        }

        /// <summary>
        /// 清理10天前的数据
        /// </summary>
        /// <param name="last_utc"></param>
        void ClearState(long last_utc)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                var dt = ToDateTime(last_utc);
                dt = dt.AddDays(-10);
                string sql = "delete from DposState where audit_date = @audit_date";
                conn.Execute(sql, new { audit_date = dt });
                //sql = "delete from DposPayment where payment_date = @payment_date";
                //conn.Execute(sql, new { payment_date = dt });
            }
        }

        int Load()
        {
            List<DposState> states = GetDposStateLast();
            if (states.Count == 1)
            {
                states = GetDposState(states[0].audit_date);
                m_last_utc = GetTimestamp(states[0].audit_date) + 3600 * 24;
                foreach (var state in states)
                {
                    if (!m_vote_reward.ContainsKey(state.dpos_addr))
                    {
                        m_vote_reward.Add(state.dpos_addr, new Dictionary<string, VoteReward>());
                    }
                    m_vote_reward[state.dpos_addr].Add(state.client_addr, new VoteReward(state.audit_money, 0));
                }
                return GetBlockIdByTime(m_last_utc);
            }
            else
            {
                m_last_utc = GetTimestamp(BeginDate);
                return 0;
            }
        }

        bool IsDposAddr(string addr)
        {
            if (addr.Substring(0, 4) == DposPrefix)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Run(int id)
        {
            while (true)
            {
                List<Block> blocks = GetBlock(id);
                Console.WriteLine(string.Format("{0} - plan:id:{1},height:{2} -->> id:{3}.heigh:{4}",
                    DateTime.Now.ToShortTimeString(), id, blocks[0].height, id + blocks.Count, blocks[blocks.Count - 1].height));
                foreach (var block in blocks)
                {
                    string info = string.Format("{0} - run:id:{1},height:{2},date:{3},type:{4}", 
                        DateTime.Now.ToShortTimeString(), block.id, block.height, ToDateTime(block.time).ToShortDateString(), block.type);
                    Console.WriteLine(info);
                    uint last_height = GetLastHeight();
                    while (last_height < block.height + 30)
                    {
                        Console.WriteLine(DateTime.Now.ToShortTimeString() + " - sleep 60s.");
                        Thread.Sleep(6 * 1000);
                        last_height = GetLastHeight();
                    }

                    if (IsSave(m_last_utc, block.time))
                    {
                        SavePaymnet(m_last_utc);
                    }
                    decimal total = 0;
                    if (IsDposAddr(block.reward_address))
                    {
                        if (!m_vote_reward.ContainsKey(block.reward_address))
                        {
                            m_vote_reward.Add(block.reward_address, new Dictionary<string, VoteReward>());
                        }
                        foreach (var client_addr in m_vote_reward[block.reward_address].Keys)
                        {
                            total += m_vote_reward[block.reward_address][client_addr].vote;
                        }
                        foreach (var client_addr in m_vote_reward[block.reward_address].Keys)
                        {
                            //Console.WriteLine(string.Format("区块奖励地址{0};区块奖励金额{1}；奖励客户地址{2}", block.reward_address, block.reward_money, client_addr));
                            m_vote_reward[block.reward_address][client_addr].reward += block.reward_money * m_vote_reward[block.reward_address][client_addr].vote / total;
                        }
                    }
                    m_last_utc = block.time;
                    id = block.id;
                    ClearState(m_last_utc);
                    List<Tx> txs = GetTx(block.hash);
                    foreach (var tx in txs)
                    {
                        // 处理第一种类型的投票，第三方的投票
                        if (!string.IsNullOrEmpty(tx.dpos_in))
                        {
                            if (!m_vote_reward.ContainsKey(tx.dpos_in))
                            {
                                m_vote_reward.Add(tx.dpos_in, new Dictionary<string, VoteReward>());
                            }
                            if (!m_vote_reward[tx.dpos_in].ContainsKey(tx.client_in))
                            {
                                m_vote_reward[tx.dpos_in].Add(tx.client_in, new VoteReward(0, 0));
                            }
                            m_vote_reward[tx.dpos_in][tx.client_in].vote += tx.amount;
                            info = string.Format("投票1:{0}-->>{1}-{2}", tx.client_in, tx.dpos_in, tx.amount);
                            //Console.WriteLine(info);
                        }
                        // 处理第二种类型的投票，自己人的投票 和出块奖励
                        if (tx.to.Substring(0, 4) == DposPrefix)
                        {
                            if (!m_vote_reward.ContainsKey(tx.to))
                            {
                                m_vote_reward.Add(tx.to, new Dictionary<string, VoteReward>());
                            }
                            if (!m_vote_reward[tx.to].ContainsKey(tx.to))
                            {
                                m_vote_reward[tx.to].Add(tx.to, new VoteReward(0, 0));
                            }
                            m_vote_reward[tx.to][tx.to].vote += tx.amount;
                            info = string.Format("投票2:{0}-->>{1}-{2}", tx.form, tx.to, tx.amount);
                            //Console.WriteLine(info);
                        }

                        // 处理第一种类型的赎回，第三方的赎回
                        if (!string.IsNullOrEmpty(tx.dpos_out))
                        {
                            if (!m_vote_reward.ContainsKey(tx.dpos_out))
                            {
                                m_vote_reward.Add(tx.dpos_out, new Dictionary<string, VoteReward>());
                            }
                            if (!m_vote_reward[tx.dpos_out].ContainsKey(tx.client_out))
                            {
                                m_vote_reward[tx.dpos_out].Add(tx.client_out, new VoteReward(0, 0));
                            }
                            // 赎回 (应该永远不会为负数)
                            m_vote_reward[tx.dpos_out][tx.client_out].vote -= (tx.amount + tx.free);
                            info = string.Format("赎回1:{0}-->>{1}-{2}", tx.dpos_out, tx.client_out, tx.amount + tx.free);
                            //Console.WriteLine(info);
                        }

                        // 处理第二种类型的赎回，自己投票的赎回
                        if (tx.form.Substring(0, 4) == DposPrefix)
                        {
                            if (!m_vote_reward.ContainsKey(tx.form))
                            {
                                m_vote_reward.Add(tx.form, new Dictionary<string, VoteReward>());
                            }
                            if (!m_vote_reward[tx.form].ContainsKey(tx.form))
                            {
                                m_vote_reward[tx.form].Add(tx.form, new VoteReward(0, 0));
                            }
                            m_vote_reward[tx.form][tx.form].vote -= (tx.amount + tx.free);
                            info = string.Format("赎回2:{0}-->>{1}-{2}", tx.form, tx.form, tx.amount + tx.free);
                            //Console.WriteLine(info);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  更新投票信息到数据库
        /// </summary>
        void UpdateDposState(long last_utc)
        {
            var audit_date = ToDateTime(last_utc);
            using (var conn = new MySqlConnection(connStr))
            {
                foreach (var dpos_addr in m_vote_reward.Keys)
                {
                    foreach (var client_addr in m_vote_reward[dpos_addr].Keys)
                    {
                        string sql;
                        if (conn.Query<DposState>("SELECT * FROM `DposState` where dpos_addr = @dpos_addr and client_addr = @client_addr and audit_date = @audit_date",
                                new { dpos_addr = dpos_addr, client_addr = client_addr, audit_date = audit_date }).ToList().Count == 0)
                        {
                            sql = "INSERT DposState(dpos_addr, client_addr, audit_date, audit_money)VALUES(@dpos_addr, @client_addr, @audit_date, @audit_money)";
                        }
                        else
                        {
                            sql = "Update DposState set audit_money=@audit_money where dpos_addr = @dpos_addr and client_addr = @client_addr and audit_date = @audit_date";
                        }
                        conn.Execute(sql, new { dpos_addr = dpos_addr, client_addr = client_addr, audit_date = audit_date, audit_money = m_vote_reward[dpos_addr][client_addr].vote });
                    }
                }
            }
        }

        /// <summary>
        /// 保存一次清算到数据库
        /// </summary>
        /// <param name="last_utc"></param>
        public void SavePaymnet(long last_utc)
        {
            Console.WriteLine("-------------->>>>>>>> Save payment date:{0}",ToDateTime(last_utc).ToShortDateString());
            using (var conn = new MySqlConnection(connStr))
            {
                foreach (var dpos_addr in m_vote_reward.Keys)
                {
                    foreach (var client_addr in m_vote_reward[dpos_addr].Keys)
                    {
                        if (m_vote_reward[dpos_addr][client_addr].reward != 0)
                        {
                            conn.Execute("INSERT DposPayment(dpos_addr, client_addr, payment_date, payment_money)VALUES(@dpos_addr, @client_addr, @payment_date, @payment_money)",
                                new { dpos_addr = dpos_addr, client_addr = client_addr, payment_date = ToDateTime(last_utc), payment_money = m_vote_reward[dpos_addr][client_addr].reward });
                            m_vote_reward[dpos_addr][client_addr].reward = 0;
                        }
                    }
                }
            }
            UpdateDposState(last_utc);
        }
    }
}