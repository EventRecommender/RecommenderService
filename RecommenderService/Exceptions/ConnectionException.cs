using MySql.Data.MySqlClient;

namespace RecommenderService.Exceptions
{
	public class ConnectionException : Exception
	{
		public MySqlConnection con;

		public ConnectionException()
		{
			con = new();
		}
		public ConnectionException(string message) : base(message)
		{
			con = new();
		}
		public ConnectionException(string message, MySqlConnection connection) : base(message)
		{
			con = connection;
		}
	}
}
