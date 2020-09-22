using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DposTask
{
    /// <summary>
    /// BBC 奖励分布
    /// </summary>
    class TokenDistribution
    {
        class Info
        {
            public long height { get; set; }
            public long  total { get; set; }
            public int reward { get; set; }
        }

        List<Info> m_ltd;

        public TokenDistribution()
        {
            m_ltd = new List<Info>();

            long total = 0;
            long height = 0;
            for (int i = 0; i < 5; i++)
            {
                Info obj = new Info();
                obj.reward = 1153 - 110 * i;

                total += 43200 * obj.reward;
                obj.total = total;

                height += 43200;
                obj.height = height;
                m_ltd.Add(obj);
            }
            for (int i = 0; i < 11; i++)
            {
                Info obj = new Info();
                obj.reward = 603 - 53 * i;

                total += (43200 * 5) * obj.reward;
                obj.total = total;

                height += (43200 * 5);
                obj.height = height;
                m_ltd.Add(obj);
            }
        }

        /// <summary>
        /// 得到指定高度下的全部出块奖励
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public long GetTotal(long height)
        {
            long max_height = 0;
            long max_money = 0;
            foreach (var obj in m_ltd)
            {
                if (height < obj.height)
                {
                    return obj.total - (obj.height - height) * obj.reward;
                }
                max_height = obj.height;
                max_money = obj.total;
            }
            return max_money + (height - max_height) * 20;
        }
    }
}
