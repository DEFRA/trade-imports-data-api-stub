using Microsoft.AspNetCore.Builder;

namespace TradeImportsDataApiStub.Test.Config;

public class EnvironmentTest
{

   [Fact]
   public void IsNotDevModeByDefault()
   { 
       var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
       var isDev = TradeImportsDataApiStub.Config.Environment.IsDevMode(builder);
       Assert.False(isDev);
   }
}
