using System;
using System.Threading.Tasks;
using NetStoneMCP.Services;
using NUnit.Framework;

namespace NetStoneMCP.Tests.Services
{
    public class NetStoneServiceTests
    {
        [Test]
        public void GetCharacterId_WithoutInitialize_Throws()
        {
            var service = new NetStoneService();
            Assert.ThrowsAsync<Exception>(async () => await service.GetCharacterId("name", "world"));
        }
    }
}
