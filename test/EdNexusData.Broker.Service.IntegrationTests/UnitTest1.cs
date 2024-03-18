using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Data;
using EdNexusData.Broker.Service.IntegrationTests.Fixtures;

namespace EdNexusData.Broker.Service.IntegrationTests;

[Collection("BrokerWebDICollection")]
public class UnitTest1
{
    private readonly BrokerWebDIServicesFixture _services;

    public UnitTest1(BrokerWebDIServicesFixture services)
    {
        _services = services;
    }
    
    [Fact]
    public void Test1()
    {
        var dbcontext = _services.Services!.GetService<BrokerDbContext>();

        Assert.NotNull(dbcontext);
    }
}