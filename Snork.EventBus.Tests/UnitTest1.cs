using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Snork.EventBus.Tests
{
    public class abc
    {
        [Subscribe(ThreadModeEnum.Background)]
        public void OnMessageEventBackground(UnitTest1.MessageEvent @event)
        {
            System.Diagnostics.Debug.Print($"Background {@event.Id}");
        }
        [Subscribe(ThreadModeEnum.Async)]
        public void OnMessageEventAsync(UnitTest1.MessageEvent @event)
        {
            System.Diagnostics.Debug.Print($"Async {@event.Id}");
        }
        //[Subscribe(ThreadModeEnum.Posting)]
        //public void OnMessageEventPosting(UnitTest1.MessageEvent @event)
        //{
        //    System.Diagnostics.Debug.Print($"Posting {@event.Id}");
        //}
        //[Subscribe(ThreadModeEnum.Main)]
        //public void OnMessageEventMain(UnitTest1.MessageEvent @event)
        //{
        //    System.Diagnostics.Debug.Print($"Main {@event.Id}");
        //}
    }
    public class UnitTest1
    {
        public class MessageEvent
        {
            public int Id { get; set; }
        }
        [Subscribe(ThreadModeEnum.Background)]
        public void OnMessageEventBackground(MessageEvent @event)
        {

        }
        [Subscribe(ThreadModeEnum.Async)]
        public void OnMessageEventAsync(MessageEvent @event)
        {

        }
        [Subscribe(ThreadModeEnum.Posting)]
        public void OnMessageEventPosting(MessageEvent @event)
        {

        }
        [Subscribe(ThreadModeEnum.Main)]
        public void OnMessageEventMain(MessageEvent @event)
        {

        }

        [Fact]
        public void Test1()
        {
            var a = new abc();
            EventBus.Default.Register(a);
            EventBus.Default.Register<MessageEvent>(method =>
            {

            });
            var m = new List<object>();
            for (var id = 0; id < 2; id++)
                m.Add(new MessageEvent() { Id = id });
            EventBus.Default.Post(m.ToArray());
            Thread.Sleep(15000);
            EventBus.Default.Unregister(a);
        }
    }
}
