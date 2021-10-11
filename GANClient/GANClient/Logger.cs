using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace GANClient
{
  class Logger
  {
    static ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();

    public static void Enqueue(string log)
    {
      logQueue.Enqueue(DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss] ") + log);
    }

    public static List<string> Dequeue()
    {
      List<string> list = new List<string>();
      while (logQueue.Count > 0)
      {
        string log;
        if (logQueue.TryDequeue(out log))
        {
          list.Add(log);
        }

      }
      return list;
    }


  }
}
