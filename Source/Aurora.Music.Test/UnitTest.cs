
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aurora.Music.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async void OnlineSearchTest()
        {
            var result = await Core.Extension.OnlineMusicSearcher.SearchAsync("五月天");
            Assert.AreEqual(result.Code, 0);
        }
    }
}
