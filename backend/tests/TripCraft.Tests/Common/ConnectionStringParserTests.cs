using FluentAssertions;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Tests.Common;

public class ConnectionStringParserTests
{
    [Fact]
    public void Converts_postgres_url_to_npgsql_format()
    {
        var result = ConnectionStringParser.ToNpgsql("postgresql://app:p%40ss@db.example.com:6543/tripcraft?sslmode=require");

        result.Should().Contain("Host=db.example.com")
              .And.Contain("Port=6543")
              .And.Contain("Database=tripcraft")
              .And.Contain("Username=app")
              .And.Contain("Password=p@ss")
              .And.Contain("SSL Mode=Require");
    }

    [Fact]
    public void Leaves_key_value_connection_string_unchanged()
    {
        const string input = "Host=localhost;Database=tripcraft;Username=postgres;Password=postgres";

        ConnectionStringParser.ToNpgsql(input).Should().Be(input);
    }
}
